#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8618, CS8619, CS8625

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Controls.Compat;

public static partial class FluentSystemIcons
{
    public static FontFamily FontFamilyRegular => FontDictionary.FluentSystemIcons;

    public static FontFamily FontFamilyFilled => FontDictionary.FluentSystemIconsFilled;

    public static FontIconData CreateIcon(string glyph, FluentSystemIconVariants variant)
    {
        switch (variant)
        {
            case FluentSystemIconVariants.Regular:
                return new FontIconData(glyph, FontFamilyRegular);

            case FluentSystemIconVariants.Filled:
                return new FontIconData(glyph, FontFamilyFilled);
        }

        return new FontIconData(glyph);
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
