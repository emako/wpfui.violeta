namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Defines a specific tab/page (subview) within the <see cref="ColorView"/>.
/// Indexed to match the default control template ordering.
/// </summary>
public enum ColorViewTab
{
    /// <summary>Color spectrum subview with a box/ring spectrum and sliders.</summary>
    Spectrum = 0,

    /// <summary>Color palette subview with a grid of selectable colors.</summary>
    Palette = 1,

    /// <summary>Components subview with sliders and numeric input boxes.</summary>
    Components = 2,
}
