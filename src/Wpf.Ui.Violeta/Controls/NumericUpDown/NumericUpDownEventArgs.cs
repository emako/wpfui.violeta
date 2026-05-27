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
public class SpinEventArgs : RoutedEventArgs
{
    public SpinDirection Direction { get; }
    public bool UsingMouseWheel { get; }

    public SpinEventArgs(RoutedEvent routedEvent, SpinDirection direction, bool usingMouseWheel = false)
        : base(routedEvent)
    {
        Direction = direction;
        UsingMouseWheel = usingMouseWheel;
    }
}

/// <summary>
/// Event args for <see cref="NumericUpDownBase{T}.ValueChanged"/>.
/// Mirrors Ursa's <c>ValueChangedEventArgs&lt;T&gt;</c>.
/// </summary>
public class ValueChangedEventArgs<T> : RoutedEventArgs where T : struct, IComparable<T>
{
    public T? OldValue { get; }
    public T? NewValue { get; }

    public ValueChangedEventArgs(RoutedEvent routedEvent, T? oldValue, T? newValue)
        : base(routedEvent)
    {
        OldValue = oldValue;
        NewValue = newValue;
    }
}
