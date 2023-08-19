using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

namespace wspGridControl
{
    public class TextCellRenderer : ICellRenderer
    {
        #region Variables
        private readonly TextFormatter _formatter;

        private RendererRunProperties _runProperties;
        private RendererParagraphProperties _paragraphProperties;

        private TextTrimming _textTrimming;
        private VerticalAlignment _verticalAlignment = VerticalAlignment.Center;

        private string _stringFormat = "{0}";
        #endregion

        #region Properties
        public Brush Forground
        {
            get => _runProperties.ForegroundBrush;
            set { _runProperties.SetForegroundBrush(value); }
        }

        public Brush Background
        {
            get => _runProperties.BackgroundBrush;
            set { _runProperties.SetBackgroundBrush(value); }
        }

        public Typeface Typeface
        {
            get => _runProperties.Typeface;
            set { _runProperties.SetTypeface(value); }
        }

        public double FontSize
        {
            get => _runProperties.FontRenderingEmSize;
            set { _runProperties.SetFontSize(value); }
        }

        public TextAlignment Alignment
        {
            get => _paragraphProperties.TextAlignment;
            set { _paragraphProperties.SetTextAlignment(value); }
        }

        public TextWrapping Wrapping
        {
            get => _paragraphProperties.TextWrapping;
            set { _paragraphProperties.SetTextWrapping(value); }
        }

        public TextTrimming Trimming
        {
            get => _textTrimming;
            set { _textTrimming = value; }
        }

        public HorizontalAlignment HorizontalAlignment 
        { 
            get
            {
                switch(_paragraphProperties.TextAlignment)
                {
                    case TextAlignment.Left:
                        return HorizontalAlignment.Left;
                    case TextAlignment.Right:
                        return HorizontalAlignment.Right;
                    default:
                        return HorizontalAlignment.Center;
                }
            }
            set 
            {
                switch(value)
                {
                    case HorizontalAlignment.Left:
                        _paragraphProperties.SetTextAlignment(TextAlignment.Left);
                        break;
                    case HorizontalAlignment.Right:
                        _paragraphProperties.SetTextAlignment(TextAlignment.Right);
                        break;
                    default:
                        _paragraphProperties.SetTextAlignment(TextAlignment.Center);
                        break;
                }
            }
        }

        public VerticalAlignment VerticalAlignment 
        { 
            get => _verticalAlignment; 
            set
            {
                _verticalAlignment = value;
            }
        }

        public string StringFormat
        {
            get => _stringFormat;
            set { _stringFormat = value; }
        }
        #endregion

        #region Constructors
        public TextCellRenderer()
        {
            _formatter = TextFormatter.Create(TextFormattingMode.Display);
            _textTrimming = TextTrimming.WordEllipsis;
        }

        public TextCellRenderer(FontFamily fontFamily, FontStyle fontStyle, FontWeight fontWeight, FontStretch fontStretch,
            double fontSize, TextAlignment textAlignment, TextWrapping textWrapping)
            : this()
        {
            Typeface typeface = null;
            var typefaces = fontFamily.GetTypefaces();
            foreach (var tf in typefaces)
            {
                if (tf.Style == fontStyle && tf.Weight == fontWeight && tf.Stretch == fontStretch)
                {
                    typeface = tf;
                    break;
                }
            }

            if (typeface == null)
            {
                typeface = new Typeface(fontFamily, fontStyle, fontWeight, fontStretch);
            }

            _runProperties = new RendererRunProperties(typeface, fontSize);
            _runProperties.SetForegroundBrush(SystemColors.ControlTextBrush);

            _paragraphProperties = new RendererParagraphProperties(_runProperties, textAlignment, textWrapping);
        }
        #endregion

