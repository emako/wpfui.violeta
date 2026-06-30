using System.Windows;
using System.Windows.Controls;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// An inline notification banner that displays a title, optional body content,
/// an icon that reflects the <see cref="Type"/>, and an optional close button.
/// Mirrors the logic of Ursa.Avalonia's Banner control.
/// </summary>
[TemplatePart(Name = PART_CloseButton, Type = typeof(Button))]
public class Banner : HeaderedContentControl
{
    public const string PART_CloseButton = "PART_CloseButton";

    private Button? _closeButton;

    static Banner()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(Banner),
            new FrameworkPropertyMetadata(typeof(Banner)));
    }

    #region Dependency Properties

    public static readonly DependencyProperty CanCloseProperty =
        DependencyProperty.Register(
            nameof(CanClose), typeof(bool), typeof(Banner),
            new PropertyMetadata(false));

    /// <summary>Whether a close button is shown.</summary>
    public bool CanClose
    {
        get => (bool)GetValue(CanCloseProperty);
        set => SetValue(CanCloseProperty, value);
    }

    public static readonly DependencyProperty ShowIconProperty =
        DependencyProperty.Register(
            nameof(ShowIcon), typeof(bool), typeof(Banner),
            new PropertyMetadata(true));

    /// <summary>Whether the icon area (built-in or custom) is visible.</summary>
    public bool ShowIcon
    {
        get => (bool)GetValue(ShowIconProperty);
        set => SetValue(ShowIconProperty, value);
    }

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(
            nameof(Icon), typeof(object), typeof(Banner),
            new PropertyMetadata(null));

    /// <summary>
    /// Custom icon content. When <c>null</c> the built-in icon for the current <see cref="Type"/> is used.
    /// </summary>
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly DependencyProperty TypeProperty =
        DependencyProperty.Register(
            nameof(Type), typeof(BannerType), typeof(Banner),
            new PropertyMetadata(BannerType.Information));

    /// <summary>Determines the colour scheme and built-in icon of the banner.</summary>
    public BannerType Type
    {
        get => (BannerType)GetValue(TypeProperty);
        set => SetValue(TypeProperty, value);
    }

    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(
            nameof(CornerRadius), typeof(CornerRadius), typeof(Banner),
            new PropertyMetadata(new CornerRadius(0)));

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    #endregion Dependency Properties

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _closeButton?.Click -= OnCloseButtonClick;

        _closeButton = GetTemplateChild(PART_CloseButton) as Button;

        _closeButton?.Click += OnCloseButtonClick;
    }

    private void OnCloseButtonClick(object? sender, RoutedEventArgs e)
    {
        Visibility = Visibility.Collapsed;
    }
}
