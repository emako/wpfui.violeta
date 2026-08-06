using System;
using System.ComponentModel;
using System.Windows;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Attached helper that styles the Flyout <see cref="System.Windows.Controls.ContextMenu"/>
/// for upstream <c>Wpf.Ui</c> DropDownButton / SplitButton instances via Hotfix styles.
/// </summary>
public static class DropDownFlyoutAssist
{
    public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
        "IsEnabled",
        typeof(bool),
        typeof(DropDownFlyoutAssist),
        new PropertyMetadata(false, OnIsEnabledChanged));

    public static bool GetIsEnabled(DependencyObject element) =>
        (bool)element.GetValue(IsEnabledProperty);

    public static void SetIsEnabled(DependencyObject element, bool value) =>
        element.SetValue(IsEnabledProperty, value);

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement host)
        {
            return;
        }

        var flyoutProperty = ResolveFlyoutProperty(host);
        if (flyoutProperty is null)
        {
            return;
        }

        var descriptor = DependencyPropertyDescriptor.FromProperty(flyoutProperty, host.GetType());

        if ((bool)e.NewValue)
        {
            descriptor.AddValueChanged(host, OnFlyoutChanged);
            host.Loaded += OnHostLoaded;
            Apply(host);
        }
        else
        {
            descriptor.RemoveValueChanged(host, OnFlyoutChanged);
            host.Loaded -= OnHostLoaded;
        }
    }

    private static void OnHostLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement host)
        {
            Apply(host);
        }
    }

    private static void OnFlyoutChanged(object? sender, EventArgs e)
    {
        if (sender is FrameworkElement host)
        {
            Apply(host);
        }
    }

    private static void Apply(FrameworkElement host)
    {
        object? flyout = host switch
        {
            Wpf.Ui.Controls.DropDownButton dropDownButton => dropDownButton.Flyout,
            Wpf.Ui.Controls.SplitButton splitButton => splitButton.Flyout,
            _ => null,
        };

        DropDownFlyoutHelper.ApplyFlyoutContextMenuStyle(host, flyout);
    }

    private static DependencyProperty? ResolveFlyoutProperty(FrameworkElement host) =>
        host switch
        {
            Wpf.Ui.Controls.DropDownButton => Wpf.Ui.Controls.DropDownButton.FlyoutProperty,
            Wpf.Ui.Controls.SplitButton => Wpf.Ui.Controls.SplitButton.FlyoutProperty,
            _ => null,
        };
}
