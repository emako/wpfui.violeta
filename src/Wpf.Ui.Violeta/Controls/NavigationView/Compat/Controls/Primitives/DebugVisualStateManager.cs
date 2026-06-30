#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8618, CS8619, CS8625

#if DEBUG

using System.Diagnostics;
using System.Windows;

namespace Wpf.Ui.Violeta.Controls.Compat;

public class DebugVisualStateManager : VisualStateManager
{
    protected override bool GoToStateCore(
        FrameworkElement control,
        FrameworkElement stateGroupsRoot,
        string stateName,
        VisualStateGroup group,
        VisualState state,
        bool useTransitions)
    {
        if (state == null)
        {
            return false;
        }

        Debug.WriteLine($"stateName = {stateName}, useTransitions = {useTransitions}");

        return base.GoToStateCore(control, stateGroupsRoot, stateName, group, state, useTransitions);
    }
}

#endif
