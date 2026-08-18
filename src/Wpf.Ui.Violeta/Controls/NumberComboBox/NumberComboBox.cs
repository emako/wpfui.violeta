using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Abstract non-generic base for NumberComboBox controls.
/// Editable ComboBox chrome with the same numeric text input rules as <see cref="NumericUpDown"/>,
/// without spin / up-down behaviour. Item selection is not required; <see cref="ComboBox"/>
/// drop-down infrastructure is kept for appearance and optional custom items.
/// </summary>
[TemplatePart(Name = PART_EditableTextBox, Type = typeof(TextBox))]
[TemplatePart(Name = PART_InnerLeftContent, Type = typeof(ContentPresenter))]
[TemplatePart(Name = PART_InnerRightContent, Type = typeof(ContentPresenter))]
public abstract class NumberComboBox : System.Windows.Controls.ComboBox
{
    public const string PART_EditableTextBox = "PART_EditableTextBox";
    public const string PART_InnerLeftContent = "PART_InnerLeftContent";
    public const string PART_InnerRightContent = "PART_InnerRightContent";

    protected TextBox? _textBox;
    private ContentPresenter? _innerLeftContent;
    private ContentPresenter? _innerRightContent;

    /// <summary>Whether the current text update is from user typing (vs programmatic).</summary>
    protected internal bool _updateFromTextInput;

    /// <summary>
    /// When true, ignore TextBox.TextChanged caused by aligning SelectedItem
    /// (editable ComboBox overwrites Text when selection changes).
    /// </summary>
    protected bool _suppressSelectionTextSync;

    private bool _isRestrictingInput;

