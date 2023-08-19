using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;

namespace wspGridControl
{
    public class GridColumnsCollection : ObservableCollection<GridColumn>
    {
        #region Variables
        // The GridListView that we're in
        [NonSerialized]
        private DependencyObject _owner = null;

        // internal storage of CollumnCollection
        private List<GridColumn> _columns = new List<GridColumn>();

        // index of column in _columns
        // GridColumn this[i] in public interface is _columns[_actualIndices[i]]
        private List<int> _actualIndices = new List<int>();

        private bool _inViewMode;
        private bool _isImmutable;

        // EventArgs for _internalCollectionChanged event.
        // We should raise internal event just before ColletonChanged event of base class,
        // but can't pass this to OnCollectionChanged method as parameter. So store it as a class member.
        [NonSerialized]
        private ColumnCollectionChangedEventArgs _internalEventArg;
        #endregion

        #region Events
        private event NotifyCollectionChangedEventHandler _internalCollectionChanged;

        internal event NotifyCollectionChangedEventHandler InternalCollectionChanged
        {
            add { _internalCollectionChanged += value; }
            remove { _internalCollectionChanged -= value; }
        }
        #endregion

        #region Properties
        internal DependencyObject Owner
        {
            get => _owner;
            set
            {
                if (value != _owner)
                {
                    _owner = value;
                }
            }
        }

        // Column list. Columns in this list are organized in the order 
        // that they were inserted into this collection. So Move operation 
        // won't change this list.
        internal List<GridColumn> ColumnCollection { get => _columns; }

        // Actual index list of GridColumn in ColumnCollection
        // this[i] == ColumnCollection[IndexList[i]]
        internal List<int> IndexList { get => _actualIndices; }

        // the Owner is GridListView
        internal bool InViewMode
        {
            get => _inViewMode;
            set { _inViewMode = value; }
        }

        // GridListViewHeaderRowPresenter will set this field to true once reorder is started.
        private bool IsImmutable
        {
            get => _isImmutable;
            set { _isImmutable = value; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Make the collection not changabel. 
        /// Should call UnblockWrite the same times as BlockWrite to make collection writable.
        /// </summary>
        internal void BlockWrite()
        {
            IsImmutable = true;
        }

        /// <summary>
        /// Counterfact the effect of BlockWrite() - restore collection to a changable state.
        /// </summary>
        internal void UnblockWrite()
        {
            IsImmutable = false;
        }

        private void VerifyIndexInRange(int index, string indexName)
        {
            if (index < 0 || index >= _actualIndices.Count)
            {
                throw new ArgumentOutOfRangeException(indexName);
            }
        }

        private void VerifyAccess()
        {
            if (IsImmutable)
            {
                throw new InvalidOperationException();
            }

            // Although CheckReentrancy() is called in base class, we still need to call it here again,
            // otherwise, when Reentrancy is found and exception is thrown, our operation is done and can't be undo.
            CheckReentrancy();
        }

        // Throw if column is null or already existed in a GVCC
        private void ValidateColumnForInsert(GridColumn column)
        {
            if (column == null)
            {
                throw new ArgumentNullException("column");
            }

            if (column.ActualIndex >= 0)
            {
                throw new InvalidOperationException();
            }
        }

        // Remove the index of the removed column in _actualIndices
        // Correct index in _actualIndices which is bigger than actualIndex
        private void UpdateIndexList(int actualIndex, int index)
        {
            for (int sourceIndex = 0; sourceIndex < index; sourceIndex++)
            {
                int i = _actualIndices[sourceIndex];
                if (i > actualIndex)
                {
                    _actualIndices[sourceIndex] = i - 1;
                }
            }

            for (int sourceIndex = index + 1; sourceIndex < _actualIndices.Count; sourceIndex++)
            {
                int i = _actualIndices[sourceIndex];
                if (i < actualIndex)
                {
                    _actualIndices[sourceIndex - 1] = i;
                }
                else if (i > actualIndex)
                {
                    _actualIndices[sourceIndex - 1] = i - 1;
                }
            }

            _actualIndices.RemoveAt(_actualIndices.Count - 1);
        }

        // pack the actual indeices in columns from after the removed one
        private void UpdateActualIndexInColumn(int iStart)
        {
            for (int i = iStart; i < _columns.Count; i++)
            {
                _columns[i].ActualIndex = i;
            }
        }

        private ColumnCollectionChangedEventArgs MovePreprocess(int oldIndex, int newIndex)
        {
            Debug.Assert(oldIndex != newIndex, "oldIndex==newIndex when perform move action.");

            VerifyIndexInRange(oldIndex, "oldIndex");
            VerifyIndexInRange(newIndex, "newIndex");

            int actualIndex = _actualIndices[oldIndex];

            if (oldIndex < newIndex)
            {
                for (int targetIndex = oldIndex; targetIndex < newIndex; targetIndex++)
                {
                    _actualIndices[targetIndex] = _actualIndices[targetIndex + 1];
                }
            }
            else //if (oldIndex > newIndex)
            {
                for (int targetIndex = oldIndex; targetIndex > newIndex; targetIndex--)
                {
                    _actualIndices[targetIndex] = _actualIndices[targetIndex - 1];
                }
            }

            _actualIndices[newIndex] = actualIndex;

            return new ColumnCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, _columns[actualIndex], newIndex, oldIndex, actualIndex);
        }

        private ColumnCollectionChangedEventArgs ClearPreprocess()
        {
            GridColumn[] list = new GridColumn[Count];
            if (Count > 0)
            {
                CopyTo(list, 0);
            }

            // reset columns *before* remove
            foreach (GridColumn c in _columns)
            {
                c.ResetPrivateData();
                ((INotifyPropertyChanged)c).PropertyChanged -= new PropertyChangedEventHandler(ColumnPropertyChanged);
            }

            _columns.Clear();
            _actualIndices.Clear();

            return new ColumnCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset, list);
        }

