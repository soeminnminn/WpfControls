using System;
using System.Globalization;
using System.Windows.Media;

namespace wpfDialogs
{
    internal static class ColorExtensions
    {
        #region Variables
        private static readonly uint[] basicColors = new uint[] {
            0xFFFF8080, 0xFFFFFF80, 0xFF80FF80, 0xFF00FF80, 0xFF80FFFF, 0xFF0080FF, 0xFFFF80C0, 0xFFFF80FF,
            0xFFFF0000, 0xFFFFFF00, 0xFF80FF00, 0xFF00FF40, 0xFF00FFFF, 0xFF0080C0, 0xFF8080C0, 0xFFFF00FF,
            0xFF804040, 0xFFFF8040, 0xFF00FF00, 0xFF008080, 0xFF004080, 0xFF8080FF, 0xFF800040, 0xFFFF0080,
            0xFF800000, 0xFFFF8000, 0xFF008000, 0xFF008040, 0xFF0000FF, 0xFF0000A0, 0xFF800080, 0xFF8000FF,
            0xFF400000, 0xFF804000, 0xFF004000, 0xFF004040, 0xFF000080, 0xFF000040, 0xFF400040, 0xFF400080,
            0xFF000000, 0xFF808000, 0xFF808040, 0xFF808080, 0xFF408080, 0xFFC0C0C0, 0xFF400040, 0xFFFFFFFF
        };
        private static Color[] _basicColors = null;

        private static readonly uint[] staticColors = new uint[] {
            0xFFFFFFFF, 0xFFFFC0C0, 0xFFFFE0C0, 0xFFFFFFC0, 0xFFC0FFC0, 0xFFC0FFFF, 0xFFC0C0FF, 0xFFFFC0FF,
            0xFFE0E0E0, 0xFFFF8080, 0xFFFFC080, 0xFFFFFF80, 0xFF80FF80, 0xFF80FFFF, 0xFF8080FF, 0xFFFF80FF,
            0xFFC0C0C0, 0xFFFF0000, 0xFFFF8000, 0xFFFFFF00, 0xFF00FF00, 0xFF00FFFF, 0xFF0000FF, 0xFFFF00FF,
            0xFF808080, 0xFFC00000, 0xFFC04000, 0xFFC0C000, 0xFF00C000, 0xFF00C0C0, 0xFF0000C0, 0xFFC000C0,
            0xFF404040, 0xFF800000, 0xFF804000, 0xFF808000, 0xFF008000, 0xFF008080, 0xFF000080, 0xFF800080,
            0xFF000000, 0xFF400000, 0xFF804040, 0xFF404000, 0xFF004000, 0xFF004040, 0xFF000040, 0xFF400040
        };
        private static Color[] _staticColors = null;

