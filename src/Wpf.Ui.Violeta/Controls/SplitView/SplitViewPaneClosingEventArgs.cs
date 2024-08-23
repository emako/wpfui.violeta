using System;

namespace Wpf.Ui.Violeta.Controls;

public sealed class SplitViewPaneClosingEventArgs : EventArgs
{
    internal SplitViewPaneClosingEventArgs()
    {
    }

    public bool Cancel { get; set; }
}