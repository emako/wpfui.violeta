using System.Windows;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// A single label/value row inside a <see cref="Descriptions"/> list.
/// </summary>
public class DescriptionsItem : LabeledContentControl
{
    static DescriptionsItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DescriptionsItem), new FrameworkPropertyMetadata(typeof(DescriptionsItem)));
    }

    public static readonly DependencyProperty LabelPositionProperty = DependencyProperty.Register(
        nameof(LabelPosition),
        typeof(DescriptionsLabelPosition),
        typeof(DescriptionsItem),
        new PropertyMetadata(DescriptionsLabelPosition.Left));

    public DescriptionsLabelPosition LabelPosition
    {
        get => (DescriptionsLabelPosition)GetValue(LabelPositionProperty);
        set => SetValue(LabelPositionProperty, value);
    }

    public static readonly DependencyProperty ItemAlignmentProperty = DependencyProperty.Register(
        nameof(ItemAlignment),
        typeof(DescriptionsItemAlignment),
        typeof(DescriptionsItem),
        new PropertyMetadata(DescriptionsItemAlignment.Center));

    public DescriptionsItemAlignment ItemAlignment
    {
        get => (DescriptionsItemAlignment)GetValue(ItemAlignmentProperty);
        set => SetValue(ItemAlignmentProperty, value);
    }

    public static readonly DependencyProperty LabelWidthProperty = DependencyProperty.Register(
        nameof(LabelWidth),
        typeof(double),
        typeof(DescriptionsItem),
        new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

    /// <summary>
    /// Pixel width of the label column when <see cref="LabelPosition"/> is <see cref="DescriptionsLabelPosition.Left"/>.
    /// <see cref="double.NaN"/> means auto-size via shared-size scope.
    /// </summary>
    public double LabelWidth
    {
        get => (double)GetValue(LabelWidthProperty);
        set => SetValue(LabelWidthProperty, value);
    }

    public static readonly DependencyProperty SizeProperty = DependencyProperty.Register(
        nameof(Size),
        typeof(DescriptionsSize),
        typeof(DescriptionsItem),
        new PropertyMetadata(DescriptionsSize.Medium));

    /// <summary>Font size variant for top-label layout.</summary>
    public DescriptionsSize Size
    {
        get => (DescriptionsSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    internal void ApplyDescriptionsProperties(Descriptions descriptions)
    {
        if (ReadLocalValue(LabelPositionProperty) == DependencyProperty.UnsetValue)
        {
            SetCurrentValue(LabelPositionProperty, descriptions.LabelPosition);
        }

        if (ReadLocalValue(ItemAlignmentProperty) == DependencyProperty.UnsetValue)
        {
            SetCurrentValue(ItemAlignmentProperty, descriptions.ItemAlignment);
        }

        if (ReadLocalValue(LabelWidthProperty) == DependencyProperty.UnsetValue)
        {
            SetCurrentValue(LabelWidthProperty, descriptions.GetItemLabelWidth());
        }

        if (ReadLocalValue(SizeProperty) == DependencyProperty.UnsetValue)
        {
            SetCurrentValue(SizeProperty, descriptions.Size);
        }

        if (ReadLocalValue(LabelTemplateProperty) == DependencyProperty.UnsetValue && descriptions.LabelTemplate is not null)
        {
            SetCurrentValue(LabelTemplateProperty, descriptions.LabelTemplate);
        }

        if (ReadLocalValue(ContentTemplateProperty) == DependencyProperty.UnsetValue && descriptions.ItemTemplate is not null)
        {
            SetCurrentValue(ContentTemplateProperty, descriptions.ItemTemplate);
        }
    }
}