        private enum KnownColor : uint
        {
            Transparent             = 0x00FFFFFF,
            AliceBlue               = 0xFFF0F8FF,
            AntiqueWhite            = 0xFFFAEBD7,
            Aqua                    = 0xFF00FFFF,
            Aquamarine              = 0xFF7FFFD4,
            Azure                   = 0xFFF0FFFF,
            Beige                   = 0xFFF5F5DC,
            Bisque                  = 0xFFFFE4C4,
            Black                   = 0xFF000000,
            BlanchedAlmond          = 0xFFFFEBCD,
            Blue                    = 0xFF0000FF,
            BlueViolet              = 0xFF8A2BE2,
            Brown                   = 0xFFA52A2A,
            BurlyWood               = 0xFFDEB887,
            CadetBlue               = 0xFF5F9EA0,
            Chartreuse              = 0xFF7FFF00,
            Chocolate               = 0xFFD2691E,
            Coral                   = 0xFFFF7F50,
            CornflowerBlue          = 0xFF6495ED,
            Cornsilk                = 0xFFFFF8DC,
            Crimson                 = 0xFFDC143C,
            Cyan                    = 0xFF00FFFF,
            DarkBlue                = 0xFF00008B,
            DarkCyan                = 0xFF008B8B,
            DarkGoldenrod           = 0xFFB8860B,
            DarkGray                = 0xFFA9A9A9,
            DarkGreen               = 0xFF006400,
            DarkKhaki               = 0xFFBDB76B,
            DarkMagenta             = 0xFF8B008B,
            DarkOliveGreen          = 0xFF556B2F,
            DarkOrange              = 0xFFFF8C00,
            DarkOrchid              = 0xFF9932CC,
            DarkRed                 = 0xFF8B0000,
            DarkSalmon              = 0xFFE9967A,
            DarkSeaGreen            = 0xFF8FBC8F,
            DarkSlateBlue           = 0xFF483D8B,
            DarkSlateGray           = 0xFF2F4F4F,
            DarkTurquoise           = 0xFF00CED1,
            DarkViolet              = 0xFF9400D3,
            DeepPink                = 0xFFFF1493,
            DeepSkyBlue             = 0xFF00BFFF,
            DimGray                 = 0xFF696969,
            DodgerBlue              = 0xFF1E90FF,
            Firebrick               = 0xFFB22222,
            FloralWhite             = 0xFFFFFAF0,
            ForestGreen             = 0xFF228B22,
            Fuchsia                 = 0xFFFF00FF,
            Gainsboro               = 0xFFDCDCDC,
            GhostWhite              = 0xFFF8F8FF,
            Gold                    = 0xFFFFD700,
            Goldenrod               = 0xFFDAA520,
            Gray                    = 0xFF808080,
            Green                   = 0xFF008000,
            GreenYellow             = 0xFFADFF2F,
            Honeydew                = 0xFFF0FFF0,
            HotPink                 = 0xFFFF69B4,
            IndianRed               = 0xFFCD5C5C,
            Indigo                  = 0xFF4B0082,
            Ivory                   = 0xFFFFFFF0,
            Khaki                   = 0xFFF0E68C,
            Lavender                = 0xFFE6E6FA,
            LavenderBlush           = 0xFFFFF0F5,
            LawnGreen               = 0xFF7CFC00,
            LemonChiffon            = 0xFFFFFACD,
            LightBlue               = 0xFFADD8E6,
            LightCoral              = 0xFFF08080,
            LightCyan               = 0xFFE0FFFF,
            LightGoldenrodYellow    = 0xFFFAFAD2,
            LightGreen              = 0xFF90EE90,
            LightGray               = 0xFFD3D3D3,
            LightPink               = 0xFFFFB6C1,
            LightSalmon             = 0xFFFFA07A,
            LightSeaGreen           = 0xFF20B2AA,
            LightSkyBlue            = 0xFF87CEFA,
            LightSlateGray          = 0xFF778899,
            LightSteelBlue          = 0xFFB0C4DE,
            LightYellow             = 0xFFFFFFE0,
            Lime                    = 0xFF00FF00,
            LimeGreen               = 0xFF32CD32,
            Linen                   = 0xFFFAF0E6,
            Magenta                 = 0xFFFF00FF,
            Maroon                  = 0xFF800000,
            MediumAquamarine        = 0xFF66CDAA,
            MediumBlue              = 0xFF0000CD,
            MediumOrchid            = 0xFFBA55D3,
            MediumPurple            = 0xFF9370DB,
            MediumSeaGreen          = 0xFF3CB371,
            MediumSlateBlue         = 0xFF7B68EE,
            MediumSpringGreen       = 0xFF00FA9A,
            MediumTurquoise         = 0xFF48D1CC,
            MediumVioletRed         = 0xFFC71585,
            MidnightBlue            = 0xFF191970,
            MintCream               = 0xFFF5FFFA,
            MistyRose               = 0xFFFFE4E1,
            Moccasin                = 0xFFFFE4B5,
            NavajoWhite             = 0xFFFFDEAD,
            Navy                    = 0xFF000080,
            OldLace                 = 0xFFFDF5E6,
            Olive                   = 0xFF808000,
            OliveDrab               = 0xFF6B8E23,
            Orange                  = 0xFFFFA500,
            OrangeRed               = 0xFFFF4500,
            Orchid                  = 0xFFDA70D6,
            PaleGoldenrod           = 0xFFEEE8AA,
            PaleGreen               = 0xFF98FB98,
            PaleTurquoise           = 0xFFAFEEEE,
            PaleVioletRed           = 0xFFDB7093,
            PapayaWhip              = 0xFFFFEFD5,
            PeachPuff               = 0xFFFFDAB9,
            Peru                    = 0xFFCD853F,
            Pink                    = 0xFFFFC0CB,
            Plum                    = 0xFFDDA0DD,
            PowderBlue              = 0xFFB0E0E6,
            Purple                  = 0xFF800080,
            Red                     = 0xFFFF0000,
            RosyBrown               = 0xFFBC8F8F,
            RoyalBlue               = 0xFF4169E1,
            SaddleBrown             = 0xFF8B4513,
            Salmon                  = 0xFFFA8072,
            SandyBrown              = 0xFFF4A460,
            SeaGreen                = 0xFF2E8B57,
            SeaShell                = 0xFFFFF5EE,
            Sienna                  = 0xFFA0522D,
            Silver                  = 0xFFC0C0C0,
            SkyBlue                 = 0xFF87CEEB,
            SlateBlue               = 0xFF6A5ACD,
            SlateGray               = 0xFF708090,
            Snow                    = 0xFFFFFAFA,
            SpringGreen             = 0xFF00FF7F,
            SteelBlue               = 0xFF4682B4,
            Tan                     = 0xFFD2B48C,
            Teal                    = 0xFF008080,
            Thistle                 = 0xFFD8BFD8,
            Tomato                  = 0xFFFF6347,
            Turquoise               = 0xFF40E0D0,
            Violet                  = 0xFFEE82EE,
            Wheat                   = 0xFFF5DEB3,
            White                   = 0xFFFFFFFF,
            WhiteSmoke              = 0xFFF5F5F5,
            Yellow                  = 0xFFFFFF00,
            YellowGreen             = 0xFF9ACD32
        }
        private static Color[] _knownColors = null;