        #region Methods
        private TextLine GetTextLine(string text, double paragraphWidth)
        {
            var textSource = new RendererTextSource(text, _runProperties);
            var line = _formatter.FormatLine(textSource, 0, paragraphWidth, _paragraphProperties, null);

            if (_textTrimming != TextTrimming.None)
            {
                TextCollapsingProperties collapsingProps;
                if (_textTrimming == TextTrimming.CharacterEllipsis)
                    collapsingProps = new TextTrailingCharacterEllipsis(paragraphWidth, _runProperties);
                else
                    collapsingProps = new TextTrailingWordEllipsis(paragraphWidth, _runProperties);
                return line.Collapse(collapsingProps);
            }

            return line;
        }

        public void Draw(DrawingContext dc, object content, Rect bounds)
        {
            if (content == null) return;

            var stringFormat = _stringFormat;
            if (string.IsNullOrEmpty(stringFormat)) stringFormat = "{0}";

            var text = string.Format(CultureInfo.CurrentCulture, stringFormat, content.ToString());
            text = Regex.Replace(text, @"\r?\n", " ");

            var line = GetTextLine(text, bounds.Width);
            var pt = new Point(bounds.X, bounds.Y);
            var lineHeight = line.Height;

            if (_verticalAlignment == VerticalAlignment.Center)
            {
                var y = (bounds.Height - lineHeight) / 2;
                pt.Y = bounds.Y + y;
            }
            else if (_verticalAlignment == VerticalAlignment.Bottom)
            {
                pt.Y = bounds.Bottom - lineHeight;
            }

            line.Draw(dc, pt, InvertAxes.None);
        }
        #endregion

        #region Nested Types
        private class RendererTextSource : TextSource
        {
            #region Variables
            private readonly TextRunProperties _textRunProperties;
            private readonly string _text;
            #endregion

            #region Properties
            public TextRunProperties TextRunProperties
            {
                get => _textRunProperties;
            }

            public string Text
            {
                get => _text;
            }
            #endregion

            #region Constructor
            internal RendererTextSource()
                : base()
            { }

            public RendererTextSource(string text, TextRunProperties properties)
                : this()
            {
                _text = text;
                _textRunProperties = properties;
            }
            #endregion

            #region Methods
            public override TextSpan<CultureSpecificCharacterBufferRange> GetPrecedingText(int dcp)
            {
                CharacterBufferRange charString = CharacterBufferRange.Empty;
                CultureInfo culture = null;

                if (dcp > 0)
                {
                    charString = new CharacterBufferRange(Text, 0, Math.Min(dcp, Text.Length));
                    culture = TextRunProperties.CultureInfo;
                }

                return new TextSpan<CultureSpecificCharacterBufferRange>(dcp, new CultureSpecificCharacterBufferRange(culture, charString));
            }

            public override int GetTextEffectCharacterIndexFromTextSourceCharacterIndex(int textSourceCharacterIndex)
            {
                return textSourceCharacterIndex;
            }

            public override TextRun GetTextRun(int textSourceCharacterIndex)
            {
                if (textSourceCharacterIndex == 0 && !string.IsNullOrEmpty(Text))
                {
                    return new TextCharacters(Text, 0, Text.Length, TextRunProperties);
                }
                return new TextEndOfParagraph(1);
            }
            #endregion
        }

        private class RendererRunProperties : TextRunProperties
        {
            #region Variables
            private Typeface _typeface;
            private Brush _backgroundBrush = null;
            private Brush _foregroundBrush = null;
            private CultureInfo _cultureInfo = null;
            private double _renderingSize = 0.0;
            private double _hintingSize = 0.0;
            private readonly TextDecorationCollection _decorations;
            private readonly TextEffectCollection _effects;
            #endregion

            #region Properties
            public override Typeface Typeface
            {
                get => _typeface;
            }

            public override Brush BackgroundBrush
            {
                get => _backgroundBrush;
            }

            public override Brush ForegroundBrush
            {
                get => _foregroundBrush;
            }

            public override CultureInfo CultureInfo
            {
                get => _cultureInfo;
            }

            public override double FontHintingEmSize
            {
                get => _hintingSize;
            }

            public override double FontRenderingEmSize
            {
                get => _renderingSize;
            }

            public override TextDecorationCollection TextDecorations
            {
                get => _decorations;
            }

