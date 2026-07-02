using System;
using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Violeta.Resources.Localization;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// The flyout popup content for <see cref="TimePicker"/>.
/// </summary>
[TemplatePart(Name = PartPickerContainer, Type = typeof(Grid))]
[TemplatePart(Name = PartHourSelector, Type = typeof(DateTimePickerPanel))]
[TemplatePart(Name = PartMinuteSelector, Type = typeof(DateTimePickerPanel))]
[TemplatePart(Name = PartSecondSelector, Type = typeof(DateTimePickerPanel))]
[TemplatePart(Name = PartPeriodSelector, Type = typeof(DateTimePickerPanel))]
[TemplatePart(Name = PartAcceptButton, Type = typeof(Button))]
[TemplatePart(Name = PartDismissButton, Type = typeof(Button))]
public class TimePickerPresenter : Control
{
    public const string PartPickerContainer = "PART_PickerContainer";
    public const string PartHourSelector = "PART_HourSelector";
    public const string PartMinuteSelector = "PART_MinuteSelector";
    public const string PartSecondSelector = "PART_SecondSelector";
    public const string PartPeriodSelector = "PART_PeriodSelector";
    public const string PartAcceptButton = "PART_AcceptButton";
    public const string PartDismissButton = "PART_DismissButton";

    private DateTimePickerPanel? _hourSelector;
    private DateTimePickerPanel? _minuteSelector;
    private DateTimePickerPanel? _secondSelector;
    private DateTimePickerPanel? _periodSelector;

    // ------------------------------------------------------------------
    // Dependency Properties
    // ------------------------------------------------------------------

    public static readonly DependencyProperty TimeProperty =
        DependencyProperty.Register(
            nameof(Time),
            typeof(TimeSpan?),
            typeof(TimePickerPresenter),
            new PropertyMetadata(null, OnTimeChanged));

    public static readonly DependencyProperty UseSecondsProperty =
        DependencyProperty.Register(
            nameof(UseSeconds),
            typeof(bool),
            typeof(TimePickerPresenter),
            new PropertyMetadata(false));

    public static readonly DependencyProperty ClockIdentifierProperty =
        DependencyProperty.Register(
            nameof(ClockIdentifier),
            typeof(string),
            typeof(TimePickerPresenter),
            new PropertyMetadata("24HourClock", OnClockIdentifierChanged));

    public static readonly DependencyProperty HourTextProperty =
        DependencyProperty.Register(
            nameof(HourText),
            typeof(string),
            typeof(TimePickerPresenter),
            new PropertyMetadata(SH.TimePickerHourText));

    public static readonly DependencyProperty MinuteTextProperty =
        DependencyProperty.Register(
            nameof(MinuteText),
            typeof(string),
            typeof(TimePickerPresenter),
            new PropertyMetadata(SH.TimePickerMinuteText));

    public static readonly DependencyProperty SecondTextProperty =
        DependencyProperty.Register(
            nameof(SecondText),
            typeof(string),
            typeof(TimePickerPresenter),
            new PropertyMetadata(SH.TimePickerSecondText));

