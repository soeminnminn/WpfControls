using System;
using System.Collections.Specialized;
using System.Windows;

namespace wspGridControl
{
    internal enum SelectionDirections
    {
        Left,
        Right,
        Up,
        Down,
    }

    internal class SelectionManager
    {
        #region Variables
        private readonly GridControl _owner;
        private BlockOfCells _selectedBlock = null;

        private int _curColIndex = -1;
        private long _curRowIndex = -1L;
        #endregion

        #region Constructor
        public SelectionManager(GridControl owner)
        {
            _owner = owner;
            ColumnsCollectionChangedEventManager.AddHandler(owner.Columns, OnColumnCollectionChanged);
        }
        #endregion

        #region Properties
        public int CurrentColumn
        {
            get => _curColIndex;
            set { _curColIndex = value; }
        }

        public long CurrentRow
        {
            get => _curRowIndex;
            set { _curRowIndex = value; }
        }

        public BlockOfCells SelectedBlock
        {
            get => _selectedBlock;
        }

        public bool HasAnySelection
        {
            get => _selectedBlock != null && !_selectedBlock.IsEmpty && _selectedBlock.Width > 0 && _selectedBlock.Height > 0;
        }
        #endregion

        #region Methods
        private int ClampColumnIndex(int nColIndex)
        {
            int lastIdx = _owner.Columns.Count - 1;
            return Math.Min(Math.Max(0, nColIndex), lastIdx);
        }

        private long ClampRowIndex(long nRowIndex)
        {
            long lastIdx = _owner.RowCount - 1;
            return Math.Min(Math.Max(0, nRowIndex), lastIdx);
        }

        public bool StartSelection(CellInfo info)
        {
            if (info == null) return false;
            return StartSelection(info.RowIndex, info.DisplayColumnIndex);
        }

        public bool StartColumnSelection(int nColIndex)
        {
            if (StartSelection(0, nColIndex))
            {
                long rowIndex = _owner.RowCount - 1;
                UpdateSelection(rowIndex, nColIndex);
                return true;
            }
            return false;
        }

        public bool StartRowSelection(long nRowIndex)
        {
            if (StartSelection(nRowIndex, 0))
            {
                int colIdx = _owner.Columns.Count - 1;
                UpdateSelection(nRowIndex, colIdx);
                return true;
            }
            return false;
        }

        public bool StartSelection(long nRowIndex, int nColIndex)
        {
            long rowIdx = ClampRowIndex(nRowIndex);
            int colIdx = ClampColumnIndex(nColIndex);

            if (rowIdx == nRowIndex && colIdx == nColIndex)
            {
                _curRowIndex = rowIdx;
                _curColIndex = colIdx;

                _selectedBlock = new BlockOfCells(rowIdx, colIdx);
                return true;
            }
            return false;
        }

        public void UpdateSelection(CellInfo info)
        {
            if (info == null) return;
            UpdateSelection(info.RowIndex, info.DisplayColumnIndex);
        }

        public void UpdateColumnSelection(int nColIndex)
        {
            if (_selectedBlock != null)
            {
                long nRowIndex = _selectedBlock.Bottom;
                _selectedBlock.UpdateBlock(nRowIndex, nColIndex);
            }
        }

        public void UpdateRowSelection(long nRowIndex)
        {
            if (_selectedBlock == null)
            {
                StartRowSelection(nRowIndex);
            }
            else
            {
                int nColIndex = _selectedBlock.Right;
                _selectedBlock.UpdateBlock(nRowIndex, nColIndex);
            }
        }

        public void UpdateSelection(long nRowIndex, int nColIndex)
        {
            if (_selectedBlock == null)
            {
                StartSelection(nRowIndex, nColIndex);
            }
            else
            {
                long rowIdx = ClampRowIndex(nRowIndex);
                int colIdx = ClampColumnIndex(nColIndex);

                _selectedBlock.UpdateBlock(rowIdx, colIdx);
            }
        }

        public BlockOfCells SetSelection(BlockOfCells cells)
        {
            if (cells == null || cells.IsEmpty)
            {
                Clear();
                return new BlockOfCells();
            }
            else
            {
                var blocks = cells.Clone();
                blocks.X = ClampColumnIndex(cells.X);
                blocks.Y = ClampRowIndex(cells.Y);

                if (blocks.Width > 0 && blocks.Height > 0)
                    _selectedBlock = blocks;

                return blocks;
            }
        }

        public void Clear()
        {
            Clear(true);
        }

        public void Clear(bool bClearCurrentCell)
        {
            _selectedBlock = null;
            if (bClearCurrentCell)
            {
                _curColIndex = -1;
                _curRowIndex = -1L;
            }
        }

        public bool IsCellSelected(CellInfo info)
        {
            if (info == null) return false;
            return IsCellSelected(info.RowIndex, info.DisplayColumnIndex);
        }

        public bool IsCellSelected(long nRowIndex, int nColIndex)
        {
            if (_selectedBlock != null && _selectedBlock.Contains(nRowIndex, nColIndex))
            {
                return true;
            }
            return false;
        }

        public Rect GetSectionRect(Vector offset)
        {
            BlockOfCells selection = _selectedBlock;
            if (selection == null) return Rect.Empty;

            var rowHeight = _owner.RowHeight;
            var columns = _owner.Columns;

            var rowIdx = selection.Y;
            var startIdx = _owner.RowStartIndex;

            Rect bounds = new Rect();

            bounds.Y = offset.Y + (rowHeight * (rowIdx - startIdx));
            var h = selection.Height;
            bounds.Height = h * rowHeight;

            var x = selection.X;
            var x1 = selection.Right;
            double prevWidth = 0.0;
            for (int c = 0; c <= x1; c++)
            {
                if (c <= x)
                    bounds.X += prevWidth;

                prevWidth = columns[c].FinalWidth;

                if (c >= x)
                    bounds.Width += prevWidth;
            }

            return bounds;
        }

        public Rect GetCellRect(long nRowIndex, int nColIndex, Vector offset)
        {
            var rowHeight = _owner.RowHeight;
            var columns = _owner.Columns;

            long startIdx = _owner.RowStartIndex;

            Rect bounds = new Rect();

            bounds.Y = offset.Y + (rowHeight * (nRowIndex - startIdx));
            bounds.Height = rowHeight;

            double prevWidth = 0.0;
            for (int c = 0; c <= nColIndex; c++)
            {
                bounds.X += prevWidth;
                bounds.Width = prevWidth = columns[c].FinalWidth;
            }

            return bounds;
        }

        private void OnColumnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Move)
            {
                Clear();
            }
        }
        #endregion
    }
}
