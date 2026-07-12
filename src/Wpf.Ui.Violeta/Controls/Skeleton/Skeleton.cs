using System.Windows;
using System.Windows.Controls;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// A skeleton / placeholder control that overlays its content with a loading shimmer.
/// When <see cref="IsLoading"/> is <c>true</c> the content is hidden behind a solid overlay.
/// When <see cref="IsActive"/> is also <c>true</c> a horizontal shimmer sweep animation plays.
/// </summary>
public class Skeleton : ContentControl
{
    static Skeleton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(Skeleton),
            new FrameworkPropertyMetadata(typeof(Skeleton)));
    }

    public static readonly DependencyProperty IsActiveProperty =
        DependencyProperty.Register(
            nameof(IsActive), typeof(bool), typeof(Skeleton),
            new PropertyMetadata(false));

    /// <summary>
    /// Gets or sets whether the shimmer sweep animation is playing.
    /// Only effective when <see cref="IsLoading"/> is also <c>true</c>.
    /// </summary>
    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    public static readonly DependencyProperty IsLoadingProperty =
        DependencyProperty.Register(
            nameof(IsLoading), typeof(bool), typeof(Skeleton),
            new PropertyMetadata(false));

    /// <summary>
    /// Gets or sets whether the skeleton overlay is visible (hides the content).
    /// </summary>
    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }
}
