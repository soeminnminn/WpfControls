using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace ColorPicker.Converters
{
    /// <summary>
    /// Converts <see cref="Color" /> instances to <see cref="string" /> instances..
    /// </summary>
    [ValueConversion(typeof(Color), typeof(string))]
    public class ColorToStringConverter : IValueConverter
    {
        /// <summary>
        /// The string to color map.
        /// </summary>
        private static Dictionary<string, Color> colors = null;

        /// <summary>
        /// Gets the string to color map.
        /// </summary>
        /// <value>The color map.</value>
        public static Dictionary<string, Color> ColorMap
        {
            get
            {
                if (colors == null)
                {
                    colors = new Dictionary<string, Color>();
                    foreach (var key in ColorHelper.colors.Keys)
                    {
                        var c = ColorHelper.colors[key];
                        colors.Add(key, c);
                    }

                    foreach (var key in ColorHelper.systemColors.Keys)
                    {
                        var c = ColorHelper.systemColors[key];
                        colors.Add(key, c);
                    }

                    colors.Add("Undefined", ColorHelper.UndefinedColor);
                }

                return colors;
            }
        }

        public bool IsOpaque { get; set; }

        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns <c>null</c>, the valid <c>null</c> value is used.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            var color = (Color)value;

            // find the color name
            foreach (var kvp in ColorMap)
            {
                if (color == kvp.Value)
                    return kvp.Key;
            }

            return ColorHelper.ColorToHex(color, IsOpaque);
        }

        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns <c>null</c>, the valid <c>null</c> value is used.
        /// </returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var s = value as string;
            if (s == null)
                return DependencyProperty.UnsetValue;

            if (ColorMap.TryGetValue(s, out Color color))
                return color;

            var c = ColorHelper.HexToColor(s);
            if (c != ColorHelper.UndefinedColor)
                return c;

            return DependencyProperty.UnsetValue;
        }
    }
}
