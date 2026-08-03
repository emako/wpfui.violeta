using System.Windows;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Presents a color for user editing using a spectrum, palette and component sliders within a drop down.
/// Editing is available when the drop down flyout is opened; otherwise, only the preview content area is shown.
/// </summary>
public class ColorPicker : ColorView
{
    static ColorPicker()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ColorPicker),
            new FrameworkPropertyMetadata(typeof(ColorPicker)));
    }

    public static readonly DependencyProperty ContentProperty =
        DependencyProperty.Register(
            nameof(Content), typeof(object), typeof(ColorPicker),
            new PropertyMetadata(null));

    public static readonly DependencyProperty ContentTemplateProperty =
        DependencyProperty.Register(
            nameof(ContentTemplate), typeof(DataTemplate), typeof(ColorPicker),
            new PropertyMetadata(null));

    /// <summary>
    /// Gets or sets any content displayed in the ColorPicker's preview content area.
    /// </summary>
    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    /// <summary>
    /// Gets or sets the data template used to display the content of the ColorPicker's preview content area.
    /// </summary>
    public DataTemplate? ContentTemplate
    {
        get => (DataTemplate?)GetValue(ContentTemplateProperty);
        set => SetValue(ContentTemplateProperty, value);
    }

    // HorizontalContentAlignment / VerticalContentAlignment are inherited from Control.
}
