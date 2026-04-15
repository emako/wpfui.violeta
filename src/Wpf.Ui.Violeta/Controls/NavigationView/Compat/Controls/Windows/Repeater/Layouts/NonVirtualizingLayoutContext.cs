#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8618, CS8619, CS8625

using System;
using System.Collections.Generic;
using System.Windows;

namespace Wpf.Ui.Violeta.Controls.Compat;

public class NonVirtualizingLayoutContext : LayoutContext
{
    public NonVirtualizingLayoutContext()
    {
    }

    public IReadOnlyList<UIElement> Children => ChildrenCore;

    protected virtual IReadOnlyList<UIElement> ChildrenCore => throw new NotImplementedException();

    internal VirtualizingLayoutContext GetVirtualizingContextAdapter()
    {
        if (m_contextAdapter == null)
        {
            m_contextAdapter = new LayoutContextAdapter(this);
        }
        return m_contextAdapter;
    }

    private VirtualizingLayoutContext m_contextAdapter;
}
