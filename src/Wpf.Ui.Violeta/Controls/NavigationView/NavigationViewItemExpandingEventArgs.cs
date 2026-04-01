using System;

namespace Wpf.Ui.Violeta.Controls;

public sealed class NavigationViewItemExpandingEventArgs : EventArgs
{
    internal NavigationViewItemExpandingEventArgs(NavigationView navigationView)
    {
        m_navigationView = navigationView;
    }

    public NavigationViewItemBase ExpandingItemContainer { get; internal set; }

    public object ExpandingItem
    {
        get
        {
            if (m_expandingItem != null)
            {
                return m_expandingItem;
            }

            if (m_navigationView is { } nv)
            {
                m_expandingItem = nv.MenuItemFromContainer(ExpandingItemContainer);
                return m_expandingItem;
            }

            return null;
        }
    }

    private object m_expandingItem;
    private NavigationView m_navigationView;
}
