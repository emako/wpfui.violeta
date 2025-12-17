using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Wpf.Ui.Violeta.Controls.Primitives;

/// <summary>
/// Represents a group of <see cref="CheckBox"/> controls where only one
/// button can be checked at a time (similar to radio button behavior).
/// </summary>
public class CheckBoxGroup : List<CheckBox>
{
    /// <summary>
    /// Gets or sets a value that indicates whether the selected toggle can be canceled
    /// (i.e. allow all buttons to be unchecked). Default is <c>false</c>.
    /// </summary>
    public bool IsCanCancel { get; set; } = false;

    /// <summary>
    /// Gets the attached <see cref="CheckBoxGroup"/> from a dependency object.
    /// </summary>
    /// <param name="obj">Dependency object that may hold the group.</param>
    /// <returns>The attached <see cref="CheckBoxGroup"/> instance.</returns>
    public static CheckBoxGroup GetGroup(DependencyObject obj)
    {
        return (CheckBoxGroup)obj.GetValue(GroupProperty);
    }

    /// <summary>
    /// Attaches a <see cref="CheckBoxGroup"/> to a dependency object.
    /// </summary>
    /// <param name="obj">Dependency object to attach the group to.</param>
    /// <param name="value">Group instance to attach.</param>
    public static void SetGroup(DependencyObject obj, CheckBoxGroup value)
    {
        obj.SetValue(GroupProperty, value);
    }

    /// <summary>
    /// Attached dependency property used to associate a <see cref="CheckBoxGroup"/> with
    /// a <see cref="CheckBox"/> control.
    /// </summary>
    public static readonly DependencyProperty GroupProperty =
        DependencyProperty.RegisterAttached("Group", typeof(CheckBoxGroup), typeof(CheckBoxGroup), new PropertyMetadata(null!, OnGroupChanged));

    /// <summary>
    /// Called when the attached Group property changes. When a group is attached to a
    /// CheckBox, the box is joined into the group.
    /// </summary>
    private static void OnGroupChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CheckBox cb)
        {
            ((CheckBoxGroup)e.NewValue).Join(cb);
        }
    }

    /// <summary>
    /// Internal flag used to avoid re-entrancy while updating button states.
    /// </summary>
    protected bool Handling { get; set; } = false;

    /// <summary>
    /// Fluent helper to join a toggle button to the group and return the group instance.
    /// </summary>
    /// <param name="checkBox">Toggle box to join.</param>
    /// <returns>The current <see cref="CheckBoxGroup"/> instance.</returns>
    public CheckBoxGroup JoinWith(CheckBox checkBox)
    {
        Join(checkBox);
        return this;
    }

    /// <summary>
    /// Adds the specified toggle button to the group and wires up Checked/Unchecked handlers
    /// to enforce single-selection behavior.
    /// </summary>
    /// <param name="checkBox">Toggle button to add to the group.</param>
    public void Join(CheckBox checkBox)
    {
        Add(checkBox);

        // When a button is checked, uncheck other buttons in the same group.
        checkBox.Checked += (s, e) =>
        {
            if (s is CheckBox cb && GetGroup(cb) is CheckBoxGroup group)
            {
                Handling = true; // prevent Unchecked handler from fighting back
                foreach (CheckBox tb in group)
                {
                    if (tb != cb)
                    {
                        tb.IsChecked = false;
                    }
                }
                Handling = false;
            }
        };

        // When a button is unchecked, prevent leaving the group with no selection
        // unless IsCanCancel is true.
        checkBox.Unchecked += (s, e) =>
        {
            if (!IsCanCancel && !Handling && s is CheckBox cb)
            {
                // revert the uncheck to keep one item selected
                cb.IsChecked = true;
            }
        };

        // Store the group on the toggle so other code can retrieve it via GetGroup.
        SetGroup(checkBox, this);
    }

    /// <summary>
    /// Removes the toggle button from the group and clears the attached group property.
    /// </summary>
    /// <param name="checkBox">Toggle button to remove.</param>
    public void Unjoin(CheckBox checkBox)
    {
        Remove(checkBox);
        SetGroup(checkBox, null!);
    }

    /// <summary>
    /// Factory helper to create a new <see cref="CheckBoxGroup"/> instance.
    /// </summary>
    /// <returns>A new <see cref="CheckBoxGroup"/>.</returns>
    public static CheckBoxGroup New() => [];
}
