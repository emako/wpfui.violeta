#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8618, CS8619, CS8625

using System;
using System.Windows;

namespace Wpf.Ui.Violeta.Controls.Compat;

public sealed class ItemsRepeaterElementIndexChangedEventArgs : EventArgs
{
    internal ItemsRepeaterElementIndexChangedEventArgs(
        UIElement element,
        int oldIndex,
        int newIndex)
    {
        Update(element, oldIndex, newIndex);
    }

    public UIElement Element { get; private set; }
    public int OldIndex { get; private set; }
    public int NewIndex { get; private set; }

    internal void Update(UIElement element, int oldIndex, int newIndex)
    {
        Element = element;
        OldIndex = oldIndex;
        NewIndex = newIndex;
    }
}
