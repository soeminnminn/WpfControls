using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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

        #endregion

        #region Methods
        public bool? ShowDialog(Window owner)
        {
            _dialog.Owner = owner;
            _dialog.Title = Title;

            var model = new FontDialogModel(_dialog);

            _dialog.DataContext = model;

            var result = _dialog.ShowDialog();
            if (result == true)
            {
                //
            }
            return result;
        }
        #endregion
    }
}
