using System.Windows;
using System.Windows.Controls;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// A <see cref="ContentControl"/> with a paired <see cref="Label"/> property.
/// Used as the base for read-only label/value pairs such as <see cref="DescriptionsItem"/>.
/// </summary>
public class LabeledContentControl : ContentControl
{
    public const string PartLabel = "PART_Label";

    public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
        nameof(Label),
        typeof(object),
        typeof(LabeledContentControl),
        new PropertyMetadata(null));

    public object? Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public static readonly DependencyProperty LabelTemplateProperty = DependencyProperty.Register(
        nameof(LabelTemplate),
        typeof(DataTemplate),
        typeof(LabeledContentControl),
        new PropertyMetadata(null));

    public DataTemplate? LabelTemplate
    {
        get => (DataTemplate?)GetValue(LabelTemplateProperty);
        set => SetValue(LabelTemplateProperty, value);
    }
}
