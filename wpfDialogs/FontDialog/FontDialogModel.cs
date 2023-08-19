using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace wpfDialogs
{
    internal class FontDialogModel : INotifyPropertyChanged
    {
        #region Variables
        private static readonly RoutedCommand acceptCommand = new RoutedCommand("Accept", typeof(FontDialogWindow));

        private static readonly CommandBinding acceptCommandBinding = new CommandBinding(acceptCommand,
                new ExecutedRoutedEventHandler(OnAcceptExecuted));

        private readonly FontDialogWindow _owner;
        
        private readonly ObservableCollection<FontCharset> _charsets = new ObservableCollection<FontCharset>();

        private object _fontFamilyItem;
        private object _typefaceItem;
        private object _fontSizeItem;
        private object _fontColorItem;
        private object _fontCharsetItem;

        private FontFamily _fontFamily;
        private FontStyle _fontStyle;
        private FontWeight _fontWeight;
        private FontStretch _fontStretch;
        private Color _fontColor;
        private double _fontSize;

        private bool _chooseColor = true;
        private bool _strikeout = false;
        private bool _underline = false;
        private TextDecorationCollection _textDecorations;

        private string _sampleText = "AaBbYyZz";

        private bool _isChanging = false;
        #endregion

        #region Events
        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add { _propertyChanged += value; }
            remove { _propertyChanged -= value; }
        }

        private event PropertyChangedEventHandler _propertyChanged;
        #endregion

        #region Constructor
        internal FontDialogModel()
        {
            _fontFamily = TextBlock.FontFamilyProperty.DefaultMetadata.DefaultValue as FontFamily;
            _fontStyle = (FontStyle)TextBlock.FontStyleProperty.DefaultMetadata.DefaultValue;
            _fontWeight = (FontWeight)TextBlock.FontWeightProperty.DefaultMetadata.DefaultValue;
            _fontStretch = (FontStretch)TextBlock.FontStretchProperty.DefaultMetadata.DefaultValue;
            _fontSize = (double)TextBlock.FontSizeProperty.DefaultMetadata.DefaultValue;
            _fontColor = SystemColors.ControlTextColor;

            _fontFamilyItem = _fontFamily;
            _typefaceItem = new Typeface(_fontFamily, _fontStyle, _fontWeight, _fontStretch);
            _fontSizeItem = _fontSize;
            _fontColorItem = FontColors.Black;

            UpdateCharsets();
        }

        public FontDialogModel(FontDialogWindow owner)
            : this()
        {
            _owner = owner;

            if (!_owner.CommandBindings.Contains(acceptCommandBinding))
            {
                _owner.CommandBindings.Add(acceptCommandBinding);
            }
        }
        #endregion

        #region Properties
        public static FontColor[] AvailableColors => FontColors.AvailableColors;

        public ObservableCollection<FontCharset> Charsets
        {
            get => _charsets;
        }

        public object FontFamilyItem
        {
            get => _fontFamilyItem;
            set { SetProperty(ref _fontFamilyItem, value); }
        }

        public object TypefaceItem
        {
            get => _typefaceItem;
            set { SetProperty(ref _typefaceItem, value); }
        }

        public object FontSizeItem
        {
            get => _fontSizeItem;
            set { SetProperty(ref _fontSizeItem, value); }
        }

        public object FontColorItem
        {
            get => _fontColorItem;
            set { SetProperty(ref _fontColorItem, value); }
        }

        public object FontCharsetItem
        {
            get => _fontCharsetItem;
            set { SetProperty(ref _fontCharsetItem, value); }
        }

        public FontFamily FontFamily
        {
            get => _fontFamily;
            set { SetProperty(ref _fontFamily, value); }
        }

        public FontStyle FontStyle
        {
            get => _fontStyle;
            set { SetProperty(ref _fontStyle, value); }
        }

        public FontWeight FontWeight
        {
            get => _fontWeight;
            set { SetProperty(ref _fontWeight, value); }
        }

        public FontStretch FontStretch
        {
            get => _fontStretch;
            set { SetProperty(ref _fontStretch, value); }
        }

        public Color FontColor
        {
            get => _fontColor;
            set { SetProperty(ref _fontColor, value); }
        }

        public double FontSize
        {
            get => _fontSize;
            set { SetProperty(ref _fontSize, value); }
        }

        public bool ChooseColor
        {
            get => _chooseColor;
            set { SetProperty(ref _chooseColor, value); }
        }

        public bool Strikeout
        {
            get => _strikeout;
            set { SetProperty(ref _strikeout, value); }
        }

        public bool Underline
        {
            get => _underline;
            set { SetProperty(ref _underline, value); }
        }

        public TextDecorationCollection TextDecorations
        {
            get => _textDecorations;
            set { SetProperty(ref _textDecorations, value); }
        }

        public string SampleText
        {
            get => _sampleText;
            set { SetProperty(ref _sampleText, value); }
        }

        public ICommand AcceptCommand
        {
            get => acceptCommand;
        }
        #endregion

        #region Methods
        private static void OnAcceptExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (sender is FontDialogWindow win)
            {
                win.OnAcceptExecuted(e);
            }
        }

        private void SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return;
            backingStore = value;
            RaisePropertyChanged(propertyName);
        }

        private void RaisePropertyChanged(string propertyName)
        {
            var args = new PropertyChangedEventArgs(propertyName);
            if (_propertyChanged != null)
            {
                _propertyChanged(this, args);
            }
            OnPropertyChanged(args);
        }

        private void OnPropertyChanged(PropertyChangedEventArgs e)
        { 
            if (e.PropertyName == nameof(FontFamilyItem) || e.PropertyName == nameof(TypefaceItem) || e.PropertyName == nameof(FontSizeItem) || e.PropertyName == nameof(FontColorItem))
            {
                SelectedItemChanged();
            }
            else if (e.PropertyName == nameof(Strikeout) || e.PropertyName == nameof(Underline))
            {
                TextDecorationCheckStateChanged();
            }
            else if (e.PropertyName == nameof(FontColorItem))
            {
                FontColorChanged();
            }
            else if (e.PropertyName == nameof(TextDecorations))
            {
                TextDecorationChanged();
            }
        }

        private void SelectedItemChanged()
        {
            if (!_isChanging)
            {
                _isChanging = true;

                if (_typefaceItem is Typeface typeface)
                {
                    FontFamily = typeface.FontFamily;
                    FontStyle = typeface.Style;
                    FontWeight = typeface.Weight;
                    FontStretch = typeface.Stretch;
                }
                else if (_typefaceItem is TypefaceWrapper typefaceWrap)
                {
                    FontFamily = typefaceWrap.FontFamily;
                    FontStyle = typefaceWrap.Style;
                    FontWeight = typefaceWrap.Weight;
                    FontStretch = typefaceWrap.Stretch;
                }

                if (_fontSizeItem is double fontSize)
                {
                    FontSize = fontSize;
                }

                if (_fontColorItem is Color color)
                {
                    FontColor = color;
                }
                else if (_fontColorItem is FontColor fontColor)
                {
                    FontColor = fontColor.Color;
                }

                UpdateCharsets();

                _isChanging = false;
            }   
        }

        private void TextDecorationChanged()
        {
            if (!_isChanging)
            {
                _isChanging = true;

                foreach (var decoration in _textDecorations)
                {
                    if (System.Windows.TextDecorations.Underline.Contains(decoration))
                    {
                        Underline = true;
                    }
                    else if (System.Windows.TextDecorations.Strikethrough.Contains(decoration))
                    {
                        Strikeout = true;
                    }
                }

                _isChanging = false;
            }
        }

        private void TextDecorationCheckStateChanged()
        {
            if (!_isChanging)
            {
                _isChanging = true;

                var textDecorations = new TextDecorationCollection();
                if (Underline)
                {
                    textDecorations.Add(System.Windows.TextDecorations.Underline[0]);
                }
                else if (Strikeout)
                {
                    textDecorations.Add(System.Windows.TextDecorations.Strikethrough[0]);
                }
                textDecorations.Freeze();
                TextDecorations = textDecorations;

                _isChanging = false;
            }
        }

        private void FontColorChanged()
        {
            if (!_isChanging)
            {
                _isChanging = true;

                var idx = FontColors.FindIndex(_fontColor);
                if (idx > -1)
                {
                    _fontColorItem = FontColors.AvailableColors[idx];
                }

                _isChanging = false;
            }
        }

        private void UpdateCharsets()
        {
            _charsets.Clear();
            if (_fontFamily != null)
            {
                if (FontExtensions.IsSymbolFont(_fontFamily))
                {
                    _charsets.Add(FontCharsets.SYMBOL_CHARSET);
                }
                else
                {
                    _charsets.Add(FontCharsets.ANSI_CHARSET);
                }
            }

            if (_charsets.Count > 0)
            {
                FontCharsetItem = _charsets[0];
            }
        }
        #endregion
    }
}
