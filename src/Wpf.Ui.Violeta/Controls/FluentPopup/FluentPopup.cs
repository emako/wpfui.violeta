using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Wpf.Ui.Violeta.Win32;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// A Fluent Design styled <see cref="Popup"/> with acrylic background, rounded corners,
/// and slide/fade entrance animations — ported from FluentWpfCore.
/// </summary>
public class FluentPopup : Popup
{
    // ------------------------------------------------------------------
    // Animation type enum
    // ------------------------------------------------------------------

    /// <summary>Entrance / exit animation style for <see cref="FluentPopup"/>.</summary>
    public enum FluentPopupAnimation
    {
        /// <summary>No animation.</summary>
        None,

        /// <summary>Popup slides in/out vertically from the placement edge.</summary>
        Slide,

        /// <summary>Popup fades in/out.</summary>
        Fade,
    }

    // ------------------------------------------------------------------
    // Private fields
    // ------------------------------------------------------------------

    private const System.Reflection.BindingFlags PrivateInstance =
        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

    private nint _hwnd;
    private DoubleAnimation? _animation;

    // ------------------------------------------------------------------
    // Constructor
    // ------------------------------------------------------------------

    public FluentPopup()
    {
        AllowsTransparency = true;
        Opened += OnPopupOpened;
        Closed += OnPopupClosed;
    }

    // ------------------------------------------------------------------
    // Dependency Properties
    // ------------------------------------------------------------------

    /// <summary>
    /// Whether the popup repositions itself when the host window moves or resizes.
    /// </summary>
    public bool FollowWindowMoving
    {
        get => (bool)GetValue(FollowWindowMovingProperty);
        set => SetValue(FollowWindowMovingProperty, value);
    }

    public static readonly DependencyProperty FollowWindowMovingProperty =
        DependencyProperty.Register(
            nameof(FollowWindowMoving), typeof(bool), typeof(FluentPopup),
            new PropertyMetadata(false, OnFollowWindowMovingChanged));

    /// <summary>
    /// Win32 corner style applied to the popup HWND (requires Windows 11+).
    /// </summary>
    public WindowCornerPreference WindowCorner
    {
        get => (WindowCornerPreference)GetValue(WindowCornerProperty);
        set => SetValue(WindowCornerProperty, value);
    }

    public static readonly DependencyProperty WindowCornerProperty =
        DependencyProperty.Register(
            nameof(WindowCorner), typeof(WindowCornerPreference), typeof(FluentPopup),
            new PropertyMetadata(WindowCornerPreference.Round, OnWindowCornerChanged));

    /// <summary>
    /// Acrylic tint color. Keep transparent (default) to use the pure blur effect.
    /// </summary>
    public SolidColorBrush Background
    {
        get => (SolidColorBrush)GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    public static readonly DependencyProperty BackgroundProperty =
        DependencyProperty.Register(
            nameof(Background), typeof(SolidColorBrush), typeof(FluentPopup),
            new PropertyMetadata(Brushes.Transparent, OnBackgroundChanged));

    /// <summary>
    /// Entrance animation type.
    /// </summary>
    public FluentPopupAnimation ExtPopupAnimation
    {
        get => (FluentPopupAnimation)GetValue(ExtPopupAnimationProperty);
        set => SetValue(ExtPopupAnimationProperty, value);
    }

    public static readonly DependencyProperty ExtPopupAnimationProperty =
        DependencyProperty.Register(
            nameof(ExtPopupAnimation), typeof(FluentPopupAnimation), typeof(FluentPopup),
            new PropertyMetadata(FluentPopupAnimation.None));

    /// <summary>
    /// Number of pixels the popup slides when <see cref="PopupAnimation.Slide"/> is used.
    /// </summary>
    public uint SlideAnimationOffset { get; set; } = 30;

    // ------------------------------------------------------------------
    // DP change callbacks
    // ------------------------------------------------------------------

    private static void OnFollowWindowMovingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FluentPopup popup) return;
        var window = GetHostWindow(popup);
        if (window is null) return;

