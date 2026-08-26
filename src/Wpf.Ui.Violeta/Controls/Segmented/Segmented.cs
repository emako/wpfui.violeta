using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// A compact mutually exclusive selector styled after WinUI <c>Segmented</c>
/// (subtle selected fill plus an accent underline), as used by UniGetUI's view-mode switcher.
/// </summary>
/// <remarks>
/// Inherits <see cref="ListBox"/> so <see cref="System.Windows.Controls.Primitives.Selector.SelectedIndex"/>,
/// <see cref="System.Windows.Controls.Primitives.Selector.SelectedItem"/>, and
/// <see cref="System.Windows.Controls.Primitives.Selector.SelectionChanged"/> work as usual.
/// Selection is always single; clearing the current segment is not allowed.
/// The selection pill slides between segments with a Fluent follow animation.
/// </remarks>
[TemplatePart(Name = PartIndicator, Type = typeof(FrameworkElement))]
public class Segmented : ListBox
{
    private const string PartIndicator = "PART_Indicator";
    private const double IndicatorBottomMargin = 3;
    private const double ShellPadding = 0;
    private const double ItemInnerCornerRadius = 4;

    private FrameworkElement? _indicator;
    private Storyboard? _indicatorStoryboard;
    private bool _restoringSelection;

    /// <summary>Identifies the <see cref="CornerRadius"/> dependency property.</summary>
    public static readonly DependencyProperty CornerRadiusProperty =
        Border.CornerRadiusProperty.AddOwner(
            typeof(Segmented),
            new FrameworkPropertyMetadata(
                new CornerRadius(4),
                FrameworkPropertyMetadataOptions.AffectsRender,
                OnCornerRadiusChanged));

