using System;

namespace Wpf.Ui.Violeta.Controls;

public sealed class ContentWindowResultEventArgs(ContentWindowResult dialogResult) : EventArgs
{
    public ContentWindowResult DialogResult { get; set; } = dialogResult;

    public bool Handled { get; set; } = false;
}
