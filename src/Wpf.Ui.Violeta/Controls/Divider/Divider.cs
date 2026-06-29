using System.Windows;
using System.Windows.Controls;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// A horizontal or vertical divider line, optionally with a text label.
/// Mirrors the behaviour of Ursa.Avalonia's Divider control.
/// </summary>
public class Divider : ContentControl
{
    static Divider()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(Divider),
            new FrameworkPropertyMetadata(typeof(Divider)));

        IsTabStopProperty.OverrideMetadata(
            typeof(Divider),
            new FrameworkPropertyMetadata(false));

        FocusableProperty.OverrideMetadata(
            typeof(Divider),
            new FrameworkPropertyMetadata(false));
    }

    public static readonly DependencyProperty OrientationProperty =
        DependencyProperty.Register(
            nameof(Orientation),
            typeof(Orientation),
            typeof(Divider),
            new PropertyMetadata(Orientation.Horizontal));

    /// <summary>
    /// Whether the divider is horizontal (default) or vertical.
    /// </summary>
    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }
}
