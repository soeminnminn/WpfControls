using System;

namespace ColorPicker
{
    /// <summary>
    /// Provides localized strings for the color picker panel.
    /// </summary>
    internal class ColorPickerPanelStrings
    {
        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="ColorPickerPanelStrings" /> class.
        /// </summary>
        public ColorPickerPanelStrings()
        {
            ThemeColors = "Theme colors";
            StandardColors = "Standard colors";
            BasicColors = "Basic colors";
            RecentColors = "Recent colors";
            OpacityVariations = "Opacity variations";
            Values = "Values";
            CustomColor = "Custom color";
            TogglePanelToolTip = "Toggle between palette and color value panels.";
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the tool tip for the 'toggle panel' button.
        /// </summary>
        public string TogglePanelToolTip { get; set; }

        /// <summary>
        /// Gets or sets the 'HSV' string.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public string CustomColor { get; set; }

        /// <summary>
        /// Gets or sets the 'Theme colors' string.
        /// </summary>
        public string ThemeColors { get; set; }

        /// <summary>
        /// Gets or sets the 'Standard colors' string.
        /// </summary>
        public string StandardColors { get; set; }

        /// <summary>
        /// Gets or sets the 'Basic colors' string.
        /// </summary>
        public string BasicColors { get; set; }

        /// <summary>
        /// Gets or sets the 'Opacity variations' string.
        /// </summary>
        public string OpacityVariations { get; set; }

        /// <summary>
        /// Gets or sets 'RecentColors' string.
        /// </summary>
        public string RecentColors { get; set; }

        /// <summary>
        /// Gets or sets the 'Values' string.
        /// </summary>
        public string Values { get; set; }
        #endregion
    }
}
