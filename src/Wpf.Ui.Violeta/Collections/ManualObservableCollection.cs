using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Wpf.Ui.Violeta.Collections;

/// <summary>
/// <see cref="ObservableCollection{T}"/> that can suppress notifications during batch updates.
/// </summary>
public class ManualObservableCollection<T> : ObservableCollection<T>
{
    private const string CountString = "Count";
    private const string IndexerName = "Item[]";

    private int _oldCount;
    private bool _canNotify = true;

    public bool CanNotify
    {
        get => _canNotify;
        set
        {
            _canNotify = value;

            if (value)
            {
                if (_oldCount != Count)
                    OnPropertyChanged(new PropertyChangedEventArgs(CountString));

                OnPropertyChanged(new PropertyChangedEventArgs(IndexerName));
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
            else
            {
                _oldCount = Count;
            }
        }
    }

    public ManualObservableCollection()
    {
    }

    public ManualObservableCollection(IEnumerable<T> collection)
    {
        if (collection is null)
            throw new ArgumentNullException(nameof(collection));

        foreach (var item in collection)
            Items.Add(item);
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (!CanNotify)
            return;

        base.OnPropertyChanged(e);
    }

    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        if (!CanNotify)
            return;

        base.OnCollectionChanged(e);
    }
}
