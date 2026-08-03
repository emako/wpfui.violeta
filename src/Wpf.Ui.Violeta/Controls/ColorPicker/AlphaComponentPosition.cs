namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Defines the position of a color's alpha component relative to all other components.
/// </summary>
public enum AlphaComponentPosition
{
    /// <summary>
    /// The alpha component occurs before all other components (e.g. #AARRGGBB / ARGB).
    /// </summary>
    Leading,

    /// <summary>
    /// The alpha component occurs after all other components (e.g. #RRGGBBAA / RGBA).
    /// </summary>
    Trailing,
}
