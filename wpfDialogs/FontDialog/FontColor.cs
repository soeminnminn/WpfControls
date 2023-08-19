using System;
using System.Windows.Media;

namespace wpfDialogs
{
    public class FontColor
    {
        #region Variables
        private readonly Color _color;
        private readonly string _name;
        #endregion

        #region Constructors
        internal FontColor(Color color, string name)
        {
            _color = color;
            _name = name;
        }
        #endregion

        #region Properties
        public Color Color
        {
            get => _color;
        }

        public string Name
        {
            get => _name;
        }
        #endregion

        #region Methods
        public override string ToString()
        {
            return _name;
        }

        public override bool Equals(object obj)
        {
            if (obj is Color color)
                return _color == color;
            else if (obj is string str)
                return _name == str;

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return _color.GetHashCode();
        }
        #endregion
    }

    public static class FontColors
    {
        public static readonly FontColor Black = new FontColor(Colors.Black, "Black");
        public static readonly FontColor Maroon = new FontColor(Colors.Maroon, "Maroon");
        public static readonly FontColor Green = new FontColor(Colors.Green, "Green");
        public static readonly FontColor Olive = new FontColor(Colors.Olive, "Olive");
        public static readonly FontColor Navy = new FontColor(Colors.Navy, "Navy");
        public static readonly FontColor Purple = new FontColor(Colors.Purple, "Purple");
        public static readonly FontColor Teal = new FontColor(Colors.Teal, "Teal");
        public static readonly FontColor Gray = new FontColor(Colors.Gray, "Gray");
        public static readonly FontColor Silver = new FontColor(Colors.Silver, "Silver");
        public static readonly FontColor Red = new FontColor(Colors.Red, "Red");
        public static readonly FontColor Lime = new FontColor(Colors.Lime, "Lime");
        public static readonly FontColor Yellow = new FontColor(Colors.Yellow, "Yellow");
        public static readonly FontColor Blue = new FontColor(Colors.Blue, "Blue");
        public static readonly FontColor Fuchsia = new FontColor(Colors.Fuchsia, "Fuchsia");
        public static readonly FontColor Aqua = new FontColor(Colors.Aqua, "Aqua");
        public static readonly FontColor White = new FontColor(Colors.White, "White");

        private static FontColor[] _availableColors = null;
        public static FontColor[] AvailableColors
        {
            get
            {
                if (_availableColors == null)
                {
                    _availableColors = new FontColor[]
                    {
                        Black, Maroon, Green, Olive, Navy, Purple, Teal, Gray, Silver, Red, Lime, Yellow, Blue, Fuchsia, Aqua, White
                    };
                }
                return _availableColors;
            }
        }

        public static int FindIndex(Color color)
        {
            
            for (int i =0; i < AvailableColors.Length; i++)
            {
                var c = AvailableColors[i];
                if (c.Equals(color))
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
