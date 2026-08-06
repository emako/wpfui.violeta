using System.Windows;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Provides event data for the <see cref="SearchBox.QuerySubmitted"/> event.
/// </summary>
public sealed class SearchBoxQuerySubmittedEventArgs(RoutedEvent routedEvent, object source)
    : RoutedEventArgs(routedEvent, source)
{
    /// <summary>Gets the query text that was submitted.</summary>
    public string QueryText { get; set; } = string.Empty;
}
