using System.Windows;

namespace Wpf.Ui.Violeta.Controls.Compat
{
    internal sealed class TappedRoutedEventArgs : RoutedEventArgs
    {
        public TappedRoutedEventArgs()
        {
        }

        //public Point GetPosition(UIElement relativeTo);

        //public PointerDeviceType PointerDeviceType { get; }

        internal int Timestamp { get; set; }
    }
}

