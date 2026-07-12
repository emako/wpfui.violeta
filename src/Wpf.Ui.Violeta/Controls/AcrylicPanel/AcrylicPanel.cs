using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Controls;

[Obsolete("Not ready for production")]
[TemplatePart(Name = PartBlurLayer, Type = typeof(FrameworkElement))]
public class AcrylicPanel : ContentControl
{
    private const string PartBlurLayer = "PART_BlurLayer";

    private FrameworkElement? _blurLayer;

    static AcrylicPanel()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(AcrylicPanel),
            new FrameworkPropertyMetadata(typeof(AcrylicPanel)));
    }

    public AcrylicPanel()
    {
        Target ??= this;
        Source ??= this;
    }

    public static readonly DependencyProperty TargetProperty =
        DependencyProperty.Register(
            nameof(Target),
            typeof(FrameworkElement),
            typeof(AcrylicPanel),
            new PropertyMetadata(null, OnTargetOrSourceChanged));

    public FrameworkElement? Target
    {
        get => (FrameworkElement?)GetValue(TargetProperty);
        set => SetValue(TargetProperty, value);
    }

    public static readonly DependencyProperty SourceProperty =
        DependencyProperty.Register(
            nameof(Source),
            typeof(FrameworkElement),
            typeof(AcrylicPanel),
            new PropertyMetadata(null, OnTargetOrSourceChanged));

    public FrameworkElement? Source
    {
        get => (FrameworkElement?)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(AcrylicPanel),
            new PropertyMetadata(new CornerRadius(0)));

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public static readonly DependencyProperty AmountProperty =
        DependencyProperty.Register(
            nameof(Amount),
            typeof(double),
            typeof(AcrylicPanel),
            new PropertyMetadata(24d));

    public double Amount
    {
        get => (double)GetValue(AmountProperty);
        set => SetValue(AmountProperty, value);
    }

    public static readonly DependencyProperty TintColorProperty =
        DependencyProperty.Register(
            nameof(TintColor),
            typeof(Color),
            typeof(AcrylicPanel),
            new PropertyMetadata(Color.FromArgb(255, 30, 30, 30)));

    public Color TintColor
    {
        get => (Color)GetValue(TintColorProperty);
        set => SetValue(TintColorProperty, value);
    }

    public static readonly DependencyProperty TintOpacityProperty =
        DependencyProperty.Register(
            nameof(TintOpacity),
            typeof(double),
            typeof(AcrylicPanel),
            new PropertyMetadata(0.52d));

    public double TintOpacity
    {
        get => (double)GetValue(TintOpacityProperty);
        set => SetValue(TintOpacityProperty, value);
    }

    public static readonly DependencyProperty NoiseOpacityProperty =
        DependencyProperty.Register(
            nameof(NoiseOpacity),
            typeof(double),
            typeof(AcrylicPanel),
            new PropertyMetadata(0.03d));

    public double NoiseOpacity
    {
        get => (double)GetValue(NoiseOpacityProperty);
        set => SetValue(NoiseOpacityProperty, value);
    }

    public override void OnApplyTemplate()
    {
        DetachTrackedEvents();
        base.OnApplyTemplate();

        _blurLayer = GetTemplateChild(PartBlurLayer) as FrameworkElement;

        AttachTrackedEvents();
        UpdateBlurTransform();
    }

    private static void OnTargetOrSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AcrylicPanel panel)
        {
            panel.DetachTrackedEvents();
            panel.AttachTrackedEvents();
            panel.UpdateBlurTransform();
        }
    }

    private void AttachTrackedEvents()
    {
        LayoutUpdated += OnTrackedLayoutUpdated;

        Target?.LayoutUpdated += OnTrackedLayoutUpdated;

        if (Source != null && Source != Target)
        {
            Source.LayoutUpdated += OnTrackedLayoutUpdated;
        }
    }

    private void DetachTrackedEvents()
    {
        LayoutUpdated -= OnTrackedLayoutUpdated;

        Target?.LayoutUpdated -= OnTrackedLayoutUpdated;

        if (Source != null && Source != Target)
        {
            Source.LayoutUpdated -= OnTrackedLayoutUpdated;
        }
    }

    private void OnTrackedLayoutUpdated(object? sender, EventArgs e)
    {
        UpdateBlurTransform();
    }

    private void UpdateBlurTransform()
    {
        if (_blurLayer == null || Target == null || Source == null)
        {
            return;
        }

        try
        {
            Point sourceInTarget = Source.TransformToVisual(Target).Transform(new Point(0, 0));

            _blurLayer.RenderTransform = new TranslateTransform(-sourceInTarget.X, -sourceInTarget.Y);
        }
        catch
        {
            _blurLayer.RenderTransform = Transform.Identity;
        }
    }
}
