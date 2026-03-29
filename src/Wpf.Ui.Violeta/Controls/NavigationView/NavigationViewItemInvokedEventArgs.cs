using System;
using Wpf.Ui.Violeta.Controls.Compat;

namespace Wpf.Ui.Violeta.Controls;

public sealed class NavigationViewItemInvokedEventArgs : EventArgs
{
    public NavigationViewItemInvokedEventArgs()
    {
    }

    public object InvokedItem { get; internal set; }
    public bool IsSettingsInvoked { get; internal set; }

    public NavigationViewItemBase InvokedItemContainer { get; internal set; }
    public NavigationTransitionInfo RecommendedNavigationTransitionInfo { get; internal set; }
}


