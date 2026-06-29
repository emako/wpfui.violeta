using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Displays a notification indicator (count, text, or dot) anchored to one corner of its content.
/// Mirrors the behaviour of Ursa.Avalonia's Badge control.
/// </summary>
[TemplatePart(Name = PART_BadgeContainer, Type = typeof(FrameworkElement))]
public class Badge : ContentControl
{
    public const string PART_BadgeContainer = "PART_BadgeContainer";

    private FrameworkElement? _badgeContainer;

    static Badge()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(Badge),
            new FrameworkPropertyMetadata(typeof(Badge)));

        IsTabStopProperty.OverrideMetadata(
            typeof(Badge),
            new FrameworkPropertyMetadata(false));

        ClipToBoundsProperty.OverrideMetadata(
            typeof(Badge),
            new FrameworkPropertyMetadata(false));
    }

    #region BadgeContent

    public static readonly DependencyProperty BadgeContentProperty =
        DependencyProperty.Register(
            nameof(BadgeContent),
            typeof(object),
            typeof(Badge),
            new PropertyMetadata(null, OnBadgeContentOrOverflowChanged));

    /// <summary>
    /// The content shown inside the badge indicator (count, string, or any object).
    /// When <see langword="null"/> and <see cref="Dot"/> is <see langword="false"/>
    /// the badge is hidden.
    /// </summary>
    public object? BadgeContent
    {
        get => GetValue(BadgeContentProperty);
        set => SetValue(BadgeContentProperty, value);
    }

    #endregion BadgeContent

    #region Dot

    public static readonly DependencyProperty DotProperty =
        DependencyProperty.Register(
            nameof(Dot),
            typeof(bool),
            typeof(Badge),
            new PropertyMetadata(false, OnBadgeContentOrOverflowChanged));

    /// <summary>
    /// When <see langword="true"/> the badge is rendered as a small dot with no text.
    /// </summary>
    public bool Dot
    {
        get => (bool)GetValue(DotProperty);
        set => SetValue(DotProperty, value);
    }

    #endregion Dot

    #region CornerPosition

    public static readonly DependencyProperty CornerPositionProperty =
        DependencyProperty.Register(
            nameof(CornerPosition),
            typeof(BadgeCornerPosition),
            typeof(Badge),
            new PropertyMetadata(BadgeCornerPosition.TopRight, OnCornerPositionChanged));

    /// <summary>
    /// Which corner of the content the badge indicator is anchored to.
    /// </summary>
    public BadgeCornerPosition CornerPosition
    {
        get => (BadgeCornerPosition)GetValue(CornerPositionProperty);
        set => SetValue(CornerPositionProperty, value);
    }

    private static void OnCornerPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((Badge)d).UpdateBadgePosition();
    }

    #endregion CornerPosition

    #region OverflowCount

    public static readonly DependencyProperty OverflowCountProperty =
        DependencyProperty.Register(
            nameof(OverflowCount),
            typeof(int),
            typeof(Badge),
            new PropertyMetadata(0, OnBadgeContentOrOverflowChanged));

    /// <summary>
    /// When greater than zero and <see cref="BadgeContent"/> is a number exceeding this
    /// value, the badge displays "<c>n+</c>" instead of the raw number.
    /// </summary>
    public int OverflowCount
    {
        get => (int)GetValue(OverflowCountProperty);
        set => SetValue(OverflowCountProperty, value);
    }

    #endregion OverflowCount

    #region BadgeFontSize

    public static readonly DependencyProperty BadgeFontSizeProperty =
        DependencyProperty.Register(
            nameof(BadgeFontSize),
            typeof(double),
            typeof(Badge),
            new PropertyMetadata(11.0));

    public double BadgeFontSize
    {
        get => (double)GetValue(BadgeFontSizeProperty);
        set => SetValue(BadgeFontSizeProperty, value);
    }

    #endregion BadgeFontSize

    #region BadgeBackground / BadgeForeground

    public static readonly DependencyProperty BadgeBackgroundProperty =
        DependencyProperty.Register(
            nameof(BadgeBackground),
            typeof(Brush),
            typeof(Badge),
            new PropertyMetadata(null));

    public Brush? BadgeBackground
    {
        get => (Brush?)GetValue(BadgeBackgroundProperty);
        set => SetValue(BadgeBackgroundProperty, value);
    }

    public static readonly DependencyProperty BadgeForegroundProperty =
        DependencyProperty.Register(
            nameof(BadgeForeground),
            typeof(Brush),
            typeof(Badge),
            new PropertyMetadata(null));

    public Brush? BadgeForeground
    {
        get => (Brush?)GetValue(BadgeForegroundProperty);
        set => SetValue(BadgeForegroundProperty, value);
    }

    #endregion BadgeBackground / BadgeForeground

    #region IsBadgeVisible (read-only)

    private static readonly DependencyPropertyKey IsBadgeVisiblePropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(IsBadgeVisible),
            typeof(bool),
            typeof(Badge),
            new PropertyMetadata(false));

    public static readonly DependencyProperty IsBadgeVisibleProperty = IsBadgeVisiblePropertyKey.DependencyProperty;

    public bool IsBadgeVisible => (bool)GetValue(IsBadgeVisibleProperty);

    #endregion IsBadgeVisible (read-only)

    #region BadgeText (read-only)

    private static readonly DependencyPropertyKey BadgeTextPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(BadgeText),
            typeof(string),
            typeof(Badge),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty BadgeTextProperty = BadgeTextPropertyKey.DependencyProperty;

    /// <summary>String shown in the badge, accounting for <see cref="OverflowCount"/>.</summary>
    public string BadgeText => (string)GetValue(BadgeTextProperty);

    #endregion BadgeText (read-only)

    // -------------------------------------------------------------------------

    private static void OnBadgeContentOrOverflowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var badge = (Badge)d;
        badge.UpdateBadgeText();
        badge.SetValue(IsBadgeVisiblePropertyKey, badge.BadgeContent != null || badge.Dot);
    }

    private void UpdateBadgeText()
    {
        if (Dot || BadgeContent is null)
        {
            SetValue(BadgeTextPropertyKey, string.Empty);
            return;
        }

        if (OverflowCount > 0 && BadgeContent is IConvertible convertible)
        {
            try
            {
                var val = Convert.ToInt64(convertible);
                SetValue(BadgeTextPropertyKey, val > OverflowCount ? $"{OverflowCount}+" : BadgeContent.ToString() ?? string.Empty);
                return;
            }
            catch { /* fall through */ }
        }

        SetValue(BadgeTextPropertyKey, BadgeContent.ToString() ?? string.Empty);
    }

    public override void OnApplyTemplate()
    {
        _badgeContainer?.SizeChanged -= OnBadgeSizeChanged;

        base.OnApplyTemplate();

        _badgeContainer = GetTemplateChild(PART_BadgeContainer) as FrameworkElement;

        if (_badgeContainer != null)
            _badgeContainer.SizeChanged += OnBadgeSizeChanged;

        UpdateBadgePosition();
    }

    private void OnBadgeSizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateBadgePosition();
    }

    private void UpdateBadgePosition()
    {
        if (_badgeContainer is null) return;

        var w = _badgeContainer.ActualWidth;
        var h = _badgeContainer.ActualHeight;

        double tx = CornerPosition is BadgeCornerPosition.TopRight or BadgeCornerPosition.BottomRight
            ? w / 2d : -(w / 2d);
        double ty = CornerPosition is BadgeCornerPosition.BottomLeft or BadgeCornerPosition.BottomRight
            ? h / 2d : -(h / 2d);

        _badgeContainer.RenderTransform = new TranslateTransform(tx, ty);
    }
}
