using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;

namespace Wpf.Ui.Controls;

[ContentProperty(nameof(View))]
public class TreeListView : ListView
{
    /// <summary>
    /// Internal collection of rows representing visible nodes, actually displayed in the ListView
    /// </summary>
    public TreeRowCollection<TreeNode> Rows { get; private set; }

    public ITreeModel Model
    {
        get => (ITreeModel)GetValue(ModelProperty);
        set => SetValue(ModelProperty, value);
    }

    public static readonly DependencyProperty ModelProperty =
        DependencyProperty.Register(nameof(Model), typeof(ITreeModel), typeof(TreeListView), new PropertyMetadata(null!, ModelChangedCallback));

    public static void ModelChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TreeListView self)
        {
            self.Root.Children.Clear();
            self.Rows.Clear();
            self.CreateChildrenNodes(self.Root);
        }
    }

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(TreeListView), new PropertyMetadata(new CornerRadius(3)));

    internal TreeNode Root { get; set; } = null!;

    public ReadOnlyCollection<TreeNode> Nodes => Root.Nodes;

    internal TreeNode? PendingFocusNode { get; set; } = null;

    public ICollection<TreeNode> SelectedNodes => SelectedItems.Cast<TreeNode>().ToArray();

    public TreeNode? SelectedNode => SelectedItems.Count > 0 ? SelectedItems[0] as TreeNode : null;

    public TreeListView()
    {
        Rows = [];
        Root = new TreeNode(this, null!)
        {
            IsExpanded = true
        };
        ItemsSource = Rows;
        ItemContainerGenerator.StatusChanged += ItemContainerGeneratorStatusChanged;
    }

    private void ItemContainerGeneratorStatusChanged(object? sender, EventArgs e)
    {
        if (ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated && PendingFocusNode != null)
        {
            TreeListViewItem? item = ItemContainerGenerator.ContainerFromItem(PendingFocusNode) as TreeListViewItem;
            item?.Focus();
            PendingFocusNode = null!;
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

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        if (element is TreeListViewItem { } ti && item is TreeNode { } node)
        {
            ti.Node = item as TreeNode;
            base.PrepareContainerForItemOverride(element, node.Content);
        }
    }

    internal void SetIsExpanded(TreeNode node, bool value)
    {
        if (value)
        {
            if (!node.IsExpandedOnce)
            {
                node.IsExpandedOnce = true;
                node.AssignIsExpanded(value);
                CreateChildrenNodes(node);
            }
            else
            {
                node.AssignIsExpanded(value);
                CreateChildrenRows(node);
            }
        }
        else
        {
            DropChildrenRows(node, false);
            node.AssignIsExpanded(value);
        }
    }

    internal void CreateChildrenNodes(TreeNode node)
    {
        IEnumerable? children = GetChildren(node);
        if (children != null)
        {
            int rowIndex = Rows.IndexOf(node);
            node.ChildrenSource = (children as INotifyCollectionChanged)!;
            foreach (object obj in children)
            {
                TreeNode child = new(this, obj);
                child.HasChildren = HasChildren(child);
                node.Children.Add(child);
            }
            Rows.InsertRange(rowIndex + 1, [.. node.Children]);
        }
    }

    private void CreateChildrenRows(TreeNode node)
    {
        int index = Rows.IndexOf(node);
        if (index >= 0 || node == Root) // ignore invisible nodes
        {
            var nodes = node.AllVisibleChildren.ToArray();
            Rows.InsertRange(index + 1, nodes);
        }
    }

    internal void DropChildrenRows(TreeNode node, bool removeParent)
    {
        int start = Rows.IndexOf(node);
        if (start >= 0 || node == Root) // ignore invisible nodes
        {
            int count = node.VisibleChildrenCount;
            if (removeParent)
            {
                count++;
            }
            else
            {
                start++;
            }

            Rows.RemoveRange(start, count);
        }
    }

    private IEnumerable? GetChildren(TreeNode parent)
    {
        if (Model != null)
        {
            return Model.GetChildren(parent?.Content!);
        }
        else
        {
            return null;
        }
    }

    private bool HasChildren(TreeNode parent)
    {
        if (parent == Root)
        {
            return true;
        }
        else if (Model != null)
        {
            return Model.HasChildren(parent?.Content!);
        }
        else
        {
            return false;
        }
    }

    internal void InsertNewNode(TreeNode parent, object tag, int rowIndex, int index)
    {
        TreeNode node = new(this, tag);
        if (index >= 0 && index < parent.Children.Count)
        {
            parent.Children.Insert(index, node);
        }
        else
        {
            index = parent.Children.Count;
            parent.Children.Add(node);
        }
        Rows.Insert(rowIndex + index + 1, node);
    }
}
