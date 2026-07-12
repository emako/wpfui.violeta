#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8618, CS8619, CS8625

namespace Wpf.Ui.Violeta.Controls.Compat;

/// <summary>
/// Defines constants that specify input-specific transition animations that are
/// part of the default template for ScrollBar.
/// </summary>
public enum ScrollingIndicatorMode
{
    /// <summary>
    /// Do not use input-specific transitions.
    /// </summary>
    None = 0,

    /// <summary>
    /// Use input-specific transitions that are appropriate for touch input.
    /// </summary>
    TouchIndicator = 1,

    /// <summary>
    /// Use input-specific transitions that are appropriate for mouse input.
    /// </summary>
    MouseIndicator = 2,
}
