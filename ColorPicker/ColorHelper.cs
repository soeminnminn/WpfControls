using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Windows.Media;
using System.Reflection;
using System.Windows;

namespace ColorPicker
{
    /// <summary>
    /// Static <see cref="Color" /> helper methods.
    /// </summary>
    public static class ColorHelper
    {
        #region Variables
        internal static readonly uint[] themeColors = new uint[] {
            0xFFFFFFFF, 0xFF000000, 0xFFEEECE1, 0xFF1F497D, 0xFF4F81BD, 0xFFC0504D, 0xFF9BBB59, 
            0xFF8064A2, 0xFF4BACC6, 0xFFF79646, 0xFFF2F2F2, 0xFF7F7F7F, 0xFFDDD9C3, 0xFFC6D9F0, 
            0xFFDBE5F1, 0xFFF2DCDB, 0xFFEBF1DD, 0xFFE5E0EC, 0xFFDBEEF3, 0xFFFDEADA, 0xFFD8D8D8, 
            0xFF595959, 0xFFC4BD97, 0xFF8DB3E2, 0xFFB8CCE4, 0xFFE5B9B7, 0xFFD7E3BC, 0xFFCCC1D9, 
            0xFFB7DDE8, 0xFFFBD5B5, 0xFFBFBFBF, 0xFF3F3F3F, 0xFF938953, 0xFF548DD4, 0xFF95B3D7, 
            0xFFD99694, 0xFFC3D69B, 0xFFB2A2C7, 0xFF92CDDC, 0xFFFAC08F, 0xFFA5A5A5, 0xFF262626, 
            0xFF494429, 0xFF17365D, 0xFF366092, 0xFF953734, 0xFF76923C, 0xFF5F497A, 0xFF31859B, 
            0xFFE36C09, 0xFF6F7F7F, 0xFF0C0C0C, 0xFF1D1B10, 0xFF0F243E, 0xFF244061, 0xFF632423,
            0xFF4F6128, 0xFF3F3151, 0xFF205867, 0xFF974806
        };

        internal static readonly uint[] basicColors = new uint[] {
            0xFFFF8080, 0xFFFFFF80, 0xFF80FF80, 0xFF00FF80, 0xFF80FFFF, 0xFF0080FF, 0xFFFF80C0, 0xFFFF80FF,
            0xFFFF0000, 0xFFFFFF00, 0xFF80FF00, 0xFF00FF40, 0xFF00FFFF, 0xFF0080C0, 0xFF8080C0, 0xFFFF00FF,
            0xFF804040, 0xFFFF8040, 0xFF00FF00, 0xFF008080, 0xFF004080, 0xFF8080FF, 0xFF800040, 0xFFFF0080,
            0xFF800000, 0xFFFF8000, 0xFF008000, 0xFF008040, 0xFF0000FF, 0xFF0000A0, 0xFF800080, 0xFF8000FF,
            0xFF400000, 0xFF804000, 0xFF004000, 0xFF004040, 0xFF000080, 0xFF000040, 0xFF400040, 0xFF400080,
            0xFF000000, 0xFF808000, 0xFF808040, 0xFF808080, 0xFF408080, 0xFFC0C0C0, 0xFF400040, 0xFFFFFFFF
        };

        internal static readonly uint[] standardColors = new uint[] {
            0xFFB22222, 0xFFFF0000, 0xFFFF6347, 0xFFFF4500, 0xFFFFA500, 0xFFFFD700, 0xFFFFFF00,
            0xFF9ACD32, 0xFF2E8B57, 0xFF00BFFF, 0xFF6495ED, 0xFFADD8E6, 0xFF008B8B, 0xFF191970,
            0xFF9932CC, 0x00FFFFFF, 0x80000000, 0x80FFFFFF, 0x00000000
        };

        internal static readonly uint[] standardOpaqueColors = new uint[] {
            0xFFB22222, 0xFFFF0000, 0xFFFF6347, 0xFFFF4500, 0xFFFFA500, 0xFFFFD700, 0xFFFFFF00,
            0xFF9ACD32, 0xFF2E8B57, 0xFF00BFFF, 0xFF6495ED, 0xFFADD8E6, 0xFF008B8B, 0xFF191970,
            0xFF9932CC, 0x00000000
        };

