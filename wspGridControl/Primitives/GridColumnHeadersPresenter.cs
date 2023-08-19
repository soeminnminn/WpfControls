using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace wspGridControl.Primitives
{
    public class GridColumnHeadersPresenter : GridContentPresenterBase
    {
        #region Variables
        private const double c_thresholdX = 4.0;

        private List<Rect> _headersPositionList = null;

        private GridControl _parentControl;
        private GridColumnHeader _paddingHeader = null;
        private GridColumnHeader _floatingHeader = null;
        private Separator _indicator;

        private bool _isColumnChangedOrCreated;

        private GridColumnHeader _draggingSrcHeader;

        private bool _isHeaderDragging = false;
        private bool _prepareDragging;

        private Point _startPos;
        private Point _relativeStartPos;
        private Point _currentPos;

        private int _startColumnIndex;
        private int _desColumnIndex;

        private readonly TranslateTransform _translate;
        #endregion

        #region Constructors
        static GridColumnHeadersPresenter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GridColumnHeadersPresenter), new FrameworkPropertyMetadata(typeof(GridColumnHeadersPresenter)));
        }

        public GridColumnHeadersPresenter()
            : base()
        {
            _translate = new TranslateTransform();
            RenderTransform = _translate;
        }
        #endregion

        #region Dependency Properties

        #region ColumnHeaderContainerStyleProperty
        /// <summary>
        /// ColumnHeaderContainerStyleProperty DependencyProperty
        /// </summary>
        internal static readonly DependencyProperty ColumnHeaderContainerStyleProperty = GridControl.ColumnHeaderContainerStyleProperty.AddOwner(
            typeof(GridColumnHeadersPresenter),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(PropertyChanged)));

        /// <summary>
        /// header container's style
        /// </summary>
        public Style ColumnHeaderContainerStyle
        {
            get { return (Style)GetValue(ColumnHeaderContainerStyleProperty); }
            set { SetValue(ColumnHeaderContainerStyleProperty, value); }
        }
        #endregion

        #region ColumnHeaderTemplateProperty
        /// <summary>
        /// ColumnHeaderTemplate DependencyProperty
        /// </summary>
        internal static readonly DependencyProperty ColumnHeaderTemplateProperty = GridControl.ColumnHeaderTemplateProperty.AddOwner(
            typeof(GridColumnHeadersPresenter),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(PropertyChanged)));

        /// <summary>
        /// column header template
        /// </summary>
        public DataTemplate ColumnHeaderTemplate
        {
            get { return (DataTemplate)GetValue(ColumnHeaderTemplateProperty); }
            set { SetValue(ColumnHeaderTemplateProperty, value); }
        }
        #endregion

        #region ColumnHeaderTemplateSelectorProperty
        /// <summary>
        /// ColumnHeaderTemplateSelector DependencyProperty
        /// </summary>
        internal static readonly DependencyProperty ColumnHeaderTemplateSelectorProperty = GridControl.ColumnHeaderTemplateSelectorProperty.AddOwner(
            typeof(GridColumnHeadersPresenter),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(PropertyChanged)));

        /// <summary>
        /// header template selector
        /// </summary>
        /// <remarks>
        ///     This property is ignored if <seealso cref="ColumnHeaderTemplate"/> is set.
        /// </remarks>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DataTemplateSelector ColumnHeaderTemplateSelector
        {
            get { return (DataTemplateSelector)GetValue(ColumnHeaderTemplateSelectorProperty); }
            set { SetValue(ColumnHeaderTemplateSelectorProperty, value); }
        }
        #endregion

        #region ColumnHeaderStringFormatProperty
        /// <summary>
        /// ColumnHeaderStringFormat DependencyProperty
        /// </summary>
        internal static readonly DependencyProperty ColumnHeaderStringFormatProperty = GridControl.ColumnHeaderStringFormatProperty.AddOwner(
            typeof(GridColumnHeadersPresenter),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(PropertyChanged)));

        /// <summary>
        /// header template selector
        /// </summary>
        /// <remarks>
        ///     This property is ignored if <seealso cref="ColumnHeaderTemplate"/> is set.
        /// </remarks>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string ColumnHeaderStringFormat
        {
            get { return (string)GetValue(ColumnHeaderStringFormatProperty); }
            set { SetValue(ColumnHeaderStringFormatProperty, value); }
        }
        #endregion

        #region AllowsColumnReorderProperty
        /// <summary>
        /// AllowsColumnReorder DependencyProperty
        /// </summary>
        internal static readonly DependencyProperty AllowsColumnReorderProperty = GridControl.AllowsColumnReorderProperty.AddOwner(
            typeof(GridColumnHeadersPresenter));

        /// <summary>
        /// Allow column re-order or not
        /// </summary>
        public bool AllowsColumnReorder
        {
            get { return (bool)GetValue(AllowsColumnReorderProperty); }
            set { SetValue(AllowsColumnReorderProperty, value); }
        }
        #endregion

        #region ColumnHeaderContextMenuProperty
        /// <summary>
        /// ColumnHeaderContextMenu DependencyProperty
        /// </summary>
        internal static readonly DependencyProperty ColumnHeaderContextMenuProperty = GridControl.ColumnHeaderContextMenuProperty.AddOwner(
            typeof(GridColumnHeadersPresenter),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(PropertyChanged)));

        /// <summary>
        /// ColumnHeaderContextMenu
        /// </summary>
        public ContextMenu ColumnHeaderContextMenu
        {
            get { return (ContextMenu)GetValue(ColumnHeaderContextMenuProperty); }
            set { SetValue(ColumnHeaderContextMenuProperty, value); }
        }
        #endregion

        #region ColumnHeaderToolTipProperty
        /// <summary>
        /// ColumnHeaderToolTip DependencyProperty
        /// </summary>
        internal static readonly DependencyProperty ColumnHeaderToolTipProperty = GridControl.ColumnHeaderToolTipProperty.AddOwner(
            typeof(GridColumnHeadersPresenter),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(PropertyChanged)));

        /// <summary>
        /// ColumnHeaderToolTip
        /// </summary>
        public object ColumnHeaderToolTip
        {
            get { return GetValue(ColumnHeaderToolTipProperty); }
            set { SetValue(ColumnHeaderToolTipProperty, value); }
        }
        #endregion

        private static void PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GridColumnHeadersPresenter presenter)
                presenter.UpdateAllHeaders(e.Property);
        }

        #endregion

        #region Properties
        private List<Rect> HeadersPositionList
        {
            get
            {
                if (_headersPositionList == null)
                {
                    _headersPositionList = new List<Rect>();
                }

                return _headersPositionList;
            }
        }
        #endregion

        #region Methods
        protected override Size MeasureOverride(Size constraint)
        {
            GridColumnsCollection columns = Columns;
            UIElementCollection children = InternalChildren;

            double maxHeight = 0.0;           // Max height of children.
            double accumulatedWidth = 0.0;    // Total width consumed by children.
            double constraintHeight = constraint.Height;
            double constraintWidth = constraint.Width;
            bool desiredWidthListEnsured = false;

            if (columns != null)
            {
                // Measure working headers
                for (int i = 0; i < columns.Count && i < children.Count; ++i)
                {
                    UIElement child = children[GetVisualIndex(i)];
                    if (child == null) continue;

                    double childConstraintWidth = Math.Max(0.0, constraintWidth);

                    GridColumn column = columns[i];

                    if (column.State == ColumnMeasureState.Init)
                    {
                        if (!desiredWidthListEnsured)
                        {
                            EnsureDesiredWidthList();
                            LayoutUpdated += new EventHandler(OnLayoutUpdated);
                            desiredWidthListEnsured = true;
                        }

                        child.Measure(new Size(childConstraintWidth, constraintHeight));

                        DesiredWidthList[column.ActualIndex] = column.EnsureWidth(child.DesiredSize.Width);

                        accumulatedWidth += column.DesiredWidth;
                    }
                    else if (column.State == ColumnMeasureState.Headered || column.State == ColumnMeasureState.Data)
                    {
                        childConstraintWidth = Math.Min(childConstraintWidth, column.DesiredWidth);

                        child.Measure(new Size(childConstraintWidth, constraintHeight));

                        accumulatedWidth += column.DesiredWidth;
                    }
                    else // ColumnMeasureState.SpecificWidth
                    {
                        childConstraintWidth = Math.Min(childConstraintWidth, column.ColumnWidth);

                        child.Measure(new Size(childConstraintWidth, constraintHeight));

                        accumulatedWidth += column.ColumnWidth;
                    }

                    maxHeight = Math.Max(maxHeight, child.DesiredSize.Height);
                }
            }

            if (_paddingHeader != null)
            {
                // Measure padding header
                _paddingHeader.Measure(new Size(0.0, constraintHeight));
                maxHeight = Math.Max(maxHeight, _paddingHeader.DesiredSize.Height);

                // reserve space for padding header next to the last column
                accumulatedWidth += c_PaddingHeaderMinWidth;
            }

            // Measure indicator & floating header in re-ordering
            if (_isHeaderDragging && _indicator != null && _floatingHeader != null)
            {
                // Measure indicator
                _indicator.Measure(constraint);

                // Measure floating header
                _floatingHeader.Measure(constraint);
            }

            return new Size(constraintWidth, maxHeight);
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            GridColumnsCollection columns = Columns;
            UIElementCollection children = InternalChildren;

            double offset = ScrollOffset.X;
            double accumulatedWidth = 0.0;
            double remainingWidth = arrangeSize.Width + offset;
            Rect rect;

            HeadersPositionList.Clear();

            if (columns != null)
            {
                for (int i = 0; i < columns.Count && i < children.Count; ++i)
                {
                    UIElement child = children[GetVisualIndex(i)];
                    if (child == null) continue;

                    GridColumn column = columns[i];

                    double childArrangeWidth = Math.Min(remainingWidth, (column.State == ColumnMeasureState.SpecificWidth) ? column.ColumnWidth : column.DesiredWidth);

                    rect = new Rect(accumulatedWidth, 0.0, childArrangeWidth, arrangeSize.Height);
                    child.Arrange(rect);

                    HeadersPositionList.Add(rect);

                    remainingWidth -= childArrangeWidth;
                    accumulatedWidth += childArrangeWidth;
                }

                if (_isColumnChangedOrCreated)
                {
                    for (int i = 0; i < columns.Count; ++i)
                    {
                        GridColumnHeader header = children[GetVisualIndex(i)] as GridColumnHeader;
                        header.CheckWidthForPreviousHeaderGripper();
                    }

                    if (_paddingHeader != null)
                        _paddingHeader.CheckWidthForPreviousHeaderGripper();

                    _isColumnChangedOrCreated = false;
                }
            }

            // Arrange padding header
            rect = new Rect(accumulatedWidth, 0.0, Math.Max(remainingWidth, 0.0), arrangeSize.Height);
            if (_paddingHeader != null)
                _paddingHeader.Arrange(rect);

            HeadersPositionList.Add(rect);

            // if re-order started, arrange floating header & indicator
            if (_isHeaderDragging && _indicator != null && _floatingHeader != null)
            {
                _floatingHeader.Arrange(new Rect(new Point(_currentPos.X - _relativeStartPos.X, 0), HeadersPositionList[_startColumnIndex].Size));

                Point pos = FindPositionByIndex(_desColumnIndex);
                _indicator.Arrange(new Rect(pos, new Size(_indicator.DesiredSize.Width, arrangeSize.Height)));
            }

            return arrangeSize;
        }

        protected override Geometry GetLayoutClip(Size layoutSlotSize)
        {
            var size = RenderSize;
            var offset = ScrollOffset.X;
            var rect = new Rect(offset, 0, size.Width, size.Height);
            return new RectangleGeometry(rect);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            GridColumnHeader header = e.Source as GridColumnHeader;

            if (header != null && AllowsColumnReorder)
            {
                PrepareHeaderDrag(header, e.GetPosition(this), e.GetPosition(header), false);

                MakeParentControlGotFocus();
            }

            e.Handled = true;

            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            // Important to clean the prepare dragging state
            _prepareDragging = false;

            if (_isHeaderDragging)
            {
                FinishHeaderDrag(false);
            }

            e.Handled = true;

            base.OnMouseLeftButtonUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // if prepare re-order or already re-order
                if (_prepareDragging && _draggingSrcHeader != null)
                {
                    _currentPos = e.GetPosition(this);
                    _desColumnIndex = FindIndexByPosition(_currentPos, true);

                    if (!_isHeaderDragging)
                    {
                        // Re-order begins only if horizontal move exceeds threshold
                        if (CheckStartHeaderDrag(_currentPos, _startPos))
                        {
                            // header dragging start
                            StartHeaderDrag();

                            // need to measure indicator because floating header is updated
                            InvalidateMeasure();
                        }
                    }
                    else // NOTE: Not-Dragging/Dragging should be divided into two stages in MouseMove
                    {
                        // Check floating & indicator visibility
                        // Display floating header if vertical move not exceeds header.Height * 2
                        bool isDisplayingFloatingHeader = IsMousePositionValid(_floatingHeader, _currentPos, 2.0);

                        // floating header and indicator are visibile/invisible at the same time
                        _indicator.Visibility = _floatingHeader.Visibility = isDisplayingFloatingHeader ? Visibility.Visible : Visibility.Hidden;

                        InvalidateArrange();
                    }
                }
            }

            e.Handled = true;
        }

        protected override void OnLostMouseCapture(MouseEventArgs e)
        {
            base.OnLostMouseCapture(e);

            // OnLostMouseCapture is invoked before OnMouseLeftButtonUp, so we need to distinguish
            // the cause of capture lose
            //      if LeftButton is pressed when lost mouse capture, we treat it as cancel
            //      Because GridHeaderRowPresenter never capture Mouse (GridColumnHeader did this),
            //      the Mouse.Captured is always null
            if (e.LeftButton == MouseButtonState.Pressed && _isHeaderDragging)
            {
                FinishHeaderDrag(true);
            }

            // Important to clean the prepare dragging state
            _prepareDragging = false;
        }

        internal override void UpdateVisualTree(bool forceUpdate = false)
        {
            if (NeedUpdateVisualTree || forceUpdate)
            {
                // IMPORTANT!
                // The correct sequence to build the VisualTree in Z-order:
                // 1. Padding header
                // 2. The working Column header (if any)
                // 3. Indicator
                // 4. Floating header
                //

                UIElementCollection children = InternalChildren;
                GridColumnsCollection columns = Columns;
                RenewEvents();

                if (children.Count == 0)
                {
                    AddPaddingColumnHeader();
                    AddIndicator();
                    AddFloatingHeader(null);
                }
                else if (children.Count > 3)
                {
                    int count = children.Count - 3;
                    for (int i = 0; i < count; i++)
                    {
                        RemoveHeader(null, 1);
                    }
                }

                UpdateHeaderProperties(_paddingHeader);

                if (columns != null)
                {
                    int visualIndex = 1;

                    for (int columnIndex = columns.Count - 1; columnIndex >= 0; columnIndex--)
                    {
                        GridColumn column = columns[columnIndex];
                        CreateAndInsertHeader(column, visualIndex++);
                    }
                }

                BuildHeaderLinks();

                NeedUpdateVisualTree = false;
                _isColumnChangedOrCreated = true;
            }

            if (IsPresenterVisualReady && forceUpdate)
            {
                InvalidateMeasure();
            }
        }

        protected override void OnColumnPropertyChanged(GridColumn column, string propertyName)
        {
            if (column != null && column.ActualIndex >= 0)
            {
                GridColumnHeader header = FindHeaderByColumn(column);
                if (header != null)
                {
                    if (GridColumn.WidthProperty.Name.Equals(propertyName) || GridColumn.c_ActualWidthName.Equals(propertyName))
                    {
                        InvalidateMeasure();
                    }
                    else if (GridColumn.HeaderProperty.Name.Equals(propertyName))
                    {
                        if (!header.IsInternalGenerated || column.Header is GridColumnHeader)
                        {
                            int i = InternalChildren.IndexOf(header);
                            RemoveHeader(header, -1);
                            CreateAndInsertHeader(column, i);
                            BuildHeaderLinks();
                        }
                        else
                        {
                            UpdateHeaderContent(header);
                        }
                    }
                    else if (GridColumn.ColumnInfoProperty.Name.Equals(propertyName))
                    {
                        UpdateHeaderProperties(header, column);
                    }
                }
            }
        }

        protected override void OnColumnCollectionChanged(ColumnCollectionChangedEventArgs e)
        {
            base.OnColumnCollectionChanged(e);

            int index;
            GridColumnHeader header;
            UIElementCollection children = InternalChildren;
            GridColumn column;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Move:
                    int start = GetVisualIndex(e.OldStartingIndex);
                    int end = GetVisualIndex(e.NewStartingIndex);

                    header = (GridColumnHeader)children[start];
                    children.RemoveAt(start);
                    children.Insert(end, header);

                    break;

                case NotifyCollectionChangedAction.Add:
                    index = GetVisualIndex(e.NewStartingIndex);
                    column = (GridColumn)(e.NewItems[0]);

                    CreateAndInsertHeader(column, index + 1); // index + 1 because visual index is reversed from column index

                    break;

                case NotifyCollectionChangedAction.Remove:
                    RemoveHeader(null, GetVisualIndex(e.OldStartingIndex));

                    break;

                case NotifyCollectionChangedAction.Replace:
                    index = GetVisualIndex(e.OldStartingIndex);
                    RemoveHeader(null, index);

                    column = (GridColumn)(e.NewItems[0]);
                    CreateAndInsertHeader(column, index);

                    break;

                case NotifyCollectionChangedAction.Reset:
                    int count = e.ClearedColumns.Count;
                    for (int i = 0; i < count; i++)
                    {
                        RemoveHeader(null, 1);
                    }

                    break;
            }

            // Link headers
            BuildHeaderLinks();
            _isColumnChangedOrCreated = true;
        }

        protected override void OnScrollOffsetChanged(Vector oldValue, Vector newValue)
        {
            base.OnScrollOffsetChanged(oldValue, newValue);

            if (oldValue.X != newValue.X)
            {
                _translate.X = -newValue.X;
                InvalidateVisual();
            }
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
                        if (column.State == ColumnMeasureState.Init)
                        {
                            column.State = ColumnMeasureState.Headered;
                        }

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

        private void OnColumnHeadersPresenterKeyDown(object sender, KeyEventArgs e)
        {
            // if press Escape when re-ordering, cancel re-order
            if (e.Key == Key.Escape && _isHeaderDragging)
            {
                // save the source header b/c FinishHeaderDrag will clear it
                GridColumnHeader srcHeader = _draggingSrcHeader;

                FinishHeaderDrag(true);
                PrepareHeaderDrag(srcHeader, _currentPos, _relativeStartPos, true);
                InvalidateArrange();
            }
        }

        private int GetVisualIndex(int columnIndex)
        {
            // Elements in visual tree: working headers, padding header, indicator, and floating header
            int index = InternalChildren.Count - 3 - columnIndex;
            return index;
        }

        private void GetIndexRange(DependencyProperty dp, out int iStart, out int iEnd)
        {
            // whether or not include the padding header
            iStart = (dp == ColumnHeaderTemplateProperty || dp == ColumnHeaderTemplateSelectorProperty || dp == ColumnHeaderStringFormatProperty)
                    ? 1 : 0;

            iEnd = InternalChildren.Count - 3;  // skip the floating header and the indicator.
        }

        #region Create & Update Header
        private void AddPaddingColumnHeader()
        {
            GridColumnHeader paddingHeader = new GridColumnHeader();
            paddingHeader.IsInternalGenerated = true;
            paddingHeader.SetValue(GridColumnHeader.RolePropertyKey, GridColumnHeaderRole.Padding);

            paddingHeader.Content = null;
            paddingHeader.ContentTemplate = null;
            paddingHeader.ContentTemplateSelector = null;
            paddingHeader.MinWidth = 0;
            paddingHeader.Padding = new Thickness(0.0);
            paddingHeader.Width = double.NaN;
            paddingHeader.HorizontalAlignment = HorizontalAlignment.Stretch;
            paddingHeader.Focusable = false;
            paddingHeader.IsClickable = false;
            paddingHeader.IsResizable = false;

            InternalChildren.Add(paddingHeader);
            _paddingHeader = paddingHeader;
        }

        private void AddIndicator()
        {
            Separator indicator = new Separator();
            indicator.Visibility = Visibility.Hidden;

            // Indicator style:
            //
            // <Setter Property="Margin" Value="0" />
            // <Setter Property="Width" Value="2" />
            // <Setter Property="Template">
            //   <Setter.Value>
            //     <ControlTemplate TargetType="{x:Type Separator}">
            //        <Border Background="#FF000080"/>
            //     </ControlTemplate>
            //   </Setter.Value>
            // </Setter>

            indicator.Margin = new Thickness(0);
            indicator.Width = 2.0;

            FrameworkElementFactory border = new FrameworkElementFactory(typeof(Border));
            border.SetValue(Border.BackgroundProperty, new SolidColorBrush(Color.FromRgb(0x00, 0x00, 0x80)));

            ControlTemplate template = new ControlTemplate(typeof(Separator));
            template.VisualTree = border;
            template.Seal();

            indicator.Template = template;

            InternalChildren.Add(indicator);
            _indicator = indicator;
        }

        private void AddFloatingHeader(GridColumnHeader srcHeader)
        {
            GridColumnHeader header;

            Type headerType = srcHeader != null ? srcHeader.GetType() : typeof(GridColumnHeader);

            try
            {
                // Instantiate the same type for floating header
                header = Activator.CreateInstance(headerType) as GridColumnHeader;
            }
            catch (MissingMethodException)
            {
                throw new ArgumentException();
            }

            if (header != null)
            {
                header.SetValue(GridColumnHeader.RolePropertyKey, GridColumnHeaderRole.Floating);
                header.Visibility = Visibility.Hidden;

                InternalChildren.Add(header);
                _floatingHeader = header;
            }
        }

        private void UpdateAllHeaders(DependencyProperty dp)
        {
            GetIndexRange(dp, out int iStart, out int iEnd);

            UIElementCollection children = InternalChildren;
            for (int i = iStart; i <= iEnd; i++)
            {
                if (children[i] is GridColumnHeader header)
                {
                    UpdateHeaderProperties(header);
                }
            }
        }

        private void UpdateHeaderContent(GridColumnHeader header)
        {
            if (header != null && header.IsInternalGenerated)
            {
                GridColumn column = header.Column;
                if (column != null)
                {
                    GridControl owner = GridOwner;
                    if (owner == null) return;

                    ContentManager contentManager = owner.ContentMgr;
                    contentManager.ApplyColumnHeaderContent(column, ref header);
                }
            }
        }

        private void UpdateHeaderProperties(GridColumnHeader header, GridColumn column)
        {
            UpdateHeaderProperty(header, GridColumnHeader.StyleProperty, GridColumn.HeaderContainerStyleProperty, ColumnHeaderContainerStyleProperty);
            Helper.SyncProperty(this, GridColumnHeadersPresenter.ColumnHeaderContextMenuProperty, header, GridColumnHeader.ContextMenuProperty);
            Helper.SyncProperty(this, GridColumnHeadersPresenter.ColumnHeaderToolTipProperty, header, GridColumnHeader.ToolTipProperty);

            if (column != null)
            {
                Helper.SyncProperty(column, GridColumn.IsHeaderClickableProperty, header, GridColumnHeader.IsClickableProperty);
                Helper.SyncProperty(column, GridColumn.IsResizableProperty, header, GridColumnHeader.IsResizableProperty);
            }
        }

        private void UpdateHeaderProperties(GridColumnHeader header)
        {
            UpdateHeaderProperty(header, GridColumnHeader.StyleProperty, GridColumn.HeaderContainerStyleProperty, ColumnHeaderContainerStyleProperty);
            Helper.SyncProperty(this, GridColumnHeadersPresenter.ColumnHeaderContextMenuProperty, header, GridColumnHeader.ContextMenuProperty);
            Helper.SyncProperty(this, GridColumnHeadersPresenter.ColumnHeaderToolTipProperty, header, GridColumnHeader.ToolTipProperty);
        }

        private void UpdateHeaderProperty(GridColumnHeader header, DependencyProperty targetDP, DependencyProperty columnDP, DependencyProperty gvDP)
        {
            if (gvDP == ColumnHeaderContainerStyleProperty && header.Role == GridColumnHeaderRole.Padding)
            {
                // Because padding header has no chance to be instantiated by a sub-classed GridColumnHeader,
                // we ignore Grid.ColumnHeaderContainerStyle silently if its TargetType is not GridColumnHeader (or parent)
                // I.e. for padding header, only accept the GridColumnHeader as TargetType
                Style style = ColumnHeaderContainerStyle;
                if (style != null && !style.TargetType.IsAssignableFrom(typeof(GridColumnHeader)))
                {
                    // use default style for padding header in this case
                    header.Style = null;
                    return;
                }
            }

            GridColumn column = header.Column;

            object value = null;

            if (column != null && columnDP != null)
            {
                value = column.GetValue(columnDP);
            }

            if (value == null && gvDP != null)
            {
                value = this.GetValue(gvDP);
            }

            header.UpdateProperty(targetDP, value);
        }

        private void UpdateFloatingHeader(GridColumnHeader srcHeader)
        {
            if (srcHeader == null) return;
            if (_floatingHeader == null) return;

            _floatingHeader.Style = srcHeader.Style;

            _floatingHeader.FloatSourceHeader = srcHeader;
            _floatingHeader.Width = srcHeader.ActualWidth;
            _floatingHeader.Height = srcHeader.ActualHeight;
            _floatingHeader.SetValue(GridColumnHeader.ColumnPropertyKey, srcHeader.Column);
            _floatingHeader.Visibility = Visibility.Hidden;

            // override floating header's MinWidth/MinHeight to disable users to change floating header's Width/Height
            _floatingHeader.MinWidth = srcHeader.MinWidth;
            _floatingHeader.MinHeight = srcHeader.MinHeight;

            object template = srcHeader.ReadLocalValue(GridColumnHeader.ContentTemplateProperty);
            if ((template != DependencyProperty.UnsetValue) && (template != null))
            {
                _floatingHeader.ContentTemplate = srcHeader.ContentTemplate;
            }

            object selector = srcHeader.ReadLocalValue(GridColumnHeader.ContentTemplateSelectorProperty);
            if ((selector != DependencyProperty.UnsetValue) && (selector != null))
            {
                _floatingHeader.ContentTemplateSelector = srcHeader.ContentTemplateSelector;
            }

            if (!(srcHeader.Content is Visual))
            {
                _floatingHeader.Content = srcHeader.Content;
            }
        }

        private void RemoveHeader(GridColumnHeader header, int index)
        {
            if (header != null)
            {
                InternalChildren.Remove(header);
            }
            else
            {
                header = (GridColumnHeader)InternalChildren[index];
                InternalChildren.RemoveAt(index);
            }

            UnhookParentControlKeyboardEvent(header);
        }

        private GridColumnHeader CreateAndInsertHeader(GridColumn column, int index)
        {
            object header = column.Header;
            GridColumnHeader headerContainer = header as GridColumnHeader;
            if (header != null)
            {
                if (header is DependencyObject d && d is Visual headerAsVisual)
                {
                    Visual parent = VisualTreeHelper.GetParent(headerAsVisual) as Visual;
                    if (parent != null)
                    {
                        if (headerContainer != null && parent is GridColumnHeadersPresenter parentAsCHP)
                        {
                            parentAsCHP.InternalChildren.Remove(headerContainer);
                        }
                        else if (parent is GridColumnHeader parentAsCH)
                        {
                            parentAsCH.ClearValue(ContentControl.ContentProperty);
                        }
                    }
                }
            }

            if (headerContainer == null)
            {
                headerContainer = new GridColumnHeader();
                headerContainer.IsInternalGenerated = true;
            }

            headerContainer.SetValue(GridColumnHeader.ColumnPropertyKey, column);

            UpdateHeaderProperties(headerContainer, column);

            HookupParentControlKeyboardEvent(headerContainer);

            InternalChildren.Insert(index, headerContainer);

            UpdateHeaderContent(headerContainer);

            return headerContainer;
        }

        private void BuildHeaderLinks()
        {
            GridColumnHeader lastHeader = null;
            GridColumnsCollection columns = Columns;

            if (columns != null)
            {
                for (int i = 0; i < columns.Count; i++)
                {
                    GridColumnHeader header = (GridColumnHeader)InternalChildren[GetVisualIndex(i)];
                    header.PreviousVisualHeader = lastHeader;
                    lastHeader = header;
                }
            }

            if (_paddingHeader != null)
            {
                _paddingHeader.PreviousVisualHeader = lastHeader;
            }
        }
        #endregion

        #region Draging
        private bool CheckStartHeaderDrag(Point currentPos, Point originalPos)
        {
            return DoubleUtil.GreaterThan(Math.Abs(currentPos.X - originalPos.X), c_thresholdX);
        }

        private void PrepareHeaderDrag(GridColumnHeader header, Point pos, Point relativePos, bool cancelInvoke)
        {
            if (header.Role == GridColumnHeaderRole.Normal)
            {
                _prepareDragging = true;
                _isHeaderDragging = false;
                _draggingSrcHeader = header;

                _startPos = pos;
                _relativeStartPos = relativePos;

                if (!cancelInvoke)
                {
                    _startColumnIndex = FindIndexByPosition(_startPos, false);
                }
            }
        }

        private void StartHeaderDrag()
        {
            _startPos = _currentPos;
            _isHeaderDragging = true;

            // suppress src header's click event
            _draggingSrcHeader.SuppressClickEvent = true;

            // lock Columns during header dragging
            if (Columns != null)
            {
                Columns.BlockWrite();
            }

            // Remove the old floating header,
            // then create & add the new one per the source header's type
            InternalChildren.Remove(_floatingHeader);
            AddFloatingHeader(_draggingSrcHeader);

            UpdateFloatingHeader(_draggingSrcHeader);
        }

        private void FinishHeaderDrag(bool isCancel)
        {
            // clear related fields
            _prepareDragging = false;
            _isHeaderDragging = false;

            // restore src header's click event
            _draggingSrcHeader.SuppressClickEvent = false;

            _floatingHeader.Visibility = Visibility.Hidden;
            _floatingHeader.ResetFloatingHeaderCanvasBackground();

            _indicator.Visibility = Visibility.Hidden;

            // unlock Columns during header dragging
            if (Columns != null)
            {
                Columns.UnblockWrite();
            }

            // if cancelled, do nothing
            if (!isCancel)
            {
                // Display floating header if vertical move not exceeds header.Height * 2
                bool isMoveHeader = IsMousePositionValid(_floatingHeader, _currentPos, 2.0);

                Debug.Assert(Columns != null, "Columns is null in OnHeaderDragCompleted");

                // Revise the destinate column index
                int newColumnIndex = (_startColumnIndex >= _desColumnIndex) ? _desColumnIndex : _desColumnIndex - 1;

                if (isMoveHeader)
                {
                    Columns.Move(_startColumnIndex, newColumnIndex);
                }
            }
        }
        #endregion

        private void RenewEvents()
        {
            // hook up key down event from ItemsControl,
            // because GridColumnHeader and GridHeaderRowPresenter can not get focus
            GridControl oldParent = _parentControl; // backup the old value
            _parentControl = FindControlThroughTemplatedParent(this);

            if (oldParent != _parentControl)
            {
                if (oldParent != null)
                {
                    // NOTE: headers have unhooked the KeyDown event in RemoveHeader.

                    oldParent.KeyDown -= new KeyEventHandler(OnColumnHeadersPresenterKeyDown);
                }

                if (_parentControl != null)
                {
                    // register to HeadersPresenter to cancel dragging
                    _parentControl.KeyDown += new KeyEventHandler(OnColumnHeadersPresenterKeyDown);

                    // NOTE: headers will hookup the KeyDown event latter in CreateAndInsertHeader.
                }
            }
        }

        private void HookupParentControlKeyboardEvent(GridColumnHeader header)
        {
            if (header != null && _parentControl != null)
            {
                _parentControl.KeyDown += new KeyEventHandler(header.OnColumnHeaderKeyDown);
            }
        }

        private void UnhookParentControlKeyboardEvent(GridColumnHeader header)
        {
            if (header != null && _parentControl != null)
            {
                _parentControl.KeyDown -= new KeyEventHandler(header.OnColumnHeaderKeyDown);
            }
        }

        private static GridControl FindControlThroughTemplatedParent(GridColumnHeadersPresenter presenter)
        {
            FrameworkElement fe = presenter.TemplatedParent as FrameworkElement;
            GridControl itemsControl = null;

            while (fe != null)
            {
                itemsControl = fe as GridControl;
                if (itemsControl != null)
                {
                    break;
                }
                fe = fe.TemplatedParent as FrameworkElement;
            }

            return itemsControl;
        }

        private GridColumnHeader FindHeaderByColumn(GridColumn column)
        {
            GridColumnsCollection columns = Columns;
            UIElementCollection children = InternalChildren;

            if (columns != null && children.Count > columns.Count)
            {
                int index = columns.IndexOf(column);

                if (index != -1)
                {
                    // Becuase column headers is generated from right to left
                    int visualIndex = GetVisualIndex(index);

                    GridColumnHeader header = children[visualIndex] as GridColumnHeader;

                    if (header.Column != column)
                    {
                        // NOTE: if user change the GridColumn.Header/HeaderStyle
                        // in the event handler of column move. And in such case, the header
                        // we found by the algorithm above will be fail. So we turn to below
                        // a more reliable one.

                        for (int i = 1; i < children.Count; i++)
                        {
                            header = children[i] as GridColumnHeader;

                            if (header != null && header.Column == column)
                            {
                                return header;
                            }
                        }
                    }
                    else
                    {
                        return header;
                    }
                }
            }

            return null;
        }

        private int FindIndexByPosition(Point startPos, bool findNearestColumn)
        {
            int index = -1;

            if (startPos.X < 0.0)
            {
                return 0;
            }

            for (int i = 0; i < HeadersPositionList.Count; i++)
            {
                index++;

                Rect rect = HeadersPositionList[i];
                double startX = rect.X;
                double endX = startX + rect.Width;

                if (DoubleUtil.GreaterThanOrClose(startPos.X, startX) &&
                    DoubleUtil.LessThanOrClose(startPos.X, endX))
                {
                    if (findNearestColumn)
                    {
                        double midX = (startX + endX) * 0.5;
                        if (DoubleUtil.GreaterThanOrClose(startPos.X, midX))
                        {
                            // if not the padding header
                            if (i != HeadersPositionList.Count - 1)
                            {
                                index++;
                            }
                        }
                    }

                    break;
                }
            }

            return index;
        }

        private Point FindPositionByIndex(int index)
        {
            return new Point(HeadersPositionList[index].X, 0);
        }

        private static bool IsMousePositionValid(FrameworkElement floatingHeader, Point currentPos, double arrange)
        {
            return DoubleUtil.LessThanOrClose(-floatingHeader.Height * arrange, currentPos.Y) &&
                   DoubleUtil.LessThanOrClose(currentPos.Y, floatingHeader.Height * (arrange + 1));
        }

        internal void MakeParentControlGotFocus()
        {
            if (_parentControl != null && !_parentControl.IsKeyboardFocusWithin)
            {
                _parentControl.Focus();
            }
        }
        #endregion
    }
}
