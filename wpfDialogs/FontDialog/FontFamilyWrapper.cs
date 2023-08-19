using System;
using System.Globalization;
using System.Windows.Media;

namespace wpfDialogs
{
    public class FontFamilyWrapper : IComparable
    {
        #region Variables
        private readonly FontFamily _fontFamily;
        private readonly string _displayName;
        private readonly bool _isSymbol;
        #endregion

        #region Constructor
        internal FontFamilyWrapper(FontFamily fontFamily)
        {
            _fontFamily = fontFamily;
            _displayName = FontExtensions.GetDisplayName(fontFamily.FamilyNames);
            _isSymbol = FontExtensions.IsSymbolFont(fontFamily);
        }
        #endregion

        #region Properties
        public FontFamily FontFamily
        {
            get => _fontFamily;
        }

        public string DisplayName
        {
            get => _displayName;
        }

        public string Source
        {
            get => _fontFamily.Source;
        }

        public bool IsSymbol
        {
            get => _isSymbol;
        }
        #endregion

        #region Methods
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            else if (obj is FontFamily family)
                return _fontFamily.Source == family.Source;
            else if (obj is string str)
                return _fontFamily.Source == str || _displayName == str;
            else if (obj is FontFamilyWrapper wrapper)
                return _fontFamily.Source == wrapper.Source;

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return _fontFamily.GetHashCode();
        }

        public override string ToString()
        {
            return _displayName;
        }

        public int CompareTo(object other)
        {
            if (other == null) return 1;

            string name = string.Empty;

            if (other is FontFamily family)
                name = FontExtensions.GetDisplayName(family.FamilyNames);
            else if (other is string str)
                name = str;
            else if (other is FontFamilyWrapper wrapper)
                name = wrapper.DisplayName;

            return string.Compare(_displayName, name, true, CultureInfo.CurrentCulture);
        }
        #endregion

        #region Operators
        public static implicit operator FontFamilyWrapper(FontFamily value)
            => new FontFamilyWrapper(value);
        public static explicit operator FontFamily(FontFamilyWrapper value)
            => value.FontFamily;
        #endregion
    }
}
