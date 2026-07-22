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
/// A TextBox + drop-down button control (mirroring <see cref="System.Windows.Controls.DatePicker"/>'s
/// CalendarDatePicker-styled look) whose popup renders a <see cref="System.Windows.Controls.Calendar"/>
/// side-by-side with a <see cref="TimePickerPresenter"/>, so a single control can pick both date and time.
/// </summary>
[TemplatePart(Name = PartTextBox, Type = typeof(TextBox))]
[TemplatePart(Name = PartButton, Type = typeof(Button))]
[TemplatePart(Name = PartPopup, Type = typeof(Popup))]
[TemplatePart(Name = PartCalendar, Type = typeof(System.Windows.Controls.Calendar))]
[TemplatePart(Name = PartTimePresenter, Type = typeof(TimePickerPresenter))]
[TemplatePart(Name = PartAcceptButton, Type = typeof(Button))]
[TemplatePart(Name = PartDismissButton, Type = typeof(Button))]
public class CalendarDateTimePicker : Control
{
    public const string PartTextBox = "PART_TextBox";
    public const string PartButton = "PART_Button";
    public const string PartPopup = "PART_Popup";
    public const string PartCalendar = "PART_Calendar";
    public const string PartTimePresenter = "PART_TimePresenter";
    public const string PartAcceptButton = "PART_AcceptButton";
    public const string PartDismissButton = "PART_DismissButton";

    private TextBox? _textBox;
    private Button? _button;
    private Popup? _popup;
    private System.Windows.Controls.Calendar? _calendar;
    private TimePickerPresenter? _timePresenter;
    private Button? _acceptButton;
    private Button? _dismissButton;
    private Window? _parentWindow;
    private bool _windowHandlerRegistered;
    private bool _isUpdatingText;

    // ------------------------------------------------------------------
    // Dependency Properties
    // ------------------------------------------------------------------

    public static readonly DependencyProperty SelectedDateTimeProperty =
        DependencyProperty.Register(
            nameof(SelectedDateTime),
            typeof(DateTime?),
            typeof(CalendarDateTimePicker),
            new PropertyMetadata(null, OnSelectedDateTimeChanged));

    public static readonly DependencyProperty UseSecondsProperty =
        DependencyProperty.Register(
            nameof(UseSeconds),
            typeof(bool),
            typeof(CalendarDateTimePicker),
            new PropertyMetadata(false, OnFormatAffectingPropertyChanged));

    /// <summary>
    /// "12HourClock" or "24HourClock"
    /// </summary>
    public static readonly DependencyProperty ClockIdentifierProperty =
        DependencyProperty.Register(
            nameof(ClockIdentifier),
            typeof(string),
            typeof(CalendarDateTimePicker),
            new PropertyMetadata("24HourClock", OnFormatAffectingPropertyChanged));

    /// <summary>
    /// Optional explicit display format. When null, a format is derived from
    /// <see cref="UseSeconds"/> and <see cref="ClockIdentifier"/>.
    /// </summary>
    public static readonly DependencyProperty SelectedDateTimeFormatProperty =
        DependencyProperty.Register(
            nameof(SelectedDateTimeFormat),
            typeof(string),
            typeof(CalendarDateTimePicker),
            new PropertyMetadata(null, OnFormatAffectingPropertyChanged));

