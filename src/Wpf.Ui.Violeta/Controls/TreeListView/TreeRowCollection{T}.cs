using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Wpf.Ui.Violeta.Controls;

internal sealed class TreeRowCollection<T> : ObservableCollection<T>
{
    public void RemoveRange(int index, int count)
    {
        CheckReentrancy();
        if (Items is List<T> { } items)
        {
            items.RemoveRange(index, count);
        }
        OnReset();
    }

    public void InsertRange(int index, IEnumerable<T> collection)
    {
        CheckReentrancy();
        if (Items is List<T> { } items)
        {
            items.InsertRange(index, collection);
        }
        OnReset();
    }

    private void OnReset()
    {
        OnPropertyChanged(nameof(Count));
        OnPropertyChanged("Item[]");
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    private void OnPropertyChanged(string propertyName)
    {
        OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
    }
}
