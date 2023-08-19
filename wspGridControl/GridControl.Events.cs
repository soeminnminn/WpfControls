using System;

namespace wspGridControl
{
    public partial class GridControl
    {
        #region Events

        #region CurrentCellChanged
        /// <summary>
        ///     An event to notify that the value of CurrentCell changed.
        /// </summary>
        public event EventHandler<EventArgs> CurrentCellChanged;

        /// <summary>
        ///     Called when the value of CurrentCell changes.
        /// </summary>
        /// <param name="e">Empty event arguments.</param>
        protected virtual void OnCurrentCellChanged(EventArgs e)
        {
            if (CurrentCellChanged != null)
            {
                CurrentCellChanged(this, e);
            }
        }
        #endregion

        #region SelectionChanged
        /// <summary>
        ///     An event to notify that the value of Selection changed.
        /// </summary>
        public event EventHandler<SelectionChangedEventArgs> SelectionChanged;

        /// <summary>
        ///     Called when the value of Selection changes.
        /// </summary>
        protected virtual void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            if (SelectionChanged != null)
            {
                SelectionChanged(this, e);
            }
        }
        #endregion

        #region BeginningEdit
        /// <summary>
        ///     Called just before a cell will change to edit mode
        ///     to allow handlers to prevent the cell from entering edit mode.
        /// </summary>
        public event EventHandler<BeginningEditEventArgs> BeginningEdit;

        /// <summary>
        ///     Called just before a cell will change to edit mode
        ///     to all subclasses to prevent the cell from entering edit mode.
        /// </summary>
        protected virtual void OnBeginningEdit(BeginningEditEventArgs e)
        {
            if (BeginningEdit != null)
            {
                BeginningEdit(this, e);
            }
        }
        #endregion

        #region PreparingEdit
        /// <summary>
        ///     Called after a cell has changed to editing mode to allow
        ///     handlers to modify the contents of the cell.
        /// </summary>
        public event EventHandler<PreparingEditEventArgs> PreparingEdit;

        /// <summary>
        ///     Called after a cell has changed to editing mode to allow
        ///     subclasses to modify the contents of the cell.
        /// </summary>
        protected internal virtual void OnPreparingEdit(PreparingEditEventArgs e)
        {
            if (PreparingEdit != null)
            {
                PreparingEdit(this, e);
            }
        }
        #endregion

        #region EditEnding
        /// <summary>
        ///     Raised just before cell editing is ended.
        ///     Gives handlers the opportunity to cancel the operation.
        /// </summary>
        public event EventHandler<EditEndingEventArgs> EditEnding;

        /// <summary>
        ///     Called just before cell editing is ended.
        ///     Gives subclasses the opportunity to cancel the operation.
        /// </summary>
        protected virtual void OnEditEnding(EditEndingEventArgs e)
        {
            if (EditEnding != null)
            {
                EditEnding(this, e);
            }
        }
        #endregion

        #endregion
    }
}
