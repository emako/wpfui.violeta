using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Wpf.Ui.Violeta.Controls;

public class TimelinePanel : Panel
{
    public static readonly DependencyProperty ModeProperty =
        Timeline.ModeProperty.AddOwner(typeof(TimelinePanel));

    public TimelineDisplayMode Mode
    {
        get => (TimelineDisplayMode)GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }

    static TimelinePanel()
    {
        ModeProperty.OverrideMetadata(typeof(TimelinePanel),
            new FrameworkPropertyMetadata(TimelineDisplayMode.Right,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        double left = 0, icon = 0, right = 0, height = 0;
        foreach (UIElement child in InternalChildren)
        {
            child.Measure(availableSize);
            if (child is TimelineItem t)
            {
                var (l, m, r) = t.GetWidth();
                left = Math.Max(left, l);
                icon = Math.Max(icon, m);
                right = Math.Max(right, r);
            }
            height += child.DesiredSize.Height;
        }
        return new Size(left + icon + right, height);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        double left = 0, mid = 0, right = 0;
        foreach (UIElement child in InternalChildren)
        {
            if (child is TimelineItem t)
            {
                var (l, m, r) = t.GetWidth();
                left = Math.Max(left, l);
                mid = Math.Max(mid, m);
                right = Math.Max(right, r);
            }
        }

        double y = 0;
        foreach (UIElement child in InternalChildren)
        {
            if (child is TimelineItem t)
            {
                t.SetWidth(left, mid, right);
                var h = t.DesiredSize.Height;
                child.Arrange(new Rect(0, y, left + mid + right, h));
                y += h;
            }
        }
        return new Size(left + mid + right, y);
    }
}
