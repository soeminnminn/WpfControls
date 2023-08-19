using System;
using System.Windows;
using System.Windows.Media;

namespace ColorPicker
{
    /// <summary>
    /// Represents a color slider.
    /// </summary>
    /// <remarks>Original code by Ury Jamshy, 21 July 2011.
    /// See http://www.codeproject.com/KB/WPF/ColorPicker010.aspx
    /// The Code Project Open License (CPOL)
    /// http://www.codeproject.com/info/cpol10.aspx</remarks>
    internal class ColorSlider : SliderEx
    {
        #region Dependency Properties
        /// <summary>
        /// Identifies the <see cref="LeftColor"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty LeftColorProperty = DependencyProperty.Register(
            nameof(LeftColor), typeof(Color?), typeof(ColorSlider),
            new UIPropertyMetadata(Colors.Black));

        /// <summary>
        /// Identifies the <see cref="RightColor"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty RightColorProperty = DependencyProperty.Register(
            nameof(RightColor), typeof(Color?), typeof(ColorSlider),
            new UIPropertyMetadata(Colors.White));
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes static members of the <see cref="ColorSlider" /> class.
        /// </summary>
        static ColorSlider()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ColorSlider), new FrameworkPropertyMetadata(typeof(ColorSlider)));
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the left color.
        /// </summary>
        public Color? LeftColor
        {
            get => (Color)GetValue(LeftColorProperty);
            set { SetValue(LeftColorProperty, value); }
        }

        /// <summary>
        /// Gets or sets the right color.
        /// </summary>
        public Color? RightColor
        {
            get => (Color?)GetValue(RightColorProperty);
            set { SetValue(RightColorProperty, value); }
        }
        #endregion
    }
}
