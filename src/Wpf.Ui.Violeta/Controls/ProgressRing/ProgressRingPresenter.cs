using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Wpf.Ui.Violeta.Controls.Primitives;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Renders the WinUI-style ProgressRing arc geometry used by
/// <c>Hotfixes/ProgressRing.xaml</c>.
/// </summary>
public class ProgressRingPresenter : Control
{
    static ProgressRingPresenter()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ProgressRingPresenter),
            new FrameworkPropertyMetadata(typeof(ProgressRingPresenter)));
    }

    public ProgressRingPresenter()
    {
        SetValue(TemplateSettingsPropertyKey, new ProgressRingPresenterTemplateSettings());
        SizeChanged += OnSizeChanged;
    }

    #region TemplateSettings

    private static readonly DependencyPropertyKey TemplateSettingsPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(TemplateSettings),
            typeof(ProgressRingPresenterTemplateSettings),
            typeof(ProgressRingPresenter),
            null);

    public static readonly DependencyProperty TemplateSettingsProperty =
        TemplateSettingsPropertyKey.DependencyProperty;

    public ProgressRingPresenterTemplateSettings TemplateSettings =>
        (ProgressRingPresenterTemplateSettings)GetValue(TemplateSettingsProperty);

    #endregion

    #region Value

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(
            nameof(Value),
            typeof(double),
            typeof(ProgressRingPresenter),
            new FrameworkPropertyMetadata(OnValuePropertyChanged));

    private static void OnValuePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        ((ProgressRingPresenter)sender).UpdateSegment();
    }

    #endregion

    #region StrokeThickness

    public double StrokeThickness
    {
        get => (double)GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    public static readonly DependencyProperty StrokeThicknessProperty =
        DependencyProperty.Register(
            nameof(StrokeThickness),
            typeof(double),
            typeof(ProgressRingPresenter),
            new FrameworkPropertyMetadata(OnStrokeThicknessPropertyChanged));

    private static void OnStrokeThicknessPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        ((ProgressRingPresenter)sender).UpdateRing();
    }

    #endregion

    public override void OnApplyTemplate()
    {
        ApplyPathGeometry();
        base.OnApplyTemplate();
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e) => UpdateRing();

    private void ApplyPathGeometry()
    {
        var templateSettings = TemplateSettings;

        var outlineArcPart = new ArcSegment
        {
            IsLargeArc = true,
            SweepDirection = SweepDirection.Clockwise
        };

        BindingOperations.SetBinding(outlineArcPart, ArcSegment.PointProperty, new Binding
        {
            Source = templateSettings,
            Path = new PropertyPath(nameof(ProgressRingPresenterTemplateSettings.OutlineArcPoint))
        });

        BindingOperations.SetBinding(outlineArcPart, ArcSegment.SizeProperty, new Binding
        {
            Source = templateSettings,
            Path = new PropertyPath(nameof(ProgressRingPresenterTemplateSettings.OutlineArcSize))
        });

        var outlineFigurePart = new PathFigure
        {
            Segments = new PathSegmentCollection
            {
                outlineArcPart
            }
        };

        BindingOperations.SetBinding(outlineFigurePart, PathFigure.StartPointProperty, new Binding
        {
            Source = templateSettings,
            Path = new PropertyPath(nameof(ProgressRingPresenterTemplateSettings.OutlineFigureStartPoint))
        });

        templateSettings.OutlinePath = new PathGeometry
        {
            Figures = new PathFigureCollection
            {
                outlineFigurePart
            }
        };

        var ringArcPart = new ArcSegment
        {
            IsLargeArc = true,
            SweepDirection = SweepDirection.Clockwise
        };

        BindingOperations.SetBinding(ringArcPart, ArcSegment.IsLargeArcProperty, new Binding
        {
            Source = templateSettings,
            Path = new PropertyPath(nameof(ProgressRingPresenterTemplateSettings.RingArcIsLargeArc))
        });

        BindingOperations.SetBinding(ringArcPart, ArcSegment.PointProperty, new Binding
        {
            Source = templateSettings,
            Path = new PropertyPath(nameof(ProgressRingPresenterTemplateSettings.RingArcPoint))
        });

        BindingOperations.SetBinding(ringArcPart, ArcSegment.SizeProperty, new Binding
        {
            Source = templateSettings,
            Path = new PropertyPath(nameof(ProgressRingPresenterTemplateSettings.RingArcSize))
        });

        var ringFigurePart = new PathFigure
        {
            Segments = new PathSegmentCollection
            {
                ringArcPart
            }
        };

        BindingOperations.SetBinding(ringFigurePart, PathFigure.StartPointProperty, new Binding
        {
            Source = templateSettings,
            Path = new PropertyPath(nameof(ProgressRingPresenterTemplateSettings.RingFigureStartPoint))
        });

        templateSettings.RingPath = new PathGeometry
        {
            Figures = new PathFigureCollection
            {
                ringFigurePart
            }
        };
    }

    private static Size ComputeEllipseSize(double thickness, double actualWidth, double actualHeight)
    {
        double safeThickness = Math.Max(thickness, 0.0);
        double width = Math.Max((actualWidth - safeThickness) / 2.0, 0.0);
        double height = Math.Max((actualHeight - safeThickness) / 2.0, 0.0);
        return new Size(width, height);
    }

    private void UpdateSegment()
    {
        var templateSettings = TemplateSettings;

        double normalizedRange = Math.Min(Value, 0.999999940395355224609375);
        double angle = 2 * Math.PI * normalizedRange;

        double thickness = StrokeThickness;
        var size = ComputeEllipseSize(thickness, ActualWidth, ActualHeight);
        double translationFactor = Math.Max(thickness / 2.0, 0.0);

        double x = (Math.Sin(angle) * size.Width) + size.Width + translationFactor;
        double y = (((Math.Cos(angle) * size.Height) - size.Height) * -1) + translationFactor;

        templateSettings.RingArcIsLargeArc = angle >= Math.PI;
        templateSettings.RingArcPoint = new Point(x, y);
    }

    private void UpdateRing()
    {
        var templateSettings = TemplateSettings;

        double thickness = StrokeThickness;
        var size = ComputeEllipseSize(thickness, ActualWidth, ActualHeight);

        double segmentWidth = size.Width;
        double translationFactor = Math.Max(thickness / 2.0, 0.0);

        templateSettings.OutlineFigureStartPoint = new Point(segmentWidth + translationFactor, translationFactor);
        templateSettings.RingFigureStartPoint = new Point(segmentWidth + translationFactor, translationFactor);
        templateSettings.OutlineArcSize = new Size(segmentWidth, size.Height);
        templateSettings.OutlineArcPoint = new Point(segmentWidth + translationFactor - 0.05d, translationFactor);
        templateSettings.RingArcSize = new Size(segmentWidth, size.Height);

        UpdateSegment();
    }
}