            public override TextEffectCollection TextEffects
            {
                get => _effects;
            }
            #endregion

            #region Constructors
            private RendererRunProperties()
            {
                _cultureInfo = CultureInfo.CurrentCulture;
                _decorations = new TextDecorationCollection();
                _effects = new TextEffectCollection();
            }

            public RendererRunProperties(Typeface typeface)
                : this()
            {
                _typeface = typeface;
            }

            public RendererRunProperties(Typeface typeface, double fontSize)
                : this(typeface)
            {
                _renderingSize = fontSize;
                _hintingSize = fontSize;
            }
            #endregion

            #region Methods
            public void SetTypeface(Typeface value)
                => _typeface = value;

            public void SetCultureInfo(CultureInfo value)
                => _cultureInfo = value;

            public void SetBackgroundBrush(Brush value)
                => _backgroundBrush = value;

            public void SetForegroundBrush(Brush value)
                => _foregroundBrush = value;

            public void SetRenderingSize(double value)
                => _renderingSize = value;

            public void SetHintingSize(double value)
                => _hintingSize = value;

            public void SetFontSize(double value)
            {
                _renderingSize = value;
                _hintingSize = value;
            }

            public TextRunProperties Clone()
            {
                var other = new RendererRunProperties(_typeface)
                {
                    _cultureInfo = _cultureInfo,
                    _renderingSize = _renderingSize,
                    _hintingSize = _hintingSize
                };

                if (_backgroundBrush != null) other._backgroundBrush = _backgroundBrush.Clone();
                if (_foregroundBrush != null) other._foregroundBrush = _foregroundBrush.Clone();

                foreach(var decoration in _decorations)
                {
                    other.TextDecorations.Add(decoration.Clone());
                }

                foreach(var effect in _effects)
                {
                    other.TextEffects.Add(effect.Clone());
                }

                return other;
            }
            #endregion
        }

        private class RendererParagraphProperties : TextParagraphProperties
        {
            #region Variables
            private TextRunProperties _textRunProperties;
            private bool _firstLineInParagraph = false;
            private FlowDirection _flowDirection = FlowDirection.LeftToRight;
            private double _indent = 0.0;
            private double _lineHeight = double.NaN;
            private TextAlignment _textAlignment = TextAlignment.Left;
            private TextMarkerProperties _markerProperties = null;
            private TextWrapping _textWrapping = TextWrapping.NoWrap;
            #endregion

            #region Properties
            public override TextRunProperties DefaultTextRunProperties
            {
                get => _textRunProperties;
            }

            public override bool FirstLineInParagraph
            {
                get => _firstLineInParagraph;
            }

            public override FlowDirection FlowDirection
            {
                get => _flowDirection;
            }

            public override double Indent
            {
                get => _indent;
            }

            public override double LineHeight
            {
                get => _lineHeight;
            }

            public override TextAlignment TextAlignment
            {
                get => _textAlignment;
            }

            public override TextMarkerProperties TextMarkerProperties
            {
                get => _markerProperties;
            }

            public override TextWrapping TextWrapping
            {
                get => _textWrapping;
            }
            #endregion

            #region Constructor
            public RendererParagraphProperties(TextRunProperties textRunProperties, TextAlignment textAlignment, TextWrapping textWrapping)
            {
                _textRunProperties = textRunProperties;
                _textAlignment = textAlignment;
                _textWrapping = textWrapping;
            }
            #endregion

            #region Methods
            public void SetFirstLineInParagraph(bool value)
                => _firstLineInParagraph = value;

            public void SetFlowDirection(FlowDirection value)
                => _flowDirection = value;

            public void SetIndent(double value)
                => _indent = value;

            public void SetLineHeight(double value)
                => _lineHeight = value;

            public void SetTextAlignment(TextAlignment value)
                => _textAlignment = value;

            public void SetTextMarkerProperties(TextMarkerProperties value)
                => _markerProperties = value;

            public void SetTextWrapping(TextWrapping value)
                => _textWrapping = value;
            #endregion
        }
        #endregion
    }
}
