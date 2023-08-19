using System;
using System.Collections.Generic;
using System.Windows;
using wspGridControl;

namespace WpfControlDemo.Models
{
    public class GridStorage : IGridSource
    {
        #region Variables
        internal readonly string[] columnHeaders = new string[] { "Col A", "Col B", "Col C", "Col D", "Col E", "Col F", "Col G", "Col H" };
        private readonly List<GridDataRow> mData;

        public event EventHandler Updated = null;
        #endregion

        #region Constructors
        public GridStorage(List<GridDataRow> list)
        {
            this.mData = list;
        }
        #endregion

        #region Properties
        public long RowsCount
        {
            get => mData.Count;
        }

        public int ColumnsCount 
        {
            get => 40; // columnHeaders.Length + 2;
        }
        #endregion

        #region Methods
        public void PostUpdated()
        {
            if (Updated != null)
                Updated(this, EventArgs.Empty);
        }

        public GridColumnInfo GetColumnInfo(int nColIndex)
        {
            GridColumnInfo info = new GridColumnInfo();

            switch (nColIndex)
            {
                case 2:
                    info.ColumnAlignment = TextAlignment.Center;
                    break;
                case 5:
                    info.ColumnAlignment = TextAlignment.Right;
                    break;
                default:
                    info.ColumnAlignment = TextAlignment.Left;
                    break;
            }
            return info;
        }

        public string GetColumnHeaderAsString(int nColIndex)
        {
            //if (nColIndex < 0 || nColIndex >= columnHeaders.Length) return null;
            //return columnHeaders[nColIndex];
            return null;
        }

        public string GetRowHeaderAsString(long nRowIndex)
        {
            if (nRowIndex + 1 == RowsCount)
                return "+";
            return $"{nRowIndex + 1}";
        }

        public string GetCellDataAsString(long nRowIndex, int nColIndex)
        {
            GridDataRow row = this.mData[(int)nRowIndex];

            switch (nColIndex)
            {
                case 2:
                    return row.Text;
                case 3:
                    return row.DropDownText;
                case 4:
                    return row.DropDownListText;
                case 5:
                    return string.Format("{0}", row.SpinValue);
                case 6:
                    return row.HyperLinkText;
                case 7:
                    return row.ButtonText;
                default:
                    return string.Format("Col: {0}, Row: {1}", nColIndex, nRowIndex);
            }
        }

        public int IsCellEditable(long nRowIndex, int nColIndex)
        {
            switch (nColIndex)
            {
                case 2:
                    return 1; // Text box
                case 3:
                    return 2; // Dropdown
                case 4:
                    return 3; // Dropdown List
                case 5:
                    return 4; // spin
                default:
                    return 1;
            }
        }

        public bool SetCellDataFromControl(long nRowIndex, int nColIndex, FrameworkElement control, object data)
        {
            System.Diagnostics.Trace.WriteLine(data);
            return true;
        }
        #endregion
    }
}
