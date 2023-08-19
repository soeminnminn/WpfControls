using System;
using System.Windows;

namespace wspGridControl
{
    public class BeginningEditEventArgs : EventArgs
    {
        #region Variables
        private readonly CellInfo _editingCell;
        private readonly RoutedEventArgs _editingEventArgs;
        private bool _cancel;
        #endregion

        #region Constructors
        public BeginningEditEventArgs(CellInfo cell, RoutedEventArgs editingEventArgs)
        {
            _editingCell = cell;
            _editingEventArgs = editingEventArgs;
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
        ///     The event arguments, if any, that led to the cell being placed in edit mode.
        /// </summary>
        public RoutedEventArgs EditingEventArgs
        {
            get => _editingEventArgs;
        }
        #endregion
    }
}
