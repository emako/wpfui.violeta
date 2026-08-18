using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// A three-bar volume / signal-strength indicator.
/// <list type="bullet">
/// <item><see cref="Volume"/> 0 — all bars inactive</item>
/// <item><see cref="Volume"/> 1 — first bar, critical colour</item>
/// <item><see cref="Volume"/> 2 — first two bars, caution colour</item>
/// <item><see cref="Volume"/> 3 — all bars, success colour</item>
/// </list>
/// Glyph is authored in a 16×16 square (bars 3×5 / 8.5 / 12, gap 1.5) and scaled by
/// <see cref="FrameworkElement.Width"/> / <see cref="FrameworkElement.Height"/>.
/// When <see cref="VolumeThreshold"/> is non-negative, the displayed volume is derived from
/// <see cref="MaxThresholdFor3"/> and <see cref="MaxThresholdFor2"/> (lower values map to more bars).
/// </summary>
public class VolumeView : Control
{
    static VolumeView()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(VolumeView),
            new FrameworkPropertyMetadata(typeof(VolumeView)));

        IsTabStopProperty.OverrideMetadata(
            typeof(VolumeView),
            new FrameworkPropertyMetadata(false));

        FocusableProperty.OverrideMetadata(
            typeof(VolumeView),
            new FrameworkPropertyMetadata(false));
    }

    #region Volume

    public static readonly DependencyProperty VolumeProperty =
        DependencyProperty.Register(
            nameof(Volume),
            typeof(int),
            typeof(VolumeView),
            new FrameworkPropertyMetadata(
                0,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                null,
                CoerceVolume));

    /// <summary>
    /// Number of active bars, from 0 to 3.
    /// </summary>
    public int Volume
    {
        get => (int)GetValue(VolumeProperty);
        set => SetValue(VolumeProperty, value);
    }

    private static object CoerceVolume(DependencyObject d, object baseValue)
    {
        var value = (int)baseValue;
        if (value < 0)
        {
            return 0;
        }

        if (value > 3)
        {
            return 3;
        }

        return value;
    }

    #endregion Volume

    #region VolumeThreshold

    public static readonly DependencyProperty VolumeThresholdProperty =
        DependencyProperty.Register(
            nameof(VolumeThreshold),
            typeof(int),
            typeof(VolumeView),
            new PropertyMetadata(-1, OnThresholdMappingChanged));

    /// <summary>
    /// Raw metric used to derive <see cref="Volume"/>.
    /// Values below 0 leave <see cref="Volume"/> unchanged.
    /// Values up to <see cref="MaxThresholdFor3"/> map to 3 bars,
    /// up to <see cref="MaxThresholdFor2"/> map to 2 bars, otherwise 1 bar.
    /// </summary>
    public int VolumeThreshold
    {
        get => (int)GetValue(VolumeThresholdProperty);
        set => SetValue(VolumeThresholdProperty, value);
    }

    public static readonly DependencyProperty MaxThresholdFor3Property =
        DependencyProperty.Register(
            nameof(MaxThresholdFor3),
            typeof(int),
            typeof(VolumeView),
            new PropertyMetadata(128, OnThresholdMappingChanged));

    /// <summary>
    /// Inclusive upper bound of the 3-bar (success) range.
    /// </summary>
    public int MaxThresholdFor3
    {
        get => (int)GetValue(MaxThresholdFor3Property);
        set => SetValue(MaxThresholdFor3Property, value);
    }

    public static readonly DependencyProperty MaxThresholdFor2Property =
        DependencyProperty.Register(
            nameof(MaxThresholdFor2),
            typeof(int),
            typeof(VolumeView),
            new PropertyMetadata(256, OnThresholdMappingChanged));

    /// <summary>
    /// Inclusive upper bound of the 2-bar (caution) range.
    /// Values above this display 1 bar.
    /// </summary>
    public int MaxThresholdFor2
    {
        get => (int)GetValue(MaxThresholdFor2Property);
        set => SetValue(MaxThresholdFor2Property, value);
    }

    private static void OnThresholdMappingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((VolumeView)d).UpdateVolumeFromThreshold();
    }

    private void UpdateVolumeFromThreshold()
    {
        var threshold = VolumeThreshold;
        if (threshold < 0)
        {
            return;
        }

        if (threshold <= MaxThresholdFor3)
        {
            Volume = 3;
        }
        else if (threshold <= MaxThresholdFor2)
        {
            Volume = 2;
        }
        else
        {
            Volume = 1;
        }
    }

    #endregion VolumeThreshold

    #region Fills

    public static readonly DependencyProperty InactiveFillProperty =
        DependencyProperty.Register(
            nameof(InactiveFill),
            typeof(Brush),
            typeof(VolumeView),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>
    /// Fill used by bars that are not active for the current <see cref="Volume"/>.
    /// </summary>
    public Brush? InactiveFill
    {
        get => (Brush?)GetValue(InactiveFillProperty);
        set => SetValue(InactiveFillProperty, value);
    }

    public static readonly DependencyProperty Level1FillProperty =
        DependencyProperty.Register(
            nameof(Level1Fill),
            typeof(Brush),
            typeof(VolumeView),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>
    /// Fill used when <see cref="Volume"/> is 1.
    /// </summary>
    public Brush? Level1Fill
    {
        get => (Brush?)GetValue(Level1FillProperty);
        set => SetValue(Level1FillProperty, value);
    }

    public static readonly DependencyProperty Level2FillProperty =
        DependencyProperty.Register(
            nameof(Level2Fill),
            typeof(Brush),
            typeof(VolumeView),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>
    /// Fill used when <see cref="Volume"/> is 2.
    /// </summary>
    public Brush? Level2Fill
    {
        get => (Brush?)GetValue(Level2FillProperty);
        set => SetValue(Level2FillProperty, value);
    }

    public static readonly DependencyProperty Level3FillProperty =
        DependencyProperty.Register(
            nameof(Level3Fill),
            typeof(Brush),
            typeof(VolumeView),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>
    /// Fill used when <see cref="Volume"/> is 3.
    /// </summary>
    public Brush? Level3Fill
    {
        get => (Brush?)GetValue(Level3FillProperty);
        set => SetValue(Level3FillProperty, value);
    }

    #endregion Fills

    #region Layout

    public static readonly DependencyProperty StretchProperty =
        DependencyProperty.Register(
            nameof(Stretch),
            typeof(Stretch),
            typeof(VolumeView),
            new FrameworkPropertyMetadata(
                Stretch.Uniform,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

    /// <summary>
    /// How the 16×16 design glyph is mapped onto <see cref="FrameworkElement.Width"/> /
    /// <see cref="FrameworkElement.Height"/>.
    /// <see cref="System.Windows.Media.Stretch.Uniform"/> (default) keeps the original proportions;
    /// <see cref="System.Windows.Media.Stretch.Fill"/> stretches pill height independently of width.
    /// </summary>
    public Stretch Stretch
    {
        get => (Stretch)GetValue(StretchProperty);
        set => SetValue(StretchProperty, value);
    }

    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(VolumeView),
            new FrameworkPropertyMetadata(
                new CornerRadius(1.5),
                FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>
    /// Corner radius of each bar, in 16×16 design units.
    /// Default 1.5 matches a pill on the 3px-wide bars.
    /// </summary>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public static readonly DependencyProperty BarWidthProperty =
        DependencyProperty.Register(
            nameof(BarWidth),
            typeof(double),
            typeof(VolumeView),
            new FrameworkPropertyMetadata(
                3.0,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

    /// <summary>
    /// Width of each bar, in 16×16 design units. Default 3.
    /// </summary>
    public double BarWidth
    {
        get => (double)GetValue(BarWidthProperty);
        set => SetValue(BarWidthProperty, value);
    }

    public static readonly DependencyProperty BarSpacingProperty =
        DependencyProperty.Register(
            nameof(BarSpacing),
            typeof(double),
            typeof(VolumeView),
            new FrameworkPropertyMetadata(
                1.5,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

    /// <summary>
    /// Horizontal gap between bars, in 16×16 design units. Default 1.5.
    /// </summary>
    public double BarSpacing
    {
        get => (double)GetValue(BarSpacingProperty);
        set => SetValue(BarSpacingProperty, value);
    }

    public static readonly DependencyProperty Bar1HeightProperty =
        DependencyProperty.Register(
            nameof(Bar1Height),
            typeof(double),
            typeof(VolumeView),
            new FrameworkPropertyMetadata(
                5.0,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

    /// <summary>
    /// Height of the shortest (left) bar, in 16×16 design units. Default 5.
    /// </summary>
    public double Bar1Height
    {
        get => (double)GetValue(Bar1HeightProperty);
        set => SetValue(Bar1HeightProperty, value);
    }

    public static readonly DependencyProperty Bar2HeightProperty =
        DependencyProperty.Register(
            nameof(Bar2Height),
            typeof(double),
            typeof(VolumeView),
            new FrameworkPropertyMetadata(
                8.5,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

    /// <summary>
    /// Height of the middle bar, in 16×16 design units. Default 8.5.
    /// </summary>
    public double Bar2Height
    {
        get => (double)GetValue(Bar2HeightProperty);
        set => SetValue(Bar2HeightProperty, value);
    }

    public static readonly DependencyProperty Bar3HeightProperty =
        DependencyProperty.Register(
            nameof(Bar3Height),
            typeof(double),
            typeof(VolumeView),
            new FrameworkPropertyMetadata(
                12.0,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

    /// <summary>
    /// Height of the tallest (right) bar, in 16×16 design units. Default 12.
    /// </summary>
    public double Bar3Height
    {
        get => (double)GetValue(Bar3HeightProperty);
        set => SetValue(Bar3HeightProperty, value);
    }

    #endregion Layout
}
