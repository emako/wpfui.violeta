using System;

namespace Wpf.Ui.Violeta.Controls;

public sealed class ContentDialogClosingDeferral
{
    private readonly Action _handler;

    internal ContentDialogClosingDeferral(Action handler)
    {
        _handler = handler ?? throw new ArgumentNullException(nameof(handler));
    }

    public void Complete()
    {
        _handler();
    }
}
