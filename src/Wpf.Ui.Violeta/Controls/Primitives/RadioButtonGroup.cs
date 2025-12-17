using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Wpf.Ui.Violeta.Controls.Primitives;

/// <summary>
/// Represents a group of <see cref="RadioButton"/> controls that enforces
/// single-selection behavior among the grouped radio buttons.
/// </summary>
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
        Add(radioButton);

        // When a radio button is checked, uncheck other buttons in the same group.
        radioButton.Checked += (s, e) =>
        {
            if (s is RadioButton cb && GetGroup(cb) is RadioButtonGroup group)
            {
                Handling = true; // prevent Unchecked handler from fighting back
                foreach (RadioButton tb in group)
                {
                    if (tb != cb)
                    {
                        tb.IsChecked = false;
                    }
                }
                Handling = false;
            }
        };

        // When a radio button is unchecked, revert the action unless it was caused
        // by internal handling. This keeps one item always selected in the group.
        radioButton.Unchecked += (s, e) =>
        {
            if (!Handling && s is RadioButton tb)
            {
                tb.IsChecked = true;
            }
        };

        // Store the group on the radio button so other code can retrieve it via GetGroup.
        SetGroup(radioButton, this);
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
