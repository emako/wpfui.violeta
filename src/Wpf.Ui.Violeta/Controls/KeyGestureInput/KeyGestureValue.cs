using System.Collections.Generic;
using System.Windows.Input;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Represents a keyboard shortcut consisting of a primary key and optional modifier keys.
/// </summary>
public readonly struct KeyGestureValue(Key key, ModifierKeys modifiers = ModifierKeys.None) : IEquatable<KeyGestureValue>
{
    public Key Key { get; } = key;
    public ModifierKeys Modifiers { get; } = modifiers;

    public override string ToString()
    {
        var parts = new List<string>(5);
        if ((Modifiers & ModifierKeys.Control) != 0) parts.Add("Ctrl");
        if ((Modifiers & ModifierKeys.Alt) != 0) parts.Add("Alt");
        if ((Modifiers & ModifierKeys.Shift) != 0) parts.Add("Shift");
        if ((Modifiers & ModifierKeys.Windows) != 0) parts.Add("Win");
        parts.Add(Key.ToString());
        return string.Join("+", parts);
    }

    public bool Equals(KeyGestureValue other) => Key == other.Key && Modifiers == other.Modifiers;

    public override bool Equals(object? obj) => obj is KeyGestureValue other && Equals(other);

    public override int GetHashCode() => (int)Key * 397 ^ (int)Modifiers;

    public static bool operator ==(KeyGestureValue left, KeyGestureValue right) => left.Equals(right);

    public static bool operator !=(KeyGestureValue left, KeyGestureValue right) => !left.Equals(right);
}
