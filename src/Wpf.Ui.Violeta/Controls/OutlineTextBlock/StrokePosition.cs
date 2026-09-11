namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Specifies how the stroke of an <see cref="OutlineTextBlock"/> is positioned relative to the glyph outline.
/// </summary>
public enum StrokePosition
{
    /// <summary>Stroke is centered on the glyph outline.</summary>
    Center,

    /// <summary>Stroke is drawn outside the glyph fill.</summary>
    Outside,

    /// <summary>Stroke is drawn inside the glyph fill.</summary>
    Inside,
}
