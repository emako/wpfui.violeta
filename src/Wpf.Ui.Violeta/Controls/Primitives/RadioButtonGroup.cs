using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Wpf.Ui.Violeta.Controls.Primitives;

/// <summary>
/// Represents a group of <see cref="RadioButton"/> controls that enforces
/// single-selection behavior across containers (where WPF's built-in grouping is insufficient).
/// </summary>
/// <remarks>
/// <para>
/// WPF <see cref="RadioButton"/> already performs mutual exclusion on its own:
/// when one is checked, siblings that share the same <see cref="RadioButton.GroupName"/>
/// (or the same parent when <see cref="RadioButton.GroupName"/> is empty) are unchecked
/// by the framework via its internal radio-group update logic.
/// </para>
/// <para>
/// This class layers cross-container grouping on top of that behavior. Handlers must
/// therefore cooperate with the built-in exclusion rather than fight it: never force
/// <c>IsChecked = true</c> on uncheck while another group member is already checked,
/// or the framework and this group will re-enter each other and cause a
/// <see cref="System.StackOverflowException"/>.
/// </para>
/// </remarks>
public class RadioButtonGroup : List<RadioButton>
{
    /// <summary>
    /// Gets the attached <see cref="RadioButtonGroup"/> from a dependency object.
    /// </summary>
    /// <param name="obj">Dependency object that may hold the group.</param>
    /// <returns>The attached <see cref="RadioButtonGroup"/> instance.</returns>
    public static RadioButtonGroup GetGroup(DependencyObject obj)
    {
        return (RadioButtonGroup)obj.GetValue(GroupProperty);
    }

    /// <summary>
    /// Attaches a <see cref="RadioButtonGroup"/> to a dependency object.
    /// </summary>
    /// <param name="obj">Dependency object to attach the group to.</param>
    /// <param name="value">Group instance to attach.</param>
    public static void SetGroup(DependencyObject obj, RadioButtonGroup value)
    {
        obj.SetValue(GroupProperty, value);
    }

    /// <summary>
    /// Attached dependency property used to associate a <see cref="RadioButtonGroup"/>
    /// with a <see cref="RadioButton"/> control.
    /// </summary>
    public static readonly DependencyProperty GroupProperty =
        DependencyProperty.RegisterAttached("Group", typeof(RadioButtonGroup), typeof(RadioButtonGroup), new PropertyMetadata(null!, OnGroupChanged));

    /// <summary>
    /// Called when the attached Group property changes. When a group is attached to a
    /// RadioButton, the button is joined into the group.
    /// </summary>
    private static void OnGroupChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is RadioButton tb)
        {
            ((RadioButtonGroup)e.NewValue).Join(tb);
        }
    }

    /// <summary>
    /// Internal flag used to avoid re-entrancy while updating button states.
    /// </summary>
    protected bool Handling { get; set; } = false;

    /// <summary>
    /// Fluent helper to join a radio button to the group and return the group instance.
    /// </summary>
    /// <param name="radioButton">Radio button to join.</param>
    /// <returns>The current <see cref="RadioButtonGroup"/> instance.</returns>
    public RadioButtonGroup JoinWith(RadioButton radioButton)
    {
        Join(radioButton);
        return this;
    }

    /// <summary>
    /// Adds the specified radio button to the group and wires up Checked/Unchecked handlers
    /// to enforce single-selection behavior.
    /// </summary>
    /// <param name="radioButton">Radio button to add to the group.</param>
    public void Join(RadioButton radioButton)
    {
        // Already joined (e.g. Join -> SetGroup -> OnGroupChanged -> Join).
        if (Contains(radioButton))
        {
            return;
        }

        Add(radioButton);

        // When a radio button is checked, uncheck other buttons in the same group.
        radioButton.Checked += (s, e) =>
        {
            if (Handling || s is not RadioButton cb || GetGroup(cb) is not RadioButtonGroup group)
            {
                return;
            }

            Handling = true;
            try
            {
                foreach (RadioButton tb in group)
                {
                    if (tb != cb && tb.IsChecked != false)
                    {
                        tb.IsChecked = false;
                    }
                }
            }
            finally
            {
                Handling = false;
            }
        };

        // When a radio button is unchecked, keep one item selected — but only if no other
        // group member is checked. Blindly forcing IsChecked=true re-enters RadioButton's
        // built-in sibling/GroupName mutual exclusion and causes StackOverflowException.
        radioButton.Unchecked += (s, e) =>
        {
            if (Handling || s is not RadioButton tb)
            {
                return;
            }

            foreach (RadioButton other in this)
            {
                if (other != tb && other.IsChecked == true)
                {
                    return;
                }
            }

            Handling = true;
            try
            {
                tb.IsChecked = true;
            }
            finally
            {
                Handling = false;
            }
        };

        // Store the group on the radio button so other code can retrieve it via GetGroup.
        // Skip if already attached to avoid re-entrancy through OnGroupChanged.
        if (!ReferenceEquals(GetGroup(radioButton), this))
        {
            SetGroup(radioButton, this);
        }
    }

    /// <summary>
    /// Removes the radio button from the group and clears the attached group property.
    /// </summary>
    /// <param name="checkBox">Radio button to remove.</param>
    public void Unjoin(RadioButton checkBox)
    {
        Remove(checkBox);
        SetGroup(checkBox, null!);
    }

    /// <summary>
    /// Factory helper to create a new <see cref="RadioButtonGroup"/> instance.
    /// </summary>
    /// <returns>A new <see cref="RadioButtonGroup"/>.</returns>
    public static RadioButtonGroup New() => [];
}
