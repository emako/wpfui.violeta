using System.Windows;

namespace Wpf.Ui.Violeta.Controls;

public class MessageBoxClosedEventArgs : RoutedEventArgs
{
    internal MessageBoxClosedEventArgs(MessageBoxResult result)
    {
        Result = result;
    }

    public MessageBoxResult Result { get; }
}
