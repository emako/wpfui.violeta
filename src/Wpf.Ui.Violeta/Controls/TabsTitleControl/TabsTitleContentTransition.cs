namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Selects how <see cref="TabsTitleControl"/> transitions content when the selected tab changes.
/// </summary>
public enum TabsTitleContentTransition
{
    /// <summary>Swap content immediately with no animation.</summary>
    None = 0,

    /// <summary>Slide and fade content in the selection direction.</summary>
    Slide = 1,
}
