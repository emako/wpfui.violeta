#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8618, CS8619, CS8625

using System.Windows;
using Wpf.Ui.Violeta.Controls.Compat;

namespace Wpf.Ui.Violeta.Controls;

enum NavigationViewVisualStateDisplayMode
{
    Compact,
    Expanded,
    Minimal,
    MinimalWithBackButton,
}

enum NavigationViewRepeaterPosition
{
    LeftNav,
    TopPrimary,
    TopOverflow,
    LeftFooter,
    TopFooter,
}

enum NavigationViewPropagateTarget
{
    LeftListView,
    TopListView,
    OverflowListView,
    All,
}

class NavigationViewItemHelper
{
    internal const string c_OnLeftNavigationReveal = "OnLeftNavigationReveal";
    internal const string c_OnLeftNavigation = "OnLeftNavigation";
    internal const string c_OnTopNavigationPrimary = "OnTopNavigationPrimary";
    internal const string c_OnTopNavigationPrimaryReveal = "OnTopNavigationPrimaryReveal";
    internal const string c_OnTopNavigationOverflow = "OnTopNavigationOverflow";
}

// Since RS5, a lot of functions in NavigationViewItem is moved to NavigationViewItemPresenter. So they both share some common codes.
// This class helps to initialize and maintain the status of SelectionIndicator and ToolTip
class NavigationViewItemHelper<T> : NavigationViewItemHelper
{
    public NavigationViewItemHelper()
    {
    }

    public UIElement GetSelectionIndicator()
    {
        return m_selectionIndicator;
    }

    public void Init(IControlProtected controlProtected)
    {
        m_selectionIndicator = controlProtected.GetTemplateChild(c_selectionIndicatorName) as UIElement;
    }

    private UIElement m_selectionIndicator;

    private const string c_selectionIndicatorName = "SelectionIndicator";
}
