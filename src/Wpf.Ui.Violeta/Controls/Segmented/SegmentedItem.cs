using System.Windows;
using System.Windows.Controls;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// A selectable segment used inside a <see cref="Segmented"/> control.
/// </summary>
[TemplatePart(Name = PartSelectionBackground, Type = typeof(Border))]
public class SegmentedItem : ListBoxItem
{
    private const string PartSelectionBackground = "SelectionBackground";

    private Border? _selectionBackground;

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

    /// <summary>Identifies the <see cref="SelectionCornerRadius"/> dependency property.</summary>
    public static readonly DependencyProperty SelectionCornerRadiusProperty = DependencyProperty.Register(
        nameof(SelectionCornerRadius),
        typeof(CornerRadius),
        typeof(SegmentedItem),
        new FrameworkPropertyMetadata(
            new CornerRadius(4),
            FrameworkPropertyMetadataOptions.AffectsRender,
            OnSelectionCornerRadiusChanged));

    /// <summary>
    /// Corner radius for the selected-state pill. Edge segments use larger radii on
    /// shell-facing corners; middle segments stay uniformly rounded.
    /// </summary>
    public CornerRadius SelectionCornerRadius
    {
        get => (CornerRadius)GetValue(SelectionCornerRadiusProperty);
        set => SetValue(SelectionCornerRadiusProperty, value);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _selectionBackground = GetTemplateChild(PartSelectionBackground) as Border;
        UpdateSelectionCornerRadius();
    }

    private static void OnSelectionCornerRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SegmentedItem item)
        {
            item.UpdateSelectionCornerRadius();
        }
    }

    private void UpdateSelectionCornerRadius()
    {
        if (_selectionBackground is not null)
        {
            _selectionBackground.CornerRadius = SelectionCornerRadius;
        }
    }
}
