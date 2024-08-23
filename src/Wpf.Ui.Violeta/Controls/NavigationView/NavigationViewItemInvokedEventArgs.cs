using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
