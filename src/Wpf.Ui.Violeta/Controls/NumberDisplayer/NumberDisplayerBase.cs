using System;
using System.Windows;
using System.Windows.Controls;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Non-generic base for animated number display controls.
/// Mirrors Ursa.Avalonia's <c>NumberDisplayerBase</c>.
/// </summary>
public abstract class NumberDisplayerBase : Control
{
    static NumberDisplayerBase()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(NumberDisplayerBase),
            new FrameworkPropertyMetadata(typeof(NumberDisplayerBase)));

        IsTabStopProperty.OverrideMetadata(
            typeof(NumberDisplayerBase),
            new FrameworkPropertyMetadata(false));

        FocusableProperty.OverrideMetadata(
            typeof(NumberDisplayerBase),
            new FrameworkPropertyMetadata(false));
    }

    #region InternalText

    public static readonly DependencyProperty InternalTextProperty =
        DependencyProperty.Register(
            nameof(InternalText),
            typeof(string),
            typeof(NumberDisplayerBase),
            new PropertyMetadata(string.Empty));

    /// <summary>
    /// Formatted text currently shown by the control template.
    /// Updated as the animated value interpolates.
    /// </summary>
    public string? InternalText
    {
        get => (string?)GetValue(InternalTextProperty);
        set => SetValue(InternalTextProperty, value);
    }

    #endregion InternalText

    #region Duration

    public static readonly DependencyProperty DurationProperty =
        DependencyProperty.Register(
            nameof(Duration),
            typeof(Duration),
            typeof(NumberDisplayerBase),
            new PropertyMetadata(new Duration(TimeSpan.FromMilliseconds(200))));

    /// <summary>
    /// Length of the value transition animation. Mirrors Ursa's <c>Duration</c>.
    /// </summary>
    public Duration Duration
    {
        get => (Duration)GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }

    #endregion Duration

    #region StringFormat

    public static readonly DependencyProperty StringFormatProperty =
        DependencyProperty.Register(
            nameof(StringFormat),
            typeof(string),
            typeof(NumberDisplayerBase),
            new PropertyMetadata(null, OnStringFormatChanged));

    private static void OnStringFormatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NumberDisplayerBase self)
        {
            self.OnStringFormatChanged();
        }
    }

    /// <summary>
    /// Optional format string passed to <c>ToString</c> when rendering the value.
    /// Mirrors Ursa's <c>StringFormat</c>.
    /// </summary>
    public string? StringFormat
    {
        get => (string?)GetValue(StringFormatProperty);
        set => SetValue(StringFormatProperty, value);
    }

    #endregion StringFormat

    #region IsSelectable

    public static readonly DependencyProperty IsSelectableProperty =
        DependencyProperty.Register(
            nameof(IsSelectable),
            typeof(bool),
            typeof(NumberDisplayerBase),
            new PropertyMetadata(false));

    /// <summary>
    /// When <see langword="true"/>, text is rendered with <see cref="SelectableTextBlock"/>
    /// so the user can select and copy it. Mirrors Ursa's <c>IsSelectable</c>.
    /// </summary>
    public bool IsSelectable
    {
        get => (bool)GetValue(IsSelectableProperty);
        set => SetValue(IsSelectableProperty, value);
    }

    #endregion IsSelectable

    /// <summary>
    /// Called when <see cref="StringFormat"/> changes so subclasses can refresh displayed text.
    /// </summary>
    protected virtual void OnStringFormatChanged()
    {
    }
}
