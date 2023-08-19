using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using wspGridControl.Primitives;

namespace wspGridControl
{
    internal class ContentManager
    {
        #region Variables
        private readonly GridControl _owner;
        private CellInfo _editingCell = CellInfo.Unset;
        #endregion

        #region Constructor
        public ContentManager(GridControl owner)
        {
            _owner = owner;
        }
        #endregion

        #region Properties
        public GridControl Owner
        {
            get => _owner;
        }

        public IGridSource GridSource
        {
            get
            {
                if (_owner != null)
                    return _owner.GridSource;
                return null;
            }
        }

        private GridColumnsCollection Columns
        {
            get => Owner.Columns;
        }

        public CellInfo EditingCell
        {
            get => _editingCell;
        }
        #endregion

        #region Methods
        public void NotifyPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
        }

        public static string ToColumnName(int column)
        {
            string result = string.Empty;
            while (column >= 26)
            {
                int i = column / 26;
                result += ((char)('A' + i - 1)).ToString(CultureInfo.InvariantCulture);
                column = column - (i * 26);
            }

            result += ((char)('A' + column)).ToString(CultureInfo.InvariantCulture);
            return result;
        }

        public static string ToRowName(long row)
        {
            return (row + 1).ToString(CultureInfo.InvariantCulture);
        }

        public void UpdateColumns(GridColumnsCollection columns, double avCharWidth)
        {
            int count = columns.Count;
            int colsCount = 0;

            IGridSource source = GridSource;
            if (source != null) colsCount = source.ColumnsCount;

            if (colsCount == 0)
            {
                columns.Clear();
                return;
            }

            if (colsCount < count)
            {
                int required = count - colsCount;
                for (int i = required - 1; i >= 0; i--)
                {
                    columns.RemoveAt(colsCount + i);
                }
            }
            count = columns.Count;

            for (int c = 0; c < colsCount; c++)
            {
                var info = source.GetColumnInfo(c);
                GridColumn column;

                if (c < count)
                {
                    column = columns[c];
                    column.AverageCharWidth = avCharWidth;
                    column.ActualIndex = c;
                }
                else
                {
                    column = new GridColumn()
                    {
                        AverageCharWidth = avCharWidth,
                        Width = GridColumn.c_defaultColumnWidth
                    };
                    columns.Add(column);
                }

                column.ColumnInfo = info;
                column.GridOwner = _owner;
            }
        }

        #region ColumnHeaderContent
        public void ApplyColumnHeaderContent(GridColumn column, ref GridColumnHeader header)
        {
            if (column == null || header == null) return;

            int colIdx = column.ActualIndex;
            object content = column.Header;

            if (content is UIElement element)
            {
                header.ClearValue(GridRowHeader.ContentTemplateSelectorProperty);
                header.ClearValue(GridColumnHeader.ContentTemplateProperty);
                
                header.SetValue(GridRowHeader.ContentStringFormatProperty, column.HeaderStringFormat);
                header.SetValue(GridColumnHeader.ContentProperty, element);
                
                header.DataContext = null;
            }
            else
            {
                if (content == null)
                {
                    content = GetColumnHeaderContent(colIdx);
                }

                DataTemplate template = ChooseColumnHeaderTemplate(column, content);

                header.ClearValue(GridRowHeader.ContentTemplateSelectorProperty);
                header.ClearValue(GridRowHeader.ContentStringFormatProperty);
                header.SetValue(GridColumnHeader.ContentTemplateProperty, template);
                header.SetValue(GridColumnHeader.ContentProperty, content);
                header.DataContext = content;
            }
        }

        private DataTemplate ChooseColumnHeaderTemplate(GridColumn column, object content)
        {
            DataTemplate template = column.GetValue(GridColumn.HeaderTemplateProperty) as DataTemplate;
            DataTemplateSelector templateSelector = column.GetValue(GridColumn.HeaderTemplateSelectorProperty) as DataTemplateSelector;

            if (template == null && templateSelector != null)
            {
                template = templateSelector.SelectTemplate(content, column);
            }
            if (template == null)
            {
                template = column.DefaultHeaderTemplate;
            }

            return template;
        }

        internal object GetColumnHeaderContent(int colIndex)
        {
            int columnCount = _owner.Columns.Count;
            IGridSource source = GridSource;
            
            if (source != null && columnCount > colIndex)
            {
                string content = source.GetColumnHeaderAsString(colIndex);
                if (content != null) return content;
                return ToColumnName(colIndex);
            }

            return null;
        }
        #endregion

        #region RowHeaderContent
        public void ApplyRowHeaderContent(ref GridRowHeader header)
        {
            if (header == null || _owner == null) return;

            long rowIndex = header.RowIndex;
            object content = GetRowHeaderContent(rowIndex);
            string stringFormat = _owner.RowHeaderStringFormat;

            DataTemplate template = ChooseRowHeaderTemplate(header, content);

            if (template != null)
            {
                header.ClearValue(GridRowHeader.ContentTemplateSelectorProperty);
                header.SetValue(GridRowHeader.ContentStringFormatProperty, stringFormat);
                header.SetValue(GridRowHeader.ContentTemplateProperty, template);
                header.SetValue(GridRowHeader.ContentProperty, content);
            }
            else
            {
                var contentElement = GetDefaultRowHeaderElement(content, stringFormat);

                header.ClearValue(GridRowHeader.ContentTemplateSelectorProperty);
                header.ClearValue(GridRowHeader.ContentStringFormatProperty);
                header.ClearValue(GridRowHeader.ContentTemplateProperty);
                header.SetValue(GridRowHeader.ContentProperty, contentElement);
            }
        }

