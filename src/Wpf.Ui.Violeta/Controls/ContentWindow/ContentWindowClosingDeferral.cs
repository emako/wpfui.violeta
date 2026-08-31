using System;

namespace Wpf.Ui.Violeta.Controls;

public sealed class ContentWindowClosingDeferral
{
    private readonly Action _handler;

    internal ContentWindowClosingDeferral(Action handler)
    {
        _handler = handler ?? throw new ArgumentNullException(nameof(handler));
    }

    public void Complete()
    {
        _handler();
    }
}