        private enum ConsoleColors : uint
        {
            Black           = 0xFF000000, // black
            DarkBlue        = 0xFF000080, // navy
            DarkGreen       = 0xFF008000, // green
            DarkCyan        = 0xFF008080, // teal
            DarkRed         = 0xFF800000, // maroon
            DarkMagenta     = 0xFF800080, // purple
            DarkYellow      = 0xFF808000, // olive
            Gray            = 0xFFC0C0C0, // silver
            DarkGray        = 0xFF808080, // grey
            Blue            = 0xFF0000FF, // blue
            Green           = 0xFF00FF00, // lime
            Cyan            = 0xFF00FFFF, // aqua
            Red             = 0xFFFF0000, // red
            Magenta         = 0xFFFF00FF, // fuchsia
            Yellow          = 0xFFFFFF00, // yellow
            White           = 0xFFFFFFFF, // white
        }

        private const int AlphaShift = 24;
        private const int RedShift = 16;
        private const int GreenShift = 8;
        private const int BlueShift = 0;

        private const int Win32RedShift = 0;
        private const int Win32GreenShift = 8;
        private const int Win32BlueShift = 16;
        #endregion

        #region Properties
        public static Color[] BasicColors
        {
            get
            {
                if (_basicColors == null)
                {
                    _basicColors = new Color[basicColors.Length];

                    for (int i = 0; i < basicColors.Length; i++)
                    {
                        _basicColors[i] = FromUInt32(basicColors[i]);
                    }
                }
                return _basicColors;
            }
        }

        public static Color[] StaticColors
        {
            get
            {
                if (_staticColors == null)
                {
                    _staticColors = new Color[staticColors.Length];
                    for (int i = 0; i < staticColors.Length; i++)
                    {
                        _staticColors[i] = FromUInt32(staticColors[i]);
                    }
                }
                return _staticColors;
            }
        }

        public static Color[] KnownColors
        {
            get
            {
                if (_knownColors == null)
                {
                    var values = Enum.GetValues(typeof(KnownColor));
                    _knownColors = new Color[values.Length];

                    for (int i = 0; i < values.Length; i++)
                    {
                        var val = (uint)values.GetValue(i);
                        _knownColors[i] = FromUInt32(val);
                    }
                }

                return _knownColors;
            }
        }
        #endregion

        #region Methods
        public static string GetKnownColorName(Color color)
        {
            uint value = ToUInt32(color);
            var name = Enum.GetName(typeof(KnownColor), value);
            return name == null ? ToHex(color) : name;
        }

