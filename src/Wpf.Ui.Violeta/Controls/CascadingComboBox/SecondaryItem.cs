using System.Collections.Generic;
using System.Linq;

namespace Wpf.Ui.Violeta.Controls;

public sealed class SecondaryItem(string group, IEnumerable<ISecondarySubItem> items) : ISecondaryItem
{
    public string Group { get; set; } = group;

    public object? Tag { get; set; }

    public IEnumerable<ISecondarySubItem> Items { get; set; } = items;

    // ICascadingItem explicit implementation
    string ICascadingItem.Label => Group;
    IEnumerable<ICascadingItem>? ICascadingItem.Children => Items?.Cast<ICascadingItem>();
}

/// <summary>
/// Secondary menu data item interface (2-level shorthand). Implements <see cref="ICascadingItem"/>.
/// </summary>
public interface ISecondaryItem : ICascadingItem
{
    public string Group { get; set; }

    public IEnumerable<ISecondarySubItem> Items { get; set; }
}
