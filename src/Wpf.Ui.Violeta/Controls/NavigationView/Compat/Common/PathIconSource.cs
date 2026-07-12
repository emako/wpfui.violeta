#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8618, CS8619, CS8625

using System.Windows;
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Controls.Compat;

/// <summary>
/// Represents an icon source that uses a vector path as its content.
/// </summary>
public class PathIconSource : IconSource
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PathIconSource"/> class.
    /// </summary>
    public PathIconSource()
    {
    }

    /// <summary>
    /// Identifies the <see cref="Data"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty DataProperty =
        DependencyProperty.Register(
            nameof(Data),
            typeof(Geometry),
            typeof(PathIconSource));

    /// <summary>
    /// Gets or sets a <see cref="Geometry"/>.
    /// </summary>
    /// <returns>
    /// A description of the shape to be drawn.
    /// </returns>
    public Geometry Data
    {
        get => (Geometry)GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    /// <inheritdoc/>
    protected override IconElement CreateIconElementCore()
    {
        PathIcon pathIcon = new PathIcon();

        var data = Data;
        if (data != null)
        {
            pathIcon.Data = data;
        }

        var newForeground = Foreground;
        if (newForeground != null)
        {
            pathIcon.Foreground = newForeground;
        }

        return pathIcon;
    }
}
