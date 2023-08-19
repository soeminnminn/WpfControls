using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace wpfDialogs
{
    [TemplatePart(Name = PART_EditableTextBox, Type = typeof(TextBox))]
    public class FontFamilyList : ListBox
    {
        #region Variables
        private const string PART_EditableTextBox = "PART_EditableTextBox";

        private readonly ObservableCollection<FontFamilyWrapper> itemsSource = new ObservableCollection<FontFamilyWrapper>();

        private TextBox _textBox = null;
        private int _textBoxSelectionStart = 0;
        private bool _isListValid = false;
        private bool _updatePending = false;
        #endregion

        #region Constructors
        static FontFamilyList()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FontFamilyList), new FrameworkPropertyMetadata(typeof(FontFamilyList)));
        }

        public FontFamilyList()
            : base()
        {
            ItemsSource = itemsSource;
        }
        #endregion

        #region Dependency Properties

        #region SelectedFontFamilyProperty
        public static readonly DependencyProperty SelectedFontFamilyProperty = DependencyProperty.Register(
            nameof(SelectedFontFamily), typeof(FontFamily), typeof(FontFamilyList),
            new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnSelectedFontFamilyChanged)));

        public FontFamily SelectedFontFamily
        {
            get => (FontFamily)GetValue(SelectedFontFamilyProperty);
            set { SetValue(SelectedFontFamilyProperty, value); }
        }

        private static void OnSelectedFontFamilyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FontFamilyList c)
            {
                c.OnSelectedFontFamilyChanged(e.NewValue as FontFamily);
            }
        }
        #endregion

        #endregion

        #region Methods
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            if (!_isListValid)
            {
                UpdateItemsSource();
                _isListValid = true;
                OnSelectedFontFamilyChanged(SelectedFontFamily);
            }
            ScheduleUpdate();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _textBox = GetTemplateChild(PART_EditableTextBox) as TextBox;
            if (_textBox != null)
            {
                _textBox.SelectionChanged += TextBox_SelectionChanged;
                _textBox.TextChanged += TextBox_TextChanged;
                _textBox.PreviewKeyDown += TextBox_PreviewKeyDown;
            }
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new FontFamilyListItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is FontFamilyListItem;
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            var container = element as FontFamilyListItem;
            if (container == null)
            {
                throw new InvalidOperationException("Container not created.");
            }

            var obj = item as FontFamilyWrapper;
            if (obj == null)
            {
                throw new InvalidOperationException("Item not specified.");
            }

            container.FontFamily = obj.FontFamily;
            container.Content = obj.ToString();
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);

            if (SelectedItem is FontFamilyWrapper wrapper && !wrapper.Equals(SelectedFontFamily))
            {
                SetValue(SelectedFontFamilyProperty, wrapper.FontFamily);
            }
            else if (SelectedItem is FontFamily family && !family.Equals(SelectedFontFamily))
            {
                SetValue(SelectedFontFamilyProperty, family);
            }
        }

        private void OnSelectedFontFamilyChanged(FontFamily family)
        {
            if (_isListValid && family != null)
            {
                var displayName = FontExtensions.GetDisplayName(family.FamilyNames);

                bool isSelected = false;
                var item = SelectedItem as FontFamilyWrapper;
                if (item != null && item.Equals(family))
                {
                    isSelected = true;
                }
                else if (TrySelectItem(displayName))
                {
                    isSelected = true;
                }

                if (isSelected && _textBox != null && string.Compare(_textBox.Text, displayName, true, CultureInfo.CurrentCulture) != 0)
                {
                    _textBox.Text = displayName;
                }
            }
        }

        private void TextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            _textBoxSelectionStart = _textBox.SelectionStart;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                var text = textBox.Text;
                if (SelectListItem(text) == null)
                {
                    if (text.Length > _textBoxSelectionStart && textBox.SelectionStart == text.Length)
                    {
                        var item = Items.CurrentItem as FontFamilyWrapper;
                        if (item != null)
                        {
                            // Does the text box text match the beginning of the family name?
                            var name = item.DisplayName;
                            if (string.Compare(text, 0, name, 0, text.Length, true, CultureInfo.CurrentCulture) == 0)
                            {
                                // Set the text box text to the complete family name and select the part not typed in.
                                textBox.Text = name;
                                textBox.SelectionStart = text.Length;
                                textBox.SelectionLength = name.Length - text.Length;
                            }
                        }
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

        private FontFamilyWrapper SelectListItem(string displayName)
        {
            var item = SelectedItem as FontFamilyWrapper;
            if (item != null && string.Compare(item.DisplayName, displayName, true, CultureInfo.CurrentCulture) == 0)
            {
                return item;
            }

            if (TrySelectItem(displayName))
            {
                return SelectedItem as FontFamilyWrapper;
            }

            return null;
        }

        private void UpdateItemsSource()
        {
            var fontFamilies = Fonts.SystemFontFamilies.Select(x => new FontFamilyWrapper(x)).ToList();
            fontFamilies.Sort();

            itemsSource.Clear();
            foreach (var item in fontFamilies)
            {
                itemsSource.Add(item);
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

        private bool TrySelectItem(string source)
        {
            var itemList = Items;

            // Perform a binary search for the item.
            var first = 0;
            var limit = itemList.Count;

            while (first < limit)
            {
                var i = first + (limit - first) / 2;
                var item = itemList[i];
                var comparison = ((IComparable)item).CompareTo(source);
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

        private void ScheduleUpdate()
        {
            if (!_updatePending)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(OnUpdate));
                _updatePending = true;
            }
        }

        private void OnUpdate()
        {
            _updatePending = false;
            if (!_isListValid)
            {
                UpdateItemsSource();
                _isListValid = true;
                OnSelectedFontFamilyChanged(SelectedFontFamily);
                ScheduleUpdate();
            }
            else if (SelectedItem == null && SelectedFontFamily != null)
            {
                OnSelectedFontFamilyChanged(SelectedFontFamily);
            }
        }
        #endregion
    }

    public class FontFamilyListItem : ListBoxItem
    {
        #region Constructors
        static FontFamilyListItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FontFamilyListItem), new FrameworkPropertyMetadata(typeof(FontFamilyListItem)));
        }

        public FontFamilyListItem()
            : base()
        { }
        #endregion
    }
}
