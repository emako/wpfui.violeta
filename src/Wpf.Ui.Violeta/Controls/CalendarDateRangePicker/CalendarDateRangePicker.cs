using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Selects a contiguous date range. The first click sets <see cref="Start"/>, the second sets <see cref="End"/>.
/// </summary>
[TemplatePart(Name = PartTextBox, Type = typeof(TextBox))]
[TemplatePart(Name = PartButton, Type = typeof(Button))]
[TemplatePart(Name = PartCalendar, Type = typeof(Calendar))]
public class CalendarDateRangePicker : Control
{
    private const string PartTextBox = "PART_TextBox";
    private const string PartButton = "PART_Button";
    private const string PartCalendar = "PART_Calendar";

    private TextBox? _textBox;
    private Button? _button;
    private Calendar? _calendar;
    private MouseButtonEventHandler? _calendarMouseUp;
    private bool _updatingCalendar;
    private bool _applyingRange;
    private bool _awaitingEnd;

    /// <summary>Identifies the <see cref="Start"/> dependency property.</summary>
    public static readonly DependencyProperty StartProperty = DependencyProperty.Register(
        nameof(Start),
        typeof(DateTime?),
        typeof(CalendarDateRangePicker),
        new FrameworkPropertyMetadata(null, OnRangeEndpointChanged));

    /// <summary>Gets or sets the first day of the range.</summary>
    public DateTime? Start
    {
        get => (DateTime?)GetValue(StartProperty);
        set => SetValue(StartProperty, value);
    }

    /// <summary>Identifies the <see cref="End"/> dependency property.</summary>
    public static readonly DependencyProperty EndProperty = DependencyProperty.Register(
        nameof(End),
        typeof(DateTime?),
        typeof(CalendarDateRangePicker),
        new FrameworkPropertyMetadata(null, OnRangeEndpointChanged));

    /// <summary>Gets or sets the last day of the range.</summary>
    public DateTime? End
    {
        get => (DateTime?)GetValue(EndProperty);
        set => SetValue(EndProperty, value);
    }

    /// <summary>Identifies the <see cref="Placeholder"/> dependency property.</summary>
    public static readonly DependencyProperty PlaceholderProperty = DependencyProperty.Register(
        nameof(Placeholder),
        typeof(string),
        typeof(CalendarDateRangePicker),
        new FrameworkPropertyMetadata(string.Empty));