    public static readonly RoutedEvent ConfirmedEvent =
        EventManager.RegisterRoutedEvent(nameof(Confirmed), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TimePickerPresenter));

    public static readonly RoutedEvent DismissedEvent =
        EventManager.RegisterRoutedEvent(nameof(Dismissed), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TimePickerPresenter));

    // ------------------------------------------------------------------
    // Properties
    // ------------------------------------------------------------------

    public TimeSpan? Time
    {
        get => (TimeSpan?)GetValue(TimeProperty);
        set => SetValue(TimeProperty, value);
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

    public event RoutedEventHandler Confirmed
    {
        add => AddHandler(ConfirmedEvent, value);
        remove => RemoveHandler(ConfirmedEvent, value);
    }

    public event RoutedEventHandler Dismissed
    {
        add => AddHandler(DismissedEvent, value);
        remove => RemoveHandler(DismissedEvent, value);
    }

    // ------------------------------------------------------------------
    // Constructor
    // ------------------------------------------------------------------

    static TimePickerPresenter()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(TimePickerPresenter),
            new FrameworkPropertyMetadata(typeof(TimePickerPresenter)));
    }

    // ------------------------------------------------------------------
    // Template
    // ------------------------------------------------------------------

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _hourSelector = GetTemplateChild(PartHourSelector) as DateTimePickerPanel;
        _minuteSelector = GetTemplateChild(PartMinuteSelector) as DateTimePickerPanel;
        _secondSelector = GetTemplateChild(PartSecondSelector) as DateTimePickerPanel;
        _periodSelector = GetTemplateChild(PartPeriodSelector) as DateTimePickerPanel;

        if (_hourSelector != null)
        {
            _hourSelector.PanelType = DateTimePickerPanelType.Hour;
            _hourSelector.ClockIdentifier = ClockIdentifier;
            _hourSelector.ShouldLoop = true;
        }
        if (_minuteSelector != null)
        {
            _minuteSelector.PanelType = DateTimePickerPanelType.Minute;
            _minuteSelector.ShouldLoop = true;
        }
        if (_secondSelector != null)
        {
            _secondSelector.PanelType = DateTimePickerPanelType.Second;
            _secondSelector.ShouldLoop = true;
        }
        if (_periodSelector != null)
        {
            _periodSelector.PanelType = DateTimePickerPanelType.TimePeriod;
            _periodSelector.ShouldLoop = false;
        }

        if (GetTemplateChild(PartAcceptButton) is Button accept)
            accept.Click += (_, _) => RaiseEvent(new RoutedEventArgs(ConfirmedEvent, this));
        if (GetTemplateChild(PartDismissButton) is Button dismiss)
            dismiss.Click += (_, _) => RaiseEvent(new RoutedEventArgs(DismissedEvent, this));

        SyncPanelsToTime();
    }

    // ------------------------------------------------------------------
    // Sync helpers
    // ------------------------------------------------------------------

    private void SyncPanelsToTime()
    {
        var time = Time ?? TimeSpan.Zero;
        bool is12h = ClockIdentifier == "12HourClock";

        if (_hourSelector != null)
        {
            int hour = time.Hours;
            if (is12h)
            {
                // Convert to 12h format index (1-12, so index = h-1)
                int h12 = hour % 12;
                if (h12 == 0) h12 = 12;
                _hourSelector.SelectedIndex = h12 - 1;
            }
            else
            {
                _hourSelector.SelectedIndex = hour;
            }
        }

        if (_minuteSelector != null)
            _minuteSelector.SelectedIndex = time.Minutes;

        if (_secondSelector != null)
            _secondSelector.SelectedIndex = time.Seconds;

        if (_periodSelector != null && is12h)
            _periodSelector.SelectedIndex = time.Hours >= 12 ? 1 : 0;
    }

    private static void OnTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TimePickerPresenter p && p._hourSelector != null)
            p.SyncPanelsToTime();
    }

    private static void OnClockIdentifierChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TimePickerPresenter p && p._hourSelector != null)
        {
            p._hourSelector.ClockIdentifier = (string)e.NewValue;
            p.SyncPanelsToTime();
        }
    }

    // ------------------------------------------------------------------
    // Get selected time
    // ------------------------------------------------------------------

    public TimeSpan? GetSelectedTime()
    {
        if (_hourSelector == null || _minuteSelector == null)
            return null;

        bool is12h = ClockIdentifier == "12HourClock";
        int hour;

        if (is12h)
        {
            int hourIndex = _hourSelector.SelectedIndex; // 0-11 → 1-12
            hour = hourIndex + 1;
            bool isPm = _periodSelector?.SelectedIndex == 1;
            if (hour == 12) hour = 0;
            if (isPm) hour += 12;
        }
        else
        {
            hour = _hourSelector.SelectedIndex;
        }

        int minute = _minuteSelector.SelectedIndex;
        int second = UseSeconds ? (_secondSelector?.SelectedIndex ?? 0) : 0;

        return new TimeSpan(hour, minute, second);
    }
}
