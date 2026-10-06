using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// A <see cref="ScrollViewer"/> with smooth, fluid scrolling animations supporting mouse wheel,
/// touchpad (WM_MOUSEHWHEEL), and touch/manipulation input.
/// Ported from FluentWpfCore.Controls.SmoothScrollViewer with the class renamed to avoid
/// conflicts with the existing <see cref="SmoothScrollViewer"/>.
/// <br/>
/// </summary>
/// <remarks>
/// <para>
/// The physics model can be replaced via the <see cref="Physics"/> property.
/// Default is <see cref="DefaultScrollPhysics"/>; swap to <see cref="ExponentialScrollPhysics"/>
/// for a different feel.
/// </para>
/// <para>
/// <b>Important:</b> <see cref="ScrollViewer.Content"/> must be a <see cref="UIElement"/>
/// because the control applies a <see cref="TranslateTransform"/> to produce the visual lag effect.
/// </para>
/// </remarks>
public class FluentScrollViewer : ScrollViewer
{
    // Minimum accumulated logical movement before syncing the real ScrollViewer offset.
    // Keeps virtualization panels happy by not spamming ScrollToOffset on every frame.
    private const double LogicalOffsetUpdateDistanceThreshold = 20.0;

    // Cap delta time to avoid huge jumps after GC pause / app suspend / window drag.
    private const double MaxDeltaTime = 0.2;

    // --- Vertical state ---
    private double _logicalOffsetVertical;

    private double _currentVisualOffsetVertical;
    private double _visualDeltaVertical;
    private double _logicalOffsetUpdateAccumulatorVertical;

    // --- Horizontal state ---
    private double _logicalOffsetHorizontal;

    private double _currentVisualOffsetHorizontal;
    private double _visualDeltaHorizontal;
    private double _logicalOffsetUpdateAccumulatorHorizontal;

    // --- Rendering ---
    private long _lastTimestamp;

    private bool _isRendering;

    // --- Visual / template parts ---
    private TranslateTransform? _transform;
    private ScrollBar? _PART_VerticalScrollBar;
    private ScrollBar? _PART_HorizontalScrollBar;

    // --- Physics ---
    private IScrollPhysics _verticalScrollPhysics = new DefaultScrollPhysics();

    private IScrollPhysics _horizontalScrollPhysics = new DefaultScrollPhysics();

    // ------------------------------------------------------------------
    // Constructor
    // ------------------------------------------------------------------

    public FluentScrollViewer()
    {
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        HorizontalMouseWheel.AddMouseWheelHandler(this, OnHorizontalMouseWheel);
    }

    // ------------------------------------------------------------------
    // Public API
    // ------------------------------------------------------------------

    /// <summary>
    /// Gets or sets the scroll physics model. Assigning a new value clones it into independent
    /// vertical and horizontal instances.
    /// </summary>
    /// <exception cref="ArgumentNullException"/>
    public IScrollPhysics Physics
    {
        get => _verticalScrollPhysics;
        set
        {
            _ = value ?? throw new ArgumentNullException(nameof(value));
            _verticalScrollPhysics = value.Clone();
            _horizontalScrollPhysics = value.Clone();
        }
    }

    /// <summary>Smoothly scrolls to the given vertical offset.</summary>
    public void AnimatedScrollToVerticalOffset(double offset, bool usePreciseMode = false)
    {
        if (!IsEnableSmoothScrolling) { ScrollToVerticalOffset(offset); return; }
        HandleScroll(VerticalOffset - offset, 0, usePreciseMode);
    }

    /// <summary>Smoothly scrolls to the given horizontal offset.</summary>
    public void AnimatedScrollToHorizontalOffset(double offset, bool usePreciseMode = false)
    {
        if (!IsEnableSmoothScrolling) { ScrollToHorizontalOffset(offset); return; }
        HandleScroll(0, HorizontalOffset - offset, usePreciseMode);
    }

