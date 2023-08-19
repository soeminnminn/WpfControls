using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfDialogs
{
    public class FontCharset
    {
        #region Variables
        private readonly string _displayName;
        private readonly byte _charset;
        #endregion

        #region Constructors
        internal FontCharset(string displayName, byte charset)
        {
            _displayName = displayName;
            _charset = charset;
        }
        #endregion

        #region Properties
        public string DisplayName
        {
            get => _displayName;
        }

        public byte Charset
        {
            get => _charset;
        }
        #endregion

        #region Methods
        public override string ToString()
        {
            return _displayName;
        }
        #endregion
    }

    public static class FontCharsets
    {
        public static FontCharset ANSI_CHARSET = new FontCharset("Western", 0x00);
        public static FontCharset DEFAULT_CHARSET = new FontCharset("Default", 0x01);
        public static FontCharset SYMBOL_CHARSET = new FontCharset("Symbol", 0x02);
        public static FontCharset MAC_CHARSET = new FontCharset("Standard Roman", 0x4D);
        public static FontCharset SHIFTJIS_CHARSET = new FontCharset("Japanese", 0x80);
        public static FontCharset HANGUL_CHARSET = new FontCharset("Hangul", 0x81);
        public static FontCharset JOHAB_CHARSET = new FontCharset("Korean", 0x82);
        public static FontCharset GB2312_CHARSET = new FontCharset("Chinese Simplified", 0x86);
        public static FontCharset CHINESEBIG5_CHARSET = new FontCharset("Chinese Traditional", 0x88);
        public static FontCharset GREEK_CHARSET = new FontCharset("Greek", 0xA1);
        public static FontCharset TURKISH_CHARSET = new FontCharset("Turkish", 0xA2);
        public static FontCharset VIETNAMESE_CHARSET = new FontCharset("Vietnamese", 0xA3);
        public static FontCharset HEBREW_CHARSET = new FontCharset("Hebrew", 0xB1);
        public static FontCharset ARABIC_CHARSET = new FontCharset("Arabic", 0xB2);
        public static FontCharset BALTIC_CHARSET = new FontCharset("Baltic", 0xBA);
        public static FontCharset RUSSIAN_CHARSET = new FontCharset("Cyrillic", 0xCC);
        public static FontCharset THAI_CHARSET = new FontCharset("Thai", 0xDE);
        public static FontCharset EASTEUROPE_CHARSET = new FontCharset("Central European", 0xEE);
        public static FontCharset OEM_CHARSET = new FontCharset("OEM", 0xFF);
    }
}
