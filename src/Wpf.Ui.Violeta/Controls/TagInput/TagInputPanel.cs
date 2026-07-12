using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// A wrap panel that lays out regular children in a wrapping flow and then renders
/// a special <see cref="TrailingItem"/> element at the end of the last row (or on a new
/// row when there is not enough remaining space). This is a WPF port of Ursa.Avalonia's
/// <c>WrapPanelWithTrailingItem</c>.
/// </summary>
public class TagInputPanel : Panel
{
    private UIElement? _trailingItem;

    /// <summary>
    /// Minimum remaining row-width needed to keep the trailing item on the same row.
    /// If less space is available, the trailing item wraps to a new row.
    /// </summary>
    public static readonly DependencyProperty TrailingMinWidthProperty =
        DependencyProperty.Register(
            nameof(TrailingMinWidth),
            typeof(double),
            typeof(TagInputPanel),
            new FrameworkPropertyMetadata(
                60.0,
                FrameworkPropertyMetadataOptions.AffectsMeasure |
                FrameworkPropertyMetadataOptions.AffectsArrange));

    public double TrailingMinWidth
    {
        get => (double)GetValue(TrailingMinWidthProperty);
        set => SetValue(TrailingMinWidthProperty, value);
    }

    /// <summary>
    /// The element that is always placed after all regular children in wrap order.
    /// Typically the input <see cref="TextBox"/> inside a <see cref="TagInput"/>.
    /// </summary>
    public UIElement? TrailingItem
    {
        get => _trailingItem;
        set
        {
            if (_trailingItem == value)
                return;

            if (_trailingItem is not null)
            {
                RemoveVisualChild(_trailingItem);
                RemoveLogicalChild(_trailingItem);
            }

            _trailingItem = value;

            if (_trailingItem is not null)
            {
                AddVisualChild(_trailingItem);
                AddLogicalChild(_trailingItem);
            }

            InvalidateMeasure();
        }
    }

    // ── Visual children ───────────────────────────────────────────────────

    protected override int VisualChildrenCount =>
        InternalChildren.Count + (_trailingItem is not null ? 1 : 0);

    protected override Visual GetVisualChild(int index)
    {
        if (index < InternalChildren.Count)
            return InternalChildren[index];
        if (_trailingItem is not null && index == InternalChildren.Count)
            return _trailingItem;
        throw new ArgumentOutOfRangeException(nameof(index));
    }

    // ── Layout ────────────────────────────────────────────────────────────

    protected override Size MeasureOverride(Size availableSize)
    {
        double lineX = 0, lineH = 0, totalH = 0;

        foreach (UIElement child in InternalChildren)
        {
            child.Measure(availableSize);
            double w = child.DesiredSize.Width;
            double deltaX = availableSize.Width - lineX;

            if (deltaX >= w)
            {
                lineX += w;
                lineH = Math.Max(lineH, child.DesiredSize.Height);
            }
            else
            {
                totalH += lineH;
                lineX = w;
                lineH = child.DesiredSize.Height;
            }
        }

        if (_trailingItem is not null)
        {
            _trailingItem.Measure(new Size(availableSize.Width, double.PositiveInfinity));
            double remaining = availableSize.Width - lineX;

            if (remaining < TrailingMinWidth)
            {
                // Trailing item goes to a new row.
                totalH += lineH;
                totalH += _trailingItem.DesiredSize.Height;
            }
            else
            {
                lineH = Math.Max(lineH, _trailingItem.DesiredSize.Height);
                totalH += lineH;
            }
        }
        else
        {
            totalH += lineH;
        }

        return new Size(availableSize.Width, totalH);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        double lineX = 0, lineH = 0, totalH = 0;

        foreach (UIElement child in InternalChildren)
        {
            double w = child.DesiredSize.Width;
            double deltaX = finalSize.Width - lineX;

            if (deltaX >= w)
            {
                child.Arrange(new Rect(lineX, totalH, w, Math.Max(child.DesiredSize.Height, lineH)));
                lineX += w;
                lineH = Math.Max(lineH, child.DesiredSize.Height);
            }
            else
            {
                totalH += lineH;
                child.Arrange(new Rect(0, totalH, Math.Min(w, finalSize.Width), child.DesiredSize.Height));
                lineX = w;
                lineH = child.DesiredSize.Height;
            }
        }

        if (_trailingItem is not null)
        {
            double remaining = finalSize.Width - lineX;

            if (remaining < TrailingMinWidth)
            {
                // New row for the trailing item.
                totalH += lineH;
                _trailingItem.Arrange(new Rect(0, totalH, finalSize.Width, _trailingItem.DesiredSize.Height));
                totalH += _trailingItem.DesiredSize.Height;
            }
            else
            {
                // When there are no tag chips the trailing item should fill the full row height so
                // the inner TextBox vertically centres inside the control.
                double h = InternalChildren.Count == 0
                    ? finalSize.Height
                    : Math.Max(lineH, _trailingItem.DesiredSize.Height);

                _trailingItem.Arrange(new Rect(lineX, totalH, remaining, h));
                totalH += Math.Max(lineH, _trailingItem.DesiredSize.Height);
            }
        }
        else
        {
            totalH += lineH;
        }

        return new Size(finalSize.Width, totalH);
    }
}
