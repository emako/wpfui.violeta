using System;
using System.Windows;

namespace Wpf.Ui.Violeta.Controls;

public sealed class ItemsRepeaterElementClearingEventArgs : EventArgs
{
    internal ItemsRepeaterElementClearingEventArgs(
        UIElement element)
    {
        Update(element);
    }

    public UIElement Element { get; private set; }

    internal void Update(UIElement element)
    {
        Element = element;
    }
}