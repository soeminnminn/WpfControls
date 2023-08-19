using System;
using System.Windows.Input;

namespace wspGridControl
{
    public partial class GridControl
    {
        #region Commands
        public static readonly RoutedCommand BeginEditCommand = new RoutedCommand("BeginEdit", typeof(GridControl));
        public static readonly RoutedCommand CommitEditCommand = new RoutedCommand("CommitEdit", typeof(GridControl));
        public static readonly RoutedCommand CancelEditCommand = new RoutedCommand("CancelEdit", typeof(GridControl));

        private static void OnCanExecuteBeginEdit(object sender, CanExecuteRoutedEventArgs e)
        {
            if (sender is GridControl c)
                c.OnCanExecuteBeginEdit(e);
        }

        private static void OnExecutedBeginEdit(object sender, ExecutedRoutedEventArgs e)
        {
            if (sender is GridControl c)
                c.OnExecutedBeginEdit(e);
        }

        private static void OnCanExecuteCommitEdit(object sender, CanExecuteRoutedEventArgs e)
        {
            if (sender is GridControl c)
                c.OnCanExecuteCommitEdit(e);
        }

        private static void OnExecutedCommitEdit(object sender, ExecutedRoutedEventArgs e)
        {
            if (sender is GridControl c)
                c.OnExecutedCommitEdit(e);
        }

        private static void OnCanExecuteCancelEdit(object sender, CanExecuteRoutedEventArgs e)
        {
            if (sender is GridControl c)
                c.OnCanExecuteCancelEdit(e);
        }

        private static void OnExecutedCancelEdit(object sender, ExecutedRoutedEventArgs e)
        {
            if (sender is GridControl c)
                c.OnExecutedCancelEdit(e);
        }
        #endregion

        #region Nested Types
        private class AutoScrollCommand : ICommand
        {
            #region Variables
            private readonly GridControl _owner;

            private event EventHandler _canExecuteChanged = null;

            public event EventHandler CanExecuteChanged
            {
                add => _canExecuteChanged += value;
                remove => _canExecuteChanged -= value;
            }
            #endregion

            #region Constructors
            public AutoScrollCommand(GridControl owner)
            {
                _owner = owner;
            }
            #endregion

            #region Methods
            public bool CanExecute(object parameter)
            {
                return _owner.CanAutoScroll();
            }

            public void Execute(object parameter)
            {
                _owner.DoAutoScroll();
            }

            public void RaiseCanExecuteChanged()
            {
                if (_canExecuteChanged != null)
                {
                    _canExecuteChanged(this, EventArgs.Empty);
                }
            }
            #endregion
        }
        #endregion
    }
}
