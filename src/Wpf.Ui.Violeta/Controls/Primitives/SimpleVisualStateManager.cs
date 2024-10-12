using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Wpf.Ui.Violeta.Controls.Primitives;

/// <summary>
///     Manages visual states and their transitions on a control.
/// </summary>
public class SimpleVisualStateManager : VisualStateManager
{
    public static bool IsAnimationsEnabled => SystemParameters.ClientAreaAnimation &&
                                              RenderCapability.Tier > 0;

    /// <summary>
    ///     Allows subclasses to override the GoToState logic.
    /// </summary>
    protected override bool GoToStateCore(FrameworkElement control, FrameworkElement stateGroupsRoot, string stateName, VisualStateGroup group, VisualState state, bool useTransitions)
    {
        if (state != null)
        {
            useTransitions &= IsAnimationsEnabled;

            if (group.Transitions.Count > 0 && VisualStateGroupHelper.IsSupported)
            {
                return GoToStateInternal(control, stateGroupsRoot, group, state, useTransitions);
            }
            else
            {
                return base.GoToStateCore(control, stateGroupsRoot, stateName, group, state, useTransitions);
            }
        }

        return false;
    }

    #region VisualStateGroups

    internal static Collection<VisualStateGroup> GetVisualStateGroupsInternal(FrameworkElement obj)
    {
        if (obj == null)
        {
            throw new ArgumentNullException("obj");
        }

        // We don't want to get the default value because it will create/return an empty collection.
        BaseValueSource source = DependencyPropertyHelper.GetValueSource(obj, VisualStateGroupsProperty).BaseValueSource;
        if (source != BaseValueSource.Default)
        {
            return (obj.GetValue(VisualStateManager.VisualStateGroupsProperty) as Collection<VisualStateGroup>)!;
        }

        return null!;
    }

    #endregion VisualStateGroups

    #region State Change

    internal static bool TryGetState(IList<VisualStateGroup> groups, string stateName, out VisualStateGroup group, out VisualState state)
    {
        for (int groupIndex = 0; groupIndex < groups.Count; ++groupIndex)
        {
            VisualStateGroup g = groups[groupIndex];
            VisualState s = g.GetState(stateName);
            if (s != null)
            {
                group = g;
                state = s;
                return true;
            }
        }

        group = null!;
        state = null!;
        return false;
    }

    private bool GoToStateInternal(FrameworkElement control, FrameworkElement stateGroupsRoot, VisualStateGroup group, VisualState state, bool useTransitions)
    {
        if (stateGroupsRoot == null)
        {
            throw new ArgumentNullException("stateGroupsRoot");
        }

        if (state == null)
        {
            throw new ArgumentNullException("state");
        }

        if (group == null)
        {
            throw new InvalidOperationException();
        }

        VisualState lastState = group.CurrentState;
        if (lastState == state)
        {
            return true;
        }

        // Get the transition Storyboard. Even if there are no transitions specified, there might
        // be properties that we're rolling back to their default values.
        VisualTransition transition = useTransitions ? GetTransition(stateGroupsRoot, group, lastState, state) : null!;

        // If the transition is null, then we want to instantly snap. The dynamicTransition will
        // consist of everything that is being moved back to the default state.
        // If the transition.Duration and explicit storyboard duration is zero, then we want both the dynamic
        // and state Storyboards to happen in the same tick, so we start them at the same time.
        if (transition == null || transition.GeneratedDuration == DurationZero &&
                                        (transition.Storyboard == null || transition.Storyboard.Duration == DurationZero))
        {
            // Start new state Storyboard and stop any previously running Storyboards
            if (transition != null && transition.Storyboard != null)
            {
                group.StartNewThenStopOld(stateGroupsRoot, transition.Storyboard, state.Storyboard);
            }
            else
            {
                group.StartNewThenStopOld(stateGroupsRoot, state.Storyboard);
            }

            // Fire both CurrentStateChanging and CurrentStateChanged events
            RaiseCurrentStateChanging(group, lastState, state, control, stateGroupsRoot);
            RaiseCurrentStateChanged(group, lastState, state, control, stateGroupsRoot);
        }
        else
        {
            if (transition.Storyboard != null/* && transition.ExplicitStoryboardCompleted == true*/)
            {
                EventHandler transitionCompleted = null!;
                transitionCompleted = new EventHandler((object? sender, EventArgs e) =>
                {
                    if (ShouldRunStateStoryboard(control, stateGroupsRoot, state, group))
                    {
                        group.StartNewThenStopOld(stateGroupsRoot, state.Storyboard);
                    }

                    RaiseCurrentStateChanged(group, lastState, state, control, stateGroupsRoot);

                    transition.Storyboard.Completed -= transitionCompleted;
                    //transition.ExplicitStoryboardCompleted = true;
                });

                // hook up explicit storyboard's Completed event handler
                //transition.ExplicitStoryboardCompleted = false;
                transition.Storyboard.Completed += transitionCompleted;
            }

            // Start transition and dynamicTransition Storyboards
            // Stop any previously running Storyboards
            group.StartNewThenStopOld(stateGroupsRoot, transition.Storyboard!);

            RaiseCurrentStateChanging(group, lastState, state, control, stateGroupsRoot);
        }

        group.SetCurrentState(state);

        return true;
    }

