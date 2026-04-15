#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8618, CS8619, CS8625

using System;
using System.Windows;

namespace Wpf.Ui.Violeta.Controls.Compat;

public sealed class ItemsRepeaterElementPreparedEventArgs : EventArgs
{
    internal ItemsRepeaterElementPreparedEventArgs(
        UIElement element,
        int index)
    {
        Update(element, index);
    }

    public UIElement Element { get; private set; }
    public int Index { get; private set; }

    internal void Update(UIElement element, int index)
    {
        Element = element;
        Index = index;
    }
}
