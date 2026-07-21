using System;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Attached behavior that, while the built-in <see cref="DatePicker"/>'s drop-down popup
/// (styled as a CalendarDatePicker) is open, blocks mouse-wheel events that don't occur over
/// the popup content itself — so the host panel behind the control can't be scrolled,
/// matching the standard <see cref="ComboBox"/> drop-down behavior (which achieves this via
/// mouse capture). Enabled from the CalendarDatePicker style since there's no dedicated
/// control class to hold this logic.
/// </summary>
public static class CalendarDatePickerScrollBlock
{
    private sealed class State
    {
        public Popup? Popup;
        public Window? ParentWindow;
        public bool HandlerRegistered;
        public EventHandler? OpenedHandler;
        public EventHandler? ClosedHandler;
        public MouseWheelEventHandler? WheelHandler;
    }

    private static readonly ConditionalWeakTable<System.Windows.Controls.DatePicker, State> States = [];

    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(CalendarDatePickerScrollBlock),
            new PropertyMetadata(false, OnIsEnabledChanged));

    public static bool GetIsEnabled(DependencyObject obj) => (bool)obj.GetValue(IsEnabledProperty);

    public static void SetIsEnabled(DependencyObject obj, bool value) => obj.SetValue(IsEnabledProperty, value);

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not System.Windows.Controls.DatePicker picker)
            return;

        if ((bool)e.NewValue)
        {
            picker.Loaded += OnPickerLoaded;
            picker.Unloaded += OnPickerUnloaded;
            if (picker.IsLoaded)
                Hook(picker);
        }
        else
        {
            picker.Loaded -= OnPickerLoaded;
            picker.Unloaded -= OnPickerUnloaded;
            Unhook(picker);
        }
    }

    private static void OnPickerLoaded(object sender, RoutedEventArgs e) => Hook((System.Windows.Controls.DatePicker)sender);

    private static void OnPickerUnloaded(object sender, RoutedEventArgs e) => Unhook((System.Windows.Controls.DatePicker)sender);

    private static void Hook(System.Windows.Controls.DatePicker picker)
    {
        picker.ApplyTemplate();
        if (picker.Template?.FindName("PART_Popup", picker) is not Popup popup)
            return;

        var state = States.GetOrCreateValue(picker);
        if (ReferenceEquals(state.Popup, popup))
            return;

        if (state.Popup is not null)
            Unsubscribe(state);

        state.Popup = popup;

        state.OpenedHandler = (_, _) =>
        {
            state.ParentWindow ??= Window.GetWindow(picker);
            if (state.ParentWindow is not null && !state.HandlerRegistered)
            {
                state.WheelHandler ??= (_, e2) => OnWindowPreviewMouseWheel(popup, e2);
                state.ParentWindow.PreviewMouseWheel += state.WheelHandler;
                state.HandlerRegistered = true;
            }
        };
        state.ClosedHandler = (_, _) =>
        {
            if (state.ParentWindow is not null && state.HandlerRegistered && state.WheelHandler is not null)
            {
                state.ParentWindow.PreviewMouseWheel -= state.WheelHandler;
                state.HandlerRegistered = false;
            }
        };

        popup.Opened += state.OpenedHandler;
        popup.Closed += state.ClosedHandler;
    }

    private static void Unhook(System.Windows.Controls.DatePicker picker)
    {
        if (States.TryGetValue(picker, out var state))
        {
            Unsubscribe(state);
            States.Remove(picker);
        }
    }

    private static void Unsubscribe(State state)
    {
        if (state.Popup is not null)
        {
            if (state.OpenedHandler is not null)
                state.Popup.Opened -= state.OpenedHandler;
            if (state.ClosedHandler is not null)
                state.Popup.Closed -= state.ClosedHandler;
        }
        if (state.ParentWindow is not null && state.HandlerRegistered && state.WheelHandler is not null)
            state.ParentWindow.PreviewMouseWheel -= state.WheelHandler;
        state.HandlerRegistered = false;
    }

    private static void OnWindowPreviewMouseWheel(Popup popup, MouseWheelEventArgs e)
    {
        if (e.OriginalSource is not DependencyObject target)
            return;

        if (popup.Child is UIElement popupChild && IsVisualDescendantOf(target, popupChild))
            return;

        e.Handled = true;
    }

    private static bool IsVisualDescendantOf(DependencyObject element, DependencyObject ancestor)
    {
        DependencyObject? current = element;
        while (current is not null)
        {
            if (current == ancestor)
                return true;
            current = VisualTreeHelper.GetParent(current);
        }
        return false;
    }
}
