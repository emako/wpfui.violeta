using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Selects how the <see cref="TabStrip"/> selection indicator transitions
/// between tabs — the underline in the default/Card styles, or the filled
/// pill in the Button style.
/// </summary>
public enum TabStripIndicatorAnimation
{
    /// <summary>
    /// Windows Fluent Design "follow" transition: width and position animate
    /// directly and simultaneously with a steep ease-out, so the indicator
    /// slides and stretches to the newly selected tab in one motion.
    /// </summary>
    Fluent,

    /// <summary>
    /// The indicator jumps to the new tab's position/width immediately, then
    /// grows into view via a horizontal scale animation (0 → 1), fast first
    /// then slowing into place.
    /// </summary>
    Lengthening,
}

/// <summary>
/// A standalone tab header strip with no content area.
/// Mirrors the behaviour of Semi.Avalonia's <c>TabStrip</c>.
/// </summary>
/// <remarks>
/// Three visual variants are available via style keys:
/// <list type="bullet">
///   <item><description>Default (line underline): applied automatically — no style key needed.</description></item>
///   <item><description>Card: <c>Style="{DynamicResource CardTabStripStyle}"</c></description></item>
///   <item><description>Button (filled pill): <c>Style="{DynamicResource ButtonTabStripStyle}"</c></description></item>
/// </list>
/// All variants support <see cref="System.Windows.Controls.ItemsControl.ItemsSource"/> and data binding.
/// </remarks>
[TemplatePart(Name = PartIndicator, Type = typeof(FrameworkElement))]
public class TabStrip : ListBox
{
    private const string PartIndicator = "PART_Indicator";

    private FrameworkElement? _indicator;
    private Storyboard? _indicatorStoryboard;

    public static readonly DependencyProperty IsSelectedItemBoldProperty = DependencyProperty.Register(
        nameof(IsSelectedItemBold),
        typeof(bool),
        typeof(TabStrip),
        new FrameworkPropertyMetadata(false));

    public bool IsSelectedItemBold
    {
        get => (bool)GetValue(IsSelectedItemBoldProperty);
        set => SetValue(IsSelectedItemBoldProperty, value);
    }

    public static readonly DependencyProperty IsSeparatorVisibleProperty = DependencyProperty.Register(
        nameof(IsSeparatorVisible),
        typeof(bool),
        typeof(TabStrip),
        new FrameworkPropertyMetadata(true));

    /// <summary>
    /// Gets or sets whether the 1 px horizontal separator line under the tab strip is shown.
    /// Defaults to <see langword="true"/>. Applies to the default and Card styles; the Button
    /// style has no separator.
    /// </summary>
    public bool IsSeparatorVisible
    {
        get => (bool)GetValue(IsSeparatorVisibleProperty);
        set => SetValue(IsSeparatorVisibleProperty, value);
    }

    public static readonly DependencyProperty IndicatorAnimationProperty = DependencyProperty.Register(
        nameof(IndicatorAnimation),
        typeof(TabStripIndicatorAnimation),
        typeof(TabStrip),
        new FrameworkPropertyMetadata(TabStripIndicatorAnimation.Fluent));

    /// <summary>
    /// Controls how the selection indicator transitions between tabs.
    /// Defaults to <see cref="TabStripIndicatorAnimation.Fluent"/>.
    /// </summary>
    public TabStripIndicatorAnimation IndicatorAnimation
    {
        get => (TabStripIndicatorAnimation)GetValue(IndicatorAnimationProperty);
        set => SetValue(IndicatorAnimationProperty, value);
    }

    /// <summary>
    /// Button-style pill sits with <see cref="VerticalAlignment.Top"/> and matches
    /// the selected item's bounds. Line/Card keep a 2 px bar aligned to the bottom.
    /// </summary>
    private bool IsFillIndicator =>
        _indicator is not null && _indicator.VerticalAlignment != VerticalAlignment.Bottom;

