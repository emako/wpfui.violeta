using System.Windows;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Event data for <see cref="TabsTitleControl.CloseTab"/>.
/// Set <see cref="RoutedEventArgs.Handled"/> to <see langword="true"/> to cancel removal.
/// </summary>
public class TabsTitleCloseRoutedEventArgs : RoutedEventArgs
{
    /// <summary>Gets the tab item being closed.</summary>
    public TabsTitleControlItem TabItem { get; }

    public TabsTitleCloseRoutedEventArgs(RoutedEvent routedEvent, TabsTitleControlItem tabItem)
        : base(routedEvent, tabItem)
    {
        TabItem = tabItem;
    }
}
