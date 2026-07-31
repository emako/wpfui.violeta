using System.Windows.Automation.Peers;
using System.Windows.Automation;

namespace Wpf.Ui.Violeta.Controls;

public class TeachingTipAutomationPeer(TeachingTip owner) : FrameworkElementAutomationPeer(owner)
{
    protected override AutomationControlType GetAutomationControlTypeCore()
    {
        if (GetTeachingTip().IsLightDismissEnabled)
        {
            return AutomationControlType.Window;
        }
        else
        {
            return AutomationControlType.Pane;
        }
    }

    protected override string GetClassNameCore()
    {
        return nameof(TeachingTip);
    }

    private WindowInteractionState InteractionState()
    {
        var teachingTip = GetTeachingTip();
        if (teachingTip.m_isIdle && teachingTip.IsOpen)
        {
            return WindowInteractionState.ReadyForUserInteraction;
        }
        else if (teachingTip.m_isIdle && !teachingTip.IsOpen)
        {
            return WindowInteractionState.BlockedByModalWindow;
        }
        else if (!teachingTip.m_isIdle && !teachingTip.IsOpen)
        {
            return WindowInteractionState.Closing;
        }
        else
        {
            return WindowInteractionState.Running;
        }
    }

    private bool IsModal()
    {
        return GetTeachingTip().IsLightDismissEnabled;
    }

    private bool IsTopMost()
    {
        return GetTeachingTip().IsOpen;
    }

    private bool Maximizable()
    {
        return false;
    }

    private bool Minimizable()
    {
        return false;
    }

    private WindowVisualState VisualState()
    {
        return WindowVisualState.Normal;
    }

    private void Close()
    {
        GetTeachingTip().IsOpen = false;
    }

    private void SetVisualState(WindowVisualState state)
    {
    }

    private bool WaitForInputIdle(int milliseconds)
    {
        return true;
    }

    // WPF does not expose AutomationEvents.WindowOpened / WindowClosed like WinUI.
    internal void RaiseWindowClosedEvent()
    {
    }

    internal void RaiseWindowOpenedEvent(string displayString)
    {
        _ = displayString;
    }

    private TeachingTip GetTeachingTip()
    {
        var owner = Owner;
        return (TeachingTip)owner;
    }
}
