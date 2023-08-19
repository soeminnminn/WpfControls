using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ComboBox = System.Windows.Controls.ComboBox;

namespace ColorPicker
{
    /// <summary>
    /// Represents a control that lets the user pick a color.
    /// </summary>
    [TemplatePart(Name = PartColorPickerPanel, Type = typeof(ColorPickerPanel))]
    public class ColorPicker : ComboBox
    {
        #region Dependency Properties
        /// <summary>
        /// Identifies the <see cref="SelectedColor"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectedColorProperty = DependencyProperty.Register(
            nameof(SelectedColor), typeof(Color?), typeof(ColorPicker),
            new FrameworkPropertyMetadata(Color.FromArgb(0, 0, 0, 0),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                SelectedColorChanged, CoerceSelectedColorValue));

        /// <summary>
        /// Identifies the <see cref="IsOpaqueColor"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsOpaqueColorProperty = DependencyProperty.Register(
            nameof(IsOpaqueColor), typeof(bool), typeof(ColorPicker),
            new FrameworkPropertyMetadata(false, 
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, IsOpaqueColorChanged));
        #endregion

        #region Variables
        /// <summary>
        /// The color picker panel part constant.
        /// </summary>
        private const string PartColorPickerPanel = "PART_ColorPickerPanel";

        /// <summary>
        /// The color picker panel.
        /// </summary>
        private ColorPickerPanel colorPickerPanel;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes static members of the <see cref="ColorPicker" /> class.
        /// </summary>
        static ColorPicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ColorPicker), new FrameworkPropertyMetadata(typeof(ColorPicker)));

            SelectedValueProperty.OverrideMetadata(typeof(ColorPicker),
                new FrameworkPropertyMetadata(Color.FromArgb(0, 0, 0, 0),
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    SelectedColorChanged, CoerceSelectedColorValue));
        }
        #endregion

        #region Properties
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
        #endregion

        #region Methods
        /// <summary>
        /// Called when <see cref="M:System.Windows.FrameworkElement.ApplyTemplate" /> is called.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            colorPickerPanel = GetTemplateChild(PartColorPickerPanel) as ColorPickerPanel;
            if (colorPickerPanel != null)
            {
                colorPickerPanel.PredefinedColorPanelSelectionChangedEvent += OnPredefinedColorPanelSelectionChanged;
            }
        }

        /// <summary>
        /// Coerces the value of the <see cref="SelectedColor" /> property.
        /// </summary>
        /// <param name="basevalue">The base value.</param>
        /// <returns>
        /// The coerced <see cref="SelectedColor" /> value.
        /// </returns>
        protected virtual object CoerceSelectedColorValue(object basevalue)
        {
            if (basevalue == null)
                return SelectedColor;

            return basevalue;
        }

        /// <summary>
        /// Reports when a combo box's popup opens.
        /// </summary>
        /// <param name="e">The event data for the <see cref="E:System.Windows.Controls.ComboBox.DropDownOpened" /> event.</param>
        protected override void OnDropDownOpened(EventArgs e)
        {
            base.OnDropDownOpened(e);
            colorPickerPanel.Focus();
        }

        /// <summary>
        /// Handles changes in selected color.
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs" /> instance containing the event data.</param>
        private static void SelectedColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ColorPicker)d).OnSelectedColorChanged(e);
        }

        /// <summary>
        /// Coerces the selected color value.
        /// </summary>
        /// <param name="d">The sender.</param>
        /// <param name="basevalue">The base value.</param>
        /// <returns>
        /// The coerced value.
        /// </returns>
        private static object CoerceSelectedColorValue(DependencyObject d, object basevalue)
        {
            return ((ColorPicker)d).CoerceSelectedColorValue(basevalue);
        }

        /// <summary>
        /// Handles changes in selected color.
        /// </summary>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs" /> instance containing the event data.</param>
        protected virtual void OnSelectedColorChanged(DependencyPropertyChangedEventArgs e)
        {
        }

        /// <summary>
        /// Handles changes in is opaque color.
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs" /> instance containing the event data.</param>
        private static void IsOpaqueColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ColorPicker)d).OnIsOpaqueColorChanged(e);
        }

        /// <summary>
        /// Handles changes in is opaque color.
        /// </summary>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs" /> instance containing the event data.</param>
        protected virtual void OnIsOpaqueColorChanged(DependencyPropertyChangedEventArgs e)
        {
        }

        /// <summary>
        /// Handles the <see cref="E:PredefinedColorPanelSelectionChangedEvent" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="SelectionChangedEventArgs" /> instance containing the event data.</param>
        private void OnPredefinedColorPanelSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (IsDropDownOpen)
                IsDropDownOpen = false;

            args.Handled = true;
        }
        #endregion
    }
}
