using System.Reflection;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Interface for scroll physics models that control smooth scrolling animation behavior.
/// </summary>
public interface IScrollPhysics
{
    /// <summary>Called when a scroll input is received.</summary>
    /// <param name="delta">The scroll amount.</param>
    void OnScroll(double delta);

    /// <summary>
    /// Updates the scroll offset based on the current state and elapsed time.
    /// </summary>
    /// <param name="currentOffset">The current scroll offset.</param>
    /// <param name="dt">Delta time since last update in seconds.</param>
    /// <returns>The new scroll offset.</returns>
    double Update(double currentOffset, double dt);

    /// <summary>Gets a value indicating whether the scrolling animation has stabilized.</summary>
    bool IsStable { get; }

    /// <summary>Gets or sets a value indicating whether precise mode is enabled (e.g., for touchpad input).</summary>
    bool IsPreciseMode { get; set; }
}

internal static class ScrollPhysicsExtensions
{
    /// <summary>Creates a deep clone of the physics instance by copying all public read/write properties.</summary>
    internal static IScrollPhysics Clone(this IScrollPhysics source)
    {
        Type type = source.GetType();
        if (Activator.CreateInstance(type) is not IScrollPhysics clone)
            throw new InvalidOperationException($"Cannot create instance of physics type {type.FullName}.");

        foreach (PropertyInfo prop in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            if (!prop.CanRead || !prop.CanWrite) continue;
            if (prop.GetIndexParameters().Length > 0) continue;
            prop.SetValue(clone, prop.GetValue(source, null), null);
        }

        return clone;
    }
}
