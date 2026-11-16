using FluentUI.FontGlyphs.SourceGenerator;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Glyph string constants generated from Resources/Fonts/Violeta Fluent Icons.ttf
/// (built from Resources/Icons/*.svg via FluentUI.Svg2Font.Tools).
/// </summary>
[FontGlyphs("Resources/Fonts/Violeta Fluent Icons.ttf")]
public static partial class VioletaFontIconSymbols;

#if DEBUG

/// <summary>
/// This class is used to ensure that the VioletaFontIconSymbols class is initialized during debug builds.
/// </summary>
file static class VioletaFontIconSymbolsDebug
{
    static VioletaFontIconSymbolsDebug()
    {
        _ = VioletaFontIconSymbols.Edit;
    }
}

#endif
