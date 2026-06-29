using System.Windows;
using System.Windows.Controls;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// A form container that hosts <see cref="FormItem"/> and <see cref="FormGroup"/> children.
/// Propagates <see cref="LabelWidth"/>, <see cref="LabelPosition"/>, and <see cref="LabelAlignment"/>
/// down to every contained <see cref="FormItem"/>.
/// </summary>
public class Form : ItemsControl
{
    static Form()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Form), new FrameworkPropertyMetadata(typeof(Form)));
    }

    #region Dependency Properties

    public static readonly DependencyProperty LabelWidthProperty = DependencyProperty.Register(
        nameof(LabelWidth),
        typeof(GridLength),
        typeof(Form),
        new PropertyMetadata(new GridLength(1, GridUnitType.Star), OnFormLayoutPropertyChanged));

    /// <summary>
    /// Width of the label column.
    /// <list type="bullet">
    ///   <item><term>Absolute</term><description>Fixed pixel width for all labels.</description></item>
    ///   <item><term>Star / Auto</term><description>Labels align to the widest label via shared-size scope.</description></item>
    /// </list>
    /// </summary>
    public GridLength LabelWidth
    {
        get => (GridLength)GetValue(LabelWidthProperty);
        set => SetValue(LabelWidthProperty, value);
    }

    public static readonly DependencyProperty LabelPositionProperty = DependencyProperty.Register(
        nameof(LabelPosition),
        typeof(FormLabelPosition),
        typeof(Form),
        new PropertyMetadata(FormLabelPosition.Top, OnFormLayoutPropertyChanged));

    /// <summary>Whether labels appear above (<see cref="FormLabelPosition.Top"/>) or to the left (<see cref="FormLabelPosition.Left"/>) of fields.</summary>
    public FormLabelPosition LabelPosition
    {
        get => (FormLabelPosition)GetValue(LabelPositionProperty);
        set => SetValue(LabelPositionProperty, value);
    }

    public static readonly DependencyProperty LabelAlignmentProperty = DependencyProperty.Register(
        nameof(LabelAlignment),
        typeof(HorizontalAlignment),
        typeof(Form),
        new PropertyMetadata(HorizontalAlignment.Left, OnFormLayoutPropertyChanged));

    /// <summary>Horizontal alignment of label text within the label column.</summary>
    public HorizontalAlignment LabelAlignment
    {
        get => (HorizontalAlignment)GetValue(LabelAlignmentProperty);
        set => SetValue(LabelAlignmentProperty, value);
    }

    #endregion

    #region ItemsControl Overrides

    protected override bool IsItemItsOwnContainerOverride(object item)
        => item is FormItem || item is FormGroup;

    protected override DependencyObject GetContainerForItemOverride()
        => new FormItem();

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);
        ApplyFormProperties(element);
    }

    #endregion

    #region Property Propagation

    private static void OnFormLayoutPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Form form)
        {
            form.PropagateToAllContainers();
        }
    }

    private void PropagateToAllContainers()
    {
        foreach (object item in Items)
        {
            var container = ItemContainerGenerator.ContainerFromItem(item);
            if (container is not null)
            {
                ApplyFormProperties(container);
            }
        }
    }

    internal void ApplyFormProperties(DependencyObject container)
    {
        if (container is FormItem formItem)
        {
            formItem.SetFormProperties(this);
        }
        else if (container is FormGroup formGroup)
        {
            formGroup.SetFormProperties(this);
        }
    }

    #endregion

    /// <summary>
    /// Converts <see cref="LabelWidth"/> to a pixel value for <see cref="FormItem.LabelWidth"/>.
    /// Returns <see cref="double.NaN"/> when shared-size (Star / Auto) alignment should be used.
    /// </summary>
    internal double GetItemLabelWidth()
    {
        return LabelWidth.IsAbsolute ? LabelWidth.Value : double.NaN;
    }
}
