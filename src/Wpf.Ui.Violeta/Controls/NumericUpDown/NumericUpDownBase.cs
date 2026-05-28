using System;
using System.Globalization;
using System.Windows;
using System.Windows.Input;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Abstract generic base for typed NumericUpDown controls.
/// Contains the typed Value / Minimum / Maximum / Step dependency properties,
/// and the text↔value synchronisation logic.
/// Mirrors Ursa's <c>NumericUpDownBase&lt;T&gt;</c>.
/// </summary>
/// <typeparam name="T">A numeric value type implementing <see cref="IComparable{T}"/>.</typeparam>
public abstract class NumericUpDownBase<T> : NumericUpDown
    where T : struct, IComparable<T>
{
    private bool _isSyncingTextAndValue;

    // --- Value ---------------------------------------------------------------

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(
            nameof(Value),
            typeof(T?),
            typeof(NumericUpDownBase<T>),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal,
                OnValueChanged,
                CoerceValue));

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NumericUpDownBase<T> self)
        {
            var args = new ValueChangedEventArgs<T>(ValueChangedEvent, (T?)e.OldValue, (T?)e.NewValue);
            self.RaiseEvent(args);

            if (self._isSyncingTextAndValue) return;

            self.SyncTextAndValue(false, null, true);
            self.SetValidSpinDirection();
            self.ExecuteCommand();
        }
    }

    private static object? CoerceValue(DependencyObject d, object? baseValue)
    {
        if (d is NumericUpDownBase<T> self)
        {
            var val = (T?)baseValue;
            if (val is null) return self.EmptyInputValue;
            if (val.Value.CompareTo(self.Minimum) < 0) return (T?)self.Minimum;
            if (val.Value.CompareTo(self.Maximum) > 0) return (T?)self.Maximum;
            return val;
        }
        return baseValue;
    }

    /// <summary>Current value. Null when the text box is empty. Mirrors Ursa's <c>Value</c>.</summary>
    public T? Value
    {
        get => (T?)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    // --- ValueChanged routed event --------------------------------------------

    public static readonly RoutedEvent ValueChangedEvent =
        EventManager.RegisterRoutedEvent(
            nameof(ValueChanged),
            RoutingStrategy.Bubble,
            typeof(EventHandler<ValueChangedEventArgs<T>>),
            typeof(NumericUpDownBase<T>));

    /// <summary>Raised when <see cref="Value"/> changes. Mirrors Ursa's <c>ValueChanged</c>.</summary>
    public event EventHandler<ValueChangedEventArgs<T>> ValueChanged
    {
        add => AddHandler(ValueChangedEvent, value);
        remove => RemoveHandler(ValueChangedEvent, value);
    }

    // --- Minimum -------------------------------------------------------------

    public static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register(
            nameof(Minimum),
            typeof(T),
            typeof(NumericUpDownBase<T>),
            new PropertyMetadata(default(T), OnMinimumChanged, CoerceMinimum));

    private static void OnMinimumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NumericUpDownBase<T> self)
        {
            self.CoerceValue(ValueProperty);
            self.SetValidSpinDirection();
        }
    }

    private static object CoerceMinimum(DependencyObject d, object baseValue)
    {
        if (d is NumericUpDownBase<T> self)
        {
            var min = (T)baseValue;
            if (min.CompareTo(self.Maximum) > 0) return self.Maximum;
        }
        return baseValue;
    }

    /// <summary>Minimum allowed value. Mirrors Ursa's <c>Minimum</c>.</summary>
    public T Minimum
    {
        get => (T)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    // --- Maximum -------------------------------------------------------------

    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(
            nameof(Maximum),
            typeof(T),
            typeof(NumericUpDownBase<T>),
            new PropertyMetadata(default(T), OnMaximumChanged, CoerceMaximum));

    private static void OnMaximumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NumericUpDownBase<T> self)
        {
            self.CoerceValue(ValueProperty);
            self.SetValidSpinDirection();
        }
    }

    private static object CoerceMaximum(DependencyObject d, object baseValue)
    {
        if (d is NumericUpDownBase<T> self)
        {
            var max = (T)baseValue;
            if (max.CompareTo(self.Minimum) < 0) return self.Minimum;
        }
        return baseValue;
    }

    /// <summary>Maximum allowed value. Mirrors Ursa's <c>Maximum</c>.</summary>
    public T Maximum
    {
        get => (T)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    // --- Step ----------------------------------------------------------------

    public static readonly DependencyProperty StepProperty =
        DependencyProperty.Register(
            nameof(Step),
            typeof(T),
            typeof(NumericUpDownBase<T>),
            new PropertyMetadata(default(T)));

    /// <summary>Amount added/subtracted per spin action. Mirrors Ursa's <c>Step</c>.</summary>
    public T Step
    {
        get => (T)GetValue(StepProperty);
        set => SetValue(StepProperty, value);
    }

    // --- EmptyInputValue -----------------------------------------------------

    public static readonly DependencyProperty EmptyInputValueProperty =
        DependencyProperty.Register(
            nameof(EmptyInputValue),
            typeof(T?),
            typeof(NumericUpDownBase<T>),
            new PropertyMetadata(null));

    /// <summary>
    /// Value substituted when the user clears the input.
    /// Null means a null Value is allowed. Mirrors Ursa's <c>EmptyInputValue</c>.
    /// </summary>
    public T? EmptyInputValue
    {
        get => (T?)GetValue(EmptyInputValueProperty);
        set => SetValue(EmptyInputValueProperty, value);
    }

    // --- Command -------------------------------------------------------------

    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register(
            nameof(Command),
            typeof(ICommand),
            typeof(NumericUpDownBase<T>),
            new PropertyMetadata(null));

    /// <summary>Executed when <see cref="Value"/> changes. Mirrors Ursa's <c>Command</c>.</summary>
    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public static readonly DependencyProperty CommandParameterProperty =
        DependencyProperty.Register(
            nameof(CommandParameter),
            typeof(object),
            typeof(NumericUpDownBase<T>),
            new PropertyMetadata(null));

    /// <summary>Parameter passed to <see cref="Command"/>. Mirrors Ursa's <c>CommandParameter</c>.</summary>
    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    private void ExecuteCommand()
    {
        var cmd = Command;
        var param = CommandParameter;
        if (cmd?.CanExecute(param) == true)
            cmd.Execute(param);
    }

    // --- Initialization ------------------------------------------------------

    protected NumericUpDownBase()
    {
        Loaded += (_, _) =>
        {
            SyncTextAndValue(false, null, true);
            SetValidSpinDirection();
        };
    }

    // --- Core sync logic -----------------------------------------------------

    protected override bool SyncTextAndValue(
        bool fromTextToValue = false,
        string? text = null,
        bool forceTextUpdate = false)
    {
        if (_isSyncingTextAndValue) return true;
        _isSyncingTextAndValue = true;
        try
        {
            if (fromTextToValue)
            {
                // Parse text → value
                var input = text ?? _textBox?.Text;
                var parsedValue = ConvertTextToValue(input);
                if (parsedValue is null && EmptyInputValue.HasValue)
                    parsedValue = EmptyInputValue;

                // If parsed value is out of range it will be coerced
                var oldValue = Value;
                Value = parsedValue;

                if (!forceTextUpdate && oldValue?.CompareTo(Value ?? default) == 0)
                    return true;

                // If the value was coerced, update the displayed text to reflect the coercion
                if (!EqualityComparer(parsedValue, Value))
                {
                    forceTextUpdate = true;
                }
            }

            if (forceTextUpdate && _textBox != null && !_updateFromTextInput)
            {
                var newText = ConvertValueToText(Value);
                if (_textBox.Text != newText)
                    _textBox.Text = newText ?? string.Empty;
            }

            SetValidSpinDirection();
            return true;
        }
        catch
        {
            return false;
        }
        finally
        {
            _isSyncingTextAndValue = false;
        }
    }

    private static bool EqualityComparer(T? a, T? b)
    {
        if (a is null && b is null) return true;
        if (a is null || b is null) return false;
        return a.Value.CompareTo(b.Value) == 0;
    }

    // --- Text ↔ value conversion ----------------------------------------------

    protected virtual T? ConvertTextToValue(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return EmptyInputValue;

        var converter = TextConverter;
        if (converter != null)
        {
            var result = converter.Convert(text, typeof(T?), null, CultureInfo.CurrentCulture);
            return result == DependencyProperty.UnsetValue ? EmptyInputValue : (T?)result;
        }

        var trimmed = TrimString(text, ParsingNumberStyle);
        return ParseText(trimmed, NumberFormat ?? NumberFormatInfo.CurrentInfo, ParsingNumberStyle);
    }

    protected virtual string? ConvertValueToText(T? value)
    {
        if (value is null) return null;

        var converter = TextConverter;
        if (converter != null)
        {
            var result = converter.ConvertBack(value, typeof(string), null, CultureInfo.CurrentCulture);
            return result?.ToString();
        }

        var fmt = FormatString;
        var numFmt = NumberFormat ?? NumberFormatInfo.CurrentInfo;
        if (!string.IsNullOrEmpty(fmt))
        {
            if (fmt.Contains("{0"))
                return string.Format(numFmt, fmt, value.Value);
            return value.Value.ValueToString(fmt, numFmt);
        }
        return value.Value.ToString();
    }

    // --- Spin direction validation --------------------------------------------

    protected override void SetValidSpinDirection()
    {
        if (IsReadOnly || !AllowSpin)
        {
            _canIncrease = false;
            _canDecrease = false;
        }
        else
        {
            _canIncrease = Value is null || Value.Value.CompareTo(Maximum) < 0;
            _canDecrease = Value is null || Value.Value.CompareTo(Minimum) > 0;
        }

        _increaseButton?.IsEnabled = _canIncrease;
        _decreaseButton?.IsEnabled = _canDecrease;
    }

    // --- Increase / Decrease -------------------------------------------------

    protected override void Increase()
    {
        if (Value is null)
            Value = ClampToRange(Zero);
        else
            Value = ClampToRange(Add(Value.Value, Step));
    }

    protected override void Decrease()
    {
        if (Value is null)
            Value = ClampToRange(Zero);
        else
            Value = ClampToRange(Subtract(Value.Value, Step));
    }

    private T ClampToRange(T value)
    {
        if (value.CompareTo(Maximum) > 0) return Maximum;
        if (value.CompareTo(Minimum) < 0) return Minimum;
        return value;
    }

    // --- Clear ---------------------------------------------------------------

    public override void Clear()
    {
        Value = EmptyInputValue;
        _textBox?.Text = string.Empty;
    }

    // --- RestrictInput override ----------------------------------------------

    /// <inheritdoc/>
    protected override bool IsNegativeInputAllowed() => Minimum.CompareTo(Zero) < 0;

    // --- Abstract members -----------------------------------------------------

    protected abstract T Zero { get; }
    protected abstract T Add(T a, T b);
    protected abstract T Subtract(T a, T b);
    protected abstract T? ParseText(string? text, NumberFormatInfo numberFormat, NumberStyles numberStyles);

    // --- Helpers -------------------------------------------------------------

    /// <summary>
    /// Strips hex/binary literal prefixes and underscore separators from the raw input string.
    /// Mirrors Ursa's <c>TrimString</c>.
    /// </summary>
    internal static string TrimString(string? text, NumberStyles numberStyles)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;

        var s = text!.Replace("_", string.Empty);

        if ((numberStyles & NumberStyles.AllowHexSpecifier) != 0)
        {
            if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                s = s.Substring(2);
            else if (s.StartsWith("h'", StringComparison.OrdinalIgnoreCase))
                s = s.Substring(2).TrimEnd('\'');
            else if (s.EndsWith("h", StringComparison.OrdinalIgnoreCase))
                s = s.Substring(0, s.Length - 1);
        }

#if NET5_0_OR_GREATER
        if ((numberStyles & NumberStyles.AllowBinarySpecifier) != 0)
        {
            if (s.StartsWith("0b", StringComparison.OrdinalIgnoreCase))
                s = s.Substring(2);
            else if (s.StartsWith("b'", StringComparison.OrdinalIgnoreCase))
                s = s.Substring(2).TrimEnd('\'');
            else if (s.EndsWith("b", StringComparison.OrdinalIgnoreCase))
                s = s.Substring(0, s.Length - 1);
        }
#endif
        return s;
    }
}

// --- Extension helper ---------------------------------------------------------

internal static class NumericFormatExtensions
{
    /// <summary>
    /// Calls <c>ToString(format, formatProvider)</c> via the <see cref="IFormattable"/> interface
    /// if T implements it; otherwise falls back to <c>ToString()</c>.
    /// </summary>
    public static string ValueToString<T>(this T value, string format, IFormatProvider formatProvider)
        where T : struct
    {
        if (value is IFormattable formattable)
            return formattable.ToString(format, formatProvider);
        return value.ToString() ?? string.Empty;
    }
}
