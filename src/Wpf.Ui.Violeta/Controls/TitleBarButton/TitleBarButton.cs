using System.Windows;
using System.Windows.Controls.Primitives;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Defines the press animation used by a <see cref="TitleBarButton"/>.
/// </summary>
public enum TitleBarButtonPressAnimation
{
    /// <summary>
    /// Disables press animation.
    /// </summary>
    None,

    /// <summary>
    /// Compresses the content horizontally toward its center, then restores it from the center.
    /// </summary>
    Center,

    /// <summary>
    /// Compresses the content horizontally toward the right, then restores it from the left.
    /// </summary>
    RightToLeft,
}

public partial class TitleBarButton : ButtonBase
{
    static TitleBarButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(TitleBarButton), new FrameworkPropertyMetadata(typeof(TitleBarButton)));
    }

    /// <summary>
    /// Gets or sets the visual feedback displayed while the button is pressed.
    /// Defaults to <see cref="TitleBarButtonPressAnimation.None"/>.
    /// </summary>
    public TitleBarButtonPressAnimation PressAnimation
    {
        get => (TitleBarButtonPressAnimation)GetValue(PressAnimationProperty);
        set => SetValue(PressAnimationProperty, value);
    }

    public static readonly DependencyProperty PressAnimationProperty = DependencyProperty.Register(
        nameof(PressAnimation),
        typeof(TitleBarButtonPressAnimation),
        typeof(TitleBarButton),
        new FrameworkPropertyMetadata(TitleBarButtonPressAnimation.None));

    public static readonly DependencyProperty IsActiveProperty =
        DependencyProperty.Register(nameof(IsActive), typeof(bool), typeof(TitleBarButton), new PropertyMetadata(true));

    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }
}
