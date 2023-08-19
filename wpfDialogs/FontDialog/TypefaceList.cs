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
    public class TypefaceList : ListBox
    {
        #region Variables
        private const string PART_EditableTextBox = "PART_EditableTextBox";

        private readonly ObservableCollection<TypefaceWrapper> itemsSource = new ObservableCollection<TypefaceWrapper>();

        private TextBox _textBox = null;
        private int _textBoxSelectionStart = 0;
        private bool _isListValid = false;
        private bool _updatePending = false;
        #endregion

        #region Constructors
        static TypefaceList()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TypefaceList), new FrameworkPropertyMetadata(typeof(TypefaceList)));
        }

        public TypefaceList()
            : base()
        {
            ItemsSource = itemsSource;
        }
        #endregion

        #region Dependency Properties

        #region SelectedFontFamilyProperty
        public static readonly DependencyProperty SelectedFontFamilyProperty = DependencyProperty.Register(
            nameof(SelectedFontFamily), typeof(object), typeof(TypefaceList), 
            new FrameworkPropertyMetadata(new PropertyChangedCallback(OnSelectedFontFamilyChanged)));

        public object SelectedFontFamily
        {
            get => GetValue(SelectedFontFamilyProperty);
            set { SetValue(SelectedFontFamilyProperty, value); }
        }

        private static void OnSelectedFontFamilyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TypefaceList c)
            {
                c.UpdateItemsSource(e.NewValue);
            }
        }
        #endregion

        #region SelectedTypefaceProperty
        public static readonly DependencyProperty SelectedTypefaceProperty = DependencyProperty.Register(
            nameof(SelectedTypeface), typeof(Typeface), typeof(TypefaceList), 
            new FrameworkPropertyMetadata(new PropertyChangedCallback(OnSelectedTypefaceChanged)));

        public Typeface SelectedTypeface
        {
            get => (Typeface)GetValue(SelectedTypefaceProperty);
            set { SetValue(SelectedTypefaceProperty, value); }
        }

        private static void OnSelectedTypefaceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TypefaceList c)
            {
                c.OnSelectedTypefaceChanged(e.NewValue as Typeface);
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
                UpdateItemsSource(SelectedFontFamily);
                _isListValid = true;
                OnSelectedTypefaceChanged(SelectedTypeface);
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

        protected override bool ShouldSerializeProperty(DependencyProperty dp)
        {
            if (dp == ItemsSourceProperty) return false;
            return base.ShouldSerializeProperty(dp);
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new TypefaceListItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is TypefaceListItem;
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            var container = element as TypefaceListItem;
            if (container == null)
            {
                throw new InvalidOperationException("Container not created.");
            }

            var obj = item as TypefaceWrapper;
            if (obj == null)
            {
                throw new InvalidOperationException("Item not specified.");
            }

            container.FontFamily = obj.FontFamily;
            container.FontStyle = obj.Style;
            container.FontWeight = obj.Weight;
            container.Content = obj.ToString();
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);

            if (SelectedItem is TypefaceWrapper wrapper && !wrapper.Equals(SelectedTypeface))
            {
                SetValue(SelectedTypefaceProperty, wrapper.Typeface);
            }
            else if (SelectedItem is Typeface typeface && !typeface.Equals(SelectedTypeface))
            {
                SetValue(SelectedTypefaceProperty, typeface);
            }
        }

        private void UpdateItemsSource(object obj)
        {
            FontFamily fontFamily = FontFamily;
            if (obj is FontFamily family)
                fontFamily = family;
            else if (obj is FontFamilyWrapper wrapper)
                fontFamily = wrapper.FontFamily;

            if (fontFamily == null) return;

            var typefaces = fontFamily.GetTypefaces().Select(x => new TypefaceWrapper(x)).ToList();
            typefaces.Sort();

            itemsSource.Clear();

            TypefaceWrapper selectedItem = null;
            for (int i = 0; i < typefaces.Count; i++)
            {
                var item = typefaces[i];
                itemsSource.Add(item);

                if (item.Weight == FontWeights.Regular)
                {
                    selectedItem = item;
                }
            }

            if (selectedItem != null)
            {
                SelectedTypeface = selectedItem.Typeface;
            }
        }

        private void OnSelectedTypefaceChanged(Typeface typeface)
        {
            if (_isListValid && typeface != null)
            {
                var displayName = FontExtensions.GetDisplayName(typeface.FaceNames);

                bool isSelected = false;
                var item = SelectedItem as TypefaceWrapper;
                if (item != null && item.Equals(typeface))
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
                        var item = Items.CurrentItem as TypefaceWrapper;
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

        private TypefaceWrapper SelectListItem(string displayName)
        {
            var item = SelectedItem as TypefaceWrapper;
            if (item != null && string.Compare(item.DisplayName, displayName, true, CultureInfo.CurrentCulture) == 0)
            {
                return item;
            }

            if (TrySelectItem(displayName))
            {
                return SelectedItem as TypefaceWrapper;
            }

            return null;
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
                UpdateItemsSource(SelectedFontFamily);
                _isListValid = true;
                OnSelectedTypefaceChanged(SelectedTypeface);

                ScheduleUpdate();
            }
            else if (SelectedItem == null && SelectedTypeface != null)
            {
                OnSelectedTypefaceChanged(SelectedTypeface);
            }
        }
        #endregion
    }

    public class TypefaceListItem : ListBoxItem
    {
        #region Constructors
        static TypefaceListItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TypefaceListItem), new FrameworkPropertyMetadata(typeof(TypefaceListItem)));
        }

        public TypefaceListItem()
            : base()
        { }
        #endregion
    }
}
