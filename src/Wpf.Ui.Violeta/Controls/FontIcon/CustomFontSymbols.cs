using System;

namespace Wpf.Ui.Controls;

/// <summary>
/// Violeta Fluent Icons
/// https://github.com/emako/fluentui-violeta-icons
/// </summary>
#if DEBUG // Pending the migration to FluentUI.FontGlyphs.SourceGenerator, this class is marked as obsolete and will be removed in a future release.
[Obsolete("Use FluentUI.FontGlyphs.SourceGenerator instead.")]
#endif
public sealed class CustomFontSymbols
{
    public const string Edit = "\xe900";
    public const string Empty = "\xe901";
    public const string Extension = "\xe902";
    public const string File = "\xe903";
}
