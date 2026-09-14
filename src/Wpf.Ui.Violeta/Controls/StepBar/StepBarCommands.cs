using System.Windows.Input;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Commands used by <see cref="StepBar"/> navigation.
/// </summary>
public static class StepBarCommands
{
    /// <summary>Advance to the next step.</summary>
    public static readonly RoutedCommand Next = new(nameof(Next), typeof(StepBarCommands));

    /// <summary>Go back to the previous step.</summary>
    public static readonly RoutedCommand Prev = new(nameof(Prev), typeof(StepBarCommands));
}
