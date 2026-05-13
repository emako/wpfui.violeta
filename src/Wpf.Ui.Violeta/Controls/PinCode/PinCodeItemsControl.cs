using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>Internal items host that generates <see cref="PinCodeItem"/> containers.</summary>
public class PinCodeItemsControl : ItemsControl
{
    static PinCodeItemsControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(PinCodeItemsControl),
            new FrameworkPropertyMetadata(typeof(PinCodeItemsControl)));
    }

    protected override bool IsItemItsOwnContainerOverride(object item) => item is PinCodeItem;

    protected override DependencyObject GetContainerForItemOverride() => new PinCodeItem();

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);

        // Suppress Left/Right so WPF's default focus navigation doesn't interfere.
        // PinCode handles these keys at a higher level (tunneling fires PinCode first).
        if (e.Key is Key.Left or Key.Right)
            e.Handled = true;
    }
}
