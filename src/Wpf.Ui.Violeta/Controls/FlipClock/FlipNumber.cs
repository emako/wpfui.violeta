using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using Wpf.Ui.Appearance;
using Wpf.Ui.Violeta.Appearance;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// A single flip-card digit rendered with WPF 3D. Used by <see cref="FlipClock"/>.
/// Ported from HandyControl's FlipNumber, with theme-aware brush updates.
/// </summary>
public class FlipNumber : Viewport3D
{
    private bool _isLoaded;
    private bool _isAnimating;

    private TextBlock? _page1TextDown;
    private TextBlock? _page2TextUp;
    private TextBlock? _page2TextDown;
    private TextBlock? _page3TextUp;

    private ContainerUIElement3D? _page1;
    private ContainerUIElement3D? _page2;
    private ContainerUIElement3D? _page3;

    private readonly AxisAngleRotation3D _pageRotation3D;
    private readonly DoubleAnimation _animation;
    private readonly List<Border> _borders = [];
    private readonly List<TextBlock> _textBlocks = [];

    public FlipNumber()
    {
        Children.Add(new ModelVisual3D { Content = new DirectionalLight() });

        _pageRotation3D = new AxisAngleRotation3D
        {
            Angle = 0,
            Axis = new Vector3D(1, 0, 0)
        };

        _animation = new DoubleAnimation(0, 180, new Duration(TimeSpan.FromSeconds(0.8)))
        {
            FillBehavior = FillBehavior.Stop
        };
        _animation.Completed += OnAnimationCompleted;

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    #region CornerRadius

    public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
        nameof(CornerRadius),
        typeof(CornerRadius),
        typeof(FlipNumber),
        new PropertyMetadata(new CornerRadius(4)));

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    #endregion

    #region Background

    public static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register(
        nameof(Background),
        typeof(Brush),
        typeof(FlipNumber),
        new PropertyMetadata(null, OnAppearanceChanged));

