using System;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Threading;

namespace wspGridControl.Primitives
{
    public class GridRowsPresenter : GridContentPresenterBase
    {
        #region Variables
        internal const int MaxRowOnScreen = 750;

        private int _availableRows = 0;
        private int _visibleRows = 0;
        private bool _isScrollEnded = false;
        private Pen _gridLinePen = null;
        private long _startIndex = 0;
        private int _startChangedColumn = 0;

        private CancellationTokenSource _tokenUpdateRows = null;
        #endregion

        #region Constructors
        static GridRowsPresenter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GridRowsPresenter), new FrameworkPropertyMetadata(typeof(GridRowsPresenter)));
        }

        public GridRowsPresenter()
            : base()
        {
        }
        #endregion

        #region Dependency Properties

        #region RowHeightProperty
        internal static readonly DependencyProperty RowHeightProperty = GridControl.RowHeightProperty.AddOwner(
            typeof(GridRowsPresenter), new FrameworkPropertyMetadata(0.0,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        private double RowHeight
        {
            get => (double)GetValue(RowHeightProperty);
        }
        #endregion

        #region LineThicknessProperty
        internal static readonly DependencyProperty LineThicknessProperty = GridControl.LineThicknessProperty.AddOwner(
            typeof(GridRowsPresenter), new FrameworkPropertyMetadata(0.5, FrameworkPropertyMetadataOptions.AffectsRender));

        private double LineThickness
        {
            get => (double)GetValue(LineThicknessProperty);
        }
        #endregion

        #region LineBrushProperty
        internal static readonly DependencyProperty LineBrushProperty = GridControl.LineBrushProperty.AddOwner(
            typeof(GridRowsPresenter), new FrameworkPropertyMetadata(SystemColors.ActiveBorderBrush, FrameworkPropertyMetadataOptions.AffectsRender));

        private Brush LineBrush
        {
            get { return (Brush)GetValue(LineBrushProperty); }
        }
        #endregion

        #region RowsCountProperty
        internal static readonly DependencyProperty RowsCountProperty = GridControl.RowsCountProperty.AddOwner(
            typeof(GridRowsPresenter), new FrameworkPropertyMetadata(0L, FrameworkPropertyMetadataOptions.AffectsMeasure));

        private long RowsCount
        {
            get => (long)GetValue(RowsCountProperty);
        }
        #endregion

        #endregion

        #region Properties
        internal long StartIndex
        {
            get => _startIndex;
        }

        internal int MaxRowCount
        {
            get
            {
                double rowHeight = RowHeight;
                if (rowHeight == 0) return 0;
                var screenHeight = SystemParameters.WorkArea.Height;
                if (screenHeight <= 0) return MaxRowOnScreen;

                return (int)Math.Ceiling(screenHeight / rowHeight);
            }
        }
        #endregion

        #region Methods
        protected override Size MeasureOverride(Size constraint)
        {
            double rowHeight = RowHeight;
            double constraintHeight = constraint.Height;
            bool desiredWidthListEnsured = false;

            if (_availableRows == 0)
            {
                CalculateAvailableRows(constraintHeight);
            }

            GridColumnsCollection columns = Columns;
            if (columns == null || _availableRows == 0) return new Size();

            var colIdx = _startChangedColumn;
            for (var c = colIdx; c < columns.Count; c++)
            {
                var column = columns[c];
                var cells = GetCellsForColumn(column.ActualIndex);

                for (var i = 0; i < cells.Length; i++)
                {
                    UIElement child = cells[i];
                    if (child == null) continue;

                    double childConstraintWidth = Math.Max(0.0, constraint.Width);

                    if (column.State == ColumnMeasureState.Init || column.State == ColumnMeasureState.Headered)
                    {
                        if (!desiredWidthListEnsured)
                        {
                            EnsureDesiredWidthList();
                            LayoutUpdated += new EventHandler(OnLayoutUpdated);
                            desiredWidthListEnsured = true;
                        }

                        // Measure child.
                        child.Measure(new Size(childConstraintWidth, rowHeight));
                        DesiredWidthList[column.ActualIndex] = column.DesiredWidth;
                    }
                    else if (column.State == ColumnMeasureState.Data)
                    {
                        childConstraintWidth = Math.Min(childConstraintWidth, column.DesiredWidth);
                        child.Measure(new Size(childConstraintWidth, rowHeight));
                    }
                    else // ColumnMeasureState.SpecificWidth
                    {
                        childConstraintWidth = Math.Min(childConstraintWidth, column.ColumnWidth);
                        child.Measure(new Size(childConstraintWidth, rowHeight));
                    }
                }
            }

            return constraint;
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            GridColumnsCollection columns = Columns;
            if (columns == null) return arrangeSize;

            double rowHeight = RowHeight;
            double offset = ScrollOffset.X;
            double accumulatedWidth = -offset;
            double remainingWidth = arrangeSize.Width + offset;

            var colIdx = _startChangedColumn;
            for (var c = 0; c < columns.Count; c++)
            {
                var column = columns[c];
                var cells = GetCellsForColumn(c);

                double childArrangeWidth = Math.Min(remainingWidth, (column.State == ColumnMeasureState.SpecificWidth) ? column.ColumnWidth : column.DesiredWidth);
                double y = 0.0;

                if (colIdx <= c)
                {
                    for (var i = 0; i < cells.Length; i++)
                    {
                        UIElement child = cells[i];
                        if (child == null) continue;

                        child.Arrange(new Rect(accumulatedWidth, y, childArrangeWidth, rowHeight));
                        y += rowHeight;
                    }
                }

                remainingWidth -= childArrangeWidth;
                accumulatedWidth += childArrangeWidth;
                if (remainingWidth < 0) break;
            }

            _startChangedColumn = 0;
            return arrangeSize;
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            double border = LineThickness;
            var borderBrush = LineBrush;
            var horizOffset = ScrollOffset.X;

            if (!DoubleUtil.IsZero(LineThickness) && borderBrush != null)
            {
                Pen linePen = _gridLinePen;
                if (linePen == null)
                {
                    linePen = new Pen();
                    linePen.Brush = borderBrush;
                    linePen.Thickness = border;

                    if (borderBrush.IsFrozen)
                    {
                        linePen.Freeze();
                    }
                }

                double rowHeight = RowHeight;
                var columns = Columns;
                int colsCount = columns == null ? 0 : columns.Count;

                int totalRows = _availableRows;
                long rowCount = RowsCount;
                if (rowCount > 0 && colsCount > 0)
                {
                    totalRows = (int)Math.Min(_availableRows, rowCount + 1);
                }

                double x = 0;
                double x1 = ActualWidth + horizOffset;
                double y = 0;
                double y1 = ActualHeight;
                
                for (int i = 0; i < totalRows; i++)
                {
                    y += rowHeight;
                    if (y < y1)
                    {
                        dc.DrawLine(linePen, new Point(x, y), new Point(x1, y));
                    }
                }

                if (columns != null && rowCount > 0)
                {
                    x = -horizOffset;
                    y = 0;
                    for (int c = 0; c < colsCount; c++)
                    {
                        double colWidth = columns[c].FinalWidth;
                        x += colWidth;
                        if (x >= 0 && x < x1)
                        {
                            dc.DrawLine(linePen, new Point(x, y), new Point(x, y1));
                        }
                    }
                }
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            if (sizeInfo.HeightChanged)
            {
                double height = sizeInfo.NewSize.Height;
                CalculateAvailableRows(height);
                InvalidateVisual();
            }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            DependencyProperty dp = e.Property;
            if (dp == RowsCountProperty)
            {
                NeedUpdateVisualTree = true;
                UpdateVisualTree();
                InvalidateMeasure();
            }
        }

        protected override Geometry GetLayoutClip(Size layoutSlotSize)
        {
            var size = RenderSize;
            var rect = new Rect(0, 0, size.Width, size.Height);
            long rowCount = RowsCount;

            if (_isScrollEnded && RowHeight > 0 && _visibleRows < rowCount)
            {
                rect.Height = (_visibleRows * RowHeight) + LineThickness;
            }

            return new RectangleGeometry(rect);
        }

        private void OnLayoutUpdated(object sender, EventArgs e)
        {
            bool desiredWidthChanged = false; // whether the shared minimum width has been changed since last layout

            GridColumnsCollection columns = Columns;
            if (columns != null)
            {
                foreach (GridColumn column in columns)
                {
                    if (column.State != ColumnMeasureState.SpecificWidth)
                    {
                        column.State = ColumnMeasureState.Data;

                        if (DesiredWidthList == null || column.ActualIndex >= DesiredWidthList.Count)
                        {
                            desiredWidthChanged = true;
                            break;
                        }

                        if (!DoubleUtil.AreClose(column.DesiredWidth, DesiredWidthList[column.ActualIndex]))
                        {
                            DesiredWidthList[column.ActualIndex] = column.DesiredWidth;
                            desiredWidthChanged = true;
                        }
                    }
                }
            }

            if (desiredWidthChanged)
            {
                InvalidateMeasure();
            }

            LayoutUpdated -= new EventHandler(OnLayoutUpdated);
        }

        protected override void OnScrollOffsetChanged(Vector oldValue, Vector newValue)
        {
            base.OnScrollOffsetChanged(oldValue, newValue);

            if (oldValue.Y != newValue.Y)
            {
                UpdateStartIndex();
                UpdateCells();
            }

            if (oldValue.X != newValue.X)
            {
                _startChangedColumn = 0;
                InvalidateArrange();
                InvalidateVisual();
            }
        }

        protected override void OnColumnCollectionChanged(ColumnCollectionChangedEventArgs e)
        {
            base.OnColumnCollectionChanged(e);

            if (e.Action == NotifyCollectionChangedAction.Move)
            {
                UpdateCells();
                InvalidateArrange();
                InvalidateVisual();
            }
            else
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                    case NotifyCollectionChangedAction.Remove:
                    case NotifyCollectionChangedAction.Replace:
                        {
                            NeedUpdateVisualTree = true;
                            UpdateVisualTree();
                        }
                        break;

                    case NotifyCollectionChangedAction.Reset:
                        InternalChildren.Clear();
                        break;

                    default:
                        break;
                }

                InvalidateMeasure();
            }
        }

        protected override void OnColumnPropertyChanged(GridColumn column, string propertyName)
        {
            if (column == null) return;

            if (GridColumn.WidthProperty.Name.Equals(propertyName))
            {
                int index = column.ActualIndex;
                if (index >= 0 && index < InternalChildren.Count)
                {
                    _startChangedColumn = index;
                    InvalidateMeasure();
                }

                InvalidateVisual();
            }
        }

        internal override void UpdateVisualTree(bool forceUpdate = false)
        {
            if (NeedUpdateVisualTree || forceUpdate)
            {
                long rowCount = RowsCount;
                if (rowCount == 0)
                {
                    InternalChildren.Clear();
                    return;
                }

                var columns = Columns.ColumnCollection;
                if (columns != null && rowCount > 0)
                {
                    long rowIdx = StartIndex;
                    int colCount = columns.Count;
                    int availableRows = (int)Math.Min(_availableRows, rowCount);

                    UIElementCollection children = InternalChildren;
                    int childCount = children.Count;

                    int totalChild = colCount * Math.Min(MaxRowCount, (int)rowCount);
                    if (totalChild < childCount)
                    {
                        int removed = childCount - totalChild;
                        children.RemoveRange(totalChild, removed);
                    }

                    int idx = 0;
                    for (int r = 0; r < availableRows; r++)
                    {
                        for (int c = 0; c < colCount; c++)
                        {
                            GridColumn column = columns[c];

                            CellInfo info = new CellInfo(idx, r, c, rowIdx, column.ActualIndex);
                            if (idx < childCount)
                            {
                                if (children[idx] is GridCell cell)
                                {
                                    ApplyCellData(cell, info, column);
                                }
                                else
                                {
                                    RenewCell(idx, info, column);
                                }
                            }
                            else
                            {
                                var cell = CreateCell(info, column);
                                InternalChildren.Add(cell);
                            }

                            idx++;
                        }
                        rowIdx++;
                    }
                }

                NeedUpdateVisualTree = false;
            }
        }

        private void CalculateAvailableRows(double height)
        {
            double rowHeight = RowHeight;

            int available = 0;
            int visible = 0;

            if (height > 0.0 && rowHeight > 0.0)
            {
                available = (int)Math.Round(height / rowHeight);
                visible = (int)Math.Floor(height / rowHeight);
            }

            UpdateAvailableRows(available, visible);
        }

        private async void UpdateAvailableRows(int available, int visible)
        {
            int current = _availableRows;
            if (current == available) return;

            if (_tokenUpdateRows != null)
            {
                _tokenUpdateRows.Cancel();
            }
            _tokenUpdateRows = new CancellationTokenSource();

            try
            {
                await Task.Run(() =>
                {
                    if (_tokenUpdateRows.IsCancellationRequested) return;
                    current = _availableRows = available;

                    // wait until VisualTree updated
                    while (NeedUpdateVisualTree && !_tokenUpdateRows.IsCancellationRequested)
                    {
                        Thread.Sleep(10);
                    }
                }, _tokenUpdateRows.Token);
            }
            catch(Exception)
            {
                return;
            }

            _visibleRows = visible;

            if (GridOwner != null)
            {
                GridOwner.SetValue(GridControl.AvailableRowsProperty, available);
                GridOwner.SetValue(GridControl.VisibleRowsProperty, visible);
            }

            if (!NeedUpdateVisualTree)
            {
                NeedUpdateVisualTree = true;
                UpdateVisualTree();
            }
            InvalidateVisual();
        }

        private void UpdateStartIndex()
        {
            double delta = GridScrollViewer.ScrollLineDelta;
            double offset = ScrollOffset.Y;
            long start = (long)(offset / delta);

            long rowCount = RowsCount;
            double height = ActualHeight;
            if (rowCount > 0 && height > 0)
            {   
                long num = start + _visibleRows + 1;
                _isScrollEnded = num >= rowCount;
            }

            _startIndex = start;

            if (GridOwner != null)
            {
                GridOwner.SetValue(GridControl.RowStartIndexProperty, start);
            }
        }

        internal void UpdateCellsSelection()
        {
            if (NeedUpdateVisualTree || InternalChildren.Count == 0) return;

            SelectionManager selection = GridOwner != null ? GridOwner.SelectionMgr : null;
            if (selection == null) return;

            UIElementCollection children = InternalChildren;
            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] is GridCell cell)
                {
                    CellInfo info = cell.CellInfo;
                    if (info == null) continue;

                    if (selection.IsCellSelected(info))
                    {
                        cell.SetValue(GridCell.IsSelectedProperty, true);
                    }
                    else
                    {
                        cell.SetValue(GridCell.IsSelectedProperty, false);
                    }
                }
            }
        }

        private void UpdateCells()
        {
            if (NeedUpdateVisualTree || InternalChildren.Count == 0) return;

            GridControl owner = GridOwner;
            if (owner == null) return;
            
            SelectionManager selectionMgr = owner.SelectionMgr;
            ContentManager contentManager = owner.ContentMgr;

            UIElementCollection children = InternalChildren;
            long startIndex = StartIndex;

            var columns = Columns;
            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] is GridCell cell)
                {
                    CellInfo oldInfo = cell.CellInfo;
                    if (oldInfo == null) continue;

                    long rowIndex = startIndex + oldInfo.DisplayRowIndex;

                    GridColumn column = cell.Column;
                    if (column == null) continue;

                    CellInfo info = new CellInfo(oldInfo, rowIndex, column.ActualIndex);
                    info.DisplayColumnIndex = columns.IndexOf(column);
                    cell.SetValue(GridCell.CellInfoProperty, info);

                    bool canFocus = info.DisplayRowIndex < _visibleRows;
                    cell.SetValue(GridCell.FocusableProperty, canFocus);

                    bool isSelected = info.IsValid && selectionMgr.IsCellSelected(info);
                    cell.SetValue(GridCell.IsSelectedProperty, isSelected);

                    var isEditing = contentManager.IsEditingCell(info);
                    cell.SetValue(GridCell.IsEditingProperty, isEditing);

                    contentManager.UpdateCellContent(cell, info);
                }
            }
        }

        private void ApplyCellData(GridCell cell, CellInfo cellInfo, GridColumn column)
        {
            if (cell == null || cellInfo == null || GridOwner == null) return;

            if (column == null)
            {
                cell.SetValue(GridCell.CellInfoProperty, CellInfo.Unset);
                cell.ClearValue(GridCell.ContentProperty);
                cell.SetValue(GridCell.FocusableProperty, false);
                return;
            }

            long rowCount = RowsCount;
            if (rowCount == 0 || cellInfo.RowIndex >= rowCount)
            {
                cellInfo.MakeUnset();
                cell.ClearValue(GridCell.ContentProperty);
                cell.SetValue(GridCell.FocusableProperty, false);
                return;
            }

            cell.SetValue(GridCell.CellInfoProperty, cellInfo);
            cell.SetValue(GridCell.ColumnPropertyKey, column);

            bool canFocus = cellInfo.DisplayRowIndex < _visibleRows;
            cell.SetValue(GridCell.FocusableProperty, canFocus);

            Helper.SyncProperty(column, GridColumn.CellContainerStyleProperty, cell, GridCell.StyleProperty);

            GridControl owner = GridOwner;
            if (owner != null)
            {
                SelectionManager selectionMgr = GridOwner.SelectionMgr;
                bool isSelected = cellInfo.IsValid && selectionMgr.IsCellSelected(cellInfo);
                cell.SetValue(GridCell.IsSelectedProperty, isSelected);

                ContentManager contentManager = owner.ContentMgr;

                var isEditing = contentManager.IsEditingCell(cellInfo);
                cell.SetValue(GridCell.IsEditingProperty, isEditing);

                contentManager.ApplyCellContent(cell, cellInfo);
            }
        }

        private GridCell[] GetCellsForColumn(int colIndex)
        {
            var totalCells = _availableRows;

            if (NeedUpdateVisualTree || InternalChildren.Count == 0 || totalCells == 0)
            {
                return new GridCell[0];
            }

            var children = InternalChildren;
            var result = new GridCell[totalCells];

            for (var i = 0; i < children.Count; i++)
            {
                var child = children[i];
                if (child is GridCell cell)
                {
                    var info = cell.CellInfo;
                    if (info == null) continue;

                    if (info.IsValid && info.DisplayColumnIndex == colIndex)
                    {
                        var rowIdx = info.DisplayRowIndex;
                        if (rowIdx >= 0 && rowIdx < totalCells)
                        {
                            result[rowIdx] = cell;
                        }
                    }
                }
            }

            return result;
        }

        private GridCell CreateCell(CellInfo cellInfo, GridColumn column)
        {
            if (column == null) return null;

            var cell = new GridCell();
            ApplyCellData(cell, cellInfo, column);

            return cell;
        }

        private void RenewCell(int index, CellInfo cellInfo, GridColumn column)
        {
            var children = InternalChildren;
            if (children != null && children.Count > index)
            {
                InternalChildren.RemoveAt(index);
                InternalChildren.Insert(index, CreateCell(cellInfo, column));
            }
        }

        internal void SetCellIsEditing(CellInfo cellInfo, bool isEditing)
        {
            if (NeedUpdateVisualTree || InternalChildren.Count == 0) return;

            UIElementCollection children = InternalChildren;
            if (isEditing)
            {
                for (int i = 0; i < children.Count; i++)
                {
                    if (children[i] is GridCell cell)
                    {
                        CellInfo info = cell.CellInfo;
                        bool isCellEditing = info != null && info.IsValid && info.Equals(cellInfo);
                        cell.SetValue(GridCell.IsEditingProperty, isCellEditing);
                    }
                }
            }
            else
            {
                for (int i = 0; i < children.Count; i++)
                {
                    if (children[i] is GridCell cell)
                    {
                        cell.SetValue(GridCell.IsEditingProperty, false);
                    }
                }
            }
        }

        internal GridCell FindCell(long rowIndex, int colIndex)
        {
            var children = InternalChildren;
            if (children != null)
            {
                for (int i = 0; i < children.Count; i++)
                {
                    if (children[i] is GridCell cell)
                    {
                        CellInfo cellInfo = cell.CellInfo;
                        if (cellInfo != null && cellInfo.RowIndex == rowIndex && cellInfo.ColumnIndex == colIndex)
                            return cell;
                    }
                }
            }
            return null;
        }

        private RelativeMousePositions GetRelativeMousePosition(Point pt)
        {
            RelativeMousePositions position = RelativeMousePositions.Over;

            var offset = VisualTreeHelper.GetOffset(this);
            var lastVisibleIdx = _visibleRows - 1;
            var height = lastVisibleIdx * RowHeight;

            var bounds = new Rect(offset.X, offset.Y, ActualWidth, height);
            if (bounds.IsEmpty) return position;

            if (DoubleUtil.LessThan(pt.X, bounds.Left))
            {
                position |= RelativeMousePositions.Left;
            }
            else if (DoubleUtil.GreaterThan(pt.X, bounds.Right))
            {
                position |= RelativeMousePositions.Right;
            }

            if (DoubleUtil.LessThan(pt.Y, bounds.Top))
            {
                position |= RelativeMousePositions.Above;
            }
            else if (DoubleUtil.GreaterThan(pt.Y, bounds.Bottom))
            {
                position |= RelativeMousePositions.Below;
            }

            return position;
        }

        internal GridCell GetNearestCellFromPoint(Point pt, out RelativeMousePositions position)
        {
            position = GetRelativeMousePosition(pt);
            var lastVisibleIdx = _visibleRows - 1;

            foreach (var child in InternalChildren)
            {
                if (child is GridCell cell)
                {
                    CellInfo cellInfo = cell.CellInfo;
                    if (cellInfo == null) continue;

                    Rect bounds = CalculateCellBounds(cellInfo);
                    if (position.HasFlag(RelativeMousePositions.Above) && cellInfo.DisplayRowIndex == 0 &&
                        DoubleUtil.IsBetween(pt.X, bounds.X, bounds.Right) && DoubleUtil.GreaterThanOrClose(bounds.Top, pt.Y))
                    {
                        return cell;
                    }
                    else if (position.HasFlag(RelativeMousePositions.Below) && cellInfo.DisplayRowIndex == lastVisibleIdx &&
                        DoubleUtil.IsBetween(pt.X, bounds.X, bounds.Right) && DoubleUtil.LessThanOrClose(bounds.Bottom, pt.Y))
                    {
                        return cell;
                    }

                    if (bounds.Contains(pt))
                    {
                        return cell;
                    }
                }
            }
            return null;
        }

        internal GridCell GetCellFromPoint(Point pt)
        {
            foreach (var child in InternalChildren)
            {
                if (child is GridCell cell)
                {
                    CellInfo cellInfo = cell.CellInfo;
                    if (cellInfo == null) continue;

                    Rect bounds = CalculateCellBounds(cellInfo);
                    if (bounds.Contains(pt))
                    {
                        return cell;
                    }
                }
            }
            return null;
        }

        private Rect CalculateCellBounds(CellInfo info)
        {
            Rect bounds = new Rect();
            if (info == null) return bounds;
            if (!info.IsValid) return bounds;

            var rowIdx = info.DisplayRowIndex;
            var colIdx = info.DisplayColumnIndex;

            bounds.X = -ScrollOffset.X;
            double prevWidth = 0.0;
            var columns = Columns;
            for (int c = 0; c <= colIdx; c++)
            {
                bounds.X += prevWidth;
                bounds.Width = prevWidth = columns[c].FinalWidth;
            }

            bounds.Y = rowIdx * RowHeight;
            bounds.Height = RowHeight;

            return bounds;
        }
        #endregion
    }
}
