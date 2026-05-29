using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Wpf.Ui.Violeta.Resources.Localization;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// The flyout popup content for <see cref="DatePicker"/>.
/// </summary>
[TemplatePart(Name = PartPickerContainer, Type = typeof(Grid))]
[TemplatePart(Name = PartMonthHost, Type = typeof(Panel))]
[TemplatePart(Name = PartDayHost, Type = typeof(Panel))]
[TemplatePart(Name = PartYearHost, Type = typeof(Panel))]
[TemplatePart(Name = PartMonthSelector, Type = typeof(DateTimePickerPanel))]
[TemplatePart(Name = PartDaySelector, Type = typeof(DateTimePickerPanel))]
[TemplatePart(Name = PartYearSelector, Type = typeof(DateTimePickerPanel))]
[TemplatePart(Name = PartMonthUpButton, Type = typeof(RepeatButton))]
[TemplatePart(Name = PartMonthDownButton, Type = typeof(RepeatButton))]
[TemplatePart(Name = PartDayUpButton, Type = typeof(RepeatButton))]
[TemplatePart(Name = PartDayDownButton, Type = typeof(RepeatButton))]
[TemplatePart(Name = PartYearUpButton, Type = typeof(RepeatButton))]
[TemplatePart(Name = PartYearDownButton, Type = typeof(RepeatButton))]
[TemplatePart(Name = PartAcceptButton, Type = typeof(Button))]
[TemplatePart(Name = PartDismissButton, Type = typeof(Button))]
public class DatePickerPresenter : Control
{
    public const string PartPickerContainer = "PART_PickerContainer";
    public const string PartMonthHost = "PART_MonthHost";
    public const string PartDayHost = "PART_DayHost";
    public const string PartYearHost = "PART_YearHost";
    public const string PartMonthSelector = "PART_MonthSelector";
    public const string PartDaySelector = "PART_DaySelector";
    public const string PartYearSelector = "PART_YearSelector";
    public const string PartMonthUpButton = "PART_MonthUpButton";
    public const string PartMonthDownButton = "PART_MonthDownButton";
    public const string PartDayUpButton = "PART_DayUpButton";
    public const string PartDayDownButton = "PART_DayDownButton";
    public const string PartYearUpButton = "PART_YearUpButton";
    public const string PartYearDownButton = "PART_YearDownButton";
    public const string PartAcceptButton = "PART_AcceptButton";
    public const string PartDismissButton = "PART_DismissButton";

    private DateTimePickerPanel? _monthSelector;
    private DateTimePickerPanel? _daySelector;
    private DateTimePickerPanel? _yearSelector;

    // ------------------------------------------------------------------
    // Dependency Properties
    // ------------------------------------------------------------------

    public static readonly DependencyProperty DateProperty =
        DependencyProperty.Register(
            nameof(Date),
            typeof(DateTimeOffset?),
            typeof(DatePickerPresenter),
            new PropertyMetadata(null, OnDateChanged));

    public static readonly DependencyProperty MonthVisibleProperty =
        DependencyProperty.Register(
            nameof(MonthVisible),
            typeof(bool),
            typeof(DatePickerPresenter),
            new PropertyMetadata(true));

    public static readonly DependencyProperty DayVisibleProperty =
        DependencyProperty.Register(
            nameof(DayVisible),
            typeof(bool),
            typeof(DatePickerPresenter),
            new PropertyMetadata(true));

    public static readonly DependencyProperty YearVisibleProperty =
        DependencyProperty.Register(
            nameof(YearVisible),
            typeof(bool),
            typeof(DatePickerPresenter),
            new PropertyMetadata(true));

    public static readonly DependencyProperty MonthTextProperty =
        DependencyProperty.Register(
            nameof(MonthText),
            typeof(string),
            typeof(DatePickerPresenter),
            new PropertyMetadata(SH.DatePickerMonthText));

    public static readonly DependencyProperty DayTextProperty =
        DependencyProperty.Register(
            nameof(DayText),
            typeof(string),
            typeof(DatePickerPresenter),
            new PropertyMetadata(SH.DatePickerDayText));

    public static readonly DependencyProperty YearTextProperty =
        DependencyProperty.Register(
            nameof(YearText),
            typeof(string),
            typeof(DatePickerPresenter),
            new PropertyMetadata(SH.DatePickerYearText));

    public static readonly RoutedEvent ConfirmedEvent =
        EventManager.RegisterRoutedEvent(nameof(Confirmed), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(DatePickerPresenter));

    public static readonly RoutedEvent DismissedEvent =
        EventManager.RegisterRoutedEvent(nameof(Dismissed), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(DatePickerPresenter));

    // ------------------------------------------------------------------
    // Properties
    // ------------------------------------------------------------------

    public DateTimeOffset? Date
    {
        get => (DateTimeOffset?)GetValue(DateProperty);
        set => SetValue(DateProperty, value);
    }

    public bool MonthVisible
    {
        get => (bool)GetValue(MonthVisibleProperty);
        set => SetValue(MonthVisibleProperty, value);
    }

    public bool DayVisible
    {
        get => (bool)GetValue(DayVisibleProperty);
        set => SetValue(DayVisibleProperty, value);
    }

    public bool YearVisible
    {
        get => (bool)GetValue(YearVisibleProperty);
        set => SetValue(YearVisibleProperty, value);
    }

    public string MonthText
    {
        get => (string)GetValue(MonthTextProperty);
        set => SetValue(MonthTextProperty, value);
    }

