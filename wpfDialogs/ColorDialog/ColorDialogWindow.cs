using System;
using System.Windows;
using System.Windows.Input;

namespace wpfDialogs
{
    public class ColorDialogWindow : Window
    {
        #region Variables
        private ColorDialogModel _model = null;
        #endregion

        #region Constructors
        static ColorDialogWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorDialogWindow), new FrameworkPropertyMetadata(typeof(ColorDialogWindow)));
        }

        public ColorDialogWindow()
            : base()
        {
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }
        #endregion

        #region Methods
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            if (DataContext is ColorDialogModel model)
            {
                _model = model;
            }
        }

        internal void OnDefineCustomCanExecute(CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _model != null && !_model.ShowCustoms;
        }

        internal void OnDefineCustomExecuted(ExecutedRoutedEventArgs e)
        { 
            if (_model != null)
            {
                _model.ShowCustoms = true;
                CommandManager.InvalidateRequerySuggested();
            }
        }

        internal void OnAddToCustomExecuted(ExecutedRoutedEventArgs e)
        {
            if (_model != null)
                _model.OnAddToCustomExecuted();
        }

        internal void OnAcceptExecuted(ExecutedRoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
        #endregion
    }
}
