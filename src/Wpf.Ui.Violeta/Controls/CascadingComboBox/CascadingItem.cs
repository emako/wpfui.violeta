using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Universal cascading menu node interface.
/// A node whose <see cref="Children"/> is null or empty is treated as a selectable leaf value.
/// </summary>
public interface ICascadingItem
{
    string Label { get; }

    object? Tag { get; }

    IEnumerable<ICascadingItem>? Children { get; }
}

/// <summary>
/// Default implementation of <see cref="ICascadingItem"/>.
/// </summary>
public sealed class CascadingItem : ICascadingItem
{
    public string Label { get; set; }

    public object? Tag { get; set; }

    /// <summary>Optional value payload; typically set on leaf nodes.</summary>
    public object? Value { get; set; }

    public IEnumerable<ICascadingItem>? Children { get; set; }

    public CascadingItem(string label, IEnumerable<ICascadingItem>? children = null)
    {
        Label = label;
        Children = children;
    }
}

/// <summary>
/// Holds the items and current selection for a single column in the cascading popup.
/// </summary>
public sealed class CascadingColumnData : INotifyPropertyChanged
{
    private ICascadingItem? _selectedItem;

    public int ColumnIndex { get; }

    public List<ICascadingItem> Items { get; }

    public ICascadingItem? SelectedItem
    {
        get => _selectedItem;
        set
        {
            _selectedItem = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedItem)));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public CascadingColumnData(int index, IEnumerable<ICascadingItem> items)
    {
        ColumnIndex = index;
        Items = items.ToList();
    }
}
