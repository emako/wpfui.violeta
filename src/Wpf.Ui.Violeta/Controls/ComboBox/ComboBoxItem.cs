using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// ComboBox item with optional <see cref="InputGestureText"/> (menu-style shortcut hint).
/// </summary>
public class ComboBoxItem : System.Windows.Controls.ComboBoxItem
{
    static ComboBoxItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ComboBoxItem),
            new FrameworkPropertyMetadata(typeof(ComboBoxItem)));
    }

    /// <summary>
    /// Shortcut text shown on the trailing edge of the item (same role as <c>MenuItem.InputGestureText</c>).
    /// Registered as attached so default ComboBox / ToggleComboBox item templates can bind it.
    /// </summary>
    public static readonly DependencyProperty InputGestureTextProperty =
        DependencyProperty.RegisterAttached(
            "InputGestureText",
            typeof(string),
            typeof(ComboBoxItem),
            new FrameworkPropertyMetadata(
                string.Empty,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

    public static string GetInputGestureText(DependencyObject element)
    {
        return (string)element.GetValue(InputGestureTextProperty);
    }

    public static void SetInputGestureText(DependencyObject element, string value)
    {
        element.SetValue(InputGestureTextProperty, value);
    }

    [Bindable(true)]
    [Category("Content")]
    public string InputGestureText
    {
        get => (string)GetValue(InputGestureTextProperty);
        set => SetValue(InputGestureTextProperty, value);
    }
}
