using System.Windows;

namespace Wpf.Ui.Violeta.Controls;

public delegate void ItemClickEventHandler(object sender, ItemClickEventArgs e);

public sealed class ItemClickEventArgs : RoutedEventArgs
{
    public object? ClickedItem { get; internal set; }
}
