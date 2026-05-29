using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Wpf.Ui.Violeta.Resources.Localization;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// A drum-roll style date picker, mirroring Semi.Avalonia's DatePicker.
/// Opens a popup with scrollable columns for month, day, and year.
/// </summary>
[TemplatePart(Name = PartFlyoutButton, Type = typeof(Button))]
[TemplatePart(Name = PartFlyoutButtonContentGrid, Type = typeof(Grid))]
[TemplatePart(Name = PartMonthTextBlock, Type = typeof(TextBlock))]
[TemplatePart(Name = PartDayTextBlock, Type = typeof(TextBlock))]
[TemplatePart(Name = PartYearTextBlock, Type = typeof(TextBlock))]
[TemplatePart(Name = PartPopup, Type = typeof(Popup))]
[TemplatePart(Name = PartPresenter, Type = typeof(DatePickerPresenter))]
public class DatePicker : Control
{
    public const string PartFlyoutButton = "PART_FlyoutButton";
    public const string PartFlyoutButtonContentGrid = "PART_ButtonContentGrid";
    public const string PartMonthTextBlock = "PART_MonthTextBlock";
    public const string PartDayTextBlock = "PART_DayTextBlock";
    public const string PartYearTextBlock = "PART_YearTextBlock";
    public const string PartPopup = "PART_Popup";
    public const string PartPresenter = "PART_Presenter";

    private Button? _flyoutButton;
    private TextBlock? _monthTextBlock;
    private TextBlock? _dayTextBlock;
    private TextBlock? _yearTextBlock;
    private Popup? _popup;
    private DatePickerPresenter? _presenter;

    // ------------------------------------------------------------------
    // Dependency Properties
    // ------------------------------------------------------------------

    public static readonly DependencyProperty SelectedDateProperty =
        DependencyProperty.Register(
            nameof(SelectedDate),
            typeof(DateTimeOffset?),
            typeof(DatePicker),
            new PropertyMetadata(null, OnSelectedDateChanged));

    public static readonly DependencyProperty MonthVisibleProperty =
        DependencyProperty.Register(
            nameof(MonthVisible),
            typeof(bool),
            typeof(DatePicker),
            new PropertyMetadata(true));

    public static readonly DependencyProperty DayVisibleProperty =
        DependencyProperty.Register(
            nameof(DayVisible),
            typeof(bool),
            typeof(DatePicker),
            new PropertyMetadata(true));

    public static readonly DependencyProperty YearVisibleProperty =
        DependencyProperty.Register(
            nameof(YearVisible),
            typeof(bool),
            typeof(DatePicker),
            new PropertyMetadata(true));

    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register(
            nameof(Header),
            typeof(object),
            typeof(DatePicker),
            new PropertyMetadata(null));

    public static readonly DependencyProperty HeaderTemplateProperty =
        DependencyProperty.Register(
            nameof(HeaderTemplate),
            typeof(DataTemplate),
            typeof(DatePicker),
            new PropertyMetadata(null));

    public static readonly DependencyProperty MonthTextProperty =
        DependencyProperty.Register(
            nameof(MonthText),
            typeof(string),
            typeof(DatePicker),
            new PropertyMetadata(SH.DatePickerMonthText));

    public static readonly DependencyProperty DayTextProperty =
        DependencyProperty.Register(
            nameof(DayText),
            typeof(string),
            typeof(DatePicker),
            new PropertyMetadata(SH.DatePickerDayText));

    public static readonly DependencyProperty YearTextProperty =
        DependencyProperty.Register(
            nameof(YearText),
            typeof(string),
            typeof(DatePicker),
            new PropertyMetadata(SH.DatePickerYearText));

    public static readonly RoutedEvent SelectedDateChangedEvent =
        EventManager.RegisterRoutedEvent(
            nameof(SelectedDateChanged),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(DatePicker));

    // ------------------------------------------------------------------
    // Properties
    // ------------------------------------------------------------------

    public DateTimeOffset? SelectedDate
    {
        get => (DateTimeOffset?)GetValue(SelectedDateProperty);
        set => SetValue(SelectedDateProperty, value);
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

    public event RoutedEventHandler SelectedDateChanged
    {
        add => AddHandler(SelectedDateChangedEvent, value);
        remove => RemoveHandler(SelectedDateChangedEvent, value);
    }

    // ------------------------------------------------------------------
    // Constructor
    // ------------------------------------------------------------------

    static DatePicker()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(DatePicker),
            new FrameworkPropertyMetadata(typeof(DatePicker)));
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
        _monthTextBlock = GetTemplateChild(PartMonthTextBlock) as TextBlock;
        _dayTextBlock = GetTemplateChild(PartDayTextBlock) as TextBlock;
        _yearTextBlock = GetTemplateChild(PartYearTextBlock) as TextBlock;
        _popup = GetTemplateChild(PartPopup) as Popup;
        _presenter = GetTemplateChild(PartPresenter) as DatePickerPresenter;

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

    private void OnFlyoutButtonClick(object sender, RoutedEventArgs e)
    {
        if (_popup == null) return;

        // Sync current date to presenter
        if (_presenter != null)
            _presenter.Date = SelectedDate;

        _popup.IsOpen = true;
    }

    private void OnPresenterConfirmed(object sender, RoutedEventArgs e)
    {
        if (_presenter != null)
        {
            var newDate = _presenter.GetSelectedDate();
            SelectedDate = newDate;
        }
        ClosePopup();
    }

    private void OnPresenterDismissed(object sender, RoutedEventArgs e)
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
        if (SelectedDate is { } date)
        {
            if (_monthTextBlock != null)
                _monthTextBlock.Text = date.Month.ToString("D2");
            if (_dayTextBlock != null)
                _dayTextBlock.Text = date.Day.ToString("D2");
            if (_yearTextBlock != null)
                _yearTextBlock.Text = date.Year.ToString();
        }
        else
        {
            if (_monthTextBlock != null)
                _monthTextBlock.Text = MonthText;
            if (_dayTextBlock != null)
                _dayTextBlock.Text = DayText;
            if (_yearTextBlock != null)
                _yearTextBlock.Text = YearText;
        }
    }

    private static void OnSelectedDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DatePicker picker)
        {
            picker.UpdateTextBlocks();
            picker.RaiseEvent(new RoutedEventArgs(SelectedDateChangedEvent, picker));
        }
    }
}
