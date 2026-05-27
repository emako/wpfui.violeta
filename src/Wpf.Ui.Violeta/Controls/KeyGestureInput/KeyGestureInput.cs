using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Button = Wpf.Ui.Controls.Button;
using TextBox = Wpf.Ui.Controls.TextBox;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// A control that captures keyboard shortcuts by listening to key events.
/// Inherits from <see cref="TextBox"/> and uses its
/// <see cref="System.Windows.Controls.TextBox.Text"/> property to display the captured gesture.
/// </summary>
[TemplatePart(Name = ClearButtonPartName, Type = typeof(Button))]
public class KeyGestureInput : TextBox
{
    public const string ClearButtonPartName = "ClearButton";

    static KeyGestureInput()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(KeyGestureInput),
            new FrameworkPropertyMetadata(typeof(KeyGestureInput)));

        ClearButtonEnabledProperty.OverrideMetadata(
            typeof(KeyGestureInput),
            new FrameworkPropertyMetadata(
                true,
                OnClearButtonEnabledChanged));
    }

    public KeyGestureInput()
    {
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        UpdateClearButtonEnabled();
    }

    #region Gesture

    public static readonly DependencyProperty GestureProperty =
        DependencyProperty.Register(
            nameof(Gesture),
            typeof(KeyGestureValue?),
            typeof(KeyGestureInput),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnGestureChanged));

    public KeyGestureValue? Gesture
    {
        get => (KeyGestureValue?)GetValue(GestureProperty);
        set => SetValue(GestureProperty, value);
    }

    private static void OnGestureChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is KeyGestureInput control)
        {
            control.SetValue(IsEmptyPropertyKey, e.NewValue is null);
            control.SetCurrentValue(TextProperty, ((KeyGestureValue?)e.NewValue)?.ToString() ?? string.Empty);
        }
    }

    #endregion Gesture

    #region IsEmpty (read-only)

    private static readonly DependencyPropertyKey IsEmptyPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(IsEmpty),
            typeof(bool),
            typeof(KeyGestureInput),
            new PropertyMetadata(true));

    public static readonly DependencyProperty IsEmptyProperty = IsEmptyPropertyKey.DependencyProperty;

    public bool IsEmpty => (bool)GetValue(IsEmptyProperty);

    #endregion IsEmpty (read-only)

    #region AcceptableKeys

    public static readonly DependencyProperty AcceptableKeysProperty =
        DependencyProperty.Register(
            nameof(AcceptableKeys),
            typeof(IList<Key>),
            typeof(KeyGestureInput),
            new PropertyMetadata(null));

    /// <summary>
    /// When set, only keys contained in this list will be recorded.
    /// </summary>
    public IList<Key>? AcceptableKeys
    {
        get => (IList<Key>?)GetValue(AcceptableKeysProperty);
        set => SetValue(AcceptableKeysProperty, value);
    }

    #endregion AcceptableKeys

    #region ConsiderKeyModifiers

    public static readonly DependencyProperty ConsiderKeyModifiersProperty =
        DependencyProperty.Register(
            nameof(ConsiderKeyModifiers),
            typeof(bool),
            typeof(KeyGestureInput),
            new PropertyMetadata(true));

    /// <summary>
    /// When <see langword="true"/> (default), modifier keys (Ctrl/Alt/Shift/Win) are
    /// included in the recorded gesture.
    /// </summary>
    public bool ConsiderKeyModifiers
    {
        get => (bool)GetValue(ConsiderKeyModifiersProperty);
        set => SetValue(ConsiderKeyModifiersProperty, value);
    }

    #endregion ConsiderKeyModifiers

    public static void OnClearButtonEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is KeyGestureInput self)
        {
            self.UpdateClearButtonEnabled();
        }
    }

    protected void UpdateClearButtonEnabled()
    {
        if (Template is not null)
        {
            Trigger? isReadOnlyOnTrueTrigger = Template.Triggers
                .Where(trigger => trigger is Trigger)
                .Select(trigger => trigger as Trigger)
                .FirstOrDefault(trigger => trigger?.Property?.Name == nameof(IsReadOnly)
                    && trigger.Value is bool value && value == true);

            if (isReadOnlyOnTrueTrigger is not null)
            {
                // We can't `Template.Triggers.Remove(isReadOnlyOnTrueTrigger);` method
                // which will caused `System.InvalidOperationException`.
                if (GetTemplateChild(ClearButtonPartName) is Button clearButton)
                {
                    foreach (SetterBase setterBase in isReadOnlyOnTrueTrigger.Setters)
                    {
                        if (setterBase is Setter setter && setter.TargetName is ClearButtonPartName)
                        {
                            if (setter.Property.Name == nameof(Visibility))
                            {
                                // Setting property directly is the highest priority.
                                clearButton?.Visibility = ClearButtonEnabled ? Visibility.Visible : Visibility.Collapsed;
                            }
                            else if (setter.Property.Name == nameof(Margin))
                            {
                                clearButton?.Margin = new Thickness(0, 0, 4, 0);
                            }
                        }
                    }
                }
            }
        }
    }

    // -------------------------------------------------------------------------

    protected override void OnKeyDown(KeyEventArgs e)
    {
        // In WPF, Alt+key sets e.Key = Key.System; the actual key is in e.SystemKey.
        var key = e.Key == Key.System ? e.SystemKey : e.Key;

        // Ignore IME-processed and unknown keys.
        if (key is Key.ImeProcessed or Key.None) return;

        // Ignore pure modifier key presses.
        if (IsModifierKey(key)) return;

        // Filter by acceptable keys if specified.
        if (AcceptableKeys is { } allowed && !allowed.Contains(key)) return;

        var gesture = ConsiderKeyModifiers
            ? new KeyGestureValue(key, e.KeyboardDevice.Modifiers)
            : new KeyGestureValue(key, ModifierKeys.None);

        SetCurrentValue(GestureProperty, (KeyGestureValue?)gesture);
        e.Handled = true;
    }

    protected override void OnPreviewTextInput(TextCompositionEventArgs e)
    {
        // Block all direct text input; the control only accepts key gestures.
        e.Handled = true;
    }

    /// <summary>Clears the current gesture.</summary>
    public new void Clear() => SetCurrentValue(GestureProperty, (KeyGestureValue?)null);

    // -------------------------------------------------------------------------

    private static bool IsModifierKey(Key key) =>
        key is Key.LeftCtrl or Key.RightCtrl
            or Key.LeftAlt or Key.RightAlt
            or Key.LeftShift or Key.RightShift
            or Key.LWin or Key.RWin;
}
