using System.Windows;
using System.Windows.Controls;

namespace Wpf.Ui.Controls;

public class TreeListView : TreeView
{
    public GridViewColumnCollection Columns
    {
        get => (GridViewColumnCollection)GetValue(ColumnsProperty);
        set => SetValue(ColumnsProperty, value);
    }

    public static readonly DependencyProperty ColumnsProperty =
        DependencyProperty.Register(nameof(Columns), typeof(GridViewColumnCollection), typeof(TreeListView), new PropertyMetadata(null));

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new TreeListViewItem();
    }

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is TreeListViewItem;
    }
}
