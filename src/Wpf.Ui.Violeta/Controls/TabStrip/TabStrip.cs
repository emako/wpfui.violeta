using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Wpf.Ui.Violeta.Controls;

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
            // have its properties set. Replace it with a fresh, unfrozen instance.
            _indicator.RenderTransform = new ScaleTransform(0, 1);

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

        _indicator.Margin = new Thickness(itemLeft, 0, 0, 0);
        _indicator.Width = itemWidth;

        if (animate)
        {
            var storyboard = new Storyboard();

            var scaleAnim = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(180),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };

            Storyboard.SetTarget(scaleAnim, _indicator);
            Storyboard.SetTargetProperty(scaleAnim, new PropertyPath("RenderTransform.ScaleX"));
            storyboard.Children.Add(scaleAnim);
            storyboard.Begin();
        }
        else
        {
            if (_indicator.RenderTransform is ScaleTransform scale)
                scale.ScaleX = 1;
        }
    }
}
