﻿using System;

namespace Wpf.Ui.Violeta.Controls;

public sealed class ContentDialogButtonClickDeferral
{
    private readonly Action _handler;

    internal ContentDialogButtonClickDeferral(Action handler)
    {
        _handler = handler ?? throw new ArgumentNullException(nameof(handler));
    }

    public void Complete()
    {
        _handler();
    }
}
