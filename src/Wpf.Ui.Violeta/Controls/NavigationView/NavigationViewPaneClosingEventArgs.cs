using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wpf.Ui.Violeta.Controls;

public sealed class NavigationViewPaneClosingEventArgs : EventArgs
{
    internal NavigationViewPaneClosingEventArgs()
    {
    }

    public bool Cancel
    {
        get => m_cancelled;
        set
        {
            m_cancelled = value;

            if (m_splitViewClosingArgs is { } args)
            {
                args.Cancel = value;
            }
        }
    }

    internal void SplitViewClosingArgs(SplitViewPaneClosingEventArgs value)
    {
        m_splitViewClosingArgs = value;
    }

    private SplitViewPaneClosingEventArgs m_splitViewClosingArgs;
    private bool m_cancelled;
}