    static TabStrip()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(TabStrip),
            new FrameworkPropertyMetadata(typeof(TabStrip)));

        // Always single-selection — overriding prevents users from accidentally
        // setting Extended/Multiple mode which makes no sense for a tab strip.
        SelectionModeProperty.OverrideMetadata(
            typeof(TabStrip),
            new FrameworkPropertyMetadata(SelectionMode.Single));
    }

    protected override DependencyObject GetContainerForItemOverride() => new TabStripItem();

    protected override bool IsItemItsOwnContainerOverride(object item) => item is TabStripItem;

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _indicatorStoryboard?.Stop();
        _indicatorStoryboard = null;
        _indicator = GetTemplateChild(PartIndicator) as FrameworkElement;

        SizeChanged -= OnTabStripSizeChanged;
        ItemContainerGenerator.StatusChanged -= OnItemContainerGeneratorStatusChanged;

        if (_indicator is null)
        {
            return;
        }

        // The ScaleTransform declared inline in the template has no bindings,
        // so WPF freezes it for perf — a frozen Freezable can't be animated or
        // have its properties set. Replace it with a fresh, unfrozen instance
        // (used by the Lengthening animation).
        _indicator.RenderTransform = new ScaleTransform(1, 1);

        // Containers may not exist yet at this point (they're generated
        // asynchronously), so an immediate attempt often no-ops. Re-run
        // once the generator reports containers are ready.
        ItemContainerGenerator.StatusChanged += OnItemContainerGeneratorStatusChanged;
        SizeChanged += OnTabStripSizeChanged;
        UpdateIndicatorPosition(animate: false);
    }

    private void OnTabStripSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (!e.WidthChanged && !e.HeightChanged)
        {
            return;
        }

        if (_indicatorStoryboard is not null)
        {
            return;
        }

        UpdateIndicatorPosition(animate: false);
    }

    private void OnItemContainerGeneratorStatusChanged(object? sender, EventArgs e)
    {
        if (ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
        {
            return;
        }

        // Defer again so the newly generated containers have gone through a
        // layout pass and report their real ActualWidth.
        Dispatcher.BeginInvoke(() => UpdateIndicatorPosition(animate: false));
    }

    private void OnContainerLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            element.Loaded -= OnContainerLoaded;
        }

        UpdateIndicatorPosition(animate: false);
    }

    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        base.OnSelectionChanged(e);

        if (_indicator is null)
        {
            return;
        }

        // Defer to let the layout update so containers have their final sizes
        Dispatcher.BeginInvoke(() => UpdateIndicatorPosition(animate: true));
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
            StopIndicatorAnimation(applyCurrent: false);
            _indicator.Width = 0;
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

        // Indicator lives in a padded host for the Button style — measure
        // against its parent, not the TabStrip itself, or the pill sits offset.
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

        var itemWidth = container.ActualWidth;
        var itemHeight = container.ActualHeight;
        var targetMargin = IsFillIndicator
            ? new Thickness(origin.X, origin.Y, 0, 0)
            : new Thickness(origin.X, 0, 0, 0);

        if (!animate)
        {
            ApplyIndicatorBounds(targetMargin, itemWidth, itemHeight);
            return;
        }

        switch (IndicatorAnimation)
        {
            case TabStripIndicatorAnimation.Lengthening:
                AnimateLengthening(targetMargin, itemWidth, itemHeight);
                break;

            case TabStripIndicatorAnimation.Fluent:
            default:
                AnimateFluent(targetMargin, itemWidth, itemHeight);
                break;
        }
    }

    private void ApplyIndicatorBounds(Thickness margin, double width, double height)
    {
        if (_indicator is null)
        {
            return;
        }

        StopIndicatorAnimation(applyCurrent: false);
        _indicator.Margin = margin;
        _indicator.Width = width;

        if (IsFillIndicator)
        {
            _indicator.Height = height;
        }

        if (_indicator.RenderTransform is ScaleTransform resetScale)
        {
            resetScale.ScaleX = 1;
        }
    }

    /// <summary>
    /// Fluent Design "follow" transition — width and position animate directly
    /// and simultaneously: a steep PowerEase EaseOut so most of the travel happens
    /// immediately and only the tail eases out, reading as quick rather than
    /// sluggish.
    /// </summary>
    private void AnimateFluent(Thickness targetMargin, double itemWidth, double itemHeight)
    {
        if (_indicator is null)
        {
            return;
        }

        StopIndicatorAnimation(applyCurrent: true);

        var delta = Math.Abs(targetMargin.Left - _indicator.Margin.Left)
            + Math.Abs(itemWidth - _indicator.Width);
        if (delta < 0.5 && (!IsFillIndicator || Math.Abs(itemHeight - _indicator.Height) < 0.5))
        {
            ApplyIndicatorBounds(targetMargin, itemWidth, itemHeight);
            return;
        }

        var easing = new PowerEase { Power = 8, EasingMode = EasingMode.EaseOut };
        var duration = TimeSpan.FromMilliseconds(300);
        var storyboard = CreateIndicatorStoryboard();

        var widthAnim = new DoubleAnimation
        {
            To = itemWidth,
            Duration = duration,
            EasingFunction = easing,
        };
        Storyboard.SetTarget(widthAnim, _indicator);
        Storyboard.SetTargetProperty(widthAnim, new PropertyPath(WidthProperty));
        storyboard.Children.Add(widthAnim);

        var marginAnim = new ThicknessAnimation
        {
            To = targetMargin,
            Duration = duration,
            EasingFunction = easing,
        };
        Storyboard.SetTarget(marginAnim, _indicator);
        Storyboard.SetTargetProperty(marginAnim, new PropertyPath(MarginProperty));
        storyboard.Children.Add(marginAnim);

        if (IsFillIndicator)
        {
            var heightAnim = new DoubleAnimation
            {
                To = itemHeight,
                Duration = duration,
                EasingFunction = easing,
            };
            Storyboard.SetTarget(heightAnim, _indicator);
            Storyboard.SetTargetProperty(heightAnim, new PropertyPath(HeightProperty));
            storyboard.Children.Add(heightAnim);
        }

        _indicatorStoryboard = storyboard;
        storyboard.Begin();
    }

    /// <summary>
    /// Lengthening transition — the indicator jumps straight to the new tab's
    /// position/width, then grows into view via a horizontal scale (0 → 1)
    /// with a steep ease-out: fast at the start, slowing into place, rather
    /// than building up speed toward the end.
    /// </summary>
    private void AnimateLengthening(Thickness targetMargin, double itemWidth, double itemHeight)
    {
        if (_indicator is null)
        {
            return;
        }

        ApplyIndicatorBounds(targetMargin, itemWidth, itemHeight);

        var storyboard = CreateIndicatorStoryboard();
        var scaleAnim = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromMilliseconds(180),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
        };

        Storyboard.SetTarget(scaleAnim, _indicator);
        Storyboard.SetTargetProperty(scaleAnim, new PropertyPath("RenderTransform.ScaleX"));
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
            var currentWidth = (double)_indicator.GetValue(WidthProperty);
            var currentHeight = (double)_indicator.GetValue(HeightProperty);
            var currentScale = _indicator.RenderTransform is ScaleTransform scale
                ? scale.ScaleX
                : 1;

            _indicatorStoryboard?.Stop();
            _indicatorStoryboard = null;
            _indicator.BeginAnimation(MarginProperty, null);
            _indicator.BeginAnimation(WidthProperty, null);
            _indicator.BeginAnimation(HeightProperty, null);
            _indicator.Margin = currentMargin;
            _indicator.Width = currentWidth;

            if (IsFillIndicator && !double.IsNaN(currentHeight))
            {
                _indicator.Height = currentHeight;
            }

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
        _indicator.BeginAnimation(WidthProperty, null);
        _indicator.BeginAnimation(HeightProperty, null);
        if (_indicator.RenderTransform is ScaleTransform resetScale)
        {
            resetScale.BeginAnimation(ScaleTransform.ScaleXProperty, null);
            resetScale.ScaleX = 1;
        }
    }
}
