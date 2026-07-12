#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8618, CS8619, CS8625

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
