using System.Windows;
using System.Windows.Controls;

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

    public DescriptionsSize Size
    {
        get => (DescriptionsSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    internal void ApplyDescriptionsProperties(Descriptions descriptions)
    {
        SetIfUnset(LabelPositionProperty, descriptions.LabelPosition);
        SetIfUnset(ItemAlignmentProperty, descriptions.ItemAlignment);
        SetIfUnset(LabelWidthProperty, descriptions.GetItemLabelWidth());
        SetIfUnset(SizeProperty, descriptions.Size);

        if (ReadLocalValue(LabelTemplateProperty) == DependencyProperty.UnsetValue && descriptions.LabelTemplate is not null)
        {
            SetCurrentValue(LabelTemplateProperty, descriptions.LabelTemplate);
        }

        if (string.IsNullOrEmpty(descriptions.DisplayMemberPath)
            && ReadLocalValue(ContentTemplateProperty) == DependencyProperty.UnsetValue
            && descriptions.ItemTemplate is not null)
        {
            SetCurrentValue(ContentTemplateProperty, descriptions.ItemTemplate);
        }

        if (IsLeftLayout(descriptions))
        {
            HorizontalAlignment = HorizontalAlignment.Stretch;
        }
        else
        {
            HorizontalAlignment = HorizontalAlignment.Left;
        }
    }

    private static bool IsLeftLayout(Descriptions descriptions)
        => descriptions.LabelPosition is DescriptionsLabelPosition.Left or DescriptionsLabelPosition.Right
           && descriptions.Orientation == Orientation.Vertical;

    private void SetIfUnset(DependencyProperty property, object value)
    {
        if (ReadLocalValue(property) == DependencyProperty.UnsetValue)
        {
            SetCurrentValue(property, value);
        }
    }
}
