using System;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
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
/// <remarks>
/// Backdrop follows FluentWpfCore <c>PopupHelper.SetPopupWindowMaterial</c>.
/// Do not set <see cref="Popup.AllowsTransparency"/> unless needed (e.g. drop shadows);
/// layered popups break Win11 acrylic / Mica. Keep child backgrounds transparent.
/// Use <see cref="Material"/> for Acrylic (tinted blur) or Mica / MicaAlt / SystemAcrylic
/// (Win11 wallpaper materials — not dependent on the host window's solid fill).
/// </remarks>
public class FluentPopup : Popup
{
    public enum FluentPopupAnimation
    {
        None,

        /// <summary>Content slides in from the edge nearest the placement target (HWND stays put).</summary>
        Slide,

        Fade,

        /// <summary>Scales up from slightly smaller (ease-out).</summary>
        Scale,
    }

    private const System.Reflection.BindingFlags PrivateInstance =
        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

    private nint _hwnd;
    private DoubleAnimation? _animation;
    private UIElement? _animationRoot;
    private const double ScaleFrom = 0.88;
    private static readonly TimeSpan ScaleDuration = TimeSpan.FromMilliseconds(280);

    public FluentPopup()
    {
        // Must stay false: WPF sets UsesPerPixelOpacity/WS_EX_LAYERED from AllowsTransparency at
        // HWND creation; layered popups kill Win11 acrylic blur (FluentWpfCore leaves default false).
        AllowsTransparency = false;
        Opened += OnPopupOpened;
        Closed += OnPopupClosed;
    }

    public bool FollowWindowMoving
    {
        get => (bool)GetValue(FollowWindowMovingProperty);
        set => SetValue(FollowWindowMovingProperty, value);
    }

    public static readonly DependencyProperty FollowWindowMovingProperty =
        DependencyProperty.Register(
            nameof(FollowWindowMoving), typeof(bool), typeof(FluentPopup),
            new PropertyMetadata(false, OnFollowWindowMovingChanged));

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
    /// Acrylic tint — only for <see cref="FluentPopupMaterial.Acrylic"/>.
    /// Must stay <see cref="Brushes.Transparent"/> when using Mica / MicaAlt / SystemAcrylic
    /// (system backdrop requires a fully transparent composition surface).
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
    /// Backdrop material. Default is legacy acrylic (FluentWpfCore).
    /// Mica / MicaAlt / SystemAcrylic require Win11 and a transparent <see cref="Background"/>.
    /// </summary>
    public FluentPopupMaterial Material
    {
        get => (FluentPopupMaterial)GetValue(MaterialProperty);
        set => SetValue(MaterialProperty, value);
    }

    public static readonly DependencyProperty MaterialProperty =
        DependencyProperty.Register(
            nameof(Material), typeof(FluentPopupMaterial), typeof(FluentPopup),
            new PropertyMetadata(FluentPopupMaterial.Acrylic, OnMaterialChanged));

    public FluentPopupAnimation ExtPopupAnimation
    {
        get => (FluentPopupAnimation)GetValue(ExtPopupAnimationProperty);
        set => SetValue(ExtPopupAnimationProperty, value);
    }

    public static readonly DependencyProperty ExtPopupAnimationProperty =
        DependencyProperty.Register(
            nameof(ExtPopupAnimation), typeof(FluentPopupAnimation), typeof(FluentPopup),
            new PropertyMetadata(FluentPopupAnimation.None));

