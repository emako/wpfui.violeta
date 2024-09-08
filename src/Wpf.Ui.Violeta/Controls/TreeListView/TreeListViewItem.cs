using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;

namespace Wpf.Ui.Violeta.Controls;

public class TreeListViewItem : ListViewItem, INotifyPropertyChanged
{
    private TreeNode? _node;

    public TreeNode? Node
    {
        get => _node;
        internal set
        {
            _node = value;
            OnPropertyChanged(nameof(Node));
        }
    }

    public TreeListViewItem()
    {
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (Node != null)
        {
            switch (e.Key)
            {
                case Key.Right:
                    e.Handled = true;
                    if (!Node.IsExpanded)
                    {
                        Node.IsExpanded = true;
                        ChangeFocus(Node);
                    }
                    else if (Node.Children.Count > 0)
                    {
                        ChangeFocus(Node.Children[0]);
                    }

                    break;

                case Key.Left:

                    e.Handled = true;
                    if (Node.IsExpanded && Node.IsExpandable)
                    {
                        Node.IsExpanded = false;
                        ChangeFocus(Node);
                    }
                    else
                    {
                        ChangeFocus(Node.Parent);
                    }

                    break;

                case Key.Subtract:
                    e.Handled = true;
                    Node.IsExpanded = false;
                    ChangeFocus(Node);
                    break;

                case Key.Add:
                    e.Handled = true;
                    Node.IsExpanded = true;
                    ChangeFocus(Node);
                    break;
            }
        }

        if (!e.Handled)
        {
            base.OnKeyDown(e);
        }
    }

    private void ChangeFocus(TreeNode? node)
    {
        if (node?.Tree is TreeListView { } tree)
        {
            if (tree.ItemContainerGenerator.ContainerFromItem(node) is TreeListViewItem item)
            {
                item.Focus();
            }
            else
            {
                tree.PendingFocusNode = node;
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string name)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
