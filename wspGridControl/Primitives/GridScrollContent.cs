using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;

namespace wspGridControl.Primitives
{
    public class GridScrollContent : GridContentPresenterBase
    {
        #region Variables
        private static readonly Thickness SelectionBorderThickness = new Thickness(2.0);
        private Border _selectionBorder = null;

        private FrameworkElement _editingElement = null;
        private CellInfo _editingCell = null;
        #endregion

        #region Constructors
        static GridScrollContent()
        {
            FocusableProperty.OverrideMetadata(typeof(GridScrollContent), new FrameworkPropertyMetadata(false));
        }

        public GridScrollContent()
            : base()
        {
            Focusable = false;
        }
        #endregion

        #region Dependency Properties

        #region RowHeightProperty
        internal static readonly DependencyProperty RowHeightProperty = GridControl.RowHeightProperty.AddOwner(
            typeof(GridScrollContent), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

        private double RowHeight
        {
            get => (double)GetValue(RowHeightProperty);
        }
        #endregion

        #region RowsCountProperty
        internal static readonly DependencyProperty RowsCountProperty = GridControl.RowsCountProperty.AddOwner(
            typeof(GridScrollContent), new FrameworkPropertyMetadata(0L, FrameworkPropertyMetadataOptions.AffectsMeasure));

        private long RowsCount
        {
            get => (long)GetValue(RowsCountProperty);
        }
        #endregion

        #region VisibleRowsProperty
        internal static readonly DependencyProperty VisibleRowsProperty = GridControl.VisibleRowsProperty.AddOwner(
            typeof(GridScrollContent), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsMeasure));

        private int VisibleRows
        {
            get => (int)GetValue(VisibleRowsProperty);
        }
        #endregion

