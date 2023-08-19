using System;
using System.Windows.Media;

namespace wpfDialogs
{
    public class ColorChangedEventArgs : EventArgs
    {
        #region Variables
        private readonly Color _color;
        #endregion

        #region Constructor
        internal ColorChangedEventArgs(Color color)
        {
            _color = color;
        }
        #endregion

        #region Properties
        public Color Color
        {
            get => _color;
        }
        #endregion
    }
}
