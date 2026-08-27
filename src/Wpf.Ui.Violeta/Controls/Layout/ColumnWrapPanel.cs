using System;
using System.Windows;
using System.Windows.Controls;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// A panel that arranges children horizontally and wraps to the next row when needed.
/// Ported from Ursa <see cref="ColumnWrapPanel"/>.
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

    protected override Size MeasureOverride(Size availableSize)
    {
        if (Children.Count == 0)
        {
            return new Size(0, 0);
        }

        if (double.IsPositiveInfinity(availableSize.Width) || availableSize.Width <= 0)
        {
            double width = 0;
            double height = 0;
            foreach (UIElement child in Children)
            {
                child.Measure(availableSize);
                width = Math.Max(width, child.DesiredSize.Width);
                height = Math.Max(height, child.DesiredSize.Height);
            }

            return new Size(width, height);
        }

        double unit = availableSize.Width / Column;
        double x = 0;
        double y = 0;
        double rowHeight = 0;

        foreach (UIElement child in Children)
        {
            child.Measure(availableSize);
            Size desiredSize = child.DesiredSize;
            int colSpan = (int)Math.Ceiling(desiredSize.Width / unit);
            if (colSpan > Column)
            {
                colSpan = Column;
            }

            double childWidth = colSpan * unit;
            if (GreaterThan(x + childWidth, availableSize.Width) && x > 0)
            {
                x = 0;
                y += rowHeight;
                rowHeight = 0;
            }

            x += childWidth;
            rowHeight = Math.Max(rowHeight, desiredSize.Height);
        }

        return new Size(availableSize.Width, y + rowHeight);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (Children.Count == 0 || finalSize.Width <= 0)
        {
            return finalSize;
        }

        double unit = finalSize.Width / Column;
        double x = 0;
        double y = 0;
        double rowHeight = 0;

        foreach (UIElement child in Children)
        {
            Size desiredSize = child.DesiredSize;
            double remainingWidth = finalSize.Width - x;

            if (GreaterThan(desiredSize.Width, remainingWidth) && x > 0)
            {
                x = 0;
                y += rowHeight;
                rowHeight = 0;
            }

            child.Arrange(new Rect(x, y, desiredSize.Width, desiredSize.Height));

            int colSpan = (int)Math.Ceiling(desiredSize.Width / unit);
            if (colSpan > Column)
            {
                colSpan = Column;
            }

            x += colSpan * unit;
            rowHeight = Math.Max(rowHeight, desiredSize.Height);
        }

        return finalSize;
    }

    private static bool GreaterThan(double a, double b) => a > b + 0.001;
}
