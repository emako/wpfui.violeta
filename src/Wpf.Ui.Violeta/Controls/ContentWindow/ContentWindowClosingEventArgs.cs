using System.Diagnostics;
using System.Windows;

namespace Wpf.Ui.Violeta.Controls;

public sealed class ContentWindowClosingEventArgs : RoutedEventArgs
{
    private ContentWindowClosingDeferral _deferral = null!;
    private int _deferralCount;

    internal ContentWindowClosingEventArgs(ContentWindowResult result)
    {
        Result = result;
    }

    public bool Cancel { get; set; }

    public ContentWindowResult Result { get; }

    public ContentWindowClosingDeferral GetDeferral()
    {
        _deferralCount++;

        return new ContentWindowClosingDeferral(() =>
        {
            DecrementDeferralCount();
        });
    }

    internal void SetDeferral(ContentWindowClosingDeferral deferral)
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
