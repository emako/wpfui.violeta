using System;
using System.Globalization;
using System.Windows;
using System.Windows.Input;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Abstract generic base for typed NumberComboBox controls.
/// Contains the typed Value / Minimum / Maximum dependency properties,
/// and the same text↔value synchronisation logic as <see cref="NumericUpDownBase{T}"/>.
/// </summary>
/// <typeparam name="T">A numeric value type implementing <see cref="IComparable{T}"/>.</typeparam>
public abstract class NumberComboBoxBase<T> : NumberComboBox
    where T : struct, IComparable<T>
{
    private bool _isSyncingTextAndValue;
    private bool _isSyncingSelectedItem;

    // --- Value ---------------------------------------------------------------

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(
            nameof(Value),
            typeof(T?),
            typeof(NumberComboBoxBase<T>),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal,
                OnValueChanged,
                CoerceValue));

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NumberComboBoxBase<T> self)
        {
            var args = new ValueChangedEventArgs<T>(ValueChangedEvent, (T?)e.OldValue, (T?)e.NewValue);
            self.RaiseEvent(args);

            if (!self._isSyncingTextAndValue)
                self.SyncTextAndValue(false, null, true);

            // Keep the dropdown pill (IsSelected) aligned with Value.
            if (!self._isSyncingSelectedItem)
                self.SyncSelectedItemFromValue();

            self.ExecuteCommand();
        }
    }

    private static object? CoerceValue(DependencyObject d, object? baseValue)
    {
        if (d is NumberComboBoxBase<T> self)
        {
            var val = (T?)baseValue;
            if (val is null) return self.EmptyInputValue;
            if (val.Value.CompareTo(self.Minimum) < 0) return (T?)self.Minimum;
            if (val.Value.CompareTo(self.Maximum) > 0) return (T?)self.Maximum;
            return val;
        }
        return baseValue;
    }

    /// <summary>Current value. Null when the text box is empty.</summary>
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
            typeof(NumberComboBoxBase<T>));

    /// <summary>Raised when <see cref="Value"/> changes.</summary>
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
            typeof(NumberComboBoxBase<T>),
            new PropertyMetadata(default(T), OnMinimumChanged, CoerceMinimum));

    private static void OnMinimumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NumberComboBoxBase<T> self)
            self.CoerceValue(ValueProperty);
    }

    private static object CoerceMinimum(DependencyObject d, object baseValue)
    {
        if (d is NumberComboBoxBase<T> self)
        {
            var min = (T)baseValue;
            if (min.CompareTo(self.Maximum) > 0) return self.Maximum;
        }
        return baseValue;
    }

    /// <summary>Minimum allowed value.</summary>
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
            typeof(NumberComboBoxBase<T>),
            new PropertyMetadata(default(T), OnMaximumChanged, CoerceMaximum));

    private static void OnMaximumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NumberComboBoxBase<T> self)
            self.CoerceValue(ValueProperty);
    }

    private static object CoerceMaximum(DependencyObject d, object baseValue)
    {
        if (d is NumberComboBoxBase<T> self)
        {
            var max = (T)baseValue;
            if (max.CompareTo(self.Minimum) < 0) return self.Minimum;
        }
        return baseValue;
    }

    /// <summary>Maximum allowed value.</summary>
    public T Maximum
    {
        get => (T)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    // --- EmptyInputValue -----------------------------------------------------

    public static readonly DependencyProperty EmptyInputValueProperty =
        DependencyProperty.Register(
            nameof(EmptyInputValue),
            typeof(T?),
            typeof(NumberComboBoxBase<T>),
            new PropertyMetadata(null));

    /// <summary>
    /// Value substituted when the user clears the input.
    /// Null means a null Value is allowed.
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
            typeof(NumberComboBoxBase<T>),
            new PropertyMetadata(null));

    /// <summary>Executed when <see cref="Value"/> changes.</summary>
    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public static readonly DependencyProperty CommandParameterProperty =
        DependencyProperty.Register(
            nameof(CommandParameter),
            typeof(object),
            typeof(NumberComboBoxBase<T>),
            new PropertyMetadata(null));

    /// <summary>Parameter passed to <see cref="Command"/>.</summary>
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

    protected NumberComboBoxBase()
    {
        Loaded += (_, _) =>
        {
            SyncTextAndValue(false, null, true);
            SyncSelectedItemFromValue();
        };
    }

    /// <inheritdoc/>
    protected override void ApplySelectedItem()
    {
        if (_isSyncingSelectedItem)
            return;

        if (SelectedItem is T typed)
            Value = typed;
        else
            CommitInput(true);
    }

    protected override void OnItemsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        base.OnItemsChanged(e);
        if (!_isSyncingSelectedItem)
            SyncSelectedItemFromValue();
    }

    /// <summary>
    /// Aligns <see cref="System.Windows.Controls.Primitives.Selector.SelectedItem"/> with <see cref="Value"/>
    /// so the dropdown pill highlights the matching item (or none when the value is free-typed).
    /// Editable ComboBox rewrites <see cref="System.Windows.Controls.ComboBox.Text"/> when selection
    /// changes, so the current display text is preserved across the update.
    /// </summary>
    private void SyncSelectedItemFromValue()
    {
        if (_isSyncingSelectedItem)
            return;

        _isSyncingSelectedItem = true;
        _suppressSelectionTextSync = true;
        try
        {
            object? match = null;
            if (Value is T value)
            {
                foreach (var item in Items)
                {
                    if (item is T typed && typed.CompareTo(value) == 0)
                    {
                        match = item;
                        break;
                    }
                }
            }

            if (Equals(SelectedItem, match))
                return;

            var savedText = _textBox?.Text ?? Text;
            var savedCaret = _textBox?.CaretIndex ?? -1;

            SetCurrentValue(SelectedItemProperty, match);

            // Editable ComboBox clears/replaces Text when SelectedItem changes.
            SetCurrentValue(TextProperty, savedText);
            if (_textBox != null)
            {
                if (_textBox.Text != savedText)
                    _textBox.Text = savedText;
                if (savedCaret >= 0)
                    _textBox.CaretIndex = Math.Min(savedCaret, savedText.Length);
            }
        }
        finally
        {
            _suppressSelectionTextSync = false;
            _isSyncingSelectedItem = false;
        }
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
                var input = text ?? _textBox?.Text ?? Text;
                var parsedValue = ConvertTextToValue(input);
                if (parsedValue is null && EmptyInputValue.HasValue)
                    parsedValue = EmptyInputValue;

                var oldValue = Value;
                Value = parsedValue;

                if (!forceTextUpdate && oldValue?.CompareTo(Value ?? default) == 0)
                    return true;

                if (!EqualityComparer(parsedValue, Value))
                    forceTextUpdate = true;
            }

            if (forceTextUpdate && !_updateFromTextInput)
            {
                var newText = ConvertValueToText(Value) ?? string.Empty;
                ApplyDisplayText(newText);
            }

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

    private void ApplyDisplayText(string newText)
    {
        if (_textBox != null)
        {
            if (_textBox.Text != newText)
                _textBox.Text = newText;
        }
        else if (Text != newText)
        {
            SetCurrentValue(TextProperty, newText);
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

        var trimmed = NumericUpDownBase<T>.TrimString(text, ParsingNumberStyle);
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

    // --- Clear ---------------------------------------------------------------

    public override void Clear()
    {
        Value = EmptyInputValue;
        ApplyDisplayText(string.Empty);
    }

    // --- RestrictInput override ----------------------------------------------

    /// <inheritdoc/>
    protected override bool IsNegativeInputAllowed() => Minimum.CompareTo(Zero) < 0;

    // --- Abstract members -----------------------------------------------------

    protected abstract T Zero { get; }

    protected abstract T? ParseText(string? text, NumberFormatInfo numberFormat, NumberStyles numberStyles);
}
