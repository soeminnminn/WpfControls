using System;
using System.Collections;
using System.Collections.Generic;

namespace wspGridControl
{
    public class GridSourceEnumerable : ICollection, IEnumerable<string[]>, IEnumerable
    {
        #region Variables
        private readonly IGridSource _gridSource;
        private int _version = 0;
        #endregion

        #region Constructor
        public GridSourceEnumerable(IGridSource gridSource)
        {
            _gridSource = gridSource;
            _version = 0;

            gridSource.Updated += GridSource_Updated;
        }
        #endregion

        #region Properties
        public int Count
        {
            get => _gridSource == null ? 0 : (int)_gridSource.RowsCount;
        }

        bool ICollection.IsSynchronized
        {
            get => false;
        }

        object ICollection.SyncRoot
        {
            get => _gridSource;
        }
        #endregion

        #region Methods
        private void GridSource_Updated(object sender, EventArgs e)
        {
            _version++;
        }

        public IEnumerator<string[]> GetEnumerator() => new Enumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);

        public void CopyTo(Array array, int index)
        {
            if (array == null) return;
            if (array.IsReadOnly) return;

            var source = _gridSource;
            if (source == null) return;

            int length = source.ColumnsCount;
            if (length == 0) return;

            long count = Count;
            if (index < 0 || index >= count) return;

            for (int i = 0; i < length && i < array.Length; i++)
            {
                var value = source.GetCellDataAsString(index, i);
                array.SetValue(value, i);
            }
        }
        #endregion

        #region Nested Types
        public struct Enumerator : IEnumerator<string[]>, IEnumerator
        {
            #region Variables
            private readonly GridSourceEnumerable _owner;
            private readonly int _version;

            private readonly long _rowsCount;
            private readonly int _columnsCount;
            private long _index;
            private string[] _current;
            #endregion

            #region Constructor
            public Enumerator(GridSourceEnumerable owner)
            {
                _owner = owner;
                _version = owner._version;
                
                _rowsCount = owner._gridSource.RowsCount;
                _columnsCount = owner._gridSource.ColumnsCount;

                _index = 0;
                _current = null;
            }
            #endregion

            #region Properties
            public string[] Current => _current;

            object IEnumerator.Current
            {
                get
                {
                    if (_index == 0 || _index == _rowsCount + 1)
                        throw new InvalidOperationException();
                    return Current;
                }
            }
            #endregion

            #region Methods
            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                var localList = _owner._gridSource;

                if (localList != null && _columnsCount > 0 && _version == _owner._version && _index < _rowsCount)
                {
                    var values = new string[_columnsCount];
                    for (var i = 0; i< values.Length; i++)
                    {
                        values[i] = localList.GetCellDataAsString(_index, i);
                    }

                    _current = values;
                    _index++;
                    return true;
                }

                return MoveNextRare();
            }

            private bool MoveNextRare()
            {
                if (_version != _owner._version)
                    throw new InvalidOperationException();

                _index = _rowsCount + 1;
                _current = null;
                return false;
            }

            void IEnumerator.Reset()
            {
                if (_version != _owner._version)
                    throw new InvalidOperationException();

                _index = 0;
                _current = null;
            }
            #endregion
        }
        #endregion
    }
}
