using System.Windows;

namespace Wpf.Ui.Controls;

/// <summary>
/// Attached properties that extend the stock WPF <see cref="System.Windows.Controls.ComboBox"/>.
/// </summary>
/// <remarks>
/// <see cref="PlaceholderTextProperty"/> reuses <see cref="TextBox.PlaceholderTextProperty"/> via
/// <see cref="DependencyProperty.AddOwner(System.Type)"/>, so values are stored on the same DP.
/// Prefer <c>ui:ComboBoxHelper.PlaceholderText</c> on a ComboBox — <c>ui:TextBox.PlaceholderText</c>
/// is not an attached property and cannot be set on ComboBox in XAML.
/// </remarks>
public static class ComboBoxHelper
{
    /// <summary>
    /// Identifies the PlaceholderText attached property.
    /// </summary>
    public static readonly DependencyProperty PlaceholderTextProperty =
        TextBox.PlaceholderTextProperty.AddOwner(typeof(ComboBoxHelper));

    /// <summary>
    /// Gets the placeholder text displayed when the ComboBox has no selected item
    /// (and, when editable, when the text is empty).
    /// </summary>
    public static string GetPlaceholderText(DependencyObject element) =>
        (string)element.GetValue(PlaceholderTextProperty);

    /// <summary>
    /// Sets the placeholder text displayed when the ComboBox has no selected item
    /// (and, when editable, when the text is empty).
    /// </summary>
    public static void SetPlaceholderText(DependencyObject element, string value) =>
        element.SetValue(PlaceholderTextProperty, value);
}
