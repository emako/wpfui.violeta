using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Generic animated number display. When <see cref="Value"/> changes, the displayed
/// number interpolates from the previous value to the new one over <see cref="NumberDisplayerBase.Duration"/>.
/// Mirrors Ursa.Avalonia's <c>NumberDisplayer&lt;T&gt;</c>.
/// </summary>
/// <typeparam name="T">Value type being displayed.</typeparam>
public abstract class NumberDisplayer<T> : NumberDisplayerBase
{
    private bool _templateApplied;
    private T _fromValue = default!;
    private T _toValue = default!;

    #region Value

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(
            nameof(Value),
            typeof(T),
            typeof(NumberDisplayer<T>),
            new FrameworkPropertyMetadata(
                default(T),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnValueChanged));

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NumberDisplayer<T> self)
        {
            self.OnValueChanged((T)e.OldValue!, (T)e.NewValue!);
        }
    }

    /// <summary>Target value to display (animated into view). Mirrors Ursa's <c>Value</c>.</summary>
    public T Value
    {
        get => (T)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value!);
    }

    #endregion Value

    #region InternalValue

    private static readonly DependencyProperty InternalValueProperty =
        DependencyProperty.Register(
            nameof(InternalValue),
            typeof(T),
            typeof(NumberDisplayer<T>),
            new PropertyMetadata(default(T), OnInternalValueChanged));

    private static void OnInternalValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NumberDisplayer<T> self)
        {
            self.InternalText = self.GetString((T)e.NewValue!);
        }
    }

    private T InternalValue
    {
        get => (T)GetValue(InternalValueProperty);
        set => SetValue(InternalValueProperty, value!);
    }

    #endregion InternalValue

    #region AnimationProgress

    private static readonly DependencyProperty AnimationProgressProperty =
        DependencyProperty.Register(
            "AnimationProgress",
            typeof(double),
            typeof(NumberDisplayer<T>),
            new PropertyMetadata(0d, OnAnimationProgressChanged));

    private static void OnAnimationProgressChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NumberDisplayer<T> self)
        {
            var progress = (double)e.NewValue;
            self.SetCurrentValue(
                InternalValueProperty,
                self.Interpolate(progress, self._fromValue, self._toValue)!);
        }
    }

    #endregion AnimationProgress

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _templateApplied = true;

        // Show the current value immediately on first apply (covers default-value case).
        SetCurrentValue(InternalValueProperty, Value);
        InternalText = GetString(Value);
    }

    protected override void OnStringFormatChanged()
    {
        InternalText = GetString(InternalValue);
    }

    private void OnValueChanged(T oldValue, T newValue)
    {
        if (!_templateApplied || !Duration.HasTimeSpan || Duration.TimeSpan <= TimeSpan.Zero)
        {
            BeginAnimation(AnimationProgressProperty, null);
            SetCurrentValue(InternalValueProperty, newValue!);
            return;
        }

        BeginAnimation(AnimationProgressProperty, null);

        _fromValue = oldValue;
        _toValue = newValue;

        var animation = new DoubleAnimation(0d, 1d, Duration)
        {
            FillBehavior = FillBehavior.Stop,
        };

        animation.Completed += (_, _) =>
        {
            BeginAnimation(AnimationProgressProperty, null);
            SetCurrentValue(InternalValueProperty, newValue!);
        };

        BeginAnimation(AnimationProgressProperty, animation);
    }

    /// <summary>Interpolates between <paramref name="oldValue"/> and <paramref name="newValue"/> at <paramref name="progress"/> (0–1).</summary>
    protected abstract T Interpolate(double progress, T oldValue, T newValue);

    /// <summary>Formats <paramref name="value"/> using <see cref="NumberDisplayerBase.StringFormat"/>.</summary>
    protected abstract string GetString(T value);
}
