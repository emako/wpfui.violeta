using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Wpf.Ui.Violeta.Controls;

[TemplatePart(Name = ElementContent, Type = typeof(FrameworkElement))]
[TemplatePart(Name = ElementPanel, Type = typeof(Panel))]
public class RunningBlock : ContentControl
{
    private const string ElementContent = "PART_ContentElement";

    private const string ElementPanel = "PART_Panel";

    protected Storyboard? _storyboard;

    private FrameworkElement? _elementContent;

    private FrameworkElement? _elementPanel;

    public override void OnApplyTemplate()
    {
        if (_elementPanel != null)
        {
            _elementPanel.SizeChanged -= ElementPanel_SizeChanged;
        }

        base.OnApplyTemplate();

        _elementContent = GetTemplateChild(ElementContent) as FrameworkElement;
        _elementPanel = GetTemplateChild(ElementPanel) as Panel;

        if (_elementPanel != null)
        {
            _elementPanel.SizeChanged += ElementPanel_SizeChanged;
        }

        UpdateContent();
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        UpdateContent();
    }

    private void ElementPanel_SizeChanged(object sender, SizeChangedEventArgs e) => UpdateContent();

    public static readonly DependencyProperty RunawayProperty = DependencyProperty.Register(
        nameof(Runaway), typeof(bool), typeof(RunningBlock), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

    public bool Runaway
    {
        get => (bool)GetValue(RunawayProperty);
        set => SetValue(RunawayProperty, value);
    }

    public static readonly DependencyProperty AutoRunProperty = DependencyProperty.Register(
        nameof(AutoRun), typeof(bool), typeof(RunningBlock), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

    public bool AutoRun
    {
        get => (bool)GetValue(AutoRunProperty);
        set => SetValue(AutoRunProperty, value);
    }

    public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
        nameof(Orientation), typeof(Orientation), typeof(RunningBlock), new FrameworkPropertyMetadata(default(Orientation), FrameworkPropertyMetadataOptions.AffectsRender));

    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public static readonly DependencyProperty DurationProperty = DependencyProperty.Register(
        nameof(Duration), typeof(Duration), typeof(RunningBlock), new FrameworkPropertyMetadata(new Duration(TimeSpan.FromSeconds(5)), FrameworkPropertyMetadataOptions.AffectsRender));

    public Duration Duration
    {
        get => (Duration)GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }

    public static readonly DependencyProperty SpeedProperty = DependencyProperty.Register(
        nameof(Speed), typeof(double), typeof(RunningBlock), new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsRender));

    public double Speed
    {
        get => (double)GetValue(SpeedProperty);
        set => SetValue(SpeedProperty, value);
    }

    public static readonly DependencyProperty IsRunningProperty = DependencyProperty.Register(
        nameof(IsRunning), typeof(bool), typeof(RunningBlock), new PropertyMetadata(true, (o, args) =>
        {
            var ctl = (RunningBlock)o;
            var v = (bool)args.NewValue;
            if (v)
            {
                ctl._storyboard?.Resume();
            }
            else
            {
                ctl._storyboard?.Pause();
            }
        }));

    public bool IsRunning
    {
        get => (bool)GetValue(IsRunningProperty);
        set => SetValue(IsRunningProperty, value);
    }

    public static readonly DependencyProperty AutoReverseProperty = DependencyProperty.Register(
        nameof(AutoReverse), typeof(bool), typeof(RunningBlock), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

    public bool AutoReverse
    {
        get => (bool)GetValue(AutoReverseProperty);
        set => SetValue(AutoReverseProperty, value);
    }

    public static readonly DependencyProperty RunningDirectionProperty = DependencyProperty.Register(
        nameof(RunningDirection), typeof(RunningDirection), typeof(RunningBlock), new PropertyMetadata(RunningDirection.EndToStart));

    public RunningDirection RunningDirection
    {
        get => (RunningDirection)GetValue(RunningDirectionProperty);
        set => SetValue(RunningDirectionProperty, value);
    }

    private void UpdateContent()
    {
        if (_elementContent == null || _elementPanel == null) return;
        if (MathHelper.IsZero(_elementPanel.ActualWidth) || MathHelper.IsZero(_elementPanel.ActualHeight)) return;

        _storyboard?.Stop();

        double from;
        double to;
        PropertyPath propertyPath;

        if (Orientation == Orientation.Horizontal)
        {
            if (AutoRun && _elementPanel.ActualWidth < ActualWidth)
            {
                return;
            }

            if (Runaway)
            {
                from = -_elementPanel.ActualWidth;
                to = ActualWidth;
            }
            else
            {
                from = 0;
                to = ActualWidth - _elementPanel.ActualWidth;
                SetCurrentValue(AutoReverseProperty, true);
            }
            propertyPath = new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(TranslateTransform.X)");
        }
        else
        {
            if (AutoRun && _elementPanel.ActualHeight < ActualHeight)
            {
                return;
            }

            if (Runaway)
            {
                from = -_elementPanel.ActualHeight;
                to = ActualHeight;
            }
            else
            {
                from = 0;
                to = ActualHeight - _elementPanel.ActualHeight;
                SetCurrentValue(AutoReverseProperty, true);
            }
            propertyPath = new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(TranslateTransform.Y)");
        }

        var duration = double.IsNaN(Speed)
            ? Duration
            : !MathHelper.IsVerySmall(Speed)
                ? TimeSpan.FromSeconds(Math.Abs(to - from) / Speed)
                : Duration;

        var animation = RunningDirection is RunningDirection.EndToStart
            ? new DoubleAnimation(to, from, duration)
            : new DoubleAnimation(from, to, duration);
        animation.RepeatBehavior = RepeatBehavior.Forever;
        animation.AutoReverse = AutoReverse;

        Storyboard.SetTargetProperty(animation, propertyPath);
        Storyboard.SetTarget(animation, _elementContent);

        _storyboard = new Storyboard();
        _storyboard.Children.Add(animation);
        _storyboard.Begin();
    }
}

file static class MathHelper
{
    public const double Epsilon = 2.2204460492503131e-016;

    public static bool IsZero(double value) => Math.Abs(value) < 10.0 * Epsilon;

    public static bool IsVerySmall(double value) => Math.Abs(value) < 1E-06;
}