    public uint SlideAnimationOffset { get; set; } = 16;

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
        if (d is FluentPopup { IsOpen: true, _hwnd: not 0 } popup)
            DwmApi.SetWindowCorner(popup._hwnd, popup.WindowCorner);
    }

    private static void OnBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FluentPopup popup)
            popup.ApplyFluentHwnd();
    }

    private static void OnMaterialChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FluentPopup popup) return;

        // System backdrop cannot sit under a tinted/opaque Background — same as Window + Mica.
        if (e.NewValue is FluentPopupMaterial material && IsSystemBackdrop(material) &&
            popup.Background is { Color.A: not 0 })
        {
            popup.SetCurrentValue(BackgroundProperty, Brushes.Transparent);
        }

        popup.ApplyFluentHwnd();
    }

    private static bool IsSystemBackdrop(FluentPopupMaterial material) =>
        material is FluentPopupMaterial.Mica
            or FluentPopupMaterial.MicaAlt
            or FluentPopupMaterial.SystemAcrylic;

    private void OnHostWindowMoved(object? sender, EventArgs e) => FollowMove();

    private void OnHostWindowSizeChanged(object? sender, SizeChangedEventArgs e) => FollowMove();

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        if (e.Property == IsOpenProperty && e.NewValue is true
            && ExtPopupAnimation == FluentPopupAnimation.Scale)
        {
            EnsureScaleStartTransform();
        }

        base.OnPropertyChanged(e);
    }

    private void OnPopupOpened(object? sender, EventArgs e)
    {
        if (IsSystemBackdrop(Material) && Background is { Color.A: not 0 })
            SetCurrentValue(BackgroundProperty, Brushes.Transparent);

        _hwnd = GetNativeHwnd(this);
        ApplyFluentHwnd();
        ThemeManager.Changed += OnApplicationThemeChanged;
        Dispatcher.Invoke(PlayEntranceAnimation);
    }

    private void OnPopupClosed(object? sender, EventArgs e)
    {
        ThemeManager.Changed -= OnApplicationThemeChanged;
        ResetAnimation();
        _hwnd = 0;
    }

    private void OnApplicationThemeChanged(ApplicationTheme theme, Color systemAccent) =>
        ApplyFluentHwnd();

    /// <summary>FluentWpfCore <c>ApplyFluentHwnd</c>.</summary>
    private void ApplyFluentHwnd()
    {
        if (!IsOpen)
            return;

        if (_hwnd == 0)
            _hwnd = GetNativeHwnd(this);

        bool isDark = ThemeManager.GetAppTheme() == ApplicationTheme.Dark;
        // System materials: always pass fully transparent tint (never apply GradientColor).
        Color tint = IsSystemBackdrop(Material) ? Colors.Transparent : Background.Color;
        int systemBackdrop = Material switch
        {
            FluentPopupMaterial.Mica => (int)WindowBackdropPreference.Mica,             // DWMSBT_MAINWINDOW = 2
            FluentPopupMaterial.SystemAcrylic => (int)WindowBackdropPreference.Acrylic, // DWMSBT_TRANSIENTWINDOW = 3
            FluentPopupMaterial.MicaAlt => (int)WindowBackdropPreference.Tabbed,        // DWMSBT_TABBEDWINDOW = 4
            _ => 0, // legacy composition acrylic
        };
        DwmApi.ApplyPopupMaterial(_hwnd, tint, WindowCorner, isDark, systemBackdrop);
    }

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
        if (!IsOpen)
            return;

        ResetAnimation();

        switch (ExtPopupAnimation)
        {
            case FluentPopupAnimation.Slide:
                {
                    // Content translate inside fixed HWND — do not animate VerticalOffset
                    // (that sweeps the window across the placement target).
                    if (Child is null)
                        break;

                    _animationRoot = Child;
                    var from = GetSlideFromOffset();
                    var transform = new TranslateTransform(from.X, from.Y);
                    Child.RenderTransform = transform;

                    var duration = TimeSpan.FromMilliseconds(280);
                    var easing = new CubicEase { EasingMode = EasingMode.EaseOut };
                    if (from.X != 0)
                    {
                        transform.BeginAnimation(
                            TranslateTransform.XProperty,
                            new DoubleAnimation(from.X, 0d, duration) { EasingFunction = easing });
                    }

                    if (from.Y != 0)
                    {
                        transform.BeginAnimation(
                            TranslateTransform.YProperty,
                            new DoubleAnimation(from.Y, 0d, duration) { EasingFunction = easing });
                    }

                    break;
                }
            case FluentPopupAnimation.Fade:
                {
                    _animationRoot = Child;
                    _animation = new DoubleAnimation(0d, 1d, TimeSpan.FromMilliseconds(300));
                    Child?.BeginAnimation(OpacityProperty, _animation);
                    break;
                }
            case FluentPopupAnimation.Scale:
                {
                    if (Child is not FrameworkElement child)
                        break;

                    _animationRoot = child;
                    if (child.LayoutTransform is not ScaleTransform scale)
                    {
                        EnsureScaleStartTransform();
                        if (child.LayoutTransform is not ScaleTransform prepared)
                            break;
                        scale = prepared;
                    }

                    var easing = new CubicEase { EasingMode = EasingMode.EaseOut };
                    scale.BeginAnimation(
                        ScaleTransform.ScaleXProperty,
                        new DoubleAnimation(ScaleFrom, 1d, ScaleDuration) { EasingFunction = easing });
                    scale.BeginAnimation(
                        ScaleTransform.ScaleYProperty,
                        new DoubleAnimation(ScaleFrom, 1d, ScaleDuration) { EasingFunction = easing });
                    break;
                }
        }
    }

    private void ResetAnimation()
    {
        switch (ExtPopupAnimation)
        {
            case FluentPopupAnimation.Slide:
                if (Child?.RenderTransform is TranslateTransform translate)
                {
                    translate.BeginAnimation(TranslateTransform.XProperty, null);
                    translate.BeginAnimation(TranslateTransform.YProperty, null);
                }

                Child?.RenderTransform = null;
                break;

            case FluentPopupAnimation.Fade:
                Child?.BeginAnimation(OpacityProperty, null);
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

    private Vector GetSlideFromOffset()
    {
        double distance = SlideAnimationOffset;
        return Placement switch
        {
            PlacementMode.Left => new Vector(distance, 0),
            PlacementMode.Right => new Vector(-distance, 0),
            PlacementMode.Top => new Vector(0, distance),
            PlacementMode.Bottom => new Vector(0, -distance),
            _ => GetAnimateFromBottom(this) ? new Vector(0, -distance) : new Vector(0, distance),
        };
    }

    private static Point GetScaleTransformOrigin(PlacementMode placement) => placement switch
    {
        PlacementMode.Top => new Point(0.5, 1),
        PlacementMode.Left => new Point(1, 0.5),
        PlacementMode.Right => new Point(0, 0.5),
        PlacementMode.Center => new Point(0.5, 0.5),
        _ => new Point(0.5, 0),
    };

    private void FollowMove()
    {
        if (IsOpen)
            CallUpdatePosition(this);
    }

    private static Window? GetHostWindow(FluentPopup popup) =>
        Window.GetWindow(popup.PlacementTarget ?? popup.Child);

    /// <summary>HWND of the popup window (Child PresentationSource, else FluentWpfCore _secHelper).</summary>
    private static nint GetNativeHwnd(Popup popup)
    {
        // PresentationSource is the authoritative HWND after Opened (avoids reflection TFM quirks).
        if (popup.Child is not null &&
            PresentationSource.FromVisual(popup.Child) is HwndSource source &&
            source.Handle != IntPtr.Zero)
        {
            return source.Handle;
        }

        var field = typeof(Popup).GetField("_secHelper", PrivateInstance);
        if (field?.GetValue(popup) is { } secHelper &&
            secHelper.GetType().GetProperty("Handle", PrivateInstance) is { } prop)
        {
            var value = prop.GetValue(secHelper);
            if (value is IntPtr ptr && ptr != IntPtr.Zero)
                return ptr;
            if (value is nint hwnd && hwnd != 0)
                return hwnd;
        }

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
