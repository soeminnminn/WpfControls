using System;
using System.Windows;
using System.Windows.Media;

namespace wpfDialogs
{
    public class FontDialog : DependencyObject
    {
        #region Variables
        private readonly FontDialogWindow _dialog;
        #endregion

        #region Constructor
        public FontDialog()
        {
            _dialog = new FontDialogWindow();
        }
        #endregion

        #region Dependency Properties

        #region TitleProperty
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            nameof(Title), typeof(string), typeof(FontDialog),
            new FrameworkPropertyMetadata("Select font"));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set { SetValue(TitleProperty, value); }
        }
        #endregion

        #region ShowColorProperty
        public static readonly DependencyProperty ShowColorProperty = DependencyProperty.Register(
            nameof(ShowColor), typeof(bool), typeof(FontDialog));
        public bool ShowColor
        {
            get => (bool)GetValue(ShowColorProperty);
            set { SetValue(ShowColorProperty, value); }
        }
        #endregion

        #region FontFamilyProperty
        public static readonly DependencyProperty FontFamilyProperty = DependencyProperty.Register(
            nameof(FontFamily), typeof(FontFamily), typeof(FontDialog));
        public FontFamily FontFamily
        {
            get => (FontFamily)GetValue(FontFamilyProperty);
            set { SetValue(FontFamilyProperty, value); }
        }
        #endregion

        #region FontStyleProperty
        public static readonly DependencyProperty FontStyleProperty = DependencyProperty.Register(
            nameof(FontStyle), typeof(FontStyle), typeof(FontDialog));
        public FontStyle FontStyle
        {
            get => (FontStyle)GetValue(FontStyleProperty);
            set { SetValue(FontStyleProperty, value); }
        }
        #endregion

        #region FontWeightProperty
        public static readonly DependencyProperty FontWeightProperty = DependencyProperty.Register(
            nameof(FontWeight), typeof(FontWeight), typeof(FontDialog));
        public FontWeight FontWeight
        {
            get => (FontWeight)GetValue(FontWeightProperty);
            set { SetValue(FontWeightProperty, value); }
        }
        #endregion

        #region FontStretchProperty
        public static readonly DependencyProperty FontStretchProperty = DependencyProperty.Register(
            nameof(FontStretch), typeof(FontStretch), typeof(FontDialog));
        public FontStretch FontStretch
        {
            get => (FontStretch)GetValue(FontStretchProperty);
            set { SetValue(FontStretchProperty, value); }
        }
        #endregion

        #region FontColorProperty
        public static readonly DependencyProperty FontColorProperty = DependencyProperty.Register(
            nameof(FontColor), typeof(Color), typeof(FontDialog));
        public Color FontColor
        {
            get => (Color)GetValue(FontColorProperty);
            set { SetValue(FontColorProperty, value); }
        }
        #endregion

        #region FontSizeProperty
        public static readonly DependencyProperty FontSizeProperty = DependencyProperty.Register(
            nameof(FontSize), typeof(double), typeof(FontDialog));
        public double FontSize
        {
            get => (double)GetValue(FontSizeProperty);
            set { SetValue(FontSizeProperty, value); }
        }
        #endregion

        #region StrikeoutProperty
        public static readonly DependencyProperty StrikeoutProperty = DependencyProperty.Register(
            nameof(Strikeout), typeof(bool), typeof(FontDialog));
        public bool Strikeout
        {
            get => (bool)GetValue(StrikeoutProperty);
            set { SetValue(StrikeoutProperty, value); }
        }
        #endregion

        #region UnderlineProperty
        public static readonly DependencyProperty UnderlineProperty = DependencyProperty.Register(
            nameof(Underline), typeof(bool), typeof(FontDialog));
        public bool Underline
        {
            get => (bool)GetValue(UnderlineProperty);
            set { SetValue(UnderlineProperty, value); }
        }
        #endregion

        #endregion

        #region Methods
        public bool? ShowDialog(Window owner)
        {
            _dialog.Owner = owner;
            _dialog.Title = Title;

            var model = new FontDialogModel(_dialog);
            model.ChooseColor = ShowColor;

            _dialog.DataContext = model;

            var result = _dialog.ShowDialog();
            if (result == true)
            {
                FontFamily = model.FontFamily;
                FontStyle = model.FontStyle;
                FontWeight = model.FontWeight;
                FontStretch = model.FontStretch;
                FontColor = model.FontColor;
                FontSize = model.FontSize;
                Strikeout = model.Strikeout;
                Underline = model.Underline;
            }
            return result;
        }
        #endregion
    }
}