    public Brush? Background
    {
        get => (Brush?)GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    #endregion

    #region Foreground

    public static readonly DependencyProperty ForegroundProperty = DependencyProperty.Register(
        nameof(Foreground),
        typeof(Brush),
        typeof(FlipNumber),
        new PropertyMetadata(null, OnAppearanceChanged));

    public Brush? Foreground
    {
        get => (Brush?)GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    #endregion

    #region FontSize

    public static readonly DependencyProperty FontSizeProperty = DependencyProperty.Register(
        nameof(FontSize),
        typeof(double),
        typeof(FlipNumber),
        new PropertyMetadata(70.0, OnAppearanceChanged));

    [TypeConverter(typeof(FontSizeConverter))]
    public double FontSize
    {
        get => (double)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    #endregion

    #region Number

    public static readonly DependencyProperty NumberProperty = DependencyProperty.Register(
        nameof(Number),
        typeof(int),
        typeof(FlipNumber),
        new PropertyMetadata(0, OnNumberChanged));

    public int Number
    {
        get => (int)GetValue(NumberProperty);
        set => SetValue(NumberProperty, value);
    }

    private static void OnNumberChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        ((FlipNumber)d).HandleNumberChanged();

    #endregion

    private static void OnAppearanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        ((FlipNumber)d).ApplyAppearance();

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        ThemeManager.Changed -= OnThemeChanged;
        ThemeManager.Changed += OnThemeChanged;

        if (_isLoaded)
        {
            ApplyAppearance();
            return;
        }

        _isLoaded = true;
        InitNumber();
        if (_page2 is not null)
        {
            _page2.Transform = new RotateTransform3D(_pageRotation3D);
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e) =>
        ThemeManager.Changed -= OnThemeChanged;

    private void OnThemeChanged(ApplicationTheme theme, Color accent) =>
        Dispatcher.BeginInvoke(ApplyAppearance, DispatcherPriority.Loaded);

    private void ApplyAppearance()
    {
        foreach (var border in _borders)
        {
            border.Background = Background;
            border.CornerRadius = new CornerRadius(CornerRadius.TopLeft, CornerRadius.TopRight, 0, 0);
        }

        foreach (var text in _textBlocks)
        {
            text.Foreground = Foreground;
            text.FontSize = FontSize;
        }
    }

    private void OnAnimationCompleted(object? sender, EventArgs e)
    {
        _isAnimating = false;
        UpdateNumber();
    }

    private void InitNumber()
    {
        _borders.Clear();
        _textBlocks.Clear();

        _page1 = new ContainerUIElement3D();
        var next = Number > 8 ? 0 : Number + 1;
        _page1.Children.Add(CreateNumber(next, false, out _page1TextDown));

        _page2 = new ContainerUIElement3D();
        _page2.Children.Add(CreateNumber(next, true, out _page2TextUp));
        _page2.Children.Add(CreateNumber(Number, false, out _page2TextDown));

        _page3 = new ContainerUIElement3D();
        _page3.Children.Add(CreateNumber(Number, true, out _page3TextUp));
        _page3.Transform = new RotateTransform3D(new AxisAngleRotation3D
        {
            Angle = 180,
            Axis = new Vector3D(1, 0, 0)
        });

        Children.Add(new ContainerUIElement3D
        {
            Children = { _page1, _page2, _page3 }
        });
    }

    private bool CheckReady() =>
        _page1TextDown is not null
        && _page2TextUp is not null
        && _page2TextDown is not null
        && _page3TextUp is not null;

    private void HandleNumberChanged()
    {
        if (!CheckReady())
        {
            return;
        }

        InitNewNumber();

        if (_isAnimating)
        {
            _isAnimating = false;
            UpdateNumber();
            return;
        }

        _isAnimating = true;
        _pageRotation3D.BeginAnimation(AxisAngleRotation3D.AngleProperty, _animation);
    }

    private void InitNewNumber()
    {
        var text = Number.ToString();
        _page1TextDown!.Text = text;
        _page2TextUp!.Text = text;
    }

    private void UpdateNumber()
    {
        _pageRotation3D.BeginAnimation(AxisAngleRotation3D.AngleProperty, null);
        _pageRotation3D.Angle = 0;

        var text = Number.ToString();
        _page2TextDown!.Text = text;
        _page3TextUp!.Text = text;
        _isAnimating = false;
    }

    private Viewport2DVisual3D CreateNumber(int num, bool isUp, out TextBlock textBlock)
    {
        int flag;
        var rotateTransform = new RotateTransform();

        if (isUp)
        {
            flag = -1;
            rotateTransform.Angle = 180;
        }
        else
        {
            flag = 1;
        }

        var halfWidth = ActualWidth / 2;
        var quarterWidth = ActualWidth / 4;
        var quarterHeight = ActualHeight / 4;

        var material = new DiffuseMaterial();
        Viewport2DVisual3D.SetIsVisualHostMaterial(material, true);

        textBlock = new TextBlock
        {
            RenderTransformOrigin = new Point(0.5, 0.5),
            Foreground = Foreground,
            FontSize = FontSize,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center,
            Text = num.ToString(),
            RenderTransform = rotateTransform,
            Margin = new Thickness(0, 0, 0, -quarterHeight),
            FontFamily = new FontFamily("Consolas")
        };

        var border = new Border
        {
            ClipToBounds = true,
            CornerRadius = new CornerRadius(CornerRadius.TopLeft, CornerRadius.TopRight, 0, 0),
            Background = Background,
            Width = halfWidth,
            Height = quarterHeight,
            Child = textBlock
        };

        _borders.Add(border);
        _textBlocks.Add(textBlock);

        var geometry = new MeshGeometry3D
        {
            Positions =
            [
                new(-quarterWidth * flag, quarterHeight, 0),
                new(-quarterWidth * flag, 0, 0),
                new(quarterWidth * flag, 0, 0),
                new(quarterWidth * flag, quarterHeight, 0)
            ],
            TriangleIndices = [0, 1, 2, 0, 2, 3],
            TextureCoordinates =
            [
                new(0, 0),
                new(0, 1),
                new(1, 1),
                new(1, 0)
            ]
        };

        return new Viewport2DVisual3D
        {
            Geometry = geometry,
            Visual = border,
            Material = material
        };
    }
}
