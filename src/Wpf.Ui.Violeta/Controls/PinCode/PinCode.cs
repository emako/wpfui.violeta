using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// A PIN / verification-code entry control that displays <see cref="Count"/> individual cells.
/// Mirrors the logic of Ursa.Avalonia's PinCode control.
/// </summary>
[TemplatePart(Name = PART_ItemsControl, Type = typeof(PinCodeItemsControl))]
public class PinCode : Control
{
    public const string PART_ItemsControl = "PART_ItemsControl";

    private PinCodeItemsControl? _itemsControl;
    private int _currentIndex;

    #region Dependency Properties

    public static readonly DependencyProperty CompleteCommandProperty =
        DependencyProperty.Register(
            nameof(CompleteCommand), typeof(ICommand), typeof(PinCode),
            new PropertyMetadata(null));

    public ICommand? CompleteCommand
    {
        get => (ICommand?)GetValue(CompleteCommandProperty);
        set => SetValue(CompleteCommandProperty, value);
    }

    public static readonly DependencyProperty CountProperty =
        DependencyProperty.Register(
            nameof(Count), typeof(int), typeof(PinCode),
            new PropertyMetadata(4, OnCountChanged));

    public int Count
    {
        get => (int)GetValue(CountProperty);
        set => SetValue(CountProperty, value);
    }

    public static readonly DependencyProperty PasswordCharProperty =
        DependencyProperty.Register(
            nameof(PasswordChar), typeof(char), typeof(PinCode),
            new PropertyMetadata('\0'));

    public char PasswordChar
    {
        get => (char)GetValue(PasswordCharProperty);
        set => SetValue(PasswordCharProperty, value);
    }

    public static readonly DependencyProperty ModeProperty =
        DependencyProperty.Register(
            nameof(Mode), typeof(PinCodeMode), typeof(PinCode),
            new PropertyMetadata(PinCodeMode.Digit | PinCodeMode.Letter));

    public PinCodeMode Mode
    {
        get => (PinCodeMode)GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }

