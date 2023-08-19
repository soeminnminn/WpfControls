using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace ColorPicker
{
    /// <summary>
    /// The hsv control.
    /// </summary>
    /// <remarks>Original code by Ury Jamshy, 21 July 2011.
    /// See http://www.codeproject.com/KB/WPF/ColorPicker010.aspx
    /// The Code Project Open License (CPOL)
    /// http://www.codeproject.com/info/cpol10.aspx</remarks>
    [TemplatePart(Name = PartThumb, Type = typeof(Thumb))]
    internal class HsvControl : Control
    {
        #region Dependency Properties
        /// <summary>
        /// Identifies the <see cref="Hue"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HueProperty = DependencyProperty.Register(
            nameof(Hue), typeof(double), typeof(HsvControl),
            new FrameworkPropertyMetadata((double)0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnHueChanged));

        /// <summary>
        /// Identifies the <see cref="Saturation"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SaturationProperty = DependencyProperty.Register(
            nameof(Saturation), typeof(double), typeof(HsvControl),
            new FrameworkPropertyMetadata((double)0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSaturationChanged));

        /// <summary>
        /// Identifies the <see cref="SelectedColor"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectedColorProperty = DependencyProperty.Register(
            nameof(SelectedColor), typeof(Color?), typeof(HsvControl),
            new FrameworkPropertyMetadata(Colors.Transparent, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// Identifies the <see cref="Value"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value), typeof(double), typeof(HsvControl),
            new FrameworkPropertyMetadata((double)0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));
        #endregion

        #region Variables
        /// <summary>
        /// The thumb name.
        /// </summary>
        private const string PartThumb = "PART_Thumb";

        /// <summary>
        /// The thumb transform.
        /// </summary>
        private readonly TranslateTransform thumbTransform = new TranslateTransform();

        /// <summary>
        /// The thumb.
        /// </summary>
        private Thumb thumb;

#pragma warning disable 649

        /// <summary>
        /// The within update flag.
        /// </summary>
        internal bool withinUpdate;
