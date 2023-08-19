using System;
using System.Globalization;
using System.Windows.Media;
using System.Windows;

namespace wpfDialogs
{
    public class TypefaceWrapper : IComparable
    {
        #region Variables
        private readonly Typeface _typeface;
        private readonly string _displayName;
        private readonly bool _isSymbol;
        #endregion

        #region Constructor
        internal TypefaceWrapper(Typeface typeface)
        {
            _typeface = typeface;
            _displayName = FontExtensions.GetDisplayName(typeface.FaceNames);

            if (typeface.TryGetGlyphTypeface(out GlyphTypeface face))
            {
                _isSymbol = face.Symbol;
                
            }
            else
                _isSymbol = false;
        }
        #endregion

        #region Properties
        public Typeface Typeface
        {
            get => _typeface;
        }

        public string DisplayName
        {
            get => _displayName;
        }

        public FontFamily FontFamily
        {
            get => _typeface.FontFamily;
        }

        public FontStyle Style
        {
            get => _typeface.Style;
        }

        public FontWeight Weight
        {
            get => _typeface.Weight;
        }

        public FontStretch Stretch
        {
            get => _typeface.Stretch;
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
                return _typeface.FontFamily.Source == family.Source;
            else if (obj is Typeface typeface)
                return _typeface.Style == typeface.Style && _typeface.Weight == typeface.Weight && _typeface.Stretch == typeface.Stretch;
            else if (obj is string str)
                return _displayName == str;
            else if (obj is TypefaceWrapper wrapper)
                return Equals(wrapper._typeface);

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return _typeface.GetHashCode();
        }

        public override string ToString()
        {
            return _displayName;
        }

        public int CompareTo(object other)
        {
            if (other == null) return 1;

            string name = string.Empty;

            if (other is Typeface typeface)
                name = FontExtensions.GetDisplayName(typeface.FaceNames);
            else if (other is string str)
                name = str;
            else if (other is TypefaceWrapper wrapper)
                name = wrapper.DisplayName;

            return string.Compare(_displayName, name, true, CultureInfo.CurrentCulture);
        }
        #endregion

        #region Operators
        public static implicit operator TypefaceWrapper(Typeface value)
            => new TypefaceWrapper(value);
        public static explicit operator Typeface(TypefaceWrapper value)
            => value.Typeface;
        #endregion
    }
}
