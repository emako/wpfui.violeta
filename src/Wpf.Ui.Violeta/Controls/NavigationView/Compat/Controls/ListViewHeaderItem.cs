#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8618, CS8619, CS8625

using System.Windows;

namespace Wpf.Ui.Violeta.Controls.Compat;

/// <summary>
/// Represents items in the header for grouped data inside a ListView.
/// </summary>
public class ListViewHeaderItem : ListViewBaseHeaderItem
{
    static ListViewHeaderItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ListViewHeaderItem), new FrameworkPropertyMetadata(typeof(ListViewHeaderItem)));
    }

    /// <summary>
    /// Initializes a new instance of the ListViewHeaderItem class.
    /// </summary>
    public ListViewHeaderItem()
    {
    }
}
