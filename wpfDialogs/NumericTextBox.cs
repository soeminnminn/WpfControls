using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows;

namespace wpfDialogs
{
    public class NumericTextBox : TextBox
    {
        #region Variables

        #region ValueChanged Event
        //Due to a bug in Visual Studio, you cannot create event handlers for generic T args in XAML, so I have to use object instead.
        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<double>), typeof(NumericTextBox));

        public event RoutedPropertyChangedEventHandler<double> ValueChanged
        {
            add
            {
                AddHandler(ValueChangedEvent, value);
            }
            remove
            {
                RemoveHandler(ValueChangedEvent, value);
            }
        }
        #endregion

        /// <summary>
        /// Flags if the Text and Value properties are in the process of being sync'd
        /// </summary>
        private bool _isSyncingTextAndValueProperties;
        private bool _internalValueSet;

        #endregion

        #region Constructor
        public NumericTextBox()
        {
            DataObject.AddPastingHandler(this, OnPasting);
        }
        #endregion

        #region Properties

        #region MaxValue
        public static readonly DependencyProperty MaxValueProperty = DependencyProperty.Register("MaxValue", typeof(double), typeof(NumericTextBox),
            new UIPropertyMetadata(100D, OnMaxValueChanged, OnCoerceMaxValue));
        public double MaxValue
        {
            get
            {
                return (double)GetValue(MaxValueProperty);
            }
            set
            {
                SetValue(MaxValueProperty, value);
            }
        }

        private static void OnMaxValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            NumericTextBox control = o as NumericTextBox;
            if (control != null)
                control.OnMaxValueChanged((double)e.OldValue, (double)e.NewValue);
        }

        protected virtual void OnMaxValueChanged(double oldValue, double newValue)
        {
            this.Value = this.CoerceValueMinMax(this.Value);
        }

        private static object OnCoerceMaxValue(DependencyObject d, object baseValue)
        {
            NumericTextBox control = d as NumericTextBox;
            if (control != null)
                return control.OnCoerceMaxValue((double)baseValue);

            return baseValue;
        }

        protected virtual double OnCoerceMaxValue(double baseValue)
        {
            return baseValue;
        }
        #endregion //MaxValue

        #region MinValue
        public static readonly DependencyProperty MinValueProperty = DependencyProperty.Register("MinValue", typeof(double), typeof(NumericTextBox),
            new UIPropertyMetadata(0D, OnMinValueChanged, OnCoerceMinValue));
        public double MinValue
        {
            get
            {
                return (double)GetValue(MinValueProperty);
            }
            set
            {
                SetValue(MinValueProperty, value);
            }
        }

        private static void OnMinValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            NumericTextBox control = o as NumericTextBox;
            if (control != null)
                control.OnMinValueChanged((double)e.OldValue, (double)e.NewValue);
        }

        protected virtual void OnMinValueChanged(double oldValue, double newValue)
        {
            this.Value = this.CoerceValueMinMax(this.Value);
        }

        private static object OnCoerceMinValue(DependencyObject d, object baseValue)
        {
            NumericTextBox upDown = d as NumericTextBox;
            if (upDown != null)
                return upDown.OnCoerceMinValue((double)baseValue);

            return baseValue;
        }

        protected virtual double OnCoerceMinValue(double baseValue)
        {
            return baseValue;
        }
        #endregion //MinValue

        #region Increment
        public static readonly DependencyProperty IncrementProperty = DependencyProperty.Register("Increment", typeof(double), typeof(NumericTextBox),
            new PropertyMetadata(1D, OnIncrementChanged, OnCoerceIncrement));
        public double Increment
        {
            get
            {
                return (double)GetValue(IncrementProperty);
            }
            set
            {
                SetValue(IncrementProperty, value);
            }
        }

        private static void OnIncrementChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            NumericTextBox control = o as NumericTextBox;
            if (control != null)
                control.OnIncrementChanged((double)e.OldValue, (double)e.NewValue);
        }

        protected virtual void OnIncrementChanged(double oldValue, double newValue)
        {
        }

        private static object OnCoerceIncrement(DependencyObject d, object baseValue)
        {
            NumericTextBox control = d as NumericTextBox;
            if (control != null)
                return control.OnCoerceIncrement((double)baseValue);

            return baseValue;
        }

        protected virtual double OnCoerceIncrement(double baseValue)
        {
            return baseValue;
        }
        #endregion

        #region Format
        public static readonly DependencyProperty FormatProperty = DependencyProperty.Register("Format", typeof(string), typeof(NumericTextBox),
            new UIPropertyMetadata("N0", OnFormatChanged, OnCoerceFormat));
        public string Format
        {
            get
            {
                return (string)GetValue(FormatProperty);
            }
            set
            {
                SetValue(FormatProperty, value);
            }
        }

        private static object OnCoerceFormat(DependencyObject o, object baseValue)
        {
            NumericTextBox control = o as NumericTextBox;
            if (control != null)
                return control.OnCoerceFormat((string)baseValue);

            return baseValue;
        }

        protected virtual string OnCoerceFormat(string baseValue)
        {
            return baseValue ?? string.Empty;
        }

        private static void OnFormatChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            NumericTextBox numericUpDown = o as NumericTextBox;
            if (numericUpDown != null)
                numericUpDown.OnFormatChanged((string)e.OldValue, (string)e.NewValue);
        }

        protected virtual void OnFormatChanged(string oldValue, string newValue)
        {
            if (IsInitialized)
            {
                this.SyncTextAndValueProperties(false, null);
            }
        }
        #endregion //Format

        #region DefaultValue
        public static readonly DependencyProperty DefaultValueProperty = DependencyProperty.Register("DefaultValue", typeof(double), typeof(NumericTextBox),
            new UIPropertyMetadata(0D, OnDefaultValueChanged));
        public double DefaultValue
        {
            get
            {
                return (double)GetValue(DefaultValueProperty);
            }
            set
            {
                SetValue(DefaultValueProperty, value);
            }
        }

        private static void OnDefaultValueChanged(DependencyObject source, DependencyPropertyChangedEventArgs args)
        {
            ((NumericTextBox)source).OnDefaultValueChanged((double)args.OldValue, (double)args.NewValue);
        }

        protected virtual void OnDefaultValueChanged(double oldValue, double newValue)
        {
            if (this.IsInitialized && string.IsNullOrEmpty(Text))
            {
                this.SyncTextAndValueProperties(true, Text);
            }
        }
        #endregion //DefaultValue

        #region Value
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(double), typeof(NumericTextBox),
          new FrameworkPropertyMetadata(0D, OnValueChanged, OnCoerceValue));
        public double Value
        {
            get
            {
                return (double)GetValue(ValueProperty);
            }
            set
            {
                SetValue(ValueProperty, value);
            }
        }

        private void SetValueInternal(double value)
        {
            _internalValueSet = true;
            try
            {
                this.Value = value;
            }
            finally
            {
                _internalValueSet = false;
            }
        }

        private static object OnCoerceValue(DependencyObject o, object basevalue)
        {
            return ((NumericTextBox)o).OnCoerceValue(basevalue);
        }

        protected virtual object OnCoerceValue(object newValue)
        {
            return newValue;
        }

        private static void OnValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            NumericTextBox control = o as NumericTextBox;
            if (control != null)
                control.OnValueChanged((double)e.OldValue, (double)e.NewValue);
        }

        protected virtual void OnValueChanged(double oldValue, double newValue)
        {
            if (!_internalValueSet && this.IsInitialized)
            {
                SyncTextAndValueProperties(false, null);
            }

            this.RaiseValueChangedEvent(oldValue, newValue);
        }
        #endregion //Value

        #endregion

        #region Methods
        protected virtual void RaiseValueChangedEvent(double oldValue, double newValue)
        {
            RoutedPropertyChangedEventArgs<double> args = new RoutedPropertyChangedEventArgs<double>(oldValue, newValue);
            args.RoutedEvent = ValueChangedEvent;
            RaiseEvent(args);
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            // When both Value and Text are initialized, Value has priority.
            // To be sure that the value is not initialized, it should
            // have no local value, no binding, and equal to the default value.
            bool updateValueFromText =
              (this.ReadLocalValue(ValueProperty) == DependencyProperty.UnsetValue)
              && (BindingOperations.GetBinding(this, ValueProperty) == null)
              && (object.Equals(this.Value, ValueProperty.DefaultMetadata.DefaultValue));

            this.SyncTextAndValueProperties(updateValueFromText, Text);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            if (!e.Handled && IsFocused)
            {
                if (e.Delta < 0)
                {
                    OnDecrement();
                    e.Handled = true;
                }
                else if (e.Delta > 0)
                {
                    OnIncrement();
                    e.Handled = true;
                }
            }
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);
            if (this.IsInitialized)
                this.SyncTextAndValueProperties(true, Text);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            if (e.Key == Key.Up)
            {
                OnIncrement();
                e.Handled = true;
            }
            else if (e.Key == Key.Down)
            {
                OnDecrement();
                e.Handled = true;
            }
        }

        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            string newText = Text;
            if (SelectionLength > 0)
                newText = Text.Remove(SelectionStart, SelectionLength);

            newText = newText.Insert(SelectionStart, e.Text);
            if (!string.IsNullOrEmpty(newText))
            {
                if (!TryParse(newText, out _))
                {
                    e.Handled = true;
                }
            }

            base.OnPreviewTextInput(e);
        }

        protected virtual void OnPasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (!SyncTextAndValueProperties(false, text))
                {
                    e.CancelCommand();
                }
            }
        }

        private double CoerceValueMinMax(double value)
        {
            if (IsLowerThan(value, MinValue))
                return MinValue;
            else if (IsGreaterThan(value, MaxValue))
                return MaxValue;
            else
                return value;
        }

        private bool IsLowerThan(double? value1, double? value2)
        {
            if (value1 == null || value2 == null)
                return false;

            return value1.Value < value2.Value;
        }

        private bool IsGreaterThan(double? value1, double? value2)
        {
            if (value1 == null || value2 == null)
                return false;

            return value1.Value > value2.Value;
        }

        protected virtual void OnDecrement()
        {
            var result = Value - Increment;
            this.Value = this.CoerceValueMinMax(result);
        }

        protected virtual void OnIncrement()
        {
            var result = Value + Increment;
            this.Value = this.CoerceValueMinMax(result);
        }

        private bool TryParse(string text, out double value)
        {
            if (!string.IsNullOrEmpty(text))
            {
                double result;
                if (double.TryParse(text, out result))
                {
                    value = CoerceValueMinMax(result);
                    return true;
                }
            }
            value = DefaultValue;
            return false;
        }

        private string ToStringValue()
        {
            //Manage Format of type "{}{0:N2} °" (in xaml) or "{0:N2} °" in code-behind.
            if (Format.Contains("{0"))
                return string.Format(CultureInfo.InvariantCulture, Format, Value);

            return Value.ToString(Format, CultureInfo.InvariantCulture);
        }

        protected bool SyncTextAndValueProperties(bool updateValueFromText, string text)
        {
            if (_isSyncingTextAndValueProperties)
                return true;

            _isSyncingTextAndValueProperties = true;
            bool parsedTextIsValid = true;
            try
            {
                if (updateValueFromText)
                {
                    double newValue;
                    parsedTextIsValid = TryParse(text, out newValue);
                    if (parsedTextIsValid)
                    {
                        SetValueInternal(newValue);
                    }
                }

                if (parsedTextIsValid)
                {
                    string newText = ToStringValue();
                    if (Text != newText)
                    {
                        int caretIdx = CaretIndex;
                        if (Text.Length == caretIdx)
                            caretIdx = newText.Length;

                        Text = newText;

                        if (caretIdx >= newText.Length)
                            caretIdx = newText.Length;

                        CaretIndex = caretIdx;
                    }
                }
            }
            finally
            {
                _isSyncingTextAndValueProperties = false;
            }
            return parsedTextIsValid;
        }
        #endregion
    }
}