    static NumberComboBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(NumberComboBox),
            new FrameworkPropertyMetadata(typeof(NumberComboBox)));

        IsEditableProperty.OverrideMetadata(
            typeof(NumberComboBox),
            new FrameworkPropertyMetadata(true, null, CoerceIsEditable));

        IsTextSearchEnabledProperty.OverrideMetadata(
            typeof(NumberComboBox),
            new FrameworkPropertyMetadata(false));
    }

    private static object CoerceIsEditable(DependencyObject d, object baseValue) => true;

    #region RestrictInput

    public static readonly DependencyProperty RestrictInputProperty =
        DependencyProperty.Register(
            nameof(RestrictInput),
            typeof(bool),
            typeof(NumberComboBox),
            new PropertyMetadata(true, OnRestrictInputChanged));

    private static void OnRestrictInputChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NumberComboBox self)
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

    #region PlaceholderText

    public static readonly DependencyProperty PlaceholderTextProperty =
        Wpf.Ui.Controls.ControlHelper.PlaceholderTextProperty.AddOwner(typeof(NumberComboBox));

    /// <summary>Placeholder shown when the value is null/empty.</summary>
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
            typeof(NumberComboBox),
            new PropertyMetadata(NumberFormatInfo.CurrentInfo, OnFormatPropertyChanged));

    private static void OnFormatPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NumberComboBox self && self.IsLoaded)
            self.SyncTextAndValue(false, null, true);
    }

    /// <summary>Format info used when parsing and formatting numbers.</summary>
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
            typeof(NumberComboBox),
            new PropertyMetadata(string.Empty, OnFormatPropertyChanged));

    /// <summary>
    /// .NET format string applied when converting the value to text (e.g. <c>"N2"</c>, <c>"X"</c>).
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
            typeof(NumberComboBox),
            new PropertyMetadata(NumberStyles.Any));

    /// <summary><see cref="NumberStyles"/> flags used when parsing the text input.</summary>
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
            typeof(NumberComboBox),
            new PropertyMetadata(null, OnFormatPropertyChanged));

    /// <summary>
    /// Optional <see cref="IValueConverter"/> that overrides the default text↔value conversion.
    /// </summary>
    public IValueConverter? TextConverter
    {
        get => (IValueConverter?)GetValue(TextConverterProperty);
        set => SetValue(TextConverterProperty, value);
    }

    #endregion TextConverter

    #region InnerLeftContent / InnerRightContent

    public static readonly DependencyProperty InnerLeftContentProperty =
        DependencyProperty.Register(
            nameof(InnerLeftContent),
            typeof(object),
            typeof(NumberComboBox),
            new PropertyMetadata(null, OnInnerContentChanged));

    public static readonly DependencyProperty InnerRightContentProperty =
        DependencyProperty.Register(
            nameof(InnerRightContent),
            typeof(object),
            typeof(NumberComboBox),
            new PropertyMetadata(null, OnInnerContentChanged));

    private static void OnInnerContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NumberComboBox self)
            self.UpdateInnerContentVisibility();
    }

    private void UpdateInnerContentVisibility()
    {
        if (_innerLeftContent != null)
            _innerLeftContent.Visibility = InnerLeftContent != null ? Visibility.Visible : Visibility.Collapsed;
        if (_innerRightContent != null)
            _innerRightContent.Visibility = InnerRightContent != null ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <summary>Optional content rendered to the left of the text input (e.g. currency symbol).</summary>
    public object? InnerLeftContent
    {
        get => GetValue(InnerLeftContentProperty);
        set => SetValue(InnerLeftContentProperty, value);
    }

    /// <summary>Optional content rendered to the right of the text input (e.g. unit suffix).</summary>
    public object? InnerRightContent
    {
        get => GetValue(InnerRightContentProperty);
        set => SetValue(InnerRightContentProperty, value);
    }

    #endregion InnerLeftContent / InnerRightContent

    public override void OnApplyTemplate()
    {
        if (_textBox != null)
        {
            _textBox.TextChanged -= OnTextBoxTextChanged;
            _textBox.PreviewKeyDown -= OnTextBoxPreviewKeyDown;
            _textBox.PreviewTextInput -= OnTextBoxPreviewTextInput;
        }

        base.OnApplyTemplate();

        _textBox = GetTemplateChild(PART_EditableTextBox) as TextBox;
        _innerLeftContent = GetTemplateChild(PART_InnerLeftContent) as ContentPresenter;
        _innerRightContent = GetTemplateChild(PART_InnerRightContent) as ContentPresenter;
        UpdateInnerContentVisibility();

        if (_textBox != null)
        {
            _textBox.TextChanged += OnTextBoxTextChanged;
            _textBox.PreviewKeyDown += OnTextBoxPreviewKeyDown;
            _textBox.PreviewTextInput += OnTextBoxPreviewTextInput;
            UpdateInputMethodState();
        }

        SyncTextAndValue(false, null, true);
    }

    private void OnTextBoxTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (_textBox is null || _suppressSelectionTextSync) return;

        if (RestrictInput && !_isRestrictingInput)
        {
            _isRestrictingInput = true;
            try { SanitizeRestrictedText(); }
            finally { _isRestrictingInput = false; }
        }

        _updateFromTextInput = true;
        try
        {
            // Keep Value (and thus the dropdown pill) in sync while typing.
            SyncTextAndValue(true, _textBox.Text, false);
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
        }

        var cleaned = sb.ToString();
        if (cleaned == raw) return;

        int caret = Math.Min(_textBox.CaretIndex, cleaned.Length);
        _textBox.Text = cleaned;
        _textBox.CaretIndex = caret;
    }

    private void OnTextBoxPreviewTextInput(object? sender, TextCompositionEventArgs e)
    {
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

            e.Handled = true;
            return;
        }
    }

    private void OnTextBoxPreviewKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;

        var ok = CommitInput(true);
        e.Handled = !ok;
    }

    protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnIsKeyboardFocusWithinChanged(e);
        if (!(bool)e.NewValue)
            CommitInput(true);
    }

    protected override void OnDropDownOpened(EventArgs e)
    {
        // Opening the popup keeps keyboard focus within the control, so focus-lost
        // commit never runs — sync from the visible text before items paint.
        CommitInput(false);
        base.OnDropDownOpened(e);
    }

    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        base.OnSelectionChanged(e);
        if (SelectedItem is not null && !_updateFromTextInput)
            ApplySelectedItem();
    }

    /// <summary>
    /// Applies the current <see cref="ComboBox.SelectedItem"/> to the numeric value.
    /// Override in <see cref="NumberComboBoxBase{T}"/> to prefer typed items.
    /// </summary>
    protected virtual void ApplySelectedItem() => CommitInput(true);

    /// <summary>
    /// Returns <see langword="true"/> when the numeric type accepts a fractional part
    /// (double, float, decimal). Override in concrete classes.
    /// </summary>
    protected virtual bool IsFloatingPointInput => false;

    /// <summary>
    /// Returns <see langword="true"/> when a minus sign is currently a valid first
    /// character (i.e. the current Minimum allows negative values).
    /// Override in <see cref="NumberComboBoxBase{T}"/>.
    /// </summary>
    protected virtual bool IsNegativeInputAllowed() => true;

    protected virtual bool CommitInput(bool forceTextUpdate = false)
        => SyncTextAndValue(true, _textBox?.Text ?? Text, forceTextUpdate);

    protected abstract bool SyncTextAndValue(bool fromTextToValue = false, string? text = null, bool forceTextUpdate = false);

    public abstract void Clear();
}
