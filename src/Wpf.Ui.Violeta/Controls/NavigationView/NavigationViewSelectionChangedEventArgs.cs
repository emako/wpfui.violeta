using System;

namespace Wpf.Ui.Violeta.Controls;

public sealed class NavigationViewSelectionChangedEventArgs : EventArgs
{
    internal NavigationViewSelectionChangedEventArgs()
    {
    }

    public object SelectedItem { get; internal set; }
    public bool IsSettingsSelected { get; internal set; }

    public NavigationViewItemBase SelectedItemContainer { get; internal set; }
    public NavigationTransitionInfo RecommendedNavigationTransitionInfo { get; internal set; }
}
