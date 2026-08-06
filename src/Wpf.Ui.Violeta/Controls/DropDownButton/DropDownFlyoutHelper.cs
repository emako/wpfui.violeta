using System.Windows;
using System.Windows.Controls;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Applies <c>DropDownFlyoutContextMenuStyle</c> to DropDownButton / SplitButton flyouts
/// so the menu does not slide over the host control (upstream UiContextMenu uses From=-90).
/// </summary>
internal static class DropDownFlyoutHelper
{
    internal const string ContextMenuStyleKey = "DropDownFlyoutContextMenuStyle";

    /// <summary>
    /// Applies the flyout ContextMenu style when <paramref name="flyout"/> is a
    /// <see cref="ContextMenu"/> without an explicit local <see cref="FrameworkElement.Style"/>.
    /// </summary>
    public static void ApplyFlyoutContextMenuStyle(FrameworkElement host, object? flyout)
    {
        if (flyout is not ContextMenu contextMenu)
        {
            return;
        }

        // Respect an author-specified Style on the Flyout ContextMenu.
        if (contextMenu.ReadLocalValue(FrameworkElement.StyleProperty) != DependencyProperty.UnsetValue)
        {
            return;
        }

        var style =
            host.TryFindResource(ContextMenuStyleKey) as Style
            ?? Application.Current?.TryFindResource(ContextMenuStyleKey) as Style;

        if (style is null)
        {
            return;
        }

        contextMenu.Style = style;
    }
}
