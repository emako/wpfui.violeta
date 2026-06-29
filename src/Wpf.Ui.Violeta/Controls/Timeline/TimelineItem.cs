using System;
using System.Windows;
using System.Windows.Controls;

namespace Wpf.Ui.Violeta.Controls;

public class TimelineItem : HeaderedContentControl
{
    public const string PART_Header = "PART_Header";
    public const string PART_Icon = "PART_Icon";
    public const string PART_Content = "PART_Content";
    public const string PART_Time = "PART_Time";
    public const string PART_RootGrid = "PART_RootGrid";

    private ContentPresenter? _headerPresenter;
    private Panel? _iconPresenter;
    private ContentPresenter? _contentPresenter;
    private System.Windows.Controls.TextBlock? _timePresenter;
    private Grid? _rootGrid;

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(object), typeof(TimelineItem),
            new PropertyMetadata(null, OnIconChanged));

    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly DependencyProperty IconTemplateProperty =
        DependencyProperty.Register(nameof(IconTemplate), typeof(DataTemplate), typeof(TimelineItem),
            new PropertyMetadata(null));

    public DataTemplate? IconTemplate
    {
        get => (DataTemplate?)GetValue(IconTemplateProperty);
        set => SetValue(IconTemplateProperty, value);
    }

    public static readonly DependencyProperty TypeProperty =
        DependencyProperty.Register(nameof(Type), typeof(TimelineItemType), typeof(TimelineItem),
            new PropertyMetadata(TimelineItemType.Default));

    public TimelineItemType Type
    {
        get => (TimelineItemType)GetValue(TypeProperty);
        set => SetValue(TypeProperty, value);
    }

    public static readonly DependencyProperty PositionProperty =
        DependencyProperty.Register(nameof(Position), typeof(TimelineItemPosition), typeof(TimelineItem),
            new FrameworkPropertyMetadata(TimelineItemPosition.Right,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange,
                OnPositionChanged));

    public TimelineItemPosition Position
    {
        get => (TimelineItemPosition)GetValue(PositionProperty);
        set => SetValue(PositionProperty, value);
    }

    public static readonly DependencyProperty TimeProperty =
        DependencyProperty.Register(nameof(Time), typeof(DateTime), typeof(TimelineItem),
            new PropertyMetadata(default(DateTime)));

    public DateTime Time
    {
        get => (DateTime)GetValue(TimeProperty);
        set => SetValue(TimeProperty, value);
    }

    public static readonly DependencyProperty TimeFormatProperty =
        DependencyProperty.Register(nameof(TimeFormat), typeof(string), typeof(TimelineItem),
            new PropertyMetadata("yyyy-MM-dd HH:mm:ss"));

    public string? TimeFormat
    {
        get => (string?)GetValue(TimeFormatProperty);
        set => SetValue(TimeFormatProperty, value);
    }

    // Internal state flags for first/last item
    private bool _isFirst;

    private bool _isLast;

    static TimelineItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(TimelineItem),
            new FrameworkPropertyMetadata(typeof(TimelineItem)));
    }

    private static void OnIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Trigger re-evaluation of icon visibility
        ((TimelineItem)d).UpdateVisualState();
    }

    private static void OnPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((TimelineItem)d).UpdateGridLayout();
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _rootGrid = GetTemplateChild(PART_RootGrid) as Grid;
        _headerPresenter = GetTemplateChild(PART_Header) as ContentPresenter;
        _iconPresenter = GetTemplateChild(PART_Icon) as Panel;
        _contentPresenter = GetTemplateChild(PART_Content) as ContentPresenter;
        _timePresenter = GetTemplateChild(PART_Time) as System.Windows.Controls.TextBlock;
        UpdateVisualState();
        UpdateGridLayout();
    }

    private void UpdateVisualState()
    {
        // Icon visibility is handled by triggers in XAML
    }

    private void UpdateGridLayout()
    {
        if (_rootGrid is null) return;
        ApplyPositionLayout(Position);
    }

    private void ApplyPositionLayout(TimelineItemPosition position)
    {
        if (_rootGrid is null) return;
        // Layout is driven by XAML triggers; this can force re-measure
        InvalidateMeasure();
        InvalidateArrange();
    }

    internal void SetEnd(bool isFirst, bool isLast)
    {
        _isFirst = isFirst;
        _isLast = isLast;
        // Notify XAML triggers via property-level signals
        // We set attached pseudo-states via IsFirst/IsLast properties below
        SetValue(IsFirstProperty, isFirst);
        SetValue(IsLastProperty, isLast);
    }

    // Internal attached-like DPs used by triggers
    internal static readonly DependencyProperty IsFirstProperty =
        DependencyProperty.Register("IsFirst", typeof(bool), typeof(TimelineItem),
            new PropertyMetadata(false));

    internal static readonly DependencyProperty IsLastProperty =
        DependencyProperty.Register("IsLast", typeof(bool), typeof(TimelineItem),
            new PropertyMetadata(false));

    internal bool IsFirst => _isFirst;
    internal bool IsLast => _isLast;

    internal (double left, double mid, double right) GetWidth()
    {
        if (_rootGrid is null)
            return (0, 0, 0);
        double header = _headerPresenter?.DesiredSize.Width ?? 0;
        double icon = _iconPresenter?.DesiredSize.Width ?? 0;
        double content = _contentPresenter?.DesiredSize.Width ?? 0;
        double time = _timePresenter?.DesiredSize.Width ?? 0;
        double maxContent = Math.Max(header, content);

        return Position switch
        {
            TimelineItemPosition.Left => (0, icon, Math.Max(maxContent, time)),
            TimelineItemPosition.Right => (Math.Max(maxContent, time), icon, 0),
            TimelineItemPosition.Separate => (time, icon, maxContent),
            _ => (0, 0, 0),
        };
    }

    internal void SetWidth(double left, double mid, double right)
    {
        if (_rootGrid is null) return;
        _rootGrid.ColumnDefinitions[0].Width = new GridLength(left);
        _rootGrid.ColumnDefinitions[1].Width = new GridLength(mid);
        _rootGrid.ColumnDefinitions[2].Width = new GridLength(right);
    }

    protected override void OnVisualParentChanged(DependencyObject oldParent)
    {
        base.OnVisualParentChanged(oldParent);
        if (Parent is Timeline tl)
        {
            tl.InvalidateContainers();
        }
    }
}
