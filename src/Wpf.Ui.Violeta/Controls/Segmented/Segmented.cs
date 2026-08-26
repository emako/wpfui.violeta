using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Selects how the <see cref="Segmented"/> accent underline transitions between segments.
/// </summary>
public enum SegmentedIndicatorAnimation
{
    /// <summary>
    /// Windows Fluent Design follow transition: the indicator slides toward the new segment
    /// while stretching horizontally, then settles back to its resting width.
    /// </summary>
    Fluent,

    /// <summary>
    /// The indicator jumps to the new segment's position immediately, then grows into view
    /// via a horizontal scale animation (0 → 1) with an ease-out deceleration.
    /// </summary>
    Lengthening,
}

/// <summary>
/// A compact mutually exclusive selector styled after WinUI <c>Segmented</c>
/// (subtle selected fill plus an accent underline), as used by UniGetUI's view-mode switcher.
/// </summary>
/// <remarks>
/// Inherits <see cref="ListBox"/> so <see cref="System.Windows.Controls.Primitives.Selector.SelectedIndex"/>,
/// <see cref="System.Windows.Controls.Primitives.Selector.SelectedItem"/>, and
/// <see cref="System.Windows.Controls.Primitives.Selector.SelectionChanged"/> work as usual.
/// Selection is always single; clearing the current segment is not allowed.
/// The accent underline uses <see cref="IndicatorAnimation"/> (default: lengthening).
/// </remarks>
[TemplatePart(Name = PartIndicator, Type = typeof(FrameworkElement))]
public class Segmented : ListBox
{
    private const string PartIndicator = "PART_Indicator";
    private const double IndicatorEdgeMargin = 3;
    private const double IndicatorLength = 16;
    private const double IndicatorThickness = 3;
    private static readonly Thickness HorizontalItemPadding = new(10, 5, 10, 7);
    private static readonly Thickness VerticalItemPadding = new(12, 5, 10, 5);
    private const double ShellPadding = 0;
    private const double ItemInnerCornerRadius = 4;
    private const double IndicatorPressedMinScale = 0.625;

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

    public static readonly DependencyProperty IndicatorAnimationProperty = DependencyProperty.Register(
        nameof(IndicatorAnimation),
        typeof(SegmentedIndicatorAnimation),
        typeof(Segmented),
        new FrameworkPropertyMetadata(SegmentedIndicatorAnimation.Lengthening));

    /// <summary>
    /// Controls how the accent underline transitions between segments.
    /// Defaults to <see cref="SegmentedIndicatorAnimation.Lengthening"/>.
    /// </summary>
    public SegmentedIndicatorAnimation IndicatorAnimation
    {
        get => (SegmentedIndicatorAnimation)GetValue(IndicatorAnimationProperty);
        set => SetValue(IndicatorAnimationProperty, value);
    }