        #region SelectionBrushProperty
        internal static readonly DependencyProperty SelectionBrushProperty = GridControl.SelectionBrushProperty.AddOwner(
            typeof(GridScrollContent), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        private Brush SelectionBrush
        {
            get { return (Brush)GetValue(SelectionBrushProperty); }
        }
        #endregion

        #region SelectionBorderBrushProperty
        internal static readonly DependencyProperty SelectionBorderBrushProperty = GridControl.SelectionBorderBrushProperty.AddOwner(
            typeof(GridScrollContent), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        private Brush SelectionBorderBrush
        {
            get { return (Brush)GetValue(SelectionBorderBrushProperty); }
        }
        #endregion

        #region IsEditModeChangingProperty
        internal static readonly DependencyProperty IsEditModeChangingProperty = DependencyProperty.Register(
            nameof(IsEditModeChanging), typeof(bool), typeof(GridScrollContent));

        internal bool IsEditModeChanging
        {
            get => (bool)GetValue(IsEditModeChangingProperty);
            set { SetValue(IsEditModeChangingProperty, value); }
        }
        #endregion

        #region IsEditingProperty
        internal static readonly DependencyProperty IsEditingProperty = DependencyProperty.Register(
            nameof(IsEditing), typeof(bool), typeof(GridScrollContent));

        internal bool IsEditing
        {
            get => (bool)GetValue(IsEditingProperty);
            set { SetValue(IsEditingProperty, value); }
        }
        #endregion

        #endregion

        #region Properties
        internal FrameworkElement EditingElement
        {
            get => _editingElement;
        }
        #endregion

        #region Methods
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            CreateSelectionBorder();
        }

        protected override Size MeasureOverride(Size constraint)
        {
            Size desiredSize = new Size(0, 0);
            if (Columns == null) return desiredSize;

            double delta = GridScrollViewer.ScrollLineDelta;
            var columns = Columns.ColumnCollection;

            double totalWidth = 0;

            for (var c = 0; c < columns.Count; c++)
            {
                var columnWidth = columns[c].FinalWidth;
                totalWidth += columnWidth;
            }

            desiredSize.Width = totalWidth;

            long rowCount = RowsCount;
            if (rowCount == 0) return desiredSize;

            rowCount = rowCount + 1;
            var rowHeight = RowHeight;
            var visibleRows = VisibleRows;
            if (visibleRows > 0)
            {
                long remainRows = rowCount - visibleRows;
                double visableHeight = visibleRows * rowHeight;
                desiredSize.Height = visableHeight + (remainRows * delta);
            }

            return desiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var children = InternalChildren;

            if (children.Count > 0)
            {
                foreach(UIElement child in children)
                {
                    if (child == _editingElement)
                    {
                        Rect rectChild = GetEditingElementRect();
                        if (rectChild.IsEmpty) continue;

                        child.Arrange(rectChild);
                    }
                    else if (child == _selectionBorder)
                    {
                        Rect rectChild = GetSectionRect();
                        if (rectChild.IsEmpty) continue;

                        child.Arrange(rectChild);
                    }
                }
            }

            return finalSize;
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            DependencyProperty dp = e.Property;
            if (dp == SelectionBorderBrushProperty || dp == SelectionBrushProperty)
            {
                UpdateSelectionBorder();
            }
        }

        protected override void OnScrollOffsetChanged(Vector oldValue, Vector newValue)
        {
            InvalidateArrange();
        }

        protected override void OnColumnCollectionChanged(ColumnCollectionChangedEventArgs e)
        {
            base.OnColumnCollectionChanged(e);
            UpdateSelection();
        }

        protected override void OnColumnPropertyChanged(GridColumn column, string propertyName)
        {
            if (GridColumn.WidthProperty.Name.Equals(propertyName) || GridColumn.c_ActualWidthName.Equals(propertyName))
            {
                InvalidateMeasure();
                UpdateSelection();
            }
        }

        private void CreateSelectionBorder()
        {
            _selectionBorder = new Border();

            _selectionBorder.SetValue(Border.BorderThicknessProperty, SelectionBorderThickness);

            UpdateSelectionBorder();
            InternalChildren.Add(_selectionBorder);
        }

        internal void UpdateSelectionBorder()
        {
            bool isActive = GridOwner != null && GridOwner.FocusedCell != null;
            
            if (_selectionBorder != null)
            {
                if (isActive || IsEditing)
                {
                    _selectionBorder.SetValue(Border.BorderBrushProperty, SelectionBorderBrush);
                    _selectionBorder.SetValue(Border.BackgroundProperty, SelectionBrush);
                }
                else
                {
                    _selectionBorder.SetValue(Border.BorderBrushProperty, SystemColors.ActiveBorderBrush);
                    _selectionBorder.SetValue(Border.BackgroundProperty, SystemColors.ControlBrush);
                }                
            }
        }

        internal void UpdateSelection()
        {
            UpdateSelectionBorder();
            InvalidateArrange();
        }

        private Rect GetSectionRect()
        {
            if (GridOwner == null) return Rect.Empty;
            SelectionManager selection = GridOwner.SelectionMgr;
            if (selection != null)
            {
                return selection.GetSectionRect(ScrollOffset);
            }

            return Rect.Empty;
        }

        private Rect GetEditingElementRect()
        {
            if (GridOwner == null) return Rect.Empty;

            var selection = GridOwner.SelectionMgr;
            var content = GridOwner.ContentMgr;
            var cellInfo = content.EditingCell;

            if (cellInfo != null && cellInfo.IsValid)
            {
                return selection.GetCellRect(cellInfo.RowIndex,  cellInfo.DisplayColumnIndex, ScrollOffset);
            }
            return Rect.Empty;
        }

        internal void ShowEditingElement(CellInfo cellInfo, GridColumn column, object content, RoutedEventArgs args)
        {
            if (cellInfo == null) return;
            if (!cellInfo.IsValid) return;
            if (column == null) return;
            if (GridOwner == null) return;

            IsEditModeChanging = true;

            if (!CellInfo.IsNullOrInvalid(_editingCell) && _editingCell.Equals(cellInfo) && _editingElement != null)
            {
                RaisePreparingEdit(cellInfo, content, _editingElement, args);

                column.UpdateEditingElement(_editingElement, content);
                _editingElement.Visibility = Visibility.Visible;

                InvalidateArrange();
                _editingElement.Focus();

                IsEditModeChanging = false;
                IsEditing = true;
                return;
            }

            if (_editingElement != null)
            {
                InternalChildren.Remove(_editingElement);
                _editingElement = null;
            }

            _editingCell = cellInfo.Clone();
            _editingElement = column.GetCellEditingElement();
            column.UpdateEditingElement(_editingElement, content);

            RaisePreparingEdit(_editingCell, content, _editingElement, args);

            _editingElement.Loaded += EditingElement_Loaded;

            InternalChildren.Add(_editingElement);
            InvalidateArrange();
        }

        internal void HideEditingElement()
        {
            IsEditModeChanging = true;

            if (_editingElement != null)
            {
                _editingElement.Visibility = Visibility.Hidden;
                InvalidateArrange();
            }

            IsEditing = false;
            IsEditModeChanging = false;
        }

        private void EditingElement_Loaded(object sender, RoutedEventArgs e)
        {
            if (_editingElement != null)
            {
                IsEditing = true;
                _editingElement.Loaded -= EditingElement_Loaded;

                _editingElement.Focus();
            }
            UpdateSelectionBorder();

            Dispatcher.BeginInvoke(new Action(OnEditModeChanged), DispatcherPriority.ContextIdle);
        }

        private void OnEditModeChanged()
        {
            IsEditModeChanging = false;
        }

        private void RaisePreparingEdit(CellInfo cellInfo, object content, FrameworkElement editingElement, RoutedEventArgs editingArgs)
        {
            PreparingEditEventArgs args = new PreparingEditEventArgs(cellInfo, editingArgs, editingElement, content);
            if (GridOwner != null)
            {
                GridOwner.OnPreparingEdit(args);
            }
        }
        #endregion
    }
}