        internal static readonly Dictionary<string, Color> colors = new Dictionary<string, Color>();
        internal static readonly Dictionary<string, Color> systemColors = new Dictionary<string, Color>();
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes static members of the <see cref="ColorHelper" /> class.
        /// </summary>
        static ColorHelper()
        {
            UndefinedColor = Color.FromArgb(0, 0, 0, 0);

            var scFields = typeof(SystemColors).GetProperties(BindingFlags.Public | BindingFlags.Static);
            foreach (var fi in scFields)
            {
                if (fi.GetValue(null, null) is Color c)
                {
                    systemColors.Add(fi.Name, c);
                }
            }

            var cFields = typeof(Colors).GetProperties(BindingFlags.Public | BindingFlags.Static);
            foreach (var fi in cFields)
            {
                var c = (Color)fi.GetValue(null, null);
                colors.Add(fi.Name, c);
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the undefined color.
        /// </summary>
        public static Color UndefinedColor { get; private set; }
        #endregion

        #region HEX
        /// <summary>
        /// Convert a <see cref="Color" /> to a hexadecimal string.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>
        /// The color to hex.
        /// </returns>
        public static string ColorToHex(this Color color, bool withoutAlpha = false)
        {
            if (withoutAlpha)
                return string.Format("#{0:X2}{1:X2}{2:X2}", color.R, color.G, color.B);

            return string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", color.A, color.R, color.G, color.B);
        }

        /// <summary>
        /// Convert a <see cref="Color" /> to unsigned int
        /// </summary>
        /// <param name="c">The source color.</param>
        /// <returns>
        /// The color to uint.
        /// </returns>
        public static uint ColorToUint(this Color c)
        {
            uint u = (uint)c.A << 24;
            u += (uint)c.R << 16;
            u += (uint)c.G << 8;
            u += c.B;
            return u;
        }

        /// <summary>
        /// Convert a hexadecimal string to <see cref="Color" />.
        /// </summary>
        /// <param name="value">The hex input string.</param>
        /// <returns>
        /// The color.
        /// </returns>
        public static Color HexToColor(string value)
        {
            value = value.Trim('#');
            if (value.Length == 0)
            {
                return UndefinedColor;
            }

            if (value.Length <= 6)
            {
                value = "FF" + value.PadLeft(6, '0');
            }

            uint u;
            if (uint.TryParse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out u))
            {
                return UIntToColor(u);
            }

            return UndefinedColor;
        }

        /// <summary>
        /// Convert an unsigned int (32bit) to <see cref="Color" />.
        /// </summary>
        /// <param name="color">The unsigned integer.</param>
        /// <returns>
        /// The color.
        /// </returns>
        public static Color UIntToColor(uint color)
        {
            var a = (byte)(color >> 24);
            var r = (byte)(color >> 16);
            var g = (byte)(color >> 8);
            var b = (byte)(color >> 0);
            return Color.FromArgb(a, r, g, b);
        }

        #endregion

        #region CMYK
        /// <summary>
        /// Converts RGB values to CMYK.
        /// </summary>
        /// <param name="r">The r.</param>
        /// <param name="g">The g.</param>
        /// <param name="b">The b.</param>
        /// <returns>
        /// The CMYK values.
        /// </returns>
        public static double[] ColorToCmyk(byte r, byte g, byte b)
        {
            if (r == 0 && g == 0 && b == 0)
            {
                return new[] { 0, 0, 0, 1.0 };
            }

            double computedC = 1 - (r / 255.0);
            double computedM = 1 - (g / 255.0);
            double computedY = 1 - (b / 255.0);

            var min = Math.Min(computedC, Math.Min(computedM, computedY));
            computedC = (computedC - min) / (1 - min);
            computedM = (computedM - min) / (1 - min);
            computedY = (computedY - min) / (1 - min);
            double computedK = min;

            return new[] { computedC, computedM, computedY, computedK };
        }

        /// <summary>
        /// Converts from a <see cref="Color" /> to to CMYK.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>
        /// The CMYK values.
        /// </returns>
        public static double[] ColorToCmyk(this Color color)
        {
            return ColorToCmyk(color.R, color.G, color.B);
        }
        
        /// <summary>
        /// Converts CMYK values to a <see cref="Color" />.
        /// </summary>
        /// <param name="c">The cyan value.</param>
        /// <param name="m">The magenta value.</param>
        /// <param name="y">The yellow value.</param>
        /// <param name="k">The black value.</param>
        /// <returns>
        /// The color.
        /// </returns>
        public static Color CmykToColor(double c, double m, double y, double k)
        {
            double r = 1 - (c / 100) * (1 - (k / 100)) - (k / 100);
            double g = 1 - (m / 100) * (1 - (k / 100)) - (k / 100);
            double b = 1 - (y / 100) * (1 - (k / 100)) - (k / 100);
            return Color.FromRgb((byte)(255 * r), (byte)(255 * g), (byte)(255 * b));
        }
        #endregion

        #region HSV
        /// <summary>
        /// Converts from a <see cref="Color" /> to HSV values (double)
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>
        /// Array of [Hue,Saturation,Value] in the range [0,1]
        /// </returns>
        public static double[] ColorToHsv(this Color color)
        {
            byte r = color.R;
            byte g = color.G;
            byte b = color.B;

            double h = 0;
            double s;

            double min = Math.Min(Math.Min(r, g), b);
            double v = Math.Max(Math.Max(r, g), b);
            double delta = v - min;

            if (v == 0.0)
            {
                s = 0;
            }
            else
            {
                s = delta / v;
            }

            if (s == 0)
            {
                h = 0.0;
            }
            else
            {
                if (r == v)
                {
                    h = (g - b) / delta;
                }
                else if (g == v)
                {
                    h = 2 + (b - r) / delta;
                }
                else if (b == v)
                {
                    h = 4 + (r - g) / delta;
                }

                h *= 60;
                if (h < 0.0)
                {
                    h = h + 360;
                }
            }

            var hsv = new double[3];
            hsv[0] = h / 360.0;
            hsv[1] = s;
            hsv[2] = v / 255.0;
            return hsv;
        }

        /// <summary>
        /// Converts from a <see cref="Color" /> to HSV values (byte)
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>
        /// Array of [Hue,Saturation,Value] in the range [0,255]
        /// </returns>
        public static byte[] ColorToHsvBytes(this Color color)
        {
            double[] hsv1 = ColorToHsv(color);
            var hsv2 = new byte[3];
            hsv2[0] = (byte)(hsv1[0] * 255);
            hsv2[1] = (byte)(hsv1[1] * 255);
            hsv2[2] = (byte)(hsv1[2] * 255);
            return hsv2;
        }

        /// <summary>
        /// Converts from HSV to a RGB <see cref="Color" />.
        /// </summary>
        /// <param name="hue">The hue.</param>
        /// <param name="saturation">The saturation.</param>
        /// <param name="value">The value.</param>
        /// <param name="alpha">The alpha.</param>
        /// <returns>
        /// The color.
        /// </returns>
        public static Color HsvToColor(byte hue, byte saturation, byte value, byte alpha = 255)
        {
            double r, g, b;
            double h = hue * 360.0 / 255;
            double s = saturation / 255.0;
            double v = value / 255.0;

            if (s == 0)
            {
                r = v;
                g = v;
                b = v;
            }
            else
            {
                if (h == 360)
                {
                    h = 0;
                }
                else
                {
                    h = h / 60;
                }

                var i = (int)Math.Truncate(h);
                double f = h - i;

                double p = v * (1.0 - s);
                double q = v * (1.0 - (s * f));
                double t = v * (1.0 - (s * (1.0 - f)));

                switch (i)
                {
                    case 0:
                        r = v;
                        g = t;
                        b = p;
                        break;

                    case 1:
                        r = q;
                        g = v;
                        b = p;
                        break;

                    case 2:
                        r = p;
                        g = v;
                        b = t;
                        break;

                    case 3:
                        r = p;
                        g = q;
                        b = v;
                        break;

                    case 4:
                        r = t;
                        g = p;
                        b = v;
                        break;

                    default:
                        r = v;
                        g = p;
                        b = q;
                        break;
                }
            }

            return Color.FromArgb(alpha, (byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
        }

        /// <summary>
        /// Convert from HSV to <see cref="Color" />.
        /// http://en.wikipedia.org/wiki/HSL_color_space
        /// </summary>
        /// <param name="hue">The Hue value [0,1].</param>
        /// <param name="sat">The saturation value [0,1].</param>
        /// <param name="val">The brightness value [0,1].</param>
        /// <param name="alpha">The alpha value [0.1].</param>
        /// <returns>
        /// The color.
        /// </returns>
        public static Color HsvToColor(double hue, double sat, double val, double alpha = 1.0)
        {
            double r = 0;
            double g = 0;
            double b = 0;

            if (sat == 0)
            {
                // Gray scale
                r = g = b = val;
            }
            else
            {
                if (hue == 1.0)
                {
                    hue = 0;
                }

                hue *= 6.0;
                var i = (int)Math.Floor(hue);
                double f = hue - i;
                double aa = val * (1 - sat);
                double bb = val * (1 - (sat * f));
                double cc = val * (1 - (sat * (1 - f)));
                switch (i)
                {
                    case 0:
                        r = val;
                        g = cc;
                        b = aa;
                        break;
                    case 1:
                        r = bb;
                        g = val;
                        b = aa;
                        break;
                    case 2:
                        r = aa;
                        g = val;
                        b = cc;
                        break;
                    case 3:
                        r = aa;
                        g = bb;
                        b = val;
                        break;
                    case 4:
                        r = cc;
                        g = aa;
                        b = val;
                        break;
                    case 5:
                        r = val;
                        g = aa;
                        b = bb;
                        break;
                }
            }

            return Color.FromArgb((byte)(alpha * 255), (byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
        }

        /// <summary>
        /// Calculate the difference in hue between two <see cref="Color" />s.
        /// </summary>
        /// <param name="c1">The first color.</param>
        /// <param name="c2">The second color.</param>
        /// <returns>
        /// The hue difference.
        /// </returns>
        public static double HueDifference(Color c1, Color c2)
        {
            double[] hsv1 = ColorToHsv(c1);
            double[] hsv2 = ColorToHsv(c2);
            double dh = hsv1[0] - hsv2[0];

            // clamp to [-0.5,0.5]
            if (dh > 0.5)
            {
                dh -= 1.0;
            }

            if (dh < -0.5)
            {
                dh += 1.0;
            }

            double e = dh * dh;
            return Math.Sqrt(e);
        }
        #endregion

        #region HSL
        private static double calcH(double r, double g, double b, double diff)
        {
            if (r == g && g == b)
                return 0;
            else if (r >= g && r >= b)
                return hueTerm(0, g, b, diff);
            else if (g >= r && g >= b)
                return hueTerm(2, b, r, diff);
            else
                return hueTerm(4, r, g, diff);
        }

        private static double hueTerm(int s, double n1, double n2, double diff)
        {
            double res = 60 * (s + (n1 - n2) / diff);
            return res > 0 ? res : res + 360;
        }

        /// <summary>
        /// Convert RGB to HSL.
        /// </summary>
        /// <param name="r">Red value between 0 and 1.</param>
        /// <param name="g">Green value between 0 and 1.</param>
        /// <param name="b">Blue value between 0 and 1.</param>
        /// <returns>Array with three elements containing H value between 0 and 360 and S and L value between 0 and 1.</returns>
        public static double[] RGBToHSL(double r, double g, double b)
        {
            double max = (double)Math.Max(Math.Max(r, g), b);
            double min = (double)Math.Min(Math.Min(r, g), b);
            double diff = max - min;
            double h = calcH(r, g, b, diff);
            double s = (max == 0 || min == 1) ? 0 : diff / (1 - Math.Abs(max + min - 1));
            double l = (max + min) / 2;

            return new double[] { h, s, l };
        }

        /// <summary>
        /// Convert HSL to RGB.
        /// </summary>
        /// <param name="h">Hue value between 0 and 360.</param>
        /// <param name="s">Saturation value between 0 and 1.</param>
        /// <param name="l">Lightness value between 0 and 1.</param>
        /// <returns>Array with three elements containing R, G and B value.</returns>
        public static double[] HSLToRGB(int h, double s, double l)
        {
            double c = (1 - Math.Abs(2 * l - 1)) * s;
            double hh = h / 60.0;
            double x = c * (1 - Math.Abs(hh % 2.0 - 1));
            double[] rgb = new double[] { 0, 0, 0 };

            if (hh <= 1)
                rgb = new double[] { c, x, 0 };
            else if (hh <= 2)
                rgb = new double[] { x, c, 0 };
            else if (hh <= 3)
                rgb = new double[] { 0, c, x };
            else if (hh <= 4)
                rgb = new double[] { 0, x, c };
            else if (hh <= 5)
                rgb = new double[] { x, 0, c };
            else if (hh <= 6)
                rgb = new double[] { c, 0, x };

            double m = l - 0.5 * c;
            rgb[0] += m; 
            rgb[1] += m; 
            rgb[2] += m;
            return rgb;
        }
        #endregion

        #region Lab
        private const double D65X = 0.9505;
        private const double D65Y = 1.0;
        private const double D65Z = 1.0890;

        private static double Fxyz(double t)
        {
            return (t > 0.008856) ? Math.Pow(t, 1.0 / 3.0) : (7.787 * t + 16.0 / 116.0);
        }

        /// <summary>
        /// Converts from a <see cref="Color" /> to to Lab.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>
        /// The Lab values.
        /// </returns>
        public static double[] ColorToLab(this Color c)
        {
            // normalize red, green, blue values
            double rLinear = c.R / 255.0;
            double gLinear = c.G / 255.0;
            double bLinear = c.B / 255.0;

            double r = (rLinear > 0.04045) ? Math.Pow((rLinear + 0.055) / (1 + 0.055), 2.2) : (rLinear / 12.92);
            double g = (gLinear > 0.04045) ? Math.Pow((gLinear + 0.055) / (1 + 0.055), 2.2) : (gLinear / 12.92);
            double b = (bLinear > 0.04045) ? Math.Pow((bLinear + 0.055) / (1 + 0.055), 2.2) : (bLinear / 12.92);

            double CIEX = r * 0.4124 + g * 0.3576 + b * 0.1805;
            double CIEY = r * 0.2126 + g * 0.7152 + b * 0.0722;
            double CIEZ = r * 0.0193 + g * 0.1192 + b * 0.9505;

            double lVal = 116.0 * Fxyz(CIEY / D65Y) - 16;
            double aVal = 500.0 * (Fxyz(CIEX / D65X) - Fxyz(CIEY / D65Y));
            double bVal = 200.0 * (Fxyz(CIEY / D65Y) - Fxyz(CIEZ / D65Z));

            return new [] { lVal, aVal, bVal };
        }

        /// <summary>
        /// Converts Lab values to a <see cref="Color" />.
        /// </summary>
        /// <param name="l">The lightness value.</param>
        /// <param name="a">The red/green value.</param>
        /// <param name="b">The blue/yellow value.</param>
        /// <returns>
        /// The color.
        /// </returns>
        public static Color LabToColor(double l, double a, double b)
        {
            double theta = 6.0 / 29.0;

            double fy = (l + 16) / 116.0;
            double fx = fy + (a / 500.0);
            double fz = fy - (b / 200.0);

            var x = (fx > theta) ? D65X * (fx * fx * fx) : (fx - 16.0 / 116.0) * 3 * (theta * theta) * D65X;
            var y = (fy > theta) ? D65Y * (fy * fy * fy) : (fy - 16.0 / 116.0) * 3 * (theta * theta) * D65Y;
            var z = (fz > theta) ? D65Z * (fz * fz * fz) : (fz - 16.0 / 116.0) * 3 * (theta * theta) * D65Z;

            x = (x > 0.9505) ? 0.9505 : ((x < 0) ? 0 : x);
            y = (y > 1.0) ? 1.0 : ((y < 0) ? 0 : y);
            z = (z > 1.089) ? 1.089 : ((z < 0) ? 0 : z);

            double[] clinear = new double[3];
            clinear[0] = x * 3.2410 - y * 1.5374 - z * 0.4986; // red
            clinear[1] = -x * 0.9692 + y * 1.8760 - z * 0.0416; // green
            clinear[2] = x * 0.0556 - y * 0.2040 + z * 1.0570; // blue

            for (int i = 0; i < 3; i++)
            {
                clinear[i] = (clinear[i] <= 0.0031308) ? 12.92 * clinear[i] : (1 + 0.055) * Math.Pow(clinear[i], (1.0 / 2.4)) - 0.055;
                clinear[i] = Math.Min(clinear[i], 1);
                clinear[i] = Math.Max(clinear[i], 0);
            }

            return Color.FromRgb(
                (byte)(clinear[0] * 255.0),
                (byte)(clinear[1] * 255.0),
                (byte)(clinear[2] * 255.0));
        }
        #endregion

        #region Methods
        /// <summary>
        /// Change the alpha value of a color.
        /// </summary>
        /// <param name="c">The source color.</param>
        /// <param name="alpha">The new alpha value.</param>
        /// <returns>
        /// The new color.
        /// </returns>
        public static Color ChangeAlpha(this Color c, byte alpha)
        {
            return Color.FromArgb(alpha, c.R, c.G, c.B);
        }

        /// <summary>
        /// Calculates the difference between two <see cref="Color" />s.
        /// </summary>
        /// <param name="c1">The first color.</param>
        /// <param name="c2">The second color.</param>
        /// <returns>
        /// L2-norm in RGBA space.
        /// </returns>
        public static double ColorDifference(Color c1, Color c2)
        {
            // http://en.wikipedia.org/wiki/Color_difference
            // http://mathworld.wolfram.com/L2-Norm.html
            double dr = (c1.R - c2.R) / 255.0;
            double dg = (c1.G - c2.G) / 255.0;
            double db = (c1.B - c2.B) / 255.0;
            double da = (c1.A - c2.A) / 255.0;
            double e = dr * dr + dg * dg + db * db + da * da;
            return Math.Sqrt(e);
        }

        /// <summary>
        /// Calculates the complementary color.
        /// </summary>
        /// <param name="c">The source color.</param>
        /// <returns>
        /// The complementary color.
        /// </returns>
        public static Color Complementary(this Color c)
        {
            // http://en.wikipedia.org/wiki/Complementary_color
            double[] hsv = ColorToHsv(c);
            double newHue = hsv[0] - 0.5;

            // clamp to [0,1]
            if (newHue < 0)
            {
                newHue += 1.0;
            }

            return HsvToColor(newHue, hsv[1], hsv[2]);
        }

        /// <summary>
        /// Gets the hue spectrum colors.
        /// </summary>
        /// <param name="colorCount">The number of colors.</param>
        /// <returns>
        /// The spectrum.
        /// </returns>
        public static Color[] GetSpectrumColors(int colorCount)
        {
            var spectrumColors = new Color[colorCount];
            for (int i = 0; i < colorCount; ++i)
            {
                double hue = (i * 1.0) / (colorCount - 1);
                spectrumColors[i] = HsvToColor(hue, 1.0, 1.0);
            }

            return spectrumColors;
        }

        /// <summary>
        /// Linear interpolation between two <see cref="Color" />s.
        /// </summary>
        /// <param name="c0">The first color.</param>
        /// <param name="c1">The second color.</param>
        /// <param name="x">The interpolation factor.</param>
        /// <returns>
        /// The interpolated color.
        /// </returns>
        public static Color Interpolate(Color c0, Color c1, double x)
        {
            double r = c0.R * (1 - x) + c1.R * x;
            double g = c0.G * (1 - x) + c1.G * x;
            double b = c0.B * (1 - x) + c1.B * x;
            double a = c0.A * (1 - x) + c1.A * x;
            return Color.FromArgb((byte)a, (byte)r, (byte)g, (byte)b);
        }
        #endregion
    }
}
