using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Wpf.Ui.Violeta.Controls.Primitives;

public class RadioButtonGroup : List<RadioButton>
{
    public static RadioButtonGroup GetGroup(DependencyObject obj)
    {
        return (RadioButtonGroup)obj.GetValue(GroupProperty);
    }

    public static void SetGroup(DependencyObject obj, RadioButtonGroup value)
    {
        obj.SetValue(GroupProperty, value);
    }

    public static readonly DependencyProperty GroupProperty =
        DependencyProperty.RegisterAttached("Group", typeof(RadioButtonGroup), typeof(RadioButtonGroup), new PropertyMetadata(null!, OnGroupChanged));

    private static void OnGroupChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is RadioButton tb)
        {
            ((RadioButtonGroup)e.NewValue).Join(tb);
        }
    }

    protected bool Handling { get; set; } = false;

    public RadioButtonGroup JoinWith(RadioButton radioButton)
    {
        Join(radioButton);
        return this;
    }

    public void Join(RadioButton radioButton)
    {
        Add(radioButton);

        radioButton.Checked += (s, e) =>
        {
            if (s is RadioButton cb && GetGroup(cb) is RadioButtonGroup group)
            {
                Handling = true;
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
        radioButton.Unchecked += (s, e) =>
        {
            if (!Handling && s is RadioButton tb)
            {
                tb.IsChecked = true;
            }
        };
        SetGroup(radioButton, this);
    }

    public void Unjoin(RadioButton checkBox)
    {
        Remove(checkBox);
        SetGroup(checkBox, null!);
    }

    public static RadioButtonGroup New() => [];
}
