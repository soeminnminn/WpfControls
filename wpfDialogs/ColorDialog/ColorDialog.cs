using System;
using System.Windows;
using System.Windows.Media;

namespace wpfDialogs
{
    public class ColorDialog : DependencyObject
    {
        #region Variables
        private readonly ColorDialogWindow _dialog;
        #endregion

        #region Constructor
        public ColorDialog()
        {
            _dialog = new ColorDialogWindow();
        }
        #endregion

        #region Dependency Properties

        #region TitleProperty
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            nameof(Title), typeof(string), typeof(ColorDialog), 
            new FrameworkPropertyMetadata("Select color"));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set { SetValue(TitleProperty, value); }
        }
        #endregion

        #region ColorProperty
        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register(
            nameof(Color), typeof(Color), typeof(ColorDialog));

        public Color Color
        {
            get => (Color)GetValue(ColorProperty);
            set { SetValue(ColorProperty, value); }
        }
        #endregion

        #endregion

        #region Methods
        public bool? ShowDialog(Window owner)
        {
            _dialog.Owner = owner;
            _dialog.Title = Title;

            var model = new ColorDialogModel(_dialog);
            model.SetCurrentColor(Color);

            _dialog.DataContext = model;

            var result = _dialog.ShowDialog();
            if (result == true)
            {
                Color = model.SelectedColor;
            }
            return result;
        }
        #endregion
    }
}
