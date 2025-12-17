using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace Wpf.Ui.Violeta.Controls.Primitives;

/// <summary>
/// Represents a group of <see cref="ToggleButton"/> controls where only one
/// button can be checked at a time (similar to radio button behavior).
/// </summary>
public class ToggleButtonGroup : List<ToggleButton>
{
    /// <summary>
    /// Gets or sets a value that indicates whether the selected toggle can be canceled
    /// (i.e. allow all buttons to be unchecked). Default is <c>false</c>.
    /// </summary>
    public bool IsCanCancel { get; set; } = false;

    /// <summary>
    /// Gets the attached <see cref="ToggleButtonGroup"/> from a dependency object.
    /// </summary>
    /// <param name="obj">Dependency object that may hold the group.</param>
    /// <returns>The attached <see cref="ToggleButtonGroup"/> instance.</returns>
    public static ToggleButtonGroup GetGroup(DependencyObject obj)
    {
        return (ToggleButtonGroup)obj.GetValue(GroupProperty);
    }

    /// <summary>
    /// Attaches a <see cref="ToggleButtonGroup"/> to a dependency object.
    /// </summary>
    /// <param name="obj">Dependency object to attach the group to.</param>
    /// <param name="value">Group instance to attach.</param>
    public static void SetGroup(DependencyObject obj, ToggleButtonGroup value)
    {
        obj.SetValue(GroupProperty, value);
    }

    /// <summary>
    /// Attached dependency property used to associate a <see cref="ToggleButtonGroup"/> with
    /// a <see cref="ToggleButton"/> control.
    /// </summary>
    public static readonly DependencyProperty GroupProperty =
        DependencyProperty.RegisterAttached("Group", typeof(ToggleButtonGroup), typeof(ToggleButtonGroup), new PropertyMetadata(null!, OnGroupChanged));

    /// <summary>
    /// Called when the attached Group property changes. When a group is attached to a
    /// ToggleButton, the button is joined into the group.
    /// </summary>
    private static void OnGroupChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ToggleButton tb)
        {
            ((ToggleButtonGroup)e.NewValue).Join(tb);
        }
    }

    /// <summary>
    /// Internal flag used to avoid re-entrancy while updating button states.
    /// </summary>
    protected bool Handling { get; set; } = false;

    /// <summary>
    /// Fluent helper to join a toggle button to the group and return the group instance.
    /// </summary>
    /// <param name="toggleButton">Toggle button to join.</param>
    /// <returns>The current <see cref="ToggleButtonGroup"/> instance.</returns>
    public ToggleButtonGroup JoinWith(ToggleButton toggleButton)
    {
        Join(toggleButton);
        return this;
    }

    /// <summary>
    /// Adds the specified toggle button to the group and wires up Checked/Unchecked handlers
    /// to enforce single-selection behavior.
    /// </summary>
    /// <param name="toggleButton">Toggle button to add to the group.</param>
    public void Join(ToggleButton toggleButton)
    {
        Add(toggleButton);

        // When a button is checked, uncheck other buttons in the same group.
        toggleButton.Checked += (s, e) =>
        {
            if (s is ToggleButton cb && GetGroup(cb) is ToggleButtonGroup group)
            {
                Handling = true; // prevent Unchecked handler from fighting back
                foreach (ToggleButton tb in group)
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
        toggleButton.Unchecked += (s, e) =>
        {
            if (!IsCanCancel && !Handling && s is ToggleButton cb)
            {
                // revert the uncheck to keep one item selected
                cb.IsChecked = true;
            }
        };

        // Store the group on the toggle so other code can retrieve it via GetGroup.
        SetGroup(toggleButton, this);
    }

    /// <summary>
    /// Removes the toggle button from the group and clears the attached group property.
    /// </summary>
    /// <param name="checkBox">Toggle button to remove.</param>
    public void Unjoin(ToggleButton checkBox)
    {
        Remove(checkBox);
        SetGroup(checkBox, null!);
    }

    /// <summary>
    /// Factory helper to create a new <see cref="ToggleButtonGroup"/> instance.
    /// </summary>
    /// <returns>A new <see cref="ToggleButtonGroup"/>.</returns>
    public static ToggleButtonGroup New() => [];
}
