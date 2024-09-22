using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace Wpf.Ui.Controls;

public class TreeNode : ITreeNode, INotifyPropertyChanged
{
    private class NodeCollection(TreeNode owner) : Collection<TreeNode>
    {
        private TreeNode? _owner = owner;

        protected override void ClearItems()
        {
            while (Count != 0)
            {
                RemoveAt(Count - 1);
            }
        }

        protected override void InsertItem(int index, TreeNode item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (item.Parent != _owner)
            {
                item.Parent?.Children.Remove(item);

                item._parent = _owner;
                item._index = index;
                for (int i = index; i < Count; i++)
                {
                    this[i]._index++;
                }

                base.InsertItem(index, item);
            }
        }

        protected override void RemoveItem(int index)
        {
            TreeNode item = this[index];
            item._parent = null!;
            item._index = -1;
            for (int i = index + 1; i < Count; i++)
            {
                this[i]._index--;
            }

            base.RemoveItem(index);
        }

        protected override void SetItem(int index, TreeNode item)
        {
            _ = item ?? throw new ArgumentNullException(nameof(item));

            RemoveAt(index);
            InsertItem(index, item);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string name)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    private TreeListView _tree;

    public TreeListView Tree => _tree;

    private INotifyCollectionChanged _childrenSource = null!;

    public INotifyCollectionChanged ChildrenSource
    {
        get => _childrenSource;
        set
        {
            if (_childrenSource != null)
            {
                _childrenSource.CollectionChanged -= ChildrenChanged;
            }

            _childrenSource = value;

            if (_childrenSource != null)
            {
                _childrenSource.CollectionChanged += ChildrenChanged;
            }
        }
    }

    private int _index = -1;

    public int Index => _index;

    /// <summary>
    /// Returns true if all parent nodes of this node are expanded.
    /// </summary>
    public bool IsVisible
    {
        get
        {
            TreeNode? node = _parent;
            while (node != null)
            {
                if (!node.IsExpanded)
                {
                    return false;
                }

                node = node.Parent;
            }
            return true;
        }
    }

    public bool IsExpandedOnce { get; set; }

    public bool HasChildren { get; set; }

    private bool _isExpanded;

    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            if (value != IsExpanded)
            {
                Tree.SetIsExpanded(this, value);
                OnPropertyChanged(nameof(IsExpanded));
                OnPropertyChanged(nameof(IsExpandable));
            }
        }
    }

    public void AssignIsExpanded(bool value)
    {
        _isExpanded = value;
    }

    public bool IsExpandable => (HasChildren && !IsExpandedOnce) || Nodes.Count > 0;

    private bool _isSelected;

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (value != _isSelected)
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }
    }

    private TreeNode? _parent = null;

    public TreeNode? Parent => _parent;

    public int Level => _parent == null ? -1 : _parent.Level + 1;

    public TreeNode? PreviousNode
    {
        get
        {
            if (_parent != null)
            {
                int index = Index;
                if (index > 0)
                {
                    return _parent.Nodes[index - 1];
                }
            }
            return null;
        }
    }

    public TreeNode? NextNode
    {
        get
        {
            if (_parent != null)
            {
                int index = Index;
                if (index < _parent.Nodes.Count - 1)
                {
                    return _parent.Nodes[index + 1];
                }
            }
            return null;
        }
    }

    public TreeNode? BottomNode
    {
        get
        {
            if (Parent != null)
            {
                return Parent.NextNode ?? Parent.BottomNode;
            }
            return null;
        }
    }

    public TreeNode? NextVisibleNode
    {
        get
        {
            if (IsExpanded && Nodes.Count > 0)
            {
                return Nodes[0];
            }
            else
            {
                return NextNode ?? BottomNode;
            }
        }
    }

    public int VisibleChildrenCount => AllVisibleChildren.Count();

    public IEnumerable<TreeNode> AllVisibleChildren
    {
        get
        {
            TreeNode? node = this;
            while (true)
            {
                node = node.NextVisibleNode;
                if (node != null && node.Level > Level)
                {
                    yield return node;
                }
                else
                {
                    break;
                }
            }
        }
    }

    private object? _content = null;

    public object? Content => _content;

    private Collection<TreeNode> _children = null!;

    public Collection<TreeNode> Children => _children;

    private ReadOnlyCollection<TreeNode> _nodes = null!;

    public ReadOnlyCollection<TreeNode> Nodes => _nodes;

    public TreeNode(TreeListView tree, object? content)
    {
        _ = tree ?? throw new ArgumentNullException(nameof(tree));

        _tree = tree;
        _children = new NodeCollection(this);
        _nodes = new ReadOnlyCollection<TreeNode>(_children);
        _content = content;
    }

    public override string ToString()
    {
        return Content != null ? Content.ToString()! : base.ToString()!;
    }

    private void ChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                if (e.NewItems != null)
                {
                    int index = e.NewStartingIndex;
                    int rowIndex = Tree.Rows.IndexOf(this);
                    foreach (object obj in e.NewItems)
                    {
                        Tree.InsertNewNode(this, obj, rowIndex, index);
                        index++;
                    }
                }
                break;

            case NotifyCollectionChangedAction.Remove:
                if (Children.Count > e.OldStartingIndex)
                {
                    RemoveChildAt(e.OldStartingIndex);
                }

                break;

            case NotifyCollectionChangedAction.Move:
            case NotifyCollectionChangedAction.Replace:
            case NotifyCollectionChangedAction.Reset:
                while (Children.Count > 0)
                {
                    RemoveChildAt(0);
                }

                Tree.CreateChildrenNodes(this);
                break;
        }
        HasChildren = Children.Count > 0;
        OnPropertyChanged(nameof(IsExpandable));
    }

    private void RemoveChildAt(int index)
    {
        var child = Children[index];
        Tree.DropChildrenRows(child, true);
        ClearChildrenSource(child);
        Children.RemoveAt(index);
    }

    private void ClearChildrenSource(TreeNode node)
    {
        node.ChildrenSource = null!;
        foreach (TreeNode n in node.Children)
        {
            ClearChildrenSource(n);
        }
    }
}

public interface ITreeNode
{
    public TreeListView Tree { get; }

    public bool IsExpanded { get; set; }

    public bool IsExpandable { get; }

    public bool IsSelected { get; set; }

    public int Level { get; }

    public object? Content { get; }
}
