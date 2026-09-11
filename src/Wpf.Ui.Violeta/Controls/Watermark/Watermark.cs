using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Tiled watermark overlay that renders a mark (text, geometry, or UI element)
/// across its background while hosting arbitrary content.
/// Ported from HandyControl's Watermark control.
/// </summary>
[TemplatePart(Name = PART_Root, Type = typeof(Border))]
[ContentProperty(nameof(Content))]
public class Watermark : Control
{
    public const string PART_Root = "PART_Root";

    private Border? _borderRoot;
    private DrawingBrush? _brush;

    static Watermark()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(Watermark),
            new FrameworkPropertyMetadata(typeof(Watermark)));
    }

    #region Angle

    public static readonly DependencyProperty AngleProperty =
        DependencyProperty.Register(
            nameof(Angle),
            typeof(double),
            typeof(Watermark),
            new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Rotation angle (degrees) applied to the tiled watermark brush.</summary>
    public double Angle
    {
        get => (double)GetValue(AngleProperty);
        set => SetValue(AngleProperty, value);
    }

    #endregion Angle

    #region Content

    public static readonly DependencyProperty ContentProperty =
        DependencyProperty.Register(
            nameof(Content),
            typeof(object),
            typeof(Watermark),
            new PropertyMetadata(null));

    /// <summary>Foreground content hosted above the watermark.</summary>
    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    #endregion Content

    #region Mark

    public static readonly DependencyProperty MarkProperty =
        DependencyProperty.Register(
            nameof(Mark),
            typeof(object),
            typeof(Watermark),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>
    /// Watermark mark: a <see cref="string"/>, <see cref="Geometry"/>, or any UI content.
    /// </summary>
    public object? Mark
    {
        get => GetValue(MarkProperty);
        set => SetValue(MarkProperty, value);
    }

    #endregion Mark

    #region MarkWidth / MarkHeight

    public static readonly DependencyProperty MarkWidthProperty =
        DependencyProperty.Register(
            nameof(MarkWidth),
            typeof(double),
            typeof(Watermark),
            new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Fixed mark width when <see cref="AutoSizeEnabled"/> is <see langword="false"/>, or Path width for geometry marks.</summary>
    public double MarkWidth
    {
        get => (double)GetValue(MarkWidthProperty);
        set => SetValue(MarkWidthProperty, value);
    }

    public static readonly DependencyProperty MarkHeightProperty =
        DependencyProperty.Register(
            nameof(MarkHeight),
            typeof(double),
            typeof(Watermark),
            new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Fixed mark height when <see cref="AutoSizeEnabled"/> is <see langword="false"/>, or Path height for geometry marks.</summary>
    public double MarkHeight
    {
        get => (double)GetValue(MarkHeightProperty);
        set => SetValue(MarkHeightProperty, value);
    }

    #endregion MarkWidth / MarkHeight

    #region MarkBrush

    public static readonly DependencyProperty MarkBrushProperty =
        DependencyProperty.Register(
            nameof(MarkBrush),
            typeof(Brush),
            typeof(Watermark),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Brush used for text / geometry marks.</summary>
    public Brush? MarkBrush
    {
        get => (Brush?)GetValue(MarkBrushProperty);
        set => SetValue(MarkBrushProperty, value);
    }

    #endregion MarkBrush

    #region AutoSizeEnabled

    public static readonly DependencyProperty AutoSizeEnabledProperty =
        DependencyProperty.Register(
            nameof(AutoSizeEnabled),
            typeof(bool),
            typeof(Watermark),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>
    /// When <see langword="true"/>, tile size is measured from the mark content;
    /// otherwise <see cref="MarkWidth"/> / <see cref="MarkHeight"/> are used.
    /// </summary>
    public bool AutoSizeEnabled
    {
        get => (bool)GetValue(AutoSizeEnabledProperty);
        set => SetValue(AutoSizeEnabledProperty, value);
    }

    #endregion AutoSizeEnabled

    #region MarkMargin

    public static readonly DependencyProperty MarkMarginProperty =
        DependencyProperty.Register(
            nameof(MarkMargin),
            typeof(Thickness),
            typeof(Watermark),
            new FrameworkPropertyMetadata(default(Thickness), FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Padding around each mark tile.</summary>
    public Thickness MarkMargin
    {
        get => (Thickness)GetValue(MarkMarginProperty);
        set => SetValue(MarkMarginProperty, value);
    }

    #endregion MarkMargin

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _borderRoot = GetTemplateChild(PART_Root) as Border;
        EnsureBrush();
    }

    protected override void OnRender(DrawingContext drawingContext) => EnsureBrush();

    private void EnsureBrush()
    {
        var presenter = new ContentPresenter();

        if (Mark is Geometry geometry)
        {
            presenter.Content = new Path
            {
                Width = MarkWidth,
                Height = MarkHeight,
                Fill = MarkBrush,
                Stretch = Stretch.Uniform,
                Data = geometry
            };
        }
        else if (Mark is string str)
        {
            presenter.Content = new TextBlock
            {
                Text = str,
                FontSize = FontSize,
                FontFamily = FontFamily,
                FontWeight = FontWeight,
                FontStyle = FontStyle,
                Foreground = MarkBrush
            };
        }
        else
        {
            presenter.Content = Mark;
        }

        Size markSize;
        if (AutoSizeEnabled)
        {
            presenter.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            markSize = presenter.DesiredSize;
        }
        else
        {
            markSize = new Size(MarkWidth, MarkHeight);
        }

        _brush = new DrawingBrush
        {
            ViewportUnits = BrushMappingMode.Absolute,
            Stretch = Stretch.Uniform,
            TileMode = TileMode.Tile,
            Transform = new RotateTransform(Angle),
            Drawing = new GeometryDrawing
            {
                Brush = new VisualBrush(new Border
                {
                    Background = Brushes.Transparent,
                    Padding = MarkMargin,
                    Child = presenter
                }),
                Geometry = new RectangleGeometry(new Rect(markSize))
            },
            Viewport = new Rect(markSize)
        };

        RenderOptions.SetCacheInvalidationThresholdMinimum(_brush, 0.5);
        RenderOptions.SetCacheInvalidationThresholdMaximum(_brush, 2);
        RenderOptions.SetCachingHint(_brush, CachingHint.Cache);

        if (_borderRoot != null)
            _borderRoot.Background = _brush;
    }
}
