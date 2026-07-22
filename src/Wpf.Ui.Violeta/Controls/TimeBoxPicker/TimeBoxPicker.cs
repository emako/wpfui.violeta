using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Wpf.Ui.Violeta.Resources.Localization;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// A TextBox + drop-down button control (mirroring <see cref="CalendarDateTimePicker"/>'s look)
/// whose popup renders a <see cref="TimeBoxPickerPresenter"/> — one drum-roll column per
/// Hour/Minute/Second/Millisecond field in <see cref="Format"/>. There is no AM/PM selection;
/// <see cref="ClockIdentifier"/> only controls whether the committed text renders in 12- or
/// 24-hour notation (via 'h'/'H' + optional 'tt' in <see cref="Format"/>).
/// </summary>
[TemplatePart(Name = PartTextBox, Type = typeof(TextBox))]
[TemplatePart(Name = PartButton, Type = typeof(Button))]
[TemplatePart(Name = PartPopup, Type = typeof(Popup))]
[TemplatePart(Name = PartPresenter, Type = typeof(TimeBoxPickerPresenter))]
[TemplatePart(Name = PartAcceptButton, Type = typeof(Button))]
[TemplatePart(Name = PartDismissButton, Type = typeof(Button))]
public class TimeBoxPicker : Control
{
    public const string PartTextBox = "PART_TextBox";
    public const string PartButton = "PART_Button";
    public const string PartPopup = "PART_Popup";
    public const string PartPresenter = "PART_Presenter";
    public const string PartAcceptButton = "PART_AcceptButton";
    public const string PartDismissButton = "PART_DismissButton";

    private TextBox? _textBox;
    private Button? _button;
    private Popup? _popup;
    private TimeBoxPickerPresenter? _presenter;
    private Button? _acceptButton;
    private Button? _dismissButton;
    private Window? _parentWindow;
    private bool _windowHandlerRegistered;
    private bool _isUpdatingText;

    // ------------------------------------------------------------------
    // Dependency Properties
    // ------------------------------------------------------------------

    public static readonly DependencyProperty SelectedTimeProperty =
        DependencyProperty.Register(
            nameof(SelectedTime),
            typeof(TimeSpan?),
            typeof(TimeBoxPicker),
            new PropertyMetadata(null, OnSelectedTimeChanged));

    /// <summary>
    /// Optional .NET custom time format string, e.g. "HH:mm:ss", "hh:mm:ss.fff tt", "mm:ss".
    /// Supported field specifiers: H/HH, h/hh (both bind to the same 0-23 wheel — 'h' just
    /// makes the committed text render as 12-hour), m/mm, s/ss, f.../F... (millisecond digits).
    /// 'tt'/'t' (AM/PM designator) and any other characters are rendered as literal text; the
    /// popup does not offer an AM/PM picker, so a 12-hour format always displays the wheel's
    /// value converted to the corresponding AM/PM text. When null/empty, a format is derived
    /// from <see cref="ClockIdentifier"/> ("HH:mm:ss" or "hh:mm:ss tt").
    /// </summary>
    public static readonly DependencyProperty FormatProperty =
        DependencyProperty.Register(
            nameof(Format),
            typeof(string),
            typeof(TimeBoxPicker),
            new PropertyMetadata(null, OnFormatAffectingPropertyChanged));

    /// <summary>
    /// "12HourClock" or "24HourClock". Purely a text-rendering concern — see <see cref="Format"/>.
    /// </summary>
    public static readonly DependencyProperty ClockIdentifierProperty =
        DependencyProperty.Register(
            nameof(ClockIdentifier),
            typeof(string),
            typeof(TimeBoxPicker),
            new PropertyMetadata("24HourClock", OnFormatAffectingPropertyChanged));

