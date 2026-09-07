using System;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Wpf.Ui.Appearance;
using Wpf.Ui.Violeta.Appearance;
using Wpf.Ui.Violeta.Win32;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// A Fluent Design styled <see cref="Popup"/> with acrylic background, rounded corners,
/// and slide/fade/scale entrance animations — ported from FluentWpfCore.
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

        /// <summary>Popup scales up from slightly smaller (ease-out).</summary>
        Scale,
    }

    // ------------------------------------------------------------------
    // Private fields
    // ------------------------------------------------------------------

    private const System.Reflection.BindingFlags PrivateInstance =
        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

    private nint _hwnd;
    private DoubleAnimation? _animation;
    private UIElement? _animationRoot;
    private const double ScaleFrom = 0.88;
    private static readonly TimeSpan ScaleDuration = TimeSpan.FromMilliseconds(280);

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
            popup.ApplyCurrentThemeMaterial();
    }

    private static void OnBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FluentPopup popup && popup._hwnd != 0)
            popup.ApplyCurrentThemeMaterial();
    }

    // ------------------------------------------------------------------
    // Event handlers
    // ------------------------------------------------------------------

    private void OnHostWindowMoved(object? sender, EventArgs e) => FollowMove();

    private void OnHostWindowSizeChanged(object? sender, SizeChangedEventArgs e) => FollowMove();

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        // Apply the starting scale before the popup HWND is created/shown so the window
        // opens already small and then grows — no full-size flash.
        if (e.Property == IsOpenProperty && e.NewValue is true
            && ExtPopupAnimation == FluentPopupAnimation.Scale)
        {
            EnsureScaleStartTransform();
        }

        base.OnPropertyChanged(e);
    }

    private void OnPopupOpened(object? sender, EventArgs e)
    {
        _hwnd = GetNativeHwnd(this);
        ApplyCurrentThemeMaterial();
        ThemeManager.Changed += OnApplicationThemeChanged;
        Dispatcher.InvokeAsync(PlayEntranceAnimation);
    }

    private void OnPopupClosed(object? sender, EventArgs e)
    {
        ThemeManager.Changed -= OnApplicationThemeChanged;
        ResetAnimation();
        _hwnd = 0;
    }

    private void OnApplicationThemeChanged(ApplicationTheme theme, Color systemAccent)
    {
        if (_hwnd != 0)
            ApplyCurrentThemeMaterial();
    }

    private void ApplyCurrentThemeMaterial()
    {
        bool isDark = ThemeManager.GetAppTheme() == ApplicationTheme.Dark;
        DwmApi.ApplyPopupMaterial(_hwnd, Background.Color, WindowCorner, isDark);
    }

    // ------------------------------------------------------------------
    // Animation
    // ------------------------------------------------------------------

    private void EnsureScaleStartTransform()
    {
        if (Child is not FrameworkElement child)
            return;

        child.ApplyTemplate();
        child.LayoutTransform = null;
        child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        var size = child.DesiredSize;
        if (size.Width <= 0 || size.Height <= 0)
            return;

        var origin = GetScaleTransformOrigin(Placement);
        child.LayoutTransform = new ScaleTransform(ScaleFrom, ScaleFrom)
        {
            CenterX = size.Width * origin.X,
            CenterY = size.Height * origin.Y,
        };
    }

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
                    // Match WPF PopupRoot fade: animate the popup window root, not Child.
                    _animationRoot = GetPopupRoot();
                    _animation = new DoubleAnimation(0d, 1d, TimeSpan.FromMilliseconds(300));
                    (_animationRoot ?? Child)?.BeginAnimation(OpacityProperty, _animation);
                    break;
                }
            case FluentPopupAnimation.Scale:
                {
                    // LayoutTransform changes layout size so the popup HWND itself grows.
                    if (Child is not FrameworkElement child)
                    {
                        _animation = null;
                        break;
                    }

                    _animationRoot = child;
                    if (child.LayoutTransform is not ScaleTransform scale)
                    {
                        EnsureScaleStartTransform();
                        if (child.LayoutTransform is not ScaleTransform prepared)
                        {
                            _animation = null;
                            break;
                        }

                        scale = prepared;
                    }

                    var easing = new CubicEase { EasingMode = EasingMode.EaseOut };
                    scale.BeginAnimation(
                        ScaleTransform.ScaleXProperty,
                        new DoubleAnimation(ScaleFrom, 1d, ScaleDuration) { EasingFunction = easing });
                    scale.BeginAnimation(
                        ScaleTransform.ScaleYProperty,
                        new DoubleAnimation(ScaleFrom, 1d, ScaleDuration) { EasingFunction = easing });
                    _animation = null;
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
                (_animationRoot ?? Child)?.BeginAnimation(OpacityProperty, null);
                break;

            case FluentPopupAnimation.Scale:
                if (Child is FrameworkElement { LayoutTransform: ScaleTransform scale })
                {
                    scale.BeginAnimation(ScaleTransform.ScaleXProperty, null);
                    scale.BeginAnimation(ScaleTransform.ScaleYProperty, null);
                }

                if (Child is FrameworkElement fe)
                    fe.LayoutTransform = null;
                break;
        }

        _animationRoot = null;
        _animation = null;
    }

    /// <summary>
    /// Popup HWND root visual (<c>PopupRoot</c>), i.e. the popup window itself — not <see cref="Popup.Child"/>.
    /// </summary>
    private UIElement? GetPopupRoot()
    {
        if (Child is null) return null;
        return PresentationSource.FromVisual(Child)?.RootVisual as UIElement;
    }

    private static Point GetScaleTransformOrigin(PlacementMode placement) => placement switch
    {
        PlacementMode.Top => new Point(0.5, 1),
        PlacementMode.Left => new Point(1, 0.5),
        PlacementMode.Right => new Point(0, 0.5),
        PlacementMode.Center => new Point(0.5, 0.5),
        _ => new Point(0.5, 0), // Bottom / Relative / Mouse / Absolute / etc.
    };

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
