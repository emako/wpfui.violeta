using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Wpf.Ui.Controls;

[ContentProperty(nameof(Children))]
public class WrapPanel : Panel
{
    public static readonly DependencyProperty OrientationProperty =
        DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(WrapPanel), new FrameworkPropertyMetadata(Orientation.Horizontal, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public static readonly DependencyProperty ItemWidthProperty =
        DependencyProperty.Register(nameof(ItemWidth), typeof(double), typeof(WrapPanel), new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public double ItemWidth
    {
        get => (double)GetValue(ItemWidthProperty);
        set => SetValue(ItemWidthProperty, value);
    }

    public static readonly DependencyProperty ItemHeightProperty =
        DependencyProperty.Register(nameof(ItemHeight), typeof(double), typeof(WrapPanel), new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public double ItemHeight
    {
        get => (double)GetValue(ItemHeightProperty);
        set => SetValue(ItemHeightProperty, value);
    }

    public static readonly DependencyProperty VerticalSpacingProperty =
        DependencyProperty.Register(nameof(VerticalSpacing), typeof(double), typeof(WrapPanel), new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public double VerticalSpacing
    {
        get => (double)GetValue(VerticalSpacingProperty);
        set => SetValue(VerticalSpacingProperty, value);
    }

    public static readonly DependencyProperty HorizontalSpacingProperty =
        DependencyProperty.Register(nameof(HorizontalSpacing), typeof(double), typeof(WrapPanel), new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public double HorizontalSpacing
    {
        get { return (double)GetValue(HorizontalSpacingProperty); }
        set { SetValue(HorizontalSpacingProperty, value); }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var itemWidth = ItemWidth;
        var itemHeight = ItemHeight;
        var verticalSpacing = VerticalSpacing;
        var horizontalSpacing = HorizontalSpacing;

        var offsetX = 0d;
        var offsetY = 0d;
        var maxLineLength = 0d;
        var currentLineSize = 0d;
        var currentLineLength = 0d;

        Func<Size, double> childWidthGetter = double.IsNaN(itemWidth) ? size => size.Width : size => itemWidth;
        Func<Size, double> childHeightGetter = double.IsNaN(itemHeight) ? size => size.Height : size => itemHeight;

        var internalChildren = InternalChildren;
        if (Orientation == Orientation.Horizontal)
        {
            availableSize.Height = double.PositiveInfinity;
            for (int i = 0; i < internalChildren.Count; i++)
            {
                var child = internalChildren[i];

                child.Measure(availableSize);
                var childDesiredSize = child.DesiredSize;
                var childWidth = childWidthGetter.Invoke(childDesiredSize);
                var childHeight = childHeightGetter.Invoke(childDesiredSize);

                offsetX += childWidth;

                if (child.Visibility != Visibility.Collapsed)
                    offsetX += horizontalSpacing;

                if (offsetX - horizontalSpacing > availableSize.Width)
                {
                    currentLineLength = offsetX - horizontalSpacing - childWidth - horizontalSpacing;
                    if (currentLineLength > maxLineLength)
                        maxLineLength = currentLineLength;

                    offsetX = childWidth + horizontalSpacing;
                    offsetY += currentLineSize;
                    offsetY += verticalSpacing;
                    currentLineSize = 0;
                }

                if (childHeight > currentLineSize)
                    currentLineSize = childHeight;
            }

            currentLineLength = offsetX - horizontalSpacing;
            if (currentLineLength > maxLineLength)
                maxLineLength = currentLineLength;

            offsetX = 0;
            offsetY += currentLineSize;
            offsetY += verticalSpacing;

            currentLineSize = 0;

            return new Size(maxLineLength, offsetY - verticalSpacing);
        }
        else
        {
            availableSize.Width = double.PositiveInfinity;
            for (int i = 0; i < internalChildren.Count; i++)
            {
                var child = internalChildren[i];

                child.Measure(availableSize);
                var childDesiredSize = child.DesiredSize;
                var childWidth = childWidthGetter.Invoke(childDesiredSize);
                var childHeight = childHeightGetter.Invoke(childDesiredSize);

                offsetY += childHeight;

                if (child.Visibility != Visibility.Collapsed)
                    offsetY += verticalSpacing;

                if (offsetY - verticalSpacing > availableSize.Height)
                {
                    currentLineLength = offsetY - horizontalSpacing - childHeight - verticalSpacing;
                    if (currentLineLength > maxLineLength)
                        maxLineLength = currentLineLength;

                    offsetY = childHeight + verticalSpacing;
                    offsetX += currentLineSize;
                    offsetX += horizontalSpacing;
                    currentLineSize = 0;
                }

                if (childWidth > currentLineSize)
                    currentLineSize = childWidth;
            }

            currentLineLength = offsetY - verticalSpacing;
            if (currentLineLength > maxLineLength)
                maxLineLength = currentLineLength;

            offsetY = 0;
            offsetX += currentLineSize;
            offsetX += horizontalSpacing;
            currentLineSize = 0;

            return new Size(offsetX - horizontalSpacing, maxLineLength);
        }
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var itemWidth = ItemWidth;
        var itemHeight = ItemHeight;
        var verticalSpacing = VerticalSpacing;
        var horizontalSpacing = HorizontalSpacing;

        var tempOffset = 0d;

        Func<Size, double> childWidthGetter = double.IsNaN(itemWidth) ? size => size.Width : size => itemWidth;
        Func<Size, double> childHeightGetter = double.IsNaN(itemHeight) ? size => size.Height : size => itemHeight;

        var internalChildren = InternalChildren;
        var currentLineSize = 0d;
        var currentLineOffsetX = 0d;
        var currentLineOffsetY = 0d;
        var currentLineIndexStart = 0;
        if (Orientation == Orientation.Horizontal)
        {
            for (int i = 0; i < internalChildren.Count; i++)
            {
                var child = internalChildren[i];
                var childDesiredSize = child.DesiredSize;
                var childWidth = childWidthGetter.Invoke(childDesiredSize);
                var childHeight = childHeightGetter.Invoke(childDesiredSize);

                tempOffset += childWidth;

                if (child.Visibility != Visibility.Collapsed)
                    tempOffset += horizontalSpacing;

                if (tempOffset - horizontalSpacing > finalSize.Width)
                {
                    ArrangeLineHorizontal(
                        internalChildren,
                        currentLineIndexStart,
                        i,
                        currentLineOffsetX,
                        currentLineOffsetY,
                        currentLineSize,
                        horizontalSpacing,
                        childWidthGetter,
                        childHeightGetter);

                    currentLineOffsetX = 0;
                    currentLineOffsetY += currentLineSize;
                    currentLineOffsetY += verticalSpacing;
                    currentLineIndexStart = i;

                    currentLineSize = 0;
                    tempOffset = childWidth + horizontalSpacing;
                }

                if (childHeight > currentLineSize)
                    currentLineSize = childHeight;
            }

            ArrangeLineHorizontal(
                internalChildren,
                currentLineIndexStart,
                internalChildren.Count,
                currentLineOffsetX,
                currentLineOffsetY,
                currentLineSize,
                horizontalSpacing,
                childWidthGetter,
                childHeightGetter);
        }
        else
        {
            for (int i = 0; i < internalChildren.Count; i++)
            {
                var child = internalChildren[i];
                var childDesiredSize = child.DesiredSize;
                var childWidth = childWidthGetter.Invoke(childDesiredSize);
                var childHeight = childHeightGetter.Invoke(childDesiredSize);

                tempOffset += childHeight;

                if (child.Visibility != Visibility.Collapsed)
                    tempOffset += verticalSpacing;

                if (tempOffset - verticalSpacing > finalSize.Height)
                {
                    ArrangeLineVertical(
                        internalChildren,
                        currentLineIndexStart,
                        i,
                        currentLineOffsetX,
                        currentLineOffsetY,
                        currentLineSize,
                        verticalSpacing,
                        childWidthGetter,
                        childHeightGetter);

                    currentLineOffsetY = 0;
                    currentLineOffsetX += currentLineSize;
                    currentLineOffsetX += horizontalSpacing;
                    currentLineIndexStart = i;

                    currentLineSize = 0;
                    tempOffset = childHeight + verticalSpacing;
                }

                if (childWidth > currentLineSize)
                    currentLineSize = childWidth;
            }

            ArrangeLineVertical(
                internalChildren,
                currentLineIndexStart,
                internalChildren.Count,
                currentLineOffsetX,
                currentLineOffsetY,
                currentLineSize,
                verticalSpacing,
                childWidthGetter,
                childHeightGetter);
        }

        return finalSize;

        static void ArrangeLineHorizontal(
            UIElementCollection children,
            int childIndexStart,
            int childIndexEnd,
            double currentLineOffsetX,
            double currentLineOffsetY,
            double currentLineSize,
            double spacing,
            Func<Size, double> widthGetter,
            Func<Size, double> heightGetter)
        {
            var lineChildOffset = 0d;
            for (int j = childIndexStart; j < childIndexEnd; j++)
            {
                var lineChild = children[j];
                var lineChildDesiredSize = lineChild.DesiredSize;
                var lineChildWidth = widthGetter.Invoke(lineChildDesiredSize);
                var lineChildHeight = heightGetter.Invoke(lineChildDesiredSize);

                lineChild.Arrange(new Rect(currentLineOffsetX + lineChildOffset, currentLineOffsetY, lineChildWidth, currentLineSize));

                lineChildOffset += lineChildWidth;

                if (lineChild.Visibility != Visibility.Collapsed)
                    lineChildOffset += spacing;
            }
        }

        static void ArrangeLineVertical(
            UIElementCollection children,
            int childIndexStart,
            int childIndexEnd,
            double currentLineOffsetX,
            double currentLineOffsetY,
            double currentLineSize,
            double spacing,
            Func<Size, double> widthGetter,
            Func<Size, double> heightGetter)
        {
            var lineChildOffset = 0d;
            for (int j = childIndexStart; j < childIndexEnd; j++)
            {
                var lineChild = children[j];
                var lineChildDesiredSize = lineChild.DesiredSize;
                var lineChildWidth = widthGetter.Invoke(lineChildDesiredSize);
                var lineChildHeight = heightGetter.Invoke(lineChildDesiredSize);

                lineChild.Arrange(new Rect(currentLineOffsetX, currentLineOffsetY + lineChildOffset, currentLineSize, lineChildHeight));

                lineChildOffset += lineChildHeight;

                if (lineChild.Visibility != Visibility.Collapsed)
                    lineChildOffset += spacing;
            }
        }
    }
}
