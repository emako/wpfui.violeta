using FluentUI.FontGlyphs.SourceGenerator;

namespace Wpf.Ui.Violeta.Controls;

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
        _ = VioletaFontIconSymbols.UniE900;
    }
}

#endif
