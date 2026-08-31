using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// A shields.io-style key/value badge (subject + status).
/// Mirrors HandyControl's Shield control.
/// </summary>
[ContentProperty(nameof(Status))]
public class Shield : ButtonBase
{
    static Shield()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(Shield),
            new FrameworkPropertyMetadata(typeof(Shield)));
    }

    #region Subject

    public static readonly DependencyProperty SubjectProperty =
        DependencyProperty.Register(
            nameof(Subject),
            typeof(string),
            typeof(Shield),
            new PropertyMetadata(null));

    /// <summary>Left-side label text (e.g. ".net", "c#").</summary>
    public string? Subject
    {
        get => (string?)GetValue(SubjectProperty);
        set => SetValue(SubjectProperty, value);
    }

    #endregion Subject

    #region Status

    public static readonly DependencyProperty StatusProperty =
        DependencyProperty.Register(
            nameof(Status),
            typeof(object),
            typeof(Shield),
            new PropertyMetadata(null));

    /// <summary>Right-side value content (e.g. ">=4.0", "7.0").</summary>
    public object? Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    #endregion Status

    #region Color

    public static readonly DependencyProperty ColorProperty =
        DependencyProperty.Register(
            nameof(Color),
            typeof(Brush),
            typeof(Shield),
            new PropertyMetadata(null));

    /// <summary>Background brush for the status (right) segment.</summary>
    public Brush? Color
    {
        get => (Brush?)GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    #endregion Color

    #region SubjectBackground

    public static readonly DependencyProperty SubjectBackgroundProperty =
        DependencyProperty.Register(
            nameof(SubjectBackground),
            typeof(Brush),
            typeof(Shield),
            new PropertyMetadata(null));

    /// <summary>Background brush for the subject (left) segment.</summary>
    public Brush? SubjectBackground
    {
        get => (Brush?)GetValue(SubjectBackgroundProperty);
        set => SetValue(SubjectBackgroundProperty, value);
    }

    #endregion SubjectBackground

    #region CornerRadius

    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(Shield),
            new PropertyMetadata(new CornerRadius(3)));

    /// <summary>Outer corner radius of the badge; split across left/right segments.</summary>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    #endregion CornerRadius
}
