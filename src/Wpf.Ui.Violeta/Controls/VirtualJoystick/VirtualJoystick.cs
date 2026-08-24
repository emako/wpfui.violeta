using System;
using System.Collections.Generic;
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
[TemplatePart(Name = PartSurface, Type = typeof(Canvas))]
[TemplatePart(Name = PartArcUp, Type = typeof(Path))]
[TemplatePart(Name = PartArcRight, Type = typeof(Path))]
[TemplatePart(Name = PartArcDown, Type = typeof(Path))]
[TemplatePart(Name = PartArcLeft, Type = typeof(Path))]
[TemplatePart(Name = PartArrowUp, Type = typeof(Viewbox))]
[TemplatePart(Name = PartArrowDown, Type = typeof(Viewbox))]
[TemplatePart(Name = PartArrowLeft, Type = typeof(Viewbox))]
[TemplatePart(Name = PartArrowRight, Type = typeof(Viewbox))]
[TemplatePart(Name = PartArrowUpLine, Type = typeof(Polyline))]
[TemplatePart(Name = PartArrowDownLine, Type = typeof(Polyline))]
[TemplatePart(Name = PartArrowLeftLine, Type = typeof(Polyline))]
[TemplatePart(Name = PartArrowRightLine, Type = typeof(Polyline))]
[TemplatePart(Name = PartInnerGuide, Type = typeof(Ellipse))]
[TemplatePart(Name = PartKnob, Type = typeof(Grid))]
[TemplatePart(Name = PartKnobEllipse, Type = typeof(Ellipse))]
public class VirtualJoystick : Control
{
    private const string PartSurface = "PART_Surface";
    private const string PartArcUp = "PART_ArcUp";
    private const string PartArcRight = "PART_ArcRight";
    private const string PartArcDown = "PART_ArcDown";
    private const string PartArcLeft = "PART_ArcLeft";
    private const string PartArrowUp = "PART_ArrowUp";
    private const string PartArrowDown = "PART_ArrowDown";
    private const string PartArrowLeft = "PART_ArrowLeft";
    private const string PartArrowRight = "PART_ArrowRight";
    private const string PartArrowUpLine = "PART_ArrowUpLine";
    private const string PartArrowDownLine = "PART_ArrowDownLine";
    private const string PartArrowLeftLine = "PART_ArrowLeftLine";
    private const string PartArrowRightLine = "PART_ArrowRightLine";
    private const string PartInnerGuide = "PART_InnerGuide";
    private const string PartKnob = "PART_Knob";
    private const string PartKnobEllipse = "PART_KnobEllipse";

    public static readonly DependencyProperty PadDiameterProperty = DependencyProperty.Register(
        nameof(PadDiameter), typeof(double), typeof(VirtualJoystick),
        new FrameworkPropertyMetadata(100d, OnPadDiameterChanged, CoercePadDiameter));

    public static readonly DependencyProperty ArcPathBrushProperty = DependencyProperty.Register(
        nameof(ArcPathBrush), typeof(Brush), typeof(VirtualJoystick),
        new PropertyMetadata(null, OnVisualPropertyChanged));

    public static readonly DependencyProperty ArrowIdleBrushProperty = DependencyProperty.Register(
        nameof(ArrowIdleBrush), typeof(Brush), typeof(VirtualJoystick),
        new PropertyMetadata(null, OnVisualPropertyChanged));

    public static readonly DependencyProperty ArrowActiveBrushProperty = DependencyProperty.Register(
        nameof(ArrowActiveBrush), typeof(Brush), typeof(VirtualJoystick),
        new PropertyMetadata(null, OnVisualPropertyChanged));

    public static readonly DependencyProperty ArrowStrokeThicknessProperty = DependencyProperty.Register(
        nameof(ArrowStrokeThickness), typeof(double), typeof(VirtualJoystick),
        new FrameworkPropertyMetadata(2d, OnVisualPropertyChanged, CoerceArrowStrokeThickness));

