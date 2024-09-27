using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Wpf.Ui.Violeta.Controls.Primitives;

public class MenuItemGroup : List<MenuItem>
{
    public bool IsCanCancel { get; set; } = false;

    public static MenuItemGroup GetGroup(DependencyObject obj)
    {
        return (MenuItemGroup)obj.GetValue(GroupProperty);
    }

    public static void SetGroup(DependencyObject obj, MenuItemGroup value)
    {
        obj.SetValue(GroupProperty, value);
    }

    public static readonly DependencyProperty GroupProperty =
        DependencyProperty.RegisterAttached("Group", typeof(MenuItemGroup), typeof(MenuItemGroup), new PropertyMetadata(null!, OnGroupChanged));

    private static void OnGroupChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MenuItem tb)
        {
            ((MenuItemGroup)e.NewValue).Join(tb);
        }
    }

    protected bool Handling { get; set; } = false;

    public MenuItemGroup JoinWith(MenuItem menuItem)
    {
        Join(menuItem);
        return this;
    }

    public void Join(MenuItem menuItem)
    {
        Add(menuItem);

        menuItem.Checked += (s, e) =>
        {
            if (s is MenuItem mi && GetGroup(mi) is MenuItemGroup group)
            {
                Handling = true;
                foreach (MenuItem tb in group)
                {
                    if (tb != mi)
                    {
                        tb.IsChecked = false;
                    }
                }
                Handling = false;
            }
        };
        menuItem.Unchecked += (s, e) =>
        {
            if (!IsCanCancel && !Handling && s is MenuItem mi)
            {
                mi.IsChecked = true;
            }
        };
        SetGroup(menuItem, this);
    }

    public void Unjoin(MenuItem checkBox)
    {
        Remove(checkBox);
        SetGroup(checkBox, null!);
    }

    public static MenuItemGroup New() => [];
}
