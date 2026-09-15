using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Selects how the default <see cref="TabControl"/> accent underline transitions between tabs.
/// Mirrors <see cref="TabStripIndicatorAnimation"/>.
/// </summary>
public enum TabControlIndicatorAnimation
{
    /// <summary>
    /// Windows Fluent Design "follow" transition: width and position animate
    /// directly and simultaneously with a steep ease-out.
    /// </summary>
    Fluent,

    /// <summary>
    /// The indicator jumps to the new tab's position/width immediately, then
    /// grows into view via a scale animation (0 → 1).
    /// </summary>
    Lengthening,
}

/// <summary>
/// Attached behavior used by <c>TabControl.xaml</c> to drive the shared
/// sliding selection indicator (<c>PART_Indicator</c>), matching the default
/// <see cref="TabStrip"/> line style.
/// </summary>
public static class TabControlHelper
{
    private const string PartIndicator = "PART_Indicator";

    #region IsEnabled

    public static bool GetIsEnabled(TabControl element) =>
        (bool)element.GetValue(IsEnabledProperty);

    public static void SetIsEnabled(TabControl element, bool value) =>
        element.SetValue(IsEnabledProperty, value);

    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(TabControlHelper),
            new PropertyMetadata(false, OnIsEnabledChanged));

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TabControl tabControl || Equals(e.OldValue, e.NewValue))
        {
            return;
        }

        if (e.NewValue is true)
        {
            Attach(tabControl);
        }
        else
        {
            Detach(tabControl);
        }
    }

    #endregion

    #region IndicatorAnimation

    public static TabControlIndicatorAnimation GetIndicatorAnimation(TabControl element) =>
        (TabControlIndicatorAnimation)element.GetValue(IndicatorAnimationProperty);

    public static void SetIndicatorAnimation(TabControl element, TabControlIndicatorAnimation value) =>
        element.SetValue(IndicatorAnimationProperty, value);

    public static readonly DependencyProperty IndicatorAnimationProperty =
        DependencyProperty.RegisterAttached(
            "IndicatorAnimation",
            typeof(TabControlIndicatorAnimation),
            typeof(TabControlHelper),
            new PropertyMetadata(TabControlIndicatorAnimation.Fluent));

    #endregion

    #region IsSelectedItemBold

    public static bool GetIsSelectedItemBold(TabControl element) =>
        (bool)element.GetValue(IsSelectedItemBoldProperty);

    public static void SetIsSelectedItemBold(TabControl element, bool value) =>
        element.SetValue(IsSelectedItemBoldProperty, value);

    public static readonly DependencyProperty IsSelectedItemBoldProperty =
        DependencyProperty.RegisterAttached(
            "IsSelectedItemBold",
            typeof(bool),
            typeof(TabControlHelper),
            new PropertyMetadata(false));

    #endregion

    #region IsSeparatorVisible

    public static bool GetIsSeparatorVisible(TabControl element) =>
        (bool)element.GetValue(IsSeparatorVisibleProperty);

    public static void SetIsSeparatorVisible(TabControl element, bool value) =>
        element.SetValue(IsSeparatorVisibleProperty, value);

    public static readonly DependencyProperty IsSeparatorVisibleProperty =
        DependencyProperty.RegisterAttached(
            "IsSeparatorVisible",
            typeof(bool),
            typeof(TabControlHelper),
            new PropertyMetadata(true));

    #endregion

    #region Controller

    private static readonly DependencyProperty ControllerProperty =
        DependencyProperty.RegisterAttached(
            "Controller",
            typeof(TabControlIndicatorController),
            typeof(TabControlHelper));

    private static void Attach(TabControl tabControl)
    {
        if (tabControl.GetValue(ControllerProperty) is TabControlIndicatorController)
        {
            return;
        }

        var controller = new TabControlIndicatorController(tabControl);
        tabControl.SetValue(ControllerProperty, controller);
        controller.Attach();
    }

    private static void Detach(TabControl tabControl)
    {
        if (tabControl.GetValue(ControllerProperty) is TabControlIndicatorController controller)
        {
            controller.Detach();
            tabControl.ClearValue(ControllerProperty);
        }
    }

    #endregion

    private sealed class TabControlIndicatorController
    {
        private readonly TabControl _owner;
        private readonly EventHandler _onTemplateChanged;
        private readonly DependencyPropertyDescriptor _templateDescriptor;
        private readonly DependencyPropertyDescriptor _placementDescriptor;

        private FrameworkElement? _indicator;
        private Storyboard? _indicatorStoryboard;
        private bool _attached;

        public TabControlIndicatorController(TabControl owner)
        {
            _owner = owner;
            _onTemplateChanged = (_, _) => ApplyTemplateParts();
            _templateDescriptor = DependencyPropertyDescriptor.FromProperty(
                Control.TemplateProperty,
                typeof(TabControl));
            _placementDescriptor = DependencyPropertyDescriptor.FromProperty(
                TabControl.TabStripPlacementProperty,
                typeof(TabControl));
        }

        public void Attach()
        {
            if (_attached)
            {
                return;
            }

            _attached = true;
            _owner.Loaded += OnLoaded;
            _owner.SizeChanged += OnSizeChanged;
            _owner.SelectionChanged += OnSelectionChanged;
            _owner.ItemContainerGenerator.StatusChanged += OnItemContainerGeneratorStatusChanged;
            _templateDescriptor.AddValueChanged(_owner, _onTemplateChanged);
            _placementDescriptor.AddValueChanged(_owner, OnPlacementChanged);

            if (_owner.IsLoaded)
            {
                ApplyTemplateParts();
            }
        }

        public void Detach()
        {
            if (!_attached)
            {
                return;
            }

            _attached = false;
            _owner.Loaded -= OnLoaded;
            _owner.SizeChanged -= OnSizeChanged;
            _owner.SelectionChanged -= OnSelectionChanged;
            _owner.ItemContainerGenerator.StatusChanged -= OnItemContainerGeneratorStatusChanged;
            _templateDescriptor.RemoveValueChanged(_owner, _onTemplateChanged);
            _placementDescriptor.RemoveValueChanged(_owner, OnPlacementChanged);
            StopIndicatorAnimation(applyCurrent: false);
            _indicator = null;
        }

        private void OnLoaded(object sender, RoutedEventArgs e) => ApplyTemplateParts();

        private void OnPlacementChanged(object? sender, EventArgs e) =>
            _owner.Dispatcher.BeginInvoke(() =>
            {
                ApplyIndicatorOrientationChrome();
                UpdateIndicatorPosition(animate: false);
            });

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if ((!e.WidthChanged && !e.HeightChanged) || _indicatorStoryboard is not null)
            {
                return;
            }

            UpdateIndicatorPosition(animate: false);
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_indicator is null)
            {
                return;
            }

            _owner.Dispatcher.BeginInvoke(() => UpdateIndicatorPosition(animate: true));
        }

        private void OnItemContainerGeneratorStatusChanged(object? sender, EventArgs e)
        {
            if (_owner.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
            {
                return;
            }

            _owner.Dispatcher.BeginInvoke(() => UpdateIndicatorPosition(animate: false));
        }

        private void OnContainerLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element)
            {
                element.Loaded -= OnContainerLoaded;
            }

            UpdateIndicatorPosition(animate: false);
        }

        private void ApplyTemplateParts()
        {
            StopIndicatorAnimation(applyCurrent: false);
            _indicator = null;

            _owner.ApplyTemplate();
            if (_owner.Template is null)
            {
                return;
            }

            _indicator = _owner.Template.FindName(PartIndicator, _owner) as FrameworkElement;
            if (_indicator is null)
            {
                return;
            }

            _indicator.RenderTransform = new ScaleTransform(1, 1);
            ApplyIndicatorOrientationChrome();
            UpdateIndicatorPosition(animate: false);
        }

        private bool IsVerticalPlacement =>
            _owner.TabStripPlacement is Dock.Left or Dock.Right;

        /// <summary>
        /// Sets alignment and the fixed thickness axis only.
        /// Never clears the sliding dimension (Width/Height) — that would force
        /// Fluent animations to restart from 0 and look like a grow-in instead of a slide.
        /// </summary>
        private void ApplyIndicatorOrientationChrome()
        {
            if (_indicator is null)
            {
                return;
            }

            if (IsVerticalPlacement)
            {
                _indicator.Width = 2;
                _indicator.HorizontalAlignment = _owner.TabStripPlacement == Dock.Left
                    ? HorizontalAlignment.Right
                    : HorizontalAlignment.Left;
                _indicator.VerticalAlignment = VerticalAlignment.Top;
            }
            else
            {
                _indicator.Height = 2;
                _indicator.HorizontalAlignment = HorizontalAlignment.Left;
                _indicator.VerticalAlignment = _owner.TabStripPlacement == Dock.Bottom
                    ? VerticalAlignment.Top
                    : VerticalAlignment.Bottom;
            }
        }

        private void UpdateIndicatorPosition(bool animate)
        {
            if (_indicator is null)
            {
                return;
            }

            var selectedItem = _owner.SelectedItem;
            if (selectedItem is null)
            {
                StopIndicatorAnimation(applyCurrent: false);
                if (IsVerticalPlacement)
                {
                    _indicator.Height = 0;
                }
                else
                {
                    _indicator.Width = 0;
                }

                return;
            }

            var container = ResolveSelectedContainer(selectedItem);
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

            var host = _indicator.Parent as Visual ?? _owner;
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

            switch (GetIndicatorAnimation(_owner))
            {
                case TabControlIndicatorAnimation.Lengthening:
                    AnimateLengthening(targetMargin, itemWidth, itemHeight);
                    break;

                case TabControlIndicatorAnimation.Fluent:
                default:
                    AnimateFluent(targetMargin, itemWidth, itemHeight);
                    break;
            }
        }

        private FrameworkElement? ResolveSelectedContainer(object selectedItem)
        {
            if (_owner.ItemContainerGenerator.ContainerFromItem(selectedItem) is FrameworkElement fromItem)
            {
                return fromItem;
            }

            if (_owner.SelectedIndex >= 0
                && _owner.ItemContainerGenerator.ContainerFromIndex(_owner.SelectedIndex) is FrameworkElement fromIndex)
            {
                return fromIndex;
            }

            return selectedItem as FrameworkElement;
        }

        private bool HasValidContainerSize(FrameworkElement container) =>
            IsVerticalPlacement
                ? container.ActualHeight > 0
                : container.ActualWidth > 0;

        private Thickness BuildTargetMargin(Point origin) =>
            IsVerticalPlacement
                ? new Thickness(0, origin.Y, 0, 0)
                : new Thickness(origin.X, 0, 0, 0);

        private void ApplyIndicatorBounds(Thickness margin, double width, double height)
        {
            if (_indicator is null)
            {
                return;
            }

            StopIndicatorAnimation(applyCurrent: false);
            _indicator.Margin = margin;

            if (IsVerticalPlacement)
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

            if (!IsVerticalPlacement)
            {
                var widthAnim = new DoubleAnimation
                {
                    To = itemWidth,
                    Duration = duration,
                    EasingFunction = easing,
                };
                Storyboard.SetTarget(widthAnim, _indicator);
                Storyboard.SetTargetProperty(widthAnim, new PropertyPath(FrameworkElement.WidthProperty));
                storyboard.Children.Add(widthAnim);
            }
            else
            {
                var heightAnim = new DoubleAnimation
                {
                    To = itemHeight,
                    Duration = duration,
                    EasingFunction = easing,
                };
                Storyboard.SetTarget(heightAnim, _indicator);
                Storyboard.SetTargetProperty(heightAnim, new PropertyPath(FrameworkElement.HeightProperty));
                storyboard.Children.Add(heightAnim);
            }

            var marginAnim = new ThicknessAnimation
            {
                To = targetMargin,
                Duration = duration,
                EasingFunction = easing,
            };
            Storyboard.SetTarget(marginAnim, _indicator);
            Storyboard.SetTargetProperty(marginAnim, new PropertyPath(FrameworkElement.MarginProperty));
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

            if (IsVerticalPlacement)
            {
                return Math.Abs(targetMargin.Top - _indicator.Margin.Top)
                    + Math.Abs(itemHeight - _indicator.Height);
            }

            return Math.Abs(targetMargin.Left - _indicator.Margin.Left)
                + Math.Abs(itemWidth - _indicator.Width);
        }

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
                new PropertyPath(IsVerticalPlacement ? "RenderTransform.ScaleY" : "RenderTransform.ScaleX"));
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
                var currentMargin = (Thickness)_indicator.GetValue(FrameworkElement.MarginProperty);
                var currentWidth = (double)_indicator.GetValue(FrameworkElement.WidthProperty);
                var currentHeight = (double)_indicator.GetValue(FrameworkElement.HeightProperty);
                var currentScaleX = 1.0;
                var currentScaleY = 1.0;
                if (_indicator.RenderTransform is ScaleTransform scale)
                {
                    currentScaleX = scale.ScaleX;
                    currentScaleY = scale.ScaleY;
                }

                _indicatorStoryboard?.Stop();
                _indicatorStoryboard = null;
                _indicator.BeginAnimation(FrameworkElement.MarginProperty, null);
                _indicator.BeginAnimation(FrameworkElement.WidthProperty, null);
                _indicator.BeginAnimation(FrameworkElement.HeightProperty, null);
                _indicator.Margin = currentMargin;

                if (IsVerticalPlacement && !double.IsNaN(currentHeight))
                {
                    _indicator.Height = currentHeight;
                }
                else if (!IsVerticalPlacement && !double.IsNaN(currentWidth))
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
            _indicator.BeginAnimation(FrameworkElement.MarginProperty, null);
            _indicator.BeginAnimation(FrameworkElement.WidthProperty, null);
            _indicator.BeginAnimation(FrameworkElement.HeightProperty, null);
            if (_indicator.RenderTransform is ScaleTransform resetScale)
            {
                resetScale.BeginAnimation(ScaleTransform.ScaleXProperty, null);
                resetScale.BeginAnimation(ScaleTransform.ScaleYProperty, null);
                resetScale.ScaleX = 1;
                resetScale.ScaleY = 1;
            }
        }
    }
}
