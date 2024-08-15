using System.Diagnostics;
using System.Windows;

namespace Wpf.Ui.Violeta.Controls;

public class MessageBoxButtonClickEventArgs : RoutedEventArgs
{
    private MessageBoxButtonClickDeferral _deferral = null!;
    private int _deferralCount;

    internal MessageBoxButtonClickEventArgs()
    {
    }

    public bool Cancel { get; set; }

    public MessageBoxButtonClickDeferral GetDeferral()
    {
        _deferralCount++;

        return new MessageBoxButtonClickDeferral(() =>
        {
            DecrementDeferralCount();
        });
    }

    internal void SetDeferral(MessageBoxButtonClickDeferral deferral)
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
