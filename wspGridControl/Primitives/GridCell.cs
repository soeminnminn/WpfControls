using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace wspGridControl.Primitives
{
    public class GridCell : Decorator
    {
        #region Variables
        private GridControl _owner = null;

        private Pen _borderPen = null;
        #endregion

        #region Constructors
        static GridCell()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GridCell), new FrameworkPropertyMetadata(typeof(GridCell)));

            FocusableProperty.OverrideMetadata(typeof(GridCell), new FrameworkPropertyMetadata(true));
            KeyboardNavigation.TabNavigationProperty.OverrideMetadata(typeof(GridCell), new FrameworkPropertyMetadata(KeyboardNavigationMode.Local));

            SnapsToDevicePixelsProperty.OverrideMetadata(typeof(GridCell), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsArrange));
            ClipToBoundsProperty.OverrideMetadata(typeof(GridCell), new FrameworkPropertyMetadata(true));

            EventManager.RegisterClassHandler(typeof(GridCell), LostFocusEvent, new RoutedEventHandler(OnAnyLostFocus), true);
            EventManager.RegisterClassHandler(typeof(GridCell), GotFocusEvent, new RoutedEventHandler(OnAnyGotFocus), true);
        }

        public GridCell()
            : base()
        {
        }
        #endregion

        #region Dependency Properties

        #region ColumnProperty
        /// <summary>
        ///     The DependencyPropertyKey that allows writing the Column property value.
        /// </summary>
        public static readonly DependencyPropertyKey ColumnPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(Column), typeof(GridColumn), typeof(GridCell), 
            new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnColumnChanged)));

        /// <summary>
        ///     The DependencyProperty for the Columns property.
        /// </summary>
        public static readonly DependencyProperty ColumnProperty = ColumnPropertyKey.DependencyProperty;

        /// <summary>
        ///     The column that defines how this cell should appear.
        /// </summary>
        public GridColumn Column
        {
            get { return (GridColumn)GetValue(ColumnProperty); }
            internal set { SetValue(ColumnPropertyKey, value); }
        }

        private static void OnColumnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GridCell cell)
            {
                var column = e.NewValue as GridColumn;
                if (column != null)
                {
                    cell.Child = column.GetCellDisplayElement();
                }
                cell.OnColumnChanged((GridColumn)e.OldValue, (GridColumn)e.NewValue);
            }
        }

        protected virtual void OnColumnChanged(GridColumn oldColumn, GridColumn newColumn)
        { 
        }
        #endregion

        #region CellInfoProperty
        public static readonly DependencyProperty CellInfoProperty = DependencyProperty.Register(
            nameof(CellInfo), typeof(CellInfo), typeof(GridCell), null);

        public CellInfo CellInfo
        {
            get
            {
                CellInfo info = GetValue(CellInfoProperty) as CellInfo;
                if (info == null) info = CellInfo.Unset;
                return info;
            }
            set
            {
                if (value == null)
                {
                    SetValue(CellInfoProperty, CellInfo.Unset);
                }
                else
                {
                    SetValue(CellInfoProperty, value);
                }
            }
        }
        #endregion

        #region IsEditingProperty
        /// <summary>
        ///     Represents the IsEditing property.
        /// </summary>
        public static readonly DependencyProperty IsEditingProperty = DependencyProperty.Register(
            nameof(IsEditing), typeof(bool), typeof(GridCell), 
            new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnIsEditingChanged)));
        
        /// <summary>
        ///     Whether the cell is in editing mode.
        /// </summary>
        public bool IsEditing
        {
            get { return (bool)GetValue(IsEditingProperty); }
            set { SetValue(IsEditingProperty, value); }
        }

        private static void OnIsEditingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GridCell c)
            {
                bool isEditing = (bool)e.NewValue;
                c.Visibility = isEditing ? Visibility.Hidden : Visibility.Visible;

                if (!isEditing && c.IsKeyboardFocusWithin && !c.IsKeyboardFocused)
                {
                    c.Focus();
                }
                c.InvalidateVisual();
                
                c.OnIsEditingChanged((bool)e.NewValue);
            }
        }

        protected virtual void OnIsEditingChanged(bool isEditing)
        {   
        }
        #endregion

        #region IsSelectedProperty
        /// <summary>
        ///     Represents the IsSelected property.
        /// </summary>
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
            nameof(IsSelected), typeof(bool), typeof(GridCell), 
            new FrameworkPropertyMetadata(false,
                FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender,
                new PropertyChangedCallback(OnIsSelectedChanged)));

        /// <summary>
        ///     Whether the cell is selected or not.
        /// </summary>
        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        { }
        #endregion

        #region PaddingProperty
        public static readonly DependencyProperty PaddingProperty = DependencyProperty.Register(
            nameof(Padding), typeof(Thickness), typeof(GridCell),
            new FrameworkPropertyMetadata(GridControl.c_defaultCellRenderPadding));

        /// <summary>
        /// The Padding property inflates the effective size of the child by the specified thickness.  This
        /// achieves the same effect as adding margin on the child, but is present here for convenience.
        /// </summary>
        public Thickness Padding
        {
            get => (Thickness)GetValue(PaddingProperty);
            set { SetValue(PaddingProperty, value); }
        }
        #endregion

        #region ContentProperty
        public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(
            nameof(Content), typeof(object), typeof(GridCell),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, 
                new PropertyChangedCallback(OnContentChanged)));

        public object Content
        {
            get => GetValue(ContentProperty);
            set { SetValue(ContentProperty, value); }
        }

        private static void OnContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GridCell c)
            {
                c.RemoveLogicalChild(e.OldValue);
                c.AddLogicalChild(e.NewValue);
                c.UpdateChildContent();
            }
        }
        #endregion

        #region BorderThicknessProperty
        public static readonly DependencyProperty BorderThicknessProperty = DependencyProperty.Register(
            nameof(BorderThickness), typeof(double), typeof(GridCell),
            new FrameworkPropertyMetadata(2.0, 
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
                new PropertyChangedCallback(OnClearPenCache)));

        public double BorderThickness
        {
            get => (double) GetValue(BorderThicknessProperty);
            set { SetValue(BorderThicknessProperty, value); }
        }
        #endregion

        #region BorderBrushProperty
        public static readonly DependencyProperty BorderBrushProperty = DependencyProperty.Register(
            nameof(BorderBrush), typeof(Brush), typeof(GridCell),
            new FrameworkPropertyMetadata((Brush)null, 
                FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender,
                new PropertyChangedCallback(OnClearPenCache)));

        /// <summary>
        /// The BorderBrush property defines the brush used to fill the border region.
        /// </summary>
        public Brush BorderBrush
        {
            get => (Brush)GetValue(BorderBrushProperty);
            set { SetValue(BorderBrushProperty, value); }
        }
        #endregion

        #region BackgroundProperty
        public static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register(
            nameof(Background), typeof(Brush), typeof(GridCell),
            new FrameworkPropertyMetadata((Brush)null, 
                FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender));

        /// <summary>
        /// The Background property defines the brush used to fill the area within the border.
        /// </summary>
        public Brush Background
        {
            get => (Brush)GetValue(BackgroundProperty);
            set { SetValue(BackgroundProperty, value); }
        }
        #endregion

        private static void OnClearPenCache(DependencyObject d, DependencyPropertyChangedEventArgs e)
        { 
            if (d is GridCell c)
            {
                c._borderPen = null;
            }
        }
        #endregion

        #region Properties
        private GridControl GridOwner
        {
            get
            {
                if (_owner == null)
                {
                    if (Parent is GridRowsPresenter presenter)
                        _owner = presenter.GridOwner;
                    else if (Column != null && Column.GridOwner != null)
                        _owner = Column.GridOwner;
                    else
                        _owner = Helper.FindVisualParent<GridControl>(this);
                }

                return _owner;
            }
        }

        private ICellRenderer Renderer
        {
            get
            {
                var column = Column;
                if (column != null) return column.CellRenderer;
                return null;
            }
        }
        #endregion

        #region Methods
        protected override Size MeasureOverride(Size constraint)
        {
            UIElement child = Child;
            Size mySize = new Size();
            var borders = BorderThickness;

            Size border = HelperCollapseThickness(borders);
            Size padding = HelperCollapseThickness(this.Padding);

            if (child != null)
            {
                // Combine into total decorating size
                Size combined = new Size(border.Width + padding.Width, border.Height + padding.Height);

                // Remove size of border only from child's reference size.
                Size childConstraint = new Size(Math.Max(0.0, constraint.Width - combined.Width),
                                                Math.Max(0.0, constraint.Height - combined.Height));


                child.Measure(childConstraint);
                Size childSize = child.DesiredSize;

                // Now use the returned size to drive our size, by adding back the margins, etc.
                mySize.Width = childSize.Width + combined.Width;
                mySize.Height = childSize.Height + combined.Height;
            }
            else
            {
                // Combine into total decorating size
                mySize = new Size(border.Width + padding.Width, border.Height + padding.Height);
            }

            return mySize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var borders = BorderThickness;
            Rect boundRect = new Rect(finalSize);
            Rect innerRect = HelperDeflateRect(boundRect, borders);

            UIElement child = Child;
            if (child != null)
            {
                Rect childRect = HelperDeflateRect(innerRect, Padding);
                child.Arrange(childRect);
            }

            return finalSize;
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            if (!IsEditing)
            {
                var border = BorderThickness;
                Brush borderBrush;

                if (!DoubleUtil.IsZero(border) && !IsSelected && (borderBrush = BorderBrush) != null)
                {
                    Pen pen = _borderPen;
                    if (pen == null)
                    {
                        pen = new Pen();
                        pen.Brush = borderBrush;
                        pen.Thickness = border;
                        if (borderBrush.IsFrozen)
                        {
                            pen.Freeze();
                        }
                    }

                    double halfThickness = pen.Thickness * 0.5;
                    Rect rect = new Rect(
                        new Point(halfThickness, halfThickness),
                        new Point(RenderSize.Width - halfThickness, RenderSize.Height - halfThickness)
                    );

                    dc.DrawRectangle(null, pen, rect);
                }

                Brush background = Background;
                if (background != null && !IsSelected)
                {
                    Point ptTL = new Point(border, border);
                    Point ptBR = new Point(RenderSize.Width - border, RenderSize.Height - border);
                    if (ptBR.X > ptTL.X && ptBR.Y > ptTL.Y)
                    {
                        dc.DrawRectangle(background, null, new Rect(ptTL, ptBR));
                    }
                }

                if (VisualChildrenCount == 0)
                {
                    var content = Content;
                    var renderer = Renderer;
                    if (content != null && renderer != null)
                    {
                        Rect textBounds = new Rect();
                        var padding = Padding;

                        textBounds.Width = Math.Max(ActualWidth - (padding.Left + padding.Right), 0);
                        textBounds.Height = Math.Max(ActualHeight - (padding.Top + padding.Bottom), 0);

                        if (textBounds.Width > 0 && textBounds.Height > 0)
                        {
                            textBounds.Offset(padding.Left, padding.Top);
                            renderer.Draw(dc, content, textBounds);
                        }
                    }
                }
            }
        }

        protected override bool ShouldSerializeProperty(DependencyProperty dp)
        {
            if (dp == CellInfoProperty || dp == ContentProperty)
                return true;

            return false;
        }

        protected override Geometry GetLayoutClip(Size layoutSlotSize)
        {
            var size = RenderSize;
            var rect = new Rect(0, 0, size.Width, size.Height);
            return new RectangleGeometry(rect);
        }

        private static void OnAnyLostFocus(object sender, RoutedEventArgs e)
        {
            GridCell cell = Helper.FindVisualParent<GridCell>(e.OriginalSource as UIElement);
            if (cell != null && cell == sender)
            {
                GridControl owner = cell.GridOwner;
                if (owner != null && !cell.IsKeyboardFocusWithin && owner.FocusedCell == cell)
                {
                    owner.FocusedCell = null;
                }
            }
        }

        private static void OnAnyGotFocus(object sender, RoutedEventArgs e)
        {
            GridCell cell = Helper.FindVisualParent<GridCell>(e.OriginalSource as UIElement);
            if (cell != null && cell == sender)
            {
                GridControl owner = cell.GridOwner;
                if (owner != null)
                {
                    owner.FocusedCell = cell;
                }
            }
        }

        private void UpdateChildContent()
        {
            var column = Column;
            if (Child != null && Child is FrameworkElement element && column != null)
            {
                var content = Content;
                column.UpdateDisplayContent(element, content);
            }
        }

        // Helper function to add up the left and right size as width, as well as the top and bottom size as height
        private static Size HelperCollapseThickness(double th)
        {
            return new Size(th + th, th + th);
        }

        private static Size HelperCollapseThickness(Thickness th)
        {
            return new Size(th.Left + th.Right, th.Top + th.Bottom);
        }

        /// Helper to deflate rectangle by thickness
        private static Rect HelperDeflateRect(Rect rt, double thick)
        {
            return HelperDeflateRect(rt, new Thickness(thick));
        }

        private static Rect HelperDeflateRect(Rect rt, Thickness thick)
        {
            return new Rect(rt.Left + thick.Left,
                            rt.Top + thick.Top,
                            Math.Max(0.0, rt.Width - thick.Left - thick.Right),
                            Math.Max(0.0, rt.Height - thick.Top - thick.Bottom));
        }
        #endregion
    }
}
