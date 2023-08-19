using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace wpfDialogs
{
    [TemplatePart(Name = Part_Thumb, Type = typeof(Thumb))]
    [TemplatePart(Name = Part_Palette, Type = typeof(Image))]
    public class PaletteSlider : Control
    {
        #region Variables
        private const double MinSize = 256.0;
        private const string Part_Thumb = "PART_Thumb";
        private const string Part_Palette = "PART_Palette";

        private readonly WriteableBitmap bitmap;
        private readonly TranslateTransform thumbTransform;
        private Thumb thumb = null;

        private bool _isChanging = false;
        #endregion

        #region Constructors
        static PaletteSlider()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PaletteSlider), new FrameworkPropertyMetadata(typeof(PaletteSlider)));

            FocusableProperty.OverrideMetadata(typeof(PaletteSlider), new FrameworkPropertyMetadata(true));

            EventManager.RegisterClassHandler(typeof(PaletteSlider), KeyDownEvent, new KeyEventHandler(OnAnyKeyDown));
            EventManager.RegisterClassHandler(typeof(PaletteSlider), Thumb.DragDeltaEvent, new DragDeltaEventHandler(OnThumbDragDelta));
            EventManager.RegisterClassHandler(typeof(PaletteSlider), Thumb.DragCompletedEvent, new DragCompletedEventHandler(OnThumbDragCompleted));
        }

        public PaletteSlider()
            : base()
        {
            MinHeight = MinSize;
            MinWidth = MinSize;
            
            Focusable = true;

            thumbTransform = new TranslateTransform();
            bitmap = new WriteableBitmap(256, 256, 96, 96, PixelFormats.Bgr24, null);
        }
        #endregion

        #region Dependency Properties

        #region HueProperty
        public static readonly DependencyProperty HueProperty = DependencyProperty.Register(
            nameof(Hue), typeof(double), typeof(PaletteSlider), 
            new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnHueSaturationChanged)));

        public double Hue
        {
            get => (double)GetValue(HueProperty);
            set { SetValue(HueProperty, value); }
        }
        #endregion

        #region SaturationProperty
        public static readonly DependencyProperty SaturationProperty = DependencyProperty.Register(
            nameof(Saturation), typeof(double), typeof(PaletteSlider), 
            new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnHueSaturationChanged)));

        public double Saturation
        {
            get => (double)GetValue(SaturationProperty);
            set { SetValue(SaturationProperty, value); }
        }
        #endregion

        private static void OnHueSaturationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PaletteSlider c)
            {
                double hue = e.Property == HueProperty ? (double)e.NewValue : c.Hue;
                double saturation = e.Property == SaturationProperty ? (double)e.NewValue : c.Saturation;

                c.UpdateValuesPosition(hue, saturation);
            }
        }
        #endregion

        #region Methods
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (GetTemplateChild(Part_Palette) is Image image)
            {
                CreateHslColorPalette();
                image.Source = bitmap;
            }

            thumb = GetTemplateChild(Part_Thumb) as Thumb;
            if (thumb != null)
            {
                thumb.RenderTransform = thumbTransform;
                UpdateValuesPosition(Hue, Saturation);
            }
        }

        protected override Size MeasureOverride(Size constraint)
        {
            double height = constraint.Height;
            double width = constraint.Width;
            double size = MinSize;

            bool validHeight = !double.IsInfinity(height) && !double.IsNaN(height);
            bool validWidth = !double.IsInfinity(width) && !double.IsNaN(width);

            if (!validHeight && validWidth)
                size = Math.Max(width, size);
            else if (validHeight && !validWidth)
                size = Math.Max(height, size);
            else
                size = Math.Max(Math.Max(width, height), size);

            return new Size(size, size);
        }

        private static void OnThumbDragDelta(object sender, DragDeltaEventArgs e)
        {
            if (sender is PaletteSlider c)
                c.OnThumbDragDelta(e);
        }

        private void OnThumbDragDelta(DragDeltaEventArgs e)
        {
            double offsetX = thumbTransform.X + e.HorizontalChange;
            double offsetY = thumbTransform.Y + e.VerticalChange;

            UpdateThumbPosition(offsetX, offsetY);
        }

        private static void OnThumbDragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (sender is PaletteSlider c)
                c.OnThumbDragCompleted(e);
        }

        private void OnThumbDragCompleted(DragCompletedEventArgs e)
        {
            if (DataContext is IEditableObject editableObject)
            {
                editableObject.EndEdit();
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (DataContext is IEditableObject editableObject)
            {
                editableObject.BeginEdit();
            }

            Focus();

            if (thumb != null)
            {
                Point position = e.GetPosition(this);
                UpdateThumbPosition(position.X, position.Y);
                thumb.RaiseEvent(e);
            }

            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            UpdateValuesPosition(Hue, Saturation);
        }

        private static void OnAnyKeyDown(object sender, KeyEventArgs e)
        {
            if (sender is PaletteSlider c)
            {
                switch (e.Key)
                {
                    case Key.Left:
                    case Key.Right:
                    case Key.Up:
                    case Key.Down:
                        c.OnArrowKeyDown(e);
                        break;
                }
            }
        }

        private void OnArrowKeyDown(KeyEventArgs e)
        {
            e.Handled = true;

            double xDelta = (ActualWidth / 256.0);
            double yDelta = (ActualHeight / 256.0);

            double offsetX = thumbTransform.X;
            double offsetY = thumbTransform.Y;

            switch (e.Key)
            {
                case Key.Left:
                    offsetX -= xDelta;
                    break;
                case Key.Right:
                    offsetX += xDelta;
                    break;
                case Key.Up:
                    offsetY -= yDelta;
                    break;
                case Key.Down:
                    offsetY += yDelta;
                    break;
            }

            UpdateThumbPosition(offsetX, offsetY);
        }

        private void CreateHslColorPalette()
        {
            bitmap.Lock();

            var rect = new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight);
            var pixels = new byte[bitmap.PixelWidth * bitmap.PixelHeight * (bitmap.Format.BitsPerPixel / 8)];

            float ru = 1.0f / 256.0f;
            float cu = 360.0f / 256.0f;
            float lightness = 0.5f;

            for (int y = 0; y < bitmap.PixelHeight; y++)
            {
                for (int x = 0; x < bitmap.PixelWidth; x++)
                {
                    float hue = 360.0f - (cu * (256.0f - x));
                    float saturation = ru * (256.0f - y);

                    var color = ColorExtensions.FromHsb(hue, saturation, lightness);
                    int i = (x + (bitmap.PixelWidth * y)) * (bitmap.Format.BitsPerPixel / 8);
                    pixels[i] = color.B;
                    pixels[i + 1] = color.G;
                    pixels[i + 2] = color.R;
                }
            }

            int stride = bitmap.PixelWidth * (bitmap.Format.BitsPerPixel / 8);
            bitmap.WritePixels(rect, pixels, stride, 0);

            bitmap.Unlock();
            bitmap.Freeze();
        }

        private void UpdateThumbPosition(double x, double y)
        {
            if (!_isChanging)
            {
                _isChanging = true;

                double posX = Math.Min(Math.Max(0, x), ActualWidth);
                double posY = Math.Min(Math.Max(0, y), ActualHeight);

                thumbTransform.X = posX;
                thumbTransform.Y = posY;

                double ax = (posX / ActualWidth) * 256.0;
                double ay = (posY / ActualHeight) * 256.0;

                double ru = 100.0 / 256.0;
                double cu = 360.0 / 256.0;

                double hue = 360.0 - ((256.0 - ax) * cu);
                double saturation = (256.0 - ay) * ru;

                SetValue(HueProperty, hue);
                SetValue(SaturationProperty, saturation);

                _isChanging = false;
            }   
        }

        private void UpdateValuesPosition(double hue, double saturation)
        {
            if (!_isChanging)
            {
                _isChanging = true;

                double x = (hue / 360.0) * 255.0;
                double y = 255.0 - ((saturation / 100.0) * 255.0);

                thumbTransform.X = (ActualWidth / 256.0) * x;
                thumbTransform.Y = (ActualHeight / 256.0) * y;

                _isChanging = false;
            }
        }
        #endregion
    }
}
