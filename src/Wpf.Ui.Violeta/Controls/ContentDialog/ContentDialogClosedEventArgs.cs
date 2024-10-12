using System.Windows;

namespace Wpf.Ui.Violeta.Controls;

public class ContentDialogClosedEventArgs : RoutedEventArgs
{
    internal ContentDialogClosedEventArgs(ContentDialogResult result)
    {
        Result = result;
    }

    public ContentDialogResult Result { get; }
}
