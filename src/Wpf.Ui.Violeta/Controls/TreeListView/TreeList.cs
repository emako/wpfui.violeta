using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Wpf.Ui.Violeta.Controls;

public class TreeList : ListView
{
    /// <summary>
    /// Internal collection of rows representing visible nodes, actually displayed in the ListView
    /// </summary>
    internal TreeRowCollection<TreeNode> Rows { get; private set; }

    public ITreeModel Model
    {
        get => (ITreeModel)GetValue(ModelProperty);
        set => SetValue(ModelProperty, value);
    }

    public static readonly DependencyProperty ModelProperty =
        DependencyProperty.Register(nameof(Model), typeof(ITreeModel), typeof(TreeList), new PropertyMetadata(null!, PropertyChangedCallback));

    public static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TreeList self)
        {
            self._root.Children.Clear();
            self.Rows.Clear();
            self.CreateChildrenNodes(self._root);
        }
    }

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(TreeList), new PropertyMetadata(new CornerRadius(3)));

    private TreeNode _root;

    internal TreeNode Root => _root;

    public ReadOnlyCollection<TreeNode> Nodes => Root.Nodes;

    internal TreeNode? PendingFocusNode { get; set; }

    public ICollection<TreeNode> SelectedNodes => SelectedItems.Cast<TreeNode>().ToArray();

    public TreeNode? SelectedNode => SelectedItems.Count > 0 ? SelectedItems[0] as TreeNode : null;

    public TreeList()
    {
        Rows = [];
        _root = new TreeNode(this, null!)
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
            TreeListItem? item = ItemContainerGenerator.ContainerFromItem(PendingFocusNode) as TreeListItem;
            item?.Focus();
            PendingFocusNode = null!;
        }
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new TreeListItem();
    }

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is TreeListItem;
    }

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        if (element is TreeListItem { } ti && item is TreeNode { } node)
        {
            ti.Node = item as TreeNode;
            base.PrepareContainerForItemOverride(element, node.Tag);
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
        var children = GetChildren(node);
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
            Rows.InsertRange(rowIndex + 1, node.Children.ToArray());
        }
    }

    private void CreateChildrenRows(TreeNode node)
    {
        int index = Rows.IndexOf(node);
        if (index >= 0 || node == _root) // ignore invisible nodes
        {
            var nodes = node.AllVisibleChildren.ToArray();
            Rows.InsertRange(index + 1, nodes);
        }
    }

    internal void DropChildrenRows(TreeNode node, bool removeParent)
    {
        int start = Rows.IndexOf(node);
        if (start >= 0 || node == _root) // ignore invisible nodes
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
            return Model.GetChildren(parent.Tag);
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
            return Model.HasChildren(parent.Tag);
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
