using System.Windows;
using Wpf.Ui.Violeta.Controls.Compat;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Represents the header for a group of items in a <see cref="GridView"/>.
/// </summary>
public class GridViewHeaderItem : ListViewBaseHeaderItem
{
    static GridViewHeaderItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(GridViewHeaderItem),
            new FrameworkPropertyMetadata(typeof(GridViewHeaderItem)));
    }

    public static readonly DependencyProperty DividerVisibilityProperty =
        DependencyProperty.Register(
            nameof(DividerVisibility),
            typeof(Visibility),
            typeof(GridViewHeaderItem),
            new PropertyMetadata(Visibility.Visible));

    public Visibility DividerVisibility
    {
        get => (Visibility)GetValue(DividerVisibilityProperty);
        set => SetValue(DividerVisibilityProperty, value);
    }
}
