using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// A Fluent UI–style carousel that displays pages with slide/fade motion,
/// optional circular looping, drag, autoplay, and integrated <see cref="CarouselNav"/>.
/// API mirrors <c>@fluentui/react-carousel</c> <c>Carousel</c>.
/// </summary>
[TemplatePart(Name = PART_ScrollViewer, Type = typeof(AnimatableScrollViewer))]
[TemplatePart(Name = PART_NavHost, Type = typeof(ContentPresenter))]
[TemplatePart(Name = PART_CircularClone, Type = typeof(ContentPresenter))]
public class Carousel : Selector, ICarouselNavHost
{
    public const string PART_ScrollViewer = "PART_ScrollViewer";
    public const string PART_NavHost = "PART_NavHost";
    public const string PART_CircularClone = "PART_CircularClone";

    private AnimatableScrollViewer? _scrollViewer;
    private ContentPresenter? _circularClone;
    private bool _isAnimating;
    private bool _suppressSelectionSync;
    private bool _suppressActiveIndexCallback;
    private DispatcherTimer? _autoplayTimer;
    private Point _dragStart;
    private double _dragStartOffset;
    private bool _isDragging;
    private bool _autoplayPaused;
    private int _slideFromIndex = -1;

    static Carousel()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(Carousel),
            new FrameworkPropertyMetadata(typeof(Carousel)));
    }

    public Carousel()
    {
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    // --- Dependency Properties ------------------------------------------------

    public static readonly DependencyProperty ActiveIndexProperty =
        DependencyProperty.Register(
            nameof(ActiveIndex),
            typeof(int),
            typeof(Carousel),
            new FrameworkPropertyMetadata(
                -1,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnActiveIndexChanged,
                CoerceActiveIndex));

    public static readonly DependencyProperty CircularProperty =
        DependencyProperty.Register(
            nameof(Circular),
            typeof(bool),
            typeof(Carousel),
            new PropertyMetadata(false, OnCircularChanged));

    public static readonly DependencyProperty AppearanceProperty =
        DependencyProperty.Register(
            nameof(Appearance),
            typeof(CarouselAppearance),
            typeof(Carousel),
            new PropertyMetadata(CarouselAppearance.Flat));

    public static readonly DependencyProperty AlignProperty =
        DependencyProperty.Register(
            nameof(Align),
            typeof(CarouselAlign),
            typeof(Carousel),
            new PropertyMetadata(CarouselAlign.Center, OnLayoutAffectingPropertyChanged));

    public static readonly DependencyProperty MotionProperty =
        DependencyProperty.Register(
            nameof(Motion),
            typeof(CarouselMotion),
            typeof(Carousel),
            new PropertyMetadata(CarouselMotion.Slide));

    public static readonly DependencyProperty MotionDurationProperty =
        DependencyProperty.Register(
            nameof(MotionDuration),
            typeof(Duration),
            typeof(Carousel),
            new PropertyMetadata(new Duration(TimeSpan.FromMilliseconds(400))));

    public static readonly DependencyProperty DraggableProperty =
        DependencyProperty.Register(
            nameof(Draggable),
            typeof(bool),
            typeof(Carousel),
            new PropertyMetadata(false));

    public static readonly DependencyProperty AutoplayIntervalProperty =
        DependencyProperty.Register(
            nameof(AutoplayInterval),
            typeof(int),
            typeof(Carousel),
            new PropertyMetadata(4000, OnAutoplaySettingsChanged));

    public static readonly DependencyProperty IsAutoplayEnabledProperty =
        DependencyProperty.Register(
            nameof(IsAutoplayEnabled),
            typeof(bool),
            typeof(Carousel),
            new PropertyMetadata(false, OnAutoplaySettingsChanged));

    public static readonly DependencyProperty NavProperty =
        DependencyProperty.Register(
            nameof(Nav),
            typeof(object),
            typeof(Carousel),
            new PropertyMetadata(null));

    public static readonly DependencyProperty ShowNavProperty =
        DependencyProperty.Register(
            nameof(ShowNav),
            typeof(bool),
            typeof(Carousel),
            new PropertyMetadata(true));

    public static readonly RoutedEvent ActiveIndexChangedEvent =
        EventManager.RegisterRoutedEvent(
            nameof(ActiveIndexChanged),
            RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<int>),
            typeof(Carousel));

    // --- Properties -----------------------------------------------------------

    /// <summary>Zero-based index of the active page (Fluent <c>activeIndex</c>).</summary>
    public int ActiveIndex
    {
        get => (int)GetValue(ActiveIndexProperty);
        set => SetValue(ActiveIndexProperty, value);
    }

    /// <summary>When true, navigation wraps past the first/last page.</summary>
    public bool Circular
    {
        get => (bool)GetValue(CircularProperty);
        set => SetValue(CircularProperty, value);
    }

    /// <summary>Visual treatment for the carousel container.</summary>
    public CarouselAppearance Appearance
    {
        get => (CarouselAppearance)GetValue(AppearanceProperty);
        set => SetValue(AppearanceProperty, value);
    }

    /// <summary>Alignment of the active page within the viewport.</summary>
    public CarouselAlign Align
    {
        get => (CarouselAlign)GetValue(AlignProperty);
        set => SetValue(AlignProperty, value);
    }

    /// <summary>Transition motion between pages.</summary>
    public CarouselMotion Motion
    {
        get => (CarouselMotion)GetValue(MotionProperty);
        set => SetValue(MotionProperty, value);
    }

    /// <summary>Duration of slide/fade transitions.</summary>
    public Duration MotionDuration
    {
        get => (Duration)GetValue(MotionDurationProperty);
        set => SetValue(MotionDurationProperty, value);
    }

    /// <summary>Enables pointer drag to change pages.</summary>
    public bool Draggable
    {
        get => (bool)GetValue(DraggableProperty);
        set => SetValue(DraggableProperty, value);
    }

    /// <summary>Delay between autoplay transitions in milliseconds (default 4000).</summary>
    public int AutoplayInterval
    {
        get => (int)GetValue(AutoplayIntervalProperty);
        set => SetValue(AutoplayIntervalProperty, value);
    }

    /// <summary>
    /// Enables automatic page advancement. Fluent requires <c>CarouselAutoplayButton</c>;
    /// WPF exposes this flag directly.
    /// </summary>
    public bool IsAutoplayEnabled
    {
        get => (bool)GetValue(IsAutoplayEnabledProperty);
        set => SetValue(IsAutoplayEnabledProperty, value);
    }

    /// <summary>
    /// Optional nav content (typically a <see cref="CarouselNav"/>) rendered below the viewport.
    /// </summary>
    public object? Nav
    {
        get => GetValue(NavProperty);
        set => SetValue(NavProperty, value);
    }

    /// <summary>Whether the <see cref="Nav"/> host is visible.</summary>
    public bool ShowNav
    {
        get => (bool)GetValue(ShowNavProperty);
        set => SetValue(ShowNavProperty, value);
    }

    /// <summary>Raised when <see cref="ActiveIndex"/> changes.</summary>
    public event RoutedPropertyChangedEventHandler<int> ActiveIndexChanged
    {
        add => AddHandler(ActiveIndexChangedEvent, value);
        remove => RemoveHandler(ActiveIndexChangedEvent, value);
    }

    /// <summary>Total number of slides (equals <see cref="ItemsControl.Items"/>.Count).</summary>
    public int TotalSlides => Items.Count;

    // --- Template ------------------------------------------------------------

    public override void OnApplyTemplate()
    {
        DetachDragHandlers();
        SizeChanged -= OnSizeChanged;

        base.OnApplyTemplate();

        _scrollViewer = GetTemplateChild(PART_ScrollViewer) as AnimatableScrollViewer;
        _circularClone = GetTemplateChild(PART_CircularClone) as ContentPresenter;

        SizeChanged += OnSizeChanged;
        AttachDragHandlers();

        if (SelectedIndex < 0 && Items.Count > 0)
            SelectedIndex = 0;

        EnsureDefaultNav();
        SyncActiveIndexFromSelection(raiseEvent: false);
        UpdateItemStates();
        UpdateCircularClone();
        GoToActiveIndex(animate: false);
        UpdateAutoplayTimer();
    }

    private void EnsureDefaultNav()
    {
        if (Nav == null && ShowNav)
            Nav = new CarouselNav();

        if (Nav is CarouselNav nav)
        {
            nav.SetCurrentValue(CarouselNav.CarouselProperty, this);
            nav.SetCurrentValue(CarouselNav.TotalSlidesProperty, Items.Count);
            nav.SetCurrentValue(CarouselNav.SelectedIndexProperty, ActiveIndex >= 0 ? ActiveIndex : SelectedIndex);
        }
    }

    // --- ItemsControl overrides -----------------------------------------------

    protected override bool IsItemItsOwnContainerOverride(object item) =>
        item is CarouselItem;

    protected override DependencyObject GetContainerForItemOverride() =>
        new CarouselItem();

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);
        if (element is CarouselItem container)
            ApplyItemSize(container);

        // First container may appear after ApplyTemplate; refresh the circular clone.
        if (Circular && Motion == CarouselMotion.Slide)
            Dispatcher.BeginInvoke(UpdateCircularClone, DispatcherPriority.Loaded);
    }

    protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
    {
        base.OnItemsChanged(e);

        if (SelectedIndex < 0 && Items.Count > 0)
            SelectedIndex = 0;
        else if (Items.Count == 0)
            SelectedIndex = -1;
        else if (SelectedIndex >= Items.Count)
            SelectedIndex = Items.Count - 1;

        SyncActiveIndexFromSelection(raiseEvent: true);
        UpdateItemSizes();
        UpdateItemStates();
        UpdateCircularClone();
        GoToActiveIndex(animate: false);
        UpdateAutoplayTimer();
        NotifyNavSlidesChanged();
    }

    private void NotifyNavSlidesChanged()
    {
        if (Nav is CarouselNav nav)
        {
            nav.SetCurrentValue(CarouselNav.TotalSlidesProperty, Items.Count);
            nav.SetCurrentValue(CarouselNav.SelectedIndexProperty, ActiveIndex);
        }
    }

    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        base.OnSelectionChanged(e);

        if (_suppressSelectionSync)
            return;

        // Prefer ActiveIndex → SelectedIndex path; avoid a second GoToActiveIndex that
        // would interrupt circular wrap (last→first) mid-animation.
        if (ActiveIndex != SelectedIndex)
        {
            SyncActiveIndexFromSelection(raiseEvent: true);
            return;
        }

        UpdateItemStates();
        GoToActiveIndex(animate: true);
        ResetAutoplay();
    }

    // --- Keyboard / wheel -----------------------------------------------------

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key is Key.Left or Key.Up)
        {
            MovePrevious();
            e.Handled = true;
        }
        else if (e.Key is Key.Right or Key.Down)
        {
            MoveNext();
            e.Handled = true;
        }
    }

    protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
    {
        base.OnPreviewMouseWheel(e);
        if (e.Delta < 0)
            MoveNext();
        else
            MovePrevious();
        e.Handled = true;
        ResetAutoplay();
    }

    protected override void OnMouseEnter(MouseEventArgs e)
    {
        base.OnMouseEnter(e);
        _autoplayPaused = true;
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);
        _autoplayPaused = false;
    }

    // --- Public navigation ----------------------------------------------------

    public void MovePrevious()
    {
        int count = Items.Count;
        if (count == 0) return;
        int next = ActiveIndex - 1;
        if (next < 0)
        {
            if (Circular) next = count - 1;
            else return;
        }
        ActiveIndex = next;
    }

    public void MoveNext()
    {
        int count = Items.Count;
        if (count == 0) return;
        int next = ActiveIndex + 1;
        if (next >= count)
        {
            if (Circular) next = 0;
            else return;
        }
        ActiveIndex = next;
    }

    public void SelectPageByIndex(int index)
    {
        if (index < 0 || index >= Items.Count) return;
        ActiveIndex = index;
    }

    /// <summary>Resets the autoplay countdown (Fluent <c>resetAutoplay</c>).</summary>
    public void ResetAutoplay()
    {
        if (_autoplayTimer == null) return;
        _autoplayTimer.Stop();
        if (IsAutoplayEnabled && Items.Count > 1)
            _autoplayTimer.Start();
    }

    // --- ActiveIndex sync -----------------------------------------------------

    private static void OnActiveIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var carousel = (Carousel)d;
        if (carousel._suppressActiveIndexCallback) return;

        int oldIndex = (int)e.OldValue;
        int newIndex = (int)e.NewValue;

        carousel._suppressSelectionSync = true;
        try
        {
            if (newIndex != carousel.SelectedIndex)
                carousel.SelectedIndex = newIndex;
        }
        finally
        {
            carousel._suppressSelectionSync = false;
        }

        carousel._slideFromIndex = oldIndex;
        carousel.UpdateItemStates();
        carousel.GoToActiveIndex(animate: true);
        carousel.RaiseEvent(new RoutedPropertyChangedEventArgs<int>(oldIndex, newIndex, ActiveIndexChangedEvent));
        carousel.ResetAutoplay();
    }

    private static object CoerceActiveIndex(DependencyObject d, object baseValue)
    {
        var carousel = (Carousel)d;
        int value = (int)baseValue;
        int count = carousel.Items.Count;
        if (count == 0) return -1;
        if (value < 0) return 0;
        if (value >= count) return count - 1;
        return value;
    }

    private void SyncActiveIndexFromSelection(bool raiseEvent)
    {
        int index = SelectedIndex;
        if (ActiveIndex == index) return;

        if (raiseEvent)
        {
            SetCurrentValue(ActiveIndexProperty, index);
            return;
        }

        _suppressActiveIndexCallback = true;
        try
        {
            SetCurrentValue(ActiveIndexProperty, index);
        }
        finally
        {
            _suppressActiveIndexCallback = false;
        }
    }

    // --- Layout / animation ---------------------------------------------------

    private void OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        UpdateItemSizes();
        UpdateCircularClone();
        GoToActiveIndex(animate: false);
    }

    private static void OnCircularChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var carousel = (Carousel)d;
        carousel.UpdateCircularClone();
        carousel.GoToActiveIndex(animate: false);
    }

    private static void OnLayoutAffectingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var carousel = (Carousel)d;
        carousel.UpdateItemSizes();
        carousel.UpdateCircularClone();
        carousel.GoToActiveIndex(animate: false);
    }

    private void ApplyItemSize(CarouselItem container)
    {
        double width = _scrollViewer?.ActualWidth > 0 ? _scrollViewer.ActualWidth : ActualWidth;
        double height = _scrollViewer?.ActualHeight > 0 ? _scrollViewer.ActualHeight : ActualHeight;
        if (width <= 0 || height <= 0) return;

        container.Width = width;
        container.Height = height;
    }

    private void UpdateItemSizes()
    {
        for (int i = 0; i < Items.Count; i++)
        {
            if (ItemContainerGenerator.ContainerFromIndex(i) is CarouselItem container)
                ApplyItemSize(container);
        }
    }

    private void UpdateItemStates()
    {
        for (int i = 0; i < Items.Count; i++)
        {
            if (ItemContainerGenerator.ContainerFromIndex(i) is CarouselItem container)
            {
                bool active = i == ActiveIndex;
                container.IsActive = active;
                container.IsHitTestVisible = active || Draggable;
                if (Motion == CarouselMotion.Fade)
                {
                    // Non-active cards stay in place for fade; hide interaction.
                    Panel.SetZIndex(container, active ? 1 : 0);
                }
            }
        }
    }

    private void GoToActiveIndex(bool animate)
    {
        if (ActiveIndex < 0 || Items.Count == 0) return;

        if (Motion == CarouselMotion.Fade)
        {
            AnimateFade(animate);
            _slideFromIndex = ActiveIndex;
            return;
        }

        AnimateSlide(animate);
        _slideFromIndex = ActiveIndex;
    }

    private void UpdateCircularClone()
    {
        if (_circularClone == null) return;

        int count = Items.Count;
        bool showClone = Circular
            && Motion == CarouselMotion.Slide
            && count > 1
            && _scrollViewer != null
            && _scrollViewer.ActualWidth > 0;

        if (!showClone)
        {
            _circularClone.Visibility = Visibility.Collapsed;
            _circularClone.Width = 0;
            _circularClone.Height = 0;
            _circularClone.Content = null;
            return;
        }

        double width = _scrollViewer!.ActualWidth;
        double height = _scrollViewer.ActualHeight > 0 ? _scrollViewer.ActualHeight : ActualHeight;
        if (height <= 0) height = ActualHeight;

        FrameworkElement? visual = ItemContainerGenerator.ContainerFromIndex(0) as FrameworkElement;
        if (visual == null)
        {
            // Containers may not be ready yet; retry after layout.
            Dispatcher.BeginInvoke(UpdateCircularClone, DispatcherPriority.Loaded);
            return;
        }

        _circularClone.Width = width;
        _circularClone.Height = height;
        _circularClone.Content = new Border
        {
            Width = width,
            Height = height,
            Background = new VisualBrush(visual)
            {
                Stretch = Stretch.None,
                AlignmentX = AlignmentX.Left,
                AlignmentY = AlignmentY.Top,
            },
        };
        _circularClone.Visibility = Visibility.Visible;
    }

    private void AnimateSlide(bool animate)
    {
        if (_scrollViewer == null || _scrollViewer.ActualWidth <= 0 || ActiveIndex < 0) return;

        // Ensure scroll viewer is used (fade may have collapsed offsets)
        for (int i = 0; i < Items.Count; i++)
        {
            if (ItemContainerGenerator.ContainerFromIndex(i) is CarouselItem container)
            {
                container.BeginAnimation(OpacityProperty, null);
                container.Opacity = 1;
                container.Visibility = Visibility.Visible;
                container.Margin = new Thickness(0);
            }
        }

        UpdateCircularClone();

        int count = Items.Count;
        double pageWidth = _scrollViewer.ActualWidth;
        int fromIndex = _slideFromIndex;
        int toIndex = ActiveIndex;
        double targetOffset = toIndex * pageWidth;

        // Align adjustment for partial last pages is a no-op when page == viewport width.
        targetOffset = Align switch
        {
            CarouselAlign.End => Math.Max(0, targetOffset),
            CarouselAlign.Start => targetOffset,
            _ => targetOffset,
        };

        bool wrapForward = Circular
            && count > 1
            && fromIndex == count - 1
            && toIndex == 0;
        bool wrapBackward = Circular
            && count > 1
            && fromIndex == 0
            && toIndex == count - 1;

        if (!animate || MotionDuration.TimeSpan.TotalMilliseconds <= 0)
        {
            _isAnimating = false;
            _scrollViewer.BeginAnimation(AnimatableScrollViewer.HorizontalOffsetProperty, null);
            _scrollViewer.ScrollToHorizontalOffset(targetOffset);
            return;
        }

        if (_isAnimating)
        {
            _isAnimating = false;
            _scrollViewer.BeginAnimation(AnimatableScrollViewer.HorizontalOffsetProperty, null);
            _scrollViewer.ScrollToHorizontalOffset(targetOffset);
            return;
        }

        if (wrapForward)
        {
            // Animate onto the trailing first-slide clone, then snap to real index 0.
            double cloneOffset = count * pageWidth;
            AnimateHorizontalOffset(_scrollViewer.ContentHorizontalOffset, cloneOffset, () =>
            {
                _scrollViewer.BeginAnimation(AnimatableScrollViewer.HorizontalOffsetProperty, null);
                _scrollViewer.ScrollToHorizontalOffset(0);
            });
            return;
        }

        if (wrapBackward)
        {
            // Jump to the clone (visually identical to index 0), then slide left to last.
            double cloneOffset = count * pageWidth;
            _scrollViewer.BeginAnimation(AnimatableScrollViewer.HorizontalOffsetProperty, null);
            _scrollViewer.ScrollToHorizontalOffset(cloneOffset);
            AnimateHorizontalOffset(cloneOffset, (count - 1) * pageWidth, null);
            return;
        }

        AnimateHorizontalOffset(_scrollViewer.ContentHorizontalOffset, targetOffset, null);
    }

    private void AnimateHorizontalOffset(double fromOffset, double toOffset, Action? onCompleted)
    {
        if (_scrollViewer == null) return;

        _isAnimating = true;
        var animation = new DoubleAnimation
        {
            From = fromOffset,
            To = toOffset,
            Duration = MotionDuration,
            EasingFunction = new ExponentialEase { Exponent = 6d, EasingMode = EasingMode.EaseOut },
            FillBehavior = FillBehavior.HoldEnd,
        };
        animation.Completed += (_, _) =>
        {
            _isAnimating = false;
            onCompleted?.Invoke();
        };
        _scrollViewer.BeginAnimation(AnimatableScrollViewer.HorizontalOffsetProperty, animation);
    }

    private void AnimateFade(bool animate)
    {
        // Snap scroll to 0 — cards stack visually via opacity.
        if (_scrollViewer != null)
        {
            _scrollViewer.BeginAnimation(AnimatableScrollViewer.HorizontalOffsetProperty, null);
            _scrollViewer.ScrollToHorizontalOffset(0);
        }

        for (int i = 0; i < Items.Count; i++)
        {
            if (ItemContainerGenerator.ContainerFromIndex(i) is not CarouselItem container)
                continue;

            bool active = i == ActiveIndex;
            container.Width = _scrollViewer?.ActualWidth > 0 ? _scrollViewer.ActualWidth : ActualWidth;

            // Stack all cards at the start for fade mode by overlapping via negative margin after first.
            if (i > 0)
                container.Margin = new Thickness(-container.Width, 0, 0, 0);
            else
                container.Margin = new Thickness(0);

            double targetOpacity = active ? 1 : 0;
            if (!animate || MotionDuration.TimeSpan.TotalMilliseconds <= 0)
            {
                container.BeginAnimation(OpacityProperty, null);
                container.Opacity = targetOpacity;
            }
            else
            {
                var anim = new DoubleAnimation(targetOpacity, MotionDuration)
                {
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut },
                };
                container.BeginAnimation(OpacityProperty, anim);
            }
        }
    }

    // --- Drag -----------------------------------------------------------------

    private void AttachDragHandlers()
    {
        if (_scrollViewer == null) return;
        _scrollViewer.PreviewMouseLeftButtonDown += OnDragMouseDown;
        _scrollViewer.PreviewMouseMove += OnDragMouseMove;
        _scrollViewer.PreviewMouseLeftButtonUp += OnDragMouseUp;
        _scrollViewer.LostMouseCapture += OnDragLostCapture;
    }

    private void DetachDragHandlers()
    {
        if (_scrollViewer == null) return;
        _scrollViewer.PreviewMouseLeftButtonDown -= OnDragMouseDown;
        _scrollViewer.PreviewMouseMove -= OnDragMouseMove;
        _scrollViewer.PreviewMouseLeftButtonUp -= OnDragMouseUp;
        _scrollViewer.LostMouseCapture -= OnDragLostCapture;
    }

    private void OnDragMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (!Draggable || Motion != CarouselMotion.Slide || _scrollViewer == null) return;
        _isDragging = true;
        _dragStart = e.GetPosition(_scrollViewer);
        _dragStartOffset = _scrollViewer.ContentHorizontalOffset;
        _scrollViewer.CaptureMouse();
        _scrollViewer.BeginAnimation(AnimatableScrollViewer.HorizontalOffsetProperty, null);
        e.Handled = true;
    }

    private void OnDragMouseMove(object sender, MouseEventArgs e)
    {
        if (!_isDragging || _scrollViewer == null) return;
        Point pos = e.GetPosition(_scrollViewer);
        double delta = _dragStart.X - pos.X;
        _scrollViewer.ScrollToHorizontalOffset(_dragStartOffset + delta);
    }

    private void OnDragMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (!_isDragging || _scrollViewer == null) return;
        FinishDrag();
        e.Handled = true;
    }

    private void OnDragLostCapture(object sender, MouseEventArgs e)
    {
        if (_isDragging)
            FinishDrag();
    }

    private void FinishDrag()
    {
        _isDragging = false;
        _scrollViewer?.ReleaseMouseCapture();
        if (_scrollViewer == null || _scrollViewer.ActualWidth <= 0) return;

        double pageWidth = _scrollViewer.ActualWidth;
        int count = Items.Count;
        double offset = _scrollViewer.ContentHorizontalOffset;
        int target = (int)Math.Round(offset / pageWidth);

        if (Circular && count > 1)
        {
            // Dragged onto the trailing first-slide clone → wrap to index 0 without reverse slide.
            if (target >= count)
            {
                _isAnimating = false;
                _scrollViewer.BeginAnimation(AnimatableScrollViewer.HorizontalOffsetProperty, null);
                _scrollViewer.ScrollToHorizontalOffset(0);
                _slideFromIndex = 0;
                if (ActiveIndex != 0)
                    ActiveIndex = 0;
                else
                    GoToActiveIndex(animate: false);
                ResetAutoplay();
                return;
            }

            if (target < 0)
                target = count - 1;
        }

        target = Math.Max(0, Math.Min(count - 1, target));
        if (target == ActiveIndex)
            GoToActiveIndex(animate: true);
        else
            ActiveIndex = target;
        ResetAutoplay();
    }

    // --- Autoplay -------------------------------------------------------------

    private static void OnAutoplaySettingsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((Carousel)d).UpdateAutoplayTimer();
    }

    private void OnLoaded(object sender, RoutedEventArgs e) => UpdateAutoplayTimer();

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _autoplayTimer?.Stop();
        _autoplayTimer = null;
    }

    private void UpdateAutoplayTimer()
    {
        if (!IsAutoplayEnabled || Items.Count <= 1 || AutoplayInterval <= 0)
        {
            _autoplayTimer?.Stop();
            return;
        }

        _autoplayTimer ??= new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(AutoplayInterval) };
        _autoplayTimer.Tick -= OnAutoplayTick;
        _autoplayTimer.Tick += OnAutoplayTick;
        _autoplayTimer.Interval = TimeSpan.FromMilliseconds(AutoplayInterval);
        _autoplayTimer.Stop();
        _autoplayTimer.Start();
    }

    private void OnAutoplayTick(object? sender, EventArgs e)
    {
        if (_autoplayPaused || _isDragging || IsMouseOver) return;
        if (!Circular && ActiveIndex >= Items.Count - 1)
        {
            _autoplayTimer?.Stop();
            return;
        }
        MoveNext();
    }
}
