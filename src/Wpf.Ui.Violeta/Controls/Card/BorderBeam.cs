using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// A card-like container with an animated gradient beam traveling along its border,
/// similar to Ant Design's <c>BorderBeam</c>.
/// </summary>
[TemplatePart(Name = PART_BeamIndicator, Type = typeof(BorderBeamIndicator))]
public class BorderBeam : ContentControl
{
    public const string PART_BeamIndicator = "PART_BeamIndicator";

    static BorderBeam()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(BorderBeam),
            new FrameworkPropertyMetadata(typeof(BorderBeam)));

        FocusableProperty.OverrideMetadata(
            typeof(BorderBeam),
            new FrameworkPropertyMetadata(false));

        IsTabStopProperty.OverrideMetadata(
            typeof(BorderBeam),
            new FrameworkPropertyMetadata(false));
    }

    #region IsActive

    public static readonly DependencyProperty IsActiveProperty =
        DependencyProperty.Register(
            nameof(IsActive),
            typeof(bool),
            typeof(BorderBeam),
            new PropertyMetadata(true));

    /// <summary>
    /// Gets or sets whether the border beam animation is visible and running.
    /// </summary>
    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    #endregion IsActive

    #region Duration

    public static readonly DependencyProperty DurationProperty =
        DependencyProperty.Register(
            nameof(Duration),
            typeof(TimeSpan),
            typeof(BorderBeam),
            new PropertyMetadata(TimeSpan.FromSeconds(6)));

    /// <summary>
    /// Gets or sets the time required for one complete loop around the border.
    /// </summary>
    public TimeSpan Duration
    {
        get => (TimeSpan)GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }

    #endregion Duration

    #region BeamSize

    public static readonly DependencyProperty BeamSizeProperty =
        DependencyProperty.Register(
            nameof(BeamSize),
            typeof(double),
            typeof(BorderBeam),
            new PropertyMetadata(100.0));

    /// <summary>
    /// Gets or sets the side length of the moving gradient square.
    /// </summary>
    public double BeamSize
    {
        get => (double)GetValue(BeamSizeProperty);
        set => SetValue(BeamSizeProperty, value);
    }

    #endregion BeamSize

    #region LineWidth

    public static readonly DependencyProperty LineWidthProperty =
        DependencyProperty.Register(
            nameof(LineWidth),
            typeof(double),
            typeof(BorderBeam),
            new PropertyMetadata(1.0));

    /// <summary>
    /// Gets or sets the visible width of the beam along the border.
    /// </summary>
    public double LineWidth
    {
        get => (double)GetValue(LineWidthProperty);
        set => SetValue(LineWidthProperty, value);
    }

    #endregion LineWidth

    #region Outset

    public static readonly DependencyProperty OutsetProperty =
        DependencyProperty.Register(
            nameof(Outset),
            typeof(double),
            typeof(BorderBeam),
            new PropertyMetadata(double.NaN));

    /// <summary>
    /// Gets or sets how far the beam layer extends beyond the card edge.
    /// When unset, the control border thickness is used.
    /// </summary>
    public double Outset
    {
        get => (double)GetValue(OutsetProperty);
        set => SetValue(OutsetProperty, value);
    }

    #endregion Outset

    #region Count

    public static readonly DependencyProperty CountProperty =
        DependencyProperty.Register(
            nameof(Count),
            typeof(int),
            typeof(BorderBeam),
            new PropertyMetadata(1));

    /// <summary>
    /// Gets or sets the number of beams traveling around the border.
    /// </summary>
    public int Count
    {
        get => (int)GetValue(CountProperty);
        set => SetValue(CountProperty, value);
    }

    #endregion Count

    #region CornerRadius

    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(BorderBeam),
            new PropertyMetadata(new CornerRadius(0)));

    /// <summary>
    /// Gets or sets the card corner radius.
    /// </summary>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    #endregion CornerRadius

    #region BeamColor

    public static readonly DependencyProperty BeamColorProperty =
        DependencyProperty.Register(
            nameof(BeamColor),
            typeof(Color),
            typeof(BorderBeam),
            new PropertyMetadata(Colors.DodgerBlue));

    /// <summary>
    /// Gets or sets the primary beam color.
    /// </summary>
    public Color BeamColor
    {
        get => (Color)GetValue(BeamColorProperty);
        set => SetValue(BeamColorProperty, value);
    }

    #endregion BeamColor

    #region BeamHighlightColor

    public static readonly DependencyProperty BeamHighlightColorProperty =
        DependencyProperty.Register(
            nameof(BeamHighlightColor),
            typeof(Color),
            typeof(BorderBeam),
            new PropertyMetadata(Colors.DeepSkyBlue));

    /// <summary>
    /// Gets or sets the secondary beam color used in the gradient tail.
    /// </summary>
    public Color BeamHighlightColor
    {
        get => (Color)GetValue(BeamHighlightColorProperty);
        set => SetValue(BeamHighlightColorProperty, value);
    }

    #endregion BeamHighlightColor
}
