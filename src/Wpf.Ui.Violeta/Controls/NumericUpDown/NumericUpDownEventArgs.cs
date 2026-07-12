using System;
using System.Windows;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>Direction of a NumericUpDown spin action.</summary>
public enum SpinDirection
{
    Increase,
    Decrease,
}

/// <summary>
/// Event args for the <see cref="NumericUpDown.Spinned"/> event.
/// Mirrors Ursa's <c>SpinEventArgs</c>.
/// </summary>
public class SpinEventArgs(RoutedEvent routedEvent, SpinDirection direction, bool usingMouseWheel = false) : RoutedEventArgs(routedEvent)
{
    public SpinDirection Direction { get; } = direction;
    public bool UsingMouseWheel { get; } = usingMouseWheel;
}

/// <summary>
/// Event args for <see cref="NumericUpDownBase{T}.ValueChanged"/>.
/// Mirrors Ursa's <c>ValueChangedEventArgs&lt;T&gt;</c>.
/// </summary>
public class ValueChangedEventArgs<T>(RoutedEvent routedEvent, T? oldValue, T? newValue) : RoutedEventArgs(routedEvent) where T : struct, IComparable<T>
{
    public T? OldValue { get; } = oldValue;
    public T? NewValue { get; } = newValue;
}