        if (e.NewValue is true)
        {
            window.LocationChanged += popup.OnHostWindowMoved;
            window.SizeChanged += popup.OnHostWindowSizeChanged;
        }
        else
        {
            window.LocationChanged -= popup.OnHostWindowMoved;
            window.SizeChanged -= popup.OnHostWindowSizeChanged;
        }
    }

    private static void OnWindowCornerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FluentPopup popup && popup._hwnd != 0)
            DwmApi.SetWindowCorner(popup._hwnd, popup.WindowCorner);
    }

    private static void OnBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FluentPopup popup && popup._hwnd != 0)
            DwmApi.ApplyPopupMaterial(popup._hwnd, popup.Background.Color, popup.WindowCorner);
    }

    // ------------------------------------------------------------------
    // Event handlers
    // ------------------------------------------------------------------

    private void OnHostWindowMoved(object? sender, EventArgs e) => FollowMove();

    private void OnHostWindowSizeChanged(object sender, SizeChangedEventArgs e) => FollowMove();

    private void OnPopupOpened(object? sender, EventArgs e)
    {
        _hwnd = GetNativeHwnd(this);
        DwmApi.ApplyPopupMaterial(_hwnd, Background.Color, WindowCorner);
        Dispatcher.InvokeAsync(PlayEntranceAnimation);
    }

    private void OnPopupClosed(object? sender, EventArgs e)
    {
        ResetAnimation();
        _hwnd = 0;
    }

    // ------------------------------------------------------------------
    // Animation
    // ------------------------------------------------------------------

    private void PlayEntranceAnimation()
    {
        switch (ExtPopupAnimation)
        {
            case FluentPopupAnimation.Slide:
                {
                    bool fromBottom = GetAnimateFromBottom(this);
                    double offset = fromBottom ? SlideAnimationOffset : -(double)SlideAnimationOffset;
                    _animation = new DoubleAnimation(
                        VerticalOffset + offset,
                        VerticalOffset,
                        TimeSpan.FromMilliseconds(300))
                    {
                        EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
                    };
                    BeginAnimation(VerticalOffsetProperty, _animation);
                    break;
                }
            case FluentPopupAnimation.Fade:
                {
                    _animation = new DoubleAnimation(0d, 1d, TimeSpan.FromMilliseconds(300));
                    Child?.BeginAnimation(OpacityProperty, _animation);
                    break;
                }
            default:
                _animation = null;
                break;
        }
    }

    private void ResetAnimation()
    {
        switch (ExtPopupAnimation)
        {
            case FluentPopupAnimation.Slide:
                BeginAnimation(VerticalOffsetProperty, null);
                break;

            case FluentPopupAnimation.Fade:
                Child?.BeginAnimation(OpacityProperty, null);
                break;
        }
        _animation = null;
    }

    // ------------------------------------------------------------------
    // Native window helpers
    // ------------------------------------------------------------------

    private void FollowMove()
    {
        if (IsOpen)
            CallUpdatePosition(this);
    }

    private static Window? GetHostWindow(FluentPopup popup) =>
        Window.GetWindow(popup.PlacementTarget ?? popup.Child);

    /// <summary>
    /// Retrieves the native HWND of a <see cref="Popup"/>'s internal window via reflection.
    /// </summary>
    private static nint GetNativeHwnd(Popup popup)
    {
        var secHelper = typeof(Popup).GetField("_secHelper", PrivateInstance)?.GetValue(popup);
        if (secHelper is null) return 0;

        var handleProp = secHelper.GetType().GetProperty("Handle", PrivateInstance);
        if (handleProp?.GetValue(secHelper) is nint hwnd)
            return hwnd;

        return 0;
    }

#if NET8_0_OR_GREATER
    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "UpdatePosition")]
    private static extern void CallUpdatePosition(Popup popup);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_AnimateFromBottom")]
    private static extern bool GetAnimateFromBottom(Popup popup);
#else

    private static void CallUpdatePosition(Popup popup) =>
        typeof(Popup)
            .GetMethod("UpdatePosition", PrivateInstance)
            ?.Invoke(popup, null);

    private static bool GetAnimateFromBottom(Popup popup) =>
        typeof(Popup)
            .GetProperty("AnimateFromBottom", PrivateInstance)
            ?.GetValue(popup) is true;

#endif
}
