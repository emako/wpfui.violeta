using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// The flyout popup content for <see cref="TimeBoxPicker"/>. Renders one drum-roll column per
/// Hour/Minute/Second/Millisecond field found in <see cref="Format"/>, with literal separators
/// taken verbatim from the format string (e.g. "HH:mm:ss.fff" produces ':' and '.' separators
/// at the same positions). There is no AM/PM column — the hour wheel always covers the full
/// 0-23 range regardless of whether Format uses 'H' or 'h'; 12/24-hour presentation is purely
/// a text-formatting concern handled by <see cref="TimeBoxPicker"/>, not a selection concern.
/// </summary>
[TemplatePart(Name = PartPickerContainer, Type = typeof(Grid))]
[TemplatePart(Name = PartAcceptButton, Type = typeof(Button))]
[TemplatePart(Name = PartDismissButton, Type = typeof(Button))]
public class TimeBoxPickerPresenter : Control
{
    public const string PartPickerContainer = "PART_PickerContainer";
    public const string PartAcceptButton = "PART_AcceptButton";
    public const string PartDismissButton = "PART_DismissButton";

    private Grid? _pickerContainer;
    private readonly List<(DateTimePickerPanelType Kind, DateTimePickerPanel Panel)> _fields = [];

    // ------------------------------------------------------------------
    // Dependency Properties
    // ------------------------------------------------------------------

    public static readonly DependencyProperty TimeProperty =
        DependencyProperty.Register(
            nameof(Time),
            typeof(TimeSpan?),
            typeof(TimeBoxPickerPresenter),
            new PropertyMetadata(null, OnTimeChanged));

    /// <summary>
    /// .NET custom date/time format string. Supported field specifiers: H/HH and h/hh (hour —
    /// both map to the same 0-23 wheel; 12/24 only changes how the *text* renders elsewhere),
    /// m/mm (minute), s/ss (second), f...'/'F...' (millisecond). Any other character — including
    /// literal text, quoted literals, and unsupported specifiers such as 't'/'tt' — is rendered
    /// verbatim as a static separator between wheel columns, at its original position.
    /// </summary>
    public static readonly DependencyProperty FormatProperty =
        DependencyProperty.Register(
            nameof(Format),
            typeof(string),
            typeof(TimeBoxPickerPresenter),
            new PropertyMetadata("HH:mm:ss", OnFormatChanged));

    /// <summary>
    /// Number of items shown at once in each drum-roll column. Forwarded to every child
    /// <see cref="DateTimePickerPanel"/>. Default 5.
    /// </summary>
    public static readonly DependencyProperty VisibleItemCountProperty =
        DependencyProperty.Register(
            nameof(VisibleItemCount),
            typeof(int),
            typeof(TimeBoxPickerPresenter),
            new PropertyMetadata(5, OnVisibleItemCountChanged));

