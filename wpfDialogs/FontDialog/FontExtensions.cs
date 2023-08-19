using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Markup;
using System.Windows.Media;

namespace wpfDialogs
{
    internal static class FontExtensions
    {
        public static string GetDisplayName(LanguageSpecificStringDictionary nameDictionary)
        {
            return GetDisplayName(nameDictionary, CultureInfo.CurrentUICulture.IetfLanguageTag);
        }

        public static string GetDisplayName(LanguageSpecificStringDictionary nameDictionary, string languageTag)
        {
            // Look up the display name based on the UI culture, which is the same culture
            // used for resource loading.
            var userLanguage = XmlLanguage.GetLanguage(languageTag);

            // Look for an exact match.
            string name;
            if (nameDictionary.TryGetValue(userLanguage, out name))
            {
                return name;
            }

            // No exact match; return the name for the most closely related language.
            var bestRelatedness = -1;
            var bestName = string.Empty;

            foreach (var pair in nameDictionary)
            {
                var relatedness = GetRelatedness(pair.Key, userLanguage);
                if (relatedness > bestRelatedness)
                {
                    bestRelatedness = relatedness;
                    bestName = pair.Value;
                }
            }

            return bestName;
        }

        public static string GetDisplayName(IDictionary<CultureInfo, string> nameDictionary)
        {
            return GetDisplayName(nameDictionary, CultureInfo.CurrentUICulture.IetfLanguageTag);
        }

        public static string GetDisplayName(IDictionary<CultureInfo, string> nameDictionary, string languageTag)
        {
            CultureInfo cultureInfo = CultureInfo.GetCultureInfoByIetfLanguageTag(languageTag);

            // Look for an exact match.
            string name;
            if (nameDictionary.TryGetValue(cultureInfo, out name))
            {
                return name;
            }

            // No exact match; return the name for the most closely related language.
            var bestRelatedness = -1;
            var bestName = string.Empty;

            var userLanguage = XmlLanguage.GetLanguage(languageTag);

            foreach (var pair in nameDictionary)
            {
                var relatedness = GetRelatedness(XmlLanguage.GetLanguage(pair.Key.IetfLanguageTag), userLanguage);
                if (relatedness > bestRelatedness)
                {
                    bestRelatedness = relatedness;
                    bestName = pair.Value;
                }
            }

            return bestName;
        }

        private static int GetRelatedness(XmlLanguage keyLang, XmlLanguage userLang)
        {
            try
            {
                // Get equivalent cultures.
                var keyCulture = CultureInfo.GetCultureInfoByIetfLanguageTag(keyLang.IetfLanguageTag);
                var userCulture = CultureInfo.GetCultureInfoByIetfLanguageTag(userLang.IetfLanguageTag);
                if (!userCulture.IsNeutralCulture)
                {
                    userCulture = userCulture.Parent;
                }

                // If the key is a prefix or parent of the user language it's a good match.
                if (IsPrefixOf(keyLang.IetfLanguageTag, userLang.IetfLanguageTag) || userCulture.Equals(keyCulture))
                {
                    return 2;
                }

                // If the key and user language share a common prefix or parent neutral culture, it's a reasonable match.
                if (IsPrefixOf(TrimSuffix(userLang.IetfLanguageTag), keyLang.IetfLanguageTag) ||
                    userCulture.Equals(keyCulture.Parent))
                {
                    return 1;
                }
            }
            catch (ArgumentException)
            {
                // Language tag with no corresponding CultureInfo.
            }

            // They're unrelated languages.
            return 0;
        }

        private static string TrimSuffix(string tag)
        {
            var i = tag.LastIndexOf('-');
            if (i > 0)
            {
                return tag.Substring(0, i);
            }
            return tag;
        }

        private static bool IsPrefixOf(string prefix, string tag)
        {
            return prefix.Length < tag.Length &&
                   tag[prefix.Length] == '-' &&
                   string.CompareOrdinal(prefix, 0, tag, 0, prefix.Length) == 0;
        }

        public static bool IsSymbolFont(FontFamily fontFamily)
        {
            foreach (var typeface in fontFamily.GetTypefaces())
            {
                if (typeface.TryGetGlyphTypeface(out GlyphTypeface face))
                {
                    return face.Symbol;
                }
            }
            return false;
        }

        public static bool FuzzyEqual(double a, double b) => Math.Abs(a - b) < 0.01;

        public static double PointsToPixels(double value) => value * (96.0 / 72.0);

        public static double PixelsToPoints(double value) => value * (72.0 / 96.0);

        public static string StripVerticalName(string familyName)
        {
            if (familyName != null && familyName.Length > 1 && familyName[0] == '@')
            {
                return familyName.Substring(1);
            }
            return familyName;
        }
    }
}
