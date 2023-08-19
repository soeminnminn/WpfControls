using System;
using System.Windows;

namespace wspGridControl
{
    public class EditEndingEventArgs : EventArgs
    {
        #region Variables
        private readonly CellInfo _editingCell;
        private readonly RoutedEventArgs _editingEventArgs;
        private readonly FrameworkElement _editingElement;
        private bool _isCommiting;
        private bool _cancel;
        #endregion

        #region Constructors
        public EditEndingEventArgs(CellInfo cell, RoutedEventArgs editingEventArgs, FrameworkElement editingElement, bool isCommiting)
        {
            _editingCell = cell;
            _editingEventArgs = editingEventArgs;
            _editingElement = editingElement;
            _isCommiting = isCommiting;
        }
        #endregion

        #region Properties
        /// <summary>
        ///     When true, prevents the cell from entering edit mode.
        /// </summary>
        public bool Cancel
        {
            get { return _cancel; }
            set { _cancel = value; }
        }

        /// <summary>
        ///     The editing cell info.
        /// </summary>
        public CellInfo Cell
        {
            get => _editingCell;
        }

        /// <summary>
        ///     The editing element within the cell container.
        /// </summary>
        public FrameworkElement EditingElement
        {
            get => _editingElement;
        }

        /// <summary>
        ///     When true, is commiting.
        /// </summary>
        public bool IsCommiting
        {
            get => _isCommiting;
        }

        /// <summary>
        ///     The event arguments, if any, that led to the cell being placed in edit mode.
        /// </summary>
        public RoutedEventArgs EditingEventArgs
        {
            get => _editingEventArgs;
        }
        #endregion
    }
}
