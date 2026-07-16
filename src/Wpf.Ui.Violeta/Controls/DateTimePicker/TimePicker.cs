using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Wpf.Ui.Violeta.Resources.Localization;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// A drum-roll style time picker, mirroring Semi.Avalonia's TimePicker.
/// Opens a popup with scrollable columns for hour, minute (and optionally second + AM/PM).
/// </summary>
[TemplatePart(Name = PartFlyoutButton, Type = typeof(Button))]
[TemplatePart(Name = PartFirstPickerHost, Type = typeof(Border))]
[TemplatePart(Name = PartSecondPickerHost, Type = typeof(Border))]
[TemplatePart(Name = PartThirdPickerHost, Type = typeof(Border))]
[TemplatePart(Name = PartHourTextBlock, Type = typeof(TextBlock))]
[TemplatePart(Name = PartMinuteTextBlock, Type = typeof(TextBlock))]
[TemplatePart(Name = PartSecondTextBlock, Type = typeof(TextBlock))]
[TemplatePart(Name = PartPeriodTextBlock, Type = typeof(TextBlock))]
[TemplatePart(Name = PartPopup, Type = typeof(Popup))]
[TemplatePart(Name = PartPresenter, Type = typeof(TimePickerPresenter))]
public class TimePicker : Control
{
    public const string PartFlyoutButton = "PART_FlyoutButton";
    public const string PartFirstPickerHost = "PART_FirstPickerHost";
    public const string PartSecondPickerHost = "PART_SecondPickerHost";
    public const string PartThirdPickerHost = "PART_ThirdPickerHost";
    public const string PartHourTextBlock = "PART_HourTextBlock";
    public const string PartMinuteTextBlock = "PART_MinuteTextBlock";
    public const string PartSecondTextBlock = "PART_SecondTextBlock";
    public const string PartPeriodTextBlock = "PART_PeriodTextBlock";
    public const string PartPopup = "PART_Popup";
    public const string PartPresenter = "PART_Presenter";

    private Button? _flyoutButton;
    private TextBlock? _hourTextBlock;
    private TextBlock? _minuteTextBlock;
    private TextBlock? _secondTextBlock;
    private TextBlock? _periodTextBlock;
    private Popup? _popup;
    private TimePickerPresenter? _presenter;

    // ------------------------------------------------------------------
    // Dependency Properties
    // ------------------------------------------------------------------

    public static readonly DependencyProperty SelectedTimeProperty =
        DependencyProperty.Register(
            nameof(SelectedTime),
            typeof(TimeSpan?),
            typeof(TimePicker),
            new PropertyMetadata(null, OnSelectedTimeChanged));

    public static readonly DependencyProperty UseSecondsProperty =
        DependencyProperty.Register(
            nameof(UseSeconds),
            typeof(bool),
            typeof(TimePicker),
            new PropertyMetadata(false));

    public static readonly DependencyProperty ClockIdentifierProperty =
        DependencyProperty.Register(
            nameof(ClockIdentifier),
            typeof(string),
            typeof(TimePicker),
            new PropertyMetadata("24HourClock"));

    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register(
            nameof(Header),
            typeof(object),
            typeof(TimePicker),
            new PropertyMetadata(null));

    public static readonly DependencyProperty HeaderTemplateProperty =
        DependencyProperty.Register(
            nameof(HeaderTemplate),
            typeof(DataTemplate),
            typeof(TimePicker),
            new PropertyMetadata(null));

    public static readonly DependencyProperty HourTextProperty =
        DependencyProperty.Register(
            nameof(HourText),
            typeof(string),
            typeof(TimePicker),
            new PropertyMetadata(SH.TimePickerHourText));

    public static readonly DependencyProperty MinuteTextProperty =
        DependencyProperty.Register(
            nameof(MinuteText),
            typeof(string),
            typeof(TimePicker),
            new PropertyMetadata(SH.TimePickerMinuteText));

    public static readonly DependencyProperty SecondTextProperty =
        DependencyProperty.Register(
            nameof(SecondText),
            typeof(string),
            typeof(TimePicker),
            new PropertyMetadata(SH.TimePickerSecondText));

    public static readonly RoutedEvent SelectedTimeChangedEvent =
        EventManager.RegisterRoutedEvent(
            nameof(SelectedTimeChanged),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(TimePicker));

    // ------------------------------------------------------------------
    // Properties
    // ------------------------------------------------------------------

    public TimeSpan? SelectedTime
    {
        get => (TimeSpan?)GetValue(SelectedTimeProperty);
        set => SetValue(SelectedTimeProperty, value);
    }

    public bool UseSeconds
    {
        get => (bool)GetValue(UseSecondsProperty);
        set => SetValue(UseSecondsProperty, value);
    }

