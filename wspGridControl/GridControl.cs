using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using wspGridControl.Primitives;

namespace wspGridControl
{
    [TemplatePart(Name = PART_ScrollViewer, Type = typeof(GridScrollViewer))]
    [TemplatePart(Name = PART_GridScrollContent, Type = typeof(GridScrollContent))]
    public partial class GridControl : Control
    {
        #region Variables
        private const string PART_ScrollViewer = "PART_ScrollViewer";
        private const string PART_GridScrollContent = "PART_GridScrollContent";
        private const string PART_SelectionButton = "PART_SelectionButton";
        internal const string PART_ColumnHeadersPresenter = "PART_ColumnHeadersPresenter";
        internal const string PART_RowHeadersPresenter = "PART_RowHeadersPresenter";
        internal const string PART_RowsPresenter = "PART_RowsPresenter";

        internal static readonly Thickness c_defaultRowHeaderPadding = new Thickness(4, 0, 4, 0);
        internal static readonly Thickness c_defaultCellPadding = new Thickness(0);
        internal static readonly Thickness c_defaultCellRenderPadding = new Thickness(8, 0, 8, 0);
        internal static readonly Thickness c_defaultCellTextBoxMargin = new Thickness(4, 2, 4, 2);
        internal static readonly Thickness c_defaultCellTextBoxPadding = new Thickness(2, 0, 2, 0);

        private double _cAvCharWidth = 0.0;

        private readonly PropertyTracker _propertyTracker;
        private readonly SelectionManager _selectionManager;
        private readonly ContentManager _contentManager;

        private GridColumnsCollection _columns = null;
        private GridScrollViewer _scrollViewer = null;
        private GridColumnHeadersPresenter _columnHeadersPresenter = null;
        private GridRowHeadersPresenter _rowHeadersPresenter = null;
        private GridRowsPresenter _rowsPresenter = null;
        private GridScrollContent _scrollContent = null;
        private FrameworkElement _selectionButton = null;

        private GridCell _focusedCell = null;
        private GridCell _currentCell = null;
        private GridCell _pendingCurrentCell = null;

        private bool _isDraggingSelection;
        private bool _isRowDragging;
        private Point _dragPoint;
        #endregion

