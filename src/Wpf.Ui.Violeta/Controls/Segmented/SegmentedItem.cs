using System.Windows;
using System.Windows.Controls;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// A selectable segment used inside a <see cref="Segmented"/> control.
/// </summary>
public class SegmentedItem : ListBoxItem
{
    static SegmentedItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(SegmentedItem),
            new FrameworkPropertyMetadata(typeof(SegmentedItem)));
    }

    /// <summary>Identifies the <see cref="Icon"/> dependency property.</summary>
    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
        nameof(Icon),
        typeof(object),
        typeof(SegmentedItem),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure));

    /// <summary>
    /// Gets or sets the optional icon shown before <see cref="ContentControl.Content"/>.
    /// Icon-only segments omit content.
    /// </summary>
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
}
