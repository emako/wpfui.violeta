using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Wpf.Ui.Controls;

public class TreeModelCollection<T> : IEnumerable<T>, IList<T>, ICollection<T>, ITreeModel where T : TreeModelObject<T>, new()
{
    public virtual T Root { get; set; } = new T();

    public virtual ObservableCollection<T> Children
    {
        get => Root.Children;
        set => Root.Children = value;
    }

    public virtual int Count => Root.Children.Count;
    public virtual bool IsReadOnly => false;

    public T this[int index]
    {
        get => Root.Children[index];
        set => Root.Children[index] = value;
    }

    public virtual void Add(T item)
    {
        Root.Children.Add(item);
    }

    public virtual void Clear()
    {
        Root.Children.Clear();
    }

    public virtual bool Contains(T item)
    {
        return Root.Children.Contains(item);
    }

    public virtual void CopyTo(T[] array, int arrayIndex)
    {
        Root.Children.CopyTo(array, arrayIndex);
    }

    public virtual bool Remove(T item)
    {
        return Root.Children.Remove(item);
    }

    public virtual void RemoveAt(int index)
    {
        if (IsReadOnly)
        {
            return;
        }

        if ((uint)index >= (uint)Count)
        {
            return;
        }

        Root.Children.RemoveAt(index);
    }

    public virtual int IndexOf(T item)
    {
        return Root.Children.IndexOf(item);
    }

    public virtual void Insert(int index, T item)
    {
        Root.Children.Insert(index, item);
    }

    public virtual IEnumerator<T> GetEnumerator()
    {
        return Root.Children.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return Root.Children.GetEnumerator();
    }

    public virtual IEnumerable? GetChildren(object parent)
    {
        parent ??= Root;

        if (parent is T { } root)
        {
            return root.Children;
        }
        return null;
    }

    public virtual bool HasChildren(object parent)
    {
        if (parent is T { } root)
        {
            return root.Children.Count > 0;
        }
        return false;
    }

    public virtual void AddChild(T child)
    {
        Root.Children.Add(child);
    }
}