    public static readonly RoutedEvent ConfirmedEvent =
        EventManager.RegisterRoutedEvent(nameof(Confirmed), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TimeBoxPickerPresenter));

    public static readonly RoutedEvent DismissedEvent =
        EventManager.RegisterRoutedEvent(nameof(Dismissed), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TimeBoxPickerPresenter));

    // ------------------------------------------------------------------
    // Properties
    // ------------------------------------------------------------------

    public TimeSpan? Time
    {
        get => (TimeSpan?)GetValue(TimeProperty);
        set => SetValue(TimeProperty, value);
    }

    public string Format
    {
        get => (string)GetValue(FormatProperty);
        set => SetValue(FormatProperty, value);
    }

    public int VisibleItemCount
    {
        get => (int)GetValue(VisibleItemCountProperty);
        set => SetValue(VisibleItemCountProperty, value);
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

    static TimeBoxPickerPresenter()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(TimeBoxPickerPresenter),
            new FrameworkPropertyMetadata(typeof(TimeBoxPickerPresenter)));
    }

    // ------------------------------------------------------------------
    // Template
    // ------------------------------------------------------------------

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _pickerContainer = GetTemplateChild(PartPickerContainer) as Grid;
        RebuildColumns();

        if (GetTemplateChild(PartAcceptButton) is Button accept)
            accept.Click += (_, _) => RaiseEvent(new RoutedEventArgs(ConfirmedEvent, this));
        if (GetTemplateChild(PartDismissButton) is Button dismiss)
            dismiss.Click += (_, _) => RaiseEvent(new RoutedEventArgs(DismissedEvent, this));
    }

    // ------------------------------------------------------------------
    // Format parsing
    // ------------------------------------------------------------------

    private enum SegmentKind
    {
        Literal,
        Hour,
        Minute,
        Second,
        Millisecond,
    }

    private readonly record struct Segment(SegmentKind Kind, string Text);

    /// <summary>
    /// Splits a .NET custom format string into an ordered list of literal-text and
    /// field segments, honoring quoted literals ('...'/"...") and backslash-escapes.
    /// </summary>
    private static List<Segment> ParseFormat(string? format)
    {
        var segments = new List<Segment>();
        if (string.IsNullOrEmpty(format))
            return segments;

        var literal = new StringBuilder();
        SegmentKind? currentField = null;

        void FlushLiteral()
        {
            if (literal.Length > 0)
            {
                segments.Add(new Segment(SegmentKind.Literal, literal.ToString()));
                literal.Clear();
            }
        }

        void FlushField()
        {
            if (currentField is { } kind)
            {
                segments.Add(new Segment(kind, string.Empty));
                currentField = null;
            }
        }

        int i = 0;
        while (i < format!.Length)
        {
            char c = format[i];

            // Quoted literal: '...' or "..."
            if (c is '\'' or '"')
            {
                FlushField();
                char quote = c;
                i++;
                while (i < format.Length && format[i] != quote)
                {
                    literal.Append(format[i]);
                    i++;
                }
                i++; // skip closing quote (or run off the end)
                continue;
            }

            // Backslash-escaped literal: \x
            if (c == '\\' && i + 1 < format.Length)
            {
                FlushField();
                literal.Append(format[i + 1]);
                i += 2;
                continue;
            }

            SegmentKind? kind = c switch
            {
                'H' or 'h' => SegmentKind.Hour,
                'm' => SegmentKind.Minute,
                's' => SegmentKind.Second,
                'f' or 'F' => SegmentKind.Millisecond,
                _ => null,
            };

            if (kind is { } k)
            {
                if (currentField != k)
                {
                    FlushLiteral();
                    FlushField();
                    currentField = k;
                }
                i++;
                continue;
            }

            FlushField();
            literal.Append(c);
            i++;
        }

        FlushField();
        FlushLiteral();
        return segments;
    }

    // ------------------------------------------------------------------
    // Column building
    // ------------------------------------------------------------------

    private void RebuildColumns()
    {
        if (_pickerContainer == null)
            return;

        _pickerContainer.Children.Clear();
        _pickerContainer.ColumnDefinitions.Clear();
        _fields.Clear();

        var segments = ParseFormat(Format);
        if (segments.Count == 0)
            segments = ParseFormat("HH:mm:ss");

        int columnIndex = 0;
        foreach (var segment in segments)
        {
            if (segment.Kind == SegmentKind.Literal)
            {
                _pickerContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var tb = new TextBlock
                {
                    Text = segment.Text,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(2, 0, 2, 0),
                };
                tb.SetResourceReference(TextBlock.ForegroundProperty, "DateTimePickerItemForeground");

                Grid.SetColumn(tb, columnIndex++);
                _pickerContainer.Children.Add(tb);
                continue;
            }

            var panelType = segment.Kind switch
            {
                SegmentKind.Hour => DateTimePickerPanelType.Hour,
                SegmentKind.Minute => DateTimePickerPanelType.Minute,
                SegmentKind.Second => DateTimePickerPanelType.Second,
                SegmentKind.Millisecond => DateTimePickerPanelType.Millisecond,
                _ => DateTimePickerPanelType.Minute,
            };

            _pickerContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var panel = new DateTimePickerPanel
            {
                PanelType = panelType,
                ItemHeight = 36,
                ShouldLoop = true,
                VisibleItemCount = VisibleItemCount,
                // Never expose AM/PM — the hour wheel is always the full 0-23 range.
                ClockIdentifier = "24HourClock",
            };

            Grid.SetColumn(panel, columnIndex++);
            _pickerContainer.Children.Add(panel);
            panel.ApplyTemplate();
            _fields.Add((panelType, panel));
        }

        SyncPanelsToTime();
    }

    // ------------------------------------------------------------------
    // Sync helpers
    // ------------------------------------------------------------------

    private void SyncPanelsToTime()
    {
        if (_fields.Count == 0)
            return;

        var time = Time ?? TimeSpan.Zero;
        foreach (var (kind, panel) in _fields)
        {
            panel.SelectedIndex = kind switch
            {
                DateTimePickerPanelType.Hour => time.Hours,
                DateTimePickerPanelType.Minute => time.Minutes,
                DateTimePickerPanelType.Second => time.Seconds,
                DateTimePickerPanelType.Millisecond => time.Milliseconds,
                _ => 0,
            };
        }
    }

    private static void OnTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TimeBoxPickerPresenter p)
            p.SyncPanelsToTime();
    }

    private static void OnFormatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TimeBoxPickerPresenter p && p._pickerContainer != null)
            p.RebuildColumns();
    }

    private static void OnVisibleItemCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TimeBoxPickerPresenter p)
        {
            foreach (var (_, panel) in p._fields)
                panel.VisibleItemCount = p.VisibleItemCount;
        }
    }

    // ------------------------------------------------------------------
    // Get selected time
    // ------------------------------------------------------------------

    public TimeSpan GetSelectedTime()
    {
        int hour = 0, minute = 0, second = 0, millisecond = 0;

        foreach (var (kind, panel) in _fields)
        {
            switch (kind)
            {
                case DateTimePickerPanelType.Hour: hour = panel.SelectedIndex; break;
                case DateTimePickerPanelType.Minute: minute = panel.SelectedIndex; break;
                case DateTimePickerPanelType.Second: second = panel.SelectedIndex; break;
                case DateTimePickerPanelType.Millisecond: millisecond = panel.SelectedIndex; break;
            }
        }

        return new TimeSpan(0, hour, minute, second, millisecond);
    }
}
