using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Wpf.Ui.Violeta.Controls.Primitives;

/// <summary>
/// Represents a group of <see cref="MenuItem"/> controls where only one
/// item can be checked at a time (similar to radio button behavior).
/// </summary>
public class MenuItemGroup : List<MenuItem>
{
    /// <summary>
    /// Gets or sets a value that indicates whether the selected menu item can be canceled
    /// (i.e. allow all items to be unchecked). Default is <c>false</c>.
    /// </summary>
    public bool IsCanCancel { get; set; } = false;

    /// <summary>
    /// Gets the attached <see cref="MenuItemGroup"/> from a dependency object.
    /// </summary>
    /// <param name="obj">Dependency object that may hold the group.</param>
    /// <returns>The attached <see cref="MenuItemGroup"/> instance.</returns>
    public static MenuItemGroup GetGroup(DependencyObject obj)
    {
        return (MenuItemGroup)obj.GetValue(GroupProperty);
    }

    /// <summary>
    /// Attaches a <see cref="MenuItemGroup"/> to a dependency object.
    /// </summary>
    /// <param name="obj">Dependency object to attach the group to.</param>
    /// <param name="value">Group instance to attach.</param>
    public static void SetGroup(DependencyObject obj, MenuItemGroup value)
    {
        obj.SetValue(GroupProperty, value);
    }

    /// <summary>
    /// Attached dependency property used to associate a <see cref="MenuItemGroup"/>
    /// with a <see cref="MenuItem"/> control.
    /// </summary>
    public static readonly DependencyProperty GroupProperty =
        DependencyProperty.RegisterAttached("Group", typeof(MenuItemGroup), typeof(MenuItemGroup), new PropertyMetadata(null!, OnGroupChanged));

    /// <summary>
    /// Called when the attached Group property changes. When a group is attached to a
    /// MenuItem, the item is joined into the group.
    /// </summary>
    private static void OnGroupChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MenuItem tb)
        {
            ((MenuItemGroup)e.NewValue).Join(tb);
        }
    }

    /// <summary>
    /// Internal flag used to avoid re-entrancy while updating item states.
    /// </summary>
    protected bool Handling { get; set; } = false;

    /// <summary>
    /// Fluent helper to join a menu item to the group and return the group instance.
    /// </summary>
    /// <param name="menuItem">Menu item to join.</param>
    /// <returns>The current <see cref="MenuItemGroup"/> instance.</returns>
    public MenuItemGroup JoinWith(MenuItem menuItem)
    {
        Join(menuItem);
        return this;
    }

    /// <summary>
    /// Adds the specified menu item to the group and wires up Checked/Unchecked handlers
    /// to enforce single-selection behavior.
    /// </summary>
    /// <param name="menuItem">Menu item to add to the group.</param>
    public void Join(MenuItem menuItem)
    {
        Add(menuItem);

        // When an item is checked, uncheck other items in the same group.
        menuItem.Checked += (s, e) =>
        {
            if (s is MenuItem mi && GetGroup(mi) is MenuItemGroup group)
            {
                Handling = true; // prevent Unchecked handler from fighting back
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

        // When an item is unchecked, prevent leaving the group with no selection
        // unless IsCanCancel is true.
        menuItem.Unchecked += (s, e) =>
        {
            if (!IsCanCancel && !Handling && s is MenuItem mi)
            {
                // revert the uncheck to keep one item selected
                mi.IsChecked = true;
            }
        };

        // Store the group on the menu item so other code can retrieve it via GetGroup.
        SetGroup(menuItem, this);
    }

    /// <summary>
    /// Removes the menu item from the group and clears the attached group property.
    /// </summary>
    /// <param name="checkBox">Menu item to remove.</param>
    public void Unjoin(MenuItem checkBox)
    {
        Remove(checkBox);
        SetGroup(checkBox, null!);
    }

    /// <summary>
    /// Factory helper to create a new <see cref="MenuItemGroup"/> instance.
    /// </summary>
    /// <returns>A new <see cref="MenuItemGroup"/>.</returns>
    public static MenuItemGroup New() => [];
}
