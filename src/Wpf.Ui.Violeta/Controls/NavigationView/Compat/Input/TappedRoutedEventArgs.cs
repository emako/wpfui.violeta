#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8618, CS8619, CS8625

using System.Windows;

namespace Wpf.Ui.Violeta.Controls.Compat;

internal sealed class TappedRoutedEventArgs : RoutedEventArgs
{
    public TappedRoutedEventArgs()
    {
    }

    //public Point GetPosition(UIElement relativeTo);

    //public PointerDeviceType PointerDeviceType { get; }

    internal int Timestamp { get; set; }
}
