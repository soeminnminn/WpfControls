using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;
using wspGridControl.Primitives;

namespace wspGridControl
{
    public partial class GridControl
    {
        #region Dependency Properties

        #region GridSourceProperty
        public static readonly DependencyProperty GridSourceProperty = DependencyProperty.Register(
            nameof(GridSource), typeof(IGridSource), typeof(GridControl),
            new FrameworkPropertyMetadata(null,
                FrameworkPropertyMetadataOptions.AffectsMeasure |
                FrameworkPropertyMetadataOptions.AffectsRender,
                new PropertyChangedCallback(OnGridSourceChanged)));

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IGridSource GridSource
        {
            get => (IGridSource)GetValue(GridSourceProperty);
            set => SetValue(GridSourceProperty, value);
        }

        private static void OnGridSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GridControl c)
            {
                c.OnGridSourceChanged((IGridSource)e.OldValue, (IGridSource)e.NewValue);
            }
        }

        private void OnGridSourceChanged(IGridSource oldValue, IGridSource newValue)
        {
            if (oldValue != null)
                oldValue.Updated -= OnGridSourceUpdated;

            if (newValue != null)
                newValue.Updated += OnGridSourceUpdated;

            OnGridSourceUpdated(newValue, EventArgs.Empty);
        }
        #endregion

        #region AllowsColumnReorderProperty
        /// <summary>
        /// AllowsColumnReorderProperty DependencyProperty
        /// </summary>
        public static readonly DependencyProperty AllowsColumnReorderProperty = DependencyProperty.Register(
            nameof(AllowsColumnReorder), typeof(bool), typeof(GridControl),
            new FrameworkPropertyMetadata(true));

        /// <summary>
        /// AllowsColumnReorder
        /// </summary>
        public bool AllowsColumnReorder
        {
            get { return (bool)GetValue(AllowsColumnReorderProperty); }
            set { SetValue(AllowsColumnReorderProperty, value); }
        }
        #endregion

        #region ColumnHeaderContainerStyleProperty
        /// <summary>
        /// ColumnHeaderContainerStyleProperty DependencyProperty
        /// </summary>
        public static readonly DependencyProperty ColumnHeaderContainerStyleProperty = DependencyProperty.Register(
            nameof(ColumnHeaderContainerStyle), typeof(Style), typeof(GridControl));

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
        public static readonly DependencyProperty ColumnHeaderTemplateProperty = DependencyProperty.Register(
            nameof(ColumnHeaderTemplate), typeof(DataTemplate), typeof(GridControl),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(OnColumnHeaderTemplateChanged)));

        /// <summary>
        /// column header template
        /// </summary>
        public DataTemplate ColumnHeaderTemplate
        {
            get { return (DataTemplate)GetValue(ColumnHeaderTemplateProperty); }
            set { SetValue(ColumnHeaderTemplateProperty, value); }
        }

        private static void OnColumnHeaderTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }
        #endregion

        #region ColumnHeaderTemplateSelectorProperty
        /// <summary>
        /// ColumnHeaderTemplateSelector DependencyProperty
        /// </summary>
        public static readonly DependencyProperty ColumnHeaderTemplateSelectorProperty = DependencyProperty.Register(
            nameof(ColumnHeaderTemplateSelector), typeof(DataTemplateSelector), typeof(GridControl),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(OnColumnHeaderTemplateSelectorChanged)));

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

        private static void OnColumnHeaderTemplateSelectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }
        #endregion

        #region ColumnHeaderStringFormatProperty
        /// <summary>
        /// ColumnHeaderStringFormat DependencyProperty
        /// </summary>
        public static readonly DependencyProperty ColumnHeaderStringFormatProperty = DependencyProperty.Register(
                nameof(ColumnHeaderStringFormat), typeof(string), typeof(GridControl));

        /// <summary>
        /// column header string format
        /// </summary>
        public string ColumnHeaderStringFormat
        {
            get { return (string)GetValue(ColumnHeaderStringFormatProperty); }
            set { SetValue(ColumnHeaderStringFormatProperty, value); }
        }
        #endregion

        #region ColumnHeaderContextMenuProperty
        /// <summary>
        /// ColumnHeaderContextMenuProperty DependencyProperty
        /// </summary>
        public static readonly DependencyProperty ColumnHeaderContextMenuProperty = DependencyProperty.Register(
            nameof(ColumnHeaderContextMenu), typeof(ContextMenu), typeof(GridControl));

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
        /// ColumnHeaderToolTipProperty DependencyProperty
        /// </summary>
        public static readonly DependencyProperty ColumnHeaderToolTipProperty = DependencyProperty.Register(
            nameof(ColumnHeaderToolTip), typeof(object), typeof(GridControl));

        /// <summary>
        /// ColumnHeaderToolTip
        /// </summary>
        public object ColumnHeaderToolTip
        {
            get { return GetValue(ColumnHeaderToolTipProperty); }
            set { SetValue(ColumnHeaderToolTipProperty, value); }
        }
        #endregion

        #region RowHeightProperty
        public static readonly DependencyProperty RowHeightProperty = DependencyProperty.Register(
            nameof(RowHeight), typeof(double), typeof(GridControl),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure,
                new PropertyChangedCallback(OnRowHeightChanged)));

        public double RowHeight
        {
            get => (double)GetValue(RowHeightProperty);
            set => SetValue(RowHeightProperty, value);
        }

        private static void OnRowHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GridControl c)
            {
                c.OnRowHeightChanged((double)e.OldValue, (double)e.NewValue);
            }
        }

        protected void OnRowHeightChanged(double oldValue, double newValue)
        { }
        #endregion

        #region RowHeaderContainerStyleProperty
        /// <summary>
        /// RowHeaderContainerStyleProperty DependencyProperty
        /// </summary>
        public static readonly DependencyProperty RowHeaderContainerStyleProperty = DependencyProperty.Register(
            nameof(RowHeaderContainerStyle), typeof(Style), typeof(GridControl));

        /// <summary>
        /// header container's style
        /// </summary>
        public Style RowHeaderContainerStyle
        {
            get { return (Style)GetValue(RowHeaderContainerStyleProperty); }
            set { SetValue(RowHeaderContainerStyleProperty, value); }
        }
        #endregion

        #region RowHeaderTemplateProperty
        /// <summary>
        /// RowHeaderTemplate DependencyProperty
        /// </summary>
        public static readonly DependencyProperty RowHeaderTemplateProperty = DependencyProperty.Register(
            nameof(RowHeaderTemplate), typeof(DataTemplate), typeof(GridControl),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(OnRowHeaderTemplateChanged)));

        /// <summary>
        /// column header template
        /// </summary>
        public DataTemplate RowHeaderTemplate
        {
            get { return (DataTemplate)GetValue(RowHeaderTemplateProperty); }
            set { SetValue(RowHeaderTemplateProperty, value); }
        }

        private static void OnRowHeaderTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }
        #endregion

        #region RowHeaderTemplateSelectorProperty
        /// <summary>
        /// RowHeaderTemplateSelector DependencyProperty
        /// </summary>
        public static readonly DependencyProperty RowHeaderTemplateSelectorProperty = DependencyProperty.Register(
            nameof(RowHeaderTemplateSelector), typeof(DataTemplateSelector), typeof(GridControl),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(OnRowHeaderTemplateSelectorChanged)));

        /// <summary>
        /// header template selector
        /// </summary>
        /// <remarks>
        ///     This property is ignored if <seealso cref="RowHeaderTemplate"/> is set.
        /// </remarks>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DataTemplateSelector RowHeaderTemplateSelector
        {
            get { return (DataTemplateSelector)GetValue(RowHeaderTemplateSelectorProperty); }
            set { SetValue(RowHeaderTemplateSelectorProperty, value); }
        }

        private static void OnRowHeaderTemplateSelectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }
        #endregion

        #region RowHeaderStringFormatProperty
        /// <summary>
        /// RowHeaderStringFormat DependencyProperty
        /// </summary>
        public static readonly DependencyProperty RowHeaderStringFormatProperty = DependencyProperty.Register(
                nameof(RowHeaderStringFormat), typeof(string), typeof(GridControl));

        /// <summary>
        /// row header string format
        /// </summary>
        public string RowHeaderStringFormat
        {
            get { return (string)GetValue(RowHeaderStringFormatProperty); }
            set { SetValue(RowHeaderStringFormatProperty, value); }
        }
        #endregion

        #region RowHeaderContextMenuProperty
        /// <summary>
        /// RowHeaderContextMenuProperty DependencyProperty
        /// </summary>
        public static readonly DependencyProperty RowHeaderContextMenuProperty = DependencyProperty.Register(
            nameof(RowHeaderContextMenu), typeof(ContextMenu), typeof(GridControl));

        /// <summary>
        /// RowHeaderContextMenu
        /// </summary>
        public ContextMenu RowHeaderContextMenu
        {
            get { return (ContextMenu)GetValue(RowHeaderContextMenuProperty); }
            set { SetValue(RowHeaderContextMenuProperty, value); }
        }
        #endregion

        #region RowHeaderToolTipProperty
        /// <summary>
        /// RowHeaderToolTipProperty DependencyProperty
        /// </summary>
        public static readonly DependencyProperty RowHeaderToolTipProperty = DependencyProperty.Register(
            nameof(RowHeaderToolTip), typeof(object), typeof(GridControl));

        /// <summary>
        /// RowHeaderToolTip
        /// </summary>
        public object RowHeaderToolTip
        {
            get { return GetValue(RowHeaderToolTipProperty); }
            set { SetValue(RowHeaderToolTipProperty, value); }
        }
        #endregion

        #region LineThicknessProperty
        public static readonly DependencyProperty LineThicknessProperty = DependencyProperty.Register(
            nameof(LineThickness), typeof(double), typeof(GridControl),
            new FrameworkPropertyMetadata(0.5, FrameworkPropertyMetadataOptions.AffectsRender,
                new PropertyChangedCallback(OnLineThicknessPropertyChanged)));

        public double LineThickness
        {
            get => (double)GetValue(LineThicknessProperty);
            set { SetValue(LineThicknessProperty, value); }
        }

        private static void OnLineThicknessPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GridControl ctrl)
            {
                ctrl.InvalidateVisual();
            }
        }
        #endregion

        #region LineBrushProperty
        public static readonly DependencyProperty LineBrushProperty = DependencyProperty.Register(
            nameof(LineBrush), typeof(Brush), typeof(GridControl),
            new FrameworkPropertyMetadata(SystemColors.ActiveBorderBrush, FrameworkPropertyMetadataOptions.AffectsRender,
                new PropertyChangedCallback(OnLineBrushPropertyChanged)));

        public Brush LineBrush
        {
            get { return (Brush)GetValue(LineBrushProperty); }
            set { SetValue(LineBrushProperty, value); }
        }

        private static void OnLineBrushPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GridControl ctrl)
            {
                ctrl.InvalidateVisual();
            }
        }
        #endregion

        #region SelectionBrushProperty
        public static readonly DependencyProperty SelectionBrushProperty = DependencyProperty.Register(
            nameof(SelectionBrush), typeof(Brush), typeof(GridControl),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public Brush SelectionBrush
        {
            get { return (Brush)GetValue(SelectionBrushProperty); }
            set { SetValue(SelectionBrushProperty, value); }
        }
        #endregion

        #region SelectionBorderBrushProperty
        public static readonly DependencyProperty SelectionBorderBrushProperty = DependencyProperty.Register(
            nameof(SelectionBorderBrush), typeof(Brush), typeof(GridControl),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public Brush SelectionBorderBrush
        {
            get { return (Brush)GetValue(SelectionBorderBrushProperty); }
            set { SetValue(SelectionBorderBrushProperty, value); }
        }
        #endregion

        #region IsReadOnlyProperty
        /// <summary>
        /// The DependencyProperty for IsReadOnly.
        /// </summary>
        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(
            nameof(IsReadOnly), typeof(bool), typeof(GridControl),
            new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnIsReadOnlyChanged)));

        /// <summary>
        /// Whether the DataList's rows and cells can be placed in edit mode.
        /// </summary>
        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        private static void OnIsReadOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GridControl c)
            {
                if ((bool)e.NewValue)
                    c.CancelAnyEdit();

                CommandManager.InvalidateRequerySuggested();
            }
        }
        #endregion

        #region CellHeightProperty
        internal static readonly DependencyProperty CellHeightProperty = DependencyProperty.Register(
            nameof(CellHeight), typeof(double), typeof(GridControl), new FrameworkPropertyMetadata(0.0));

        internal double CellHeight
        {
            get => (double)GetValue(CellHeightProperty);
        }
        #endregion

        #region RowsCountProperty
        internal static readonly DependencyProperty RowsCountProperty = DependencyProperty.Register(
            nameof(RowCount), typeof(long), typeof(GridControl), new FrameworkPropertyMetadata(0L));

        internal long RowCount
        {
            get => (long)GetValue(RowsCountProperty);
        }
        #endregion

        #region AvailableRowsProperty
        internal static readonly DependencyProperty AvailableRowsProperty = DependencyProperty.Register(
            "AvailableRows", typeof(int), typeof(GridControl), new FrameworkPropertyMetadata(0));

        internal int AvailableRows
        {
            get => (int)GetValue(AvailableRowsProperty);
        }
        #endregion

        #region VisibleRowsProperty
        internal static readonly DependencyProperty VisibleRowsProperty = DependencyProperty.Register(
            "VisibleRows", typeof(int), typeof(GridControl), new FrameworkPropertyMetadata(0));
        #endregion

        #region RowStartIndexProperty
        internal static readonly DependencyProperty RowStartIndexProperty = DependencyProperty.Register(
            "RowStartIndex", typeof(long), typeof(GridControl), new FrameworkPropertyMetadata(0L));

        internal long RowStartIndex
        {
            get => (long)GetValue(RowStartIndexProperty);
        }
        #endregion

        #region CurrentCellProperty
        internal static readonly DependencyProperty CurrentCellProperty = DependencyProperty.Register(
            nameof(CurrentCell), typeof(CellInfo), typeof(GridControl),
            new FrameworkPropertyMetadata(CellInfo.Unset, new PropertyChangedCallback(OnCurrentCellChanged)));

        public CellInfo CurrentCell
        {
            get => (CellInfo)GetValue(CurrentCellProperty);
            set { SetValue(CurrentCellProperty, value); }
        }

        private static void OnCurrentCellChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GridControl grid)
            {
                CellInfo currentCell = (CellInfo)e.NewValue;

                if (grid._currentCell != null && grid._currentCell.IsEditing)
                {
                    grid.CommitEdit(grid._currentCell, true);
                }

                grid._currentCell = null;

                if (currentCell != null && currentCell.IsValid && grid.IsKeyboardFocusWithin)
                {
                    GridCell cell = grid._pendingCurrentCell;
                    if (cell == null)
                    {
                        cell = grid.CurrentCellContainer;
                        if (cell == null)
                        {
                            grid.ScrollCellIntoView(currentCell.RowIndex, currentCell.ColumnIndex);
                            cell = grid.CurrentCellContainer;
                        }
                    }

                    if (cell != null)
                    {
                        if (!cell.IsKeyboardFocusWithin)
                        {
                            cell.Focus();
                        }
                    }
                }

                grid.OnCurrentCellChanged(EventArgs.Empty);
            }
        }
        #endregion

        #region Font Properties Changed
        private static void OnFontFamilyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GridControl c)
                c.OnFontChanged(e);
        }

        private static void OnFontSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GridControl c)
                c.OnFontChanged(e);
        }

        private static void OnFontWeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GridControl c)
                c.OnFontChanged(e);
        }

        private static void OnFontStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GridControl c)
                c.OnFontChanged(e);
        }

        private static void OnFontStretchChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GridControl c)
                c.OnFontChanged(e);
        }
        #endregion

        #endregion
    }
}
