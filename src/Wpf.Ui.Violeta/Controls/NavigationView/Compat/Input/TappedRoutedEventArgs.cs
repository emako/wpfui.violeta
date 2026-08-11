using System.Windows;

namespace Wpf.Ui.Violeta.Controls.Compat;

internal sealed class TappedRoutedEventArgs : RoutedEventArgs
{
    public TappedRoutedEventArgs()
    {
    }

    internal int Timestamp { get; set; }
}
