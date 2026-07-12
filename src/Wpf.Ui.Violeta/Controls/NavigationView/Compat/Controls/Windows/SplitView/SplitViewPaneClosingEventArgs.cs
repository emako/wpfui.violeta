#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8618, CS8619, CS8625

using System;

namespace Wpf.Ui.Violeta.Controls.Compat;

public sealed class SplitViewPaneClosingEventArgs : EventArgs
{
    internal SplitViewPaneClosingEventArgs()
    {
    }

    public bool Cancel { get; set; }
}