    /// <summary>
    /// "12HourClock" or "24HourClock"
    /// </summary>
    public string ClockIdentifier
    {
        get => (string)GetValue(ClockIdentifierProperty);
        set => SetValue(ClockIdentifierProperty, value);
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

    public string HourText
    {
        get => (string)GetValue(HourTextProperty);
        set => SetValue(HourTextProperty, value);
    }

    public string MinuteText
    {
        get => (string)GetValue(MinuteTextProperty);
        set => SetValue(MinuteTextProperty, value);
    }

    public string SecondText
    {
        get => (string)GetValue(SecondTextProperty);
        set => SetValue(SecondTextProperty, value);
    }

    public event RoutedEventHandler SelectedTimeChanged
    {
        add => AddHandler(SelectedTimeChangedEvent, value);
        remove => RemoveHandler(SelectedTimeChangedEvent, value);
    }

    // ------------------------------------------------------------------
    // Constructor
    // ------------------------------------------------------------------

    static TimePicker()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(TimePicker),
            new FrameworkPropertyMetadata(typeof(TimePicker)));
    }

    // ------------------------------------------------------------------
    // Template
    // ------------------------------------------------------------------

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (_flyoutButton != null)
            _flyoutButton.Click -= OnFlyoutButtonClick;
        if (_presenter != null)
        {
            _presenter.Confirmed -= OnPresenterConfirmed;
            _presenter.Dismissed -= OnPresenterDismissed;
        }

        _flyoutButton = GetTemplateChild(PartFlyoutButton) as Button;
        _hourTextBlock = GetTemplateChild(PartHourTextBlock) as TextBlock;
        _minuteTextBlock = GetTemplateChild(PartMinuteTextBlock) as TextBlock;
        _secondTextBlock = GetTemplateChild(PartSecondTextBlock) as TextBlock;
        _periodTextBlock = GetTemplateChild(PartPeriodTextBlock) as TextBlock;
        _popup = GetTemplateChild(PartPopup) as Popup;
        _presenter = GetTemplateChild(PartPresenter) as TimePickerPresenter;

        if (_flyoutButton != null)
            _flyoutButton.Click += OnFlyoutButtonClick;

        if (_presenter != null)
        {
            _presenter.Confirmed += OnPresenterConfirmed;
            _presenter.Dismissed += OnPresenterDismissed;
        }

        UpdateTextBlocks();
    }

    // ------------------------------------------------------------------
    // Event handlers
    // ------------------------------------------------------------------

    private void OnFlyoutButtonClick(object? sender, RoutedEventArgs e)
    {
        if (_popup == null) return;

        if (_presenter != null)
        {
            _presenter.Time = SelectedTime;
            _presenter.ClockIdentifier = ClockIdentifier;
            _presenter.UseSeconds = UseSeconds;
        }

        _popup.IsOpen = true;
    }

    private void OnPresenterConfirmed(object? sender, RoutedEventArgs e)
    {
        if (_presenter != null)
            SelectedTime = _presenter.GetSelectedTime();
        ClosePopup();
    }

    private void OnPresenterDismissed(object? sender, RoutedEventArgs e)
    {
        ClosePopup();
    }

    private void ClosePopup()
    {
        if (_popup != null)
            _popup.IsOpen = false;
    }

    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------

    private void UpdateTextBlocks()
    {
        if (SelectedTime is { } time)
        {
            bool is12h = ClockIdentifier == "12HourClock";

            if (is12h)
            {
                int h = time.Hours % 12;
                if (h == 0) h = 12;
                if (_hourTextBlock != null)
                    _hourTextBlock.Text = h.ToString("D2");
                if (_periodTextBlock != null)
                    _periodTextBlock.Text = time.Hours >= 12 ? "PM" : "AM";
            }
            else
            {
                if (_hourTextBlock != null)
                    _hourTextBlock.Text = time.Hours.ToString("D2");
            }

            if (_minuteTextBlock != null)
                _minuteTextBlock.Text = time.Minutes.ToString("D2");
            if (_secondTextBlock != null)
                _secondTextBlock.Text = time.Seconds.ToString("D2");
        }
        else
        {
            if (_hourTextBlock != null)
                _hourTextBlock.Text = HourText;
            if (_minuteTextBlock != null)
                _minuteTextBlock.Text = MinuteText;
            if (_secondTextBlock != null)
                _secondTextBlock.Text = SecondText;
            if (_periodTextBlock != null)
                _periodTextBlock.Text = string.Empty;
        }
    }

    private static void OnSelectedTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TimePicker picker)
        {
            picker.UpdateTextBlocks();
            picker.RaiseEvent(new RoutedEventArgs(SelectedTimeChangedEvent, picker));
        }
    }
}