    /// <summary>Identifies the <see cref="Orientation"/> dependency property.</summary>
    public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
        nameof(Orientation),
        typeof(Orientation),
        typeof(Segmented),
        new FrameworkPropertyMetadata(
            Orientation.Horizontal,
            FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange,
            OnOrientationChanged));

    /// <summary>
    /// Gets or sets whether segments are arranged horizontally (default) or vertically.
    /// Vertical layout places the accent indicator on the left edge of the selected segment.
    /// </summary>
    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    private bool IsVertical => Orientation == Orientation.Vertical;

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

        // Template Freezables are immutable — install a fresh transform for axis stretch.
        _indicator.RenderTransform = new ScaleTransform(1, 1);
        ApplyIndicatorOrientation();

        ItemContainerGenerator.StatusChanged -= OnItemContainerGeneratorStatusChanged;
        ItemContainerGenerator.StatusChanged += OnItemContainerGeneratorStatusChanged;
        SizeChanged -= OnSegmentedSizeChanged;
        SizeChanged += OnSegmentedSizeChanged;
        UpdateItemVisuals();
        UpdateIndicatorPosition(animate: false);
    }

    /// <summary>
    /// Scales the bottom accent indicator while the selected segment is pressed.
    /// </summary>
    internal void SetIndicatorPressed(bool pressed)
    {
        if (_indicator?.RenderTransform is not ScaleTransform scale)
        {
            return;
        }

        var scaleProperty = GetIndicatorScaleProperty();
        scale.BeginAnimation(scaleProperty, null);

        var scaleAnim = new DoubleAnimation
        {
            To = pressed ? IndicatorPressedMinScale : 1.0,
            Duration = TimeSpan.FromMilliseconds(167),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
        };
        scale.BeginAnimation(scaleProperty, scaleAnim);
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

    private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Segmented segmented)
        {
            return;
        }

        segmented.ApplyIndicatorOrientation();
        segmented.UpdateItemVisuals();
        segmented.UpdateIndicatorPosition(animate: false);
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

            item.Padding = IsVertical ? VerticalItemPadding : HorizontalItemPadding;
            item.SelectionCornerRadius = GetSelectionCornerRadius(index, count, outerRadius, IsVertical);
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

    private static CornerRadius GetSelectionCornerRadius(int index, int count, double outerRadius, bool isVertical)
    {
        if (count == 1)
        {
            return new CornerRadius(outerRadius);
        }

        if (isVertical)
        {
            if (index == 0)
            {
                return new CornerRadius(outerRadius, outerRadius, ItemInnerCornerRadius, ItemInnerCornerRadius);
            }

            if (index == count - 1)
            {
                return new CornerRadius(ItemInnerCornerRadius, ItemInnerCornerRadius, outerRadius, outerRadius);
            }

            return new CornerRadius(ItemInnerCornerRadius);
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
        if (_indicatorStoryboard is not null)
        {
            return;
        }

        if (IsVertical ? e.HeightChanged : e.WidthChanged)
        {
            UpdateIndicatorPosition(animate: false);
        }
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

        if (!container.IsLoaded || !HasValidContainerSize(container))
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

        var targetMargin = CreateIndicatorMargin(origin, container);

        _indicator.Opacity = 1;

        if (!animate)
        {
            ApplyIndicatorBounds(targetMargin);
            return;
        }

        switch (IndicatorAnimation)
        {
            case SegmentedIndicatorAnimation.Fluent:
                AnimateFluent(targetMargin);
                break;

            case SegmentedIndicatorAnimation.Lengthening:
            default:
                AnimateLengthening(targetMargin);
                break;
        }
    }

    private void ApplyIndicatorBounds(Thickness margin)
    {
        if (_indicator is null)
        {
            return;
        }

        StopIndicatorAnimation(applyCurrent: false);
        _indicator.Margin = margin;

        if (_indicator.RenderTransform is ScaleTransform resetScale)
        {
            resetScale.ScaleX = 1;
            resetScale.ScaleY = 1;
        }
    }

    private void ResetIndicatorScale(ScaleTransform scale, double value)
    {
        scale.BeginAnimation(GetIndicatorScaleProperty(), null);
        SetIndicatorScale(scale, value);
    }

    private void ApplyIndicatorOrientation()
    {
        if (_indicator is null)
        {
            return;
        }

        if (IsVertical)
        {
            _indicator.Width = IndicatorThickness;
            _indicator.Height = IndicatorLength;
            _indicator.HorizontalAlignment = HorizontalAlignment.Left;
            _indicator.VerticalAlignment = VerticalAlignment.Top;
            _indicator.RenderTransformOrigin = new Point(0, 0.5);
        }
        else
        {
            _indicator.Width = IndicatorLength;
            _indicator.Height = IndicatorThickness;
            _indicator.HorizontalAlignment = HorizontalAlignment.Left;
            _indicator.VerticalAlignment = VerticalAlignment.Bottom;
            _indicator.RenderTransformOrigin = new Point(0.5, 1);
        }

        if (_indicator.RenderTransform is ScaleTransform scale)
        {
            scale.ScaleX = 1;
            scale.ScaleY = 1;
        }
    }

    private bool HasValidContainerSize(FrameworkElement container) =>
        IsVertical ? container.ActualHeight > 0 : container.ActualWidth > 0;

    private Thickness CreateIndicatorMargin(Point origin, FrameworkElement container)
    {
        var pillLength = GetPillLength();

        if (IsVertical)
        {
            var top = origin.Y + ((container.ActualHeight - pillLength) / 2);
            return new Thickness(IndicatorEdgeMargin, top, 0, 0);
        }

        var left = origin.X + ((container.ActualWidth - pillLength) / 2);
        return new Thickness(left, 0, 0, IndicatorEdgeMargin);
    }

    private double GetPillLength()
    {
        if (_indicator is null)
        {
            return IndicatorLength;
        }

        if (IsVertical)
        {
            if (!double.IsNaN(_indicator.Height) && _indicator.Height > 0)
            {
                return _indicator.Height;
            }

            return _indicator.ActualHeight > 0 ? _indicator.ActualHeight : IndicatorLength;
        }

        if (!double.IsNaN(_indicator.Width) && _indicator.Width > 0)
        {
            return _indicator.Width;
        }

        return _indicator.ActualWidth > 0 ? _indicator.ActualWidth : IndicatorLength;
    }

    private static double GetIndicatorPrimaryMargin(Thickness margin, bool isVertical) =>
        isVertical ? margin.Top : margin.Left;

    private DependencyProperty GetIndicatorScaleProperty() =>
        IsVertical ? ScaleTransform.ScaleYProperty : ScaleTransform.ScaleXProperty;

    private string GetIndicatorScalePath() =>
        IsVertical ? "RenderTransform.ScaleY" : "RenderTransform.ScaleX";

    private double GetIndicatorScale(ScaleTransform scale) =>
        IsVertical ? scale.ScaleY : scale.ScaleX;

    private void SetIndicatorScale(ScaleTransform scale, double value)
    {
        if (IsVertical)
        {
            scale.ScaleY = value;
        }
        else
        {
            scale.ScaleX = value;
        }
    }

    private void AnimateFluent(Thickness targetMargin)
    {
        if (_indicator is null)
        {
            return;
        }

        StopIndicatorAnimation(applyCurrent: true);

        var fromPrimary = GetIndicatorPrimaryMargin(_indicator.Margin, IsVertical);
        var targetPrimary = GetIndicatorPrimaryMargin(targetMargin, IsVertical);
        var delta = Math.Abs(targetPrimary - fromPrimary);
        if (delta < 0.5)
        {
            ApplyIndicatorBounds(targetMargin);
            return;
        }

        var duration = TimeSpan.FromMilliseconds(280);
        var stretch = 1.0 + Math.Min(0.75, Math.Max(0.22, delta / 56.0));
        var fromScale = _indicator.RenderTransform is ScaleTransform scale
            ? GetIndicatorScale(scale)
            : 1;

        var storyboard = CreateIndicatorStoryboard();

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
        Storyboard.SetTargetProperty(scaleAnim, new PropertyPath(GetIndicatorScalePath()));
        storyboard.Children.Add(scaleAnim);

        _indicatorStoryboard = storyboard;
        storyboard.Begin();
    }

    private void AnimateLengthening(Thickness targetMargin)
    {
        if (_indicator is null)
        {
            return;
        }

        ApplyIndicatorBounds(targetMargin);

        var storyboard = CreateIndicatorStoryboard();
        var scaleAnim = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromMilliseconds(180),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
        };

        Storyboard.SetTarget(scaleAnim, _indicator);
        Storyboard.SetTargetProperty(scaleAnim, new PropertyPath(GetIndicatorScalePath()));
        storyboard.Children.Add(scaleAnim);

        _indicatorStoryboard = storyboard;
        storyboard.Begin();
    }

    private Storyboard CreateIndicatorStoryboard()
    {
        var storyboard = new Storyboard();
        storyboard.Completed += (_, _) =>
        {
            if (ReferenceEquals(_indicatorStoryboard, storyboard))
            {
                _indicatorStoryboard = null;
            }
        };

        return storyboard;
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
                ? GetIndicatorScale(scale)
                : 1;

            _indicatorStoryboard?.Stop();
            _indicatorStoryboard = null;
            _indicator.BeginAnimation(MarginProperty, null);
            _indicator.Margin = currentMargin;
            if (_indicator.RenderTransform is ScaleTransform liveScale)
            {
                ResetIndicatorScale(liveScale, currentScale);
            }

            return;
        }

        _indicatorStoryboard?.Stop();
        _indicatorStoryboard = null;
        _indicator.BeginAnimation(MarginProperty, null);
        if (_indicator.RenderTransform is ScaleTransform resetScale)
        {
            ResetIndicatorScale(resetScale, applyCurrent ? GetIndicatorScale(resetScale) : 1);
        }
    }
}
