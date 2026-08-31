using System.Windows;

namespace Wpf.Ui.Violeta.Controls;

public class ContentWindowClosedEventArgs : RoutedEventArgs
{
    internal ContentWindowClosedEventArgs(ContentWindowResult result)
    {
        Result = result;
    }

    public ContentWindowResult Result { get; }
}
