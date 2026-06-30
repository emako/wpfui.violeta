#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8618, CS8619, CS8625

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
