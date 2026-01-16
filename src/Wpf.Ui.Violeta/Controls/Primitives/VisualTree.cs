using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Controls.Primitives;

public static class VisualTree
{
    public static FrameworkElement? FindDescendantByName(this DependencyObject element, string name)
    {
        if (element == null || string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        if (name.Equals((element as FrameworkElement)?.Name, StringComparison.OrdinalIgnoreCase))
        {
            return element as FrameworkElement;
        }

        int childrenCount = VisualTreeHelper.GetChildrenCount(element);
        for (int i = 0; i < childrenCount; i++)
        {
            FrameworkElement? frameworkElement = VisualTreeHelper.GetChild(element, i).FindDescendantByName(name);
            if (frameworkElement != null)
            {
                return frameworkElement;
            }
        }

        return null;
    }

    public static T? FindDescendant<T>(this DependencyObject element) where T : DependencyObject
    {
        T? val = null;
        int childrenCount = VisualTreeHelper.GetChildrenCount(element);
        for (int i = 0; i < childrenCount; i++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(element, i);
            if (child is T val2)
            {
                val = val2;
                break;
            }

            val = child.FindDescendant<T>();
            if (val != null)
            {
                break;
            }
        }

        return val;
    }

    public static object? FindDescendant(this DependencyObject element, Type type)
    {
        object? obj = null;
        int childrenCount = VisualTreeHelper.GetChildrenCount(element);
        for (int i = 0; i < childrenCount; i++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(element, i);
            if (child.GetType() == type)
            {
                obj = child;
                break;
            }

            obj = child.FindDescendant(type);
            if (obj != null)
            {
                break;
            }
        }

        return obj;
    }

    public static IEnumerable<T> FindDescendants<T>(this DependencyObject element) where T : DependencyObject
    {
        int childrenCount = VisualTreeHelper.GetChildrenCount(element);
        for (int i = 0; i < childrenCount; i++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(element, i);
            if (child is T val)
            {
                yield return val;
            }

            foreach (T item in child.FindDescendants<T>())
            {
                yield return item;
            }
        }
    }

    public static FrameworkElement? FindAscendantByName(this DependencyObject element, string name)
    {
        if (element == null || string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        DependencyObject parent = VisualTreeHelper.GetParent(element);
        if (parent == null)
        {
            return null;
        }

        if (name.Equals((parent as FrameworkElement)?.Name, StringComparison.OrdinalIgnoreCase))
        {
            return parent as FrameworkElement;
        }

        return parent.FindAscendantByName(name);
    }

    public static T? FindAscendant<T>(this DependencyObject element) where T : DependencyObject
    {
        DependencyObject parent = VisualTreeHelper.GetParent(element);
        if (parent == null)
        {
            return null;
        }

        if (parent is T)
        {
            return parent as T;
        }

        return parent.FindAscendant<T>();
    }

    public static object? FindAscendant(this DependencyObject element, Type type)
    {
        DependencyObject parent = VisualTreeHelper.GetParent(element);
        if (parent == null)
        {
            return null;
        }

        if (parent.GetType() == type)
        {
            return parent;
        }

        return parent.FindAscendant(type);
    }

    public static IEnumerable<DependencyObject> FindAscendants(this DependencyObject element)
    {
        for (DependencyObject parent = VisualTreeHelper.GetParent(element); parent != null; parent = VisualTreeHelper.GetParent(parent))
        {
            yield return parent;
        }
    }

    public static bool DetachFromParent(this FrameworkElement element, DependencyObject parent)
    {
        try
        {
            if (parent is Panel panel)
            {
                panel.Children.Remove(element);
                return true;
            }

            if (parent is Decorator decorator)
            {
                if (decorator.Child == element)
                {
                    decorator.Child = null;
                    return true;
                }
            }
            else
            {
                if (parent is ContentControl contentControl)
                {
                    contentControl.Content = null;
                    return true;
                }

                if (parent is ContentPresenter contentPresenter)
                {
                    contentPresenter.Content = null;
                    return true;
                }

                if (parent is Popup popup)
                {
                    popup.Child = null;
                    return true;
                }

                if (parent is ItemsControl itemsControl)
                {
                    if (itemsControl.Items.Contains(element))
                    {
                        itemsControl.Items.Remove(element);
                        return true;
                    }

                    if (itemsControl.Items.Contains(element.DataContext))
                    {
                        itemsControl.Items.Remove(element.DataContext);
                        return true;
                    }
                }
                else
                {
                    Type type = parent.GetType();
                    BindingFlags bindingAttr = BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public;
                    List<PropertyInfo> obj =
                    [
                        type.GetProperty("Children", bindingAttr)!,
                        type.GetProperty("Child", bindingAttr)!,
                        type.GetProperty("Content", bindingAttr)!,
                        type.GetProperty("Items", bindingAttr)!
                    ];
                    bool flag = false;
                    foreach (PropertyInfo item in obj)
                    {
                        switch (item.Name.ToLower())
                        {
                            case "children":
                                {
                                    object? value = item.GetValue(parent, null);
                                    MethodInfo[] methods = value?.GetType().GetMethods() ?? [];
                                    foreach (MethodInfo methodInfo in methods)
                                    {
                                        if (methodInfo.Name.ToLower() == "remove")
                                        {
                                            methodInfo.Invoke(value, [element]);
                                            flag = true;
                                        }
                                    }

                                    break;
                                }
                            case "child":
                            case "content":
                                if (item.GetValue(parent) == element)
                                {
                                    item.SetValue(parent, null);
                                    flag = true;
                                }

                                break;
                        }
                    }

                    if (flag)
                    {
                        return true;
                    }
                }
            }
        }
        catch
        {
            if (Debugger.IsAttached)
            {
                throw;
            }
        }

        return false;
    }

    public static bool DetachFromLogicalParent(this FrameworkElement element)
    {
        DependencyObject parent = element.Parent;
        element.DetachFromParent(parent);
        return element.Parent != parent;
    }

    public static bool DetachFromVisualParent(this FrameworkElement element)
    {
        DependencyObject parent = VisualTreeHelper.GetParent(element);
        element.DetachFromParent(parent);
        return VisualTreeHelper.GetParent(element) != parent;
    }
}
