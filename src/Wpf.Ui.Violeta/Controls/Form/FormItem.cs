using System.Windows;
using System.Windows.Controls;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// A single labeled field row inside a <see cref="Form"/>.
/// Supports attached properties (<see cref="LabelProperty"/>, <see cref="IsRequiredProperty"/>,
/// <see cref="NoLabelProperty"/>) so inner controls can declare their own label inline.
/// </summary>
public class FormItem : ContentControl
{
    static FormItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(FormItem), new FrameworkPropertyMetadata(typeof(FormItem)));
    }

    #region Attached Properties

    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.RegisterAttached(
            "Label",
            typeof(object),
            typeof(FormItem),
            new PropertyMetadata(null));

    public static void SetLabel(DependencyObject obj, object? value) => obj.SetValue(LabelProperty, value);
    public static object? GetLabel(DependencyObject obj) => obj.GetValue(LabelProperty);

    public static readonly DependencyProperty IsRequiredProperty =
        DependencyProperty.RegisterAttached(
            "IsRequired",
            typeof(bool),
            typeof(FormItem),
            new PropertyMetadata(false));

    public static void SetIsRequired(DependencyObject obj, bool value) => obj.SetValue(IsRequiredProperty, value);
    public static bool GetIsRequired(DependencyObject obj) => (bool)obj.GetValue(IsRequiredProperty);

    public static readonly DependencyProperty NoLabelProperty =
        DependencyProperty.RegisterAttached(
            "NoLabel",
            typeof(bool),
            typeof(FormItem),
            new PropertyMetadata(false));

    public static void SetNoLabel(DependencyObject obj, bool value) => obj.SetValue(NoLabelProperty, value);
    public static bool GetNoLabel(DependencyObject obj) => (bool)obj.GetValue(NoLabelProperty);

    #endregion

    #region Instance Properties (reuse the attached DPs above)

    /// <summary>The label content displayed next to or above the field.</summary>
    public object? Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>When <see langword="true"/>, an asterisk is displayed next to the label.</summary>
    public bool IsRequired
    {
        get => (bool)GetValue(IsRequiredProperty);
        set => SetValue(IsRequiredProperty, value);
    }

    /// <summary>When <see langword="true"/>, the label area is entirely hidden (useful for full-width action rows).</summary>
    public bool NoLabel
    {
        get => (bool)GetValue(NoLabelProperty);
        set => SetValue(NoLabelProperty, value);
    }

    public static readonly DependencyProperty LabelWidthProperty = DependencyProperty.Register(
        nameof(LabelWidth),
        typeof(double),
        typeof(FormItem),
        new PropertyMetadata(double.NaN, (d, _) => ((FormItem)d).ApplyLabelColumnWidth()));

    /// <summary>
    /// Pixel width of the label column when <see cref="LabelPosition"/> is <see cref="FormLabelPosition.Left"/>.
    /// <see cref="double.NaN"/> means auto-size (uses <c>SharedSizeGroup</c>).
    /// </summary>
    public double LabelWidth
    {
        get => (double)GetValue(LabelWidthProperty);
        set => SetValue(LabelWidthProperty, value);
    }

    public static readonly DependencyProperty LabelPositionProperty = DependencyProperty.Register(
        nameof(LabelPosition),
        typeof(FormLabelPosition),
        typeof(FormItem),
        new PropertyMetadata(FormLabelPosition.Top));

    /// <summary>Controls whether the label is placed above or to the left of the field.</summary>
    public FormLabelPosition LabelPosition
    {
        get => (FormLabelPosition)GetValue(LabelPositionProperty);
        set => SetValue(LabelPositionProperty, value);
    }

    public static readonly DependencyProperty LabelAlignmentProperty = DependencyProperty.Register(
        nameof(LabelAlignment),
        typeof(HorizontalAlignment),
        typeof(FormItem),
        new PropertyMetadata(HorizontalAlignment.Left));

    /// <summary>Horizontal alignment of the label text.</summary>
    public HorizontalAlignment LabelAlignment
    {
        get => (HorizontalAlignment)GetValue(LabelAlignmentProperty);
        set => SetValue(LabelAlignmentProperty, value);
    }

    #endregion

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        ApplyLabelColumnWidth();
    }

    private void ApplyLabelColumnWidth()
    {
        // Only applies to the Left template which has a "LabelColumn" ColumnDefinition.
        if (GetTemplateChild("LabelColumn") is ColumnDefinition col)
        {
            double w = LabelWidth;
            if (!double.IsNaN(w))
            {
                col.Width = new GridLength(w, GridUnitType.Pixel);
                col.SharedSizeGroup = null;
            }
            else
            {
                col.Width = GridLength.Auto;
                col.SharedSizeGroup = "FormLabel";
            }
        }
    }

    /// <summary>
    /// Called by the parent <see cref="Form"/> to push layout settings down.
    /// Also reads attached properties from the inner content control if it was declared inline.
    /// </summary>
    internal void SetFormProperties(Form form)
    {
        LabelWidth = form.GetItemLabelWidth();
        LabelPosition = form.LabelPosition;
        LabelAlignment = form.LabelAlignment;

        // Promote attached properties from the inner Content when the FormItem was
        // auto-generated (i.e. Form wrapped a plain control in a FormItem container).
        if (Content is DependencyObject child)
        {
            object? labelValue = child.GetValue(LabelProperty);
            if (labelValue is not null && GetValue(LabelProperty) is null)
            {
                Label = labelValue;
            }

            if ((bool)child.GetValue(IsRequiredProperty))
            {
                IsRequired = true;
            }

            if ((bool)child.GetValue(NoLabelProperty))
            {
                NoLabel = true;
            }
        }
    }
}
