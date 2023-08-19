using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Linq;
using System.Windows.Media;

namespace ColorPicker
{
    /// <summary>
    /// Represents a control that lets the user pick a color.
    /// </summary>
    [TemplatePart(Name = PartHsv, Type = typeof(HsvControl))]
    [TemplatePart(Name = PartPredefinedColorPanel, Type = typeof(StackPanel))]
    internal class ColorPickerPanel : Control, INotifyPropertyChanged
    {
        #region Dependency Properties
        /// <summary>
        /// Identifies the <see cref="Alpha"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty AlphaProperty = DependencyProperty.Register(
            nameof(Alpha), typeof(int), typeof(ColorPickerPanel),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, ComponentChanged));

        /// <summary>
        /// Identifies the <see cref="Blue"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty BlueProperty = DependencyProperty.Register(
            nameof(Blue), typeof(int), typeof(ColorPickerPanel),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, ComponentChanged));

        /// <summary>
        /// Identifies the <see cref="Brightness"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty BrightnessProperty = DependencyProperty.Register(
            nameof(Brightness), typeof(int), typeof(ColorPickerPanel),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, ComponentChanged));

        /// <summary>
        /// Identifies the <see cref="Green"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty GreenProperty = DependencyProperty.Register(
            nameof(Green), typeof(int), typeof(ColorPickerPanel),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, ComponentChanged));

        /// <summary>
        /// Identifies the <see cref="Hue"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HueProperty = DependencyProperty.Register(
            nameof(Hue), typeof(int), typeof(ColorPickerPanel),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, ComponentChanged));

        /// <summary>
        /// Identifies the <see cref="Red"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty RedProperty = DependencyProperty.Register(
            nameof(Red), typeof(int), typeof(ColorPickerPanel),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, ComponentChanged));

        /// <summary>
        /// Identifies the <see cref="Saturation"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SaturationProperty = DependencyProperty.Register(
            nameof(Saturation), typeof(int), typeof(ColorPickerPanel),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, ComponentChanged));

        /// <summary>
        /// Identifies the <see cref="SelectedColor"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectedColorProperty = DependencyProperty.Register(
            nameof(SelectedColor), typeof(Color?), typeof(ColorPickerPanel),
            new FrameworkPropertyMetadata(Color.FromArgb(0, 0, 0, 0),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, SelectedColorChanged, CoerceSelectedColorValue));

        /// <summary>
        /// Identifies the <see cref="IsOpaqueColor"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsOpaqueColorProperty = DependencyProperty.Register(
            nameof(IsOpaqueColor), typeof(bool), typeof(ColorPickerPanel),
            new FrameworkPropertyMetadata(false,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, IsOpaqueColorChanged));
        #endregion

        #region Variables
        /// <summary>
        /// The HSV control part name.
        /// </summary>
        private const string PartHsv = "PART_HSV";

        /// <summary>
        /// The predefined color panel part name
        /// </summary>
        private const string PartPredefinedColorPanel = "PART_PredefinedColorPanel";

        /// <summary>
        /// The max number of recent colors.
        /// </summary>
        private static int maxNumberOfRecentColors = 20;

        /// <summary>
        /// The show hsv panel.
        /// </summary>
        private bool showHsvPanel;

        /// <summary>
        /// The HSV control.
        /// </summary>
        private HsvControl hsvControl;

        /// <summary>
        /// The within color change.
        /// </summary>
        private bool withinColorChange;

        /// <summary>
        /// The within component change.
        /// </summary>
        private bool withinComponentChange;

        /// <summary>
        /// The property changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// The predefined colors selection changed event
        /// </summary>
        public event SelectionChangedEventHandler PredefinedColorPanelSelectionChangedEvent;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes static members of the <see cref="ColorPickerPanel" /> class.
        /// </summary>
        static ColorPickerPanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ColorPickerPanel), new FrameworkPropertyMetadata(typeof(ColorPickerPanel)));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ColorPickerPanel" /> class.
        /// </summary>
        public ColorPickerPanel()
        {
            InitPalette(false);
            Strings = new ColorPickerPanelStrings();

            Unloaded += PanelUnloaded;
            OpacityVariations = new ObservableCollection<Color>();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the localized strings.
        /// </summary>
        public ColorPickerPanelStrings Strings { get; set; }

        /// <summary>
        /// Gets the theme colors.
        /// </summary>
        /// <value>The theme colors.</value>
        public ObservableCollection<Color> ThemeColors { get; private set; }

        /// <summary>
        /// Gets the standard colors.
        /// </summary>
        /// <value>The standard colors.</value>
        public ObservableCollection<Color> StandardColors { get; private set; }

        /// <summary>
        /// Gets the basic colors.
        /// </summary>
        /// <value>The basic colors.</value>
        public ObservableCollection<Color> BasicColors { get; private set; }

        /// <summary>
        /// Gets the opacity colors.
        /// </summary>
        /// <value>The opacity colors.</value>
        public ObservableCollection<Color> OpacityVariations { get; private set; }

        /// <summary>
        /// Gets the recent colors.
        /// </summary>
        /// <value>The recent colors.</value>
        public ObservableCollection<Color> RecentColors { get; private set; }

        /// <summary>
        /// Gets or sets the alpha value.
        /// </summary>
        /// <value>The alpha.</value>
        public int Alpha
        {
            get => (int)GetValue(AlphaProperty);
            set { SetValue(AlphaProperty, value); }
        }

        /// <summary>
        /// Gets or sets the blue.
        /// </summary>
        /// <value>The blue.</value>
        public int Blue
        {
            get => (int)GetValue(BlueProperty);
            set { SetValue(BlueProperty, value); }
        }

        /// <summary>
        /// Gets or sets the brightness.
        /// </summary>
        /// <value>The brightness.</value>
        public int Brightness
        {
            get => (int)GetValue(BrightnessProperty);
            set { SetValue(BrightnessProperty, value); }
        }

        /// <summary>
        /// Gets or sets the green.
        /// </summary>
        /// <value>The green.</value>
        public int Green
        {
            get => (int)GetValue(GreenProperty);
            set { SetValue(GreenProperty, value); }
        }

        /// <summary>
        /// Gets or sets the hue.
        /// </summary>
        /// <value>The hue.</value>
        public int Hue
        {
            get => (int)GetValue(HueProperty);
            set { SetValue(HueProperty, value); }
        }

        /// <summary>
        /// Gets or sets the max number of recent colors.
        /// </summary>
        /// <value>The max number of recent colors.</value>
        public int MaxNumberOfRecentColors
        {
            get => maxNumberOfRecentColors;
            set { maxNumberOfRecentColors = value; }
        }

        /// <summary>
        /// Gets or sets the red value.
        /// </summary>
        /// <value>The red.</value>
        public int Red
        {
            get => (int)GetValue(RedProperty);
            set { SetValue(RedProperty, value); }
        }

        /// <summary>
        /// Gets or sets the saturation.
        /// </summary>
        /// <value>The saturation.</value>
        public int Saturation
        {
            get => (int)GetValue(SaturationProperty);
            set { SetValue(SaturationProperty, value); }
        }

        /// <summary>
        /// Gets or sets the selected color.
        /// </summary>
        /// <value>The color of the selected.</value>
        public Color? SelectedColor
        {
            get => (Color?)GetValue(SelectedColorProperty);
            set { SetValue(SelectedColorProperty, value); }
        }

        /// <summary>
        /// Gets or sets the is opaque color.
        /// </summary>
        /// <value>The color of the selected.</value>
        public bool IsOpaqueColor
        {
            get => (bool)GetValue(SelectedColorProperty);
            set { SetValue(SelectedColorProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to show the HSV panel.
        /// </summary>
        /// <remarks>The backing field is static.</remarks>
        public bool ShowHsvPanel
        {
            get => showHsvPanel;
            set
            {
                showHsvPanel = value;
                RaisePropertyChanged("ShowHsvPanel");
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal processes call <see
        /// cref="M:System.Windows.FrameworkElement.ApplyTemplate" />.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            hsvControl = GetTemplateChild(PartHsv) as HsvControl;
            var predefinedColorPanel = GetTemplateChild(PartPredefinedColorPanel) as StackPanel;
            if (predefinedColorPanel != null)
            {
                predefinedColorPanel.AddHandler(Selector.SelectionChangedEvent, (SelectionChangedEventHandler)OnPredefinedColorPanelSelectionChanged, true);
            }
        }

        /// <summary>
        /// Coerces the selected color value.
        /// </summary>
        /// <param name="baseValue">The base value.</param>
        /// <returns>
        /// The coerced selected color value.
        /// </returns>
        protected virtual object CoerceSelectedColorValue(object baseValue)
        {
            if (baseValue == null)
            {
                return SelectedColor;
            }

            return baseValue;
        }

        /// <summary>
        /// Called when a color component is changed.
        /// </summary>
        /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs" /> instance containing the event data.</param>
        protected virtual void OnComponentChanged(DependencyPropertyChangedEventArgs e)
        {
            if (withinColorChange)
            {
                return;
            }

            if (SelectedColor == null)
            {
                return;
            }

            var color = SelectedColor.Value;
            withinComponentChange = true;
            withinColorChange = true;
            var i = Convert.ToInt32(e.NewValue);
            byte x = i <= 255 ? (byte)i : (byte)255;
            if (e.Property == AlphaProperty)
            {
                SelectedColor = Color.FromArgb(x, color.R, color.G, color.B);
            }

            if (e.Property == RedProperty)
            {
                SelectedColor = Color.FromArgb(color.A, x, color.G, color.B);
                UpdateHSV(color);
            }

            if (e.Property == GreenProperty)
            {
                SelectedColor = Color.FromArgb(color.A, color.R, x, color.B);
                UpdateHSV(color);
            }

            if (e.Property == BlueProperty)
            {
                SelectedColor = Color.FromArgb(color.A, color.R, color.G, x);
                UpdateHSV(color);
            }

            var hsv = color.ColorToHsv();
            double y = Convert.ToDouble(e.NewValue);
            if (e.Property == HueProperty)
            {
                SelectedColor = ColorHelper.HsvToColor(y / 360, hsv[1], hsv[2], color.A / 255.0);
                UpdateRGB(SelectedColor.Value);
            }

            if (e.Property == SaturationProperty)
            {
                SelectedColor = ColorHelper.HsvToColor(hsv[0], y / 100, hsv[2], color.A / 255.0);
                UpdateRGB(SelectedColor.Value);
            }

            if (e.Property == BrightnessProperty)
            {
                SelectedColor = ColorHelper.HsvToColor(hsv[0], hsv[1], y / 100, color.A / 255.0);
                UpdateRGB(SelectedColor.Value);
            }

            withinColorChange = false;
            withinComponentChange = false;
        }

        /// <summary>
        /// Called when the selected color changed.
        /// </summary>
        /// <param name="newColor">The new color.</param>
        /// <param name="oldColor">The old color.</param>
        protected virtual void OnSelectedColorChanged(Color? newColor, Color? oldColor)
        {
            if (!withinColorChange && !withinComponentChange && newColor != null)
            {
                UpdateRGB(newColor.Value);
                UpdateHSV(newColor.Value);
                UpdateOpacityVariations(newColor.Value);
            }
        }

        /// <summary>
        /// Handles changes in is opaque color.
        /// </summary>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs" /> instance containing the event data.</param>
        protected virtual void OnIsOpaqueColorChanged(bool oldvalue, bool newValue)
        {
            if (oldvalue != newValue)
            {
                InitPalette(newValue);
                if (newValue)
                    SelectedColor?.ChangeAlpha(255);
            }
        }

        /// <summary>
        /// The raise property changed.
        /// </summary>
        /// <param name="property">The property.</param>
        protected void RaisePropertyChanged(string property)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(property));
            }
        }

        /// <summary>
        /// Coerces the selected color value.
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="basevalue">The base value.</param>
        /// <returns>
        /// The coerce selected color value.
        /// </returns>
        private static object CoerceSelectedColorValue(DependencyObject d, object basevalue)
        {
            return ((ColorPickerPanel)d).CoerceSelectedColorValue(basevalue);
        }

        /// <summary>
        /// Called when a color component is changed.
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs" /> instance containing the event data.</param>
        private static void ComponentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ColorPickerPanel)d).OnComponentChanged(e);
        }

        /// <summary>
        /// Initializes the palettes.
        /// </summary>
        private void InitPalette(bool opaqueOnly)
        {
            ThemeColors = new ObservableCollection<Color>(ColorHelper.themeColors.Select(x => ColorHelper.UIntToColor(x)));
            
            if (opaqueOnly)
                StandardColors = new ObservableCollection<Color>(ColorHelper.standardOpaqueColors.Select(x => ColorHelper.UIntToColor(x)));
            else
                StandardColors = new ObservableCollection<Color>(ColorHelper.standardColors.Select(x => ColorHelper.UIntToColor(x)));

            BasicColors = new ObservableCollection<Color>(ColorHelper.basicColors.Select(x => ColorHelper.UIntToColor(x)));

            RecentColors = new ObservableCollection<Color>();
        }

        /// <summary>
        /// The selected color changed.
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The e.</param>
        private static void SelectedColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ColorPickerPanel)d).OnSelectedColorChanged((Color?)e.NewValue, (Color?)e.OldValue);
        }

        /// <summary>
        /// Handles changes in is opaque color.
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs" /> instance containing the event data.</param>
        private static void IsOpaqueColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ColorPickerPanel)d).OnIsOpaqueColorChanged((bool)e.OldValue, (bool)e.NewValue);
        }

        /// <summary>
        /// The add color to recent colors if missing.
        /// </summary>
        /// <param name="color">The color.</param>
        private void AddColorToRecentColorsIfMissing(Color color)
        {
            // Check if the color exists
            if (RecentColors.Contains(color))
            {
                var index = RecentColors.IndexOf(color);
                RecentColors.Move(index, 0);
                return;
            }

            if (RecentColors.Count >= MaxNumberOfRecentColors)
            {
                RecentColors.RemoveAt(RecentColors.Count - 1);
            }

            RecentColors.Insert(0, color);
        }

        /// <summary>
        /// Handles the <see cref="E:PredefinedColorPanelSelectionChangedEvent" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="SelectionChangedEventArgs" /> instance containing the event data.</param>
        private void OnPredefinedColorPanelSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            var listBox = args.OriginalSource as ListBox;

            if (listBox != null && args.AddedItems.Count != 0)
            {
                SelectedColor = (Color)args.AddedItems[0];
                listBox.UnselectAll();
            }

            if (PredefinedColorPanelSelectionChangedEvent != null)
            {
                PredefinedColorPanelSelectionChangedEvent(sender, args);
            }
        }

        /// <summary>
        /// Called when the panel is unloaded.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs" /> instance containing the event data.</param>
        private void PanelUnloaded(object sender, RoutedEventArgs e)
        {
            if (SelectedColor != null)
            {
                AddColorToRecentColorsIfMissing(SelectedColor.Value);
            }
        }

        /// <summary>
        /// Updates the opacity variation collection.
        /// </summary>
        /// <param name="color">The currently selected color.</param>
        private void UpdateOpacityVariations(Color color)
        {
            OpacityVariations.Clear();
            for (int i = 1; i <= 9; i++)
            {
                OpacityVariations.Add(Color.FromArgb((byte)(255 * (i * 0.1)), color.R, color.G, color.B));
            }
        }

        /// <summary>
        /// Updates the hue, saturation and brightness properties.
        /// </summary>
        /// <param name="color">The currently selected color.</param>
        // ReSharper disable once InconsistentNaming
        private void UpdateHSV(Color color)
        {
            withinColorChange = true;
            if (hsvControl != null)
            {
                hsvControl.withinUpdate = true;
            }

            var hsv = color.ColorToHsv();
            Hue = (int)(hsv[0] * 360);
            Saturation = (int)(hsv[1] * 100);
            Brightness = (int)(hsv[2] * 100);
            withinColorChange = false;
            if (hsvControl != null)
            {
                hsvControl.withinUpdate = false;
            }
        }

        /// <summary>
        /// Updates the red, green, blue and alpha properties.
        /// </summary>
        /// <param name="color">The color.</param>
        // ReSharper disable once InconsistentNaming
        private void UpdateRGB(Color color)
        {
            withinColorChange = true;
            if (hsvControl != null)
            {
                hsvControl.withinUpdate = true;
            }

            Alpha = color.A;
            Red = color.R;
            Green = color.G;
            Blue = color.B;
            withinColorChange = false;
            if (hsvControl != null)
            {
                hsvControl.withinUpdate = false;
            }
        }
        #endregion
    }
}
