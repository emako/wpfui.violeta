using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Hosts <see cref="ToolBar"/> items that have been marked as overflow.
/// Visual-only parenting: items stay logical children of <see cref="ToolBar"/>.
/// </summary>
public class ToolBarOverflowPanel : Panel
{
    public static readonly DependencyProperty WrapWidthProperty = DependencyProperty.Register(
        nameof(WrapWidth),
        typeof(double),
        typeof(ToolBarOverflowPanel),
        new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public static readonly DependencyProperty MaxItemsPerRowProperty = DependencyProperty.Register(
        nameof(MaxItemsPerRow),
        typeof(int),
        typeof(ToolBarOverflowPanel),
        new FrameworkPropertyMetadata(3, FrameworkPropertyMetadataOptions.AffectsMeasure));

    /// <summary>
    /// Maximum row width before wrapping. Values &lt;= 0 do not constrain the row width.
    /// </summary>
    public double WrapWidth
    {
        get => (double)GetValue(WrapWidthProperty);
        set => SetValue(WrapWidthProperty, value);
    }

    /// <summary>
    /// Maximum number of overflow items in a horizontal row. Values &lt;= 0 disable the item-count limit.
    /// </summary>
    public int MaxItemsPerRow
    {
        get => (int)GetValue(MaxItemsPerRowProperty);
        set => SetValue(MaxItemsPerRowProperty, value);
    }

    internal ToolBar? ToolBar { get; set; }

    /// <summary>
    /// Avoid claiming logical ownership — ToolBar items are already logical children of the ItemsControl.
    /// </summary>
    protected override UIElementCollection CreateUIElementCollection(FrameworkElement logicalParent)
        => new(this, logicalParent: null!);

    protected override Size MeasureOverride(Size constraint)
    {
        SyncOverflowChildren();

        double spacing = Math.Max(0, ToolBar?.ItemSpacing ?? 0);
        double wrapWidth = WrapWidth;
        int maxItemsPerRow = MaxItemsPerRow;
        bool hasWidthLimit = wrapWidth > 0;
        bool hasItemLimit = maxItemsPerRow > 0;

        double panelWidth = 0;
        double panelHeight = 0;
        double rowWidth = 0;
        double rowHeight = 0;
        int rowItemCount = 0;
        int rowCount = 0;

        var infinite = new Size(
            hasWidthLimit ? wrapWidth : double.PositiveInfinity,
            double.PositiveInfinity);

        foreach (UIElement child in Children)
        {
            child.Measure(infinite);
            Size size = child.DesiredSize;

            double gap = rowItemCount > 0 ? spacing : 0;
            bool exceedsWidth = hasWidthLimit && rowItemCount > 0 && rowWidth + gap + size.Width > wrapWidth;
            bool exceedsItemCount = hasItemLimit && rowItemCount >= maxItemsPerRow;
            if (exceedsWidth || exceedsItemCount)
            {
                panelWidth = Math.Max(panelWidth, rowWidth);
                panelHeight += rowHeight + (rowCount > 0 ? spacing : 0);
                rowCount++;
                rowWidth = 0;
                rowHeight = 0;
                rowItemCount = 0;
                gap = 0;
            }

            rowWidth += gap + size.Width;
            rowHeight = Math.Max(rowHeight, size.Height);
            rowItemCount++;
        }

        panelWidth = Math.Max(panelWidth, rowWidth);
        if (rowItemCount > 0)
        {
            panelHeight += rowHeight + (rowCount > 0 ? spacing : 0);
        }

        return new Size(panelWidth, panelHeight);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        double spacing = Math.Max(0, ToolBar?.ItemSpacing ?? 0);
        double wrapWidth = WrapWidth;
        int maxItemsPerRow = MaxItemsPerRow;
        bool hasWidthLimit = wrapWidth > 0;
        bool hasItemLimit = maxItemsPerRow > 0;

        double x = 0;
        double y = 0;
        double rowHeight = 0;
        int rowItemCount = 0;
        double limit = hasWidthLimit ? Math.Min(wrapWidth, finalSize.Width) : finalSize.Width;

        foreach (UIElement child in Children)
        {
            Size size = child.DesiredSize;
            bool exceedsWidth = hasWidthLimit && rowItemCount > 0 && x + spacing + size.Width > limit;
            bool exceedsItemCount = hasItemLimit && rowItemCount >= maxItemsPerRow;
            if (exceedsWidth || exceedsItemCount)
            {
                x = 0;
                y += rowHeight + spacing;
                rowHeight = 0;
                rowItemCount = 0;
            }

            if (rowItemCount > 0)
            {
                x += spacing;
            }

            child.Arrange(new Rect(x, y, size.Width, size.Height));
            x += size.Width;
            rowHeight = Math.Max(rowHeight, size.Height);
            rowItemCount++;
        }

        return finalSize;
    }

    private void SyncOverflowChildren()
    {
        var toolBar = ToolBar;
        if (toolBar is null)
        {
            return;
        }

        var generator = toolBar.ItemContainerGenerator;
        var desired = new List<UIElement>();

        for (int i = 0; i < toolBar.Items.Count; i++)
        {
            if (generator.ContainerFromIndex(i) is not UIElement child)
            {
                continue;
            }

            if (!ToolBar.GetIsOverflowItem(child))
            {
                continue;
            }

            if (child is Separator)
            {
                DetachChild(child);
                continue;
            }

            desired.Add(child);
        }

        for (int i = Children.Count - 1; i >= 0; i--)
        {
            if (!desired.Contains(Children[i]))
            {
                Children.RemoveAt(i);
            }
        }

        for (int i = 0; i < desired.Count; i++)
        {
            UIElement child = desired[i];
            var parent = VisualTreeHelper.GetParent(child);

            if (ReferenceEquals(parent, this))
            {
                int currentIndex = Children.IndexOf(child);
                if (currentIndex != i && currentIndex >= 0)
                {
                    Children.RemoveAt(currentIndex);
                    Children.Insert(Math.Min(i, Children.Count), child);
                }

                continue;
            }

            if (parent is ToolBarPanel toolBarPanel)
            {
                toolBarPanel.DetachChild(child);
            }
            else if (parent is Panel panel)
            {
                panel.Children.Remove(child);
            }

            Children.Insert(Math.Min(i, Children.Count), child);
        }
    }

    internal void DetachChild(UIElement child)
    {
        int index = Children.IndexOf(child);
        if (index >= 0)
        {
            Children.RemoveAt(index);
        }
    }

    internal void RemoveOrphans(HashSet<UIElement> keep)
    {
        for (int i = Children.Count - 1; i >= 0; i--)
        {
            if (!keep.Contains(Children[i]))
            {
                Children.RemoveAt(i);
            }
        }
    }
}
