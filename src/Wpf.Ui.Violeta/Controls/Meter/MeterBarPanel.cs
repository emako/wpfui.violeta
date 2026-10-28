using System;
using System.Windows;
using System.Windows.Controls;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Lays out <see cref="MeterBarItem"/> children left to right, each sized by its share of <see cref="Total"/>.
/// </summary>
public class MeterBarPanel : Panel
{
    public static readonly DependencyProperty TotalProperty =
        DependencyProperty.Register(
            nameof(Total),
            typeof(double),
            typeof(MeterBarPanel),
            new FrameworkPropertyMetadata(100d, FrameworkPropertyMetadataOptions.AffectsArrange));

    public double Total
    {
        get => (double)GetValue(TotalProperty);
        set => SetValue(TotalProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        foreach (UIElement child in InternalChildren)
        {
            child.Measure(availableSize);
        }

        return default;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        double total = Total;
        bool valid = !double.IsNaN(total) && !double.IsInfinity(total) && total > 0;
        double used = 0;

        foreach (UIElement child in InternalChildren)
        {
            double left = valid ? finalSize.Width * used / total : 0;
            if (valid && child is MeterBarItem item)
            {
                double value = double.IsNaN(item.Value) || item.Value <= 0 ? 0 : item.Value;
                used += Math.Min(value, total - used);
            }

            double right = valid ? finalSize.Width * used / total : 0;
            child.Arrange(new Rect(left, 0, right - left, finalSize.Height));
        }

        return finalSize;
    }

    protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
    {
        base.OnVisualChildrenChanged(visualAdded, visualRemoved);
        (ItemsControl.GetItemsOwner(this) as MeterBar)?.SyncItems(InternalChildren);
    }
}
