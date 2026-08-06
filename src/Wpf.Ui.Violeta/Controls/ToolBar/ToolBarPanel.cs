using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Primary items host for <see cref="ToolBar"/>. Measures available width and marks / reparents overflow items.
/// Visual-only parenting — items remain logical children of <see cref="ToolBar"/>.
/// </summary>
public class ToolBarPanel : Panel
{
    internal ToolBar? ToolBar { get; set; }

    /// <summary>
    /// Do not take logical ownership; ToolBar items are already logical children of the ItemsControl.
    /// </summary>
    protected override UIElementCollection CreateUIElementCollection(FrameworkElement logicalParent)
        => new(this, logicalParent: null!);

    protected override Size MeasureOverride(Size constraint)
    {
        var toolBar = ToolBar;
        if (toolBar is null)
        {
            return MeasureAsStack(constraint, spacing: 0);
        }

        toolBar.EnsureContainersRealized();

        var generator = toolBar.ItemContainerGenerator;
        int count = toolBar.Items.Count;
        if (count == 0)
        {
            toolBar.SetHasOverflowItems(false);
            return new Size(0, 0);
        }

        double spacing = Math.Max(0, toolBar.ItemSpacing);
        var infinite = new Size(double.PositiveInfinity, constraint.Height);

        double maxHeight = 0;
        var modes = new ToolBarOverflowMode[count];
        var sizes = new Size[count];

        for (int i = 0; i < count; i++)
        {
            if (generator.ContainerFromIndex(i) is not UIElement child)
            {
                modes[i] = ToolBarOverflowMode.AsNeeded;
                sizes[i] = new Size();
                continue;
            }

            modes[i] = ResolveOverflowMode(toolBar, i, child);
            child.Measure(infinite);
            sizes[i] = child.DesiredSize;
            maxHeight = Math.Max(maxHeight, sizes[i].Height);
        }

        double available = double.IsInfinity(constraint.Width) ? double.PositiveInfinity : constraint.Width;
        double remaining = available;
        bool sendToOverflow = false;
        bool hasAlways = false;
        bool hasAsNeededOverflow = false;
        double primaryWidth = 0;
        int primaryCount = 0;

        for (int i = 0; i < count; i++)
        {
            if (generator.ContainerFromIndex(i) is not UIElement child)
            {
                continue;
            }

            bool overflow;
            switch (modes[i])
            {
                case ToolBarOverflowMode.Always:
                    overflow = true;
                    hasAlways = true;
                    break;

                case ToolBarOverflowMode.Never:
                    overflow = false;
                    break;

                default:
                    {
                        double gap = primaryCount > 0 ? spacing : 0;
                        if (sendToOverflow
                            || (!double.IsInfinity(remaining) && sizes[i].Width + gap > remaining))
                        {
                            overflow = true;
                            sendToOverflow = true;
                            hasAsNeededOverflow = true;
                        }
                        else
                        {
                            overflow = false;
                        }

                        break;
                    }
            }

            ApplyOverflowState(child, overflow);

            if (!overflow)
            {
                if (primaryCount > 0)
                {
                    primaryWidth += spacing;
                    if (!double.IsInfinity(remaining))
                    {
                        remaining -= spacing;
                    }
                }

                primaryWidth += sizes[i].Width;
                if (!double.IsInfinity(remaining))
                {
                    remaining -= sizes[i].Width;
                }

                primaryCount++;
            }
        }

        toolBar.SetHasOverflowItems(hasAlways || hasAsNeededOverflow);

        double width = double.IsInfinity(constraint.Width) ? primaryWidth : Math.Min(primaryWidth, constraint.Width);
        double height = double.IsInfinity(constraint.Height) ? maxHeight : Math.Min(maxHeight, constraint.Height);
        return new Size(Math.Max(0, width), Math.Max(0, height));
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var toolBar = ToolBar;
        if (toolBar is null)
        {
            return ArrangeAsStack(finalSize, spacing: 0);
        }

        double spacing = Math.Max(0, toolBar.ItemSpacing);
        var generator = toolBar.ItemContainerGenerator;
        double x = 0;
        double height = finalSize.Height;
        bool first = true;

        for (int i = 0; i < toolBar.Items.Count; i++)
        {
            if (generator.ContainerFromIndex(i) is not UIElement child)
            {
                continue;
            }

            if (ToolBar.GetIsOverflowItem(child))
            {
                continue;
            }

            if (!ReferenceEquals(VisualTreeHelper.GetParent(child), this))
            {
                continue;
            }

            if (!first)
            {
                x += spacing;
            }

            first = false;
            double w = child.DesiredSize.Width;
            child.Arrange(new Rect(x, 0, w, height));
            x += w;
        }

        return finalSize;
    }

    internal void DetachChild(UIElement child)
    {
        int index = Children.IndexOf(child);
        if (index >= 0)
        {
            Children.RemoveAt(index);
        }
    }

    internal void AttachChild(UIElement child)
    {
        if (Children.IndexOf(child) < 0)
        {
            Children.Add(child);
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

    private void ApplyOverflowState(UIElement child, bool overflow)
    {
        ToolBar.SetIsOverflowItem(child, overflow);

        var parent = VisualTreeHelper.GetParent(child);

        if (overflow)
        {
            if (ReferenceEquals(parent, this))
            {
                DetachChild(child);
            }
        }
        else
        {
            if (parent is ToolBarOverflowPanel overflowPanel)
            {
                overflowPanel.DetachChild(child);
                parent = null;
            }
            else if (parent is Panel otherPanel && !ReferenceEquals(otherPanel, this))
            {
                otherPanel.Children.Remove(child);
                parent = null;
            }

            if (parent is null)
            {
                AttachChild(child);
            }
        }
    }

    private static ToolBarOverflowMode ResolveOverflowMode(ToolBar toolBar, int index, UIElement container)
    {
        if (toolBar.Items[index] is DependencyObject item && !ReferenceEquals(item, container))
        {
            return ToolBar.GetOverflowMode(item);
        }

        return ToolBar.GetOverflowMode(container);
    }

    private Size MeasureAsStack(Size constraint, double spacing)
    {
        double width = 0;
        double height = 0;
        int index = 0;
        var infinite = new Size(double.PositiveInfinity, constraint.Height);
        foreach (UIElement child in Children)
        {
            child.Measure(infinite);
            if (index > 0)
            {
                width += spacing;
            }

            width += child.DesiredSize.Width;
            height = Math.Max(height, child.DesiredSize.Height);
            index++;
        }

        return new Size(width, height);
    }

    private Size ArrangeAsStack(Size finalSize, double spacing)
    {
        double x = 0;
        bool first = true;
        foreach (UIElement child in Children)
        {
            if (!first)
            {
                x += spacing;
            }

            first = false;
            double w = child.DesiredSize.Width;
            child.Arrange(new Rect(x, 0, w, finalSize.Height));
            x += w;
        }

        return finalSize;
    }
}
