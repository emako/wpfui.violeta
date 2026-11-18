using System.Windows;

namespace Wpf.Ui.Controls;

/// <summary>
/// <see cref="System.Windows.Controls.ComboBox"/> with <see cref="PlaceholderText"/> and <see cref="MaxLength"/>.
/// The default style matches the library ComboBox chrome. <see cref="PlaceholderTextProperty"/>
/// reuses <see cref="TextBox.PlaceholderTextProperty"/> via <see cref="DependencyProperty.AddOwner(System.Type)"/>,
/// the same property <see cref="ControlHelper.PlaceholderTextProperty"/> reads.
/// <see cref="MaxLengthProperty"/> likewise reuses <see cref="System.Windows.Controls.TextBox.MaxLengthProperty"/>
/// (also <see cref="ControlHelper.MaxLengthProperty"/>). <c>0</c> means no limit.
/// The editable text box in the control template reads that value.
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

    /// <summary>
    /// Identifies the <see cref="MaxLength"/> dependency property.
    /// Same store as <see cref="System.Windows.Controls.TextBox.MaxLengthProperty"/>.
    /// </summary>
    public static readonly DependencyProperty MaxLengthProperty =
        System.Windows.Controls.TextBox.MaxLengthProperty.AddOwner(typeof(ComboBox));

    /// <summary>
    /// Gets or sets the maximum number of characters that can be typed when <see cref="System.Windows.Controls.ComboBox.IsEditable"/> is true.
    /// <c>0</c> means no limit. This does not trim an existing selection.
    /// </summary>
    public int MaxLength
    {
        get => (int)GetValue(MaxLengthProperty);
        set => SetValue(MaxLengthProperty, value);
    }
}
