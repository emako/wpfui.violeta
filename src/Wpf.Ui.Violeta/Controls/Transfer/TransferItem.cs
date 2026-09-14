using System.Windows;
using System.Windows.Controls;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Item container for <see cref="Transfer"/> with ListView-style selection chrome;
/// transferred items are collapsed on the source side.
/// </summary>
public class TransferItem : ListBoxItem
{
    public static readonly DependencyProperty IsTransferredProperty =
        DependencyProperty.Register(
            nameof(IsTransferred),
            typeof(bool),
            typeof(TransferItem),
            new PropertyMetadata(false));

    public bool IsTransferred
    {
        get => (bool)GetValue(IsTransferredProperty);
        set => SetValue(IsTransferredProperty, value);
    }
}
