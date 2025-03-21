﻿using System.Diagnostics;
using System.Windows;

namespace Wpf.Ui.Violeta.Controls;

public sealed class ContentDialogClosingEventArgs : RoutedEventArgs
{
    private ContentDialogClosingDeferral _deferral = null!;
    private int _deferralCount;

    internal ContentDialogClosingEventArgs(ContentDialogResult result)
    {
        Result = result;
    }

    public bool Cancel { get; set; }

    public ContentDialogResult Result { get; }

    public ContentDialogClosingDeferral GetDeferral()
    {
        _deferralCount++;

        return new ContentDialogClosingDeferral(() =>
        {
            DecrementDeferralCount();
        });
    }

    internal void SetDeferral(ContentDialogClosingDeferral deferral)
    {
        _deferral = deferral;
    }

    internal void DecrementDeferralCount()
    {
        Debug.Assert(_deferralCount > 0);
        _deferralCount--;
        if (_deferralCount == 0)
        {
            _deferral.Complete();
        }
    }

    internal void IncrementDeferralCount()
    {
        _deferralCount++;
    }
}
