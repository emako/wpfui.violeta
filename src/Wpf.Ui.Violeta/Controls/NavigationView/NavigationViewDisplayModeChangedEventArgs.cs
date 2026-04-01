using System;

namespace Wpf.Ui.Violeta.Controls;

public sealed class NavigationViewDisplayModeChangedEventArgs : EventArgs
{
    internal NavigationViewDisplayModeChangedEventArgs()
    {
    }

    public NavigationViewDisplayMode DisplayMode { get; internal set; }
}
