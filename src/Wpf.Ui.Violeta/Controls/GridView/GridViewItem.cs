using System.Windows;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Represents an item in a <see cref="GridView"/>.
/// </summary>
public class GridViewItem : ListViewBaseItem
{
    static GridViewItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(GridViewItem),
            new FrameworkPropertyMetadata(typeof(GridViewItem)));
    }
}