    public static readonly DependencyProperty PlaceholderTextProperty =
        DependencyProperty.Register(
            nameof(PlaceholderText),
            typeof(string),
            typeof(CalendarDateTimePicker),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register(
            nameof(Header),
            typeof(object),
            typeof(CalendarDateTimePicker),
            new PropertyMetadata(null));

    public static readonly DependencyProperty HeaderTemplateProperty =
        DependencyProperty.Register(
            nameof(HeaderTemplate),
            typeof(DataTemplate),
            typeof(CalendarDateTimePicker),
            new PropertyMetadata(null));

    public static readonly DependencyProperty IsDropDownOpenProperty =
        DependencyProperty.Register(
            nameof(IsDropDownOpen),
            typeof(bool),
            typeof(CalendarDateTimePicker),
            new PropertyMetadata(false, OnIsDropDownOpenChanged));

    public static readonly RoutedEvent SelectedDateTimeChangedEvent =
        EventManager.RegisterRoutedEvent(
            nameof(SelectedDateTimeChanged),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(CalendarDateTimePicker));

    // ------------------------------------------------------------------
    // Properties
    // ------------------------------------------------------------------

    public DateTime? SelectedDateTime
    {
        get => (DateTime?)GetValue(SelectedDateTimeProperty);
        set => SetValue(SelectedDateTimeProperty, value);
    }

    public bool UseSeconds
    {
        get => (bool)GetValue(UseSecondsProperty);
        set => SetValue(UseSecondsProperty, value);
    }

    public string ClockIdentifier
    {
        get => (string)GetValue(ClockIdentifierProperty);
        set => SetValue(ClockIdentifierProperty, value);
    }

    public string? SelectedDateTimeFormat
    {
        get => (string?)GetValue(SelectedDateTimeFormatProperty);
        set => SetValue(SelectedDateTimeFormatProperty, value);
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

    public event RoutedEventHandler SelectedDateTimeChanged
    {
        add => AddHandler(SelectedDateTimeChangedEvent, value);
        remove => RemoveHandler(SelectedDateTimeChangedEvent, value);
    }

    // ------------------------------------------------------------------
    // Constructor
    // ------------------------------------------------------------------

    static CalendarDateTimePicker()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(CalendarDateTimePicker),
            new FrameworkPropertyMetadata(typeof(CalendarDateTimePicker)));
    }

    public CalendarDateTimePicker()
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
        _calendar = GetTemplateChild(PartCalendar) as System.Windows.Controls.Calendar;
        _timePresenter = GetTemplateChild(PartTimePresenter) as TimePickerPresenter;
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
        DateTime datePart = _calendar?.SelectedDate?.Date ?? (SelectedDateTime?.Date ?? DateTime.Today);
        TimeSpan timePart = _timePresenter?.GetSelectedTime() ?? (SelectedDateTime?.TimeOfDay ?? TimeSpan.Zero);
        SelectedDateTime = datePart + timePart;
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
            SelectedDateTime = null;
            return;
        }

        if (DateTime.TryParse(text, CultureInfo.CurrentCulture, DateTimeStyles.AllowWhiteSpaces, out var parsed))
        {
            SelectedDateTime = parsed;
        }
        else
        {
            // Revert to the last valid display text.
            UpdateText();
        }
    }

    private static void OnIsDropDownOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not CalendarDateTimePicker picker || picker._popup == null)
            return;

        bool isOpen = (bool)e.NewValue;

        if (isOpen)
            picker.SyncPopupFromSelected();

        picker._popup.IsOpen = isOpen;
    }

    private void SyncPopupFromSelected()
    {
        DateTime baseline = SelectedDateTime ?? DateTime.Now;

        if (_calendar != null)
        {
            _calendar.SelectedDate = baseline.Date;
            _calendar.DisplayDate = baseline.Date;
        }

        if (_timePresenter != null)
        {
            _timePresenter.ClockIdentifier = ClockIdentifier;
            _timePresenter.UseSeconds = UseSeconds;
            _timePresenter.Time = baseline.TimeOfDay;
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

    private string GetEffectiveFormat()
    {
        if (!string.IsNullOrEmpty(SelectedDateTimeFormat))
            return SelectedDateTimeFormat!;

        bool is12h = ClockIdentifier == "12HourClock";
        string timeFormat = is12h
            ? (UseSeconds ? "hh:mm:ss tt" : "hh:mm tt")
            : (UseSeconds ? "HH:mm:ss" : "HH:mm");

        return $"yyyy-MM-dd {timeFormat}";
    }

    private void UpdateText()
    {
        if (_textBox == null)
            return;

        _isUpdatingText = true;
        _textBox.Text = SelectedDateTime is { } dt
            ? dt.ToString(GetEffectiveFormat(), CultureInfo.CurrentCulture)
            : string.Empty;
        _isUpdatingText = false;
    }

    private static void OnSelectedDateTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CalendarDateTimePicker picker)
        {
            picker.UpdateText();
            picker.RaiseEvent(new RoutedEventArgs(SelectedDateTimeChangedEvent, picker));
        }
    }

    private static void OnFormatAffectingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CalendarDateTimePicker picker)
            picker.UpdateText();
    }
}