    // ------------------------------------------------------------------
    // Template
    // ------------------------------------------------------------------

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _PART_VerticalScrollBar = GetTemplateChild("PART_VerticalScrollBar") as ScrollBar;
        _PART_HorizontalScrollBar = GetTemplateChild("PART_HorizontalScrollBar") as ScrollBar;
    }

    // ------------------------------------------------------------------
    // Load / Unload
    // ------------------------------------------------------------------

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (Content is not UIElement element)
            throw new InvalidOperationException(
                $"{nameof(FluentScrollViewer)}.{nameof(Content)} must be a UIElement.");

        _transform = new TranslateTransform();
        element.RenderTransform = _transform;
        element.RenderTransformOrigin = new Point(0, 0);
    }

    private void OnUnloaded(object? sender, RoutedEventArgs e)
    {
        StopRendering();
    }

    // ------------------------------------------------------------------
    // Horizontal touchpad scroll (WM_MOUSEHWHEEL via shared window hook)
    // ------------------------------------------------------------------

    private void OnHorizontalMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (e.Handled
            || !IsVisible
            || !IsEnabled
            || !IsEnableSmoothScrolling
            || !CanScrollHorizontal)
        {
            return;
        }

        bool isPrecise = e.Delta % Mouse.MouseWheelDeltaForOneLine != 0;
        HandleScroll(0, -e.Delta, isPrecise);
        e.Handled = true;
    }

    // ------------------------------------------------------------------
    // Core scroll dispatcher
    // ------------------------------------------------------------------

    private void HandleScroll(double deltaVertical, double deltaHorizontal, bool isPreciseMode = false)
    {
        if (deltaVertical == 0 && deltaHorizontal == 0) return;

        if (!_isRendering)
        {
            _logicalOffsetVertical = VerticalOffset;
            _currentVisualOffsetVertical = _logicalOffsetVertical;
            _visualDeltaVertical = 0;

            _logicalOffsetHorizontal = HorizontalOffset;
            _currentVisualOffsetHorizontal = _logicalOffsetHorizontal;
            _visualDeltaHorizontal = 0;

            // Drop residual velocity/distance from a previous boundary hit.
            _verticalScrollPhysics.Reset();
            _horizontalScrollPhysics.Reset();
        }

        _verticalScrollPhysics.IsPreciseMode = isPreciseMode;
        _horizontalScrollPhysics.IsPreciseMode = isPreciseMode;

        if (deltaVertical != 0) _verticalScrollPhysics.OnScroll(deltaVertical);
        if (deltaHorizontal != 0) _horizontalScrollPhysics.OnScroll(deltaHorizontal);

        StartRendering();
    }

    // ------------------------------------------------------------------
    // Input overrides
    // ------------------------------------------------------------------

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        if (!IsEnableSmoothScrolling) { base.OnMouseWheel(e); return; }

        e.Handled = true;

        bool shift = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
        var orientation = PreferredScrollOrientation;
        if (AllowTogglePreferredScrollOrientationByShiftKey && shift)
            orientation = orientation == Orientation.Vertical ? Orientation.Horizontal : Orientation.Vertical;

        bool isPrecise = e.Delta % Mouse.MouseWheelDeltaForOneLine != 0;

        if (orientation == Orientation.Vertical && CanScrollVertical)
            HandleScroll(e.Delta, 0, isPrecise);
        else if (orientation == Orientation.Horizontal && CanScrollHorizontal)
            HandleScroll(0, e.Delta, isPrecise);
    }

    protected override void OnManipulationStarting(ManipulationStartingEventArgs e)
    {
        base.OnManipulationStarting(e);
        if (!IsEnableSmoothManipulating) return;

        e.Mode = ManipulationModes.TranslateX | ManipulationModes.TranslateY;
        e.ManipulationContainer = this;
        e.Handled = true;
    }

    protected override void OnManipulationDelta(ManipulationDeltaEventArgs e)
    {
        if (!IsEnableSmoothManipulating) { base.OnManipulationDelta(e); return; }

        var t = e.DeltaManipulation.Translation;
        double dH = CanScrollHorizontal ? t.X : 0;
        double dV = CanScrollVertical ? t.Y : 0;

        if ((dH == 0 && dV == 0)
            || e.DeltaManipulation.Expansion.Length != 0
            || e.DeltaManipulation.Rotation != 0)
        {
            base.OnManipulationDelta(e);
            return;
        }

        HandleScroll(dV, dH, true);
        e.Handled = true;
    }

    protected override void OnManipulationInertiaStarting(ManipulationInertiaStartingEventArgs e)
    {
        base.OnManipulationInertiaStarting(e);
        if (!IsEnableSmoothManipulating) return;

        if (e.TranslationBehavior != null)
        {
            double speed = e.InitialVelocities.LinearVelocity.Length;
            double decel = speed / 800.0;
            decel = decel < 0.0012 ? 0.0012 : decel > 0.012 ? 0.012 : decel;
            e.TranslationBehavior.DesiredDeceleration = decel;
        }
        e.Handled = true;
    }

    protected override void OnScrollChanged(ScrollChangedEventArgs e)
    {
        base.OnScrollChanged(e);

        bool hasVerticalChange = e.VerticalChange != 0;
        bool hasHorizontalChange = e.HorizontalChange != 0;
        if (!hasVerticalChange && !hasHorizontalChange) return;

        if (hasVerticalChange)
        {
            _logicalOffsetVertical = e.VerticalOffset;
            if (_isRendering)
            {
                _visualDeltaVertical = _logicalOffsetVertical - _currentVisualOffsetVertical;
                _transform!.Y = _visualDeltaVertical;
            }
            else
            {
                _visualDeltaVertical = 0;
                _transform!.Y = 0;
            }
        }

        if (hasHorizontalChange)
        {
            _logicalOffsetHorizontal = e.HorizontalOffset;
            if (_isRendering)
            {
                _visualDeltaHorizontal = _logicalOffsetHorizontal - _currentVisualOffsetHorizontal;
                _transform!.X = _visualDeltaHorizontal;
            }
            else
            {
                _visualDeltaHorizontal = 0;
                _transform!.X = 0;
            }
        }
    }

    // ------------------------------------------------------------------
    // Rendering loop
    // ------------------------------------------------------------------

    private void StartRendering()
    {
        if (_isRendering) return;

        _lastTimestamp = Stopwatch.GetTimestamp();
        _logicalOffsetUpdateAccumulatorVertical = 0;
        _logicalOffsetUpdateAccumulatorHorizontal = 0;

        CompositionTarget.Rendering += OnRendering;
        _isRendering = true;
        // Do not disable hit-testing while animating: content stays unclickable until inertia finishes,
        // and RenderTransform already participates in WPF hit-testing so visual lag still maps clicks correctly.
    }

    private void StopRendering()
    {
        if (!_isRendering) return;

        CompositionTarget.Rendering -= OnRendering;
        _isRendering = false;

        double fV = Clamp(_currentVisualOffsetVertical, 0, ScrollableHeight);
        double fH = Clamp(_currentVisualOffsetHorizontal, 0, ScrollableWidth);

        if (VerticalOffset != fV)
            ScrollToVerticalOffset(fV);

        if (HorizontalOffset != fH)
            ScrollToHorizontalOffset(fH);

        _logicalOffsetVertical = fV;
        if (_visualDeltaVertical != 0)
        {
            _visualDeltaVertical = 0;
            _transform!.Y = 0;
        }

        _logicalOffsetHorizontal = fH;
        if (_visualDeltaHorizontal != 0)
        {
            _visualDeltaHorizontal = 0;
            _transform!.X = 0;
        }
    }

    private void OnRendering(object? sender, EventArgs e)
    {
        long now = Stopwatch.GetTimestamp();
        double dt = (double)(now - _lastTimestamp) / Stopwatch.Frequency;
        _lastTimestamp = now;

        if (dt > MaxDeltaTime) dt = MaxDeltaTime;

        double scrollableHeight = ScrollableHeight;
        double scrollableWidth = ScrollableWidth;

        double previousVerticalOffset = _currentVisualOffsetVertical;
        double previousHorizontalOffset = _currentVisualOffsetHorizontal;

        // A gesture normally drives only one axis. Skip a stable physics model
        // on every frame just because that axis is scrollable.
        bool updateVertical = scrollableHeight > 0 && !_verticalScrollPhysics.IsStable;
        bool updateHorizontal = scrollableWidth > 0 && !_horizontalScrollPhysics.IsStable;

        if (updateVertical)
        {
            _currentVisualOffsetVertical = Clamp(
                _verticalScrollPhysics.Update(previousVerticalOffset, dt), 0, scrollableHeight);
        }

        if (updateHorizontal)
        {
            _currentVisualOffsetHorizontal = Clamp(
                _horizontalScrollPhysics.Update(previousHorizontalOffset, dt), 0, scrollableWidth);
        }

        bool verticalMoved = _currentVisualOffsetVertical != previousVerticalOffset;
        bool horizontalMoved = _currentVisualOffsetHorizontal != previousHorizontalOffset;

        bool verticalStable = !updateVertical
            || _verticalScrollPhysics.IsStable
            || _currentVisualOffsetVertical <= 0
            || _currentVisualOffsetVertical >= scrollableHeight;
        bool horizontalStable = !updateHorizontal
            || _horizontalScrollPhysics.IsStable
            || _currentVisualOffsetHorizontal <= 0
            || _currentVisualOffsetHorizontal >= scrollableWidth;

        if (verticalStable && horizontalStable)
        {
            StopRendering();
            return;
        }

        var transform = _transform!;

        if (verticalMoved)
        {
            _logicalOffsetUpdateAccumulatorVertical +=
                Math.Abs(_currentVisualOffsetVertical - previousVerticalOffset);

            if (_logicalOffsetUpdateAccumulatorVertical >= LogicalOffsetUpdateDistanceThreshold)
            {
                _logicalOffsetUpdateAccumulatorVertical = 0;
                ScrollToVerticalOffset(_currentVisualOffsetVertical);
            }

            double visualDeltaVertical = _logicalOffsetVertical - _currentVisualOffsetVertical;
            if (_visualDeltaVertical != visualDeltaVertical)
            {
                _visualDeltaVertical = visualDeltaVertical;
                transform.Y = visualDeltaVertical;
            }

            _PART_VerticalScrollBar?.SetCurrentValue(RangeBase.ValueProperty, _currentVisualOffsetVertical);
        }

        if (horizontalMoved)
        {
            _logicalOffsetUpdateAccumulatorHorizontal +=
                Math.Abs(_currentVisualOffsetHorizontal - previousHorizontalOffset);

            if (_logicalOffsetUpdateAccumulatorHorizontal >= LogicalOffsetUpdateDistanceThreshold)
            {
                _logicalOffsetUpdateAccumulatorHorizontal = 0;
                ScrollToHorizontalOffset(_currentVisualOffsetHorizontal);
            }

            double visualDeltaHorizontal = _logicalOffsetHorizontal - _currentVisualOffsetHorizontal;
            if (_visualDeltaHorizontal != visualDeltaHorizontal)
            {
                _visualDeltaHorizontal = visualDeltaHorizontal;
                transform.X = visualDeltaHorizontal;
            }

            _PART_HorizontalScrollBar?.SetCurrentValue(RangeBase.ValueProperty, _currentVisualOffsetHorizontal);
        }
    }

    // ------------------------------------------------------------------
    // Dependency Properties
    // ------------------------------------------------------------------

    /// <summary>Whether smooth scrolling is enabled. Default: <see langword="true"/>.</summary>
    public bool IsEnableSmoothScrolling
    {
        get => (bool)GetValue(IsEnableSmoothScrollingProperty);
        set => SetValue(IsEnableSmoothScrollingProperty, value);
    }

    public static readonly DependencyProperty IsEnableSmoothScrollingProperty =
        DependencyProperty.Register(
            nameof(IsEnableSmoothScrolling), typeof(bool), typeof(FluentScrollViewer),
            new PropertyMetadata(true, OnIsEnableSmoothScrollingChanged));

    private static void OnIsEnableSmoothScrollingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FluentScrollViewer sv && e.NewValue is false && sv._isRendering)
            sv.StopRendering();
    }

    /// <summary>Whether touch/manipulation smooth scrolling is enabled. Default: <see langword="false"/>.</summary>
    public bool IsEnableSmoothManipulating
    {
        get => (bool)GetValue(IsEnableSmoothManipulatingProperty);
        set => SetValue(IsEnableSmoothManipulatingProperty, value);
    }

    public static readonly DependencyProperty IsEnableSmoothManipulatingProperty =
        DependencyProperty.Register(
            nameof(IsEnableSmoothManipulating), typeof(bool), typeof(FluentScrollViewer),
            new PropertyMetadata(false, OnIsEnableSmoothManipulatingChanged));

    private static void OnIsEnableSmoothManipulatingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FluentScrollViewer sv)
            sv.IsManipulationEnabled = e.NewValue is true;
    }

    /// <summary>
    /// Primary scroll direction when the mouse wheel is used.
    /// Holding Shift toggles it when <see cref="AllowTogglePreferredScrollOrientationByShiftKey"/> is <see langword="true"/>.
    /// Default: <see cref="Orientation.Vertical"/>.
    /// </summary>
    public Orientation PreferredScrollOrientation
    {
        get => (Orientation)GetValue(PreferredScrollOrientationProperty);
        set => SetValue(PreferredScrollOrientationProperty, value);
    }

    public static readonly DependencyProperty PreferredScrollOrientationProperty =
        DependencyProperty.Register(
            nameof(PreferredScrollOrientation), typeof(Orientation), typeof(FluentScrollViewer),
            new FrameworkPropertyMetadata(Orientation.Vertical));

    /// <summary>
    /// When <see langword="true"/> (default), holding Shift while scrolling toggles the preferred orientation.
    /// </summary>
    public bool AllowTogglePreferredScrollOrientationByShiftKey
    {
        get => (bool)GetValue(AllowTogglePreferredScrollOrientationByShiftKeyProperty);
        set => SetValue(AllowTogglePreferredScrollOrientationByShiftKeyProperty, value);
    }

    public static readonly DependencyProperty AllowTogglePreferredScrollOrientationByShiftKeyProperty =
        DependencyProperty.Register(
            nameof(AllowTogglePreferredScrollOrientationByShiftKey), typeof(bool), typeof(FluentScrollViewer),
            new FrameworkPropertyMetadata(true));

    // ------------------------------------------------------------------
    // Read-only helpers
    // ------------------------------------------------------------------

    /// <summary>Gets a value indicating whether the viewer has scrollable vertical content.</summary>
    public bool CanScrollVertical =>
        ScrollInfo is IScrollInfo v
            ? v.ExtentHeight - v.ViewportHeight > 0
            : ExtentHeight > ViewportHeight;

    /// <summary>Gets a value indicating whether the viewer has scrollable horizontal content.</summary>
    public bool CanScrollHorizontal =>
        ScrollInfo is IScrollInfo h
            ? h.ExtentWidth - h.ViewportWidth > 0
            : ExtentWidth > ViewportWidth;

    /// <summary>Gets a value indicating whether the viewer can scroll upward.</summary>
    public bool CanScrollUp =>
        ScrollInfo is IScrollInfo v
            ? v.VerticalOffset > 0.5
            : VerticalOffset > 0.5;

    /// <summary>Gets a value indicating whether the viewer can scroll downward.</summary>
    public bool CanScrollDown =>
        ScrollInfo is IScrollInfo v
            ? v.VerticalOffset + v.ViewportHeight < v.ExtentHeight - 0.5
            : VerticalOffset + ViewportHeight < ExtentHeight - 0.5;

    /// <summary>Gets a value indicating whether the viewer can scroll left.</summary>
    public bool CanScrollLeft =>
        ScrollInfo is IScrollInfo h
            ? h.HorizontalOffset > 0.5
            : HorizontalOffset > 0.5;

    /// <summary>Gets a value indicating whether the viewer can scroll right.</summary>
    public bool CanScrollRight =>
        ScrollInfo is IScrollInfo h
            ? h.HorizontalOffset + h.ViewportWidth < h.ExtentWidth - 0.5
            : HorizontalOffset + ViewportWidth < ExtentWidth - 0.5;

    // ------------------------------------------------------------------
    // Private helpers
    // ------------------------------------------------------------------

    private static double Clamp(double value, double min, double max)
        => value < min ? min : value > max ? max : value;
}
