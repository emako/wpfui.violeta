using System;

namespace Wpf.Ui.Violeta.Controls;

public sealed class NavigationViewItemCollapsedEventArgs : EventArgs
{
    internal NavigationViewItemCollapsedEventArgs(NavigationView navigationView)
    {
        m_navigationView = navigationView;
    }

    public NavigationViewItemBase CollapsedItemContainer { get; internal set; }

    public object CollapsedItem
    {
        get
        {
            if (m_collapsedItem != null)
            {
                return m_collapsedItem;
            }

            if (m_navigationView is { } nv)
            {
                m_collapsedItem = nv.MenuItemFromContainer(CollapsedItemContainer);
                return m_collapsedItem;
            }

            return null!;
        }
    }

    private object m_collapsedItem;
    private NavigationView m_navigationView;
}
