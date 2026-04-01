using System.Windows.Media;

namespace Wpf.Ui.Violeta.Controls.Compat;

public static partial class SegoeFluentIcons
{
    public static FontFamily FontFamily => FontDictionary.SegoeFluentIcons;

    public static FontIconData CreateIcon(string glyph, bool forceFluent = false)
    {
        return new FontIconData(glyph, forceFluent ? FontFamily : new FontFamily(FontIcon.SegoeIconsFontFamilyName));
    }

    public static FontIconData CreateIcon(int chara, bool forceFluent = false)
    {
        return CreateIcon(FontIconData.ToGlyph(chara), forceFluent);
    }
}
