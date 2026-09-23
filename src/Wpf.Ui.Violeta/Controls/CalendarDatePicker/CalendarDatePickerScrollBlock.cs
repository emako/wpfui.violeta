using System;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Attached behavior for the built-in <see cref="System.Windows.Controls.DatePicker"/> when styled
/// as a CalendarDatePicker. While the drop-down is open it (1) blocks mouse-wheel scrolling of
/// the host behind the popup (ComboBox-like) and (2) after close, clears keyboard focus that
/// WPF would otherwise restore onto the text box — so toggle via the drop-down button matches
/// <c>ui:CalendarDatePicker</c> (open → focused, close → unfocused). Enabled from the
/// CalendarDatePicker style.
/// </summary>
public static class CalendarDatePickerScrollBlock
{
    private sealed class State
    {
        public Popup? Popup;
        public Window? ParentWindow;
        public bool HandlerRegistered;
        public bool TextBoxFocusedOnOpen;
        public EventHandler? OpenedHandler;
        public EventHandler? ClosedHandler;
        public MouseWheelEventHandler? WheelHandler;
    }

    private static readonly ConditionalWeakTable<System.Windows.Controls.DatePicker, State> States = new();

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
            state.TextBoxFocusedOnOpen =
                picker.Template?.FindName("PART_TextBox", picker) is UIElement textBox
                && textBox.IsKeyboardFocusWithin;

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

            // DatePicker.PopUp_Closed MoveFocus'es back into PART_TextBox, which makes a second
            // click on the drop-down button leave the control focused — opposite of Button-based
            // ui:CalendarDatePicker. Clear that restored focus when the text box was not focused
            // before the drop-down opened (e.g. opened via the calendar button).
            if (state.TextBoxFocusedOnOpen)
                return;

            picker.Dispatcher.BeginInvoke(
                DispatcherPriority.Input,
                () =>
                {
                    if (picker.IsDropDownOpen)
                        return;

                    if (Keyboard.FocusedElement is DependencyObject focused
                        && IsVisualDescendantOf(focused, picker))
                    {
                        Keyboard.ClearFocus();
                    }
                });
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
