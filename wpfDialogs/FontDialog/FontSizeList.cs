using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace wpfDialogs
{
    public class FontSizeList : ListBox
    {
        #region Variables
        private const string PART_EditableTextBox = "PART_EditableTextBox";

        private static readonly double[] CommonlyUsedFontSizes =
        {
            3.0, 4.0, 5.0, 6.0, 6.5,
            7.0, 7.5, 8.0, 8.5, 9.0,
            9.5, 10.0, 10.5, 11.0, 11.5,
            12.0, 12.5, 13.0, 13.5, 14.0,
            15.0, 16.0, 17.0, 18.0, 19.0,
            20.0, 22.0, 24.0, 26.0, 28.0, 30.0, 32.0, 34.0, 36.0, 38.0,
            40.0, 44.0, 48.0, 52.0, 56.0, 60.0, 64.0, 68.0, 72.0, 76.0,
            80.0, 88.0, 96.0, 104.0, 112.0, 120.0, 128.0, 136.0, 144.0
        };

        private TextBox _textBox = null;
        #endregion

        #region Constructors
        static FontSizeList()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FontSizeList), new FrameworkPropertyMetadata(typeof(FontSizeList)));
        }

        public FontSizeList()
            : base()
        {
            ItemsSource = CommonlyUsedFontSizes;
        }
        #endregion

        #region Dependency Properties

        #region SelectedFontSizeProperty
        public static readonly DependencyProperty SelectedFontSizeProperty = DependencyProperty.Register(
            nameof(SelectedFontSize), typeof(double), typeof(FontSizeList),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(OnSelectedFontSizeChanged)));

        public double SelectedFontSize
        {
            get => (double)GetValue(SelectedFontSizeProperty);
            set { SetValue(SelectedFontSizeProperty, value); }
        }

        private static void OnSelectedFontSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FontSizeList c)
            {
                c.OnSelectedFontSizeChanged((double)e.NewValue);
            }
        }
        #endregion

        #endregion

        #region Methods
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _textBox = GetTemplateChild(PART_EditableTextBox) as TextBox;
            if (_textBox != null)
            {
                _textBox.TextChanged += TextBox_TextChanged;
                _textBox.PreviewKeyDown += TextBox_PreviewKeyDown;
                _textBox.PreviewTextInput += TextBox_PreviewTextInput;
            }
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new FontSizeListItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is FontSizeListItem;
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            var container = element as FontSizeListItem;
            if (container == null)
            {
                throw new InvalidOperationException("Container not created.");
            }

            var obj = (double)item;
            container.Content = Math.Round(obj, 2).ToString();
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);

            if (SelectedItem is double value && FontExtensions.PointsToPixels(value) != SelectedFontSize)
            {
                var sizeInPixels = FontExtensions.PointsToPixels(value);
                SetValue(SelectedFontSizeProperty, sizeInPixels);
            }
        }

        private void OnSelectedFontSizeChanged(double sizeInPixels)
        {
            var sizeInPoints = FontExtensions.PixelsToPoints(sizeInPixels);
            if (!TrySelectItem(sizeInPoints))
            {
                SelectedIndex = -1;
            }

            if (_textBox != null)
            {
                double textBoxValue;
                if (!double.TryParse(_textBox.Text, out textBoxValue) || !FontExtensions.FuzzyEqual(textBoxValue, sizeInPoints))
                {
                    _textBox.Text = sizeInPoints.ToString("0.##",CultureInfo.InvariantCulture);
                }
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                double sizeInPoints;
                if (double.TryParse(textBox.Text, out sizeInPoints))
                {
                    var sizeInPixels = FontExtensions.PointsToPixels(sizeInPoints);
                    if (!FontExtensions.FuzzyEqual(sizeInPixels, SelectedFontSize))
                    {
                        SelectedFontSize = sizeInPixels;
                    }
                }
            }
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    // Move up from the current position.
                    MoveListPosition(-1);
                    e.Handled = true;
                    break;

                case Key.Down:
                    // Move down from the current position, unless the item at the current position is
                    // not already selected in which case select it.
                    if (Items.CurrentPosition == SelectedIndex)
                    {
                        MoveListPosition(+1);
                    }
                    else
                    {
                        MoveListPosition(0);
                    }
                    e.Handled = true;
                    break;
            }
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                string newText = textBox.Text;
                if (textBox.SelectionLength > 0)
                    newText = textBox.Text.Remove(textBox.SelectionStart, textBox.SelectionLength);

                newText = newText.Insert(textBox.SelectionStart, e.Text);
                if (!string.IsNullOrEmpty(newText))
                {
                    if (!double.TryParse(newText, out _))
                    {
                        e.Handled = true;
                    }
                }
            }
        }

        private void MoveListPosition(int distance)
        {
            var i = Items.CurrentPosition + distance;
            if (i >= 0 && i < Items.Count)
            {
                Items.MoveCurrentToPosition(i);
                SelectedIndex = i;
                ScrollIntoView(Items[i]);
            }
        }

        private bool TrySelectItem(double value)
        {
            var itemList = Items;

            // Perform a binary search for the item.
            var first = 0;
            var limit = itemList.Count;

            while (first < limit)
            {
                var i = first + (limit - first) / 2;
                var item = itemList[i];

                var sizeInPoints = (double)item;
                var comparison = FontExtensions.FuzzyEqual(sizeInPoints, value) ? 0 : (sizeInPoints < value) ? -1 : 1;
                if (comparison < 0)
                {
                    // Value must be after i
                    first = i + 1;
                }
                else if (comparison > 0)
                {
                    // Value must be before i
                    limit = i;
                }
                else
                {
                    // Exact match; select the item.
                    SelectedIndex = i;
                    itemList.MoveCurrentToPosition(i);
                    ScrollIntoView(item);
                    return true;
                }
            }

            // Not an exact match; move current position to the nearest item but don't select it.
            if (itemList.Count > 0)
            {
                var i = Math.Min(first, itemList.Count - 1);
                itemList.MoveCurrentToPosition(i);
                ScrollIntoView(itemList[i]);
            }

            return false;
        }
        #endregion
    }

    public class FontSizeListItem : ListBoxItem
    {
        #region Constructors
        static FontSizeListItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FontSizeListItem), new FrameworkPropertyMetadata(typeof(FontSizeListItem)));
        }

        public FontSizeListItem()
            : base()
        { }
        #endregion
    }
}
