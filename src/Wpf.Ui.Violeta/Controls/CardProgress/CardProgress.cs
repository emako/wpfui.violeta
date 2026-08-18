using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// A content card with a <see cref="ProgressBar"/> along the bottom edge.
/// The bar itself is a straight strip; the card's rounded clip trims it to the
/// bottom corners so the indicator never grows hooks from the card radius.
/// </summary>
[TemplatePart(Name = PART_ProgressBar, Type = typeof(ProgressBar))]
public class CardProgress : ContentControl
{
    public const string PART_ProgressBar = "PART_ProgressBar";
    static CardProgress()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(CardProgress),
            new FrameworkPropertyMetadata(typeof(CardProgress)));

        FocusableProperty.OverrideMetadata(
            typeof(CardProgress),
            new FrameworkPropertyMetadata(false));

        IsTabStopProperty.OverrideMetadata(
            typeof(CardProgress),
            new FrameworkPropertyMetadata(false));
    }

    #region Value

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(
            nameof(Value),
            typeof(double),
            typeof(CardProgress),
            new FrameworkPropertyMetadata(
                0.0,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                null,
                CoerceValue));

    /// <summary>
    /// Gets or sets the current progress value, between <see cref="Minimum"/> and <see cref="Maximum"/>.
    /// </summary>
    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    private static object CoerceValue(DependencyObject d, object baseValue)
    {
        var self = (CardProgress)d;
        var value = (double)baseValue;
        if (value < self.Minimum)
        {
            return self.Minimum;
        }

        if (value > self.Maximum)
        {
            return self.Maximum;
        }

        return value;
    }

    #endregion Value

    #region Minimum

    public static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register(
            nameof(Minimum),
            typeof(double),
            typeof(CardProgress),
            new PropertyMetadata(0.0, OnRangeChanged));

    /// <summary>
    /// Gets or sets the minimum of the progress range.
    /// </summary>
    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    #endregion Minimum

    #region Maximum

    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(
            nameof(Maximum),
            typeof(double),
            typeof(CardProgress),
            new PropertyMetadata(100.0, OnRangeChanged));

    /// <summary>
    /// Gets or sets the maximum of the progress range.
    /// </summary>
    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    private static void OnRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        d.CoerceValue(ValueProperty);
    }

    #endregion Maximum

    #region IsIndeterminate

    public static readonly DependencyProperty IsIndeterminateProperty =
        DependencyProperty.Register(
            nameof(IsIndeterminate),
            typeof(bool),
            typeof(CardProgress),
            new PropertyMetadata(false));

    /// <summary>
    /// Gets or sets whether the bottom indicator shows an indeterminate animation.
    /// </summary>
    public bool IsIndeterminate
    {
        get => (bool)GetValue(IsIndeterminateProperty);
        set => SetValue(IsIndeterminateProperty, value);
    }

    #endregion IsIndeterminate

    #region ShowError

    public static readonly DependencyProperty ShowErrorProperty =
        DependencyProperty.Register(
            nameof(ShowError),
            typeof(bool),
            typeof(CardProgress),
            new PropertyMetadata(false));

    /// <summary>
    /// Gets or sets whether the indicator uses the error (critical) visual state.
    /// </summary>
    public bool ShowError
    {
        get => (bool)GetValue(ShowErrorProperty);
        set => SetValue(ShowErrorProperty, value);
    }

    #endregion ShowError

    #region ShowPaused

    public static readonly DependencyProperty ShowPausedProperty =
        DependencyProperty.Register(
            nameof(ShowPaused),
            typeof(bool),
            typeof(CardProgress),
            new PropertyMetadata(false));

    /// <summary>
    /// Gets or sets whether the indicator uses the paused (caution) visual state.
    /// </summary>
    public bool ShowPaused
    {
        get => (bool)GetValue(ShowPausedProperty);
        set => SetValue(ShowPausedProperty, value);
    }

    #endregion ShowPaused

    #region ProgressForeground

    public static readonly DependencyProperty ProgressForegroundProperty =
        DependencyProperty.Register(
            nameof(ProgressForeground),
            typeof(Brush),
            typeof(CardProgress),
            new FrameworkPropertyMetadata(null));

    /// <summary>
    /// Gets or sets the brush of the progress indicator. Defaults to <c>ProgressBarForeground</c>.
    /// </summary>
    public Brush? ProgressForeground
    {
        get => (Brush?)GetValue(ProgressForegroundProperty);
        set => SetValue(ProgressForegroundProperty, value);
    }

    #endregion ProgressForeground

    #region ProgressBackground

    public static readonly DependencyProperty ProgressBackgroundProperty =
        DependencyProperty.Register(
            nameof(ProgressBackground),
            typeof(Brush),
            typeof(CardProgress),
            new FrameworkPropertyMetadata(null));

    /// <summary>
    /// Gets or sets the brush of the bottom track. Defaults to <c>ProgressBarBackground</c>.
    /// </summary>
    public Brush? ProgressBackground
    {
        get => (Brush?)GetValue(ProgressBackgroundProperty);
        set => SetValue(ProgressBackgroundProperty, value);
    }

    #endregion ProgressBackground

    #region CornerRadius

    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(CardProgress),
            new PropertyMetadata(new CornerRadius(0)));

    /// <summary>
    /// Gets or sets the card corner radius. The bottom indicator is clipped to this shape.
    /// </summary>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    #endregion CornerRadius
}