    public static readonly DependencyProperty PlaceholderTextProperty =
        DependencyProperty.Register(
            nameof(PlaceholderText),
            typeof(string),
            typeof(TimeBoxPicker),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register(
            nameof(Header),
            typeof(object),
            typeof(TimeBoxPicker),
            new PropertyMetadata(null));

    public static readonly DependencyProperty HeaderTemplateProperty =
        DependencyProperty.Register(
            nameof(HeaderTemplate),
            typeof(DataTemplate),
            typeof(TimeBoxPicker),
            new PropertyMetadata(null));

    public static readonly DependencyProperty IsDropDownOpenProperty =
        DependencyProperty.Register(
            nameof(IsDropDownOpen),
            typeof(bool),
            typeof(TimeBoxPicker),
            new PropertyMetadata(false, OnIsDropDownOpenChanged));

    public static readonly RoutedEvent SelectedTimeChangedEvent =
        EventManager.RegisterRoutedEvent(
            nameof(SelectedTimeChanged),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(TimeBoxPicker));

    // ------------------------------------------------------------------
    // Properties
    // ------------------------------------------------------------------

    public TimeSpan? SelectedTime
    {
        get => (TimeSpan?)GetValue(SelectedTimeProperty);
        set => SetValue(SelectedTimeProperty, value);
    }

    public string? Format
    {
        get => (string?)GetValue(FormatProperty);
        set => SetValue(FormatProperty, value);
    }

    public string ClockIdentifier
    {
        get => (string)GetValue(ClockIdentifierProperty);
        set => SetValue(ClockIdentifierProperty, value);
    }

    public string PlaceholderText
    {
        get => (string)GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public DataTemplate? HeaderTemplate
    {
        get => (DataTemplate?)GetValue(HeaderTemplateProperty);
        set => SetValue(HeaderTemplateProperty, value);
    }

    public bool IsDropDownOpen
    {
        get => (bool)GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }

    public event RoutedEventHandler SelectedTimeChanged
    {
        add => AddHandler(SelectedTimeChangedEvent, value);
        remove => RemoveHandler(SelectedTimeChangedEvent, value);
    }

    // ------------------------------------------------------------------
    // Constructor
    // ------------------------------------------------------------------

    static TimeBoxPicker()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(TimeBoxPicker),
            new FrameworkPropertyMetadata(typeof(TimeBoxPicker)));
    }

    public TimeBoxPicker()
    {
        if (ReadLocalValue(PlaceholderTextProperty) == DependencyProperty.UnsetValue)
        {
            SetCurrentValue(PlaceholderTextProperty, SH.PleaseSelect);
        }
    }

    // ------------------------------------------------------------------
    // Template
    // ------------------------------------------------------------------

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (_button != null)
            _button.Click -= OnButtonClick;
        if (_textBox != null)
        {
            _textBox.LostFocus -= OnTextBoxLostFocus;
            _textBox.PreviewKeyDown -= OnTextBoxPreviewKeyDown;
        }
        if (_acceptButton != null)
            _acceptButton.Click -= OnAcceptButtonClick;
        if (_dismissButton != null)
            _dismissButton.Click -= OnDismissButtonClick;
        if (_popup != null)
        {
            _popup.Opened -= OnPopupOpened;
            _popup.Closed -= OnPopupClosed;
        }

        _textBox = GetTemplateChild(PartTextBox) as TextBox;
        _button = GetTemplateChild(PartButton) as Button;
        _popup = GetTemplateChild(PartPopup) as Popup;
        _presenter = GetTemplateChild(PartPresenter) as TimeBoxPickerPresenter;
        _acceptButton = GetTemplateChild(PartAcceptButton) as Button;
        _dismissButton = GetTemplateChild(PartDismissButton) as Button;

        if (_button != null)
            _button.Click += OnButtonClick;

        if (_textBox != null)
        {
            _textBox.LostFocus += OnTextBoxLostFocus;
            _textBox.PreviewKeyDown += OnTextBoxPreviewKeyDown;
        }

        if (_acceptButton != null)
            _acceptButton.Click += OnAcceptButtonClick;

        if (_dismissButton != null)
            _dismissButton.Click += OnDismissButtonClick;

        if (_popup != null)
        {
            _popup.Opened += OnPopupOpened;
            _popup.Closed += OnPopupClosed;
        }

