using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// A drum-roll style scrollable panel for date/time selection, mirroring Semi.Avalonia's DateTimePickerPanel.
/// </summary>
[TemplatePart(Name = PartScrollViewer, Type = typeof(ScrollViewer))]
[TemplatePart(Name = PartItemsPanel, Type = typeof(Panel))]
public class DateTimePickerPanel : Control
{
    public const string PartScrollViewer = "PART_ScrollViewer";
    public const string PartItemsPanel = "PART_ItemsPanel";

    private const int VisibleItemCount = 5;

    private ScrollViewer? _scrollViewer;
    private StackPanel? _itemsPanel;
    private bool _isApplyingTemplate;
    private double _touchStartY;
    private double _scrollStartOffset;
    private bool _isTouchScrolling;
    private readonly List<TextBlock> _itemElements = [];

    // ------------------------------------------------------------------
    // Dependency Properties
    // ------------------------------------------------------------------

    public static readonly DependencyProperty PanelTypeProperty =
        DependencyProperty.Register(
            nameof(PanelType),
            typeof(DateTimePickerPanelType),
            typeof(DateTimePickerPanel),
            new PropertyMetadata(DateTimePickerPanelType.Day, OnPanelTypeChanged));

    public static readonly DependencyProperty SelectedIndexProperty =
        DependencyProperty.Register(
            nameof(SelectedIndex),
            typeof(int),
            typeof(DateTimePickerPanel),
            new PropertyMetadata(0, OnSelectedIndexChanged));

    public static readonly DependencyProperty ItemHeightProperty =
        DependencyProperty.Register(
            nameof(ItemHeight),
            typeof(double),
            typeof(DateTimePickerPanel),
            new PropertyMetadata(36.0, OnItemHeightChanged));

    public static readonly DependencyProperty ShouldLoopProperty =
        DependencyProperty.Register(
            nameof(ShouldLoop),
            typeof(bool),
            typeof(DateTimePickerPanel),
            new PropertyMetadata(true));

    // MinYear / MaxYear for the Year panel
    public static readonly DependencyProperty MinYearProperty =
        DependencyProperty.Register(
            nameof(MinYear),
            typeof(int),
            typeof(DateTimePickerPanel),
            new PropertyMetadata(1900, OnPanelTypeChanged));

    public static readonly DependencyProperty MaxYearProperty =
        DependencyProperty.Register(
            nameof(MaxYear),
            typeof(int),
            typeof(DateTimePickerPanel),
            new PropertyMetadata(2100, OnPanelTypeChanged));

    // MaxDay for Day panel (1-31 etc.)
    public static readonly DependencyProperty MaxDayProperty =
        DependencyProperty.Register(
            nameof(MaxDay),
            typeof(int),
            typeof(DateTimePickerPanel),
            new PropertyMetadata(31, OnMaxDayChanged));

    // ClockIdentifier for Hour panel: "12HourClock" or "24HourClock"
    public static readonly DependencyProperty ClockIdentifierProperty =
        DependencyProperty.Register(
            nameof(ClockIdentifier),
            typeof(string),
            typeof(DateTimePickerPanel),
            new PropertyMetadata("24HourClock", OnPanelTypeChanged));

    public static readonly RoutedEvent SelectionChangedEvent =
        EventManager.RegisterRoutedEvent(
            nameof(SelectionChanged),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(DateTimePickerPanel));

    // ------------------------------------------------------------------
    // Properties
    // ------------------------------------------------------------------

    public DateTimePickerPanelType PanelType
    {
        get => (DateTimePickerPanelType)GetValue(PanelTypeProperty);
        set => SetValue(PanelTypeProperty, value);
    }

