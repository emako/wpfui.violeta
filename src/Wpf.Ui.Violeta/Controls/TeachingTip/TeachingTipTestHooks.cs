using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;
using Wpf.Ui.Violeta.Controls.Compat;

namespace Wpf.Ui.Violeta.Controls;

internal class TeachingTipTestHooks
{
    private static readonly TeachingTipTestHooks s_testHooks = new();

    internal static TeachingTipTestHooks GetGlobalTestHooks()
    {
        return s_testHooks;
    }

    internal static TeachingTipTestHooks EnsureGlobalTestHooks()
    {
        return s_testHooks;
    }

    internal static void SetExpandEasingFunction(TeachingTip teachingTip, EasingFunctionBase easingFunction)
    {
        if (teachingTip != null && easingFunction != null)
        {
            teachingTip.SetExpandEasingFunction(easingFunction);
        }
    }

    internal static void SetContractEasingFunction(TeachingTip teachingTip, EasingFunctionBase easingFunction)
    {
        if (teachingTip != null && easingFunction != null)
        {
            teachingTip.SetContractEasingFunction(easingFunction);
        }
    }

    internal static void SetTipShouldHaveShadow(TeachingTip teachingTip, bool tipShouldHaveShadow)
    {
        teachingTip?.SetTipShouldHaveShadow(tipShouldHaveShadow);
    }

    internal static void SetUseTestWindowBounds(TeachingTip teachingTip, bool useTestWindowBounds)
    {
        teachingTip?.SetUseTestWindowBounds(useTestWindowBounds);
    }

    internal static void SetTestWindowBounds(TeachingTip teachingTip, Rect testWindowBounds)
    {
        teachingTip?.SetTestWindowBounds(testWindowBounds);
    }

    internal static void SetUseTestScreenBounds(TeachingTip teachingTip, bool useTestScreenBounds)
    {
        teachingTip?.SetUseTestScreenBounds(useTestScreenBounds);
    }

    internal static void SetTestScreenBounds(TeachingTip teachingTip, Rect testScreenBounds)
    {
        teachingTip?.SetTestScreenBounds(testScreenBounds);
    }

    internal static void SetTipFollowsTarget(TeachingTip teachingTip, bool tipFollowsTarget)
    {
        teachingTip?.SetTipFollowsTarget(tipFollowsTarget);
    }

    internal static void SetReturnTopForOutOfWindowPlacement(TeachingTip teachingTip, bool returnTopForOutOfWindowPlacement)
    {
        teachingTip?.SetReturnTopForOutOfWindowPlacement(returnTopForOutOfWindowPlacement);
    }

    internal static void SetExpandAnimationDuration(TeachingTip teachingTip, TimeSpan expandAnimationDuration)
    {
        teachingTip?.SetExpandAnimationDuration(expandAnimationDuration);
    }

    internal static void SetContractAnimationDuration(TeachingTip teachingTip, TimeSpan contractAnimationDuration)
    {
        teachingTip?.SetContractAnimationDuration(contractAnimationDuration);
    }

    internal static void NotifyOpenedStatusChanged(TeachingTip sender)
    {
        OpenedStatusChanged?.Invoke(sender, null!);
    }

    internal static void NotifyIdleStatusChanged(TeachingTip sender)
    {
        IdleStatusChanged?.Invoke(sender, null!);
    }

    internal static bool GetIsIdle(TeachingTip teachingTip)
    {
        if (teachingTip != null)
        {
            return teachingTip.GetIsIdle();
        }
        return true;
    }

    internal static void NotifyEffectivePlacementChanged(TeachingTip sender)
    {
        EffectivePlacementChanged?.Invoke(sender, null!);
    }

    internal static TeachingTipPlacementMode GetEffectivePlacement(TeachingTip teachingTip)
    {
        if (teachingTip != null)
        {
            return teachingTip.GetEffectivePlacement();
        }
        return TeachingTipPlacementMode.Auto;
    }

    internal static void NotifyEffectiveHeroContentPlacementChanged(TeachingTip sender)
    {
        EffectiveHeroContentPlacementChanged?.Invoke(sender, null!);
    }

    internal static TeachingTipHeroContentPlacementMode GetEffectiveHeroContentPlacement(TeachingTip teachingTip)
    {
        if (teachingTip != null)
        {
            return teachingTip.GetEffectiveHeroContentPlacement();
        }
        return TeachingTipHeroContentPlacementMode.Auto;
    }

    internal static void NotifyOffsetChanged(TeachingTip sender)
    {
        OffsetChanged?.Invoke(sender, null!);
    }

    internal static void NotifyTitleVisibilityChanged(TeachingTip sender)
    {
        TitleVisibilityChanged?.Invoke(sender, null!);
    }

    internal static void NotifySubtitleVisibilityChanged(TeachingTip sender)
    {
        SubtitleVisibilityChanged?.Invoke(sender, null!);
    }

    internal static double GetVerticalOffset(TeachingTip teachingTip)
    {
        if (teachingTip != null)
        {
            return teachingTip.GetVerticalOffset();
        }
        return 0.0;
    }

    internal static double GetHorizontalOffset(TeachingTip teachingTip)
    {
        if (teachingTip != null)
        {
            return teachingTip.GetHorizontalOffset();
        }
        return 0.0;
    }

    internal static Visibility GetTitleVisibility(TeachingTip teachingTip)
    {
        if (teachingTip != null)
        {
            return teachingTip.GetTitleVisibility();
        }
        return Visibility.Collapsed;
    }

    internal static Visibility GetSubtitleVisibility(TeachingTip teachingTip)
    {
        if (teachingTip != null)
        {
            return teachingTip.GetSubtitleVisibility();
        }
        return Visibility.Collapsed;
    }

    internal static Popup GetPopup(TeachingTip teachingTip)
    {
        if (teachingTip != null)
        {
            return teachingTip.m_popup;
        }
        return null!;
    }

    internal static event TypedEventHandler<TeachingTip, object>? OpenedStatusChanged;

    internal static event TypedEventHandler<TeachingTip, object>? IdleStatusChanged;

    internal static event TypedEventHandler<TeachingTip, object>? OffsetChanged;

    internal static event TypedEventHandler<TeachingTip, object>? EffectivePlacementChanged;

    internal static event TypedEventHandler<TeachingTip, object>? EffectiveHeroContentPlacementChanged;

    internal static event TypedEventHandler<TeachingTip, object>? TitleVisibilityChanged;

    internal static event TypedEventHandler<TeachingTip, object>? SubtitleVisibilityChanged;
}
