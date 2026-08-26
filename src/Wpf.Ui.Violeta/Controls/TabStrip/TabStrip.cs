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
/// Set <see cref="Orientation"/> to <see cref="Orientation.Vertical"/> for a side tab strip
/// (left accent line and right separator in the default/Card styles).
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
    /// Gets or sets whether the 1 px separator line under the tab strip is shown.
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

    public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
        nameof(Orientation),
        typeof(Orientation),
        typeof(TabStrip),
        new FrameworkPropertyMetadata(
            Orientation.Horizontal,
            FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange,
            OnOrientationChanged));

    /// <summary>
    /// Arranges tab items horizontally (default) or vertically. Vertical mode places the line
    /// indicator on the left and the separator on the right in the default and Card styles.
    /// </summary>
    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <summary>
    /// Button style uses a <see cref="Border"/> indicator; line styles use a <see cref="Rectangle"/>.
    /// </summary>
    private bool IsFillIndicator => _indicator is Border;

    private bool IsVerticalOrientation => Orientation == Orientation.Vertical;

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

    private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TabStrip tabStrip)
        {
            tabStrip.Dispatcher.BeginInvoke(() => tabStrip.UpdateIndicatorPosition(animate: false));
        }
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
            if (IsVerticalOrientation)
            {
                _indicator.Height = 0;
            }
            else
            {
                _indicator.Width = 0;
            }

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
        var targetMargin = BuildTargetMargin(origin);

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

    private bool HasValidContainerSize(FrameworkElement container)
    {
        if (IsFillIndicator)
        {
            return container.ActualWidth > 0 && container.ActualHeight > 0;
        }

        return IsVerticalOrientation
            ? container.ActualHeight > 0
            : container.ActualWidth > 0;
    }

    private Thickness BuildTargetMargin(Point origin)
    {
        if (IsFillIndicator || IsVerticalOrientation)
        {
            return new Thickness(origin.X, origin.Y, 0, 0);
        }

        return new Thickness(origin.X, 0, 0, 0);
    }

    private void ApplyIndicatorBounds(Thickness margin, double width, double height)
    {
        if (_indicator is null)
        {
            return;
        }

        StopIndicatorAnimation(applyCurrent: false);
        _indicator.Margin = margin;

        if (IsFillIndicator)
        {
            _indicator.Width = width;
            _indicator.Height = height;
        }
        else if (IsVerticalOrientation)
        {
            _indicator.Height = height;
        }
        else
        {
            _indicator.Width = width;
        }

        if (_indicator.RenderTransform is ScaleTransform resetScale)
        {
            resetScale.ScaleX = 1;
            resetScale.ScaleY = 1;
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

        var delta = GetFluentAnimationDelta(targetMargin, itemWidth, itemHeight);
        if (delta < 0.5)
        {
            ApplyIndicatorBounds(targetMargin, itemWidth, itemHeight);
            return;
        }

        var easing = new PowerEase { Power = 8, EasingMode = EasingMode.EaseOut };
        var duration = TimeSpan.FromMilliseconds(300);
        var storyboard = CreateIndicatorStoryboard();

        if (IsFillIndicator || !IsVerticalOrientation)
        {
            var widthAnim = new DoubleAnimation
            {
                To = itemWidth,
                Duration = duration,
                EasingFunction = easing,
            };
            Storyboard.SetTarget(widthAnim, _indicator);
            Storyboard.SetTargetProperty(widthAnim, new PropertyPath(WidthProperty));
            storyboard.Children.Add(widthAnim);
        }

        if (IsFillIndicator || IsVerticalOrientation)
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

        var marginAnim = new ThicknessAnimation
        {
            To = targetMargin,
            Duration = duration,
            EasingFunction = easing,
        };
        Storyboard.SetTarget(marginAnim, _indicator);
        Storyboard.SetTargetProperty(marginAnim, new PropertyPath(MarginProperty));
        storyboard.Children.Add(marginAnim);

        _indicatorStoryboard = storyboard;
        storyboard.Begin();
    }

    private double GetFluentAnimationDelta(Thickness targetMargin, double itemWidth, double itemHeight)
    {
        if (_indicator is null)
        {
            return 0;
        }

        if (IsFillIndicator)
        {
            return Math.Abs(targetMargin.Left - _indicator.Margin.Left)
                + Math.Abs(targetMargin.Top - _indicator.Margin.Top)
                + Math.Abs(itemWidth - _indicator.Width)
                + Math.Abs(itemHeight - _indicator.Height);
        }

        if (IsVerticalOrientation)
        {
            return Math.Abs(targetMargin.Top - _indicator.Margin.Top)
                + Math.Abs(itemHeight - _indicator.Height);
        }

        return Math.Abs(targetMargin.Left - _indicator.Margin.Left)
            + Math.Abs(itemWidth - _indicator.Width);
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
        Storyboard.SetTargetProperty(
            scaleAnim,
            new PropertyPath(IsVerticalOrientation ? "RenderTransform.ScaleY" : "RenderTransform.ScaleX"));
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
            var currentScaleX = 1.0;
            var currentScaleY = 1.0;
            if (_indicator.RenderTransform is ScaleTransform scale)
            {
                currentScaleX = scale.ScaleX;
                currentScaleY = scale.ScaleY;
            }

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
            else if (IsVerticalOrientation && !double.IsNaN(currentHeight))
            {
                _indicator.Height = currentHeight;
            }
            else if (!IsVerticalOrientation && !double.IsNaN(currentWidth))
            {
                _indicator.Width = currentWidth;
            }

            if (_indicator.RenderTransform is ScaleTransform liveScale)
            {
                liveScale.BeginAnimation(ScaleTransform.ScaleXProperty, null);
                liveScale.BeginAnimation(ScaleTransform.ScaleYProperty, null);
                liveScale.ScaleX = currentScaleX;
                liveScale.ScaleY = currentScaleY;
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
            resetScale.BeginAnimation(ScaleTransform.ScaleYProperty, null);
            resetScale.ScaleX = 1;
            resetScale.ScaleY = 1;
        }
    }
}
