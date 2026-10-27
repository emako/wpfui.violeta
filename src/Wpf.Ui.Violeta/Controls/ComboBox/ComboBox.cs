using System.Windows;
using System.Windows.Controls;

namespace Wpf.Ui.Controls;

/// <summary>
/// <see cref="System.Windows.Controls.ComboBox"/> with <see cref="PlaceholderText"/>.
/// The default style matches the library ComboBox chrome. <see cref="PlaceholderTextProperty"/>
/// reuses <see cref="TextBox.PlaceholderTextProperty"/> via <see cref="DependencyProperty.AddOwner(System.Type)"/>,
/// the same property <see cref="ControlHelper.PlaceholderTextProperty"/> reads.
/// </summary>
public class ComboBox : System.Windows.Controls.ComboBox
{
    static ComboBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ComboBox),
            new FrameworkPropertyMetadata(typeof(ComboBox)));
    }

    /// <summary>Identifies the <see cref="PlaceholderText"/> dependency property.</summary>
    public static readonly DependencyProperty PlaceholderTextProperty =
        TextBox.PlaceholderTextProperty.AddOwner(typeof(ComboBox));

    /// <summary>
    /// Gets or sets the placeholder shown when nothing is selected.
    /// </summary>
    public string PlaceholderText
    {
        get => (string)GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }
}
