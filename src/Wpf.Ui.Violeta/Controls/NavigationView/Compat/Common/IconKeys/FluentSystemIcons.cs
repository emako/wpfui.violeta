using System.Windows.Media;

namespace Wpf.Ui.Violeta.Controls.Compat;

public static partial class FluentSystemIcons
{
    public static FontFamily FontFamilyRegular => FontDictionary.FluentSystemIcons;

    public static FontFamily FontFamilyFilled => FontDictionary.FluentSystemIconsFilled;

    public static FontIconData CreateIcon(string glyph, FluentSystemIconVariants variant)
    {
        return variant switch
        {
            FluentSystemIconVariants.Regular => new FontIconData(glyph, FontFamilyRegular),
            FluentSystemIconVariants.Filled => new FontIconData(glyph, FontFamilyFilled),
            _ => new FontIconData(glyph),
        };
    }

    public static FontIconData CreateIcon(int chara, FluentSystemIconVariants variant)
    {
        return CreateIcon(FontIconData.ToGlyph(chara), variant);
    }
}

public enum FluentSystemIconVariants
{
    Regular,
    Filled,
}
