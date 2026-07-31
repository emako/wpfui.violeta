using System;

namespace Wpf.Ui.Violeta.Controls;

public sealed class TeachingTipClosedEventArgs : EventArgs
{
    internal TeachingTipClosedEventArgs(TeachingTipCloseReason reason)
    {
        Reason = reason;
    }

    public TeachingTipCloseReason Reason { get; }
}