        public static float GetHue(this Color color)
        {
            if (color.R == color.G && color.G == color.B)
                return 360.0f * 0.5f; // 0 makes as good an UNDEFINED value as any

            float r = (float)color.ScR;
            float g = (float)color.ScG;
            float b = (float)color.ScB;

            float max, min;
            float delta;
            float hue = 0.0f;

            max = r; min = r;

            if (g > max) max = g;
            if (b > max) max = b;

            if (g < min) min = g;
            if (b < min) min = b;

            delta = max - min;

            if (r == max)
            {
                hue = (g - b) / delta;
            }
            else if (g == max)
            {
                hue = 2 + (b - r) / delta;
            }
            else if (b == max)
            {
                hue = 4 + (r - g) / delta;
            }
            hue *= 60;

            if (hue < 0.0f)
            {
                hue += 360.0f;
            }
            return hue;
        }

        public static float GetSaturation(this Color color)
        {
            float r = (float)color.ScR;
            float g = (float)color.ScG;
            float b = (float)color.ScB;

            float max, min;
            float l, s = 0;

            max = r; min = r;

            if (g > max) max = g;
            if (b > max) max = b;

            if (g < min) min = g;
            if (b < min) min = b;

            // if max == min, then there is no color and the saturation is zero.
            if (max != min)
            {
                l = (max + min) / 2;

                if (l <= .5)
                {
                    s = (max - min) / (max + min);
                }
                else
                {
                    s = (max - min) / (2 - max - min);
                }
            }
            return s;
        }

        public static float GetBrightness(this Color color)
        {
            float r = (float)color.ScR;
            float g = (float)color.ScG;
            float b = (float)color.ScB;

            float max, min;

            max = r; min = r;

            if (g > max) max = g;
            if (b > max) max = b;

            if (g < min) min = g;
            if (b < min) min = b;

            return (max + min) / 2;
        }

        public static Color FromHsb(float hue, float saturation, float brightness)
        {
            float h = hue;
            float s = saturation;
            float l = brightness;

            float sn = h / 60.0f;
            int mod = (int)Math.Min(5, Math.Max(0, Math.Floor(sn)));

            float c = (1 - Math.Abs(2 * l - 1)) * s;
            float x = c * (1 - Math.Abs(sn % 2.0f - 1));
            float m = l - 0.5f * c;

            float[] ra = new float[] { c, x, 0, 0, x, c };
            float[] ga = new float[] { x, c, c, x, 0, 0 };
            float[] ba = new float[] { 0, 0, x, c, c, x };

            float r = ra[mod] + m;
            float b = ba[mod] + m;
            float g = ga[mod] + m;

            byte br = (byte)((int)(r * 255) & 0xFF);
            byte bg = (byte)((int)(g * 255) & 0xFF);
            byte bb = (byte)((int)(b * 255) & 0xFF);

            return Color.FromRgb(br, bg, bb);
        }

        public static Color FromUInt32(uint color)
        {
            uint value = color & 0xffffffff;
            byte a = (byte)((value >> AlphaShift) & 0xFF);
            byte r = (byte)((value >> RedShift) & 0xFF);
            byte g = (byte)((value >> GreenShift) & 0xFF);
            byte b = (byte)((value >> BlueShift) & 0xFF);

            return Color.FromArgb(a, r, g, b);
        }

        public static uint ToUInt32(Color color)
        {
            return ((uint)(color.R << RedShift | color.G << GreenShift | color.B << BlueShift | color.A << AlphaShift)) & 0xffffffff;
        }

        public static Color FromWin32(int win32Color)
        {
            byte r = (byte)((win32Color >> Win32RedShift) & 0xFF);
            byte g = (byte)((win32Color >> Win32GreenShift) & 0xFF);
            byte b = (byte)((win32Color >> Win32BlueShift) & 0xFF);
            return Color.FromRgb(r, g, b);
        }

        public static int ToWin32(this Color c)
        {
            return c.R << Win32RedShift | c.G << Win32GreenShift | c.B << Win32BlueShift;
        }

        public static string ToHex(Color color)
        {
            return string.Format(CultureInfo.InvariantCulture, "#{0:X2}{1:X2}{2:X2}{2:X2}", color.A, color.R, color.G, color.B);
        }

        public static bool IsEmpty(this Color c)
        {
            return c == Colors.Transparent;
        }
        #endregion
    }
}
