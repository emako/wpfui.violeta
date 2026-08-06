using System;
using System.Collections.Generic;
using System.Windows.Controls.Primitives;
using Wpf.Ui.Controls;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Whitelist / blacklist of item types that dismiss the <see cref="ToolBar"/> overflow flyout when
/// <see cref="OverflowFlyoutAutoCloseMode.Default"/> is used.
/// An item closes the flyout only if it matches the whitelist and does not match the blacklist.
/// The whitelist defaults to <see cref="ButtonBase"/>.
/// </summary>
public static class OverflowFlyoutAutoCloseTypes
{
    internal static readonly object SyncRoot = new();

    internal static readonly HashSet<Type> Whitelist = [typeof(ButtonBase)];

    internal static readonly HashSet<Type> Blacklist =
    [
        // Primitives
        typeof(ToggleButton), // ButtonBase
        typeof(RepeatButton), // ButtonBase

        // WPF-UI
#pragma warning disable IDE0001
        typeof(Wpf.Ui.Controls.DropDownButton), // ButtonBase
        typeof(Wpf.Ui.Controls.SplitButton), // ButtonBase
#pragma warning restore IDE0001

        // WPF-UI.Violeta
#pragma warning disable IDE0001
        typeof(Wpf.Ui.Violeta.Controls.DropDownButton), // ButtonBase
        typeof(Wpf.Ui.Violeta.Controls.SplitButton), // ButtonBase
#pragma warning restore IDE0001
    ];

    /// <summary>
    /// Adds a type (and its subclasses) to the whitelist of overflow items that may auto-close the flyout.
    /// </summary>
    public static void RegisterWhitelist(Type type)
    {
        _ = type ?? throw new ArgumentNullException(nameof(type));

        lock (SyncRoot)
        {
            Whitelist.Add(type);
        }
    }

    /// <summary>
    /// Removes a type from the whitelist. <see cref="ButtonBase"/> may also be removed.
    /// </summary>
    public static void UnregisterWhitelist(Type type)
    {
        _ = type ?? throw new ArgumentNullException(nameof(type));

        lock (SyncRoot)
        {
            Whitelist.Remove(type);
        }
    }

    /// <summary>
    /// Adds a type (and its subclasses) to the blacklist; matching items never auto-close the flyout.
    /// Blacklist takes precedence over the whitelist.
    /// </summary>
    public static void RegisterBlacklist(Type type)
    {
        _ = type ?? throw new ArgumentNullException(nameof(type));

        lock (SyncRoot)
        {
            Blacklist.Add(type);
        }
    }

    /// <summary>
    /// Removes a type from the blacklist.
    /// </summary>
    public static void UnregisterBlacklist(Type type)
    {
        _ = type ?? throw new ArgumentNullException(nameof(type));

        lock (SyncRoot)
        {
            Blacklist.Remove(type);
        }
    }

    /// <summary>
    /// Returns whether <paramref name="element"/> should auto-close the overflow flyout:
    /// whitelist match and no blacklist match.
    /// </summary>
    public static bool Matches(object? element)
    {
        if (element is null)
        {
            return false;
        }

        Type elementType = element.GetType();

        lock (SyncRoot)
        {
            if (IsAssignableFromAny(Blacklist, elementType))
            {
                return false;
            }

            return IsAssignableFromAny(Whitelist, elementType);
        }
    }

    private static bool IsAssignableFromAny(HashSet<Type> types, Type elementType)
    {
        foreach (Type type in types)
        {
            if (type.IsAssignableFrom(elementType))
            {
                return true;
            }
        }

        return false;
    }
}