        public void UpdateRowHeaderContent(ref GridRowHeader header)
        {
            if (header == null || _owner == null) return;

            long rowIndex = header.RowIndex;
            object content = GetRowHeaderContent(rowIndex);

            object contentElement = header.GetValue(GridRowHeader.ContentProperty);
            if (contentElement is TextBlock textBlock)
            {
                string stringFormat = _owner.RowHeaderStringFormat;
                if (string.IsNullOrEmpty(stringFormat))
                    stringFormat = "{0}";

                if (content != null)
                {
                    textBlock.Text = string.Format(CultureInfo.CurrentCulture, stringFormat, content.ToString());
                }
                else
                {
                    textBlock.Text = string.Empty;
                }
            }
            else
            {
                DataTemplate template = ChooseRowHeaderTemplate(header, content);
                header.SetValue(GridRowHeader.ContentTemplateProperty, template);
                header.SetValue(GridRowHeader.ContentProperty, content);
            }
        }

        private DataTemplate ChooseRowHeaderTemplate(GridRowHeader header, object content)
        {
            DataTemplate template = _owner.GetValue(GridControl.RowHeaderTemplateProperty) as DataTemplate;
            DataTemplateSelector templateSelector = _owner.GetValue(GridControl.RowHeaderTemplateSelectorProperty) as DataTemplateSelector;

            if (template == null && templateSelector != null)
            {
                template = templateSelector.SelectTemplate(content, header);
            }

            return template;
        }

        private FrameworkElement GetDefaultRowHeaderElement(object content, string stringFormat)
        {
            TextBlock element = new TextBlock();

            element.Margin = GridControl.c_defaultRowHeaderPadding;
            element.HorizontalAlignment = HorizontalAlignment.Stretch;
            element.VerticalAlignment = VerticalAlignment.Center;
            element.TextAlignment = TextAlignment.Right;
            element.FontWeight = FontWeights.SemiBold;

            if (content != null)
            {
                if (string.IsNullOrEmpty(stringFormat))
                    stringFormat = "{0}";
                element.Text = string.Format(CultureInfo.CurrentCulture, stringFormat, content.ToString());
            }

            return element;
        }

        private object GetRowHeaderContent(long rowIndex)
        {
            IGridSource source = GridSource;
            if (source != null && source.RowsCount > rowIndex)
            {
                string content = source.GetRowHeaderAsString(rowIndex);
                if (content != null) return content;
                return ToRowName(rowIndex);
            }
            return null;
        }
        #endregion

        #region CellContent
        public void ApplyCellContent(GridCell cell, CellInfo cellInfo)
        {
            if (cell == null) return;
            if (CellInfo.IsNullOrInvalid(cellInfo))
            {
                cell.ClearValue(GridCell.ContentProperty);
                return;
            }

            GridColumn column = cell.Column;
            if (column == null)
            {
                column = Columns.ColumnCollection[cellInfo.ColumnIndex];
            }

            if (column == null)
            {
                cell.ClearValue(GridCell.ContentProperty);
                return;
            }

            object content = GetCellContent(cellInfo.RowIndex, cellInfo.ColumnIndex);
            if (content == null)
            {
                cell.ClearValue(GridCell.ContentProperty);
                cell.DataContext = null;
            }
            else
            {
                cell.SetValue(GridCell.ContentProperty, content);
                cell.DataContext = content;
            }
        }

        public void UpdateCellContent(GridCell cell, CellInfo cellInfo)
        {
            if (cell == null) return;
            if (CellInfo.IsNullOrInvalid(cellInfo))
            {
                cell.ClearValue(GridCell.ContentProperty);
                return;
            }
            
            object content = GetCellContent(cellInfo.RowIndex, cellInfo.ColumnIndex);
            cell.SetValue(GridCell.ContentProperty, content);
            cell.DataContext = content;
        }

        private object GetCellContent(long rowIndex, int colIndex)
        {
            IGridSource source = GridSource;
            if (source != null && source.RowsCount > rowIndex)
            {
                return source.GetCellDataAsString(rowIndex, colIndex);
            }
            return null;
        }
        #endregion

        #region Editing
        public bool CanCellEdit(CellInfo cellInfo)
        {
            IGridSource source = GridSource;
            if (source == null) return false;

            if (cellInfo != null && cellInfo.IsValid)
            {
                return source.IsCellEditable(cellInfo.RowIndex, cellInfo.ColumnIndex) > 0;
            }
            return false;
        }

        public bool IsEditingCell(CellInfo cellInfo)
        {
            if (_editingCell == null) return false;
            if (!_editingCell.IsValid) return false;
            return _editingCell.Equals(cellInfo);
        }

        public object BeginEdit(CellInfo cellInfo)
        {
            if (!CanCellEdit(cellInfo)) return null;

            object content = GetCellContent(cellInfo.RowIndex, cellInfo.ColumnIndex);
            if (IsEditingCell(cellInfo))
            {
                return content;
            }

            _editingCell = cellInfo.Clone();
            return content;
        }

        public bool CommitEdit(GridCell cell, FrameworkElement editingElement)
        {
            IGridSource source = GridSource;
            if (source == null) return false;

            if (cell == null) return false;
            CellInfo cellInfo = _editingCell;

            if (cellInfo == null) return false;
            if (!cellInfo.IsValid) return false;
            if (editingElement == null) return false;

            if (!cellInfo.Equals(cell.CellInfo)) return false;

            GridColumn column = cell.Column;
            if (column == null) return false;

            object value = column.GetEditedValue(editingElement);
            return source.SetCellDataFromControl(cellInfo.RowIndex, cellInfo.ColumnIndex, editingElement, value);
        }

        public void EndEdit()
        {
            _editingCell = CellInfo.Unset;
        }
        #endregion
        
        #endregion
    }
}
