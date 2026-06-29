using System.Windows;
using System.Windows.Controls;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// A group of <see cref="FormItem"/> elements with an optional header and separator.
/// </summary>
public class FormGroup : HeaderedItemsControl
{
    static FormGroup()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(FormGroup), new FrameworkPropertyMetadata(typeof(FormGroup)));
    }

    /// <summary>
    /// Reference back to the parent <see cref="Form"/> for property propagation.
    /// Set by <see cref="Form.ApplyFormProperties"/>.
    /// </summary>
    internal Form? ParentForm { get; private set; }

    #region Mirrored Form Layout Properties (set by parent Form)

    internal static readonly DependencyProperty LabelWidthProperty = DependencyProperty.Register(
        nameof(LabelWidth),
        typeof(double),
        typeof(FormGroup),
        new PropertyMetadata(double.NaN, OnGroupLayoutChanged));

    internal double LabelWidth
    {
        get => (double)GetValue(LabelWidthProperty);
        private set => SetValue(LabelWidthProperty, value);
    }

    internal static readonly DependencyProperty LabelPositionProperty = DependencyProperty.Register(
        nameof(LabelPosition),
        typeof(FormLabelPosition),
        typeof(FormGroup),
        new PropertyMetadata(FormLabelPosition.Top, OnGroupLayoutChanged));

    internal FormLabelPosition LabelPosition
    {
        get => (FormLabelPosition)GetValue(LabelPositionProperty);
        private set => SetValue(LabelPositionProperty, value);
    }

    internal static readonly DependencyProperty LabelAlignmentProperty = DependencyProperty.Register(
        nameof(LabelAlignment),
        typeof(HorizontalAlignment),
        typeof(FormGroup),
        new PropertyMetadata(HorizontalAlignment.Left, OnGroupLayoutChanged));

    internal HorizontalAlignment LabelAlignment
    {
        get => (HorizontalAlignment)GetValue(LabelAlignmentProperty);
        private set => SetValue(LabelAlignmentProperty, value);
    }

    #endregion

    #region ItemsControl Overrides

    protected override bool IsItemItsOwnContainerOverride(object item)
        => item is FormItem;

    protected override DependencyObject GetContainerForItemOverride()
        => new FormItem();

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);
        if (element is FormItem formItem && ParentForm is not null)
        {
            formItem.SetFormProperties(ParentForm);
        }
    }

    #endregion

    internal void SetFormProperties(Form form)
    {
        ParentForm = form;
        LabelWidth = form.GetItemLabelWidth();
        LabelPosition = form.LabelPosition;
        LabelAlignment = form.LabelAlignment;
    }

    private static void OnGroupLayoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FormGroup group)
        {
            group.PropagateToAllContainers();
        }
    }

    private void PropagateToAllContainers()
    {
        if (ParentForm is null) return;
        foreach (object item in Items)
        {
            var container = ItemContainerGenerator.ContainerFromItem(item);
            if (container is FormItem fi)
            {
                fi.SetFormProperties(ParentForm);
            }
        }
    }
}
