using System;
using System.Windows;
using System.Windows.Controls;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// A panel that arranges children horizontally and wraps to the next row when needed.
/// Each child occupies one or more column units based on its desired width.
/// </summary>
public class ColumnWrapPanel : Panel
{
    public static readonly DependencyProperty ColumnProperty = DependencyProperty.Register(
        nameof(Column),
        typeof(int),
        typeof(ColumnWrapPanel),
        new FrameworkPropertyMetadata(int.MaxValue, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange, null, CoerceColumn));

    public int Column
    {
        get => (int)GetValue(ColumnProperty);
        set => SetValue(ColumnProperty, value);
    }

    private static object CoerceColumn(DependencyObject d, object baseValue)
    {
        var value = (int)baseValue;
        return value <= 0 ? 1 : value;
    }

    private static Size GetLayoutSize(UIElement child)
    {
        Thickness margin = child is FrameworkElement fe ? fe.Margin : default;
        Size desired = child.DesiredSize;
        return new Size(
            desired.Width + margin.Left + margin.Right,
            desired.Height + margin.Top + margin.Bottom);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        if (Children.Count == 0)
        {
            return new Size(0, 0);
        }

        double width = double.IsPositiveInfinity(availableSize.Width) ? 0 : availableSize.Width;
        double unit = width / Column;
        double x = 0;
        double y = 0;
        double rowHeight = 0;
        double maxWidth = 0;

        foreach (UIElement child in Children)
        {
            child.Measure(new Size(double.PositiveInfinity, availableSize.Height));
            Size layoutSize = GetLayoutSize(child);
            int colSpan = unit > 0
                ? Math.Min(Column, Math.Max(1, (int)Math.Ceiling(layoutSize.Width / unit)))
                : 1;
            double childWidth = colSpan * unit;

            if (x + childWidth > width + 0.001 && x > 0)
            {
                maxWidth = Math.Max(maxWidth, x);
                x = 0;
                y += rowHeight;
                rowHeight = 0;
            }

            x += childWidth;
            rowHeight = Math.Max(rowHeight, layoutSize.Height);
        }

        maxWidth = Math.Max(maxWidth, x);
        return new Size(
            double.IsPositiveInfinity(availableSize.Width) ? maxWidth : availableSize.Width,
            y + rowHeight);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (Children.Count == 0)
        {
            return finalSize;
        }

        double unit = finalSize.Width / Column;
        double x = 0;
        double y = 0;
        double rowHeight = 0;

        foreach (UIElement child in Children)
        {
            Thickness margin = child is FrameworkElement fe ? fe.Margin : default;
            Size layoutSize = GetLayoutSize(child);
            double remainingWidth = finalSize.Width - x;

            if (layoutSize.Width > remainingWidth + 0.001 && x > 0)
            {
                x = 0;
                y += rowHeight;
                rowHeight = 0;
            }

            child.Arrange(new Rect(
                x + margin.Left,
                y + margin.Top,
                child.DesiredSize.Width,
                child.DesiredSize.Height));

            int colSpan = unit > 0
                ? Math.Min(Column, Math.Max(1, (int)Math.Ceiling(layoutSize.Width / unit)))
                : 1;
            x += colSpan * unit;
            rowHeight = Math.Max(rowHeight, layoutSize.Height);
        }

        return finalSize;
    }
}
