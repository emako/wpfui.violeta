using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Provides an eight-direction virtual joystick with mouse drag and arrow-key input.
/// </summary>
public partial class VirtualJoystick : UserControl
{
    public static readonly DependencyProperty PadDiameterProperty = DependencyProperty.Register(
        nameof(PadDiameter),
        typeof(double),
        typeof(VirtualJoystick),
        new FrameworkPropertyMetadata(100d, OnPadDiameterChanged, CoercePadDiameter));

    public static readonly DependencyProperty ArcPathBrushProperty = DependencyProperty.Register(
        nameof(ArcPathBrush),
        typeof(Brush),
        typeof(VirtualJoystick),
        new PropertyMetadata(null, OnVisualPropertyChanged));

    public static readonly DependencyProperty ArrowIdleBrushProperty = DependencyProperty.Register(
        nameof(ArrowIdleBrush),
        typeof(Brush),
        typeof(VirtualJoystick),
        new PropertyMetadata(null, OnVisualPropertyChanged));

    public static readonly DependencyProperty ArrowActiveBrushProperty = DependencyProperty.Register(
        nameof(ArrowActiveBrush),
        typeof(Brush),
        typeof(VirtualJoystick),
        new PropertyMetadata(null, OnVisualPropertyChanged));

    public static readonly DependencyProperty ArrowStrokeThicknessProperty = DependencyProperty.Register(
        nameof(ArrowStrokeThickness),
        typeof(double),
        typeof(VirtualJoystick),
        new FrameworkPropertyMetadata(2d, OnVisualPropertyChanged, CoerceArrowStrokeThickness));

    public static readonly DependencyProperty KnobBrushProperty = DependencyProperty.Register(
        nameof(KnobBrush),
        typeof(Brush),
        typeof(VirtualJoystick),
        new PropertyMetadata(null, OnVisualPropertyChanged));

