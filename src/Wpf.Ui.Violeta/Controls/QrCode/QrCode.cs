using Wpf.Ui.Violeta.Controls.Encoding;
using System.Windows;
using System.Windows.Media;
using System;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// WPF implementation of a Quick Response code (QR Code) with smooth borders and support for gradient brushes.
/// </summary>
public partial class QrCode : FrameworkElement
{
    #region DependencyProperties

    public static readonly DependencyProperty BackgroundProperty =
        DependencyProperty.Register(nameof(Background), typeof(Brush), typeof(QrCode),
            new FrameworkPropertyMetadata(Brushes.White, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ForegroundProperty =
        DependencyProperty.Register(nameof(Foreground), typeof(Brush), typeof(QrCode),
            new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(QrCode),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty SymbolCornerRatioProperty =
        DependencyProperty.Register(nameof(SymbolCornerRatio), typeof(double), typeof(QrCode),
            new FrameworkPropertyMetadata(0.5, OnLayoutPropertyChanged, CoerceSymbolCornerRatio));

    private static object CoerceSymbolCornerRatio(DependencyObject d, object baseValue)
    {
        var value = (double)baseValue;
        return value < 0.0 ? 0.0 : value > 0.5 ? 0.5 : value;
    }

    public static readonly DependencyProperty PaddingProperty =
        DependencyProperty.Register(nameof(Padding), typeof(Thickness), typeof(QrCode),
            new FrameworkPropertyMetadata(default(Thickness), OnLayoutPropertyChanged));

    public static readonly DependencyProperty IsQuietZoneEnabledProperty =
        DependencyProperty.Register(nameof(IsQuietZoneEnabled), typeof(bool), typeof(QrCode),
            new FrameworkPropertyMetadata(true, OnLayoutPropertyChanged));

    public static readonly DependencyProperty ErrorCorrectionProperty =
        DependencyProperty.Register(nameof(ErrorCorrection), typeof(EccLevel), typeof(QrCode),
            new FrameworkPropertyMetadata(EccLevel.Medium, OnErrorCorrectionPropertyChanged));

    public static readonly DependencyProperty DataProperty =
        DependencyProperty.Register(nameof(Data), typeof(string), typeof(QrCode),
            new FrameworkPropertyMetadata(null, OnDataPropertyChanged));

    #endregion DependencyProperties

    #region Properties

    public Brush? Background
    {
        get => (Brush?)GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    public Brush? Foreground
    {
        get => (Brush?)GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public double SymbolCornerRatio
    {
        get => (double)GetValue(SymbolCornerRatioProperty);
        set => SetValue(SymbolCornerRatioProperty, value);
    }

    public Thickness Padding
    {
        get => (Thickness)GetValue(PaddingProperty);
        set => SetValue(PaddingProperty, value);
    }

    public bool IsQuietZoneEnabled
    {
        get => (bool)GetValue(IsQuietZoneEnabledProperty);
        set => SetValue(IsQuietZoneEnabledProperty, value);
    }

    public EccLevel ErrorCorrection
    {
        get => (EccLevel)GetValue(ErrorCorrectionProperty);
        set => SetValue(ErrorCorrectionProperty, value);
    }

    public string? Data
    {
        get => (string?)GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    #endregion Properties

    private static readonly QrEncoder _qrCodeGenerator = new();

    private Wpf.Ui.Violeta.Controls.Encoding.QrCode? _encodedQrCode;
    private PathGeometry? _qrCodeGeometry;

    private int QuietZoneCount => IsQuietZoneEnabled ? 4 : 0;
    private int QuietMargin => QuietZoneCount * 2;

    public QrCode()
    {
        SizeChanged += OnSizeChanged;
    }

    private void OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        OnLayoutChanged(_encodedQrCode);
        InvalidateVisual();
    }

    private static void OnDataPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ctrl = (QrCode)d;
        ctrl._encodedQrCode = null;
        ctrl.RegenerateQrCode();
    }

    private static void OnErrorCorrectionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ctrl = (QrCode)d;
        ctrl._encodedQrCode = null;
        ctrl.RegenerateQrCode();
    }

    private static void OnLayoutPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ctrl = (QrCode)d;
        ctrl.OnLayoutChanged(ctrl._encodedQrCode);
        ctrl.InvalidateVisual();
    }

    private void RegenerateQrCode()
    {
        if (!string.IsNullOrEmpty(Data))
        {
            if (_encodedQrCode is null)
            {
                _qrCodeGenerator.ErrorCorrectionLevel = ToQrCoderEccLevel(ErrorCorrection);
                try
                {
                    _encodedQrCode = _qrCodeGenerator.Encode(Data!);
                }
                catch
                {
                    _encodedQrCode = null;
                }
            }
        }
        else
        {
            _encodedQrCode = null;
        }

        OnLayoutChanged(_encodedQrCode);
        InvalidateVisual();
    }

    private void OnLayoutChanged(Wpf.Ui.Violeta.Controls.Encoding.QrCode? qrCodeData)
    {
        if (qrCodeData is null)
        {
            _qrCodeGeometry = null;
            return;
        }

        var width = ActualWidth;
        var height = ActualHeight;
        if (width <= 0 || height <= 0)
        {
            _qrCodeGeometry = null;
            return;
        }

        var bounds = new Rect(0, 0, width, height);
        var matrix = qrCodeData.Matrix;
        var columnCount = matrix.Width + QuietMargin;
        var rowCount = matrix.Height + QuietMargin;
        var padding = Padding;

        var symbolSize = new Size(
            (width - padding.Left - padding.Right) / columnCount,
            (height - padding.Top - padding.Bottom) / rowCount
        );
        var cornerRatio = SymbolCornerRatio;

        var geometry = new PathGeometry();
        AddPositionDetectionPattern(geometry, bounds, symbolSize, cornerRatio);

        for (var row = 0; row < matrix.Height; row++)
        {
            for (var column = 0; column < matrix.Width; column++)
            {
                ProcessSymbol(geometry, matrix, row, column, symbolSize, cornerRatio, padding);
            }
        }

        _qrCodeGeometry = geometry;
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);

        if (_qrCodeGeometry is null)
            return;

        var bounds = new Rect(0, 0, ActualWidth, ActualHeight);
        var cr = CornerRadius;

        bool hasCr = cr.TopLeft > 0 || cr.TopRight > 0 || cr.BottomRight > 0 || cr.BottomLeft > 0;
        if (hasCr)
        {
            drawingContext.PushClip(CreateRoundedRectGeometry(bounds, cr));
        }

        drawingContext.DrawRectangle(Background, null, bounds);
        drawingContext.DrawGeometry(Foreground, null, _qrCodeGeometry);

        if (hasCr)
        {
            drawingContext.Pop();
        }
    }

    private static Geometry CreateRoundedRectGeometry(Rect bounds, CornerRadius cr)
    {
        var geo = new StreamGeometry();
        using var ctx = geo.Open();
        ctx.BeginFigure(new Point(bounds.Left + cr.TopLeft, bounds.Top), true, true);
        ctx.LineTo(new Point(bounds.Right - cr.TopRight, bounds.Top), false, false);
        if (cr.TopRight > 0)
            ctx.ArcTo(new Point(bounds.Right, bounds.Top + cr.TopRight), new Size(cr.TopRight, cr.TopRight), 0, false, SweepDirection.Clockwise, false, false);
        ctx.LineTo(new Point(bounds.Right, bounds.Bottom - cr.BottomRight), false, false);
        if (cr.BottomRight > 0)
            ctx.ArcTo(new Point(bounds.Right - cr.BottomRight, bounds.Bottom), new Size(cr.BottomRight, cr.BottomRight), 0, false, SweepDirection.Clockwise, false, false);
        ctx.LineTo(new Point(bounds.Left + cr.BottomLeft, bounds.Bottom), false, false);
        if (cr.BottomLeft > 0)
            ctx.ArcTo(new Point(bounds.Left, bounds.Bottom - cr.BottomLeft), new Size(cr.BottomLeft, cr.BottomLeft), 0, false, SweepDirection.Clockwise, false, false);
        ctx.LineTo(new Point(bounds.Left, bounds.Top + cr.TopLeft), false, false);
        if (cr.TopLeft > 0)
            ctx.ArcTo(new Point(bounds.Left + cr.TopLeft, bounds.Top), new Size(cr.TopLeft, cr.TopLeft), 0, false, SweepDirection.Clockwise, false, false);
        return geo;
    }

    protected static ErrorCorrectionLevel ToQrCoderEccLevel(EccLevel eccLevel)
    {
        return eccLevel switch
        {
            EccLevel.Lowest => ErrorCorrectionLevel.L,
            EccLevel.Medium => ErrorCorrectionLevel.M,
            EccLevel.Quality => ErrorCorrectionLevel.Q,
            EccLevel.Highest => ErrorCorrectionLevel.H,
            _ => throw new ArgumentOutOfRangeException(nameof(eccLevel), eccLevel, null),
        };
    }

    [Flags]
    private enum CornerFlags
    {
        None = 0,
        TopLeft = 1 << 0,
        TopRight = 1 << 1,
        BottomRight = 1 << 2,
        BottomLeft = 1 << 3,
    }
}
