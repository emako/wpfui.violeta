using System;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Represents a deferral used by <see cref="TeachingTipClosingEventArgs"/>.
/// </summary>
public sealed class TeachingTipDeferral
{
    private readonly Action _handler;
    private bool _completed;

    internal TeachingTipDeferral(Action handler)
    {
        _handler = handler ?? throw new ArgumentNullException(nameof(handler));
    }

    public void Complete()
    {
        if (_completed)
        {
            return;
        }

        _completed = true;
        _handler();
    }
}
