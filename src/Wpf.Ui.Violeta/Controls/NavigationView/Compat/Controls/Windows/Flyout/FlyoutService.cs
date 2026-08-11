using System.Windows;
using System.Windows.Controls;

namespace Wpf.Ui.Violeta.Controls.Compat;

public static class FlyoutService
{
    public static readonly DependencyProperty FlyoutProperty =
        DependencyProperty.RegisterAttached(
            "Flyout",
            typeof(FlyoutBase),
            typeof(FlyoutService),
            new PropertyMetadata(OnFlyoutChanged));

    public static FlyoutBase GetFlyout(Button button)
    {
        return (FlyoutBase)button.GetValue(FlyoutProperty);
    }

    public static void SetFlyout(Button button, FlyoutBase value)
    {
        button.SetValue(FlyoutProperty, value);
    }

    private static void OnFlyoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var button = (Button)d;

        if (e.OldValue is FlyoutBase oldFlyout)
        {
            button.Click -= OnButtonClick;
            button.MouseRightButtonUp -= Button_MouseRightButtonUp;
        }

        if (e.NewValue is FlyoutBase newFlyout)
        {
            button.Click += OnButtonClick;
            button.MouseRightButtonUp += Button_MouseRightButtonUp;
        }
    }

    private static void Button_MouseRightButtonUp(object? sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        ButtonFlyoutOpening(sender, FlyoutOpeningMode.RightMouseButtonUp);
    }

    private static void OnButtonClick(object? sender, RoutedEventArgs e)
    {
        ButtonFlyoutOpening(sender, FlyoutOpeningMode.Click);
    }

    private static void ButtonFlyoutOpening(object? sender, FlyoutOpeningMode requested)
    {
        var button = (Button)sender!;
        var open = GetFlyoutOpeningMode(button);
        if (open.HasFlag(requested))
        {
            ButtonFlyoutOpening(button);
        }
    }

    private static void ButtonFlyoutOpening(Button button)
    {
        _ = GetFlyoutOpeningMode(button);
        var flyout = GetFlyout(button);
        flyout?.ShowAt(button);
    }

    public static readonly DependencyProperty FlyoutOpeningModeProperty =
        DependencyProperty.RegisterAttached(
            "FlyoutOpeningMode",
            typeof(FlyoutOpeningMode),
            typeof(FlyoutService),
            new PropertyMetadata(FlyoutOpeningMode.Click));

    public static FlyoutOpeningMode GetFlyoutOpeningMode(Button button)
    {
        return (FlyoutOpeningMode)button.GetValue(FlyoutOpeningModeProperty);
    }

    public static void SetFlyoutOpeningMode(Button button, FlyoutOpeningMode value)
    {
        button.SetValue(FlyoutOpeningModeProperty, value);
    }
}

public enum FlyoutOpeningMode
{
    None = 0,
    Click = 1,
    RightMouseButtonUp = 2,
}
