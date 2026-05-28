using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Abstract non-generic base for NumericUpDown controls.
/// Manages template parts, keyboard/mouse/drag interaction, and all properties
/// that are independent of the numeric type T.
/// Mirrors the non-generic <c>NumericUpDown</c> base from Ursa.Avalonia.
/// </summary>
[TemplatePart(Name = PART_TextBox, Type = typeof(TextBox))]
[TemplatePart(Name = PART_IncreaseButton, Type = typeof(RepeatButton))]
[TemplatePart(Name = PART_DecreaseButton, Type = typeof(RepeatButton))]
[TemplatePart(Name = PART_DragPanel, Type = typeof(UIElement))]
public abstract class NumericUpDown : Control
{
    public const string PART_TextBox = "PART_TextBox";
    public const string PART_IncreaseButton = "PART_IncreaseButton";
    public const string PART_DecreaseButton = "PART_DecreaseButton";
    public const string PART_DragPanel = "PART_DragPanel";

    protected TextBox? _textBox;
    protected RepeatButton? _increaseButton;
    protected RepeatButton? _decreaseButton;
    protected UIElement? _dragPanel;
    private ContentPresenter? _innerLeftContent;
    private ContentPresenter? _innerRightContent;

    // Whether the current text update is from user typing (vs programmatic)
    protected internal bool _updateFromTextInput;
    // Whether increase/decrease are currently valid
    protected internal bool _canIncrease = true;
    protected internal bool _canDecrease = true;

    // Tracks the mouse position for drag-to-spin
    private Point? _dragStartPoint;
    // Guard to avoid re-entrant TextChanged when stripping invalid chars
    private bool _isRestrictingInput;

    #region RestrictInput

    public static readonly DependencyProperty RestrictInputProperty =
        DependencyProperty.Register(
            nameof(RestrictInput),
            typeof(bool),
            typeof(NumericUpDown),
            new PropertyMetadata(true, OnRestrictInputChanged));