    private static readonly DependencyPropertyKey CurrentDirectionPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(CurrentDirection),
        typeof(JoyStickDirection),
        typeof(VirtualJoystick),
        new PropertyMetadata(JoyStickDirection.None));

    public static readonly DependencyProperty CurrentDirectionProperty = CurrentDirectionPropertyKey.DependencyProperty;

    private readonly HashSet<Key> _pressedKeys = [];
    private Point _vector;
    private Window? _hostWindow;
    private bool _isMouseDragging;
    private bool _isMouseActive;

    public VirtualJoystick()
    {
        InitializeComponent();
        SetResourceReference(ArcPathBrushProperty, "ControlFillColorSecondaryBrush");
        SetResourceReference(ArrowIdleBrushProperty, "TextFillColorSecondaryBrush");
        SetResourceReference(ArrowActiveBrushProperty, "AccentFillColorDefaultBrush");
        SetResourceReference(KnobBrushProperty, "AccentFillColorDefaultBrush");
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        IsVisibleChanged += OnIsVisibleChanged;
        RefreshLayout();
    }

    /// <summary>
    /// Gets or sets the diameter of the joystick surface.
    /// </summary>
    public double PadDiameter
    {
        get => (double)GetValue(PadDiameterProperty);
        set => SetValue(PadDiameterProperty, value);
    }

    /// <summary>
    /// Gets or sets the brush used by the directional arc segments.
    /// </summary>
    public Brush? ArcPathBrush
    {
        get => (Brush?)GetValue(ArcPathBrushProperty);
        set => SetValue(ArcPathBrushProperty, value);
    }

    /// <summary>
    /// Gets or sets the brush used by inactive direction arrows.
    /// </summary>
    public Brush? ArrowIdleBrush
    {
        get => (Brush?)GetValue(ArrowIdleBrushProperty);
        set => SetValue(ArrowIdleBrushProperty, value);
    }

    /// <summary>
    /// Gets or sets the brush used by active direction arrows.
    /// </summary>
    public Brush? ArrowActiveBrush
    {
        get => (Brush?)GetValue(ArrowActiveBrushProperty);
        set => SetValue(ArrowActiveBrushProperty, value);
    }

    /// <summary>
    /// Gets or sets the thickness of direction arrow strokes.
    /// </summary>
    public double ArrowStrokeThickness
    {
        get => (double)GetValue(ArrowStrokeThicknessProperty);
        set => SetValue(ArrowStrokeThicknessProperty, value);
    }

    /// <summary>
    /// Gets or sets the brush used by the draggable knob.
    /// </summary>
    public Brush? KnobBrush
    {
        get => (Brush?)GetValue(KnobBrushProperty);
        set => SetValue(KnobBrushProperty, value);
    }

    /// <summary>
    /// Gets the current discrete joystick direction.
    /// </summary>
    public JoyStickDirection CurrentDirection
    {
        get => (JoyStickDirection)GetValue(CurrentDirectionProperty);
        private set => SetValue(CurrentDirectionPropertyKey, value);
    }

    /// <summary>
    /// Occurs when the joystick position changes.
    /// </summary>
    public event EventHandler<JoystickMoveEventArgs>? Moved;

    private double InnerCircleRadius => PadDiameter * 0.275d;

    private double KnobRadius => PadDiameter * 0.07d;

    private double MaxDistance => InnerCircleRadius - KnobRadius;

    private double DeadZone => MaxDistance * 0.15d;

    private static void OnPadDiameterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((VirtualJoystick)d).RefreshLayout();

    private static void OnVisualPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((VirtualJoystick)d).UpdateVisualState(true);

    [SuppressMessage("Performance", "CA1859:Use concrete types when possible for improved performance")]
    private static object CoercePadDiameter(DependencyObject d, object value)
    {
        double diameter = (double)value;
        return double.IsNaN(diameter) || double.IsInfinity(diameter) ? 100d : Math.Max(32d, Math.Min(1024d, diameter));
    }

    [SuppressMessage("Performance", "CA1859:Use concrete types when possible for improved performance")]
    private static object CoerceArrowStrokeThickness(DependencyObject d, object value)
    {
        double thickness = (double)value;
        return double.IsNaN(thickness) || double.IsInfinity(thickness) ? 2d : Math.Max(0.1d, thickness);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _hostWindow = Window.GetWindow(this);
        if (_hostWindow == null)
        {
            return;
        }

        _hostWindow.PreviewKeyDown += OnHostPreviewKeyDown;
        _hostWindow.PreviewKeyUp += OnHostPreviewKeyUp;
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        Reset();
        if (_hostWindow == null)
        {
            return;
        }

        _hostWindow.PreviewKeyDown -= OnHostPreviewKeyDown;
        _hostWindow.PreviewKeyUp -= OnHostPreviewKeyUp;
        _hostWindow = null;
    }

    private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (Visibility != Visibility.Visible)
        {
            Reset();
        }
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (!IsEnabled)
        {
            return;
        }

        joystickSurface.Focus();
        _pressedKeys.Clear();
        _isMouseActive = true;
        _isMouseDragging = true;
        joystickSurface.CaptureMouse();
        SetVector(GetVectorFromMousePosition(e.GetPosition(joystickSurface)), true);
        e.Handled = true;
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (!_isMouseDragging || e.LeftButton != MouseButtonState.Pressed)
        {
            return;
        }

        SetVector(GetVectorFromMousePosition(e.GetPosition(joystickSurface)), true);
    }

    private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!_isMouseDragging)
        {
            return;
        }

        _isMouseDragging = false;
        joystickSurface.ReleaseMouseCapture();
        _isMouseActive = false;
        ApplyKeyboardState();
        e.Handled = true;
    }

    private void OnHostPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (_isMouseActive || !IsKeyboardTarget() || !IsDirectionKey(e.Key))
        {
            return;
        }

        e.Handled = true;
        if (_pressedKeys.Add(e.Key))
        {
            ApplyKeyboardState();
        }
    }

    private void OnHostPreviewKeyUp(object sender, KeyEventArgs e)
    {
        if (_isMouseActive || !IsKeyboardTarget() || !IsDirectionKey(e.Key))
        {
            return;
        }

        e.Handled = true;
        if (_pressedKeys.Remove(e.Key))
        {
            ApplyKeyboardState();
        }
    }

    private bool IsKeyboardTarget()
    {
        if (!IsEnabled || !IsKeyboardFocusWithin)
        {
            return false;
        }

        return Keyboard.FocusedElement is not TextBoxBase and not PasswordBox;
    }

    private static bool IsDirectionKey(Key key) => key is Key.Up or Key.Down or Key.Left or Key.Right;

    private void ApplyKeyboardState()
    {
        if (_isMouseActive)
        {
            return;
        }

        Point vector = GetKeyboardVector();
        SetVector(new Point(vector.X * MaxDistance, vector.Y * MaxDistance));
    }

    private Point GetKeyboardVector()
    {
        if (_pressedKeys.Count > 2 ||
            (_pressedKeys.Contains(Key.Up) && _pressedKeys.Contains(Key.Down)) ||
            (_pressedKeys.Contains(Key.Left) && _pressedKeys.Contains(Key.Right)))
        {
            return new Point();
        }

        double x = (_pressedKeys.Contains(Key.Right) ? 1d : 0d) - (_pressedKeys.Contains(Key.Left) ? 1d : 0d);
        double y = (_pressedKeys.Contains(Key.Down) ? 1d : 0d) - (_pressedKeys.Contains(Key.Up) ? 1d : 0d);
        double length = Math.Sqrt(x * x + y * y);
        return length > 0d ? new Point(x / length, y / length) : new Point();
    }

    private Point GetVectorFromMousePosition(Point position)
    {
        double center = PadDiameter / 2d;
        double x = position.X - center;
        double y = position.Y - center;
        double distance = Math.Sqrt(x * x + y * y);
        if (distance < DeadZone)
        {
            return new Point();
        }

        if (distance > MaxDistance && distance > 0d)
        {
            x *= MaxDistance / distance;
            y *= MaxDistance / distance;
        }

        return new Point(x, y);
    }

    private void SetVector(Point vector, bool instant = false)
    {
        _vector = vector;
        double maxDistance = MaxDistance;
        double normalizedX = maxDistance > 0d ? Math.Max(-1d, Math.Min(1d, vector.X / maxDistance)) : 0d;
        double normalizedY = maxDistance > 0d ? Math.Max(-1d, Math.Min(1d, vector.Y / maxDistance)) : 0d;
        Moved?.Invoke(this, new JoystickMoveEventArgs(normalizedX, normalizedY));
        UpdateVisualState(instant);
    }

    private void Reset()
    {
        _pressedKeys.Clear();
        if (_isMouseDragging)
        {
            joystickSurface.ReleaseMouseCapture();
        }

        _isMouseDragging = false;
        _isMouseActive = false;
        SetVector(new Point(), true);
    }

    private void RefreshLayout()
    {
        if (!IsInitialized)
        {
            return;
        }

        joystickSurface.Width = PadDiameter;
        joystickSurface.Height = PadDiameter;
        UpdateArcPaths();
        UpdateArrowLayout();
        UpdatePadLayoutMetrics();
        UpdateVisualState(true);
    }

    private void UpdatePadLayoutMetrics()
    {
        double innerDiameter = InnerCircleRadius * 2d;
        ellipseInnerGuide.Width = innerDiameter;
        ellipseInnerGuide.Height = innerDiameter;
        Canvas.SetLeft(ellipseInnerGuide, (PadDiameter - innerDiameter) / 2d);
        Canvas.SetTop(ellipseInnerGuide, (PadDiameter - innerDiameter) / 2d);

        double knobDiameter = KnobRadius * 2d;
        gridKnob.Width = knobDiameter;
        gridKnob.Height = knobDiameter;
    }

    private void UpdateArcPaths()
    {
        double center = PadDiameter / 2d;
        double outerRadius = center - 2d;
        double innerRadius = InnerCircleRadius + 4d;
        const double gap = 0.02d;
        const double halfAngle = Math.PI / 4d;
        double[] centerAngles = [-Math.PI / 2d, 0d, Math.PI / 2d, Math.PI];
        Path[] paths = [pathArcUp, pathArcRight, pathArcDown, pathArcLeft];

        for (int index = 0; index < paths.Length; index++)
        {
            double startAngle = centerAngles[index] - halfAngle + gap;
            double endAngle = centerAngles[index] + halfAngle - gap;
            Point outerStart = PointOnCircle(center, outerRadius, startAngle);
            Point outerEnd = PointOnCircle(center, outerRadius, endAngle);
            Point innerEnd = PointOnCircle(center, innerRadius, endAngle);
            Point innerStart = PointOnCircle(center, innerRadius, startAngle);
            string data = string.Format(
                CultureInfo.InvariantCulture,
                "M {0} {1} A {2} {2} 0 0 1 {3} {4} L {5} {6} A {7} {7} 0 0 0 {8} {9} Z",
                outerStart.X,
                outerStart.Y,
                outerRadius,
                outerEnd.X,
                outerEnd.Y,
                innerEnd.X,
                innerEnd.Y,
                innerRadius,
                innerStart.X,
                innerStart.Y);
            paths[index].Data = Geometry.Parse(data);
        }
    }

    private static Point PointOnCircle(double center, double radius, double angle) => new(center + radius * Math.Cos(angle), center + radius * Math.Sin(angle));

    private void UpdateArrowLayout()
    {
        double iconSize = PadDiameter * 0.14d;
        double outerRadius = PadDiameter / 2d - 2d;
        double innerRadius = InnerCircleRadius + 4d;
        double iconCenterRadius = (outerRadius + innerRadius) / 2d;
        double offset = PadDiameter / 2d - iconCenterRadius - iconSize / 2d;
        double center = PadDiameter / 2d;

        PlaceArrow(viewboxArrowUp, center - iconSize / 2d, offset, iconSize);
        PlaceArrow(viewboxArrowDown, center - iconSize / 2d, PadDiameter - offset - iconSize, iconSize);
        PlaceArrow(viewboxArrowLeft, offset, center - iconSize / 2d, iconSize);
        PlaceArrow(viewboxArrowRight, PadDiameter - offset - iconSize, center - iconSize / 2d, iconSize);
    }

    private static void PlaceArrow(Viewbox arrow, double left, double top, double size)
    {
        Canvas.SetLeft(arrow, left);
        Canvas.SetTop(arrow, top);
        arrow.Width = size;
        arrow.Height = size;
    }

    private void UpdateVisualState(bool instant)
    {
        JoyStickDirection direction = GetDirectionFromVector(_vector);
        CurrentDirection = direction;

        polyArrowUp.Stroke = polyArrowDown.Stroke = polyArrowLeft.Stroke = polyArrowRight.Stroke = ArrowIdleBrush;
        pathArcUp.Opacity = pathArcDown.Opacity = pathArcLeft.Opacity = pathArcRight.Opacity = 1d;

        switch (direction)
        {
            case JoyStickDirection.Up:
                SetActiveDirection(polyArrowUp, pathArcUp);
                break;

            case JoyStickDirection.UpRight:
                SetActiveDirection(polyArrowUp, pathArcUp, polyArrowRight, pathArcRight);
                break;

            case JoyStickDirection.Right:
                SetActiveDirection(polyArrowRight, pathArcRight);
                break;

            case JoyStickDirection.DownRight:
                SetActiveDirection(polyArrowDown, pathArcDown, polyArrowRight, pathArcRight);
                break;

            case JoyStickDirection.Down:
                SetActiveDirection(polyArrowDown, pathArcDown);
                break;

            case JoyStickDirection.DownLeft:
                SetActiveDirection(polyArrowDown, pathArcDown, polyArrowLeft, pathArcLeft);
                break;

            case JoyStickDirection.Left:
                SetActiveDirection(polyArrowLeft, pathArcLeft);
                break;

            case JoyStickDirection.UpLeft:
                SetActiveDirection(polyArrowUp, pathArcUp, polyArrowLeft, pathArcLeft);
                break;
        }

        double knobDiameter = KnobRadius * 2d;
        double left = PadDiameter / 2d - knobDiameter / 2d + _vector.X;
        double top = PadDiameter / 2d - knobDiameter / 2d + _vector.Y;
        if (instant || _isMouseDragging)
        {
            gridKnob.BeginAnimation(Canvas.LeftProperty, null);
            gridKnob.BeginAnimation(Canvas.TopProperty, null);
            Canvas.SetLeft(gridKnob, left);
            Canvas.SetTop(gridKnob, top);
        }
        else
        {
            var easing = new QuadraticEase { EasingMode = EasingMode.EaseOut };
            gridKnob.BeginAnimation(Canvas.LeftProperty, new DoubleAnimation(left, TimeSpan.FromMilliseconds(150d)) { EasingFunction = easing });
            gridKnob.BeginAnimation(Canvas.TopProperty, new DoubleAnimation(top, TimeSpan.FromMilliseconds(150d)) { EasingFunction = easing });
        }

        ellipseKnob.Opacity = direction == JoyStickDirection.None ? 0.5d : 0.8d;
    }

    private void SetActiveDirection(Polyline arrow, Path arc, Polyline? secondaryArrow = null, Path? secondaryArc = null)
    {
        arrow.Stroke = ArrowActiveBrush;
        arc.Opacity = 0.5d;
        if (secondaryArrow != null && secondaryArc != null)
        {
            secondaryArrow.Stroke = ArrowActiveBrush;
            secondaryArc.Opacity = 0.5d;
        }
    }

    private JoyStickDirection GetDirectionFromVector(Point vector)
    {
        if (Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y) < DeadZone)
        {
            return JoyStickDirection.None;
        }

        double angle = (Math.Atan2(vector.Y, vector.X) * 180d / Math.PI + 360d) % 360d;
        return ((int)Math.Round(angle / 45d) % 8) switch
        {
            0 => JoyStickDirection.Right,
            1 => JoyStickDirection.DownRight,
            2 => JoyStickDirection.Down,
            3 => JoyStickDirection.DownLeft,
            4 => JoyStickDirection.Left,
            5 => JoyStickDirection.UpLeft,
            6 => JoyStickDirection.Up,
            7 => JoyStickDirection.UpRight,
            _ => JoyStickDirection.None,
        };
    }
}
