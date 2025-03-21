﻿using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace Wpf.Ui.Controls;

/// <summary>
/// <seealso cref="Flyout"/>
/// </summary>
public static class FlyoutService
{
    public static Flyout GetFlyout(DependencyObject obj)
    {
        return (obj.GetValue(FlyoutProperty) as Flyout)!;
    }

    public static void SetFlyout(DependencyObject obj, Flyout value)
    {
        obj.SetValue(FlyoutProperty, value);
    }

    public static readonly DependencyProperty FlyoutProperty =
        DependencyProperty.RegisterAttached("Flyout", typeof(Flyout), typeof(FlyoutService), new(null, OnFlyoutChanged));

    public static void OnFlyoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FrameworkElement buttonExpected)
        {
            if (e.NewValue is Flyout flyout)
            {
                // Inherit data context.
                flyout.DataContext = buttonExpected.DataContext;

                buttonExpected.DataContextChanged -= ButtonExpectedDataContextChanged;
                buttonExpected.DataContextChanged += ButtonExpectedDataContextChanged;
            }

            // Binding click or leftmouse event to show flyout.
            {
                if (d is Button button)
                {
                    button.Click -= ShowFlyoutRequested;
                    button.Click += ShowFlyoutRequested;
                    return;
                }
            }
            {
                if (d is System.Windows.Controls.Button button)
                {
                    button.Click -= ShowFlyoutRequested;
                    button.Click += ShowFlyoutRequested;
                    return;
                }
            }
            {
                buttonExpected.MouseLeftButtonUp -= ShowFlyoutRequested;
                buttonExpected.MouseLeftButtonUp += ShowFlyoutRequested;
            }
        }
    }

    private static void ButtonExpectedDataContextChanged(object? sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is FrameworkElement buttonExpected)
        {
            if (GetFlyout(buttonExpected) is Flyout flyout)
            {
                // Inherit data context.
                flyout.DataContext = buttonExpected.DataContext;
            }
        }
    }

    private static void ShowFlyoutRequested(object? sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement buttonExpected)
        {
            ShowFlyout(buttonExpected);
        }
    }

    private static void ShowFlyoutRequested(object? sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement buttonExpected)
        {
            ShowFlyout(buttonExpected);
        }
    }

    public static void ShowFlyout(FrameworkElement buttonExpected)
    {
        if (GetFlyout(buttonExpected) is Flyout flyout)
        {
            // Get the flyout popup from Flyout control default template.
            flyout.ApplyTemplate();
            if (flyout.GetTemplateChild("PART_Popup") is System.Windows.Controls.Primitives.Popup popup)
            {
                // Inherit data context.
                popup.DataContext = flyout.DataContext;

                // Reset the popup placement.
                popup.PlacementTarget = buttonExpected;
                popup.Placement = flyout.Placement;

                // Remove the popup parent
                if (popup.Parent is System.Windows.Controls.Panel parent)
                {
                    parent.Children.Remove(popup);
                }

                // Set the flyout parent
                if (flyout.Parent is null)
                {
                    // Find nearest panel
                    if (buttonExpected.Parent is System.Windows.Controls.Panel parent2)
                    {
                        parent2.Children.Add(flyout);
                    }
                    // Once fallback to window top level
                    else if (Window.GetWindow(buttonExpected)?.Content is System.Windows.Controls.Panel parent3)
                    {
                        parent3.Children.Add(flyout);
                    }
                    else
                    {
                        // Flyout is not added to any parent.
                        // Flyout will be shown but the theme is not synced.
                        // See more https://github.com/emako/wpfui.violeta/issues/10.
                    }
                }

                // Following code is based on the Flyout control default template.
                // If default template is changed, this code will not work.
                // Check WPF-UI v3.0.5 since.
                if (flyout.Content is not null)
                {
                    UIElement? contentElement = flyout.Content as UIElement;
                    flyout.Content = null;
                    ((System.Windows.Controls.Border)popup.Child).Child = contentElement;
                }

                // Spoof the flyout opening state.
                flyout.IsOpen = popup.IsOpen = true;
            }
        }
    }

    public static void HideFlyout(FrameworkElement buttonExpected)
    {
        if (GetFlyout(buttonExpected) is Flyout flyout)
        {
            // Get the flyout popup from Flyout control default template.
            if (flyout.GetTemplateChild("PART_Popup") is System.Windows.Controls.Primitives.Popup popup)
            {
                // Spoof the flyout opening state.
                flyout.IsOpen = popup.IsOpen = false;
            }
        }
    }

    private static DependencyObject? GetTemplateChild(this FrameworkElement self, string childName)
    {
        MethodInfo? method = typeof(FrameworkElement)
            .GetMethod("GetTemplateChild", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod);

        return method?.Invoke(self, [childName]) as DependencyObject;
    }
}
