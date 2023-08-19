using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace wpfDialogs
{
    public class FontDialogNative : CommonDialog
    {
        #region Variables
        private const string enIetfLanguageTag = "en-US";
        private const int defaultMinSize = 0;
        private const int defaultMaxSize = 0;

        private int options;

        private Typeface typeface = null;
        private double fontSize = 0;

        private Color color;
        private bool showColor;
        private bool usingDefaultIndirectColor;

        private int minSize = defaultMinSize;
        private int maxSize = defaultMaxSize;

        #region CF
        protected const int CF_APPLY = 0x200;
        // protected const int CF_BITMAP = 2;
        //protected const int CF_DIB = 8;
        //protected const int CF_DIF = 5;
        protected const int CF_EFFECTS = 0x100;
        protected const int CF_ENABLEHOOK = 8;
        // protected const int CF_ENHMETAFILE = 14;
        protected const int CF_FIXEDPITCHONLY = 0x4000;
        protected const int CF_FORCEFONTEXIST = 0x10000;
        // protected const int CF_HDROP = 15;
        protected const int CF_INITTOLOGFONTSTRUCT = 0x40;
        protected const int CF_LIMITSIZE = 0x2000;
        // protected const int CF_LOCALE = 0x10;
        // protected const int CF_METAFILEPICT = 3;
        protected const int CF_NOSIMULATIONS = 0x1000;
        protected const int CF_NOVECTORFONTS = 0x800;
        protected const int CF_NOVERTFONTS = 0x1000000;
        // protected const int CF_OEMTEXT = 7;
        // protected const int CF_PALETTE = 9;
        // protected const int CF_PENDATA = 10;
        // protected const int CF_RIFF = 11;
        protected const int CF_SCREENFONTS = 1;
        protected const int CF_SCRIPTSONLY = 0x400;
        protected const int CF_SELECTSCRIPT = 0x400000;
        protected const int CF_SHOWHELP = 4;
        //protected const int CF_SYLK = 4;
        //protected const int CF_TEXT = 1;
        //protected const int CF_TIFF = 6;
        protected const int CF_TTONLY = 0x40000;
        //protected const int CF_UNICODETEXT = 13;
        //protected const int CF_WAVE = 12;
        #endregion

        #endregion

        #region Properties
        public bool AllowSimulations
        {
            get => !GetOption(CF_NOSIMULATIONS);
            set { SetOption(CF_NOSIMULATIONS, !value); }
        }

        public bool AllowVectorFonts
        {
            get => !GetOption(CF_NOVECTORFONTS);
            set { SetOption(CF_NOVECTORFONTS, !value); }
        }

        public bool AllowVerticalFonts
        {
            get => !GetOption(CF_NOVERTFONTS);
            set { SetOption(CF_NOVERTFONTS, !value); }
        }

        public bool AllowScriptChange
        {
            get => !GetOption(CF_SELECTSCRIPT);
            set { SetOption(CF_SELECTSCRIPT, !value); }
        }

        public bool FixedPitchOnly
        {
            get => GetOption(CF_FIXEDPITCHONLY);
            set { SetOption(CF_FIXEDPITCHONLY, value); }
        }

        public bool ScriptsOnly
        {
            get => GetOption(CF_SCRIPTSONLY);
            set { SetOption(CF_SCRIPTSONLY, value); }
        }

        public bool ShowApply
        {
            get => GetOption(CF_APPLY);
            set { SetOption(CF_APPLY, value); }
        }

        public bool ShowColor
        {
            get => showColor;
            set { showColor = value; }
        }

        public bool ShowEffects
        {
            get => GetOption(CF_EFFECTS);
            set { SetOption(CF_EFFECTS, value); }
        }

        public bool ShowHelp
        {
            get => GetOption(CF_SHOWHELP);
            set { SetOption(CF_SHOWHELP, value); }
        }

        public bool FontMustExist
        {
            get => GetOption(CF_FORCEFONTEXIST);
            set { SetOption(CF_FORCEFONTEXIST, value); }
        }

        public int MaxSize
        {
            get => maxSize;
            set
            {
                if (value < 0)
                {
                    value = 0;
                }
                maxSize = value;

                if (maxSize > 0 && maxSize < minSize)
                {
                    minSize = maxSize;
                }
            }
        }

        public int MinSize
        {
            get => minSize;
            set
            {
                if (value < 0)
                {
                    value = 0;
                }
                minSize = value;

                if (maxSize > 0 && maxSize < minSize)
                {
                    maxSize = minSize;
                }
            }
        }

        public Color Color
        {
            get
            {
                // Convert to RGB and back to resolve indirect colors like SystemColors.ControlText
                // to real color values like Color.Lime
                if (usingDefaultIndirectColor)
                {
                    return ColorExtensions.FromWin32(ColorExtensions.ToWin32(color));
                }
                return color;
            }
            set
            {
                if (!value.IsEmpty())
                {
                    color = value;
                    usingDefaultIndirectColor = false;
                }
                else
                {
                    color = SystemColors.ControlTextColor;
                    usingDefaultIndirectColor = true;
                }
            }
        }

        public Typeface Font
        {
            get => typeface;
            set { typeface = value; }
        }

        protected int Options
        {
            get => options;
        }
        #endregion

        #region Events
        private event EventHandler _apply;

        public event EventHandler Apply
        {
            add { _apply += value; }
            remove { _apply -= value; }
        }
        #endregion

        #region Constructor
        public FontDialogNative()
        {
            Reset();
        }
        #endregion

        #region Methods
        private bool GetOption(int option)
        {
            return (options & option) != 0;
        }

        private void SetOption(int option, bool value)
        {
            if (value)
            {
                options |= option;
            }
            else
            {
                options &= ~option;
            }
        }

        private void ToLogFont(ref LOGFONT logFont)
        {
            if (typeface == null) return;

            logFont.lfCharSet = (byte)FontCharset.ANSI_CHARSET;
            logFont.lfOutPrecision = (byte)FontPrecision.OUT_DEFAULT_PRECIS;
            logFont.lfClipPrecision = (byte)FontClipPrecision.CLIP_DEFAULT_PRECIS;
            logFont.lfQuality = (byte)FontQuality.DEFAULT_QUALITY;
            logFont.lfPitchAndFamily = (byte)FontPitchAndFamily.DEFAULT_PITCH;
            
            logFont.lfWidth = 0;
            logFont.lfEscapement = 0;
            logFont.lfOrientation = 0;

            if (typeface.TryGetGlyphTypeface(out GlyphTypeface glyphTypeface))
            {
                var familyName = FontExtensions.GetDisplayName(glyphTypeface.Win32FamilyNames, enIetfLanguageTag);
                logFont.lfFaceName = familyName;
            }
            else
            {
                logFont.lfFaceName = typeface.FontFamily.Source;
            }
            
            logFont.lfItalic = FontStyles.Italic == typeface.Style ? (byte)1 : (byte)0;
            logFont.lfWeight = typeface.Weight.ToOpenTypeWeight();
            logFont.lfHeight = (int)FontExtensions.PointsToPixels(fontSize);
            logFont.lfUnderline = 0;
            logFont.lfStrikeOut = 0;
        }

        public override void Reset()
        {
            ResetFont();

            options = CF_SCREENFONTS | CF_EFFECTS;
            color = SystemColors.ControlTextColor;
            usingDefaultIndirectColor = true;
            showColor = false;
            minSize = defaultMinSize;
            maxSize = defaultMaxSize;
            SetOption(CF_TTONLY, true);
        }

        private void ResetFont()
        {
            var fontFamily = TextBlock.FontFamilyProperty.DefaultMetadata.DefaultValue as FontFamily;
            if (fontFamily != null)
            {
                var fontStyle = (FontStyle)TextBlock.FontStyleProperty.DefaultMetadata.DefaultValue;
                var fontWeight = (FontWeight)TextBlock.FontWeightProperty.DefaultMetadata.DefaultValue;
                var fontStretch = (FontStretch)TextBlock.FontStretchProperty.DefaultMetadata.DefaultValue;
                typeface = new Typeface(fontFamily, fontStyle, fontWeight, fontStretch);
            }
            else
            {
                typeface = null;
            }
            
            fontSize = (double)TextBlock.FontSizeProperty.DefaultMetadata.DefaultValue;
        }

        protected override IntPtr HookProc(IntPtr hWnd, int msg, IntPtr wparam, IntPtr lparam)
        {
            switch (msg)
            {
                case WM_COMMAND:
                    if ((int)wparam == 0x402)
                    {
                        LOGFONT lf = new LOGFONT();
                        SendMessage(new HandleRef(null, hWnd), WM_CHOOSEFONT_GETLOGFONT, 0, lf);
                        UpdateFont(lf);
                        int index = (int)SendDlgItemMessage(new HandleRef(null, hWnd), 0x473, CB_GETCURSEL, IntPtr.Zero, IntPtr.Zero);
                        if (index != CB_ERR)
                        {
                            UpdateColor((int)SendDlgItemMessage(new HandleRef(null, hWnd), 0x473, CB_GETITEMDATA, (IntPtr)index, IntPtr.Zero));
                        }
                        try
                        {
                            OnApply(EventArgs.Empty);
                        }
                        catch (Exception)
                        { }
                    }
                    break;
                case WM_INITDIALOG:
                    if (!showColor)
                    {
                        IntPtr dlgItem = GetDlgItem(new HandleRef(null, hWnd), cmb4);
                        ShowWindow(new HandleRef(null, dlgItem), SW_HIDE);
                        dlgItem = GetDlgItem(new HandleRef(null, hWnd), stc4);
                        ShowWindow(new HandleRef(null, dlgItem), SW_HIDE);
                    }
                    break;
            }
            return base.HookProc(hWnd, msg, wparam, lparam);
        }

        protected override bool RunDialog(IntPtr hwndOwner)
        {
            WndProc hookProcPtr = new WndProc(HookProc);
            CHOOSEFONT cf = new CHOOSEFONT();

            LOGFONT lf = new LOGFONT();
            ToLogFont(ref lf);

            IntPtr logFontPtr = IntPtr.Zero;
            try
            {
                logFontPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(LOGFONT)));
                Marshal.StructureToPtr(lf, logFontPtr, false);

                // cf.iPointSize = (int)FontExtensions.PixelsToPoints(fontSize);
                cf.lStructSize = Marshal.SizeOf(typeof(CHOOSEFONT));
                cf.hwndOwner = hwndOwner;
                cf.hDC = IntPtr.Zero;
                cf.lpLogFont = logFontPtr;
                cf.Flags = Options | CF_INITTOLOGFONTSTRUCT | CF_ENABLEHOOK;
                if (minSize > 0 || maxSize > 0)
                {
                    cf.Flags |= CF_LIMITSIZE;
                }

                if (ShowColor || ShowEffects)
                {
                    cf.rgbColors = ColorExtensions.ToWin32(color);
                }
                else
                {
                    cf.rgbColors = ColorExtensions.ToWin32(SystemColors.ControlTextColor);
                }

                cf.lpfnHook = hookProcPtr;
                cf.hInstance = GetModuleHandle(null);
                cf.nSizeMin = minSize;

                if (maxSize == 0)
                {
                    cf.nSizeMax = int.MaxValue;
                }
                else
                {
                    cf.nSizeMax = maxSize;
                }

                if (ChooseFont(cf))
                {
                    LOGFONT lfReturned = (LOGFONT)Marshal.PtrToStructure(logFontPtr, typeof(LOGFONT));
                    if (lfReturned.lfFaceName != null && lfReturned.lfFaceName.Length > 0)
                    {
                        lf = lfReturned;
                        UpdateFont(lf);
                        UpdateColor(cf.rgbColors);
                    }

                    return true;
                }

                return false;
            }
            finally
            {
                if (logFontPtr != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(logFontPtr);
            }
        }

        private void OnApply(EventArgs e)
        {
            if (_apply != null)
            {
                _apply(this, e);
            }
        }

        private void UpdateColor(int rgb)
        {
            if (ColorExtensions.ToWin32(color) != rgb)
            {
                color = ColorExtensions.FromWin32(rgb);
                usingDefaultIndirectColor = false;
            }
        }

        private void UpdateFont(LOGFONT lf)
        { 
            try
            {
                string faceName = lf.lfFaceName;
                if (!string.IsNullOrEmpty(faceName))
                {
                    var fontFamily = new FontFamily(faceName);
                    var fontWeight = FontWeight.FromOpenTypeWeight(lf.lfWeight);
                    var fontStyle = lf.lfItalic != 0 ? FontStyles.Italic : FontStyles.Normal;

                    string ffName = null;
                    var ffNames = fontFamily.FamilyNames.Values;
                    foreach (var ffn in ffNames)
                    {
                        if (ffn == faceName || faceName.StartsWith(ffn))
                        {
                            ffName = ffn;
                            break;
                        }
                    }
                    if (!string.IsNullOrEmpty(ffName) && faceName != ffName)
                    {
                        fontFamily = new FontFamily(ffName);
                    }

                    Typeface result = null;
                    var typefaces = fontFamily.GetTypefaces();

                    foreach (var tf in typefaces)
                    {
                        if (tf.Style == fontStyle && tf.Weight == fontWeight)
                        {
                            if (faceName == ffName)
                            {
                                result = tf;
                                break;
                            }
                            else
                            {
                                var names = tf.FaceNames.Values;
                                foreach (var tfName in names)
                                {
                                    if (faceName.EndsWith(tfName))
                                    {
                                        result = tf;
                                        break;
                                    }
                                }

                                if (result != null)
                                {
                                    break;
                                }
                            }
                        }
                    }

                    if (result != null)
                    {
                        typeface = result;
                    }
                    else
                    {
                        typeface = new Typeface(fontFamily, fontStyle, fontWeight, FontStretches.Normal);
                    }
                }
            }
            catch(Exception)
            {

            }
        }

        public override string ToString()
        {
            string text = base.ToString();

            var ffName = FontExtensions.GetDisplayName(typeface.FontFamily.FamilyNames);
            var resName = FontExtensions.GetDisplayName(typeface.FaceNames);

            return text + ",  Font: " + ffName + " " + resName;
        }
        #endregion

        #region Native Methods
        protected const int WM_COMMAND = 0x0111;
        protected const int WM_CHOOSEFONT_GETLOGFONT = 0x401;

        protected const int CB_ERR = (-1);
        protected const int CB_GETCURSEL = 0x0147;
        protected const int CB_GETITEMDATA = 0x0150;

        protected const int cmb4 = 0x0473;
        protected const int stc4 = 0x0443;

        protected const int SW_HIDE = 0;

        [DllImport("comdlg32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        protected static extern bool ChooseFont([In, Out] CHOOSEFONT cf);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        protected static extern IntPtr GetModuleHandle(string modName);

        [DllImport("user32.dll")]
        protected static extern IntPtr GetDlgItem(HandleRef hDlg, int nIDDlgItem);

        [DllImport("user32.dll")]
        protected static extern IntPtr SendDlgItemMessage(HandleRef hDlg, int nIDDlgItem, int Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        protected static extern IntPtr SendMessage(HandleRef hWnd, int msg, int wParam, [In][Out] LOGFONT lParam);

        [DllImport("user32.dll")]
        protected static extern bool ShowWindow(HandleRef hWnd, int nCmdShow);
        #endregion

        #region Nested Types
        protected enum FontCharset : byte
        {
            ANSI_CHARSET = 0x00,
            DEFAULT_CHARSET = 0x01,
            SYMBOL_CHARSET = 0x02,
            MAC_CHARSET = 0x4D,
            SHIFTJIS_CHARSET = 0x80,
            HANGUL_CHARSET = 0x81,
            JOHAB_CHARSET = 0x82,
            GB2312_CHARSET = 0x86,
            CHINESEBIG5_CHARSET = 0x88,
            GREEK_CHARSET = 0xA1,
            TURKISH_CHARSET = 0xA2,
            VIETNAMESE_CHARSET = 0xA3,
            HEBREW_CHARSET = 0xB1,
            ARABIC_CHARSET = 0xB2,
            BALTIC_CHARSET = 0xBA,
            RUSSIAN_CHARSET = 0xCC,
            THAI_CHARSET = 0xDE,
            EASTEUROPE_CHARSET = 0xEE,
            OEM_CHARSET = 0xFF
        }

        protected enum FontPrecision : byte
        {
            OUT_DEFAULT_PRECIS = 0,
            OUT_STRING_PRECIS = 1,
            OUT_CHARACTER_PRECIS = 2,
            OUT_STROKE_PRECIS = 3,
            OUT_TT_PRECIS = 4,
            OUT_DEVICE_PRECIS = 5,
            OUT_RASTER_PRECIS = 6,
            OUT_TT_ONLY_PRECIS = 7,
            OUT_OUTLINE_PRECIS = 8,
            OUT_SCREEN_OUTLINE_PRECIS = 9,
            OUT_PS_ONLY_PRECIS = 10,
        }

        protected enum FontClipPrecision : byte
        {
            CLIP_DEFAULT_PRECIS = 0,
            CLIP_CHARACTER_PRECIS = 1,
            CLIP_STROKE_PRECIS = 2,
            CLIP_MASK = 0XF,
            CLIP_LH_ANGLES = (1 << 4),
            CLIP_TT_ALWAYS = (2 << 4),
            CLIP_DFA_DISABLE = (4 << 4),
            CLIP_EMBEDDED = (8 << 4),
        }

        protected enum FontQuality : byte
        {
            DEFAULT_QUALITY = 0,
            DRAFT_QUALITY = 1,
            PROOF_QUALITY = 2,
            NONANTIALIASED_QUALITY = 3,
            ANTIALIASED_QUALITY = 4,
            CLEARTYPE_QUALITY = 5,
            CLEARTYPE_NATURAL_QUALITY = 6,
        }

        [Flags]
        protected enum FontPitchAndFamily : byte
        {
            DEFAULT_PITCH = 0,
            FIXED_PITCH = 1,
            VARIABLE_PITCH = 2,
            FF_DONTCARE = (0 << 4),
            FF_ROMAN = (1 << 4),
            FF_SWISS = (2 << 4),
            FF_MODERN = (3 << 4),
            FF_SCRIPT = (4 << 4),
            FF_DECORATIVE = (5 << 4),
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        protected class CHOOSEFONT
        {
            public int lStructSize = Marshal.SizeOf(typeof(CHOOSEFONT));
            public IntPtr hwndOwner;
            public IntPtr hDC;
            public IntPtr lpLogFont;
            public int iPointSize;
            public int Flags;
            public int rgbColors;
            public IntPtr lCustData = IntPtr.Zero;
            public WndProc lpfnHook;
            public string lpTemplateName;
            public IntPtr hInstance;
            public string lpszStyle;
            public short nFontType;
            public short ___MISSING_ALIGNMENT__;
            public int nSizeMin;
            public int nSizeMax;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        protected class LOGFONT
        {
            public int lfHeight;
            public int lfWidth;
            public int lfEscapement;
            public int lfOrientation;
            public int lfWeight;
            public byte lfItalic;
            public byte lfUnderline;
            public byte lfStrikeOut;
            public byte lfCharSet;
            public byte lfOutPrecision;
            public byte lfClipPrecision;
            public byte lfQuality;
            public byte lfPitchAndFamily;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string lfFaceName;

            public LOGFONT()
            {
            }

            public LOGFONT(LOGFONT lf)
            {
                this.lfHeight = lf.lfHeight;
                this.lfWidth = lf.lfWidth;
                this.lfEscapement = lf.lfEscapement;
                this.lfOrientation = lf.lfOrientation;
                this.lfWeight = lf.lfWeight;
                this.lfItalic = lf.lfItalic;
                this.lfUnderline = lf.lfUnderline;
                this.lfStrikeOut = lf.lfStrikeOut;
                this.lfCharSet = lf.lfCharSet;
                this.lfOutPrecision = lf.lfOutPrecision;
                this.lfClipPrecision = lf.lfClipPrecision;
                this.lfQuality = lf.lfQuality;
                this.lfPitchAndFamily = lf.lfPitchAndFamily;
                this.lfFaceName = lf.lfFaceName;
            }
        }
        #endregion
    }
}
