using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Controls;

public class Timeline : ItemsControl
{
    public static readonly DependencyProperty IconMemberPathProperty =
        DependencyProperty.Register(nameof(IconMemberPath), typeof(string), typeof(Timeline),
            new PropertyMetadata(null));

    public string? IconMemberPath
    {
        get => (string?)GetValue(IconMemberPathProperty);
        set => SetValue(IconMemberPathProperty, value);
    }

    public static readonly DependencyProperty HeaderMemberPathProperty =
        DependencyProperty.Register(nameof(HeaderMemberPath), typeof(string), typeof(Timeline),
            new PropertyMetadata(null));

    public string? HeaderMemberPath
    {
        get => (string?)GetValue(HeaderMemberPathProperty);
        set => SetValue(HeaderMemberPathProperty, value);
    }

    public static readonly DependencyProperty ContentMemberPathProperty =
        DependencyProperty.Register(nameof(ContentMemberPath), typeof(string), typeof(Timeline),
            new PropertyMetadata(null));

    public string? ContentMemberPath
    {
        get => (string?)GetValue(ContentMemberPathProperty);
        set => SetValue(ContentMemberPathProperty, value);
    }

    public static readonly DependencyProperty TimeMemberPathProperty =
        DependencyProperty.Register(nameof(TimeMemberPath), typeof(string), typeof(Timeline),
            new PropertyMetadata(null));

    public string? TimeMemberPath
    {
        get => (string?)GetValue(TimeMemberPathProperty);
        set => SetValue(TimeMemberPathProperty, value);
    }

    public static readonly DependencyProperty TimeFormatProperty =
        DependencyProperty.Register(nameof(TimeFormat), typeof(string), typeof(Timeline),
            new PropertyMetadata("yyyy-MM-dd HH:mm:ss"));

    public string? TimeFormat
    {
        get => (string?)GetValue(TimeFormatProperty);
        set => SetValue(TimeFormatProperty, value);
    }

    public static readonly DependencyProperty IconTemplateProperty =
        DependencyProperty.Register(nameof(IconTemplate), typeof(DataTemplate), typeof(Timeline),
            new PropertyMetadata(null));

    public DataTemplate? IconTemplate
    {
        get => (DataTemplate?)GetValue(IconTemplateProperty);
        set => SetValue(IconTemplateProperty, value);
    }

    public static readonly DependencyProperty DescriptionTemplateProperty =
        DependencyProperty.Register(nameof(DescriptionTemplate), typeof(DataTemplate), typeof(Timeline),
            new PropertyMetadata(null));

    public DataTemplate? DescriptionTemplate
    {
        get => (DataTemplate?)GetValue(DescriptionTemplateProperty);
        set => SetValue(DescriptionTemplateProperty, value);
    }

    public static readonly DependencyProperty ModeProperty =
        DependencyProperty.Register(nameof(Mode), typeof(TimelineDisplayMode), typeof(Timeline),
            new FrameworkPropertyMetadata(TimelineDisplayMode.Right,
                FrameworkPropertyMetadataOptions.AffectsArrange,
                OnModeChanged));

    public TimelineDisplayMode Mode
    {
        get => (TimelineDisplayMode)GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }

    static Timeline()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Timeline),
            new FrameworkPropertyMetadata(typeof(Timeline)));
    }

    private static void OnModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var tl = (Timeline)d;
        tl.OnDisplayModeChanged((TimelineDisplayMode)e.NewValue);
    }

    private void OnDisplayModeChanged(TimelineDisplayMode mode)
    {
        if (GetItemsHostPanel() is TimelinePanel panel)
        {
            panel.Mode = mode;
        }
        SetItemMode();
    }

    protected override bool IsItemItsOwnContainerOverride(object item)
        => item is TimelineItem;

    protected override DependencyObject GetContainerForItemOverride()
        => new TimelineItem();

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);
        if (element is TimelineItem t)
        {
            if (!string.IsNullOrEmpty(IconMemberPath))
                BindingOperations.SetBinding(t, TimelineItem.IconProperty, new Binding(IconMemberPath));
            if (!string.IsNullOrEmpty(HeaderMemberPath))
                BindingOperations.SetBinding(t, HeaderedContentControl.HeaderProperty, new Binding(HeaderMemberPath));
            if (!string.IsNullOrEmpty(ContentMemberPath))
                BindingOperations.SetBinding(t, ContentControl.ContentProperty, new Binding(ContentMemberPath));
            if (!string.IsNullOrEmpty(TimeMemberPath))
                BindingOperations.SetBinding(t, TimelineItem.TimeProperty, new Binding(TimeMemberPath));

            if (t.ReadLocalValue(TimelineItem.TimeFormatProperty) == DependencyProperty.UnsetValue)
                t.SetValue(TimelineItem.TimeFormatProperty, TimeFormat);
            if (t.ReadLocalValue(TimelineItem.IconTemplateProperty) == DependencyProperty.UnsetValue)
                t.SetValue(TimelineItem.IconTemplateProperty, IconTemplate);
            if (t.ReadLocalValue(HeaderedContentControl.HeaderTemplateProperty) == DependencyProperty.UnsetValue)
                t.SetValue(HeaderedContentControl.HeaderTemplateProperty, ItemTemplate);
            if (t.ReadLocalValue(ContentControl.ContentTemplateProperty) == DependencyProperty.UnsetValue)
                t.SetValue(ContentControl.ContentTemplateProperty, DescriptionTemplate);
        }
    }

    protected override Size ArrangeOverride(Size arrangeBounds)
    {
        if (GetItemsHostPanel() is TimelinePanel panel)
            panel.Mode = Mode;
        SetItemMode();
        return base.ArrangeOverride(arrangeBounds);
    }

    private void SetItemMode()
    {
        if (GetItemsHostPanel() is not TimelinePanel panel) return;
        var items = panel.Children.OfType<TimelineItem>().ToList();
        switch (Mode)
        {
            case TimelineDisplayMode.Left:
                foreach (var item in items)
                    SetIfUnset(item, TimelineItem.PositionProperty, TimelineItemPosition.Left);
                break;
            case TimelineDisplayMode.Right:
                foreach (var item in items)
                    SetIfUnset(item, TimelineItem.PositionProperty, TimelineItemPosition.Right);
                break;
            case TimelineDisplayMode.Center:
                foreach (var item in items)
                    SetIfUnset(item, TimelineItem.PositionProperty, TimelineItemPosition.Separate);
                break;
            case TimelineDisplayMode.Alternate:
                bool left = false;
                foreach (var item in items)
                {
                    SetIfUnset(item, TimelineItem.PositionProperty,
                        left ? TimelineItemPosition.Left : TimelineItemPosition.Right);
                    left = !left;
                }
                break;
        }
    }

    private static void SetIfUnset(TimelineItem item, DependencyProperty property, object value)
    {
        if (item.ReadLocalValue(property) == DependencyProperty.UnsetValue)
            item.SetCurrentValue(property, value);
    }

    private TimelinePanel? GetItemsHostPanel()
    {
        return FindVisualDescendant<TimelinePanel>(this);
    }

    private static T? FindVisualDescendant<T>(DependencyObject parent) where T : DependencyObject
    {
        int count = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T found) return found;
            var result = FindVisualDescendant<T>(child);
            if (result != null) return result;
        }
        return null;
    }

    internal void InvalidateContainers()
    {
        if (GetItemsHostPanel() is not Panel host) return;
        var items = host.Children.OfType<TimelineItem>().ToList();
        for (var i = 0; i < items.Count; i++)
        {
            items[i].SetEnd(i == 0, i == items.Count - 1);
        }
    }
}
