#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8618, CS8619, CS8625

using System;
using System.Windows;

namespace Wpf.Ui.Violeta.Controls.Compat;

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
