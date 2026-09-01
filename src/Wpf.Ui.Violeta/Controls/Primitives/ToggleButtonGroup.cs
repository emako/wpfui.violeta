using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls.Primitives;
using ToggleComboBox = Wpf.Ui.Violeta.Controls.ToggleComboBox;

namespace Wpf.Ui.Violeta.Controls.Primitives;

/// <summary>
/// Represents a group of toggleable controls where only one can be checked at a time
/// (similar to radio button behavior). Supports <see cref="ToggleButton"/> and
/// <see cref="ToggleComboBox"/>.
/// </summary>
public class ToggleButtonGroup : List<FrameworkElement>
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
    /// a <see cref="ToggleButton"/> or <see cref="ToggleComboBox"/> control.
    /// </summary>
    public static readonly DependencyProperty GroupProperty =
        DependencyProperty.RegisterAttached("Group", typeof(ToggleButtonGroup), typeof(ToggleButtonGroup), new PropertyMetadata(null!, OnGroupChanged));

    /// <summary>
    /// Called when the attached Group property changes. When a group is attached, the
    /// control is joined into the group.
    /// </summary>
    private static void OnGroupChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is not ToggleButtonGroup group)
        {
            return;
        }

        switch (d)
        {
            case ToggleButton toggleButton:
                group.Join(toggleButton);
                break;
            case ToggleComboBox toggleComboBox:
                group.Join(toggleComboBox);
                break;
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
    /// Fluent helper to join a toggle combo box to the group and return the group instance.
    /// </summary>
    /// <param name="toggleComboBox">Toggle combo box to join.</param>
    /// <returns>The current <see cref="ToggleButtonGroup"/> instance.</returns>
    public ToggleButtonGroup JoinWith(ToggleComboBox toggleComboBox)
    {
        Join(toggleComboBox);
        return this;
    }

    /// <summary>
    /// Adds the specified toggle button to the group and wires up Checked/Unchecked handlers
    /// to enforce single-selection behavior.
    /// </summary>
    /// <param name="toggleButton">Toggle button to add to the group.</param>
    public void Join(ToggleButton toggleButton) => JoinCore(toggleButton);

    /// <summary>
    /// Adds the specified toggle combo box to the group and wires up Checked/Unchecked handlers
    /// to enforce single-selection behavior.
    /// </summary>
    /// <param name="toggleComboBox">Toggle combo box to add to the group.</param>
    public void Join(ToggleComboBox toggleComboBox) => JoinCore(toggleComboBox);

    private void JoinCore(FrameworkElement element)
    {
        // Already joined (e.g. Join -> SetGroup -> OnGroupChanged -> Join).
        if (Contains(element))
        {
            return;
        }

        Add(element);
        AttachHandlers(element);

        // Store the group so other code can retrieve it via GetGroup.
        // Skip if already attached to avoid re-entrancy through OnGroupChanged.
        if (!ReferenceEquals(GetGroup(element), this))
        {
            SetGroup(element, this);
        }
    }

    private void AttachHandlers(FrameworkElement element)
    {
        switch (element)
        {
            case ToggleButton toggleButton:
                toggleButton.Checked += OnMemberChecked;
                toggleButton.Unchecked += OnMemberUnchecked;
                break;
            case ToggleComboBox toggleComboBox:
                toggleComboBox.Checked += OnMemberChecked;
                toggleComboBox.Unchecked += OnMemberUnchecked;
                break;
        }
    }

    private void OnMemberChecked(object sender, RoutedEventArgs e)
    {
        if (Handling || sender is not FrameworkElement current || GetGroup(current) is not ToggleButtonGroup group)
        {
            return;
        }

        Handling = true;
        try
        {
            foreach (FrameworkElement member in group)
            {
                if (!ReferenceEquals(member, current))
                {
                    SetIsChecked(member, false);
                }
            }
        }
        finally
        {
            Handling = false;
        }
    }

    private void OnMemberUnchecked(object sender, RoutedEventArgs e)
    {
        if (IsCanCancel || Handling || sender is not FrameworkElement current)
        {
            return;
        }

        Handling = true;
        try
        {
            // revert the uncheck to keep one item selected
            SetIsChecked(current, true);
        }
        finally
        {
            Handling = false;
        }
    }

    private static void SetIsChecked(FrameworkElement element, bool? value)
    {
        switch (element)
        {
            case ToggleButton toggleButton:
                toggleButton.IsChecked = value;
                break;
            case ToggleComboBox toggleComboBox:
                toggleComboBox.IsChecked = value;
                break;
        }
    }

    /// <summary>
    /// Removes the toggle button from the group and clears the attached group property.
    /// </summary>
    /// <param name="toggleButton">Toggle button to remove.</param>
    public void Unjoin(ToggleButton toggleButton) => UnjoinCore(toggleButton);

    /// <summary>
    /// Removes the toggle combo box from the group and clears the attached group property.
    /// </summary>
    /// <param name="toggleComboBox">Toggle combo box to remove.</param>
    public void Unjoin(ToggleComboBox toggleComboBox) => UnjoinCore(toggleComboBox);

    private void UnjoinCore(FrameworkElement element)
    {
        Remove(element);
        SetGroup(element, null!);
    }

    /// <summary>
    /// Factory helper to create a new <see cref="ToggleButtonGroup"/> instance.
    /// </summary>
    /// <returns>A new <see cref="ToggleButtonGroup"/>.</returns>
    public static ToggleButtonGroup New() => [];
}
