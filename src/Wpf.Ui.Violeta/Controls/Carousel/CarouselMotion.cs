namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Transition motion used when changing the active carousel page.
/// Mirrors Fluent UI React <c>CarouselMotion</c>.
/// </summary>
public enum CarouselMotion
{
    /// <summary>Horizontal slide animation (default).</summary>
    Slide,

    /// <summary>Cross-fade between cards.</summary>
    Fade,
}
