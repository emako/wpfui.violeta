using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Displays a list of label/value pairs, similar to Ant Design Descriptions.
/// Supports data binding via <see cref="LabelMemberPath"/> / <see cref="DisplayMemberPath"/>
/// or inline <see cref="DescriptionsItem"/> declarations.
/// </summary>
public class Descriptions : ItemsControl
{
    private const string PartRoot = "PART_Root";

    private Panel? _root;

    static Descriptions()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Descriptions), new FrameworkPropertyMetadata(typeof(Descriptions)));
        Grid.IsSharedSizeScopeProperty.OverrideMetadata(
            typeof(Descriptions),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));
        DisplayMemberPathProperty.OverrideMetadata(
            typeof(Descriptions),
            new FrameworkPropertyMetadata(null, OnDisplayMemberPathChanged));
    }

    public static readonly DependencyProperty LabelTemplateProperty = DependencyProperty.Register(
        nameof(LabelTemplate),
        typeof(DataTemplate),
        typeof(Descriptions),
        new PropertyMetadata(null, OnLabelTemplateChanged));

    public DataTemplate? LabelTemplate
    {
        get => (DataTemplate?)GetValue(LabelTemplateProperty);
        set => SetValue(LabelTemplateProperty, value);
    }

    public static readonly DependencyProperty LabelMemberPathProperty = DependencyProperty.Register(
        nameof(LabelMemberPath),
        typeof(string),
        typeof(Descriptions),
        new PropertyMetadata(null, OnLabelMemberPathChanged));

    public string? LabelMemberPath
    {
        get => (string?)GetValue(LabelMemberPathProperty);
        set => SetValue(LabelMemberPathProperty, value);
    }

    public static readonly DependencyProperty LabelPositionProperty = DependencyProperty.Register(
        nameof(LabelPosition),
        typeof(DescriptionsLabelPosition),
        typeof(Descriptions),
        new PropertyMetadata(DescriptionsLabelPosition.Left, OnLayoutPropertyChanged));

    public DescriptionsLabelPosition LabelPosition
    {
        get => (DescriptionsLabelPosition)GetValue(LabelPositionProperty);
        set => SetValue(LabelPositionProperty, value);
    }

    public static readonly DependencyProperty LabelWidthProperty = DependencyProperty.Register(
        nameof(LabelWidth),
        typeof(GridLength),
        typeof(Descriptions),
        new PropertyMetadata(new GridLength(0, GridUnitType.Auto), OnLayoutPropertyChanged));

    public GridLength LabelWidth
    {
        get => (GridLength)GetValue(LabelWidthProperty);
        set => SetValue(LabelWidthProperty, value);
    }

    public static readonly DependencyProperty ItemAlignmentProperty = DependencyProperty.Register(
        nameof(ItemAlignment),
        typeof(DescriptionsItemAlignment),
        typeof(Descriptions),
        new PropertyMetadata(DescriptionsItemAlignment.Center, OnLayoutPropertyChanged));

    public DescriptionsItemAlignment ItemAlignment
    {
        get => (DescriptionsItemAlignment)GetValue(ItemAlignmentProperty);
        set => SetValue(ItemAlignmentProperty, value);
    }

    public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
        nameof(Orientation),
        typeof(Orientation),
        typeof(Descriptions),
        new PropertyMetadata(Orientation.Vertical, OnOrientationChanged));

    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public static readonly DependencyProperty SizeProperty = DependencyProperty.Register(
        nameof(Size),
        typeof(DescriptionsSize),
        typeof(Descriptions),
        new PropertyMetadata(DescriptionsSize.Medium, OnLayoutPropertyChanged));

    public DescriptionsSize Size
    {
        get => (DescriptionsSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public Descriptions()
    {
        ItemContainerGenerator.StatusChanged += OnItemContainerGeneratorStatusChanged;
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _root = GetTemplateChild(PartRoot) as Panel;
        UpdateSharedSizeScope();
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        // When parent passes infinite width but MaxWidth is set, constrain so stretch children share one width.
        if (double.IsPositiveInfinity(availableSize.Width) && MaxWidth < double.PositiveInfinity)
        {
            availableSize.Width = MaxWidth;
        }

        return base.MeasureOverride(availableSize);
    }

    protected override bool IsItemItsOwnContainerOverride(object item)
        => item is DescriptionsItem;

    protected override DependencyObject GetContainerForItemOverride()
        => new DescriptionsItem();

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);

        if (element is not DescriptionsItem descriptionItem)
        {
            return;
        }

        if (ReferenceEquals(element, item))
        {
            descriptionItem.ApplyDescriptionsProperties(this);
            return;
        }

        SetupBindings(descriptionItem, item);
    }

    protected override void ClearContainerForItemOverride(DependencyObject element, object item)
    {
        if (element is DescriptionsItem descriptionItem)
        {
            BindingOperations.ClearBinding(descriptionItem, LabeledContentControl.LabelProperty);
            BindingOperations.ClearBinding(descriptionItem, ContentControl.ContentProperty);
        }

        base.ClearContainerForItemOverride(element, item);
    }

    internal double GetItemLabelWidth()
        => LabelWidth.IsAbsolute ? LabelWidth.Value : double.NaN;

    internal bool UsesFixedWidthLayout()
        => LabelPosition is DescriptionsLabelPosition.Left or DescriptionsLabelPosition.Right
           && ItemAlignment != DescriptionsItemAlignment.Plain;

    private void SetupBindings(DescriptionsItem container, object item)
    {
        container.ApplyDescriptionsProperties(this);
        container.DataContext = item;

        if (!string.IsNullOrEmpty(LabelMemberPath))
        {
            container.ClearValue(LabeledContentControl.LabelTemplateProperty);
            BindingOperations.SetBinding(container, LabeledContentControl.LabelProperty, new Binding(LabelMemberPath));
        }
        else if (LabelTemplate is null)
        {
            container.ClearValue(LabeledContentControl.LabelProperty);
            container.Label = item;
        }

        // DisplayMemberPath auto-generates ItemTemplate on ItemsControl; bind the value property directly instead.
        if (!string.IsNullOrEmpty(DisplayMemberPath))
        {
            container.ClearValue(ContentControl.ContentTemplateProperty);
            BindingOperations.SetBinding(container, ContentControl.ContentProperty, new Binding(DisplayMemberPath));
        }
        else if (ItemTemplate is not null)
        {
            container.ClearValue(ContentControl.ContentProperty);
            container.Content = item;
            container.ContentTemplate = ItemTemplate;
        }
        else
        {
            container.ClearValue(ContentControl.ContentTemplateProperty);
            container.ClearValue(ContentControl.ContentProperty);
            container.Content = item;
        }
    }

    private static void OnDisplayMemberPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Descriptions descriptions)
        {
            descriptions.RefreshContainers();
        }
    }

    private static void OnLabelMemberPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Descriptions descriptions)
        {
            if (e.NewValue is not null && descriptions.LabelTemplate is not null)
            {
                throw new InvalidOperationException("Cannot set both LabelMemberPath and LabelTemplate.");
            }

            descriptions.RefreshContainers();
        }
    }

    private static void OnLabelTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Descriptions descriptions)
        {
            if (e.NewValue is not null && !string.IsNullOrEmpty(descriptions.LabelMemberPath))
            {
                throw new InvalidOperationException("Cannot set both LabelMemberPath and LabelTemplate.");
            }

            descriptions.RefreshContainers();
        }
    }

    private static void OnLayoutPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Descriptions descriptions)
        {
            descriptions.UpdateSharedSizeScope();
            descriptions.PropagateToAllContainers();
        }
    }

    private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Descriptions descriptions)
        {
            descriptions.PropagateToAllContainers();
        }
    }

    private void OnItemContainerGeneratorStatusChanged(object? sender, EventArgs e)
    {
        if (ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
        {
            UpdateSharedSizeScope();
            PropagateToAllContainers();
        }
    }

    private void UpdateSharedSizeScope()
    {
        bool enabled = UsesFixedWidthLayout();
        Grid.SetIsSharedSizeScope(this, enabled);

        if (_root is not null)
        {
            Grid.SetIsSharedSizeScope(_root, enabled);
        }
    }

    private void PropagateToAllContainers()
    {
        foreach (object item in Items)
        {
            if (ItemContainerGenerator.ContainerFromItem(item) is DescriptionsItem descriptionItem)
            {
                descriptionItem.ApplyDescriptionsProperties(this);
            }
        }
    }

    private void RefreshContainers()
    {
        foreach (object item in Items)
        {
            if (ItemContainerGenerator.ContainerFromItem(item) is DescriptionsItem descriptionItem)
            {
                if (ReferenceEquals(descriptionItem, item))
                {
                    descriptionItem.ApplyDescriptionsProperties(this);
                }
                else
                {
                    SetupBindings(descriptionItem, item);
                }
            }
        }
    }
}
