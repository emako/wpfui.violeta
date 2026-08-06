using System.Windows;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Provides event data for the <see cref="SearchBox.QueryChanged"/> event.
/// Mirrors WinUI <c>Windows.UI.Xaml.Controls.SearchBoxQueryChangedEventArgs</c>.
/// </summary>
public sealed class SearchBoxQueryChangedEventArgs(RoutedEvent routedEvent, object source)
    : RoutedEventArgs(routedEvent, source)
{
    /// <summary>Gets the query text of the current search.</summary>
    public string QueryText { get; set; } = string.Empty;
}
