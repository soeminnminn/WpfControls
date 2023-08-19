using System;
using System.Windows;

namespace wspGridControl
{
    public class GridColumnInfo
    {
        #region Variables
        public GridColumnWidth ColumnWidth = GridColumn.c_defaultColumnWidth;

        public TextAlignment HeaderAlignment = TextAlignment.Center;
        public TextAlignment ColumnAlignment = TextAlignment.Left; 
        
        public bool IsHeaderClickable = true;
        public bool IsResizable = true;
        #endregion

        #region Constructor
        public GridColumnInfo()
        { }
        #endregion

        #region Methods
        public override bool Equals(object obj)
        {
            if (obj is GridColumnInfo info)
            {
                return info.ColumnWidth == ColumnWidth && info.HeaderAlignment == HeaderAlignment && info.ColumnAlignment == ColumnAlignment &&
                    info.IsHeaderClickable == IsHeaderClickable && info.IsResizable == IsResizable;
            }
            else if (obj is GridColumn column)
            {
                return column.Width == ColumnWidth && column.IsHeaderClickable == IsHeaderClickable && column.IsResizable == IsResizable;
            }
            else if (obj is GridColumnWidth width)
            {
                return width.Equals(ColumnWidth);
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            int hashCode = ColumnWidth.GetHashCode() + HeaderAlignment.GetHashCode() + ColumnAlignment.GetHashCode();
            return hashCode + (IsHeaderClickable ? 1 : 0) + (IsResizable ? 1 : 0);
        }
        #endregion
    }
}
