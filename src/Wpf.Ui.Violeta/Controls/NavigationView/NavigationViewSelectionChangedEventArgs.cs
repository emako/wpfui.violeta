#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8618, CS8619, CS8625

using System;
using Wpf.Ui.Violeta.Controls.Compat;

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
