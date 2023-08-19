using System;
using System.Windows;

namespace wspGridControl
{
    public interface IGridSource
    {
        event EventHandler Updated;

        long RowsCount { get; }

        int ColumnsCount { get; }

        GridColumnInfo GetColumnInfo(int nColIndex);

        string GetColumnHeaderAsString(int nColIndex);

        string GetRowHeaderAsString(long nRowIndex);

        string GetCellDataAsString(long nRowIndex, int nColIndex);

        int IsCellEditable(long nRowIndex, int nColIndex);

        bool SetCellDataFromControl(long nRowIndex, int nColIndex, FrameworkElement control, object data);
    }
}