        #region Constructors
        static GridControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GridControl), new FrameworkPropertyMetadata(typeof(GridControl)));

            IsTabStopProperty.OverrideMetadata(typeof(GridControl), new FrameworkPropertyMetadata(false));
            KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(typeof(GridControl), new FrameworkPropertyMetadata(KeyboardNavigationMode.Contained));
            KeyboardNavigation.ControlTabNavigationProperty.OverrideMetadata(typeof(GridControl), new FrameworkPropertyMetadata(KeyboardNavigationMode.Once));

            FontFamilyProperty.OverrideMetadata(typeof(GridControl), new FrameworkPropertyMetadata(OnFontFamilyChanged));
            FontSizeProperty.OverrideMetadata(typeof(GridControl), new FrameworkPropertyMetadata(OnFontSizeChanged));
            FontWeightProperty.OverrideMetadata(typeof(GridControl), new FrameworkPropertyMetadata(OnFontWeightChanged));
            FontStyleProperty.OverrideMetadata(typeof(GridControl), new FrameworkPropertyMetadata(OnFontStyleChanged));
            FontStretchProperty.OverrideMetadata(typeof(GridControl), new FrameworkPropertyMetadata(OnFontStretchChanged));

            CommandManager.RegisterClassInputBinding(typeof(GridControl), new InputBinding(BeginEditCommand, new KeyGesture(Key.F2)));
            CommandManager.RegisterClassCommandBinding(typeof(GridControl), new CommandBinding(BeginEditCommand, new ExecutedRoutedEventHandler(OnExecutedBeginEdit), new CanExecuteRoutedEventHandler(OnCanExecuteBeginEdit)));

            CommandManager.RegisterClassCommandBinding(typeof(GridControl), new CommandBinding(CommitEditCommand, new ExecutedRoutedEventHandler(OnExecutedCommitEdit), new CanExecuteRoutedEventHandler(OnCanExecuteCommitEdit)));

            CommandManager.RegisterClassInputBinding(typeof(GridControl), new InputBinding(CancelEditCommand, new KeyGesture(Key.Escape)));
            CommandManager.RegisterClassCommandBinding(typeof(GridControl), new CommandBinding(CancelEditCommand, new ExecutedRoutedEventHandler(OnExecutedCancelEdit), new CanExecuteRoutedEventHandler(OnCanExecuteCancelEdit)));

            EventManager.RegisterClassHandler(typeof(GridControl), LostFocusEvent, new RoutedEventHandler(OnAnyLostFocus), true);
            EventManager.RegisterClassHandler(typeof(GridControl), GotFocusEvent, new RoutedEventHandler(OnAnyGotFocus), true);
            EventManager.RegisterClassHandler(typeof(GridControl), MouseLeftButtonDownEvent, new MouseButtonEventHandler(OnAnyMouseLeftButtonDownThunk), true);
            EventManager.RegisterClassHandler(typeof(GridControl), MouseUpEvent, new MouseButtonEventHandler(OnAnyMouseUpThunk), true);
            EventManager.RegisterClassHandler(typeof(GridControl), KeyDownEvent, new KeyEventHandler(OnAnyKeyDown), true);
        }

        public GridControl()
            : base()
        {
            _propertyTracker = new PropertyTracker(this);
            _selectionManager = new SelectionManager(this);
            _contentManager = new ContentManager(this);
        }
        #endregion

        #region Properties
        internal PropertyTracker Tracker
        {
            get => _propertyTracker;
        }

        internal SelectionManager SelectionMgr
        {
            get => _selectionManager;
        }

        internal ContentManager ContentMgr
        {
            get => _contentManager;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public GridColumnsCollection Columns
        {
            get
            {
                if (_columns == null)
                {
                    _columns = new GridColumnsCollection();

                    // Give the collection a back-link, this is used for the inheritance context
                    _columns.Owner = this;
                    _columns.InViewMode = true;
                }

                return _columns;
            }
        }

        internal static double CellPaddingHeight
        {
            get => c_defaultCellTextBoxMargin.Top + c_defaultCellTextBoxMargin.Bottom;
        }

        internal double AverageCharWidth
        {
            get => _cAvCharWidth;
        }

        internal GridCell FocusedCell
        {
            get => _focusedCell;
            set
            {
                if (_focusedCell != value)
                {
                    if (_focusedCell != null)
                    {
                        UpdateCurrentCell(_focusedCell, false);
                    }
                    _focusedCell = value;
                    if (_focusedCell != null)
                    {
                        UpdateCurrentCell(_focusedCell, true);
                    }
                }
            }
        }

        internal GridCell CurrentCellContainer
        {
            get
            {
                if (_currentCell == null)
                {
                    CellInfo currentCell = CurrentCell;
                    if (currentCell.IsValid)
                    {
                        _currentCell = TryFindCell(currentCell);
                    }
                }
                return _currentCell;
            }
            set
            {
                if (_currentCell != value && (value == null || value != _pendingCurrentCell))
                {
                    _pendingCurrentCell = value;
                    if (value == null)
                    {
                        SetValue(CurrentCellProperty, CellInfo.Unset);
                    }
                    else
                    {
                        SetValue(CurrentCellProperty, value.CellInfo);
                    }

                    _pendingCurrentCell = null;
                    _currentCell = value;
                }
            }
        }

        private GridCell MouseOverCell
        {
            get
            {
                GridCell cell = null;
                if (_rowsPresenter != null)
                {
                    var pt = Mouse.GetPosition(_rowsPresenter);
                    cell = _rowsPresenter.GetCellFromPoint(pt);
                }
                return cell;
            }
        }

        private GridRowHeader MouseOverRowHeader
        {
            get
            {
                GridRowHeader header = null;
                if (_rowHeadersPresenter != null)
                {
                    var pt = Mouse.GetPosition(_rowHeadersPresenter);
                    header = _rowHeadersPresenter.GetHeaderFromPoint(pt);
                }
                return header;
            }
        }
        #endregion

        #region Methods
        private void OnFontChanged(DependencyPropertyChangedEventArgs e)
        {
            double cAvCharWidth = _cAvCharWidth;
            CalculateRowHeight();

            ResetRenderers();

            if (cAvCharWidth != _cAvCharWidth)
            {
                UpdateColumnsWidth();
                UpdateRowHeaderMinWidth();
                InvalidateMeasure();
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var scrollViewer = GetTemplateChild(PART_ScrollViewer) as GridScrollViewer;
            var scrollContent = GetTemplateChild(PART_GridScrollContent) as GridScrollContent;

            if (scrollContent != null && scrollViewer != null)
            {
                CalculateRowHeight();
                UpdateColumnsWidth();

                _scrollContent = scrollContent;
                _scrollViewer = scrollViewer;

                scrollContent.GridOwner = this;
                scrollViewer.AutoScrollCommand = new AutoScrollCommand(this);

                scrollViewer.TemplateApplied += ScrollViewer_TemplateApplied;
                scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
            }

            Loaded += OnLoaded;
        }

        private static void OnAnyLostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is GridControl c)
            {
                c.UpdateSelection(true);
            }
        }

        private static void OnAnyGotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is GridControl c)
            {
                GridCell cell = c.CurrentCellContainer;
                if (cell != null)
                {
                    cell.Focus();
                    Keyboard.Focus(cell);
                }
                c.UpdateSelection(true);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_isDraggingSelection && _scrollViewer != null)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    Point currentMousePosition = Mouse.GetPosition(this);
                    if (!DoubleUtil.AreClose(currentMousePosition, _dragPoint))
                    {
                        _dragPoint = currentMousePosition;
                        RelativeMousePositions position = _scrollViewer.RelativeMousePosition;

                        if (position == RelativeMousePositions.Over)
                        {
                            if (_isRowDragging)
                            {
                                GridRowHeader header = MouseOverRowHeader;
                                if (header != null && _selectionManager.CurrentRow != header.RowIndex)
                                {
                                    _selectionManager.UpdateRowSelection(header.RowIndex);
                                    UpdateSelection(false);
                                    e.Handled = true;
                                }
                            }
                            else
                            {
                                GridCell cell = MouseOverCell;
                                if (cell != null && cell != CurrentCellContainer)
                                {
                                    cell.Focus();
                                    _selectionManager.UpdateSelection(cell.CellInfo);
                                    UpdateSelection(false);
                                    e.Handled = true;
                                }
                            }
                        }
                        else
                        {
                            if (_scrollViewer.IsAutoScrolling)
                            {
                                e.Handled = true;
                            }
                            else
                            {
                                _scrollViewer.StartAutoScroll();
                            }
                        }
                    }
                }
            }
            else
            {
                EndDragging();
            }

        }

        private static void OnAnyMouseLeftButtonDownThunk(object sender, MouseButtonEventArgs e)
        {
            if (sender is GridControl c)
                c.OnAnyMouseLeftButtonDown(e);
        }

        private void OnAnyMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (_rowsPresenter != null)
            {
                Rect rowsBounds = new Rect(0, 0, _rowsPresenter.ActualWidth, _rowsPresenter.ActualHeight);
                Point pt = e.GetPosition(_rowsPresenter);

                if (rowsBounds.Contains(pt))
                {
                    GridCell cell = _rowsPresenter.GetCellFromPoint(pt);
                    if (cell != null)
                    {
                        if (!cell.IsKeyboardFocusWithin)
                        {
                            cell.Focus();
                            FocusedCell = cell;
                        }

                        _selectionManager.StartSelection(cell.CellInfo);
                        UpdateSelection(false);

                        if (Mouse.Captured == null)
                        {
                            BeginDragging();
                        }
                    }
                }
                else if (_rowHeadersPresenter != null)
                {
                    rowsBounds = new Rect(0, 0, _rowHeadersPresenter.ActualWidth, _rowHeadersPresenter.ActualHeight);
                    pt = e.GetPosition(_rowHeadersPresenter);
                    if (rowsBounds.Contains(pt))
                    {
                        var header = _rowHeadersPresenter.GetHeaderFromPoint(pt);
                        if (header != null)
                        {
                            e.Handled = true;
                            _selectionManager.StartRowSelection(header.RowIndex);
                            UpdateSelection(false);
                            BeginRowDragging();
                        }
                    }
                }
            }
        }

        private static void OnAnyMouseUpThunk(object sender, MouseButtonEventArgs e)
        {
            if (sender is GridControl c)
                c.OnAnyMouseUp(e);
        }

        private void OnAnyMouseUp(MouseButtonEventArgs e)
        {
            EndDragging();
        }

        protected override void OnPreviewMouseDoubleClick(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDoubleClick(e);

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                GridCell hoverCell = MouseOverCell;
                GridCell currentCell = CurrentCellContainer;
                if (currentCell != null && hoverCell == currentCell)
                {
                    BeginEdit(currentCell);
                }
            }
        }

        private static void OnAnyKeyDown(object sender, KeyEventArgs e)
        {
            if (sender is GridControl c)
            {
                switch (e.Key)
                {
                    case Key.Space:
                        c.OnSpaceKeyDown(e);
                        break;

                    case Key.Enter:
                        c.OnEnterKeyDown(e);
                        break;

                    case Key.Left:
                    case Key.Right:
                    case Key.Up:
                    case Key.Down:
                        c.OnArrowKeyDown(e);
                        break;

                    case Key.Home:
                    case Key.End:
                        c.OnHomeOrEndKeyDown(e);
                        break;

                    case Key.PageUp:
                    case Key.PageDown:
                        c.OnPageUpOrDownKeyDown(e);
                        break;
                }
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (_columnHeadersPresenter != null)
            {
                _columnHeadersPresenter.UpdateVisualTree(true);
            }

            if (_rowsPresenter != null)
            {
                _rowsPresenter.UpdateVisualTree(true);

                if (_rowHeadersPresenter != null)
                {
                    _rowHeadersPresenter.UpdateVisualTree(true);
                }
            }

            _scrollContent.UpdateVisualTree();

            Loaded -= OnLoaded;
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            _propertyTracker.NotifyPropertyChanged(e);
            _contentManager.NotifyPropertyChanged(e);
        }

        private void CalculateRowHeight()
        {
            GetFontInfo(out double charHeight, out _cAvCharWidth);

            double cellHeight = charHeight + CellPaddingHeight;
            SetValue(CellHeightProperty, cellHeight);

            if (RowHeight == 0)
            {
                RowHeight = cellHeight;
            }
        }

        private void UpdateColumnsWidth()
        {
            var columns = Columns;
            if (columns.Count > 0)
            {
                foreach (var col in columns)
                {
                    col.AverageCharWidth = _cAvCharWidth;
                }
            }
        }

        private void UpdateRowHeaderMinWidth()
        {
            if (_selectionButton != null && _cAvCharWidth > 0)
            {
                var rowCountLen = Math.Max(2, string.Format(CultureInfo.CurrentCulture, "{0}", RowCount).Length) + 4;

                var width = _cAvCharWidth * rowCountLen;
                _selectionButton.MinWidth = width;
            }
        }

        private void ResetRenderers()
        {
            var columns = Columns;
            if (columns.Count > 0)
            {
                foreach (var col in columns)
                {
                    col.ResetRendererData();
                }
            }
        }

        private void GetFontInfo(out double height, out double aveWidth)
        {
            string text = "The quick brown fox jumped over the lazy dog.";
            Typeface typeFace = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);

            FormattedText formattedText = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                typeFace, FontSize, Brushes.Black, new NumberSubstitution(), VisualTreeHelper.GetDpi(this).PixelsPerDip);

            height = Math.Round((double)formattedText.Height);
            aveWidth = Math.Round((double)(formattedText.Width / 44.549996948242189));
        }

        protected virtual void OnGridSourceUpdated(object sender, EventArgs args)
        {
            if (sender is IGridSource source)
            {
                _contentManager.UpdateColumns(Columns, _cAvCharWidth);
                SetValue(RowsCountProperty, source.RowsCount);

                UpdateRowHeaderMinWidth();
            }
        }

        private void ScrollViewer_TemplateApplied(object sender, RoutedEventArgs e)
        {
            if (sender is GridScrollViewer scrollViewer)
            {
                _columnHeadersPresenter = scrollViewer.GetTemplateChild<GridColumnHeadersPresenter>(PART_ColumnHeadersPresenter);
                _rowHeadersPresenter = scrollViewer.GetTemplateChild<GridRowHeadersPresenter>(PART_RowHeadersPresenter);
                _rowsPresenter = scrollViewer.GetTemplateChild<GridRowsPresenter>(PART_RowsPresenter);
                _selectionButton = scrollViewer.GetTemplateChild<FrameworkElement>(PART_SelectionButton);

                if (_columnHeadersPresenter != null)
                {
                    _columnHeadersPresenter.GridOwner = this;
                }
                if (_rowsPresenter != null)
                {
                    _rowsPresenter.GridOwner = this;
                }
                if (_rowHeadersPresenter != null)
                {
                    _rowHeadersPresenter.GridOwner = this;
                }

                UpdateRowHeaderMinWidth();
                scrollViewer.TemplateApplied -= ScrollViewer_TemplateApplied;
            }
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (_scrollContent != null && _scrollContent.IsEditModeChanging) return;

            Vector offset = new Vector(e.HorizontalOffset, e.VerticalOffset);

            if (_columnHeadersPresenter != null)
            {
                _columnHeadersPresenter.ScrollOffset = offset;
            }

            if (_rowsPresenter != null)
            {
                _rowsPresenter.ScrollOffset = offset;
            }

            if (_scrollContent != null)
            {
                _scrollContent.ScrollOffset = offset;
            }
        }

        private void ScrollCellIntoView(long rowIndex, int colIndex)
        {
            if (_scrollContent != null && _scrollViewer != null)
            {
                Rect cellRect = _selectionManager.GetCellRect(rowIndex, colIndex, _scrollContent.ScrollOffset);
                if (!cellRect.IsEmpty)
                {
                    _scrollViewer.BringIntoView(cellRect);
                }
            }
        }

        private void OnEnterKeyDown(KeyEventArgs e)
        {
            if (_scrollContent != null)
            {
                if (_scrollContent.IsEditing)
                {
                    CommitEdit();
                }
                else
                {
                    GridCell focusedCell = Keyboard.FocusedElement as GridCell;
                    if (focusedCell != null)
                    {
                        TraversalRequest request = new TraversalRequest(FocusNavigationDirection.Down);
                        focusedCell.MoveFocus(request);
                        e.Handled = true;
                    }
                }
            }   
        }

        private void OnSpaceKeyDown(KeyEventArgs e)
        {
            if (_scrollContent != null && !_scrollContent.IsEditing)
            {
                GridCell cell = FocusedCell;
                if (cell != null)
                {
                    if (!cell.IsKeyboardFocusWithin)
                    {
                        cell.Focus();
                    }

                    _selectionManager.StartSelection(cell.CellInfo);
                    UpdateSelection(false);
                    e.Handled = true;
                }
            }
        }

        private void OnArrowKeyDown(KeyEventArgs e)
        {
            bool shifted = (e.KeyboardDevice.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;
            if (shifted && _scrollContent != null && !_scrollContent.IsEditing)
            {
                GridCell focusedElement = Keyboard.FocusedElement as GridCell;
                if (focusedElement != null && _selectionManager.HasAnySelection)
                {
                    CellInfo info = focusedElement.CellInfo;
                    if (info != null && info.IsValid)
                    {
                        _selectionManager.UpdateSelection(info);
                        UpdateSelection(false);
                        e.Handled = true;
                    }
                }
            }
        }

        private void OnHomeOrEndKeyDown(KeyEventArgs e)
        {
            if (_scrollContent != null && !_scrollContent.IsEditing)
            {
                bool end = e.Key == Key.End;
                bool controled = (e.KeyboardDevice.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
                bool shifted = (e.KeyboardDevice.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;

                int colIdx = end ? Columns.Count - 1 : 0;
                long rowIdx = end ? RowCount - 1 : 0;

                if (shifted && _selectionManager.HasAnySelection)
                {
                    if (controled)
                    {   
                        _selectionManager.UpdateSelection(rowIdx, colIdx);
                    }
                    else
                    {
                        _selectionManager.UpdateColumnSelection(colIdx);
                    }
                    UpdateSelection(false);
                }

                if (controled)
                {
                    if (end)
                        _scrollViewer.ScrollToRightEnd();
                    else
                        _scrollViewer.ScrollToHome();

                    e.Handled = true;
                }
            }
        }

        private void OnPageUpOrDownKeyDown(KeyEventArgs e)
        {
            if (_scrollContent != null && !_scrollContent.IsEditing)
            {
                bool down = e.Key == Key.PageDown;
                if ((e.KeyboardDevice.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                {
                    Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action<bool>(PageUpOrDownSelection), down);
                }
            }
        }

        private void PageUpOrDownSelection(bool down)
        {
            var rowIdx = RowStartIndex;
            if (down)
            {
                int visibleRows = (int)GetValue(VisibleRowsProperty);
                rowIdx += visibleRows - 1;
            }

            if (_selectionManager.HasAnySelection)
            {
                _selectionManager.UpdateRowSelection(rowIdx);
                UpdateSelection(false);
            }
        }

        private void UpdateCurrentCell(long rowIdx, int colIdx, bool forceInvoke)
        {
            GridCell cell = null;
            if (_rowsPresenter != null && !forceInvoke)
            {
                cell = _rowsPresenter.FindCell(rowIdx, colIdx);
            }

            if (cell != null)
            {
                UpdateCurrentCell(cell, true);
            }
            else
            {
                Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action<long, int>(FindAndUpdateCurrentCell), rowIdx, colIdx);
            }
        }

        private void FindAndUpdateCurrentCell(long rowIdx, int colIdx)
        {
            if (_rowsPresenter != null)
            {
                GridCell cell = _rowsPresenter.FindCell(rowIdx, colIdx);
                UpdateCurrentCell(cell, true);
                cell.Focus();
            }
        }

        private void UpdateCurrentCell(GridCell cell, bool isFocusWithinCell)
        {
            if (isFocusWithinCell)
            {
                CurrentCellContainer = cell;
            }
            else if (!IsKeyboardFocusWithin)
            {
                CurrentCellContainer = null;
            }
        }

        private void UpdateSelection(bool focusChanged)
        {
            if (!focusChanged && _rowsPresenter != null)
            {
                _rowsPresenter.UpdateCellsSelection();
            }

            if (_scrollContent != null)
            {
                if (focusChanged)
                    _scrollContent.UpdateSelectionBorder();
                else
                    _scrollContent.UpdateSelection();
            }

            if (!focusChanged)
            {
                SelectionChangedEventArgs args = new SelectionChangedEventArgs(_selectionManager.SelectedBlock);
                OnSelectionChanged(args);
            }
        }

        public void Select(BlockOfCells cells)
        {
            if (cells == null) return;
            var selected = _selectionManager.SetSelection(cells);

            if (!selected.IsEmpty && _selectionManager.HasAnySelection && IsLoaded)
            {
                long rowIdx = selected.Y;
                int colIdx = selected.X;

                ScrollCellIntoView(rowIdx, colIdx);
                UpdateSelection(false);
                UpdateCurrentCell(rowIdx, colIdx, false);
            }
        }

        public void SelectCell(long rowIndex, int colIndex)
        {
            if (_selectionManager.StartSelection(rowIndex, colIndex))
            {
                _selectionManager.UpdateSelection(rowIndex, colIndex);

                if (_selectionManager.HasAnySelection && IsLoaded)
                {
                    var selected = _selectionManager.SelectedBlock;

                    long rowIdx = selected.Y;
                    int colIdx = selected.X;

                    ScrollCellIntoView(rowIdx, colIdx);
                    UpdateSelection(false);
                    UpdateCurrentCell(rowIdx, colIdx, false);
                }
            }
        }

        public void SelectColumn(int columnIndex)
        {
            long rowCount = RowCount;
            int columnCount = Columns.Count;
            if (rowCount > 0 && columnCount > 0 && _selectionManager.StartColumnSelection(columnIndex))
            {
                UpdateSelection(false);
            }
        }

        public void SelectRow(long rowIndex)
        {
            long rowCount = RowCount;
            int columnCount = Columns.Count;
            if (rowCount > 0 && columnCount > 0 && _selectionManager.StartRowSelection(rowIndex))
            {
                UpdateSelection(false);
            }
        }

        public void SelectAll()
        {
            long rowCount = RowCount;
            int columnCount = Columns.Count;

            if (rowCount > 0 && columnCount > 0)
            {
                long rowIdx = 0;
                int colIdx = 0;
                _selectionManager.StartSelection(rowIdx, colIdx);

                rowIdx = rowCount - 1;
                colIdx = columnCount - 1;
                _selectionManager.UpdateSelection(rowIdx, colIdx);

                UpdateSelection(false);
            }
        }

        private GridCell TryFindCell(CellInfo info)
        {
            if (info == null) return null;
            return TryFindCell(info.RowIndex, info.ColumnIndex);
        }

        private GridCell TryFindCell(long rowIndex, int colIndex)
        {
            if (_rowsPresenter != null)
            {
                GridCell cell = _rowsPresenter.FindCell(rowIndex, colIndex);
                return cell;
            }
            return null;
        }

        private void BeginDragging()
        {
            if (Mouse.Capture(this, CaptureMode.SubTree))
            {
                _isDraggingSelection = true;
                _dragPoint = Mouse.GetPosition(this);
            }
        }

        private void BeginRowDragging()
        {
            BeginDragging();
            _isRowDragging = true;
        }

        private void EndDragging()
        {
            if (_scrollViewer != null)
            {
                _scrollViewer.StopAutoScroll();
            }

            if (Mouse.Captured == this)
            {
                ReleaseMouseCapture();
            }

            _isDraggingSelection = false;
            _isRowDragging = false;
        }

        private bool CanAutoScroll()
        {
            return _isDraggingSelection && _rowsPresenter != null;
        }

        private void DoAutoScroll()
        {
            if (_rowsPresenter != null)
            {
                Point pt = Mouse.GetPosition(_rowsPresenter);

                GridCell cell = _rowsPresenter.GetNearestCellFromPoint(pt, out RelativeMousePositions position);
                if (cell != null)
                {
                    var cellInfo = cell.CellInfo;
                    var lastRowIdx = RowCount - 1;
                    long rowIdx = cellInfo.RowIndex;
                    
                    if (position.HasFlag(RelativeMousePositions.Above)) 
                        rowIdx--;
                    else if (position.HasFlag(RelativeMousePositions.Below))
                        rowIdx++;

                    rowIdx = Math.Max(0, Math.Min(lastRowIdx, rowIdx));

                    _selectionManager.UpdateSelection(rowIdx, cellInfo.ColumnIndex);
                }
            }
        }

        #region Editing
        protected virtual void OnCanExecuteBeginEdit(CanExecuteRoutedEventArgs e)
        {
            bool canExecute = !IsReadOnly && _scrollContent != null && _rowsPresenter != null;

            if (canExecute)
            {
                GridCell cell = e.OriginalSource as GridCell;
                CellInfo cellInfo = cell != null ? cell.CellInfo : CurrentCell;

                if (cellInfo == null || cellInfo?.IsValid != true)
                {
                    canExecute = false;
                }
                else
                {
                    canExecute = _contentManager.CanCellEdit(cellInfo);
                }
            }

            if (canExecute)
            {
                e.CanExecute = true;
                e.Handled = true;
            }
            else
            {
                e.ContinueRouting = true;
            }
        }

        protected virtual void OnExecutedBeginEdit(ExecutedRoutedEventArgs e)
        {
            if (_scrollContent == null || _rowsPresenter == null) return;

            GridCell cell = e.OriginalSource as GridCell;
            if (cell == null) cell = CurrentCellContainer;

            if (cell != null)
            {
                CellInfo cellInfo = cell.CellInfo;

                BeginningEditEventArgs args = new BeginningEditEventArgs(cellInfo, e);
                OnBeginningEdit(args);

                if (!args.Cancel && _contentManager.CanCellEdit(cellInfo))
                {
                    object content = _contentManager.BeginEdit(cellInfo);
                    _rowsPresenter.SetCellIsEditing(cellInfo, true);
                    _scrollContent.ShowEditingElement(cellInfo, cell.Column, content, e);
                }
            }
        }

        protected virtual void OnCanExecuteCommitEdit(CanExecuteRoutedEventArgs e)
        {
            bool canExecute = !IsReadOnly && _scrollContent != null && _rowsPresenter != null;

            if (canExecute)
            {
                GridCell cell = e.OriginalSource as GridCell;
                CellInfo cellInfo = cell != null ? cell.CellInfo : CurrentCell;

                if (cellInfo == null || cellInfo?.IsValid != true)
                {
                    canExecute = false;
                }
                else
                {
                    var isEditing = _scrollContent.IsEditing;
                    canExecute = _contentManager.CanCellEdit(cellInfo) && isEditing;
                }
            }

            if (canExecute)
            {
                e.CanExecute = true;
                e.Handled = true;
            }
            else
            {
                e.ContinueRouting = true;
            }
        }

        protected virtual void OnExecutedCommitEdit(ExecutedRoutedEventArgs e)
        {
            if (_scrollContent != null)
            {
                GridCell cell = e.OriginalSource as GridCell;
                CellInfo cellInfo = cell != null ? cell.CellInfo : CurrentCell;

                var editingElement = _scrollContent.EditingElement;
                EditEndingEventArgs args = new EditEndingEventArgs(cellInfo, e, editingElement, true);
                OnEditEnding(args);

                if (!args.Cancel)
                {
                    _contentManager.CommitEdit(cell, _scrollContent.EditingElement);
                }
            }
        }

        protected virtual void OnCanExecuteCancelEdit(CanExecuteRoutedEventArgs e)
        {
            bool canExecute = !IsReadOnly && _scrollContent != null && _rowsPresenter != null;

            if (canExecute)
            {
                GridCell cell = e.OriginalSource as GridCell;
                CellInfo cellInfo = cell != null ? cell.CellInfo : CurrentCell;

                if (cellInfo == null || cellInfo?.IsValid != true)
                {
                    canExecute = false;
                }
                else
                {
                    var isEditing = _scrollContent.IsEditing;
                    canExecute = _contentManager.CanCellEdit(cellInfo) && isEditing;
                }
            }

            if (canExecute)
            {
                e.CanExecute = true;
                e.Handled = true;
            }
            else
            {
                e.ContinueRouting = true;
            }
        }

        protected virtual void OnExecutedCancelEdit(ExecutedRoutedEventArgs e)
        {
            if (_scrollContent != null)
            {
                GridCell cell = e.OriginalSource as GridCell;
                CellInfo cellInfo = cell != null && cell.CellInfo != null ? cell.CellInfo : CurrentCell;

                var editingElement = _scrollContent.EditingElement;
                EditEndingEventArgs args = new EditEndingEventArgs(cellInfo, e, editingElement, false);
                OnEditEnding(args);

                if (!args.Cancel)
                {
                    EndEdit(cellInfo);
                }
            }   
        }

        private void BeginEdit(GridCell cell)
        {
            if (!IsReadOnly && cell != null)
            {
                if (BeginEditCommand.CanExecute(null, cell))
                {
                    BeginEditCommand.Execute(null, cell);
                }
            }
        }

        internal void CommitEdit()
        {
            GridCell cell = CurrentCellContainer;
            CommitEdit(cell, true);
        }

        private void CommitEdit(GridCell cell, bool exitEditingMode)
        {
            if (cell != null && CommitEditCommand.CanExecute(null, cell))
            {
                CommitEditCommand.Execute(null, cell);
                if (exitEditingMode)
                {
                    EndEdit(cell.CellInfo);
                }
            }
        }

        private void CancelAnyEdit()
        {
            GridCell cell = CurrentCellContainer;
            CancelEdit(cell);
        }

        private void CancelEdit(GridCell cell)
        {
            if (cell != null && CancelEditCommand.CanExecute(null, cell))
            {
                CancelEditCommand.Execute(null, cell);
            }
        }

        private void EndEdit(CellInfo cellInfo)
        {
            if (cellInfo != null && _scrollContent != null && _rowsPresenter != null)
            {
                _contentManager.EndEdit();
                _rowsPresenter.SetCellIsEditing(cellInfo, false);
                _scrollContent.HideEditingElement();
            }
        }
        #endregion

        public FlowDocument ToFlowDocument()
        {
            using (var writer = new GridDocumentRenderer(this))
            {
                return writer.ToFlowDocument();
            }   
        }
        #endregion
    }
}
