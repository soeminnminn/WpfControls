using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace wpfDialogs
{
    public class ColorSlider : Slider
    {
        #region Variables
        private bool _isChanging = false;
        #endregion

        #region Constructors
        static ColorSlider()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorSlider), new FrameworkPropertyMetadata(typeof(ColorSlider)));
        }

        public ColorSlider()
            : base()
        {
            Minimum = 0.0;
            Maximum = 100.0;
        }
        #endregion

        #region Dependency Properties

        #region StartColorProperty
        public static readonly DependencyProperty StartColorProperty = DependencyProperty.Register(
            nameof(StartColor), typeof(Color), typeof(ColorSlider),
            new FrameworkPropertyMetadata(Colors.White));

        public Color StartColor
        {
            get => (Color)GetValue(StartColorProperty);
            set { SetValue(StartColorProperty, value); }
        }
        #endregion

        #region EndColorProperty
        public static readonly DependencyProperty EndColorProperty = DependencyProperty.Register(
            nameof(EndColor), typeof(Color), typeof(ColorSlider),
            new FrameworkPropertyMetadata(Colors.Black));

        public Color EndColor
        {
            get => (Color)GetValue(EndColorProperty);
            set { SetValue(EndColorProperty, value); }
        }
        #endregion

        #region CurrentColorProperty
        public static readonly DependencyProperty CurrentColorProperty = DependencyProperty.Register(
            nameof(CurrentColor), typeof(Color), typeof(ColorSlider), new FrameworkPropertyMetadata(Colors.Transparent));

        public Color CurrentColor
        {
            get => (Color)GetValue(CurrentColorProperty);
            set { SetValue(CurrentColorProperty, value); }
        }
        #endregion

        #region HueProperty
        public static readonly DependencyProperty HueProperty = DependencyProperty.Register(
            nameof(Hue), typeof(double), typeof(ColorSlider),
            new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnHueSaturationChanged)));

        public double Hue
        {
            get => (double)GetValue(HueProperty);
            set { SetValue(HueProperty, value); }
        }
        #endregion

        #region SaturationProperty
        public static readonly DependencyProperty SaturationProperty = DependencyProperty.Register(
            nameof(Saturation), typeof(double), typeof(ColorSlider),
            new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnHueSaturationChanged)));

        public double Saturation
        {
            get => (double)GetValue(SaturationProperty);
            set { SetValue(SaturationProperty, value); }
        }
        #endregion

        private static void OnHueSaturationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ColorSlider c)
            {
                double hue = e.Property == HueProperty ? (double)e.NewValue : c.Hue;
                double saturation = e.Property == SaturationProperty ? (double)e.NewValue : c.Saturation;

                c.UpdateCurrentColor(hue, saturation);
            }
        }

        #endregion

        #region Methods
        private void UpdateCurrentColor(double hue, double saturation)
        {
            if (!_isChanging)
            {
                _isChanging = true;

                var color = ColorExtensions.FromHsb((float)hue, (float)(saturation / 100.0f), 0.5f);
                SetValue(CurrentColorProperty, color);

                _isChanging = false;
            }
        }
        #endregion
    }
}
