using System;

namespace wspGridControl
{
    public class SelectionChangedEventArgs : EventArgs
    {
        #region Variables
        private BlockOfCells _cells;
        #endregion

        #region Constructors
        public SelectionChangedEventArgs(BlockOfCells cells)
        {
            _cells = cells;
        }
        #endregion

        #region Properties
        public BlockOfCells Cells
        {
            get => _cells;
        }
        #endregion
    }
}
