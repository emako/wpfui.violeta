using System.Windows;

namespace Wpf.Ui.Controls;

public class TreeListViewItem : TreeViewItem
{
    private int _level = -1;

    /// <summary>
    /// Item's hierarchy in the tree
    /// </summary>
    public int Level
    {
        get
        {
            if (_level == -1)
            {
                _level = (ItemsControlFromItemContainer(this) is TreeListViewItem parent) ? parent.Level + 1 : 0;
            }
            return _level;
        }
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new TreeListViewItem();
    }

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is TreeListViewItem;
    }
}
