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
            return MeasureAsStack(constraint);
        }

        toolBar.EnsureContainersRealized();

        var generator = toolBar.ItemContainerGenerator;
        int count = toolBar.Items.Count;
        if (count == 0)
        {
            toolBar.SetHasOverflowItems(false);
            return new Size(0, 0);
        }

        var infinite = new Size(double.PositiveInfinity, constraint.Height);

        double neverWidth = 0;
        double maxHeight = 0;
        bool hasAlways = false;
        var modes = new OverflowMode[count];
        var sizes = new Size[count];

        for (int i = 0; i < count; i++)
        {
            if (generator.ContainerFromIndex(i) is not UIElement child)
            {
                modes[i] = OverflowMode.AsNeeded;
                sizes[i] = new Size();
                continue;
            }

            modes[i] = ResolveOverflowMode(toolBar, i, child);
            child.Measure(infinite);
            sizes[i] = child.DesiredSize;
            maxHeight = Math.Max(maxHeight, sizes[i].Height);

            if (modes[i] == OverflowMode.Always)
            {
                hasAlways = true;
            }
            else if (modes[i] == OverflowMode.Never)
            {
                neverWidth += sizes[i].Width;
            }
        }

        double available = double.IsInfinity(constraint.Width) ? double.PositiveInfinity : constraint.Width;
        double remaining = Math.Max(0, available - neverWidth);

        bool sendToOverflow = false;
        bool hasAsNeededOverflow = false;
        double primaryWidth = neverWidth;

        for (int i = 0; i < count; i++)
        {
            if (generator.ContainerFromIndex(i) is not UIElement child)
            {
                continue;
            }

            bool overflow;
            switch (modes[i])
            {
                case OverflowMode.Always:
                    overflow = true;
                    break;

                case OverflowMode.Never:
                    overflow = false;
                    break;

                default:
                    if (sendToOverflow)
                    {
                        overflow = true;
                    }
                    else if (!double.IsInfinity(remaining) && sizes[i].Width > remaining)
                    {
                        overflow = true;
                        sendToOverflow = true;
                    }
                    else
                    {
                        overflow = false;
                        if (!double.IsInfinity(remaining))
                        {
                            remaining -= sizes[i].Width;
                        }

                        primaryWidth += sizes[i].Width;
                    }

                    if (overflow)
                    {
                        hasAsNeededOverflow = true;
                    }

                    break;
            }

            ApplyOverflowState(child, overflow);
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
            return ArrangeAsStack(finalSize);
        }

        var generator = toolBar.ItemContainerGenerator;
        double x = 0;
        double height = finalSize.Height;

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

    private static OverflowMode ResolveOverflowMode(ToolBar toolBar, int index, UIElement container)
    {
        if (toolBar.Items[index] is DependencyObject item && !ReferenceEquals(item, container))
        {
            return ToolBar.GetOverflowMode(item);
        }

        return ToolBar.GetOverflowMode(container);
    }

    private Size MeasureAsStack(Size constraint)
    {
        double width = 0;
        double height = 0;
        var infinite = new Size(double.PositiveInfinity, constraint.Height);
        foreach (UIElement child in Children)
        {
            child.Measure(infinite);
            width += child.DesiredSize.Width;
            height = Math.Max(height, child.DesiredSize.Height);
        }

        return new Size(width, height);
    }

    private Size ArrangeAsStack(Size finalSize)
    {
        double x = 0;
        foreach (UIElement child in Children)
        {
            double w = child.DesiredSize.Width;
            child.Arrange(new Rect(x, 0, w, finalSize.Height));
            x += w;
        }

        return finalSize;
    }
}
