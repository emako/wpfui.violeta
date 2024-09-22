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
public class TreeModelListView : ListView
{
    /// <summary>
    /// Internal collection of rows representing visible nodes, actually displayed in the ListView
    /// </summary>
    public TreeModelRowCollection<TreeModelNode> Rows { get; private set; }

    public ITreeModel Model
    {
        get => (ITreeModel)GetValue(ModelProperty);
        set => SetValue(ModelProperty, value);
    }

    public static readonly DependencyProperty ModelProperty =
        DependencyProperty.Register(nameof(Model), typeof(ITreeModel), typeof(TreeModelListView), new PropertyMetadata(null!, ModelChangedCallback));

    public static void ModelChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TreeModelListView self)
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
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(TreeModelListView), new PropertyMetadata(new CornerRadius(3)));

    internal TreeModelNode Root { get; set; } = null!;

    public ReadOnlyCollection<TreeModelNode> Nodes => Root.Nodes;

    internal TreeModelNode? PendingFocusNode { get; set; } = null;

    public ICollection<TreeModelNode> SelectedNodes => SelectedItems.Cast<TreeModelNode>().ToArray();

    public TreeModelNode? SelectedNode => SelectedItems.Count > 0 ? SelectedItems[0] as TreeModelNode : null;

    public TreeModelListView()
    {
        Rows = [];
        Root = new TreeModelNode(this, null!)
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
            TreeModelListViewItem? item = ItemContainerGenerator.ContainerFromItem(PendingFocusNode) as TreeModelListViewItem;
            item?.Focus();
            PendingFocusNode = null!;
        }
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new TreeModelListViewItem();
    }

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is TreeModelListViewItem;
    }

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        if (element is TreeModelListViewItem { } ti && item is TreeModelNode { } node)
        {
            ti.Node = item as TreeModelNode;
            base.PrepareContainerForItemOverride(element, node.Content);
        }
    }

    internal void SetIsExpanded(TreeModelNode node, bool value)
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

    internal void CreateChildrenNodes(TreeModelNode node)
    {
        IEnumerable? children = GetChildren(node);
        if (children != null)
        {
            int rowIndex = Rows.IndexOf(node);
            node.ChildrenSource = (children as INotifyCollectionChanged)!;
            foreach (object obj in children)
            {
                TreeModelNode child = new(this, obj);
                child.HasChildren = HasChildren(child);
                node.Children.Add(child);
            }
            Rows.InsertRange(rowIndex + 1, [.. node.Children]);
        }
    }

    private void CreateChildrenRows(TreeModelNode node)
    {
        int index = Rows.IndexOf(node);
        if (index >= 0 || node == Root) // ignore invisible nodes
        {
            var nodes = node.AllVisibleChildren.ToArray();
            Rows.InsertRange(index + 1, nodes);
        }
    }

    internal void DropChildrenRows(TreeModelNode node, bool removeParent)
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

    private IEnumerable? GetChildren(TreeModelNode parent)
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

    private bool HasChildren(TreeModelNode parent)
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

    internal void InsertNewNode(TreeModelNode parent, object tag, int rowIndex, int index)
    {
        TreeModelNode node = new(this, tag);
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
