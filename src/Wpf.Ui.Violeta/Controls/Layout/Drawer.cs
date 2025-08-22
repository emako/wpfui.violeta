using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Wpf.Ui.Controls;

/// <summary>
/// A WPF ContentControl that displays a sliding panel (Drawer) from any edge of its container.
/// Supports animated open/close, placement on any side (Left, Right, Top, Bottom), and optional automatic ZIndex management.
/// </summary>

[TemplatePart(Name = "PART_DrawerContainer", Type = typeof(Border))]
public partial class Drawer : ContentControl
{
    private Border? _container;

    public event EventHandler? Opened;

    public event EventHandler? Closed;

    public TranslateTransform TranslateTransform => (TranslateTransform)RenderTransform;

    static Drawer()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Drawer), new FrameworkPropertyMetadata(typeof(Drawer)));
    }

    public Drawer()
    {
        ResourceDictionary dict = new()
        {
            Source = new Uri("pack://application:,,,/Wpf.Ui.Violeta;component/Controls/Layout/Drawer.xaml", UriKind.Absolute)
        };
        if (!Resources.MergedDictionaries.Contains(dict))
            Resources.MergedDictionaries.Add(dict);

        if (RenderTransform as TranslateTransform is null)
            RenderTransform = new TranslateTransform();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnLoaded;
        ApplyPlacement();
        ToggleDrawer(IsOpen, false);
    }

    public static readonly DependencyProperty IsOpenProperty =
        DependencyProperty.Register(nameof(IsOpen), typeof(bool), typeof(Drawer), new(false, OnIsOpenChanged));

    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var drawer = (Drawer)d;
        bool isOpen = (bool)e.NewValue;
        drawer.ToggleDrawer(isOpen, true);
        if (isOpen)
            drawer.Opened?.Invoke(drawer, EventArgs.Empty);
        else
            drawer.Closed?.Invoke(drawer, EventArgs.Empty);
    }

    /// <summary>
    /// Identifies the Duration dependency property.
    /// Controls the animation duration (in milliseconds) for opening/closing the Drawer. Default is 300ms.
    /// </summary>
    public static readonly DependencyProperty DurationProperty =
        DependencyProperty.Register(nameof(Duration), typeof(int), typeof(Drawer), new(300));

    /// <summary>
    /// Gets or sets the animation duration (in milliseconds) for opening/closing the Drawer.
    /// </summary>
    public int Duration
    {
        get => (int)GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }

    /// <summary>
    /// Identifies the AutoZIndex dependency property.
    /// When true (default), Drawer will automatically set its ZIndex to int.MaxValue when opened,
    /// ensuring it appears above other sibling elements in the same Panel.
    /// </summary>
    public static readonly DependencyProperty AutoZIndexProperty
        = DependencyProperty.Register(nameof(AutoZIndex), typeof(bool), typeof(Drawer), new(true));

    /// <summary>
    /// Gets or sets whether the Drawer will automatically set its ZIndex to the topmost value (int.MaxValue) when opened.
    /// Set to false if you want to control ZIndex manually.
    /// </summary>
    public bool AutoZIndex
    {
        get => (bool)GetValue(AutoZIndexProperty);
        set => SetValue(AutoZIndexProperty, value);
    }

    public static readonly DependencyProperty PlacementProperty
        = DependencyProperty.Register(nameof(Placement), typeof(DrawerPlacement), typeof(Drawer), new(DrawerPlacement.Left, OnPlacementChanged));

    public DrawerPlacement Placement
    {
        get => (DrawerPlacement)GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }

    private static void OnPlacementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var drawer = (Drawer)d;
        drawer.ApplyPlacement();
    }

    private void ApplyPlacement()
    {
        if (_container == null)
            return;

        switch (Placement)
        {
            case DrawerPlacement.Left:
                _container.HorizontalAlignment = HorizontalAlignment.Left;
                _container.VerticalAlignment = VerticalAlignment.Stretch;
                break;

            case DrawerPlacement.Right:
                _container.HorizontalAlignment = HorizontalAlignment.Right;
                _container.VerticalAlignment = VerticalAlignment.Stretch;
                break;

            case DrawerPlacement.Top:
                _container.VerticalAlignment = VerticalAlignment.Top;
                _container.HorizontalAlignment = HorizontalAlignment.Stretch;
                break;

            case DrawerPlacement.Bottom:
                _container.VerticalAlignment = VerticalAlignment.Bottom;
                _container.HorizontalAlignment = HorizontalAlignment.Stretch;
                break;
        }
    }

    private void ToggleDrawer(bool isOpen, bool animated)
    {
        Point target = GetTargetPosition(isOpen);

        // Automatically set ZIndex to topmost if enabled and opening
        if (isOpen && AutoZIndex)
        {
            Panel.SetZIndex(this, int.MaxValue);
        }

        if (animated)
        {
            TimeSpan duration = TimeSpan.FromMilliseconds(Duration);
            DoubleAnimation animX = new()
            {
                To = target.X,
                Duration = duration,
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            DoubleAnimation animY = new()
            {
                To = target.Y,
                Duration = duration,
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            TranslateTransform.BeginAnimation(TranslateTransform.XProperty, animX);
            TranslateTransform.BeginAnimation(TranslateTransform.YProperty, animY);
        }
        else
        {
            TranslateTransform.BeginAnimation(TranslateTransform.XProperty, null);
            TranslateTransform.BeginAnimation(TranslateTransform.YProperty, null);
            TranslateTransform.X = target.X;
            TranslateTransform.Y = target.Y;
        }
    }

    protected Point GetTargetPosition(bool isOpen)
    {
        // Attention Point
        // When the Drawer is not loaded, ActualWidth and ActualHeight will be `0d`.
        // If the Drawer size is Stretch, Width and Height will be `NaN`.

        double sizeX = double.IsNaN(Width) ? ActualWidth : Width;
        double sizeY = double.IsNaN(Height) ? ActualHeight : Height;

        double targetX = Placement switch
        {
            DrawerPlacement.Left => isOpen ? 0 : -sizeX,
            DrawerPlacement.Right => isOpen ? 0 : sizeX,
            _ => 0d,
        };

        double targetY = Placement switch
        {
            DrawerPlacement.Top => isOpen ? 0 : -sizeY,
            DrawerPlacement.Bottom => isOpen ? 0 : sizeY,
            _ => 0d,
        };

        return new Point(targetX, targetY);
    }

    public void Show(bool animated = true)
    {
        if (animated)
            IsOpen = true; // This will trigger the animation via the IsOpen property change handler
        else
            SetCurrentValue(IsOpenProperty, true);
    }

    public void Hide(bool animated = true)
    {
        if (animated)
            IsOpen = false; // This will trigger the animation via the IsOpen property change handler
        else
            SetCurrentValue(IsOpenProperty, false);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _container = GetTemplateChild("PART_DrawerContainer") as Border;
        ApplyPlacement();
    }
}

/// <summary>
/// Specifies the position from which the <see cref="Drawer"/> control will appear and how it is laid out.
/// Determines the edge of the container where the Drawer is anchored and slides in/out.
/// </summary>
public enum DrawerPlacement
{
    /// <summary>
    /// Drawer is anchored to the left edge and slides horizontally from the left.
    /// </summary>
    Left,

    /// <summary>
    /// Drawer is anchored to the right edge and slides horizontally from the right.
    /// </summary>
    Right,

    /// <summary>
    /// Drawer is anchored to the top edge and slides vertically from the top.
    /// </summary>
    Top,

    /// <summary>
    /// Drawer is anchored to the bottom edge and slides vertically from the bottom.
    /// </summary>
    Bottom,
}
