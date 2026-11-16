using System;
using Wpf.Ui.Violeta.Controls;

namespace Wpf.Ui.Controls;

/// <summary>
/// Violeta Fluent Icons. Values are the glyphs generated from <c>Resources/Icons</c>.
/// </summary>
#if DEBUG // Pending the migration to FluentUI.FontGlyphs.SourceGenerator, this class is marked as obsolete and will be removed in a future release.
[Obsolete("Use VioletaFontIconSymbols instead.")]
#endif
public sealed class CustomFontSymbols
{
    public const string Edit = VioletaFontIconSymbols.Edit;
    public const string Empty = VioletaFontIconSymbols.Empty;
    public const string Extension = VioletaFontIconSymbols.Extension;
    public const string File = VioletaFontIconSymbols.File;
}
