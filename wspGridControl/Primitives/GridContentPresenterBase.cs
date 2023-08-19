using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace wspGridControl.Primitives
{
    /// <summary>
    /// Base class for Presenters.
    /// </summary>
    public abstract class GridContentPresenterBase : FrameworkElement, IWeakEventListener
    {
        #region Variables
        // the minimum width for dummy header when measure
        internal const double c_PaddingHeaderMinWidth = 2.0;

        private UIElementCollection _uiElementCollection;
        private bool _needUpdateVisualTree = true;
        private List<double> _desiredWidthList;
        private GridControl _owner;
        #endregion

        #region Constructors
        protected GridContentPresenterBase()
        {
            Focusable = false;
        }
        #endregion

        #region Dependency Properties

        #region ColumnsProperty
        /// <summary>
        ///  Columns DependencyProperty
        /// </summary>
        public static readonly DependencyProperty ColumnsProperty = DependencyProperty.Register(
            nameof(Columns), typeof(GridColumnsCollection), typeof(GridContentPresenterBase),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure, 
                new PropertyChangedCallback(OnColumnsPropertyChanged)));

        /// <summary>
        /// Columns Property
        /// </summary>
        public GridColumnsCollection Columns
        {
            get { return (GridColumnsCollection)GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        // Property invalidation callback invoked when ColumnCollectionProperty is invalidated
        private static void OnColumnsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            GridContentPresenterBase c = (GridContentPresenterBase)d;
            GridColumnsCollection oldCollection = (GridColumnsCollection)e.OldValue;

            if (oldCollection != null)
            {
                ColumnsCollectionChangedEventManager.RemoveHandler(oldCollection, c.ColumnCollectionChanged);
                if (!oldCollection.InViewMode && oldCollection.Owner == c.GetStableAncester())
                {
                    oldCollection.Owner = null;
                }
            }

            GridColumnsCollection newCollection = (GridColumnsCollection)e.NewValue;
            if (newCollection != null)
            {
                ColumnsCollectionChangedEventManager.AddHandler(newCollection, c.ColumnCollectionChanged);
                if (!newCollection.InViewMode && newCollection.Owner == null)
                {
                    newCollection.Owner = c.GetStableAncester();
                }
            }

            c.NeedUpdateVisualTree = true;
            c.InvalidateMeasure();
        }
        #endregion

        #region ScrollOffsetProperty
        internal static readonly DependencyProperty ScrollOffsetProperty = DependencyProperty.Register(
            nameof(ScrollOffset), typeof(Vector), typeof(GridContentPresenterBase),
            new FrameworkPropertyMetadata(new Vector(),
                new PropertyChangedCallback(OnScrollOffsetChanged)));

        internal Vector ScrollOffset
        {
            get => (Vector)GetValue(ScrollOffsetProperty);
            set => SetValue(ScrollOffsetProperty, value);
        }

        private static void OnScrollOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GridContentPresenterBase c)
            {
                c.OnScrollOffsetChanged((Vector)e.OldValue, (Vector)e.NewValue);
            }
        }

        protected virtual void OnScrollOffsetChanged(Vector oldValue, Vector newValue)
        {
        }
        #endregion

        #endregion

        #region Properties
        internal GridControl GridOwner
        {
            get => _owner;
            set 
            { 
                _owner = value; 
                if (_owner != null)
                {
                    _owner.Tracker.AddListener<GridContentPresenterBase>(this);
                }
            }
        }

        /// <summary>
        /// Returns enumerator to logical children.
        /// </summary>
        protected override IEnumerator LogicalChildren
        {
            get
            {
                if (InternalChildren.Count == 0)
                {
                    // empty GridListViewRowPresenterBase has *no* logical children; give empty enumerator
                    return Enumerable.Empty<object>().GetEnumerator();
                }

                // otherwise, its logical children is its visual children
                return InternalChildren.GetEnumerator();
            }
        }

        /// <summary>
        /// Gets the Visual children count.
        /// </summary>
        protected override int VisualChildrenCount
        {
            get
            {
                if (_uiElementCollection == null)
                {
                    return 0;
                }
                else
                {
                    return _uiElementCollection.Count;
                }
            }
        }

        /// <summary>
        /// list of currently reached max value of DesiredWidth of cell in the column
        /// </summary>
        internal List<double> DesiredWidthList
        {
            get { return _desiredWidthList; }
            private set { _desiredWidthList = value; }
        }

        /// <summary>
        /// if visual tree is out of date
        /// </summary>
        internal bool NeedUpdateVisualTree
        {
            get { return _needUpdateVisualTree; }
            set { _needUpdateVisualTree = value; }
        }

        /// <summary>
        /// collection if children
        /// </summary>
        internal UIElementCollection InternalChildren
        {
            get
            {
                if (_uiElementCollection == null)
                {
                    _uiElementCollection = new UIElementCollection(this, this);
                }

                return _uiElementCollection;
            }
        }

        // if and only if both conditions below are satisfied, row presenter visual is ready.
        // 1. is initialized, which ensures RowPresenter is created
        // 2. !NeedUpdateVisualTree, which ensures all visual elements generated by RowPresenter are created
        protected bool IsPresenterVisualReady
        {
            get { return IsInitialized && !NeedUpdateVisualTree; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Override of <seealso cref="FrameworkElement.OnApplyTemplate"/>.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (NeedUpdateVisualTree)
            {
                UpdateVisualTree();

                NeedUpdateVisualTree = false;
            }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
        }

        /// <summary>
        /// Gets the Visual child at the specified index.
        /// </summary>
        protected override Visual GetVisualChild(int index)
        {
            if (_uiElementCollection == null)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            return _uiElementCollection[index];
        }

        internal virtual void UpdateVisualTree(bool forceUpdate = false)
        {
            NeedUpdateVisualTree = false;
        }

        /// <summary>
        /// process the column collection chagned event
        /// </summary>
        protected virtual void OnColumnCollectionChanged(ColumnCollectionChangedEventArgs e)
        {
            if (DesiredWidthList != null)
            {
                if (e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Replace)
                {
                    if (DesiredWidthList.Count > e.ActualIndex)
                    {
                        DesiredWidthList.RemoveAt(e.ActualIndex);
                    }
                }
                else if (e.Action == NotifyCollectionChangedAction.Reset)
                {
                    DesiredWidthList = null;
                }
            }
        }

        /// <summary>
        /// process the column property chagned event
        /// </summary>
        protected virtual void OnColumnPropertyChanged(GridColumn column, string propertyName)
        { }

        /// <summary>
        /// ensure ShareStateList have at least columns.Count items
        /// </summary>
        internal void EnsureDesiredWidthList()
        {
            GridColumnsCollection columns = Columns;

            if (columns != null)
            {
                int count = columns.Count;

                if (DesiredWidthList == null)
                {
                    DesiredWidthList = new List<double>(count);
                }

                int c = count - DesiredWidthList.Count;
                for (int i = 0; i < c; i++)
                {
                    DesiredWidthList.Add(double.NaN);
                }
            }
        }

        private FrameworkElement GetStableAncester()
        {
            ItemsControl ic = ItemsControl.ItemsControlFromItemContainer(TemplatedParent);

            return (ic != null) ? ic : this;
        }

        /// <summary>
        /// Handle events from the centralized event table
        /// </summary>
        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs args)
        {
            return false;   // this method is no longer used (but must remain, for compat)
        }

        /// <summary>
        /// Handler of GridListViewColumnCollection.CollectionChanged event.
        /// </summary>
        private void ColumnCollectionChanged(object sender, NotifyCollectionChangedEventArgs arg)
        {
            if (arg is ColumnCollectionChangedEventArgs e && IsPresenterVisualReady)
            {
                // Property of one column changed
                if (e.Column != null)
                {
                    OnColumnPropertyChanged(e.Column, e.PropertyName);
                }
                else
                {
                    OnColumnCollectionChanged(e);
                }
            }
        }
        #endregion
    }
}
