using System;

namespace wspGridControl
{
    public sealed class BlockOfCells
    {
        #region Variables
        private int _x;
        private long _y;
        private int _originalX;
        private long _originalY;
        private int _right;
        private long _bottom;
        #endregion

        #region Constructors
        internal BlockOfCells()
        {
            _x = -1;
            _y = -1L;
            _right = -1;
            _bottom = -1L;
            _originalX = -1;
            _originalY = -1L;
        }

        public BlockOfCells(long nRowIndex, int nColIndex)
        {
            _x = -1;
            _y = -1L;
            _right = -1;
            _bottom = -1L;
            _originalX = -1;
            _originalY = -1L;

            InitNewBlock(nRowIndex, nColIndex);
        }
        #endregion

        #region Properties
        public int X
        {
            get => _x;
            set { _x = value; }
        }

        public long Y
        {
            get => _y;
            set { _y = value; }
        }

        public int OriginalX
        {
            get => _originalX;
        }

        public long OriginalY
        {
            get => _originalY;
        }

        public int Width
        {
            get
            {
                if (IsEmpty)
                {
                    return 0;
                }
                return (_right - _x) + 1;
            }
            set
            {
                if (value <= 0)
                {
                    _x = _right = -1;
                }
                else
                {
                    _right = (_x + value) - 1;
                }
            }
        }

        public long Height
        {
            get
            {
                if (IsEmpty)
                {
                    return 0L;
                }
                return ((_bottom - _y) + 1L);
            }
            set
            {
                if (value <= 0L)
                {
                    _y = _bottom = -1L;
                }
                else
                {
                    _bottom = (_y + value) - 1L;
                }
            }
        }

        public int Right
        {
            get => _right;
        }

        public long Bottom
        {
            get => _bottom;
        }

        internal int LastUpdatedCol
        {
            get
            {
                if (_x == _originalX)
                {
                    return _right;
                }
                return _x;
            }
        }

        internal long LastUpdatedRow
        {
            get
            {
                if (_y == _originalY)
                {
                    return _bottom;
                }
                return _y;
            }
        }

        public bool IsEmpty
        {
            get
            {
                if (_x != -1)
                {
                    return _y == -1L;
                }
                return true;
            }
        }
        #endregion

        #region Methods
        public bool Contains(long nRowIndex, int nColIndex)
        {
            return nColIndex >= _x && nColIndex <= _right && nRowIndex >= _y && nRowIndex <= _bottom;
        }

        public bool ColumnContain(int nColIndex)
        {
            return nColIndex >= _x && nColIndex <= _right;
        }

        private void InitNewBlock(long nRowIndex, int nColIndex)
        {
            _originalX = _x = _right = nColIndex;
            _originalY = _y = _bottom = nRowIndex;
        }

        internal void SetOriginalCell(long rowIndex, int columnIndex)
        {
            if (!Contains(rowIndex, columnIndex))
            {
                throw new ArgumentException("", "rowIndex or columnIndex");
            }
            if (!IsEmpty)
            {
                _originalY = rowIndex;
                _originalX = columnIndex;
            }
        }

        internal void UpdateBlock(long nRowIndex, int nColIndex)
        {
            if (IsEmpty)
            {
                InitNewBlock(nRowIndex, nColIndex);
            }
            else
            {
                if (nRowIndex < _originalY)
                {
                    _bottom = _originalY;
                    _y = nRowIndex;
                }
                else
                {
                    _y = _originalY;
                    _bottom = nRowIndex;
                }
                if (nColIndex < _originalX)
                {
                    _right = _originalX;
                    _x = nColIndex;
                }
                else
                {
                    _x = _originalX;
                    _right = nColIndex;
                }
            }
        }

        public BlockOfCells Clone()
        {
            var clone = new BlockOfCells();
            clone._x = _x;
            clone._y = _y;
            clone._originalX = _originalX;
            clone._originalY = _originalY;
            clone._right = _right;
            clone._bottom = _bottom;
            return clone;
        }

        public override bool Equals(object obj)
        {
            if (obj is BlockOfCells cell)
                return _x == cell._x && _y == cell._y && _right == cell._right && _bottom == cell._bottom;

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return _x.GetHashCode() & _right.GetHashCode() & _y.GetHashCode() & _bottom.GetHashCode();
        }

        public override string ToString()
        {
            return $"X: {_x}, Y: {_y}, Width: {Width}, Height: {Height}";
        }
        #endregion
    }
}