    public string DayText
    {
        get => (string)GetValue(DayTextProperty);
        set => SetValue(DayTextProperty, value);
    }

    public string YearText
    {
        get => (string)GetValue(YearTextProperty);
        set => SetValue(YearTextProperty, value);
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

    static DatePickerPresenter()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(DatePickerPresenter),
            new FrameworkPropertyMetadata(typeof(DatePickerPresenter)));
    }

    // ------------------------------------------------------------------
    // Template
    // ------------------------------------------------------------------

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        // Detach old handlers
        if (_monthSelector != null) _monthSelector.SelectionChanged -= OnSelectorSelectionChanged;
        if (_daySelector != null) _daySelector.SelectionChanged -= OnSelectorSelectionChanged;
        if (_yearSelector != null) _yearSelector.SelectionChanged -= OnSelectorSelectionChanged;

        _monthSelector = GetTemplateChild(PartMonthSelector) as DateTimePickerPanel;
        _daySelector = GetTemplateChild(PartDaySelector) as DateTimePickerPanel;
        _yearSelector = GetTemplateChild(PartYearSelector) as DateTimePickerPanel;

        if (_monthSelector != null)
        {
            _monthSelector.PanelType = DateTimePickerPanelType.Month;
            _monthSelector.ShouldLoop = true;
            _monthSelector.SelectionChanged += OnSelectorSelectionChanged;
        }
        if (_daySelector != null)
        {
            _daySelector.PanelType = DateTimePickerPanelType.Day;
            _daySelector.ShouldLoop = true;
            _daySelector.SelectionChanged += OnSelectorSelectionChanged;
        }
        if (_yearSelector != null)
        {
            _yearSelector.PanelType = DateTimePickerPanelType.Year;
            _yearSelector.ShouldLoop = false;
        }

        // Wire up RepeatButtons
        WireRepeatButton(PartMonthUpButton, () => _monthSelector?.MoveUp());
        WireRepeatButton(PartMonthDownButton, () => _monthSelector?.MoveDown());
        WireRepeatButton(PartDayUpButton, () => _daySelector?.MoveUp());
        WireRepeatButton(PartDayDownButton, () => _daySelector?.MoveDown());
        WireRepeatButton(PartYearUpButton, () => _yearSelector?.MoveUp());
        WireRepeatButton(PartYearDownButton, () => _yearSelector?.MoveDown());

        // Accept / Dismiss
        if (GetTemplateChild(PartAcceptButton) is Button accept)
            accept.Click += (_, _) => RaiseEvent(new RoutedEventArgs(ConfirmedEvent, this));
        if (GetTemplateChild(PartDismissButton) is Button dismiss)
            dismiss.Click += (_, _) => RaiseEvent(new RoutedEventArgs(DismissedEvent, this));

        // Initialize panels from current Date
        SyncPanelsToDate();
    }

    private void WireRepeatButton(string partName, Action action)
    {
        if (GetTemplateChild(partName) is RepeatButton btn)
        {
            btn.Click -= (_, _) => action();
            btn.Click += (_, _) => action();
        }
    }

    // ------------------------------------------------------------------
    // Sync helpers
    // ------------------------------------------------------------------

    private bool _suppressSync = false;

    private void SyncPanelsToDate()
    {
        if (Date is { } date)
        {
            SyncPanel(_monthSelector, date.Month - 1);
            SyncPanel(_daySelector, date.Day - 1);
            SyncPanel(_yearSelector, GetYearIndex(date.Year));
        }
        else
        {
            var today = DateTimeOffset.Now;
            SyncPanel(_monthSelector, today.Month - 1);
            SyncPanel(_daySelector, today.Day - 1);
            SyncPanel(_yearSelector, GetYearIndex(today.Year));
        }
    }

    private static void SyncPanel(DateTimePickerPanel? panel, int index)
    {
        if (panel == null) return;
        panel.SelectedIndex = Math.Max(0, index);
    }

    private int GetYearIndex(int year)
    {
        if (_yearSelector == null) return 0;
        return Math.Max(0, year - _yearSelector.MinYear);
    }

    private void OnSelectorSelectionChanged(object sender, RoutedEventArgs e)
    {
        if (_suppressSync) return;

        // Update MaxDay when month or year changes
        if (_monthSelector != null && _yearSelector != null && _daySelector != null)
        {
            int month = _monthSelector.SelectedIndex + 1;
            int year = _yearSelector.SelectedIndex + (_yearSelector.MinYear);
            int maxDay = DateTime.DaysInMonth(year, month);
            _daySelector.MaxDay = maxDay;
        }
    }

    private static void OnDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DatePickerPresenter p && p._monthSelector != null)
            p.SyncPanelsToDate();
    }

    // ------------------------------------------------------------------
    // Get selected date from panels
    // ------------------------------------------------------------------

    public DateTimeOffset? GetSelectedDate()
    {
        if (_monthSelector == null || _daySelector == null || _yearSelector == null)
            return null;

        int month = _monthSelector.SelectedIndex + 1;
        int day = _daySelector.SelectedIndex + 1;
        int year = _yearSelector.SelectedIndex + (_yearSelector.MinYear);

        // Clamp day to valid range
        int maxDay = DateTime.DaysInMonth(year, month);
        day = Math.Min(day, maxDay);

        return new DateTimeOffset(new DateTime(year, month, day));
    }
}