#pragma warning restore 649
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets Hue.
        /// </summary>
        public double Hue
        {
            get => (double)GetValue(HueProperty);
            set { SetValue(HueProperty, value); }
        }

        /// <summary>
        /// Gets or sets Saturation.
        /// </summary>
        public double Saturation
        {
            get => (double)GetValue(SaturationProperty);
            set { SetValue(SaturationProperty, value); }
        }

        /// <summary>
        /// Gets or sets Value.
        /// </summary>
        public double Value
        {
            get => (double)GetValue(ValueProperty);
            set { SetValue(ValueProperty, value); }
        }

        /// <summary>
        /// Gets or sets SelectedColor.
        /// </summary>
        public Color? SelectedColor
        {
            get => (Color?)GetValue(SelectedColorProperty);
            set { SetValue(SelectedColorProperty, value); }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes static members of the <see cref="HsvControl" /> class.
        /// </summary>
        static HsvControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(HsvControl), new FrameworkPropertyMetadata(typeof(HsvControl)));

            // Register Event Handler for the Thumb
            EventManager.RegisterClassHandler(
                typeof(HsvControl), Thumb.DragDeltaEvent, new DragDeltaEventHandler(OnThumbDragDelta));
            EventManager.RegisterClassHandler(
                typeof(HsvControl), Thumb.DragCompletedEvent, new DragCompletedEventHandler(OnThumbDragCompleted));
        }
        #endregion

        #region Methods
        /// <summary>
        /// The on thumb drag completed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private static void OnThumbDragCompleted(object sender, DragCompletedEventArgs e)
        {
            ((HsvControl)sender).OnThumbDragCompleted(e);
        }

        /// <summary>
        /// The on thumb drag completed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        private void OnThumbDragCompleted(DragCompletedEventArgs sender)
        {
            var editableObject = DataContext as IEditableObject;
            if (editableObject != null)
            {
                editableObject.EndEdit();
            }
        }

        /// <summary>
        /// The on apply template.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            thumb = GetTemplateChild(PartThumb) as Thumb;
            if (thumb != null)
            {
                UpdateThumbPosition();
                thumb.RenderTransform = thumbTransform;
            }
        }

        /// <summary>
        /// Invoked when an unhandled <see cref="E:System.Windows.UIElement.MouseLeftButtonDown" />�routed event is raised on this element. Implement this method to add class handling for this event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.Windows.Input.MouseButtonEventArgs" /> that contains the event data. The event data reports that the left mouse button was pressed.</param>
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            var editableObject = DataContext as IEditableObject;
            if (editableObject != null)
            {
                editableObject.BeginEdit();
            }

            if (thumb != null)
            {
                Point position = e.GetPosition(this);

                UpdatePositionAndSaturationAndValue(position.X, position.Y);

                // Initiate mouse event on thumb so it will start drag
                thumb.RaiseEvent(e);
            }

            base.OnMouseLeftButtonDown(e);
        }

        /// <summary>
        /// The on render size changed.
        /// </summary>
        /// <param name="sizeInfo">The size info.</param>
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            UpdateThumbPosition();

            if (sizeInfo.NewSize.Height != Height)
            {
                Height = sizeInfo.NewSize.Width;
            }

            base.OnRenderSizeChanged(sizeInfo);
        }

        /// <summary>
        /// The on hue changed.
        /// </summary>
        /// <param name="relatedObject">The related object.</param>
        /// <param name="e">The e.</param>
        private static void OnHueChanged(DependencyObject relatedObject, DependencyPropertyChangedEventArgs e)
        {
            var hsvControl = relatedObject as HsvControl;
            if (hsvControl != null && !hsvControl.withinUpdate)
            {
                hsvControl.UpdateSelectedColor();
            }
        }

        /// <summary>
        /// The on saturation changed.
        /// </summary>
        /// <param name="relatedObject">The related object.</param>
        /// <param name="e">The e.</param>
        private static void OnSaturationChanged(DependencyObject relatedObject, DependencyPropertyChangedEventArgs e)
        {
            var hsvControl = relatedObject as HsvControl;
            if (hsvControl != null && !hsvControl.withinUpdate)
            {
                hsvControl.UpdateThumbPosition();
            }
        }

        /// <summary>
        /// The on thumb drag delta.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private static void OnThumbDragDelta(object sender, DragDeltaEventArgs e)
        {
            var hsvControl = sender as HsvControl;
            if (hsvControl != null)
            {
                hsvControl.OnThumbDragDelta(e);
            }
        }

        /// <summary>
        /// The on value changed.
        /// </summary>
        /// <param name="relatedObject">The related object.</param>
        /// <param name="e">The e.</param>
        private static void OnValueChanged(DependencyObject relatedObject, DependencyPropertyChangedEventArgs e)
        {
            var hsvControl = relatedObject as HsvControl;
            if (hsvControl != null && !hsvControl.withinUpdate)
            {
                hsvControl.UpdateThumbPosition();
            }
        }

        /// <summary>
        /// Limit value to range (0 , max]
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="max">The max.</param>
        /// <returns>
        /// The limit value.
        /// </returns>
        private double LimitValue(double value, double max)
        {
            if (value < 0)
            {
                value = 0;
            }

            if (value > max)
            {
                value = max;
            }

            return value;
        }

        /// <summary>
        /// The on thumb drag delta.
        /// </summary>
        /// <param name="e">The e.</param>
        private void OnThumbDragDelta(DragDeltaEventArgs e)
        {
            double offsetX = thumbTransform.X + e.HorizontalChange;
            double offsetY = thumbTransform.Y + e.VerticalChange;

            UpdatePositionAndSaturationAndValue(offsetX, offsetY);
        }

        /// <summary>
        /// The update position and saturation and value.
        /// </summary>
        /// <param name="positionX">The position x.</param>
        /// <param name="positionY">The position y.</param>
        private void UpdatePositionAndSaturationAndValue(double positionX, double positionY)
        {
            positionX = LimitValue(positionX, ActualWidth);
            positionY = LimitValue(positionY, ActualHeight);

            thumbTransform.X = positionX;
            thumbTransform.Y = positionY;

            Saturation = 100.0 * positionX / ActualWidth;
            Value = 100.0 * (1 - positionY / ActualHeight);

            UpdateSelectedColor();
        }

        /// <summary>
        /// The update selected color.
        /// </summary>
        private void UpdateSelectedColor()
        {
            SelectedColor = ColorHelper.HsvToColor(Hue / 360.0, Saturation / 100.0, Value / 100.0);

            // ColorUtils.FireSelectedColorChangedEvent(this, SelectedColorChangedEvent, oldColor, newColor);
        }

        /// <summary>
        /// The update thumb position.
        /// </summary>
        private void UpdateThumbPosition()
        {
            thumbTransform.X = Saturation * 0.01 * ActualWidth;
            thumbTransform.Y = (100 - Value) * 0.01 * ActualHeight;

            SelectedColor = ColorHelper.HsvToColor(Hue / 360.0, Saturation / 100.0, Value / 100.0);
        }
        #endregion
    }
}
