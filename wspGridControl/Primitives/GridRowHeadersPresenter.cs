using System;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;

namespace wspGridControl.Primitives
{
    public class GridRowHeadersPresenter : GridContentPresenterBase
    {
        #region Variables
        private Pen _gridLinePen = null;
        #endregion

        #region Constructors
        static GridRowHeadersPresenter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GridRowHeadersPresenter), new FrameworkPropertyMetadata(typeof(GridRowHeadersPresenter)));
        }

        public GridRowHeadersPresenter()
            : base()
        {
        }
        #endregion

        #region Dependency Properties

        #region RowHeightProperty
        internal static readonly DependencyProperty RowHeightProperty = GridControl.RowHeightProperty.AddOwner(
            typeof(GridRowHeadersPresenter), new FrameworkPropertyMetadata(0.0,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        private double RowHeight
        {
            get => (double)GetValue(RowHeightProperty);
        }
        #endregion

        #region LineThicknessProperty
        internal static readonly DependencyProperty LineThicknessProperty = GridControl.LineThicknessProperty.AddOwner(
            typeof(GridRowHeadersPresenter), new FrameworkPropertyMetadata(0.5, FrameworkPropertyMetadataOptions.AffectsRender));

        private double LineThickness
        {
            get => (double)GetValue(LineThicknessProperty);
        }
        #endregion

        #region LineBrushProperty
        internal static readonly DependencyProperty LineBrushProperty = GridControl.LineBrushProperty.AddOwner(
            typeof(GridRowHeadersPresenter), new FrameworkPropertyMetadata(SystemColors.ActiveBorderBrush, FrameworkPropertyMetadataOptions.AffectsRender));

        private Brush LineBrush
        {
            get { return (Brush)GetValue(LineBrushProperty); }
        }
        #endregion

        #region AvailableRowsProperty
        internal static readonly DependencyProperty AvailableRowsProperty = GridControl.AvailableRowsProperty.AddOwner(
            typeof(GridScrollContent), new FrameworkPropertyMetadata(0,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
                new PropertyChangedCallback(PropertyChanged)));

        private int AvailableRows
        {
            get => (int)GetValue(AvailableRowsProperty);
        }
        #endregion

        #region RowStartIndexProperty
        internal static readonly DependencyProperty RowStartIndexProperty = GridControl.RowStartIndexProperty.AddOwner(
            typeof(GridRowHeadersPresenter), new FrameworkPropertyMetadata(0L));

        private long RowStartIndex
        {
            get { return (long)GetValue(RowStartIndexProperty); }
        }
        #endregion

        #region RowsCountProperty
        internal static readonly DependencyProperty RowsCountProperty = GridControl.RowsCountProperty.AddOwner(
            typeof(GridRowHeadersPresenter), new FrameworkPropertyMetadata(0L, FrameworkPropertyMetadataOptions.AffectsMeasure));

        private long RowsCount
        {
            get => (long)GetValue(RowsCountProperty);
        }
        #endregion

        private static void PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }
        #endregion

        #region Methods
        protected override Size MeasureOverride(Size constraint)
        {
            double rowHeight = Math.Max(0.0, RowHeight);

            UIElementCollection children = InternalChildren;

            int availableRows = AvailableRows;
            if (availableRows == 0)
            {
                return base.MeasureOverride(constraint);
            }

            Size layoutSlotSize = new Size(constraint.Width, rowHeight);

            for (int i = 0, count = children.Count; i < count; ++i)
            {
                UIElement child = children[i];
                if (child == null) continue;

                child.Measure(layoutSlotSize);
            }

            return base.MeasureOverride(constraint);
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            UIElementCollection children = InternalChildren;

            Rect rcChild = new Rect(arrangeSize);
            rcChild.Y = 0.5;
            double rowHeight = RowHeight;

            for (int i = 0, count = children.Count; i < count; ++i)
            {
                UIElement child = children[i];

                if (child == null) continue;

                rcChild.Height = rowHeight;
                child.Arrange(rcChild);

                rcChild.Y += rowHeight;
            }
            return arrangeSize;
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            double border = LineThickness;
            var borderBrush = LineBrush;

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

                double x1 = ActualWidth;
                double y1 = ActualHeight;

                dc.DrawLine(linePen, new Point(x1, 0), new Point(x1, y1));
            }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            var dp = e.Property;

            if (dp == RowStartIndexProperty)
            {
                UpdateAllHeaders();
            }
            else if (dp == AvailableRowsProperty || dp == RowsCountProperty)
            {
                NeedUpdateVisualTree = true;
                UpdateVisualTree();
                InvalidateVisual();
            }
        }

        protected override Geometry GetLayoutClip(Size layoutSlotSize)
        {
            var size = RenderSize;
            var rect = new Rect(0, 0, size.Width, size.Height);
            return new RectangleGeometry(rect);
        }

        internal override void UpdateVisualTree(bool forceUpdate = false)
        {
            if (NeedUpdateVisualTree || forceUpdate)
            {
                int colsCount = Columns != null ? Columns.Count : 0;
                long rowCount = RowsCount;

                if (rowCount == 0 || colsCount == 0)
                {
                    InternalChildren.Clear();
                    return;
                }

                int availableRows = (int)Math.Min(AvailableRows, rowCount);
                UIElementCollection children = InternalChildren;

                int totalChild = Math.Min(GridRowsPresenter.MaxRowOnScreen, (int)rowCount);
                if (totalChild < children.Count)
                {
                    int removed = children.Count - totalChild;
                    children.RemoveRange(totalChild, removed);
                }

                long startIndex = RowStartIndex;
                for (var i = 0; i < availableRows; i++)
                {
                    long displayIndex = startIndex + i;
                    if (i < children.Count)
                    {
                        children[i].SetValue(GridRowHeader.RowIndexPropertyKey, displayIndex);
                        UpdateHeader(children[i] as GridRowHeader);
                    }
                    else
                    {
                        CreateRowHeader(i, displayIndex);
                    }
                }

                NeedUpdateVisualTree = false;
            }
        }

        private void UpdateAllHeaders()
        {
            if (!IsPresenterVisualReady) return;
            
            GridControl owner = GridOwner;
            if (owner == null) return;

            long startIndex = RowStartIndex;
            UIElementCollection children = InternalChildren;
            ContentManager contentManager = owner.ContentMgr;

            for (var i = 0; i < children.Count; i++)
            {
                if (children[i] is GridRowHeader header)
                {
                    long rowIndex = startIndex + i;
                    header.SetValue(GridRowHeader.RowIndexPropertyKey, rowIndex);
                    contentManager.UpdateRowHeaderContent(ref header);
                }
            }
        }

        private void UpdateHeader(GridRowHeader header)
        {
            GridControl owner = GridOwner;
            if (owner == null) return;

            ContentManager contentManager = owner.ContentMgr;
            contentManager.ApplyRowHeaderContent(ref header);

            Helper.SyncProperty(owner, GridControl.RowHeaderContainerStyleProperty, header, GridRowHeader.StyleProperty);
            Helper.SyncProperty(owner, GridControl.RowHeaderContextMenuProperty, header, GridRowHeader.ContextMenuProperty);
            Helper.SyncProperty(owner, GridControl.RowHeaderToolTipProperty, header, GridRowHeader.ToolTipProperty);
        }

        private GridRowHeader CreateRowHeader(int index, long rowIndex)
        {
            if (AvailableRows == 0) return null;

            var header = new GridRowHeader();
            header.SetValue(GridRowHeader.RowIndexPropertyKey, rowIndex);

            InternalChildren.Insert(index, header);

            UpdateHeader(header);

            return header;
        }

        internal GridRowHeader GetHeaderFromPoint(Point pt)
        {
            Rect bounds = new Rect(0, 0, ActualWidth, RowHeight);

            foreach (var child in InternalChildren)
            {
                if (child is GridRowHeader header)
                {
                    if (bounds.Contains(pt))
                    {
                        return header;
                    }

                    bounds.Y += RowHeight;
                }
            }
            return null;
        }
        #endregion
    }
}
