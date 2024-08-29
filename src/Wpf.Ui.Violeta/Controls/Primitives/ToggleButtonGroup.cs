using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace Wpf.Ui.Violeta.Controls.Primitives;

public class ToggleButtonGroup : List<ToggleButton>
{
    public static ToggleButtonGroup GetGroup(DependencyObject obj)
    {
        return (ToggleButtonGroup)obj.GetValue(GroupProperty);
    }

    public static void SetGroup(DependencyObject obj, ToggleButtonGroup value)
    {
        obj.SetValue(GroupProperty, value);
    }

    public static readonly DependencyProperty GroupProperty =
        DependencyProperty.RegisterAttached("Group", typeof(ToggleButtonGroup), typeof(ToggleButtonGroup), new PropertyMetadata(null!, OnGroupChanged));

    private static void OnGroupChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ToggleButton tb)
        {
            ((ToggleButtonGroup)e.NewValue).Join(tb);
        }
    }

    protected bool Handling { get; set; } = false;

    public ToggleButtonGroup JoinWith(ToggleButton toggleButton)
    {
        Join(toggleButton);
        return this;
    }

    public void Join(ToggleButton toggleButton)
    {
        Add(toggleButton);

        toggleButton.Checked += (s, e) =>
        {
            if (s is ToggleButton cb && GetGroup(cb) is ToggleButtonGroup group)
            {
                Handling = true;
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
        toggleButton.Unchecked += (s, e) =>
        {
            if (!Handling && s is ToggleButton tb)
            {
                tb.IsChecked = true;
            }
        };
        SetGroup(toggleButton, this);
    }

    public void Unjoin(ToggleButton checkBox)
    {
        Remove(checkBox);
        SetGroup(checkBox, null!);
    }

    public static ToggleButtonGroup New() => [];
}
