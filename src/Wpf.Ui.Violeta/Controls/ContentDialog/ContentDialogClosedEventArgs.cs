using System;
using Wpf.Ui.Controls;

namespace Wpf.Ui.Violeta.Controls;

public class ContentDialogClosedEventArgs : EventArgs
{
    internal ContentDialogClosedEventArgs(ContentDialogResult result)
    {
        Result = result;
    }

    public ContentDialogResult Result { get; }
}
