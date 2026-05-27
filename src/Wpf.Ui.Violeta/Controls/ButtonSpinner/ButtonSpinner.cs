using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Indicates on which side of the content the spinner buttons are placed.
/// Mirrors Semi.Avalonia's <c>ButtonSpinnerLocation</c>.
/// </summary>
public enum ButtonSpinnerLocation
{
    /// <summary>Buttons appear to the right of the content (default).</summary>
    Right,

    /// <summary>Buttons appear to the left of the content.</summary>
    Left,
}

/// <summary>
/// A ContentControl that wraps arbitrary content and exposes increase / decrease spin buttons.
/// Mirrors Semi.Avalonia's <c>ButtonSpinner</c>.
/// </summary>
[TemplatePart(Name = PART_IncreaseButton, Type = typeof(RepeatButton))]
[TemplatePart(Name = PART_DecreaseButton, Type = typeof(RepeatButton))]
public class ButtonSpinner : ContentControl
{
    public const string PART_IncreaseButton = "PART_IncreaseButton";
    public const string PART_DecreaseButton = "PART_DecreaseButton";

    private RepeatButton? _increaseButton;
    private RepeatButton? _decreaseButton;

    static ButtonSpinner()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ButtonSpinner),
            new FrameworkPropertyMetadata(typeof(ButtonSpinner)));
    }

    // --- ShowButtonSpinner ----------------------------------------------------

    public static readonly DependencyProperty ShowButtonSpinnerProperty =
        DependencyProperty.Register(
            nameof(ShowButtonSpinner),
            typeof(bool),
            typeof(ButtonSpinner),
            new PropertyMetadata(true));

    /// <summary>When <see langword="false"/> the spin buttons are hidden.</summary>
    public bool ShowButtonSpinner
    {
        get => (bool)GetValue(ShowButtonSpinnerProperty);
        set => SetValue(ShowButtonSpinnerProperty, value);
    }

    // --- AllowSpin ------------------------------------------------------------

    public static readonly DependencyProperty AllowSpinProperty =
        DependencyProperty.Register(
            nameof(AllowSpin),
            typeof(bool),
            typeof(ButtonSpinner),
            new PropertyMetadata(true));

    /// <summary>When <see langword="false"/> clicking the buttons has no effect.</summary>
    public bool AllowSpin
    {
        get => (bool)GetValue(AllowSpinProperty);
        set => SetValue(AllowSpinProperty, value);
    }

    // --- ButtonSpinnerLocation ------------------------------------------------

    public static readonly DependencyProperty ButtonSpinnerLocationProperty =
        DependencyProperty.Register(
            nameof(ButtonSpinnerLocation),
            typeof(ButtonSpinnerLocation),
            typeof(ButtonSpinner),
            new PropertyMetadata(ButtonSpinnerLocation.Right));

    /// <summary>Controls whether the buttons appear to the left or right of the content.</summary>
    public ButtonSpinnerLocation ButtonSpinnerLocation
    {
        get => (ButtonSpinnerLocation)GetValue(ButtonSpinnerLocationProperty);
        set => SetValue(ButtonSpinnerLocationProperty, value);
    }

    // --- Spin routed event ----------------------------------------------------

    public static readonly RoutedEvent SpinEvent =
        EventManager.RegisterRoutedEvent(
            nameof(Spin),
            RoutingStrategy.Bubble,
            typeof(EventHandler<SpinEventArgs>),
            typeof(ButtonSpinner));

    /// <summary>Raised when the user clicks an increase or decrease button.</summary>
    public event EventHandler<SpinEventArgs> Spin
    {
        add => AddHandler(SpinEvent, value);
        remove => RemoveHandler(SpinEvent, value);
    }

    // --- Template -------------------------------------------------------------

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (_increaseButton != null) _increaseButton.Click -= OnIncreaseClick;
        if (_decreaseButton != null) _decreaseButton.Click -= OnDecreaseClick;

        _increaseButton = GetTemplateChild(PART_IncreaseButton) as RepeatButton;
        _decreaseButton = GetTemplateChild(PART_DecreaseButton) as RepeatButton;

        if (_increaseButton != null) _increaseButton.Click += OnIncreaseClick;
        if (_decreaseButton != null) _decreaseButton.Click += OnDecreaseClick;
    }

    private void OnIncreaseClick(object sender, RoutedEventArgs e)
    {
        if (AllowSpin)
            RaiseEvent(new SpinEventArgs(SpinEvent, SpinDirection.Increase));
    }

    private void OnDecreaseClick(object sender, RoutedEventArgs e)
    {
        if (AllowSpin)
            RaiseEvent(new SpinEventArgs(SpinEvent, SpinDirection.Decrease));
    }
}