    private static void OnRestrictInputChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NumericUpDown self)
            self.UpdateInputMethodState();
    }

    private void UpdateInputMethodState()
    {
        if (_textBox != null)
            InputMethod.SetIsInputMethodEnabled(_textBox, !RestrictInput);
    }

    /// <summary>
    /// When <see langword="true"/> the TextBox only accepts characters valid for the numeric type:
    /// digits, the type-appropriate decimal separator, and the minus sign (only when
    /// <see cref="IsNegativeInputAllowed"/> returns <see langword="true"/>). IME input is also
    /// disabled. Default is <see langword="true"/>.
    /// </summary>
    public bool RestrictInput
    {
        get => (bool)GetValue(RestrictInputProperty);
        set => SetValue(RestrictInputProperty, value);
    }

    #endregion RestrictInput

    #region AllowDrag

    public static readonly DependencyProperty AllowDragProperty =
        DependencyProperty.Register(
            nameof(AllowDrag),
            typeof(bool),
            typeof(NumericUpDown),
            new PropertyMetadata(false, OnAllowDragChanged));

    private static void OnAllowDragChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NumericUpDown self)
            self.UpdateDragPanelVisibility();
    }

    /// <summary>
    /// When <see langword="true"/> a transparent overlay covers the text; dragging on it
    /// increases/decreases the value, and double-clicking it reveals the text input.
    /// Mirrors Ursa's <c>AllowDrag</c>.
    /// </summary>
    public bool AllowDrag
    {
        get => (bool)GetValue(AllowDragProperty);
        set => SetValue(AllowDragProperty, value);
    }

    #endregion AllowDrag

    #region IsReadOnly

    public static readonly DependencyProperty IsReadOnlyProperty =
        DependencyProperty.Register(
            nameof(IsReadOnly),
            typeof(bool),
            typeof(NumericUpDown),
            new PropertyMetadata(false, OnIsReadOnlyChanged));

    private static void OnIsReadOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NumericUpDown self)
        {
            self._textBox?.IsReadOnly = (bool)e.NewValue;
            self.SetValidSpinDirection();
        }
    }
    /// <summary>Prevents user editing. Mirrors Ursa's <c>IsReadOnly</c>.</summary>
    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    #endregion IsReadOnly


    #region InnerLeftContent / InnerRightContent

    public static readonly DependencyProperty InnerLeftContentProperty =
        DependencyProperty.Register(
            nameof(InnerLeftContent),
            typeof(object),
            typeof(NumericUpDown),
            new PropertyMetadata(null, OnInnerContentChanged));

    private static void OnInnerContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NumericUpDown self) self.UpdateInnerContentVisibility();
    }

    private void UpdateInnerContentVisibility()
    {
        _innerLeftContent?.Visibility = InnerLeftContent != null ? Visibility.Visible : Visibility.Collapsed;
        _innerRightContent?.Visibility = InnerRightContent != null ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <summary>Optional content rendered to the left of the text input. Mirrors Ursa's <c>InnerLeftContent</c>.</summary>
    public object? InnerLeftContent
    {
        get => GetValue(InnerLeftContentProperty);
        set => SetValue(InnerLeftContentProperty, value);
    }

    public static readonly DependencyProperty InnerRightContentProperty =
        DependencyProperty.Register(
            nameof(InnerRightContent),
            typeof(object),
            typeof(NumericUpDown),
            new PropertyMetadata(null, OnInnerContentChanged));

    /// <summary>Optional content rendered to the right of the text input. Mirrors Ursa's <c>InnerRightContent</c>.</summary>
    public object? InnerRightContent
    {
        get => GetValue(InnerRightContentProperty);
        set => SetValue(InnerRightContentProperty, value);
    }

    #endregion InnerLeftContent / InnerRightContent

    #region PlaceholderText

    public static readonly DependencyProperty PlaceholderTextProperty =
        DependencyProperty.Register(
            nameof(PlaceholderText),
            typeof(string),
            typeof(NumericUpDown),
            new PropertyMetadata(null));

    /// <summary>Placeholder shown when the value is null/empty. Mirrors Ursa's <c>PlaceholderText</c>.</summary>
    public string? PlaceholderText
    {
        get => (string?)GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    #endregion PlaceholderText

    #region NumberFormat

    public static readonly DependencyProperty NumberFormatProperty =
        DependencyProperty.Register(
            nameof(NumberFormat),
            typeof(NumberFormatInfo),
            typeof(NumericUpDown),
            new PropertyMetadata(NumberFormatInfo.CurrentInfo, OnFormatPropertyChanged));

    private static void OnFormatPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NumericUpDown self && self.IsLoaded)
            self.SyncTextAndValue(false, null, true);
    }

    /// <summary>Format info used when parsing and formatting numbers. Mirrors Ursa's <c>NumberFormat</c>.</summary>
    public NumberFormatInfo? NumberFormat
    {
        get => (NumberFormatInfo?)GetValue(NumberFormatProperty);
        set => SetValue(NumberFormatProperty, value);
    }

    #endregion NumberFormat

    #region FormatString

    public static readonly DependencyProperty FormatStringProperty =
        DependencyProperty.Register(
            nameof(FormatString),
            typeof(string),
            typeof(NumericUpDown),
            new PropertyMetadata(string.Empty, OnFormatPropertyChanged));

    /// <summary>
    /// .NET format string applied when converting the value to text (e.g. <c>"N2"</c>, <c>"X"</c>).
    /// Mirrors Ursa's <c>FormatString</c>.
    /// </summary>
    public string FormatString
    {
        get => (string)GetValue(FormatStringProperty);
        set => SetValue(FormatStringProperty, value);
    }

    #endregion FormatString

    #region ParsingNumberStyle

    public static readonly DependencyProperty ParsingNumberStyleProperty =
        DependencyProperty.Register(
            nameof(ParsingNumberStyle),
            typeof(NumberStyles),
            typeof(NumericUpDown),
            new PropertyMetadata(NumberStyles.Any));

    /// <summary>
    /// <see cref="NumberStyles"/> flags used when parsing the text input.
    /// Mirrors Ursa's <c>ParsingNumberStyle</c>.
    /// </summary>
    public NumberStyles ParsingNumberStyle
    {
        get => (NumberStyles)GetValue(ParsingNumberStyleProperty);
        set => SetValue(ParsingNumberStyleProperty, value);
    }

    #endregion ParsingNumberStyle

    #region TextConverter

    public static readonly DependencyProperty TextConverterProperty =
        DependencyProperty.Register(
            nameof(TextConverter),
            typeof(IValueConverter),
            typeof(NumericUpDown),
            new PropertyMetadata(null, OnFormatPropertyChanged));

    /// <summary>
    /// Optional <see cref="IValueConverter"/> that overrides the default text↔value conversion.
    /// Mirrors Ursa's <c>TextConverter</c>.
    /// </summary>
    public IValueConverter? TextConverter
    {
        get => (IValueConverter?)GetValue(TextConverterProperty);
        set => SetValue(TextConverterProperty, value);
    }

    #endregion TextConverter

    #region AllowSpin

    public static readonly DependencyProperty AllowSpinProperty =
        DependencyProperty.Register(
            nameof(AllowSpin),
            typeof(bool),
            typeof(NumericUpDown),
            new PropertyMetadata(true));

    /// <summary>
    /// Enables keyboard (↑↓), mouse-wheel, and button spinning.
    /// Mirrors Ursa's <c>AllowSpin</c>.
    /// </summary>
    public bool AllowSpin
    {
        get => (bool)GetValue(AllowSpinProperty);
        set => SetValue(AllowSpinProperty, value);
    }

    #endregion AllowSpin

    #region ShowButtonSpinner

    public static readonly DependencyProperty ShowButtonSpinnerProperty =
        DependencyProperty.Register(
            nameof(ShowButtonSpinner),
            typeof(bool),
            typeof(NumericUpDown),
            new PropertyMetadata(true));

    /// <summary>Controls the visibility of the up/down buttons. Mirrors Ursa's <c>ShowButtonSpinner</c>.</summary>
    public bool ShowButtonSpinner
    {
        get => (bool)GetValue(ShowButtonSpinnerProperty);
        set => SetValue(ShowButtonSpinnerProperty, value);
    }

    #endregion ShowButtonSpinner

    #region Spinned routed event

    public static readonly RoutedEvent SpinnedEvent =
        EventManager.RegisterRoutedEvent(
            nameof(Spinned),
            RoutingStrategy.Bubble,
            typeof(EventHandler<SpinEventArgs>),
            typeof(NumericUpDown));

    /// <summary>Raised whenever a spin action occurs. Mirrors Ursa's <c>Spinned</c>.</summary>
    public event EventHandler<SpinEventArgs> Spinned
    {
        add => AddHandler(SpinnedEvent, value);
        remove => RemoveHandler(SpinnedEvent, value);
    }

    #endregion Spinned routed event

    // --- Template wiring ----------------------------------------------------

    static NumericUpDown()
    {
        FocusableProperty.OverrideMetadata(typeof(NumericUpDown), new FrameworkPropertyMetadata(true));
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        // Detach old handlers
        _increaseButton?.Click -= OnIncreaseButtonClick;
        _decreaseButton?.Click -= OnDecreaseButtonClick;
        if (_textBox != null)
        {
            _textBox.TextChanged -= OnTextBoxTextChanged;
            _textBox.PreviewKeyDown -= OnTextBoxPreviewKeyDown;
            _textBox.PreviewTextInput -= OnTextBoxPreviewTextInput;
        }
        if (_dragPanel != null)
        {
            _dragPanel.MouseLeftButtonDown -= OnDragPanelMouseDown;
            _dragPanel.MouseMove -= OnDragPanelMouseMove;
            _dragPanel.MouseLeftButtonUp -= OnDragPanelMouseUp;
        }

        _textBox = GetTemplateChild(PART_TextBox) as TextBox;
        _increaseButton = GetTemplateChild(PART_IncreaseButton) as RepeatButton;
        _decreaseButton = GetTemplateChild(PART_DecreaseButton) as RepeatButton;
        _dragPanel = GetTemplateChild(PART_DragPanel) as UIElement;
        _innerLeftContent = GetTemplateChild("PART_InnerLeftContent") as ContentPresenter;
        _innerRightContent = GetTemplateChild("PART_InnerRightContent") as ContentPresenter;

        if (_textBox != null)
        {
            _textBox.IsReadOnly = IsReadOnly;
            _textBox.TextChanged += OnTextBoxTextChanged;
            _textBox.PreviewKeyDown += OnTextBoxPreviewKeyDown;
            _textBox.PreviewTextInput += OnTextBoxPreviewTextInput;
            UpdateInputMethodState();
        }

        _increaseButton?.Click += OnIncreaseButtonClick;
        _decreaseButton?.Click += OnDecreaseButtonClick;

        if (_dragPanel != null)
        {
            _dragPanel.Visibility = AllowDrag ? Visibility.Visible : Visibility.Collapsed;
            _dragPanel.MouseLeftButtonDown += OnDragPanelMouseDown;
            _dragPanel.MouseMove += OnDragPanelMouseMove;
            _dragPanel.MouseLeftButtonUp += OnDragPanelMouseUp;
        }

        UpdateInnerContentVisibility();
        SyncTextAndValue(false, null, true);
        SetValidSpinDirection();
    }

    // --- Button clicks -------------------------------------------------------

    private void OnIncreaseButtonClick(object sender, RoutedEventArgs e)
    {
        if (AllowSpin && !IsReadOnly && _canIncrease)
        {
            var args = new SpinEventArgs(SpinnedEvent, SpinDirection.Increase);
            RaiseEvent(args);
            Increase();
        }
    }

    private void OnDecreaseButtonClick(object sender, RoutedEventArgs e)
    {
        if (AllowSpin && !IsReadOnly && _canDecrease)
        {
            var args = new SpinEventArgs(SpinnedEvent, SpinDirection.Decrease);
            RaiseEvent(args);
            Decrease();
        }
    }

    // --- Inner TextBox changes ------------------------------------------------

    private void OnTextBoxTextChanged(object sender, TextChangedEventArgs e)
    {
        if (_textBox is null) return;

        // Post-input sanitisation: strip any character that is illegal for this
        // numeric type. This runs AFTER the text is already in the TextBox, so it
        // catches IME composition commits, paste, drag-drop, and every other input
        // path that bypasses PreviewTextInput.
        if (RestrictInput && !_isRestrictingInput)
        {
            _isRestrictingInput = true;
            try { SanitizeRestrictedText(); }
            finally { _isRestrictingInput = false; }
        }

        _updateFromTextInput = true;
        try
        {
            // Only update the spin-direction validity while typing; do NOT commit yet.
            SyncTextAndValue(false, _textBox.Text, false);
        }
        finally
        {
            _updateFromTextInput = false;
        }
    }

    /// <summary>
    /// Removes every character from the TextBox that is not valid for the current
    /// numeric type, then restores the caret position as closely as possible.
    /// Called only when <see cref="RestrictInput"/> is <see langword="true"/>.
    /// </summary>
    private void SanitizeRestrictedText()
    {
        if (_textBox is null) return;

        var raw = _textBox.Text;
        var decSep = NumberFormat?.NumberDecimalSeparator ?? ".";
        char decChar = decSep.Length == 1 ? decSep[0] : '.';
        bool allowDecimal = IsFloatingPointInput;
        bool allowNegative = IsNegativeInputAllowed();

        var sb = new System.Text.StringBuilder(raw.Length);
        bool hasDecSep = false;
        bool hasMinus = false;

        foreach (char c in raw)
        {
            if (char.IsDigit(c))
            {
                sb.Append(c);
            }
            else if (c == '-' && !hasMinus && sb.Length == 0 && allowNegative)
            {
                hasMinus = true;
                sb.Append(c);
            }
            else if (c == decChar && allowDecimal && !hasDecSep)
            {
                hasDecSep = true;
                sb.Append(c);
            }
            // All other characters are silently dropped.
        }

        var cleaned = sb.ToString();
        if (cleaned == raw) return;

        // Restore the caret at the same logical offset (clamped to new length).
        int caret = Math.Min(_textBox.CaretIndex, cleaned.Length);
        _textBox.Text = cleaned;
        _textBox.CaretIndex = caret;
    }

    private void OnTextBoxPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        // Best-effort early rejection for direct keyboard input (before the char
        // reaches the TextBox). IME commits and paste are handled in SanitizeRestrictedText.
        if (!RestrictInput || _textBox is null) return;

        var decSep = NumberFormat?.NumberDecimalSeparator ?? ".";
        char decChar = decSep.Length == 1 ? decSep[0] : '.';

        foreach (char c in e.Text)
        {
            if (char.IsDigit(c)) continue;

            if (c == '-')
            {
                if (!IsNegativeInputAllowed()) { e.Handled = true; return; }
                continue;
            }

            if (c == decChar)
            {
                if (!IsFloatingPointInput) { e.Handled = true; return; }
                var textWithoutSelection = _textBox.Text.Remove(
                    _textBox.SelectionStart, _textBox.SelectionLength);
                if (textWithoutSelection.Contains(decSep)) { e.Handled = true; return; }
                continue;
            }

            // Letter, IME composition character, punctuation … → block.
            e.Handled = true;
            return;
        }
    }

    private void OnTextBoxPreviewKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Up:
                if (AllowSpin && !IsReadOnly && _canIncrease)
                {
                    RaiseEvent(new SpinEventArgs(SpinnedEvent, SpinDirection.Increase));
                    Increase();
                    e.Handled = true;
                }
                break;

            case Key.Down:
                if (AllowSpin && !IsReadOnly && _canDecrease)
                {
                    RaiseEvent(new SpinEventArgs(SpinnedEvent, SpinDirection.Decrease));
                    Decrease();
                    e.Handled = true;
                }
                break;

            case Key.Enter:
                var ok = CommitInput(true);
                e.Handled = !ok;
                break;

            case Key.Escape:
                if (AllowDrag && _dragPanel != null)
                {
                    _dragPanel.Visibility = Visibility.Visible;
                    SyncTextAndValue(false, null, true); // restore display text
                }
                e.Handled = true;
                break;
        }
    }

    // --- Mouse wheel ---------------------------------------------------------

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);
        if (!AllowSpin || IsReadOnly || !IsKeyboardFocusWithin) return;
        if (e.Delta > 0 && _canIncrease)
        {
            RaiseEvent(new SpinEventArgs(SpinnedEvent, SpinDirection.Increase, usingMouseWheel: true));
            Increase();
            e.Handled = true;
        }
        else if (e.Delta < 0 && _canDecrease)
        {
            RaiseEvent(new SpinEventArgs(SpinnedEvent, SpinDirection.Decrease, usingMouseWheel: true));
            Decrease();
            e.Handled = true;
        }
    }

    // --- Drag panel ----------------------------------------------------------

    private void OnDragPanelMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        _dragStartPoint = e.GetPosition(this);

        if (e.ClickCount == 2 && AllowDrag)
        {
            // Double-click: enter edit mode
            _dragPanel?.Visibility = Visibility.Collapsed;
            _textBox?.Focus();
            _textBox?.IsReadOnly = IsReadOnly;
            e.Handled = true;
        }
        else
        {
            // Single click: focus but stay in drag mode (read-only textbox)
            _textBox?.Focus();
            _textBox?.IsReadOnly = true;
            Mouse.Capture((IInputElement)sender);
        }
    }

    private void OnDragPanelMouseMove(object sender, MouseEventArgs e)
    {
        if (!AllowDrag || IsReadOnly) return;
        if (!e.LeftButton.HasFlag(MouseButtonState.Pressed)) return;
        if (_dragStartPoint is null) return;

        var pos = e.GetPosition(this);
        var delta = pos - _dragStartPoint.Value;
        int d = GetDragDelta(delta);

        if (d > 0 && _canIncrease) Increase();
        else if (d < 0 && _canDecrease) Decrease();

        _dragStartPoint = pos;
    }

    private void OnDragPanelMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        _dragStartPoint = null;
        Mouse.Capture(null);
    }

    private static int GetDragDelta(Vector delta)
    {
        bool horizontal = Math.Abs(delta.X) > Math.Abs(delta.Y);
        double v = horizontal ? delta.X : -delta.Y;
        return v switch { > 0 => 1, < 0 => -1, _ => 0 };
    }

    private void UpdateDragPanelVisibility()
    {
        _dragPanel?.Visibility = AllowDrag ? Visibility.Visible : Visibility.Collapsed;
    }

    // --- Focus ---------------------------------------------------------------

    protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnIsKeyboardFocusWithinChanged(e);
        if (!(bool)e.NewValue)
        {
            // Lost all keyboard focus → commit
            CommitInput(true);
            if (AllowDrag && _dragPanel != null)
                _dragPanel.Visibility = Visibility.Visible;
        }
    }

    protected override void OnGotFocus(RoutedEventArgs e)
    {
        base.OnGotFocus(e);
        _textBox?.Focus();
    }

    // --- Virtual helpers for RestrictInput ---------------------------------

    /// <summary>
    /// Returns <see langword="true"/> when the numeric type accepts a fractional part
    /// (double, float, decimal). Override in concrete classes.
    /// </summary>
    protected virtual bool IsFloatingPointInput => false;

    /// <summary>
    /// Returns <see langword="true"/> when a minus sign is currently a valid first
    /// character (i.e. the current Minimum allows negative values).
    /// Override in <see cref="NumericUpDownBase{T}"/>.
    /// </summary>
    protected virtual bool IsNegativeInputAllowed() => true;

    // --- Abstract interface implemented by NumericUpDownBase<T> -------------

    protected abstract void SetValidSpinDirection();

    protected abstract void Increase();

    protected abstract void Decrease();

    protected virtual bool CommitInput(bool forceTextUpdate = false)
        => SyncTextAndValue(true, _textBox?.Text, forceTextUpdate);

    protected abstract bool SyncTextAndValue(bool fromTextToValue = false, string? text = null, bool forceTextUpdate = false);

    public abstract void Clear();
}
