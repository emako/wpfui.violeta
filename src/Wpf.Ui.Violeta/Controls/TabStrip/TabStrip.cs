using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Selects how the <see cref="TabStrip"/> selection indicator (the underline
/// bar in the default/Card styles) transitions between tabs.
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
public class TabStrip : ListBox
{
    private const string PartIndicator = "PART_Indicator";

    private FrameworkElement? _indicator;

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
        _indicator = GetTemplateChild(PartIndicator) as FrameworkElement;

        if (_indicator is not null)
        {
            // The ScaleTransform declared inline in the template has no bindings,
            // so WPF freezes it for perf — a frozen Freezable can't be animated or
            // have its properties set. Replace it with a fresh, unfrozen instance
            // (used by the Lengthening animation).
            _indicator.RenderTransform = new ScaleTransform(1, 1);

            // Containers may not exist yet at this point (they're generated
            // asynchronously), so an immediate attempt often no-ops. Re-run
            // once the generator reports containers are ready.
            ItemContainerGenerator.StatusChanged -= OnItemContainerGeneratorStatusChanged;
            ItemContainerGenerator.StatusChanged += OnItemContainerGeneratorStatusChanged;
            UpdateIndicatorPosition(animate: false);
        }
    }

    private void OnItemContainerGeneratorStatusChanged(object? sender, EventArgs e)
    {
        if (ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
            return;

        // Defer again so the newly generated containers have gone through a
        // layout pass and report their real ActualWidth.
        Dispatcher.BeginInvoke(() => UpdateIndicatorPosition(animate: false));
    }

    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        base.OnSelectionChanged(e);

        if (_indicator is null)
            return;

        // Defer to let the layout update so containers have their final sizes
        Dispatcher.BeginInvoke(() => UpdateIndicatorPosition(animate: true));
    }

    private void UpdateIndicatorPosition(bool animate)
    {
        if (_indicator is null)
            return;

        var selectedItem = SelectedItem;
        if (selectedItem is null)
        {
            _indicator.Width = 0;
            return;
        }

        var container = ItemContainerGenerator.ContainerFromItem(selectedItem) as FrameworkElement;

        if (container?.IsLoaded != true)
            return;

        var itemWidth = container.ActualWidth;
        var transform = container.TransformToVisual(this);
        var itemLeft = transform.Transform(default).X;

        if (!animate)
        {
            _indicator.Margin = new Thickness(itemLeft, 0, 0, 0);
            _indicator.Width = itemWidth;

            if (_indicator.RenderTransform is ScaleTransform resetScale)
                resetScale.ScaleX = 1;

            return;
        }

        switch (IndicatorAnimation)
        {
            case TabStripIndicatorAnimation.Lengthening:
                AnimateLengthening(itemLeft, itemWidth);
                break;

            case TabStripIndicatorAnimation.Fluent:
            default:
                AnimateFluent(itemLeft, itemWidth);
                break;
        }
    }

    /// <summary>
    /// Fluent Design "follow" transition — width and position animate directly
    /// and simultaneously, mirroring YouiToolkit's CheckTitleButton PART_Rect
    /// animation: a steep PowerEase EaseOut so most of the travel happens
    /// immediately and only the tail eases out, reading as quick rather than
    /// sluggish.
    /// </summary>
    private void AnimateFluent(double itemLeft, double itemWidth)
    {
        if (_indicator is null)
            return;

        var easing = new PowerEase { Power = 8, EasingMode = EasingMode.EaseOut };
        var duration = TimeSpan.FromMilliseconds(300);

        var storyboard = new Storyboard();

        var widthAnim = new DoubleAnimation
        {
            From = _indicator.ActualWidth,
            To = itemWidth,
            Duration = duration,
            EasingFunction = easing
        };
        Storyboard.SetTarget(widthAnim, _indicator);
        Storyboard.SetTargetProperty(widthAnim, new PropertyPath(WidthProperty));
        storyboard.Children.Add(widthAnim);

        var marginAnim = new ThicknessAnimation
        {
            From = _indicator.Margin,
            To = new Thickness(itemLeft, 0, 0, 0),
            Duration = duration,
            EasingFunction = easing
        };
        Storyboard.SetTarget(marginAnim, _indicator);
        Storyboard.SetTargetProperty(marginAnim, new PropertyPath(MarginProperty));
        storyboard.Children.Add(marginAnim);

        storyboard.Begin();
    }

    /// <summary>
    /// Lengthening transition — the indicator jumps straight to the new tab's
    /// position/width, then grows into view via a horizontal scale (0 → 1)
    /// with a steep ease-out: fast at the start, slowing into place, rather
    /// than building up speed toward the end.
    /// </summary>
    private void AnimateLengthening(double itemLeft, double itemWidth)
    {
        if (_indicator is null)
            return;

        _indicator.Margin = new Thickness(itemLeft, 0, 0, 0);
        _indicator.Width = itemWidth;

        var storyboard = new Storyboard();

        var scaleAnim = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromMilliseconds(180),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        Storyboard.SetTarget(scaleAnim, _indicator);
        Storyboard.SetTargetProperty(scaleAnim, new PropertyPath("RenderTransform.ScaleX"));
        storyboard.Children.Add(scaleAnim);
        storyboard.Begin();
    }
}
