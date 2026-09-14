using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Draws a <see cref="Geometry"/> with a stroke-dash offset animation (path tracing effect).
/// Ported from HandyControl's AnimationPath.
/// </summary>
public class AnimationPath : Shape
{
    private Storyboard? _storyboard;
    private double _pathLength;

    static AnimationPath()
    {
        StretchProperty.OverrideMetadata(typeof(AnimationPath),
            new FrameworkPropertyMetadata(
                Stretch.Uniform,
                FrameworkPropertyMetadataOptions.AffectsMeasure
                | FrameworkPropertyMetadataOptions.AffectsArrange
                | FrameworkPropertyMetadataOptions.AffectsRender,
                OnPropertiesChanged));

        StrokeThicknessProperty.OverrideMetadata(typeof(AnimationPath),
            new FrameworkPropertyMetadata(
                1d,
                FrameworkPropertyMetadataOptions.AffectsMeasure
                | FrameworkPropertyMetadataOptions.AffectsRender,
                OnPropertiesChanged));
    }

    public AnimationPath()
    {
        Loaded += (_, _) => UpdatePath();
    }

    public static readonly DependencyProperty DataProperty = DependencyProperty.Register(
        nameof(Data),
        typeof(Geometry),
        typeof(AnimationPath),
        new FrameworkPropertyMetadata(null, OnPropertiesChanged));

    public Geometry? Data
    {
        get => (Geometry?)GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    protected override Geometry DefiningGeometry => Data ?? Geometry.Empty;

    public static readonly DependencyProperty PathLengthProperty = DependencyProperty.Register(
        nameof(PathLength),
        typeof(double),
        typeof(AnimationPath),
        new FrameworkPropertyMetadata(0d, OnPropertiesChanged));

    public double PathLength
    {
        get => (double)GetValue(PathLengthProperty);
        set => SetValue(PathLengthProperty, value);
    }

    public static readonly DependencyProperty DurationProperty = DependencyProperty.Register(
        nameof(Duration),
        typeof(Duration),
        typeof(AnimationPath),
        new FrameworkPropertyMetadata(new Duration(TimeSpan.FromSeconds(2)), OnPropertiesChanged));

    public Duration Duration
    {
        get => (Duration)GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }

    public static readonly DependencyProperty IsPlayingProperty = DependencyProperty.Register(
        nameof(IsPlaying),
        typeof(bool),
        typeof(AnimationPath),
        new FrameworkPropertyMetadata(true, OnIsPlayingChanged));

    public bool IsPlaying
    {
        get => (bool)GetValue(IsPlayingProperty);
        set => SetValue(IsPlayingProperty, value);
    }

    public static readonly DependencyProperty RepeatBehaviorProperty = DependencyProperty.Register(
        nameof(RepeatBehavior),
        typeof(RepeatBehavior),
        typeof(AnimationPath),
        new PropertyMetadata(RepeatBehavior.Forever));

    public RepeatBehavior RepeatBehavior
    {
        get => (RepeatBehavior)GetValue(RepeatBehaviorProperty);
        set => SetValue(RepeatBehaviorProperty, value);
    }

    public static readonly DependencyProperty FillBehaviorProperty = DependencyProperty.Register(
        nameof(FillBehavior),
        typeof(FillBehavior),
        typeof(AnimationPath),
        new PropertyMetadata(FillBehavior.Stop, OnPropertiesChanged));

    public FillBehavior FillBehavior
    {
        get => (FillBehavior)GetValue(FillBehaviorProperty);
        set => SetValue(FillBehaviorProperty, value);
    }

    public static readonly RoutedEvent CompletedEvent =
        EventManager.RegisterRoutedEvent(
            nameof(Completed),
            RoutingStrategy.Bubble,
            typeof(EventHandler),
            typeof(AnimationPath));

    public event EventHandler Completed
    {
        add => AddHandler(CompletedEvent, value);
        remove => RemoveHandler(CompletedEvent, value);
    }

    private static void OnPropertiesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AnimationPath path)
        {
            path.UpdatePath();
        }
    }

    private static void OnIsPlayingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ctl = (AnimationPath)d;
        if ((bool)e.NewValue)
        {
            ctl.UpdatePath();
        }
        else
        {
            ctl._storyboard?.Pause();
        }
    }

    private void UpdatePath()
    {
        if (!Duration.HasTimeSpan || !IsPlaying)
        {
            return;
        }

        if (Data is null)
        {
            return;
        }

        _pathLength = PathLength > 0
            ? PathLength
            : Data.GetTotalLength(new Size(ActualWidth, ActualHeight), StrokeThickness);

        if (MathHelper.IsVerySmall(_pathLength))
        {
            return;
        }

        StrokeDashOffset = _pathLength;
        StrokeDashArray = new DoubleCollection([_pathLength, _pathLength]);

        if (_storyboard is not null)
        {
            _storyboard.Stop();
            _storyboard.Completed -= OnStoryboardCompleted;
        }

        _storyboard = new Storyboard
        {
            RepeatBehavior = RepeatBehavior,
            FillBehavior = FillBehavior,
        };
        _storyboard.Completed += OnStoryboardCompleted;

        var frames = new DoubleAnimationUsingKeyFrames();
        frames.KeyFrames.Add(new LinearDoubleKeyFrame
        {
            Value = _pathLength,
            KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
        });
        frames.KeyFrames.Add(new LinearDoubleKeyFrame
        {
            Value = FillBehavior == FillBehavior.Stop ? -_pathLength : 0,
            KeyTime = KeyTime.FromTimeSpan(Duration.TimeSpan),
        });

        Storyboard.SetTarget(frames, this);
        Storyboard.SetTargetProperty(frames, new PropertyPath(StrokeDashOffsetProperty));
        _storyboard.Children.Add(frames);
        _storyboard.Begin();
    }

    private void OnStoryboardCompleted(object? sender, EventArgs e) =>
        RaiseEvent(new RoutedEventArgs(CompletedEvent));
}

file static class MathHelper
{
    public static bool IsVerySmall(double value) => Math.Abs(value) < 1E-06;
}

file static class GeometryExtensions
{
    public static double GetTotalLength(this Geometry geometry)
    {
        var pathGeometry = PathGeometry.CreateFromGeometry(geometry);
        if (pathGeometry.Figures.Count == 0)
        {
            return 0;
        }

        pathGeometry.GetPointAtFractionLength(1e-4, out var point, out _);
        return (pathGeometry.Figures[0].StartPoint - point).Length * 1e+4;
    }

    public static double GetTotalLength(this Geometry geometry, Size size, double strokeThickness = 1)
    {
        if (MathHelper.IsVerySmall(size.Width) || MathHelper.IsVerySmall(size.Height))
        {
            return 0;
        }

        var length = geometry.GetTotalLength();
        var sw = geometry.Bounds.Width / size.Width;
        var sh = geometry.Bounds.Height / size.Height;
        var min = Math.Min(sw, sh);

        if (MathHelper.IsVerySmall(min) || MathHelper.IsVerySmall(strokeThickness))
        {
            return 0;
        }

        length /= min;
        return length / strokeThickness;
    }
}
