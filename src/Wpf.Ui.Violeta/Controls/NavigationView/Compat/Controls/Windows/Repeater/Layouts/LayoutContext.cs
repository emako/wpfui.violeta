#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8618, CS8619, CS8625

using System;
using System.Windows;

namespace Wpf.Ui.Violeta.Controls.Compat;

public class LayoutContext : DependencyObject, ILayoutContextOverrides
{
    internal LayoutContext()
    {
    }

    public object LayoutState
    {
        get => LayoutStateCore;
        set => LayoutStateCore = value;
    }

    protected virtual object LayoutStateCore
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    object ILayoutContextOverrides.LayoutStateCore
    {
        get => LayoutStateCore;
        set => LayoutStateCore = value;
    }
}

internal interface ILayoutContextOverrides
{
    object LayoutStateCore { get; set; }
}