        private ColumnCollectionChangedEventArgs RemoveAtPreprocess(int index)
        {
            VerifyIndexInRange(index, "index");

            int actualIndex = _actualIndices[index];
            GridColumn column = _columns[actualIndex];

            column.ResetPrivateData();
            ((INotifyPropertyChanged)column).PropertyChanged -= new PropertyChangedEventHandler(ColumnPropertyChanged);

            _columns.RemoveAt(actualIndex);

            UpdateIndexList(actualIndex, index);

            UpdateActualIndexInColumn(actualIndex);

            return new ColumnCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, column, index, actualIndex);
        }

        private ColumnCollectionChangedEventArgs InsertPreprocess(int index, GridColumn column)
        {
            int count = _columns.Count;
            if (index < 0 || index > count)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            ValidateColumnForInsert(column);

            _columns.Add(column);
            column.ActualIndex = count;

            _actualIndices.Insert(index, count);

            ((INotifyPropertyChanged)column).PropertyChanged += new PropertyChangedEventHandler(ColumnPropertyChanged);

            return new ColumnCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, column, index, count /* actual index*/);
        }

        // This[index] = newColumn
        private ColumnCollectionChangedEventArgs SetPreprocess(int index, GridColumn newColumn)
        {
            VerifyIndexInRange(index, "index");

            GridColumn oldColumn = this[index];

            if (oldColumn != newColumn)
            {
                int oldColumnActualIndex = _actualIndices[index];

                RemoveAtPreprocess(index);
                InsertPreprocess(index, newColumn);

                return new ColumnCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newColumn, oldColumn, index, oldColumnActualIndex);
            }

            return null;
        }

        /// <summary>
        /// called by base class ObservableCollection&lt;T&gt; when the list is being cleared;
        /// GridListViewColumnCollection override this method to do some internal preprocess work
        /// </summary>
        protected override void ClearItems()
        {
            VerifyAccess();
            _internalEventArg = ClearPreprocess();
            base.ClearItems();
        }

        /// <summary>
        /// called by base class ObservableCollection&lt;T&gt; when an column is removed from list;
        /// GridListViewColumnCollection override this method to do some internal preprocess work
        /// </summary>
        protected override void RemoveItem(int index)
        {
            VerifyAccess();
            _internalEventArg = RemoveAtPreprocess(index);
            base.RemoveItem(index);
        }

        /// <summary>
        /// called by base class ObservableCollection&lt;T&gt; when an item is added to list;
        /// GridListViewColumnCollection override this method to do some internal preprocess work
        /// </summary>
        protected override void InsertItem(int index, GridColumn column)
        {
            VerifyAccess();
            _internalEventArg = InsertPreprocess(index, column);
            base.InsertItem(index, column);
        }

        /// <summary>
        /// called by base class ObservableCollection&lt;T&gt; when an column is set in list;
        /// GridListViewColumnCollection override this method to do some internal preprocess work
        /// </summary>
        protected override void SetItem(int index, GridColumn column)
        {
            VerifyAccess();
            _internalEventArg = SetPreprocess(index, column);
            if (_internalEventArg != null) // the new column is equals to the old one. 
            {
                base.SetItem(index, column);
            }
        }

        /// <summary>
        /// Move column to a different index
        /// </summary>
        /// <param name="oldIndex">index of the column which is being moved</param>
        /// <param name="newIndex">index of the column to be move to</param>
        protected override void MoveItem(int oldIndex, int newIndex)
        {
            if (oldIndex != newIndex)
            {
                VerifyAccess();
                _internalEventArg = MovePreprocess(oldIndex, newIndex);
                base.MoveItem(oldIndex, newIndex);
            }
        }

        /// <summary>
        /// raise CollectionChanged event to any listeners
        /// </summary>
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            OnInternalCollectionChanged();
            base.OnCollectionChanged(e);
        }

        private void OnInternalCollectionChanged()
        {
            if (_internalCollectionChanged != null && _internalEventArg != null)
            {
                _internalCollectionChanged(this, _internalEventArg);
                // This class member is used as parameter, so clear it after used.
                // For details, see definition.
                _internalEventArg = null;
            }
        }

        private void ColumnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            GridColumn column = sender as GridColumn;

            if (_internalCollectionChanged != null && column != null)
            {
                _internalCollectionChanged(this, new ColumnCollectionChangedEventArgs(column, e.PropertyName));
            }
        }
        #endregion
    }
}
