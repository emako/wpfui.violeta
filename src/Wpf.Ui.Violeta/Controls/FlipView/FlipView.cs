using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// A WinUI3-like FlipView control that displays one item at a time and supports
/// animated left/right navigation with previous/next buttons.
/// </summary>
[TemplatePart(Name = PART_PreviousButton, Type = typeof(Button))]
[TemplatePart(Name = PART_NextButton, Type = typeof(Button))]
[TemplatePart(Name = PART_ScrollViewer, Type = typeof(AnimatableScrollViewer))]
public class FlipView : Selector
{
    public const string PART_PreviousButton = "PART_PreviousButton";
    public const string PART_NextButton = "PART_NextButton";
    public const string PART_ScrollViewer = "PART_ScrollViewer";

    private Button? _previousButton;
    private Button? _nextButton;
    private AnimatableScrollViewer? _scrollViewer;
    private bool _isAnimating;

    // -------------------------------------------------------------------------

    static FlipView()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(FlipView),
            new FrameworkPropertyMetadata(typeof(FlipView)));
    }

    // --- Dependency Properties ------------------------------------------------

    public static readonly DependencyProperty AutoLoopProperty =
        DependencyProperty.Register(
            nameof(AutoLoop),
            typeof(bool),
            typeof(FlipView),
            new PropertyMetadata(false));

    public static readonly DependencyProperty NavigationButtonsVisibilityProperty =
        DependencyProperty.Register(
            nameof(NavigationButtonsVisibility),
            typeof(FlipViewNavigationButtonsVisibility),
            typeof(FlipView),
            new FrameworkPropertyMetadata(
                FlipViewNavigationButtonsVisibility.Auto,
                (d, _) => ((FlipView)d).UpdateButtonStates()));

    public static readonly DependencyProperty UseTouchAnimationsForAllNavigationProperty =
        DependencyProperty.Register(
            nameof(UseTouchAnimationsForAllNavigation),
            typeof(bool),
            typeof(FlipView),
            new PropertyMetadata(true));

    // --- Properties -----------------------------------------------------------

    /// <summary>Whether navigation wraps from last to first item and vice versa.</summary>
    public bool AutoLoop
    {
        get => (bool)GetValue(AutoLoopProperty);
        set => SetValue(AutoLoopProperty, value);
    }

    /// <summary>Controls when the previous/next navigation buttons are visible.</summary>
    public FlipViewNavigationButtonsVisibility NavigationButtonsVisibility
    {
        get => (FlipViewNavigationButtonsVisibility)GetValue(NavigationButtonsVisibilityProperty);
        set => SetValue(NavigationButtonsVisibilityProperty, value);
    }

    /// <summary>Whether slide animation is used for keyboard/button navigation.</summary>
    public bool UseTouchAnimationsForAllNavigation
    {
        get => (bool)GetValue(UseTouchAnimationsForAllNavigationProperty);
        set => SetValue(UseTouchAnimationsForAllNavigationProperty, value);
    }

    // --- Template ------------------------------------------------------------

    public override void OnApplyTemplate()
    {
        // Unwire old parts
        _previousButton?.Click -= OnPreviousClick;
        _nextButton?.Click -= OnNextClick;
        SizeChanged -= OnSizeChanged;

        base.OnApplyTemplate();

        _previousButton = GetTemplateChild(PART_PreviousButton) as Button;
        _nextButton = GetTemplateChild(PART_NextButton) as Button;
        _scrollViewer = GetTemplateChild(PART_ScrollViewer) as AnimatableScrollViewer;

        _previousButton?.Click += OnPreviousClick;
        _nextButton?.Click += OnNextClick;
        SizeChanged += OnSizeChanged;

        // Ensure first item is selected
        if (SelectedIndex < 0 && Items.Count > 0)
            SelectedIndex = 0;

        UpdateButtonStates();
        ScrollToSelectedIndex(animate: false);
    }

    // --- ItemsControl overrides -----------------------------------------------

    protected override bool IsItemItsOwnContainerOverride(object item) =>
        item is FlipViewItem;

    protected override DependencyObject GetContainerForItemOverride() =>
        new FlipViewItem();

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);
        if (element is FlipViewItem container)
        {
            container.Width = ActualWidth;
            container.Height = ActualHeight;
        }
    }

    protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
    {
        base.OnItemsChanged(e);
        if (SelectedIndex < 0 && Items.Count > 0)
            SelectedIndex = 0;
        UpdateItemSizes();
        UpdateButtonStates();
    }

    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        base.OnSelectionChanged(e);
        UpdateButtonStates();
        ScrollToSelectedIndex(animate: UseTouchAnimationsForAllNavigation);
    }

    // --- Keyboard navigation --------------------------------------------------

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key == Key.Left || e.Key == Key.Up)
        {
            MovePrevious();
            e.Handled = true;
        }
        else if (e.Key == Key.Right || e.Key == Key.Down)
        {
            MoveNext();
            e.Handled = true;
        }
    }

    // --- Mouse wheel navigation -----------------------------------------------

    /// <summary>
    /// Override PreviewMouseWheel (tunnel) so we intercept before the inner
    /// AnimatableScrollViewer gets a chance to handle it.
    /// </summary>
    protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
    {
        base.OnPreviewMouseWheel(e);
        if (e.Delta < 0)
            MoveNext();
        else
            MovePrevious();
        e.Handled = true;
    }

    // --- Mouse enter/leave (button fade) --------------------------------------

    protected override void OnMouseEnter(System.Windows.Input.MouseEventArgs e)
    {
        base.OnMouseEnter(e);
        AnimateButtonOpacity(1.0);
    }

    protected override void OnMouseLeave(System.Windows.Input.MouseEventArgs e)
    {
        base.OnMouseLeave(e);
        AnimateButtonOpacity(0.0);
    }

    private void AnimateButtonOpacity(double target)
    {
        if (NavigationButtonsVisibility == FlipViewNavigationButtonsVisibility.Hidden) return;
        // In Visible mode buttons are always at full opacity; skip fade-out
        if (NavigationButtonsVisibility == FlipViewNavigationButtonsVisibility.Visible && target < 1.0) return;

        var duration = new Duration(TimeSpan.FromMilliseconds(150));
        var anim = new DoubleAnimation(target, duration);
        _previousButton?.BeginAnimation(OpacityProperty, anim);
        _nextButton?.BeginAnimation(OpacityProperty, anim);
    }

    // --- Click handlers -------------------------------------------------------

    private void OnPreviousClick(object sender, RoutedEventArgs e) => MovePrevious();

    private void OnNextClick(object sender, RoutedEventArgs e) => MoveNext();

    // --- SizeChanged ----------------------------------------------------------

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateItemSizes();
        ScrollToSelectedIndex(animate: false);
    }

    private void UpdateItemSizes()
    {
        for (int i = 0; i < Items.Count; i++)
        {
            if (ItemContainerGenerator.ContainerFromIndex(i) is FlipViewItem container)
            {
                container.Width = ActualWidth;
                container.Height = ActualHeight;
            }
        }
    }

    // --- Public navigation ----------------------------------------------------

    public void MovePrevious()
    {
        int count = Items.Count;
        if (count == 0) return;
        int next = SelectedIndex - 1;
        if (next < 0)
        {
            if (AutoLoop) next = count - 1;
            else return;
        }
        SelectedIndex = next;
    }

    public void MoveNext()
    {
        int count = Items.Count;
        if (count == 0) return;
        int next = SelectedIndex + 1;
        if (next >= count)
        {
            if (AutoLoop) next = 0;
            else return;
        }
        SelectedIndex = next;
    }

    // --- Scroll / animation ---------------------------------------------------

    private void ScrollToSelectedIndex(bool animate)
    {
        if (_scrollViewer == null || _scrollViewer.ActualWidth <= 0 || SelectedIndex < 0) return;

        double targetOffset = SelectedIndex * _scrollViewer.ActualWidth;

        if (!animate)
        {
            // Cancel any running animation then snap immediately
            _isAnimating = false;
            _scrollViewer.BeginAnimation(AnimatableScrollViewer.HorizontalOffsetProperty, null);
            _scrollViewer.ScrollToHorizontalOffset(targetOffset);
            return;
        }

        if (_isAnimating)
        {
            // Interrupt current animation, jump to new target
            _isAnimating = false;
            _scrollViewer.BeginAnimation(AnimatableScrollViewer.HorizontalOffsetProperty, null);
            _scrollViewer.ScrollToHorizontalOffset(targetOffset);
            return;
        }

        _isAnimating = true;
        double fromOffset = _scrollViewer.ContentHorizontalOffset;

        var animation = new DoubleAnimation
        {
            From = fromOffset,
            To = targetOffset,
            Duration = TimeSpan.FromMilliseconds(400d),
            EasingFunction = new ExponentialEase
            {
                Exponent = 6d,
                EasingMode = EasingMode.EaseOut,
            },
            FillBehavior = FillBehavior.HoldEnd,
        };
        animation.Completed += (_, _) => _isAnimating = false;
        _scrollViewer.BeginAnimation(AnimatableScrollViewer.HorizontalOffsetProperty, animation);
    }

    // --- Button state update --------------------------------------------------

    private void UpdateButtonStates()
    {
        int index = SelectedIndex;
        int count = Items.Count;

        bool hasPrev = index >= 0 && (AutoLoop ? count > 1 : index > 0);
        bool hasNext = index >= 0 && (AutoLoop ? count > 1 : index < count - 1);

        if (_previousButton != null)
        {
            _previousButton.IsEnabled = hasPrev;
            _previousButton.Visibility = NavigationButtonsVisibility == FlipViewNavigationButtonsVisibility.Hidden
                ? Visibility.Collapsed : Visibility.Visible;
        }

        if (_nextButton != null)
        {
            _nextButton.IsEnabled = hasNext;
            _nextButton.Visibility = NavigationButtonsVisibility == FlipViewNavigationButtonsVisibility.Hidden
                ? Visibility.Collapsed : Visibility.Visible;
        }

        // For always-visible mode, snap buttons to full opacity immediately
        if (NavigationButtonsVisibility == FlipViewNavigationButtonsVisibility.Visible)
        {
            if (_previousButton != null)
            {
                _previousButton.BeginAnimation(OpacityProperty, null);
                _previousButton.Opacity = 1d;
            }
            if (_nextButton != null)
            {
                _nextButton.BeginAnimation(OpacityProperty, null);
                _nextButton.Opacity = 1d;
            }
        }
    }
}

/// <summary>
/// Controls when FlipView navigation buttons are visible.
/// </summary>
public enum FlipViewNavigationButtonsVisibility
{
    /// <summary>Buttons fade in on mouse-over, hidden otherwise (WinUI3 default).</summary>
    Auto,

    /// <summary>Buttons are always visible.</summary>
    Visible,

    /// <summary>Buttons are never shown.</summary>
    Hidden,
}

/// <summary>
/// Internal ScrollViewer subclass that exposes <see cref="HorizontalOffset"/> as a
/// writable, animatable dependency property.
/// </summary>
public sealed class AnimatableScrollViewer : ScrollViewer
{
    public new static readonly DependencyProperty HorizontalOffsetProperty =
        DependencyProperty.Register(
            nameof(HorizontalOffset),
            typeof(double),
            typeof(AnimatableScrollViewer),
            new PropertyMetadata(0d, static (d, e) =>
                ((AnimatableScrollViewer)d).ScrollToHorizontalOffset((double)e.NewValue)));

    public new double HorizontalOffset
    {
        get => (double)GetValue(HorizontalOffsetProperty);
        set => SetValue(HorizontalOffsetProperty, value);
    }
}
