using System;
using wspGridControl.Primitives;

namespace wspGridControl
{
    public class CellInfo
    {
        #region Properties
        internal static CellInfo Unset
        {
            get => new CellInfo();
        }

        public int Index { get; internal set; }

        public int DisplayRowIndex { get; internal set; }

        public long RowIndex { get; internal set; }

        public int DisplayColumnIndex { get; internal set; }

        public int ColumnIndex { get; internal set; }

        public bool IsValid
        {
            get => Index > -1;
        }
        #endregion

        #region Constructors
        private CellInfo()
        {
            Index = -1;
            DisplayRowIndex = -1;
            DisplayColumnIndex = -1;
            RowIndex = -1;
            ColumnIndex = -1;
        }

        public CellInfo(long rowIndex, int colIndex)
        {
            Index = -1;
            DisplayRowIndex = -1;
            DisplayColumnIndex = -1;
            RowIndex = rowIndex;
            ColumnIndex = colIndex;
        }

        internal CellInfo(int index, int displayRowIndex, int displayColIndex, long rowIndex, int colIndex)
        {
            Index = index;
            DisplayRowIndex = displayRowIndex;
            DisplayColumnIndex = displayColIndex;
            RowIndex = rowIndex;
            ColumnIndex = colIndex;
        }

        internal CellInfo(CellInfo other, long rowIndex, int colIndex)
        {
            Index = other.Index;
            DisplayRowIndex = other.DisplayRowIndex;
            DisplayColumnIndex = other.DisplayColumnIndex;
            RowIndex = rowIndex;
            ColumnIndex = colIndex;
        }
        #endregion

        #region Methods
        public static bool IsNullOrInvalid(CellInfo cellInfo)
        {
            if (cellInfo == null) return true;
            return !cellInfo.IsValid;
        }

        public CellInfo Clone()
        {
            return new CellInfo(this, RowIndex, ColumnIndex);
        }

        internal void MakeUnset()
        {
            Index = -1;
        }

        internal void UpdateDisplayIndex(int rowIndex, int colIndex)
        {
            DisplayRowIndex = rowIndex;
            DisplayColumnIndex = colIndex;
        }

        public override string ToString()
        {
            return $"Row: {RowIndex}, Column: {ColumnIndex}";
        }

        public override bool Equals(object obj)
        {
            if (obj is CellInfo info)
                return info.RowIndex == RowIndex && info.ColumnIndex == ColumnIndex;
            else if (obj is GridColumn column)
                return column.ActualIndex == ColumnIndex;
            else if (obj is GridCell cell)
            {
                var cellInfo = cell.CellInfo;
                return cellInfo != null && cellInfo.RowIndex == RowIndex && cellInfo.ColumnIndex == ColumnIndex;
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return RowIndex.GetHashCode() & ColumnIndex.GetHashCode();
        }
        #endregion
    }
}
