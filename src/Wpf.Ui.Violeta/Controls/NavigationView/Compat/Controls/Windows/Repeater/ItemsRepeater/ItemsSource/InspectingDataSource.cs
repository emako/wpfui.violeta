using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Wpf.Ui.Violeta.Controls.Compat;

internal class InspectingDataSource : ItemsSourceView
{
    public InspectingDataSource(object source) : base(source)
    {
        _ = source ?? throw new ArgumentNullException(nameof(source));

        if (source is IList vector)
        {
            m_vector = vector;
            ListenToCollectionChanges();
        }
        else
        {
            if (source is IEnumerable iterable)
            {
                m_vector = WrapIterable(iterable);
            }
            else
            {
                throw new ArgumentException("Argument 'source' is not a supported vector.");
            }
        }

        m_uniqueIdMaping = (source as IKeyIndexMapping)!;
    }

    ~InspectingDataSource()
    {
        UnListenToCollectionChanges();
    }

    internal override int GetSizeCore()
    {
        return m_vector.Count;
    }

    internal override object GetAtCore(int index)
    {
        return m_vector[index]!;
    }

    internal override bool HasKeyIndexMappingCore()
    {
        return m_uniqueIdMaping != null;
    }

    internal override string KeyFromIndexCore(int index)
    {
        if (m_uniqueIdMaping != null)
        {
            return m_uniqueIdMaping.KeyFromIndex(index);
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    internal override int IndexFromKeyCore(string id)
    {
        if (m_uniqueIdMaping != null)
        {
            return m_uniqueIdMaping.IndexFromKey(id);
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    internal override int IndexOfCore(object value)
    {
        int index = -1;
        if (m_vector != null)
        {
            var v = m_vector.IndexOf(value);
            if (v >= 0)
            {
                index = v;
            }
        }
        return index;
    }

    [SuppressMessage("Performance", "CA1822:Mark members as static")]
    [SuppressMessage("Performance", "CA1859:Use concrete types when possible for improved performance")]
    private IList WrapIterable(IEnumerable iterable)
    {
        List<object> vector = [];
        var iterator = iterable.GetEnumerator();
        while (iterator.MoveNext())
        {
            vector.Add(iterator.Current);
        }

        return vector;
    }

    private void UnListenToCollectionChanges()
    {
        if (m_vector is INotifyCollectionChanged incc)
        {
            CollectionChangedEventManager.RemoveHandler(incc, OnCollectionChanged);
        }
    }

    private void ListenToCollectionChanges()
    {
        Debug.Assert(m_vector != null);
        if (m_vector is INotifyCollectionChanged incc)
        {
            CollectionChangedEventManager.AddHandler(incc, OnCollectionChanged);
        }
    }

    private void OnCollectionChanged(
         object? sender,
         NotifyCollectionChangedEventArgs e)
    {
        OnItemsSourceChanged(e);
    }

    private readonly IList m_vector;
    private readonly IKeyIndexMapping m_uniqueIdMaping = null!;
}
