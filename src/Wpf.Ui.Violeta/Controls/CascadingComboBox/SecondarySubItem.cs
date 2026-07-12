using System.Collections.Generic;

namespace Wpf.Ui.Violeta.Controls;

public sealed class SecondarySubItem(string display, object? value) : ISecondarySubItem
{
    public string Display { get; set; } = display;

    public object? Tag { get; set; }

    public object? Value { get; set; } = value;

    // ICascadingItem explicit implementation (leaf node)
    string ICascadingItem.Label => Display;
    IEnumerable<ICascadingItem>? ICascadingItem.Children => null;
}

/// <summary>
/// Secondary menu leaf item interface. Implements <see cref="ICascadingItem"/> as a leaf node.
/// </summary>
public interface ISecondarySubItem : ICascadingItem
{
    public string Display { get; set; }

    public object? Value { get; set; }
}
