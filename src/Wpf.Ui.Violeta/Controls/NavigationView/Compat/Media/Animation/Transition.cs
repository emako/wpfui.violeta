#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8618, CS8619, CS8625

using System.Windows;

namespace Wpf.Ui.Violeta.Controls.Compat;

/// <summary>
/// Represents a visual behavior that occurs for predefined actions or state changes.
/// Specific theme transitions (various Transition derived classes) can be applied
/// to individual elements using the UIElement.Transitions property, or applied for
/// scenario-specific theme transition properties such as ContentControl.ContentTransitions.
/// </summary>
public class Transition : DependencyObject
{
    private protected Transition()
    {
    }
}
