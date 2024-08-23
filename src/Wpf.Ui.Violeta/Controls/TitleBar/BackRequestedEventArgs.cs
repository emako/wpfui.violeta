﻿using System.Windows;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Provides event data for the BackRequested event.
/// </summary>
public sealed class BackRequestedEventArgs : RoutedEventArgs
{
    internal BackRequestedEventArgs() : base(TitleBar.BackRequestedEvent)
    {
    }

    internal BackRequestedEventArgs(object source) : base(TitleBar.BackRequestedEvent, source)
    {
    }
}