    /// <summary>
    ///   If the stateGroupsRoot or control is removed from the tree, then the new
    ///   storyboards will not be able to resolve target names. Thus,
    ///   if the stateGroupsRoot or control is not in the tree, don't start the new
    ///   storyboards. Also if the group has already changed state, then
    ///   don't start the new storyboards.
    /// </summary>
    private static bool ShouldRunStateStoryboard(FrameworkElement control, FrameworkElement stateGroupsRoot, VisualState state, VisualStateGroup group)
    {
        bool controlInTree = true;
        bool stateGroupsRootInTree = true;

        // We cannot simply check control.IsLoaded because the control may not be in the visual tree
        // even though IsLoaded is true.  Instead we will check that it can find a PresentationSource
        // which would tell us it's in the visual tree.
        if (control != null)
        {
            // If it's visible then it's in the visual tree, so we don't even have to look for a
            // PresentationSource
            if (!control.IsVisible)
            {
                controlInTree = PresentationSource.FromVisual(control) != null;
            }
        }

        if (stateGroupsRoot != null)
        {
            if (!stateGroupsRoot.IsVisible)
            {
                stateGroupsRootInTree = PresentationSource.FromVisual(stateGroupsRoot) != null;
            }
        }

        return controlInTree && stateGroupsRootInTree && state == group.CurrentState;
    }

    #endregion State Change

    #region Transitions

    /// <summary>
    /// Get the most appropriate transition between two states.
    /// </summary>
    /// <param name="element">Element being transitioned.</param>
    /// <param name="group">Group being transitioned.</param>
    /// <param name="from">VisualState being transitioned from.</param>
    /// <param name="to">VisualState being transitioned to.</param>
    /// <returns>
    /// The most appropriate transition between the desired states.
    /// </returns>
    internal static VisualTransition GetTransition(FrameworkElement element, VisualStateGroup group, VisualState from, VisualState to)
    {
        if (element == null)
        {
            throw new ArgumentNullException("element");
        }

        if (group == null)
        {
            throw new ArgumentNullException("group");
        }

        if (to == null)
        {
            throw new ArgumentNullException("to");
        }

        VisualTransition best = null!;
        VisualTransition defaultTransition = null!;
        int bestScore = -1;

        IList<VisualTransition> transitions = (IList<VisualTransition>)group.Transitions;
        if (transitions != null)
        {
            foreach (VisualTransition transition in transitions)
            {
                if (defaultTransition == null && IsDefault(transition))
                {
                    defaultTransition = transition;
                    continue;
                }

                int score = -1;

                VisualState transitionFromState = group.GetState(transition.From);
                VisualState transitionToState = group.GetState(transition.To);

                if (from == transitionFromState)
                {
                    score += 1;
                }
                else if (transitionFromState != null)
                {
                    continue;
                }

                if (to == transitionToState)
                {
                    score += 2;
                }
                else if (transitionToState != null)
                {
                    continue;
                }

                if (score > bestScore)
                {
                    bestScore = score;
                    best = transition;
                }
            }
        }

        return (best ?? defaultTransition)!;
    }

    internal static bool IsDefault(VisualTransition transition)
    {
        return transition.From == null && transition.To == null;
    }

    #endregion Transitions

    #region Data

    private static readonly Duration DurationZero = new Duration(TimeSpan.Zero);

    #endregion Data
}

internal static class VisualStateGroupHelper
{
    internal static bool IsSupported => _setCurrentState.Value != null;

    internal static void SetCurrentState(this VisualStateGroup group, VisualState value)
    {
        if (!IsSupported)
        {
            throw new InvalidOperationException();
        }

        _setCurrentState.Value(group, value);
        Debug.Assert(group.CurrentState == value);
    }

    internal static VisualState GetState(this VisualStateGroup group, string stateName)
    {
        for (int stateIndex = 0; stateIndex < group.States.Count; ++stateIndex)
        {
            VisualState state = (VisualState)group.States[stateIndex]!;
            if (state.Name == stateName)
            {
                return state;
            }
        }

        return null!;
    }

    #region CurrentStoryboards

