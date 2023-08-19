using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace wspGridControl
{
    public class GridColumn : DependencyObject, INotifyPropertyChanged
    {
        #region Variables
        internal static readonly GridColumnWidth c_defaultColumnWidth = new GridColumnWidth(100.0);
        internal const string c_ActualWidthName = "ActualWidth";

        private GridControl _owner = null;
        private double _actualWidth = 0.0;
        private double _desiredWidth = 0.0;
        private int _actualIndex = -1;
        private ColumnMeasureState _state;

        private DataTemplate _defaultHeaderTemplate = null;
        private ICellRenderer _renderer = null;
        #endregion

        #region Event
        /// <summary>
        /// PropertyChanged event (per <see cref="INotifyPropertyChanged" />).
        /// </summary>
        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add { _propertyChanged += value; }
            remove { _propertyChanged -= value; }
        }

        private event PropertyChangedEventHandler _propertyChanged;
        #endregion

        #region Constructor
        public GridColumn()
        {
            ResetPrivateData();
            _state = double.IsNaN(ColumnWidth) ? ColumnMeasureState.Init : ColumnMeasureState.SpecificWidth;
        }
        #endregion

        #region Dependency Properties

        #region ColumnInfoProperty
        public static readonly DependencyProperty ColumnInfoProperty = DependencyProperty.Register(
            nameof(ColumnInfo), typeof(GridColumnInfo), typeof(GridColumn), 
            new FrameworkPropertyMetadata(new PropertyChangedCallback(OnColumnInfoChanged)));

        public GridColumnInfo ColumnInfo
        {
            get => (GridColumnInfo)GetValue(ColumnInfoProperty);
            set => SetValue(ColumnInfoProperty, value);
        }

        private static void OnColumnInfoChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GridColumn c)
            {
                c.ResetRendererData();
                c.ResetHeaderTemplate();
                c.OnColumnInfoChanged((GridColumnInfo)e.NewValue);
            }
        }
        #endregion

        #region HeaderProperty
        /// <summary>
        /// Header DependencyProperty
        /// </summary>
        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
                nameof(Header), typeof(object), typeof(GridColumn),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(OnHeaderChanged)));

        public object Header
        {
            get { return GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        private static void OnHeaderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GridColumn c)
                c.OnPropertyChanged(HeaderProperty.Name);
        }
        #endregion

        #region HeaderContainerStyleProperty
        /// <summary>
        /// HeaderContainerStyle DependencyProperty
        /// </summary>
        public static readonly DependencyProperty HeaderContainerStyleProperty = DependencyProperty.Register(
            nameof(HeaderContainerStyle), typeof(Style), typeof(GridColumn),
            new FrameworkPropertyMetadata(OnHeaderContainerStyleChanged));

        /// <summary>
        /// Header container's style
        /// </summary>
        public Style HeaderContainerStyle
        {
            get { return (Style)GetValue(HeaderContainerStyleProperty); }
            set { SetValue(HeaderContainerStyleProperty, value); }
        }

        private static void OnHeaderContainerStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GridColumn c)
                c.OnPropertyChanged(HeaderContainerStyleProperty.Name);
        }
        #endregion

        #region HeaderTemplateProperty
        /// <summary>
        /// HeaderTemplate DependencyProperty
        /// </summary>
        public static readonly DependencyProperty HeaderTemplateProperty = DependencyProperty.Register(
            nameof(HeaderTemplate), typeof(DataTemplate), typeof(GridColumn),
            new FrameworkPropertyMetadata(OnHeaderTemplateChanged));

        /// <summary>
        /// column header template
        /// </summary>
        public DataTemplate HeaderTemplate
        {
            get { return (DataTemplate)GetValue(HeaderTemplateProperty); }
            set { SetValue(HeaderTemplateProperty, value); }
        }

        private static void OnHeaderTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GridColumn c)
                c.OnPropertyChanged(HeaderTemplateProperty.Name);
        }
        #endregion

        #region HeaderTemplateSelectorProperty
        /// <summary>
        /// HeaderTemplateSelector DependencyProperty
        /// </summary>
        public static readonly DependencyProperty HeaderTemplateSelectorProperty = DependencyProperty.Register(
            nameof(HeaderTemplateSelector), typeof(DataTemplateSelector), typeof(GridColumn),
            new FrameworkPropertyMetadata(OnHeaderTemplateSelectorChanged));

        /// <summary>
        /// header template selector
        /// </summary>
        /// <remarks>
        ///     This property is ignored if <seealso cref="HeaderTemplate"/> is set.
        /// </remarks>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DataTemplateSelector HeaderTemplateSelector
        {
            get { return (DataTemplateSelector)GetValue(HeaderTemplateSelectorProperty); }
            set { SetValue(HeaderTemplateSelectorProperty, value); }
        }

        private static void OnHeaderTemplateSelectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GridColumn c)
                c.OnPropertyChanged(HeaderTemplateSelectorProperty.Name);
        }
        #endregion

        #region HeaderStringFormatProperty
        /// <summary>
        ///     The DependencyProperty for the HeaderStringFormat property.
        ///     Flags:              None
        ///     Default Value:      null
        /// </summary>
        public static readonly DependencyProperty HeaderStringFormatProperty = DependencyProperty.Register(
            nameof(HeaderStringFormat), typeof(string), typeof(GridColumn),
            new FrameworkPropertyMetadata(null, OnHeaderStringFormatChanged));

        /// <summary>
        ///     HeaderStringFormat is the format used to display the header content as a string.
        ///     This arises only when no template is available.
        /// </summary>
        public string HeaderStringFormat
        {
            get { return (string)GetValue(HeaderStringFormatProperty); }
            set { SetValue(HeaderStringFormatProperty, value); }
        }

        /// <summary>
        ///     Called when HeaderStringFormatProperty is invalidated on "d."
        /// </summary>
        private static void OnHeaderStringFormatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GridColumn c)
            {
                c.ResetHeaderTemplate();
                c.OnPropertyChanged(HeaderStringFormatProperty.Name);
            }
        }
        #endregion

        #region CellContainerStyleProperty
        /// <summary>
        /// CellContainerStyle DependencyProperty
        /// </summary>
        public static readonly DependencyProperty CellContainerStyleProperty = DependencyProperty.Register(
            nameof(CellContainerStyle), typeof(Style), typeof(GridColumn),
            new FrameworkPropertyMetadata(OnCellContainerStyleChanged));

        /// <summary>
        /// Cell container's style
        /// </summary>
        public Style CellContainerStyle
        {
            get { return (Style)GetValue(CellContainerStyleProperty); }
            set { SetValue(CellContainerStyleProperty, value); }
        }

        private static void OnCellContainerStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GridColumn c)
            {
                c.OnPropertyChanged(CellContainerStyleProperty.Name);
            }
        }
        #endregion

        #region CellStringFormatProperty
        /// <summary>
        ///     The DependencyProperty for the CellStringFormat property.
        ///     Flags:              None
        ///     Default Value:      null
        /// </summary>
        public static readonly DependencyProperty CellStringFormatProperty = DependencyProperty.Register(
            nameof(CellStringFormat), typeof(string), typeof(GridColumn),
            new FrameworkPropertyMetadata(null, OnCellStringFormatChanged));

        /// <summary>
        ///     CellStringFormat is the format used to display the cell content as a string.
        ///     This arises only when no template is available.
        /// </summary>
        public string CellStringFormat
        {
            get { return (string)GetValue(CellStringFormatProperty); }
            set { SetValue(CellStringFormatProperty, value); }
        }

        /// <summary>
        ///     Called when CellStringFormatProperty is invalidated on "d."
        /// </summary>
        private static void OnCellStringFormatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GridColumn c)
            {
                c.ResetRendererData();
                c.OnCellStringFormatChanged((string)e.OldValue, (string)e.NewValue);
            }
        }

        /// <summary>
        ///     This method is invoked when the CellStringFormat property changes.
        /// </summary>
        /// <param name="oldCellStringFormat">The old value of the CellStringFormat property.</param>
        /// <param name="newCellStringFormat">The new value of the CellStringFormat property.</param>
        protected virtual void OnCellStringFormatChanged(string oldCellStringFormat, string newCellStringFormat)
        {
        }
        #endregion

        #region IsHeaderClickableProperty
        public static readonly DependencyProperty IsHeaderClickableProperty = DependencyProperty.Register(
            nameof(IsHeaderClickable), typeof(bool), typeof(GridColumn),
            new FrameworkPropertyMetadata(true));

        public bool IsHeaderClickable
        {
            get => (bool)GetValue(IsHeaderClickableProperty);
            set { SetValue(IsHeaderClickableProperty, value); }
        }
        #endregion

        #region IsResizableProperty
        public static readonly DependencyProperty IsResizableProperty = DependencyProperty.Register(
            nameof(IsResizable), typeof(bool), typeof(GridColumn),
            new FrameworkPropertyMetadata(true));

        public bool IsResizable
        {
            get => (bool)GetValue(IsResizableProperty);
            set { SetValue(IsResizableProperty, value); }
        }
        #endregion

        #region WidthProperty
        internal static readonly DependencyProperty WidthProperty = DependencyProperty.Register(
                nameof(Width), typeof(GridColumnWidth), typeof(GridColumn), new PropertyMetadata(null, OnWidthChanged));

        [TypeConverter(typeof(GridColumnWidthConverter))]
        public GridColumnWidth Width
        {
            get { return (GridColumnWidth)GetValue(WidthProperty); }
            set { SetValue(WidthProperty, value); }
        }

        private static void OnWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GridColumn c)
            {
                c.UpdateColumnWidth();
            }
        }
        #endregion

        #region ColumnWidthProperty
        private static readonly DependencyProperty ColumnWidthProperty = DependencyProperty.Register(
                nameof(ColumnWidth), typeof(double), typeof(GridColumn), new PropertyMetadata(double.NaN, OnColumnWidthChanged));

        [TypeConverter(typeof(GridColumnWidthConverter))]
        internal double ColumnWidth
        {
            get { return (double)GetValue(ColumnWidthProperty); }
            set { SetValue(ColumnWidthProperty, value); }
        }

        private static void OnColumnWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GridColumn c)
            {
                double newWidth = (double)e.NewValue;

                c.State = double.IsNaN(newWidth) ? ColumnMeasureState.Init : ColumnMeasureState.SpecificWidth;
                c.OnPropertyChanged(WidthProperty.Name);
            }
        }
        #endregion

        #region AverageCharWidthProperty
        private static readonly DependencyProperty AverageCharWidthProperty = DependencyProperty.Register(
                nameof(AverageCharWidth), typeof(double), typeof(GridColumn), new PropertyMetadata(0.0, OnAverageCharWidthChanged));

        [TypeConverter(typeof(GridColumnWidthConverter))]
        internal double AverageCharWidth
        {
            get { return (double)GetValue(AverageCharWidthProperty); }
            set { SetValue(AverageCharWidthProperty, value); }
        }

        private static void OnAverageCharWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GridColumn c)
            {
                c.ResetRendererData();
                c.UpdateColumnWidth();
            }
        }
        #endregion
        
        #endregion

        #region Properties
        public double ActualWidth
        {
            get => _actualWidth;
            internal set
            {
                if (_actualWidth != value)
                {
                    _actualWidth = value;
                    OnPropertyChanged(c_ActualWidthName);
                }
            }
        }

        public double DesiredWidth
        {
            get { return _desiredWidth; }
            private set { _desiredWidth = value; }
        }

        public double FinalWidth
        {
            get
            {
                double width;
                switch (State)
                {
                    case ColumnMeasureState.Init:
                    case ColumnMeasureState.Headered:
                    case ColumnMeasureState.Data:
                        width = DesiredWidth;
                        break;
                    default:
                        width = ColumnWidth;
                        break;
                }
                return DoubleUtil.IsNaN(width) ? ActualWidth : width;
            }
        }

        public int ActualIndex
        {
            get { return _actualIndex; }
            internal set { _actualIndex = value; }
        }

        internal GridControl GridOwner
        {
            get => _owner;
            set
            {
                if (_owner != value)
                {
                    _owner = value;
                    ResetRendererData();
                }
            }
        }

        internal ColumnMeasureState State
        {
            get { return _state; }
            set
            {
                if (_state != value)
                {
                    _state = value;

                    if (value != ColumnMeasureState.Init) // Headered, Data or SpecificWidth
                    {
                        UpdateActualWidth();
                    }
                    else
                    {
                        DesiredWidth = 0.0;
                    }
                }
                else if (value == ColumnMeasureState.SpecificWidth)
                {
                    UpdateActualWidth();
                }
            }
        }

        public virtual TextAlignment HeaderTextAlignment
        {
            get
            {
                var info = ColumnInfo;
                if (info != null) return info.HeaderAlignment;
                return TextAlignment.Center;
            }
        }

        public virtual TextAlignment CellTextAlignment
        {
            get
            {
                var info = ColumnInfo;
                if (info != null) return info.ColumnAlignment;
                return TextAlignment.Left;
            }
        }

        public virtual DataTemplate DefaultHeaderTemplate
        {
            get
            {
                if (_defaultHeaderTemplate == null)
                {
                    FrameworkElementFactory factory = new FrameworkElementFactory(typeof(TextBlock));
                    factory.SetValue(TextBlock.MarginProperty, GridControl.c_defaultCellPadding);
                    factory.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
                    factory.SetValue(TextBlock.TextAlignmentProperty, HeaderTextAlignment);
                    factory.SetValue(TextBlock.FontWeightProperty, FontWeights.SemiBold);

                    Binding binding = new Binding() { Mode = BindingMode.OneWay };
                    binding.StringFormat = HeaderStringFormat;
                    factory.SetBinding(TextBlock.TextProperty, binding);

                    _defaultHeaderTemplate = new DataTemplate()
                    {
                        VisualTree = factory
                    };
                }
                return _defaultHeaderTemplate;
            }
        }

        public virtual ICellRenderer CellRenderer
        {
            get
            {
                var owner = GridOwner;
                if (_renderer == null && owner != null)
                {
                    _renderer = new TextCellRenderer(owner.FontFamily, owner.FontStyle, owner.FontWeight, owner.FontStretch, owner.FontSize,
                        CellTextAlignment, TextWrapping.NoWrap)
                    {
                        Trimming = TextTrimming.WordEllipsis,
                        StringFormat = CellStringFormat
                    };
                    _renderer.VerticalAlignment = VerticalAlignment.Center;
                }
                return _renderer;
            }
        }
        #endregion

        #region Methods
        internal void ResetPrivateData()
        {
            _actualIndex = -1;
            _desiredWidth = 0.0;
            _state = double.IsNaN(ColumnWidth) ? ColumnMeasureState.Init : ColumnMeasureState.SpecificWidth;
        }

        internal void ResetRendererData()
        {
            _renderer = null;
        }

        private void ResetHeaderTemplate()
        {
            _defaultHeaderTemplate = null;
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (_propertyChanged != null)
            {
                _propertyChanged(this, e);
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        internal double EnsureWidth(double width)
        {
            if (width > DesiredWidth)
            {
                DesiredWidth = width;
            }
            return DesiredWidth;
        }

        private void UpdateActualWidth()
        {
            ActualWidth = (State == ColumnMeasureState.SpecificWidth) ? ColumnWidth : DesiredWidth;
        }

        private void UpdateColumnWidth()
        {
            double widthValue;
            var width = Width;
            if (width == null)
            {
                widthValue = c_defaultColumnWidth.Value;
            }
            else
            {
                var charWidth = AverageCharWidth;
                if (charWidth > 0.0 && !width.IsInPixels)
                {
                    widthValue = width.Value * charWidth;
                }
                else
                {
                    widthValue = width.Value;
                }
            }

            ColumnWidth = widthValue;
        }

        private void OnColumnInfoChanged(GridColumnInfo columnInfo)
        {
            if (columnInfo != null && !columnInfo.Equals(this))
            {
                Width = columnInfo.ColumnWidth;
                IsHeaderClickable = columnInfo.IsHeaderClickable;
                IsResizable = columnInfo.IsResizable;

                OnPropertyChanged(ColumnInfoProperty.Name);
            }
        }

        public virtual FrameworkElement GetCellDisplayElement()
        {
            return null;
        }

        public virtual void UpdateDisplayContent(FrameworkElement element, object content)
        {
        }

        public virtual FrameworkElement GetCellEditingElement()
        {
            TextBox element = new TextBox();

            element.Margin = GridControl.c_defaultCellTextBoxMargin;
            element.Padding = GridControl.c_defaultCellTextBoxPadding;
            element.BorderThickness = new Thickness(0);
            element.VerticalAlignment = VerticalAlignment.Stretch;
            element.VerticalContentAlignment = VerticalAlignment.Center;
            element.TextAlignment = CellTextAlignment;

            return element;
        }

        public virtual void UpdateEditingElement(FrameworkElement element, object content)
        {
            if (element != null && element is TextBox textElement)
            {
                if (content == null)
                {
                    textElement.Text = string.Empty;
                }
                else
                {
                    string stringFormat = CellStringFormat;
                    if (string.IsNullOrEmpty(stringFormat))
                        stringFormat = "{0}";
                    textElement.SelectedText = string.Format(CultureInfo.CurrentCulture, stringFormat, content.ToString());
                }
            }
        }

        public virtual object GetEditedValue(FrameworkElement element)
        {
            if (element != null && element is TextBox textElement)
            {
                return textElement.Text;
            }
            return null;
        }
        #endregion
    }
}
