using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace wpfDialogs
{
    public class FontDialogWindow : Window
    {
        #region Constructors
        static FontDialogWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FontDialogWindow), new FrameworkPropertyMetadata(typeof(FontDialogWindow)));
        }

        public FontDialogWindow()
            : base()
        {
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }
        #endregion

        #region Methods
        internal void OnAcceptExecuted(ExecutedRoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
        #endregion
    }
}
