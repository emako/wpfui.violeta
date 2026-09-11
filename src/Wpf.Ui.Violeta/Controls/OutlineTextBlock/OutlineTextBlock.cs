using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Wpf.Ui.Violeta.Win32;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Renders text with an optional outline stroke.
/// Ported from HandyControl's OutlineText control.
/// </summary>
public class OutlineTextBlock : FrameworkElement
{
    private Pen? _pen;
    private FormattedText? _formattedText;
    private Geometry? _textGeometry;
    private PathGeometry? _clipGeometry;

    static OutlineTextBlock()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(OutlineTextBlock),
            new FrameworkPropertyMetadata(typeof(OutlineTextBlock)));

        SnapsToDevicePixelsProperty.OverrideMetadata(
            typeof(OutlineTextBlock),
            new FrameworkPropertyMetadata(true));

        UseLayoutRoundingProperty.OverrideMetadata(
            typeof(OutlineTextBlock),
            new FrameworkPropertyMetadata(true));
    }

    #region StrokePosition

    public static readonly DependencyProperty StrokePositionProperty =
        DependencyProperty.Register(
            nameof(StrokePosition),
            typeof(StrokePosition),
            typeof(OutlineTextBlock),
            new FrameworkPropertyMetadata(StrokePosition.Center, OnFormattedTextUpdated));

    public StrokePosition StrokePosition
    {
        get => (StrokePosition)GetValue(StrokePositionProperty);
        set => SetValue(StrokePositionProperty, value);
    }

    #endregion StrokePosition

    #region Text

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(OutlineTextBlock),
            new FrameworkPropertyMetadata(
                string.Empty,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
                OnFormattedTextInvalidated));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    #endregion Text

    #region TextAlignment / TextTrimming / TextWrapping

    public static readonly DependencyProperty TextAlignmentProperty =
        DependencyProperty.Register(
            nameof(TextAlignment),
            typeof(TextAlignment),
            typeof(OutlineTextBlock),
            new FrameworkPropertyMetadata(TextAlignment.Left, OnFormattedTextUpdated));

    public TextAlignment TextAlignment
    {
        get => (TextAlignment)GetValue(TextAlignmentProperty);
        set => SetValue(TextAlignmentProperty, value);
    }

    public static readonly DependencyProperty TextTrimmingProperty =
        DependencyProperty.Register(
            nameof(TextTrimming),
            typeof(TextTrimming),
            typeof(OutlineTextBlock),
            new FrameworkPropertyMetadata(TextTrimming.None, OnFormattedTextInvalidated));

    public TextTrimming TextTrimming
    {
        get => (TextTrimming)GetValue(TextTrimmingProperty);
        set => SetValue(TextTrimmingProperty, value);
    }

    public static readonly DependencyProperty TextWrappingProperty =
        DependencyProperty.Register(
            nameof(TextWrapping),
            typeof(TextWrapping),
            typeof(OutlineTextBlock),
            new FrameworkPropertyMetadata(TextWrapping.NoWrap, OnFormattedTextInvalidated));

    public TextWrapping TextWrapping
    {
        get => (TextWrapping)GetValue(TextWrappingProperty);
        set => SetValue(TextWrappingProperty, value);
    }

    #endregion TextAlignment / TextTrimming / TextWrapping

    #region Fill / Stroke / StrokeThickness

    public static readonly DependencyProperty FillProperty =
        DependencyProperty.Register(
            nameof(Fill),
            typeof(Brush),
            typeof(OutlineTextBlock),
            new FrameworkPropertyMetadata(Brushes.Black, OnFormattedTextUpdated));

    public Brush? Fill
    {
        get => (Brush?)GetValue(FillProperty);
        set => SetValue(FillProperty, value);
    }

    public static readonly DependencyProperty StrokeProperty =
        DependencyProperty.Register(
            nameof(Stroke),
            typeof(Brush),
            typeof(OutlineTextBlock),
            new FrameworkPropertyMetadata(Brushes.Black, OnFormattedTextUpdated));

    public Brush? Stroke
    {
        get => (Brush?)GetValue(StrokeProperty);
        set => SetValue(StrokeProperty, value);
    }

    public static readonly DependencyProperty StrokeThicknessProperty =
        DependencyProperty.Register(
            nameof(StrokeThickness),
            typeof(double),
            typeof(OutlineTextBlock),
            new FrameworkPropertyMetadata(0d, OnFormattedTextUpdated));

    public double StrokeThickness
    {
        get => (double)GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    #endregion Fill / Stroke / StrokeThickness

    #region Font properties

    public static readonly DependencyProperty FontFamilyProperty =
        TextElement.FontFamilyProperty.AddOwner(
            typeof(OutlineTextBlock),
            new FrameworkPropertyMetadata(OnFormattedTextUpdated));

    public FontFamily FontFamily
    {
        get => (FontFamily)GetValue(FontFamilyProperty);
        set => SetValue(FontFamilyProperty, value);
    }

    public static readonly DependencyProperty FontSizeProperty =
        TextElement.FontSizeProperty.AddOwner(
            typeof(OutlineTextBlock),
            new FrameworkPropertyMetadata(OnFormattedTextUpdated));

    [TypeConverter(typeof(FontSizeConverter))]
    public double FontSize
    {
        get => (double)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    public static readonly DependencyProperty FontStretchProperty =
        TextElement.FontStretchProperty.AddOwner(
            typeof(OutlineTextBlock),
            new FrameworkPropertyMetadata(OnFormattedTextUpdated));

    public FontStretch FontStretch
    {
        get => (FontStretch)GetValue(FontStretchProperty);
        set => SetValue(FontStretchProperty, value);
    }

    public static readonly DependencyProperty FontStyleProperty =
        TextElement.FontStyleProperty.AddOwner(
            typeof(OutlineTextBlock),
            new FrameworkPropertyMetadata(OnFormattedTextUpdated));

    public FontStyle FontStyle
    {
        get => (FontStyle)GetValue(FontStyleProperty);
        set => SetValue(FontStyleProperty, value);
    }

    public static readonly DependencyProperty FontWeightProperty =
        TextElement.FontWeightProperty.AddOwner(
            typeof(OutlineTextBlock),
            new FrameworkPropertyMetadata(OnFormattedTextUpdated));

    public FontWeight FontWeight
    {
        get => (FontWeight)GetValue(FontWeightProperty);
        set => SetValue(FontWeightProperty, value);
    }

    #endregion Font properties

    protected override void OnRender(DrawingContext drawingContext)
    {
        if (StrokeThickness > 0)
        {
            EnsureGeometry();

            if (_textGeometry is null)
                return;

            drawingContext.DrawGeometry(Fill, null, _textGeometry);

            if (StrokePosition == StrokePosition.Outside && _clipGeometry is not null)
                drawingContext.PushClip(_clipGeometry);
            else if (StrokePosition == StrokePosition.Inside)
                drawingContext.PushClip(_textGeometry);

            drawingContext.DrawGeometry(null, _pen, _textGeometry);

            if (StrokePosition is StrokePosition.Outside or StrokePosition.Inside)
                drawingContext.Pop();
        }
        else
        {
            UpdateFormattedText();
            if (_formattedText is not null)
                drawingContext.DrawText(_formattedText, new Point());
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        EnsureFormattedText();

        if (_formattedText is null)
            return default;

        // Constrain FormattedText; avoid infinity / zero which throw on MaxTextWidth/Height.
        _formattedText.MaxTextWidth = double.IsInfinity(availableSize.Width)
            ? 3579139
            : Math.Max(0, availableSize.Width);
        _formattedText.MaxTextHeight = double.IsInfinity(availableSize.Height)
            ? 3579139
            : Math.Max(0.0001d, availableSize.Height);

        UpdatePen();

        return new Size(_formattedText.Width, _formattedText.Height);
    }

    private void UpdatePen()
    {
        _pen = new Pen(Stroke, StrokeThickness);

        if (StrokePosition is StrokePosition.Outside or StrokePosition.Inside)
            _pen.Thickness = StrokeThickness * 2;
    }

    private void EnsureFormattedText()
    {
        if (_formattedText is not null || Text is null)
            return;

        var typeface = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);
        _formattedText = CreateFormattedText(Text, FlowDirection, typeface, FontSize);
        UpdateFormattedText();
    }

    private FormattedText CreateFormattedText(string text, FlowDirection flowDirection, Typeface typeface, double fontSize)
    {
        double pixelsPerDip = GetPixelsPerDip();

        return new FormattedText(
            text,
            CultureInfo.CurrentUICulture,
            flowDirection,
            typeface,
            fontSize,
            Brushes.Black,
            pixelsPerDip);
    }

    private double GetPixelsPerDip()
    {
        try
        {
            return VisualTreeHelper.GetDpi(this).PixelsPerDip;
        }
        catch
        {
            return DpiHelper.ScaleX;
        }
    }

    private void EnsureGeometry()
    {
        if (_textGeometry is not null)
            return;

        EnsureFormattedText();
        if (_formattedText is null)
            return;

        _textGeometry = _formattedText.BuildGeometry(new Point(0, 0));

        if (StrokePosition == StrokePosition.Outside)
        {
            var bounds = new RectangleGeometry(new Rect(0, 0, ActualWidth, ActualHeight));
            _clipGeometry = Geometry.Combine(bounds, _textGeometry, GeometryCombineMode.Exclude, null);
        }
    }

    private void UpdateFormattedText()
    {
        if (_formattedText is null)
            return;

        _formattedText.MaxLineCount = TextWrapping == TextWrapping.NoWrap ? 1 : int.MaxValue;
        _formattedText.TextAlignment = TextAlignment;
        _formattedText.Trimming = TextTrimming;

        _formattedText.SetFontSize(FontSize);
        _formattedText.SetFontStyle(FontStyle);
        _formattedText.SetFontWeight(FontWeight);
        _formattedText.SetFontFamily(FontFamily);
        _formattedText.SetFontStretch(FontStretch);
        _formattedText.SetForegroundBrush(Fill);
    }

    private static void OnFormattedTextUpdated(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (OutlineTextBlock)d;
        control.UpdateFormattedText();
        control._textGeometry = null;
        control._clipGeometry = null;
        control.InvalidateMeasure();
        control.InvalidateVisual();
    }

    private static void OnFormattedTextInvalidated(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (OutlineTextBlock)d;
        control._formattedText = null;
        control._textGeometry = null;
        control._clipGeometry = null;
        control.InvalidateMeasure();
        control.InvalidateVisual();
    }
}