    private static readonly DependencyProperty CurrentStoryboardsProperty =
        DependencyProperty.RegisterAttached(
            "CurrentStoryboards",
            typeof(Collection<Storyboard>),
            typeof(VisualStateGroupHelper));

    internal static Collection<Storyboard> GetCurrentStoryboards(VisualStateGroup group)
    {
        var currentStoryboards = (Collection<Storyboard>)group.GetValue(CurrentStoryboardsProperty);
        if (currentStoryboards == null)
        {
            currentStoryboards = new Collection<Storyboard>();
            group.SetValue(CurrentStoryboardsProperty, currentStoryboards);
        }
        return currentStoryboards;
    }

    #endregion CurrentStoryboards

    internal static void StartNewThenStopOld(this VisualStateGroup group, FrameworkElement element, params Storyboard[] newStoryboards)
    {
        var currentStoryboards = GetCurrentStoryboards(group);

        // Remove the old Storyboards. Remove is delayed until the next TimeManager tick, so the
        // handoff to the new storyboard is unaffected.
        for (int index = 0; index < currentStoryboards.Count; ++index)
        {
            if (currentStoryboards[index] == null)
            {
                continue;
            }

            currentStoryboards[index].Remove(element);
        }
        currentStoryboards.Clear();

        // Start the new Storyboards
        for (int index = 0; index < newStoryboards.Length; ++index)
        {
            if (newStoryboards[index] == null)
            {
                continue;
            }

            newStoryboards[index].Begin(element, HandoffBehavior.SnapshotAndReplace, true);

            // Hold on to the running Storyboards
            currentStoryboards.Add(newStoryboards[index]);

            // Silverlight had an issue where initially, a checked CheckBox would not show the check mark
            // until the second frame. They chose to do a Seek(0) at this point, which this line
            // is supposed to mimic. It does not seem to be equivalent, though, and WPF ends up
            // with some odd animation behavior. I haven't seen the CheckBox issue on WPF, so
            // commenting this out for now.
            // newStoryboards[index].SeekAlignedToLastTick(element, TimeSpan.Zero, TimeSeekOrigin.BeginTime);
        }
    }

    private static Action<VisualStateGroup, VisualState> CreateSetCurrentStateDelegate()
    {
        try
        {
            return DelegateHelper.CreatePropertySetter<VisualStateGroup, VisualState>(
                nameof(VisualStateGroup.CurrentState),
                nonPublic: true);
        }
        catch (Exception)
        {
            return null!;
        }
    }

    private static readonly Lazy<Action<VisualStateGroup, VisualState>> _setCurrentState = new(CreateSetCurrentStateDelegate);
}

public static class DelegateHelper
{
    private const BindingFlags DefaultLookup = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;

    public static T CreateDelegate<T>(MethodInfo method) where T : Delegate
    {
        return (T)Delegate.CreateDelegate(typeof(T), method);
    }

    public static T CreateDelegate<T>(object firstArgument, MethodInfo method) where T : Delegate
    {
        return (T)Delegate.CreateDelegate(typeof(T), firstArgument, method);
    }

    public static T CreateDelegate<T>(Type target, string method, BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public) where T : Delegate
    {
        if (bindingAttr != (BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public))
        {
            MethodInfo? method2 = target.GetMethod(method, bindingAttr);
            if (method2 != null)
            {
                return CreateDelegate<T>(method2);
            }

            return null!;
        }

        return (T)Delegate.CreateDelegate(typeof(T), target, method);
    }

    public static T CreateDelegate<T>(object target, string method, BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public) where T : Delegate
    {
        if (bindingAttr != (BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public))
        {
            MethodInfo? method2 = target.GetType().GetMethod(method, bindingAttr);
            if (method2 != null)
            {
                return CreateDelegate<T>(target, method2);
            }

            return null!;
        }

        return (T)Delegate.CreateDelegate(typeof(T), target, method);
    }

    public static Func<TType, TProperty> CreatePropertyGetter<TType, TProperty>(string name, BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public, bool nonPublic = false)
    {
        PropertyInfo? property = typeof(TType).GetProperty(name, bindingAttr);
        if (property != null)
        {
            MethodInfo? getMethod = property.GetGetMethod(nonPublic);
            if (getMethod != null)
            {
                return CreateDelegate<Func<TType, TProperty>>(getMethod);
            }
        }

        return null!;
    }

    public static Action<TType, TProperty> CreatePropertySetter<TType, TProperty>(string name, BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public, bool nonPublic = false)
    {
        PropertyInfo? property = typeof(TType).GetProperty(name, bindingAttr);
        if (property != null)
        {
            MethodInfo? setMethod = property.GetSetMethod(nonPublic);
            if (setMethod != null)
            {
                return CreateDelegate<Action<TType, TProperty>>(setMethod);
            }
        }

        return null!;
    }
}
