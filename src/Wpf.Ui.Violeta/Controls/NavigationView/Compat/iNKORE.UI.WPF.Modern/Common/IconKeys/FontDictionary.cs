using System;
using System.Windows.Media;

namespace iNKORE.UI.WPF.Modern.Common.IconKeys
{
    public static class FontDictionary
    {
        // Minimal fallback font mapping used by NavigationView icon helpers.
        public static FontFamily SegoeUISymbol => new("Segoe UI Symbol");

        public static FontFamily SegoeMDL2Assets => new("Segoe MDL2 Assets");

        public static FontFamily SegoeFluentIcons => new("Segoe Fluent Icons");

        public static FontFamily FluentSystemIcons => SegoeFluentIcons;

        public static FontFamily FluentSystemIconsFilled => SegoeFluentIcons;
    }

    public readonly struct FontIconData
    {
        public FontFamily? FontFamily { get; }

        public string Glyph { get; }

        public FontIconData(string glyph, FontFamily? family = null)
        {
            Glyph = glyph;
            FontFamily = family;
        }

        public static string ToGlyph(int chara)
        {
            return char.ConvertFromUtf32(chara);
        }

        public static int ToUtf32(string glyph)
        {
            if (string.IsNullOrEmpty(glyph))
            {
                throw new ArgumentException("Input glyph cannot be null or empty.");
            }

            if (glyph.Length == 1)
            {
                return char.ConvertToUtf32(glyph, 0);
            }

            if (glyph.Length == 2 && char.IsSurrogatePair(glyph[0], glyph[1]))
            {
                return char.ConvertToUtf32(glyph, 0);
            }

            throw new ArgumentException("Input glyph must be a single character or a valid surrogate pair.");
        }
    }
}