        UpdateText();
    }

    // ------------------------------------------------------------------
    // Event handlers
    // ------------------------------------------------------------------

    private void OnButtonClick(object? sender, RoutedEventArgs e)
    {
        SetCurrentValue(IsDropDownOpenProperty, !IsDropDownOpen);
    }

    private void OnAcceptButtonClick(object? sender, RoutedEventArgs e)
    {
        if (_presenter != null)
            SelectedTime = _presenter.GetSelectedTime();
        SetCurrentValue(IsDropDownOpenProperty, false);
    }

    private void OnDismissButtonClick(object? sender, RoutedEventArgs e)
    {
        SetCurrentValue(IsDropDownOpenProperty, false);
    }

    private void OnTextBoxLostFocus(object? sender, RoutedEventArgs e)
    {
        CommitTextInput();
    }

    private void OnTextBoxPreviewKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            CommitTextInput();
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            UpdateText();
            e.Handled = true;
        }
    }

    private void CommitTextInput()
    {
        if (_textBox == null || _isUpdatingText)
            return;

        string text = _textBox.Text;

        if (string.IsNullOrWhiteSpace(text))
        {
            SelectedTime = null;
            return;
        }

        if (DateTime.TryParseExact(text, GetEffectiveFormat(), CultureInfo.CurrentCulture, DateTimeStyles.NoCurrentDateDefault, out var exact))
        {
            SelectedTime = exact.TimeOfDay;
        }
        else if (TimeSpan.TryParse(text, CultureInfo.CurrentCulture, out var parsed))
        {
            SelectedTime = parsed;
        }
        else
        {
            // Revert to the last valid display text.
            UpdateText();
        }
    }

    private static void OnIsDropDownOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TimeBoxPicker picker || picker._popup == null)
            return;

        bool isOpen = (bool)e.NewValue;

        if (isOpen)
            picker.SyncPopupFromSelected();

        picker._popup.IsOpen = isOpen;
    }

    private void SyncPopupFromSelected()
    {
        if (_presenter != null)
        {
            _presenter.Format = GetEffectiveFormat();
            _presenter.Time = SelectedTime ?? TimeSpan.Zero;
        }
    }

    /// <summary>
    /// While the popup is open, block mouse-wheel events that don't occur over the popup
    /// content itself, so the host panel behind the control can't be scrolled — matching
    /// the standard <see cref="ComboBox"/> drop-down behavior (which achieves this via
    /// mouse capture).
    /// </summary>
    private void OnPopupOpened(object? sender, EventArgs e)
    {
        _parentWindow ??= Window.GetWindow(this);
        if (_parentWindow is not null && !_windowHandlerRegistered)
        {
            _parentWindow.PreviewMouseWheel += OnWindowPreviewMouseWheel;
            _windowHandlerRegistered = true;
        }
    }

    private void OnPopupClosed(object? sender, EventArgs e)
    {
        if (_parentWindow is not null && _windowHandlerRegistered)
        {
            _parentWindow.PreviewMouseWheel -= OnWindowPreviewMouseWheel;
            _windowHandlerRegistered = false;
        }

        if (IsDropDownOpen)
            SetCurrentValue(IsDropDownOpenProperty, false);
    }

    private void OnWindowPreviewMouseWheel(object? sender, MouseWheelEventArgs e)
    {
        if (e.OriginalSource is not DependencyObject target)
            return;

        if (_popup?.Child is UIElement popupChild && IsVisualDescendantOf(target, popupChild))
            return;

        e.Handled = true;
    }

    private static bool IsVisualDescendantOf(DependencyObject element, DependencyObject ancestor)
    {
        DependencyObject? current = element;
        while (current is not null)
        {
            if (current == ancestor) return true;
            current = VisualTreeHelper.GetParent(current);
        }
        return false;
    }

    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------

    private void UpdateText()
    {
        if (_textBox == null)
            return;

        _isUpdatingText = true;
        _textBox.Text = SelectedTime is { } t
            ? FormatTime(t, GetEffectiveFormat())
            : string.Empty;
        _isUpdatingText = false;
    }

    /// <summary>
    /// Resolves the format actually used for display/parsing: <see cref="Format"/> if set,
    /// otherwise one derived from <see cref="ClockIdentifier"/>.
    /// </summary>
    private string GetEffectiveFormat()
    {
        if (!string.IsNullOrEmpty(Format))
            return Format!;

        return ClockIdentifier == "12HourClock" ? "hh:mm:ss tt" : "HH:mm:ss";
    }

    /// <summary>
    /// Formats a <see cref="TimeSpan"/> using a .NET custom date/time format string. TimeSpan has
    /// no ToString overload that accepts date/time custom format strings (its own format specifiers
    /// mean something different), so the value is projected onto a DateTime (any base date works —
    /// only the time-of-day fields are used by the format specifiers this control supports) and
    /// formatted with the regular DateTime formatter, which understands 'h'/'H'/'tt' distinctly.
    /// </summary>
    private static string FormatTime(TimeSpan time, string format)
    {
        var dt = new DateTime(1, 1, 1) + time;
        return dt.ToString(format, CultureInfo.CurrentCulture);
    }

    private static void OnSelectedTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TimeBoxPicker picker)
        {
            picker.UpdateText();
            picker.RaiseEvent(new RoutedEventArgs(SelectedTimeChangedEvent, picker));
        }
    }

    private static void OnFormatAffectingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TimeBoxPicker picker)
            picker.UpdateText();
    }
}