    public int SelectedIndex
    {
        get => (int)GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    public double ItemHeight
    {
        get => (double)GetValue(ItemHeightProperty);
        set => SetValue(ItemHeightProperty, value);
    }

    public bool ShouldLoop
    {
        get => (bool)GetValue(ShouldLoopProperty);
        set => SetValue(ShouldLoopProperty, value);
    }

    public int MinYear
    {
        get => (int)GetValue(MinYearProperty);
        set => SetValue(MinYearProperty, value);
    }

    public int MaxYear
    {
        get => (int)GetValue(MaxYearProperty);
        set => SetValue(MaxYearProperty, value);
    }

    public int MaxDay
    {
        get => (int)GetValue(MaxDayProperty);
        set => SetValue(MaxDayProperty, value);
    }

    public string ClockIdentifier
    {
        get => (string)GetValue(ClockIdentifierProperty);
        set => SetValue(ClockIdentifierProperty, value);
    }

    public event RoutedEventHandler SelectionChanged
    {
        add => AddHandler(SelectionChangedEvent, value);
        remove => RemoveHandler(SelectionChangedEvent, value);
    }

    // The count of real items (not spacers)
    private int ItemCount => _itemElements.Count;

    // ------------------------------------------------------------------
    // Constructor
    // ------------------------------------------------------------------

    static DateTimePickerPanel()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(DateTimePickerPanel),
            new FrameworkPropertyMetadata(typeof(DateTimePickerPanel)));
    }

    // ------------------------------------------------------------------
    // Template
    // ------------------------------------------------------------------

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _isApplyingTemplate = true;

        _scrollViewer = GetTemplateChild(PartScrollViewer) as ScrollViewer;
        _itemsPanel = GetTemplateChild(PartItemsPanel) as StackPanel;

        if (_scrollViewer != null)
        {
            _scrollViewer.PreviewMouseWheel += OnMouseWheel;
            _scrollViewer.PreviewTouchDown += OnTouchDown;
            _scrollViewer.PreviewTouchMove += OnTouchMove;
            _scrollViewer.PreviewTouchUp += OnTouchUp;
            _scrollViewer.SizeChanged += OnScrollViewerSizeChanged;
        }

        BuildItems();
        _isApplyingTemplate = false;
        ScrollToSelected(animate: false);
    }

    // ------------------------------------------------------------------
    // Item building
    // ------------------------------------------------------------------

    private void BuildItems()
    {
        if (_itemsPanel == null)
            return;

        _itemsPanel.Children.Clear();
        _itemElements.Clear();

        var items = GenerateItems();

        // Top spacer (blank area so first item can center)
        var topSpacer = new FrameworkElement { Height = VisibleItemCount / 2 * ItemHeight };
        _itemsPanel.Children.Add(topSpacer);

        foreach (var text in items)
        {
            var tb = new TextBlock
            {
                Text = text,
                Height = ItemHeight,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center,
            };
            tb.SetResourceReference(TextBlock.StyleProperty, "DateTimePickerItemTextStyle");

            var container = new Grid
            {
                Height = ItemHeight,
            };
            container.Children.Add(tb);

            _itemsPanel.Children.Add(container);
            _itemElements.Add(tb);
        }

        // Bottom spacer
        var bottomSpacer = new FrameworkElement { Height = VisibleItemCount / 2 * ItemHeight };
        _itemsPanel.Children.Add(bottomSpacer);

        UpdateItemHighlights();
    }

    private List<string> GenerateItems()
    {
        var list = new List<string>();
        switch (PanelType)
        {
            case DateTimePickerPanelType.Month:
                for (int i = 1; i <= 12; i++)
                    list.Add(i.ToString("D2"));
                break;

            case DateTimePickerPanelType.Day:
                for (int i = 1; i <= MaxDay; i++)
                    list.Add(i.ToString("D2"));
                break;

            case DateTimePickerPanelType.Year:
                for (int y = MinYear; y <= MaxYear; y++)
                    list.Add(y.ToString());
                break;

            case DateTimePickerPanelType.Hour:
                if (ClockIdentifier == "12HourClock")
                    for (int i = 1; i <= 12; i++)
                        list.Add(i.ToString("D2"));
                else
                    for (int i = 0; i <= 23; i++)
                        list.Add(i.ToString("D2"));
                break;

            case DateTimePickerPanelType.Minute:
            case DateTimePickerPanelType.Second:
                for (int i = 0; i <= 59; i++)
                    list.Add(i.ToString("D2"));
                break;

            case DateTimePickerPanelType.TimePeriod:
                list.Add("AM");
                list.Add("PM");
                break;
        }
        return list;
    }

    // Return the display string of the currently selected item
    public string? GetSelectedValue()
    {
        if (SelectedIndex >= 0 && SelectedIndex < _itemElements.Count)
            return _itemElements[SelectedIndex].Text;
        return null;
    }

    // ------------------------------------------------------------------
    // Navigation
    // ------------------------------------------------------------------

    public void MoveUp()
    {
        int next = SelectedIndex - 1;
        if (ShouldLoop)
            next = ((next % ItemCount) + ItemCount) % ItemCount;
        else
            next = Math.Max(0, next);
        SelectedIndex = next;
    }

    public void MoveDown()
    {
        int next = SelectedIndex + 1;
        if (ShouldLoop)
            next = next % ItemCount;
        else
            next = Math.Min(ItemCount - 1, next);
        SelectedIndex = next;
    }

    // ------------------------------------------------------------------
    // Scroll management
    // ------------------------------------------------------------------

    private void ScrollToSelected(bool animate)
    {
        if (_scrollViewer == null || ItemCount == 0)
            return;

        double targetOffset = SelectedIndex * ItemHeight;

        if (animate)
        {
            var anim = new DoubleAnimation(targetOffset, new Duration(TimeSpan.FromMilliseconds(150)))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            _scrollViewer.BeginAnimation(ScrollViewerHelper.VerticalOffsetProperty, anim);
        }
        else
        {
            _scrollViewer.ScrollToVerticalOffset(targetOffset);
        }
    }

    private void UpdateItemHighlights()
    {
        for (int i = 0; i < _itemElements.Count; i++)
        {
            var distance = Math.Abs(i - SelectedIndex);
            if (distance == 0)
                _itemElements[i].SetResourceReference(TextBlock.ForegroundProperty, "TextOnAccentFillColorPrimaryBrush");
            else
                _itemElements[i].SetResourceReference(TextBlock.ForegroundProperty, "DateTimePickerItemDimmedForeground");

            // Scale effect: items further from center are slightly smaller
            double scale = distance == 0 ? 1.0 : distance == 1 ? 0.92 : 0.82;
            _itemElements[i].RenderTransformOrigin = new Point(0.5, 0.5);
            _itemElements[i].RenderTransform = new ScaleTransform(scale, scale);
        }
    }

    // ------------------------------------------------------------------
    // Property change callbacks
    // ------------------------------------------------------------------

    private static void OnPanelTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DateTimePickerPanel panel && !panel._isApplyingTemplate && panel._itemsPanel != null)
        {
            int prevIndex = panel.SelectedIndex;
            panel.BuildItems();
            panel.SelectedIndex = Math.Min(prevIndex, Math.Max(0, panel.ItemCount - 1));
            panel.ScrollToSelected(animate: false);
        }
    }

    private static void OnSelectedIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DateTimePickerPanel panel && !panel._isApplyingTemplate)
        {
            panel.UpdateItemHighlights();
            panel.ScrollToSelected(animate: true);
            panel.RaiseEvent(new RoutedEventArgs(SelectionChangedEvent, panel));
        }
    }

    private static void OnItemHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DateTimePickerPanel panel && !panel._isApplyingTemplate && panel._itemsPanel != null)
        {
            panel.BuildItems();
            panel.ScrollToSelected(animate: false);
        }
    }

    private static void OnMaxDayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DateTimePickerPanel panel && panel.PanelType == DateTimePickerPanelType.Day)
        {
            int prevIndex = panel.SelectedIndex;
            int newMax = (int)e.NewValue;
            panel.BuildItems();
            panel.SelectedIndex = Math.Min(prevIndex, Math.Max(0, newMax - 1));
            panel.ScrollToSelected(animate: false);
        }
    }

    private void OnScrollViewerSizeChanged(object sender, SizeChangedEventArgs e)
    {
        // Re-center when layout changes
        ScrollToSelected(animate: false);
    }

    // ------------------------------------------------------------------
    // Input handling
    // ------------------------------------------------------------------

    private void OnMouseWheel(object sender, MouseWheelEventArgs e)
    {
        e.Handled = true;
        if (e.Delta > 0)
            MoveUp();
        else
            MoveDown();
    }

    private void OnTouchDown(object? sender, TouchEventArgs e)
    {
        _touchStartY = e.GetTouchPoint(this).Position.Y;
        _scrollStartOffset = _scrollViewer?.VerticalOffset ?? 0;
        _isTouchScrolling = true;
        e.Handled = true;
    }

    private void OnTouchMove(object? sender, TouchEventArgs e)
    {
        if (!_isTouchScrolling || _scrollViewer == null)
            return;

        double delta = _touchStartY - e.GetTouchPoint(this).Position.Y;
        _scrollViewer.ScrollToVerticalOffset(_scrollStartOffset + delta);
        e.Handled = true;
    }

    private void OnTouchUp(object? sender, TouchEventArgs e)
    {
        if (!_isTouchScrolling || _scrollViewer == null)
            return;

        _isTouchScrolling = false;

        // Snap to nearest item
        double offset = _scrollViewer.VerticalOffset;
        int index = (int)Math.Round(offset / ItemHeight);
        index = Math.Max(0, Math.Min(ItemCount - 1, index));

        if (index == SelectedIndex)
            ScrollToSelected(animate: true);
        else
            SelectedIndex = index;

        e.Handled = true;
    }

    // Handle click on an item to select it
    protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnPreviewMouseLeftButtonUp(e);

        if (_scrollViewer == null || _itemsPanel == null)
            return;

        // Find which item was clicked by computing its index from y position
        var pos = e.GetPosition(_itemsPanel);
        double spacerHeight = VisibleItemCount / 2 * ItemHeight;
        double itemY = pos.Y - spacerHeight;
        if (itemY < 0)
            return;

        int clickedIndex = (int)(itemY / ItemHeight);
        if (clickedIndex >= 0 && clickedIndex < ItemCount)
            SelectedIndex = clickedIndex;
    }
}

/// <summary>
/// Helper to animate ScrollViewer.VerticalOffset.
/// </summary>
internal static class ScrollViewerHelper
{
    public static readonly DependencyProperty VerticalOffsetProperty =
        DependencyProperty.RegisterAttached(
            "VerticalOffset",
            typeof(double),
            typeof(ScrollViewerHelper),
            new PropertyMetadata(0.0, OnVerticalOffsetChanged));

    private static void OnVerticalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ScrollViewer sv)
            sv.ScrollToVerticalOffset((double)e.NewValue);
    }

    public static double GetVerticalOffset(DependencyObject obj) => (double)obj.GetValue(VerticalOffsetProperty);
    public static void SetVerticalOffset(DependencyObject obj, double value) => obj.SetValue(VerticalOffsetProperty, value);
}
