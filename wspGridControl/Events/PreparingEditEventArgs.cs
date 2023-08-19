using System;
using System.Windows;

namespace wspGridControl
{
    public class PreparingEditEventArgs : EventArgs
    {
        #region Variables
        private readonly CellInfo _editingCell;
        private readonly RoutedEventArgs _editingEventArgs;
        private readonly FrameworkElement _editingElement;
        private object _content;
        #endregion

        #region Constructors
        public PreparingEditEventArgs(CellInfo cell, RoutedEventArgs editingEventArgs, FrameworkElement editingElement, object content)
        {
            _editingCell = cell;
            _editingEventArgs = editingEventArgs;
            _editingElement = editingElement;
            _content = content;
        }
        #endregion

        #region Properties
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
        ///     The cell content
        /// </summary>
        public object Content
        {
            get => _content;
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
