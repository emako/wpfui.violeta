using System.Windows;
using System.Windows.Controls;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// A selectable tab header item used inside a <see cref="TabStrip"/>.
/// </summary>
public class TabStripItem : ListBoxItem
{
    static TabStripItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(TabStripItem),
            new FrameworkPropertyMetadata(typeof(TabStripItem)));
    }
}
