using System.Windows;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// A WinUI-style grid of items that wraps horizontally (default <see cref="System.Windows.Controls.WrapPanel"/>).
/// Distinct from <c>Wpf.Ui.Controls.GridView</c> used as a ListView column view.
/// </summary>
public class GridView : ListViewBase
{
    static GridView()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(GridView),
            new FrameworkPropertyMetadata(typeof(GridView)));
    }

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is GridViewItem;
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new GridViewItem();
    }
}