    public static readonly DependencyProperty KnobBrushProperty = DependencyProperty.Register(
        nameof(KnobBrush), typeof(Brush), typeof(VirtualJoystick),
        new PropertyMetadata(null, OnVisualPropertyChanged));

    private static readonly DependencyPropertyKey CurrentDirectionPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(CurrentDirection), typeof(JoyStickDirection), typeof(VirtualJoystick),
        new PropertyMetadata(JoyStickDirection.None));

    public static readonly DependencyProperty DeadZoneFactorProperty = DependencyProperty.Register(
        nameof(DeadZoneFactor), typeof(double), typeof(VirtualJoystick), 
        new PropertyMetadata(0.15));



    public static readonly DependencyProperty CurrentDirectionProperty = CurrentDirectionPropertyKey.DependencyProperty;

    private readonly HashSet<Key> _pressedKeys = [];
    private Point _vector;
    private Window? _hostWindow;
    private bool _isMouseDragging;
    private bool _isMouseActive;
    private Canvas? _surface;
    private Path? _arcUp;
    private Path? _arcRight;
    private Path? _arcDown;
    private Path? _arcLeft;
    private Viewbox? _arrowUp;
    private Viewbox? _arrowDown;
    private Viewbox? _arrowLeft;
    private Viewbox? _arrowRight;
    private Polyline? _arrowUpLine;
    private Polyline? _arrowDownLine;
    private Polyline? _arrowLeftLine;
    private Polyline? _arrowRightLine;
    private Ellipse? _innerGuide;
    private Grid? _knob;
    private Ellipse? _knobEllipse;

    static VirtualJoystick()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(VirtualJoystick), new FrameworkPropertyMetadata(typeof(VirtualJoystick)));
    }

    public VirtualJoystick()
    {
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        IsVisibleChanged += OnIsVisibleChanged;
    }

    /// <summary>Gets or sets the diameter of the joystick surface.</summary>
    public double PadDiameter
    {
        get => (double)GetValue(PadDiameterProperty);
        set => SetValue(PadDiameterProperty, value);
    }

    /// <summary>Gets or sets the brush used by the directional arc segments.</summary>
    public Brush? ArcPathBrush
    {
        get => (Brush?)GetValue(ArcPathBrushProperty);
        set => SetValue(ArcPathBrushProperty, value);
    }

    /// <summary>Gets or sets the brush used by inactive direction arrows.</summary>
    public Brush? ArrowIdleBrush
    {
        get => (Brush?)GetValue(ArrowIdleBrushProperty);
        set => SetValue(ArrowIdleBrushProperty, value);
    }

    /// <summary>Gets or sets the brush used by active direction arrows.</summary>
    public Brush? ArrowActiveBrush
    {
        get => (Brush?)GetValue(ArrowActiveBrushProperty);
        set => SetValue(ArrowActiveBrushProperty, value);
    }

    /// <summary>Gets or sets the thickness of direction arrow strokes.</summary>
    public double ArrowStrokeThickness
    {
        get => (double)GetValue(ArrowStrokeThicknessProperty);
        set => SetValue(ArrowStrokeThicknessProperty, value);
    }

    /// <summary>Gets or sets the brush used by the draggable knob.</summary>
    public Brush? KnobBrush
    {
        get => (Brush?)GetValue(KnobBrushProperty);
        set => SetValue(KnobBrushProperty, value);
    }

    /// <summary>Gets the current discrete joystick direction.</summary>
    public JoyStickDirection CurrentDirection
    {
        get => (JoyStickDirection)GetValue(CurrentDirectionProperty);
        private set => SetValue(CurrentDirectionPropertyKey, value);
    }
    
    /// <summary>
    /// Sets the dead zone factor in percent for the joystick (default: 0.15)
    /// </summary>
    public double DeadZoneFactor
    {
        get { return (double)GetValue(DeadZoneFactorProperty); }
        set { SetValue(DeadZoneFactorProperty, value); }
    }

    /// <summary>Occurs when the joystick position changes.</summary>
    public event EventHandler<JoystickMoveEventArgs>? Moved;

    private double InnerCircleRadius => PadDiameter * 0.275d;
    private double KnobRadius => PadDiameter * 0.07d;
    private double MaxDistance => InnerCircleRadius - KnobRadius;
    private double DeadZone => MaxDistance * DeadZoneFactor;

    public override void OnApplyTemplate()
    {
        DetachSurfaceEvents();
        base.OnApplyTemplate();

        _surface = GetTemplateChild(PartSurface) as Canvas;
        _arcUp = GetTemplateChild(PartArcUp) as Path;
        _arcRight = GetTemplateChild(PartArcRight) as Path;
        _arcDown = GetTemplateChild(PartArcDown) as Path;
        _arcLeft = GetTemplateChild(PartArcLeft) as Path;
        _arrowUp = GetTemplateChild(PartArrowUp) as Viewbox;
        _arrowDown = GetTemplateChild(PartArrowDown) as Viewbox;
        _arrowLeft = GetTemplateChild(PartArrowLeft) as Viewbox;
        _arrowRight = GetTemplateChild(PartArrowRight) as Viewbox;
        _arrowUpLine = GetTemplateChild(PartArrowUpLine) as Polyline;
        _arrowDownLine = GetTemplateChild(PartArrowDownLine) as Polyline;
        _arrowLeftLine = GetTemplateChild(PartArrowLeftLine) as Polyline;
        _arrowRightLine = GetTemplateChild(PartArrowRightLine) as Polyline;
        _innerGuide = GetTemplateChild(PartInnerGuide) as Ellipse;
        _knob = GetTemplateChild(PartKnob) as Grid;
        _knobEllipse = GetTemplateChild(PartKnobEllipse) as Ellipse;

        if (_surface != null)
        {
            _surface.MouseLeftButtonDown += OnSurfaceMouseLeftButtonDown;
            _surface.MouseLeftButtonUp += OnSurfaceMouseLeftButtonUp;
            _surface.MouseMove += OnSurfaceMouseMove;
        }

        RefreshLayout();
    }

    private static void OnPadDiameterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((VirtualJoystick)d).RefreshLayout();
    private static void OnVisualPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((VirtualJoystick)d).UpdateVisualState(true);

    private static object CoercePadDiameter(DependencyObject d, object value)
    {
        double diameter = (double)value;
        return double.IsNaN(diameter) || double.IsInfinity(diameter) ? 100d : Math.Max(32d, Math.Min(1024d, diameter));
    }

    private static object CoerceArrowStrokeThickness(DependencyObject d, object value)
    {
        double thickness = (double)value;
        return double.IsNaN(thickness) || double.IsInfinity(thickness) ? 2d : Math.Max(0.1d, thickness);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _hostWindow = Window.GetWindow(this);
        if (_hostWindow != null)
        {
            _hostWindow.PreviewKeyDown += OnHostPreviewKeyDown;
            _hostWindow.PreviewKeyUp += OnHostPreviewKeyUp;
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        Reset();
        if (_hostWindow != null)
        {
            _hostWindow.PreviewKeyDown -= OnHostPreviewKeyDown;
            _hostWindow.PreviewKeyUp -= OnHostPreviewKeyUp;
            _hostWindow = null;
        }
    }

    private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (Visibility != Visibility.Visible)
        {
            Reset();
        }
    }

    private void OnSurfaceMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (!IsEnabled || _surface == null)
        {
            return;
        }

        Focus();
        _pressedKeys.Clear();
        _isMouseActive = true;
        _isMouseDragging = true;
        _surface.CaptureMouse();
        SetVector(GetVectorFromMousePosition(e.GetPosition(_surface)), true);
        e.Handled = true;
    }

    private void OnSurfaceMouseMove(object sender, MouseEventArgs e)
    {
        if (!_isMouseDragging || e.LeftButton != MouseButtonState.Pressed || _surface == null)
        {
            return;
        }

        SetVector(GetVectorFromMousePosition(e.GetPosition(_surface)), true);
    }

    private void OnSurfaceMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!_isMouseDragging)
        {
            return;
        }

        _isMouseDragging = false;
        _surface?.ReleaseMouseCapture();
        _isMouseActive = false;
        ApplyKeyboardState();
        e.Handled = true;
    }

    private void OnHostPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (_isMouseActive || !CanProcessKeyboardInput() || !IsDirectionKey(e.Key))
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
        if (_isMouseActive || !CanProcessKeyboardInput() || !IsDirectionKey(e.Key))
        {
            return;
        }

        e.Handled = true;
        if (_pressedKeys.Remove(e.Key))
        {
            ApplyKeyboardState();
        }
    }

    private bool CanProcessKeyboardInput() => IsEnabled && IsKeyboardFocusWithin && Keyboard.FocusedElement is not System.Windows.Controls.Primitives.TextBoxBase and not PasswordBox;
    private static bool IsDirectionKey(Key key) => key is Key.Up or Key.Down or Key.Left or Key.Right;

    private void ApplyKeyboardState()
    {
        if (!_isMouseActive)
        {
            Point vector = GetKeyboardVector();
            SetVector(new Point(vector.X * MaxDistance, vector.Y * MaxDistance));
        }
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
            _surface?.ReleaseMouseCapture();
        }

        _isMouseDragging = false;
        _isMouseActive = false;
        SetVector(new Point(), true);
    }

    private void RefreshLayout()
    {
        if (_surface == null)
        {
            return;
        }

        _surface.Width = PadDiameter;
        _surface.Height = PadDiameter;
        UpdateArcPaths();
        UpdateArrowLayout();
        UpdatePadLayoutMetrics();
        UpdateVisualState(true);
    }

    private void UpdatePadLayoutMetrics()
    {
        if (_innerGuide == null || _knob == null)
        {
            return;
        }

        double innerDiameter = InnerCircleRadius * 2d;
        _innerGuide.Width = innerDiameter;
        _innerGuide.Height = innerDiameter;
        Canvas.SetLeft(_innerGuide, (PadDiameter - innerDiameter) / 2d);
        Canvas.SetTop(_innerGuide, (PadDiameter - innerDiameter) / 2d);
        double knobDiameter = KnobRadius * 2d;
        _knob.Width = knobDiameter;
        _knob.Height = knobDiameter;
    }

    private void UpdateArcPaths()
    {
        if (_arcUp == null || _arcRight == null || _arcDown == null || _arcLeft == null)
        {
            return;
        }

        double center = PadDiameter / 2d;
        double outerRadius = center - 2d;
        double innerRadius = InnerCircleRadius + 4d;
        const double gap = 0.02d;
        const double halfAngle = Math.PI / 4d;
        double[] centerAngles = [-Math.PI / 2d, 0d, Math.PI / 2d, Math.PI];
        Path[] paths = [_arcUp, _arcRight, _arcDown, _arcLeft];

        for (int index = 0; index < paths.Length; index++)
        {
            double startAngle = centerAngles[index] - halfAngle + gap;
            double endAngle = centerAngles[index] + halfAngle - gap;
            Point outerStart = PointOnCircle(center, outerRadius, startAngle);
            Point outerEnd = PointOnCircle(center, outerRadius, endAngle);
            Point innerEnd = PointOnCircle(center, innerRadius, endAngle);
            Point innerStart = PointOnCircle(center, innerRadius, startAngle);
            paths[index].Data = Geometry.Parse(string.Format(CultureInfo.InvariantCulture,
                "M {0} {1} A {2} {2} 0 0 1 {3} {4} L {5} {6} A {7} {7} 0 0 0 {8} {9} Z",
                outerStart.X, outerStart.Y, outerRadius, outerEnd.X, outerEnd.Y,
                innerEnd.X, innerEnd.Y, innerRadius, innerStart.X, innerStart.Y));
        }
    }

    private static Point PointOnCircle(double center, double radius, double angle) => new(center + radius * Math.Cos(angle), center + radius * Math.Sin(angle));

    private void UpdateArrowLayout()
    {
        if (_arrowUp == null || _arrowDown == null || _arrowLeft == null || _arrowRight == null)
        {
            return;
        }

        double size = PadDiameter * 0.14d;
        double outerRadius = PadDiameter / 2d - 2d;
        double innerRadius = InnerCircleRadius + 4d;
        double offset = PadDiameter / 2d - (outerRadius + innerRadius) / 2d - size / 2d;
        double center = PadDiameter / 2d;
        PlaceArrow(_arrowUp, center - size / 2d, offset, size);
        PlaceArrow(_arrowDown, center - size / 2d, PadDiameter - offset - size, size);
        PlaceArrow(_arrowLeft, offset, center - size / 2d, size);
        PlaceArrow(_arrowRight, PadDiameter - offset - size, center - size / 2d, size);
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
        if (_arcUp == null || _arcRight == null || _arcDown == null || _arcLeft == null ||
            _arrowUpLine == null || _arrowDownLine == null || _arrowLeftLine == null || _arrowRightLine == null ||
            _knob == null || _knobEllipse == null)
        {
            return;
        }

        JoyStickDirection direction = GetDirectionFromVector(_vector);
        CurrentDirection = direction;
        _arrowUpLine.Stroke = _arrowDownLine.Stroke = _arrowLeftLine.Stroke = _arrowRightLine.Stroke = ArrowIdleBrush;
        _arcUp.Opacity = _arcDown.Opacity = _arcLeft.Opacity = _arcRight.Opacity = 1d;

        switch (direction)
        {
            case JoyStickDirection.Up: SetActiveDirection(_arrowUpLine, _arcUp); break;
            case JoyStickDirection.UpRight: SetActiveDirection(_arrowUpLine, _arcUp, _arrowRightLine, _arcRight); break;
            case JoyStickDirection.Right: SetActiveDirection(_arrowRightLine, _arcRight); break;
            case JoyStickDirection.DownRight: SetActiveDirection(_arrowDownLine, _arcDown, _arrowRightLine, _arcRight); break;
            case JoyStickDirection.Down: SetActiveDirection(_arrowDownLine, _arcDown); break;
            case JoyStickDirection.DownLeft: SetActiveDirection(_arrowDownLine, _arcDown, _arrowLeftLine, _arcLeft); break;
            case JoyStickDirection.Left: SetActiveDirection(_arrowLeftLine, _arcLeft); break;
            case JoyStickDirection.UpLeft: SetActiveDirection(_arrowUpLine, _arcUp, _arrowLeftLine, _arcLeft); break;
        }

        double knobDiameter = KnobRadius * 2d;
        double left = PadDiameter / 2d - knobDiameter / 2d + _vector.X;
        double top = PadDiameter / 2d - knobDiameter / 2d + _vector.Y;
        if (instant || _isMouseDragging)
        {
            _knob.BeginAnimation(Canvas.LeftProperty, null);
            _knob.BeginAnimation(Canvas.TopProperty, null);
            Canvas.SetLeft(_knob, left);
            Canvas.SetTop(_knob, top);
        }
        else
        {
            var easing = new QuadraticEase { EasingMode = EasingMode.EaseOut };
            _knob.BeginAnimation(Canvas.LeftProperty, new DoubleAnimation(left, TimeSpan.FromMilliseconds(150d)) { EasingFunction = easing });
            _knob.BeginAnimation(Canvas.TopProperty, new DoubleAnimation(top, TimeSpan.FromMilliseconds(150d)) { EasingFunction = easing });
        }

        _knobEllipse.Opacity = direction == JoyStickDirection.None ? 0.5d : 0.8d;
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

        return ((int)Math.Round(((Math.Atan2(vector.Y, vector.X) * 180d / Math.PI + 360d) % 360d) / 45d) % 8) switch
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

    private void DetachSurfaceEvents()
    {
        if (_surface != null)
        {
            _surface.MouseLeftButtonDown -= OnSurfaceMouseLeftButtonDown;
            _surface.MouseLeftButtonUp -= OnSurfaceMouseLeftButtonUp;
            _surface.MouseMove -= OnSurfaceMouseMove;
        }
    }
}