    /// <summary>Gets or sets the text shown when <see cref="Start"/> is unset.</summary>
    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    /// <summary>Identifies the <see cref="IsDropDownOpen"/> dependency property.</summary>
    public static readonly DependencyProperty IsDropDownOpenProperty = DependencyProperty.Register(
        nameof(IsDropDownOpen),
        typeof(bool),
        typeof(CalendarDateRangePicker),
        new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsDropDownOpenChanged));

    /// <summary>Gets or sets whether the calendar popup is open.</summary>
    public bool IsDropDownOpen
    {
        get => (bool)GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }

    private static readonly DependencyPropertyKey TextPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(Text),
        typeof(string),
        typeof(CalendarDateRangePicker),
        new FrameworkPropertyMetadata(string.Empty));

    /// <summary>Identifies the <see cref="Text"/> dependency property.</summary>
    public static readonly DependencyProperty TextProperty = TextPropertyKey.DependencyProperty;

    /// <summary>Gets the formatted range, or empty when <see cref="Start"/> is unset.</summary>
    public string Text => (string)GetValue(TextProperty);

    /// <summary>Identifies the <see cref="RangeChanged"/> routed event.</summary>
    public static readonly RoutedEvent RangeChangedEvent = EventManager.RegisterRoutedEvent(
        nameof(RangeChanged),
        RoutingStrategy.Bubble,
        typeof(RoutedEventHandler),
        typeof(CalendarDateRangePicker));

    /// <summary>Occurs when <see cref="Start"/> or <see cref="End"/> changes.</summary>
    public event RoutedEventHandler RangeChanged
    {
        add => AddHandler(RangeChangedEvent, value);
        remove => RemoveHandler(RangeChangedEvent, value);
    }

    static CalendarDateRangePicker()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(CalendarDateRangePicker),
            new FrameworkPropertyMetadata(typeof(CalendarDateRangePicker)));
    }

    /// <inheritdoc />
    public override void OnApplyTemplate()
    {
        if (_button is not null)
        {
            _button.Click -= OnButtonClick;
        }

        if (_textBox is not null)
        {
            _textBox.PreviewKeyDown -= OnTextBoxPreviewKeyDown;
        }

        if (_calendar is not null && _calendarMouseUp is not null)
        {
            _calendar.RemoveHandler(MouseLeftButtonUpEvent, _calendarMouseUp);
        }

        base.OnApplyTemplate();

        _textBox = GetTemplateChild(PartTextBox) as TextBox;
        _button = GetTemplateChild(PartButton) as Button;
        _calendar = GetTemplateChild(PartCalendar) as Calendar;

        if (_button is not null)
        {
            _button.Click += OnButtonClick;
        }

        if (_textBox is not null)
        {
            _textBox.PreviewKeyDown += OnTextBoxPreviewKeyDown;
        }

        if (_calendar is null)
        {
            UpdateText();
            return;
        }

        if (TryFindResource("CalendarDatePickerCalendarStyle") is Style calendarStyle)
        {
            _calendar.Style = calendarStyle;
        }

        _calendar.SelectionMode = CalendarSelectionMode.SingleRange;
        _calendarMouseUp = OnCalendarMouseUp;
        _calendar.AddHandler(MouseLeftButtonUpEvent, _calendarMouseUp, handledEventsToo: true);
        SyncCalendar();
        UpdateText();
    }

    /// <inheritdoc />
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key == Key.Escape && IsDropDownOpen)
        {
            SetCurrentValue(IsDropDownOpenProperty, false);
            e.Handled = true;
            return;
        }

        if ((e.Key is Key.Enter or Key.Space or Key.Down) && !IsDropDownOpen && IsEnabled)
        {
            SetCurrentValue(IsDropDownOpenProperty, true);
            e.Handled = true;
        }
    }

    private void OnButtonClick(object sender, RoutedEventArgs e)
    {
        if (!IsEnabled)
        {
            return;
        }

        SetCurrentValue(IsDropDownOpenProperty, !IsDropDownOpen);
    }

    private void OnTextBoxPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape && IsDropDownOpen)
        {
            SetCurrentValue(IsDropDownOpenProperty, false);
            e.Handled = true;
            return;
        }

        if ((e.Key is Key.Enter or Key.Down) && !IsDropDownOpen && IsEnabled)
        {
            SetCurrentValue(IsDropDownOpenProperty, true);
            e.Handled = true;
        }
    }

    private static void OnRangeEndpointChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var picker = (CalendarDateRangePicker)d;
        picker.UpdateText();
        if (!picker._applyingRange)
        {
            picker.SyncCalendar();
        }

        picker.RaiseEvent(new RoutedEventArgs(RangeChangedEvent, picker));
    }

    private static void OnIsDropDownOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var picker = (CalendarDateRangePicker)d;
        if (e.NewValue is not true)
        {
            return;
        }

        picker._awaitingEnd = picker.Start.HasValue && !picker.End.HasValue;
        picker.Dispatcher.BeginInvoke(() => picker._calendar?.Focus());
    }

    private void OnCalendarMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (_updatingCalendar || FindDayButton(e.OriginalSource as DependencyObject) is not CalendarDayButton day)
        {
            return;
        }

        if (day.IsBlackedOut || day.DataContext is not DateTime clicked)
        {
            return;
        }

        clicked = clicked.Date;
        if (!_awaitingEnd || Start is not DateTime start)
        {
            _awaitingEnd = true;
            _applyingRange = true;
            SetCurrentValue(EndProperty, null);
            SetCurrentValue(StartProperty, clicked);
            _applyingRange = false;
            SyncCalendar();
            return;
        }

        var end = clicked;
        if (end < start)
        {
            (start, end) = (end, start);
        }

        _awaitingEnd = false;
        _applyingRange = true;
        SetCurrentValue(StartProperty, start);
        SetCurrentValue(EndProperty, end);
        _applyingRange = false;
        SyncCalendar();
        SetCurrentValue(IsDropDownOpenProperty, false);
    }

    private static CalendarDayButton? FindDayButton(DependencyObject? source)
    {
        while (source is not null)
        {
            if (source is CalendarDayButton day)
            {
                return day;
            }

            source = VisualTreeHelper.GetParent(source);
        }

        return null;
    }

    private void SyncCalendar()
    {
        if (_calendar is null)
        {
            return;
        }

        _updatingCalendar = true;
        try
        {
            _calendar.SelectedDates.Clear();
            if (Start is DateTime start && End is DateTime end)
            {
                var first = start.Date;
                var last = end.Date;
                if (last < first)
                {
                    (first, last) = (last, first);
                }

                _calendar.DisplayDate = first;
                _calendar.SelectedDates.AddRange(first, last);
            }
            else if (Start is DateTime only)
            {
                _calendar.DisplayDate = only.Date;
                _calendar.SelectedDates.Add(only.Date);
            }
        }
        finally
        {
            _updatingCalendar = false;
        }
    }

    private void UpdateText()
    {
        string text = Start switch
        {
            DateTime start when End is DateTime end => $"{start:d} – {end:d}",
            DateTime start => $"{start:d} –",
            _ => string.Empty,
        };

        SetValue(TextPropertyKey, text);
        if (_textBox is not null)
        {
            _textBox.Text = text;
        }
    }
}
