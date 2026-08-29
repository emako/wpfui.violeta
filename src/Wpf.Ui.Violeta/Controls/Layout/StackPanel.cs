using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Wpf.Ui.Controls;

[ContentProperty(nameof(Children))]
public class StackPanel : Panel
{
    public static readonly DependencyProperty OrientationProperty =
        DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(StackPanel), new FrameworkPropertyMetadata(Orientation.Vertical, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public static readonly DependencyProperty SpacingProperty =
        DependencyProperty.Register(nameof(Spacing), typeof(double), typeof(StackPanel), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public double Spacing
    {
        get => (double)GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var spacing = Spacing;
        var panelDesiredSize = new Size();
        var visibleCount = 0;

        if (Orientation == Orientation.Vertical)
        {
            availableSize.Height = double.PositiveInfinity;
            foreach (UIElement child in InternalChildren)
            {
                child.Measure(availableSize);

                if (child.Visibility == Visibility.Collapsed)
                {
                    continue;
                }

                var childDesiredSize = child.DesiredSize;
                panelDesiredSize.Height += childDesiredSize.Height;

                if (childDesiredSize.Width > panelDesiredSize.Width)
                {
                    panelDesiredSize.Width = childDesiredSize.Width;
                }

                visibleCount++;
            }

            if (visibleCount > 1)
            {
                panelDesiredSize.Height += spacing * (visibleCount - 1);
            }
        }
        else
        {
            availableSize.Width = double.PositiveInfinity;
            foreach (UIElement child in InternalChildren)
            {
                child.Measure(availableSize);

                if (child.Visibility == Visibility.Collapsed)
                {
                    continue;
                }

                var childDesiredSize = child.DesiredSize;
                panelDesiredSize.Width += childDesiredSize.Width;

                if (childDesiredSize.Height > panelDesiredSize.Height)
                {
                    panelDesiredSize.Height = childDesiredSize.Height;
                }

                visibleCount++;
            }

            if (visibleCount > 1)
            {
                panelDesiredSize.Width += spacing * (visibleCount - 1);
            }
        }

        return panelDesiredSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var offset = 0d;
        var spacing = Spacing;
        var isFirstVisible = true;

        if (Orientation == Orientation.Vertical)
        {
            foreach (UIElement child in InternalChildren)
            {
                if (child.Visibility == Visibility.Collapsed)
                {
                    child.Arrange(new Rect());
                    continue;
                }

                if (!isFirstVisible)
                {
                    offset += spacing;
                }

                isFirstVisible = false;

                var childDesiredSize = child.DesiredSize;
                child.Arrange(new Rect(0, offset, finalSize.Width, childDesiredSize.Height));
                offset += childDesiredSize.Height;
            }
        }
        else
        {
            foreach (UIElement child in InternalChildren)
            {
                if (child.Visibility == Visibility.Collapsed)
                {
                    child.Arrange(new Rect());
                    continue;
                }

                if (!isFirstVisible)
                {
                    offset += spacing;
                }

                isFirstVisible = false;

                var childDesiredSize = child.DesiredSize;
                child.Arrange(new Rect(offset, 0, childDesiredSize.Width, finalSize.Height));
                offset += childDesiredSize.Width;
            }
        }

        return finalSize;
    }
}