    private static readonly DependencyPropertyKey DigitsPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(Digits), typeof(List<string>), typeof(PinCode),
            new PropertyMetadata(null));

    public static readonly DependencyProperty DigitsProperty = DigitsPropertyKey.DependencyProperty;

    /// <summary>
    /// The current contents of each cell. Updated as the user types.
    /// Passed as the parameter to <see cref="CompleteCommand"/> and to the <see cref="Complete"/> event.
    /// </summary>
    public List<string> Digits => (List<string>)GetValue(DigitsProperty);

    #endregion Dependency Properties

    #region Routed Events

    public static readonly RoutedEvent CompleteEvent =
        EventManager.RegisterRoutedEvent(
            nameof(Complete), RoutingStrategy.Bubble,
            typeof(EventHandler<PinCodeCompletedEventArgs>), typeof(PinCode));

    public event EventHandler<PinCodeCompletedEventArgs> Complete
    {
        add => AddHandler(CompleteEvent, value);
        remove => RemoveHandler(CompleteEvent, value);
    }

    #endregion Routed Events

    static PinCode()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(PinCode),
            new FrameworkPropertyMetadata(typeof(PinCode)));

        // PinCode itself is not focusable; the individual PinCodeItem cells are.
        FocusableProperty.OverrideMetadata(
            typeof(PinCode),
            new FrameworkPropertyMetadata(false));

        InputMethod.IsInputMethodEnabledProperty.OverrideMetadata(
            typeof(PinCode),
            new FrameworkPropertyMetadata(false));
    }

    public PinCode()
    {
        RebuildDigits(Count);
    }

    private static void OnCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var pinCode = (PinCode)d;
        pinCode.RebuildDigits(Math.Max(0, (int)e.NewValue));
        pinCode._currentIndex = 0;
    }

    private void RebuildDigits(int count)
    {
        SetValue(DigitsPropertyKey, new List<string>(Enumerable.Repeat(string.Empty, count)));
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _itemsControl = GetTemplateChild(PART_ItemsControl) as PinCodeItemsControl;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Input handling
    // ──────────────────────────────────────────────────────────────────────────

    protected override void OnPreviewTextInput(TextCompositionEventArgs e)
    {
        base.OnPreviewTextInput(e);

        if (string.IsNullOrEmpty(e.Text) || e.Text.Length != 1)
            return;

        SyncCurrentIndexFromFocus();

        char c = e.Text[0];
        if (!ValidChar(c))
        {
            e.Handled = true;
            return;
        }

        if (_currentIndex < Count)
        {
            Digits[_currentIndex] = e.Text;
            var container = GetContainer(_currentIndex);
            if (container != null) container.Text = e.Text;

            _currentIndex++;

            if (_currentIndex < Count)
            {
                FocusCell(_currentIndex);
            }
            else
            {
                _currentIndex = Count - 1;
                FocusCell(_currentIndex);
                RaiseCompleted();
            }
        }

        e.Handled = true;
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);

        SyncCurrentIndexFromFocus();

        // ── Paste: Ctrl+V ──
        if (e.Key == Key.V && Keyboard.Modifiers == ModifierKeys.Control)
        {
            var text = Clipboard.GetText();
            if (!string.IsNullOrEmpty(text))
            {
                var filtered = text.Where(c => ValidChar(c)).Take(Count).ToArray();
                for (int i = 0; i < filtered.Length; i++)
                {
                    Digits[i] = filtered[i].ToString();
                    var container = GetContainer(i);
                    if (container != null) container.Text = filtered[i].ToString();
                }
                _currentIndex = Math.Max(0, Math.Min(filtered.Length, Count - 1));
                FocusCell(_currentIndex);
                if (filtered.Length == Count)
                    RaiseCompleted();
            }
            e.Handled = true;
            return;
        }

        // ── Backspace: clear current cell and move back ──
        if (e.Key == Key.Back)
        {
            _currentIndex = Math.Max(0, Math.Min(_currentIndex, Count - 1));
            Digits[_currentIndex] = string.Empty;
            var container = GetContainer(_currentIndex);
            if (container != null) container.Text = string.Empty;
            if (_currentIndex > 0)
            {
                _currentIndex--;
                FocusCell(_currentIndex);
            }
            e.Handled = true;
        }
        // ── Delete: clear current cell and move forward ──
        else if (e.Key == Key.Delete)
        {
            _currentIndex = Math.Max(0, Math.Min(_currentIndex, Count - 1));
            Digits[_currentIndex] = string.Empty;
            var container = GetContainer(_currentIndex);
            if (container != null) container.Text = string.Empty;
            if (_currentIndex < Count - 1)
            {
                _currentIndex++;
                FocusCell(_currentIndex);
            }
            e.Handled = true;
        }
        // ── Arrow navigation ──
        else if (e.Key == Key.Left)
        {
            if (_currentIndex > 0) _currentIndex--;
            FocusCell(_currentIndex);
            e.Handled = true;
        }
        else if (e.Key == Key.Right)
        {
            if (_currentIndex < Count - 1) _currentIndex++;
            FocusCell(_currentIndex);
            e.Handled = true;
        }
        // ── Enter: fire complete ──
        else if (e.Key == Key.Return)
        {
            RaiseCompleted();
            e.Handled = true;
        }
        // ── Tab: navigate between cells ──
        else if (e.Key == Key.Tab)
        {
            if (Keyboard.Modifiers == ModifierKeys.Shift)
                _currentIndex = Math.Max(0, _currentIndex - 1);
            else
                _currentIndex = Math.Min(Count - 1, _currentIndex + 1);
            FocusCell(_currentIndex);
            e.Handled = true;
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Updates <see cref="_currentIndex"/> by walking up the visual tree from the currently
    /// focused element to find the focused <see cref="PinCodeItem"/> and its index.
    /// </summary>
    private void SyncCurrentIndexFromFocus()
    {
        if (_itemsControl is null) return;

        DependencyObject? current = Keyboard.FocusedElement as DependencyObject;
        while (current is not null)
        {
            if (current is PinCodeItem item)
            {
                int idx = _itemsControl.ItemContainerGenerator.IndexFromContainer(item);
                if (idx >= 0)
                {
                    _currentIndex = idx;
                    return;
                }
            }
            current = VisualTreeHelper.GetParent(current);
        }
    }

    private PinCodeItem? GetContainer(int index)
    {
        if (_itemsControl is null || index < 0 || index >= Count) return null;
        return _itemsControl.ItemContainerGenerator.ContainerFromIndex(index) as PinCodeItem;
    }

    private void FocusCell(int index)
    {
        index = Math.Max(0, Math.Min(index, Count - 1));
        GetContainer(index)?.Focus();
    }

    private bool ValidChar(char c)
    {
        bool isDigit = char.IsDigit(c);
        bool isLetter = char.IsLetter(c);
        return Mode switch
        {
            PinCodeMode.Digit => isDigit,
            PinCodeMode.Letter => isLetter,
            _ => isDigit || isLetter,
        };
    }

    private void RaiseCompleted()
    {
        var args = new PinCodeCompletedEventArgs([.. Digits], CompleteEvent);
        CompleteCommand?.Execute(args.Code);
        RaiseEvent(args);
    }
}