    /// <summary>Gets or sets the corner radius of the segmented shell.</summary>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    static Segmented()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(Segmented),
            new FrameworkPropertyMetadata(typeof(Segmented)));

        SelectionModeProperty.OverrideMetadata(
            typeof(Segmented),
            new FrameworkPropertyMetadata(SelectionMode.Single));
    }

    protected override DependencyObject GetContainerForItemOverride() => new SegmentedItem();

    protected override bool IsItemItsOwnContainerOverride(object item) => item is SegmentedItem;

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _indicatorStoryboard?.Stop();
        _indicatorStoryboard = null;
        _indicator = GetTemplateChild(PartIndicator) as FrameworkElement;

        if (_indicator is null)
        {
            return;
        }

        // Template Freezables are immutable — install a fresh transform for ScaleX stretch.
        _indicator.RenderTransform = new ScaleTransform(1, 1);
        _indicator.RenderTransformOrigin = new Point(0.5, 1);

        ItemContainerGenerator.StatusChanged -= OnItemContainerGeneratorStatusChanged;
        ItemContainerGenerator.StatusChanged += OnItemContainerGeneratorStatusChanged;
        SizeChanged -= OnSegmentedSizeChanged;
        SizeChanged += OnSegmentedSizeChanged;
        UpdateItemVisuals();
        UpdateIndicatorPosition(animate: false);
    }

    protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
    {
        base.OnItemsChanged(e);
        Dispatcher.BeginInvoke(UpdateItemVisuals);
    }

    private static void OnCornerRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Segmented segmented)
        {
            segmented.UpdateItemVisuals();
        }
    }

    private void UpdateItemVisuals()
    {
        if (ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
        {
            return;
        }

        var count = Items.Count;
        if (count == 0)
        {
            return;
        }

        var outerRadius = GetShellInnerCornerRadius();

        for (var index = 0; index < count; index++)
        {
            if (ItemContainerGenerator.ContainerFromIndex(index) is not SegmentedItem item)
            {
                continue;
            }

            item.SelectionCornerRadius = GetSelectionCornerRadius(index, count, outerRadius);
        }
    }

    private double GetShellInnerCornerRadius()
    {
        var shellRadius = CornerRadius.TopLeft;
        if (CornerRadius.TopRight > 0)
        {
            shellRadius = Math.Min(shellRadius, CornerRadius.TopRight);
        }

        if (CornerRadius.BottomRight > 0)
        {
            shellRadius = Math.Min(shellRadius, CornerRadius.BottomRight);
        }

        if (CornerRadius.BottomLeft > 0)
        {
            shellRadius = Math.Min(shellRadius, CornerRadius.BottomLeft);
        }

        return Math.Max(0, shellRadius - ShellPadding);
    }

    private static CornerRadius GetSelectionCornerRadius(int index, int count, double outerRadius)
    {
        if (count == 1)
        {
            return new CornerRadius(outerRadius);
        }

        if (index == 0)
        {
            return new CornerRadius(outerRadius, ItemInnerCornerRadius, ItemInnerCornerRadius, outerRadius);
        }

        if (index == count - 1)
        {
            return new CornerRadius(ItemInnerCornerRadius, outerRadius, outerRadius, ItemInnerCornerRadius);
        }

        return new CornerRadius(ItemInnerCornerRadius);
    }

    private void OnSegmentedSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (!e.WidthChanged || _indicatorStoryboard is not null)
        {
            return;
        }

        UpdateIndicatorPosition(animate: false);
    }

    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        if (_restoringSelection)
        {
            base.OnSelectionChanged(e);
            return;
        }

        base.OnSelectionChanged(e);

        if (SelectedIndex < 0 && HasItems)
        {
            object? restore = e.RemovedItems.Count > 0 ? e.RemovedItems[0] : null;
            if (restore is not null && Items.Contains(restore))
            {
                _restoringSelection = true;
                try
                {
                    SelectedItem = restore;
                }
                finally
                {
                    _restoringSelection = false;
                }

                return;
            }
        }

        Dispatcher.BeginInvoke(() => UpdateIndicatorPosition(animate: true));
    }

    private void OnItemContainerGeneratorStatusChanged(object? sender, EventArgs e)
    {
        if (ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
        {
            return;
        }

        Dispatcher.BeginInvoke(() =>
        {
            UpdateItemVisuals();
            UpdateIndicatorPosition(animate: false);
        });
    }

    private void OnContainerLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            element.Loaded -= OnContainerLoaded;
        }

        UpdateIndicatorPosition(animate: false);
    }

    private void UpdateIndicatorPosition(bool animate)
    {
        if (_indicator is null)
        {
            return;
        }

        var selectedItem = SelectedItem;
        if (selectedItem is null)
        {
            _indicator.Opacity = 0;
            return;
        }

        var container = ItemContainerGenerator.ContainerFromItem(selectedItem) as FrameworkElement;
        if (container is null)
        {
            return;
        }

        if (!container.IsLoaded || container.ActualWidth <= 0)
        {
            container.Loaded -= OnContainerLoaded;
            container.Loaded += OnContainerLoaded;
            return;
        }

        var host = _indicator.Parent as Visual ?? this;
        Point origin;
        try
        {
            origin = container.TransformToVisual(host).Transform(new Point(0, 0));
        }
        catch (InvalidOperationException)
        {
            return;
        }

        var pillWidth = GetPillWidth();
        var left = origin.X + ((container.ActualWidth - pillWidth) / 2);
        var targetMargin = new Thickness(left, 0, 0, IndicatorBottomMargin);

        _indicator.Opacity = 1;

        if (!animate)
        {
            StopIndicatorAnimation(applyCurrent: false);
            _indicator.Margin = targetMargin;
            if (_indicator.RenderTransform is ScaleTransform scale)
            {
                scale.ScaleX = 1;
            }

            return;
        }

        AnimateIndicator(targetMargin);
    }

    private double GetPillWidth()
    {
        if (_indicator is null)
        {
            return 16;
        }

        if (!double.IsNaN(_indicator.Width) && _indicator.Width > 0)
        {
            return _indicator.Width;
        }

        return _indicator.ActualWidth > 0 ? _indicator.ActualWidth : 16;
    }

    private void AnimateIndicator(Thickness targetMargin)
    {
        if (_indicator is null)
        {
            return;
        }

        StopIndicatorAnimation(applyCurrent: true);

        var fromLeft = _indicator.Margin.Left;
        var delta = Math.Abs(targetMargin.Left - fromLeft);
        if (delta < 0.5)
        {
            _indicator.Margin = targetMargin;
            return;
        }

        var duration = TimeSpan.FromMilliseconds(280);
        var stretch = 1.0 + Math.Min(0.75, Math.Max(0.22, delta / 56.0));
        var fromScale = _indicator.RenderTransform is ScaleTransform scale
            ? scale.ScaleX
            : 1;

        var storyboard = new Storyboard();
        storyboard.Completed += (_, _) =>
        {
            if (ReferenceEquals(_indicatorStoryboard, storyboard))
            {
                _indicatorStoryboard = null;
            }
        };

        var marginAnim = new ThicknessAnimation
        {
            To = targetMargin,
            Duration = duration,
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
        };
        Storyboard.SetTarget(marginAnim, _indicator);
        Storyboard.SetTargetProperty(marginAnim, new PropertyPath(MarginProperty));
        storyboard.Children.Add(marginAnim);

        var scaleAnim = new DoubleAnimationUsingKeyFrames();
        scaleAnim.KeyFrames.Add(
            new EasingDoubleKeyFrame(fromScale, KeyTime.FromTimeSpan(TimeSpan.Zero)));
        scaleAnim.KeyFrames.Add(
            new EasingDoubleKeyFrame(stretch, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(90)))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
            });
        scaleAnim.KeyFrames.Add(
            new EasingDoubleKeyFrame(1.0, KeyTime.FromTimeSpan(duration))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut },
            });
        Storyboard.SetTarget(scaleAnim, _indicator);
        Storyboard.SetTargetProperty(scaleAnim, new PropertyPath("RenderTransform.ScaleX"));
        storyboard.Children.Add(scaleAnim);

        _indicatorStoryboard = storyboard;
        storyboard.Begin();
    }

    private void StopIndicatorAnimation(bool applyCurrent)
    {
        if (_indicator is null)
        {
            return;
        }

        if (applyCurrent)
        {
            var currentMargin = (Thickness)_indicator.GetValue(MarginProperty);
            var currentScale = _indicator.RenderTransform is ScaleTransform scale
                ? scale.ScaleX
                : 1;

            _indicatorStoryboard?.Stop();
            _indicatorStoryboard = null;
            _indicator.BeginAnimation(MarginProperty, null);
            _indicator.Margin = currentMargin;
            if (_indicator.RenderTransform is ScaleTransform liveScale)
            {
                liveScale.BeginAnimation(ScaleTransform.ScaleXProperty, null);
                liveScale.ScaleX = currentScale;
            }

            return;
        }

        _indicatorStoryboard?.Stop();
        _indicatorStoryboard = null;
        _indicator.BeginAnimation(MarginProperty, null);
        if (_indicator.RenderTransform is ScaleTransform resetScale)
        {
            resetScale.BeginAnimation(ScaleTransform.ScaleXProperty, null);
            if (!applyCurrent)
            {
                resetScale.ScaleX = 1;
            }
        }
    }
}
