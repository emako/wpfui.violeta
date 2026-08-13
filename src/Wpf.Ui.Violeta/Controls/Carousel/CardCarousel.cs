using System;
using System.Collections.Generic;
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
/// Cover-flow style carousel that shows three overlapping cards (left / center / right).
/// Ported from WPFDevelopers <c>CardCarousel</c>; chrome uses WPF UI Card brushes.
/// Pagination uses the same <see cref="Nav"/> / <see cref="CarouselNav"/> pattern as <see cref="Carousel"/>.
/// </summary>
[TemplatePart(Name = PART_ItemsPresenter, Type = typeof(ItemsPresenter))]
[TemplatePart(Name = PART_PrevButton, Type = typeof(ButtonBase))]
[TemplatePart(Name = PART_NextButton, Type = typeof(ButtonBase))]
[TemplatePart(Name = PART_NavHost, Type = typeof(ContentPresenter))]
public class CardCarousel : Selector, ICarouselNavHost
{
    public const string PART_ItemsPresenter = "PART_ItemsPresenter";
    public const string PART_PrevButton = "PART_PrevButton";
    public const string PART_NextButton = "PART_NextButton";
    public const string PART_NavHost = "PART_NavHost";

    private enum Slot
    {
        Left,
        Center,
        Right,
    }

    private enum SlotZ
    {
        Left = 20,
        Center = 30,
        Right = 20,
    }

    private const double ScaleSide = 0.95;
    private const double ScaleCenter = 1.0;
    private const double ElementScale = 0.6;
    private const double DockOffset = 0.2;
    private const double AnimationSeconds = 0.5;
    private const double ZIndexSeconds = 0.7;

    private ItemsPresenter? _itemsPresenter;
    private Canvas? _contentDock;
    private ButtonBase? _prevButton;
    private ButtonBase? _nextButton;
    private bool _listeningGenerator;
    private int _reportedActiveIndex = -1;

    private double _shellWidth;
    private double _elementWidth;
    private double _leftDock;
    private double _centerDock;
    private double _rightDock;

    private int _count;
    private bool _isAnimating;
    private bool _suppressSelection;
    private bool _autoplayPaused;

    private readonly Dictionary<int, FrameworkElement> _elements = new();
    private readonly Dictionary<Slot, int> _slots = new();
    private readonly LinkedList<int> _buffer = new();

    private Storyboard? _storyboard;
    private DispatcherTimer? _autoplayTimer;

    static CardCarousel()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(CardCarousel),
            new FrameworkPropertyMetadata(typeof(CardCarousel)));
    }

    public CardCarousel()
    {
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        SizeChanged += OnSizeChanged;
    }

    // --- Dependency properties ------------------------------------------------

    public static readonly DependencyProperty ShowNavProperty =
        DependencyProperty.Register(
            nameof(ShowNav),
            typeof(bool),
            typeof(CardCarousel),
            new PropertyMetadata(true));

    public static readonly DependencyProperty ShowArrowsProperty =
        DependencyProperty.Register(
            nameof(ShowArrows),
            typeof(bool),
            typeof(CardCarousel),
            new PropertyMetadata(true, OnShowChromeChanged));

    public static readonly DependencyProperty IsAutoplayEnabledProperty =
        DependencyProperty.Register(
            nameof(IsAutoplayEnabled),
            typeof(bool),
            typeof(CardCarousel),
            new PropertyMetadata(false, OnAutoplaySettingsChanged));

    public static readonly DependencyProperty AutoplayIntervalProperty =
        DependencyProperty.Register(
            nameof(AutoplayInterval),
            typeof(int),
            typeof(CardCarousel),
            new PropertyMetadata(3000, OnAutoplaySettingsChanged));

    public static readonly DependencyProperty MotionDurationProperty =
        DependencyProperty.Register(
            nameof(MotionDuration),
            typeof(Duration),
            typeof(CardCarousel),
            new PropertyMetadata(new Duration(TimeSpan.FromSeconds(AnimationSeconds))));

    public static readonly DependencyProperty NavProperty =
        DependencyProperty.Register(
            nameof(Nav),
            typeof(object),
            typeof(CardCarousel),
            new PropertyMetadata(null));

    public static readonly RoutedEvent ActiveIndexChangedEvent =
        EventManager.RegisterRoutedEvent(
            nameof(ActiveIndexChanged),
            RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<int>),
            typeof(CardCarousel));

    /// <summary>Whether the <see cref="Nav"/> host is visible.</summary>
    public bool ShowNav
    {
        get => (bool)GetValue(ShowNavProperty);
        set => SetValue(ShowNavProperty, value);
    }

    /// <summary>Whether previous/next arrow buttons are visible.</summary>
    public bool ShowArrows
    {
        get => (bool)GetValue(ShowArrowsProperty);
        set => SetValue(ShowArrowsProperty, value);
    }

    /// <summary>Enables automatic advancement to the next card.</summary>
    public bool IsAutoplayEnabled
    {
        get => (bool)GetValue(IsAutoplayEnabledProperty);
        set => SetValue(IsAutoplayEnabledProperty, value);
    }

    /// <summary>Delay between autoplay transitions in milliseconds.</summary>
    public int AutoplayInterval
    {
        get => (int)GetValue(AutoplayIntervalProperty);
        set => SetValue(AutoplayIntervalProperty, value);
    }

    /// <summary>Duration of the card slide / scale animation.</summary>
    public Duration MotionDuration
    {
        get => (Duration)GetValue(MotionDurationProperty);
        set => SetValue(MotionDurationProperty, value);
    }

    /// <summary>
    /// Optional nav content (typically a <see cref="CarouselNav"/>) rendered below the viewport.
    /// </summary>
    public object? Nav
    {
        get => GetValue(NavProperty);
        set => SetValue(NavProperty, value);
    }

    /// <summary>Total number of cards (equals <see cref="ItemsControl.Items"/>.Count).</summary>
    public int TotalSlides => Items.Count;

    /// <summary>Zero-based index of the active card.</summary>
    public int ActiveIndex => SelectedIndex;

    /// <summary>Raised when the active card index changes.</summary>
    public event RoutedPropertyChangedEventHandler<int> ActiveIndexChanged
    {
        add => AddHandler(ActiveIndexChangedEvent, value);
        remove => RemoveHandler(ActiveIndexChangedEvent, value);
    }

    // --- Template / ItemsControl ---------------------------------------------

    public override void OnApplyTemplate()
    {
        DetachChrome();
        DetachGenerator();
        base.OnApplyTemplate();

        _prevButton = GetTemplateChild(PART_PrevButton) as ButtonBase;
        _nextButton = GetTemplateChild(PART_NextButton) as ButtonBase;
        _itemsPresenter = GetTemplateChild(PART_ItemsPresenter) as ItemsPresenter;
        _contentDock = null;

        if (_prevButton != null)
            _prevButton.Click += OnPrevClick;
        if (_nextButton != null)
            _nextButton.Click += OnNextClick;

        if (SelectedIndex < 0 && Items.Count > 0)
            SelectedIndex = 0;

        EnsureDefaultNav();
        AttachGenerator();
        Rebuild();
        UpdateChromeVisibility();
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
            nav.SetCurrentValue(CarouselNav.SelectedIndexProperty, SelectedIndex);
        }
    }

    private void NotifyNavSlidesChanged()
    {
        if (Nav is CarouselNav nav)
        {
            nav.SetCurrentValue(CarouselNav.TotalSlidesProperty, Items.Count);
            nav.SetCurrentValue(CarouselNav.SelectedIndexProperty, SelectedIndex);
        }
    }

    protected override bool IsItemItsOwnContainerOverride(object item) =>
        item is CardCarouselItem;

    protected override DependencyObject GetContainerForItemOverride() =>
        new CardCarouselItem();

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);
        if (element is not FrameworkElement fe)
            return;

        fe.RenderTransformOrigin = new Point(0.5, 1);
        if (fe.RenderTransform is not TransformGroup)
            fe.RenderTransform = CreateTransform(ScaleSide);
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

        Rebuild();
        NotifyNavSlidesChanged();
        UpdateAutoplayTimer();
    }

    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        base.OnSelectionChanged(e);
        if (_suppressSelection)
            return;

        int oldIndex = _reportedActiveIndex;
        int newIndex = SelectedIndex;
        if (oldIndex != newIndex)
        {
            _reportedActiveIndex = newIndex;
            RaiseEvent(new RoutedPropertyChangedEventArgs<int>(oldIndex, newIndex, ActiveIndexChangedEvent));
        }

        NotifyNavSlidesChanged();
        PlayToIndex(SelectedIndex);
        ResetAutoplay();
    }

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

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        base.OnMouseUp(e);
        if (_contentDock == null) return;
        HandleClick(e.GetPosition(_contentDock));
    }

    // --- Public API ----------------------------------------------------------

    public void MoveNext()
    {
        if (_count == 0) return;
        int next = SelectedIndex + 1;
        if (next >= _count) next = 0;
        SelectedIndex = next;
    }

    public void MovePrevious()
    {
        if (_count == 0) return;
        int prev = SelectedIndex - 1;
        if (prev < 0) prev = _count - 1;
        SelectedIndex = prev;
    }

    public void SelectPageByIndex(int index)
    {
        if (index < 0 || index >= Items.Count) return;
        SelectedIndex = index;
    }

    public void ResetAutoplay()
    {
        if (_autoplayTimer == null) return;
        _autoplayTimer.Stop();
        if (IsAutoplayEnabled && Items.Count > 1)
            _autoplayTimer.Start();
    }

    // --- Lifecycle -----------------------------------------------------------

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Rebuild();
        UpdateAutoplayTimer();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        DetachGenerator();
        _autoplayTimer?.Stop();
        _autoplayTimer = null;
        _storyboard?.Stop();
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (Items.Count == 0) return;
        bool playing = IsAutoplayEnabled;
        if (playing)
            _autoplayTimer?.Stop();
        Rebuild();
        if (playing)
            UpdateAutoplayTimer();
    }

    private void DetachChrome()
    {
        if (_prevButton != null)
            _prevButton.Click -= OnPrevClick;
        if (_nextButton != null)
            _nextButton.Click -= OnNextClick;
    }

    private void AttachGenerator()
    {
        if (_listeningGenerator) return;
        ItemContainerGenerator.StatusChanged += OnGeneratorStatusChanged;
        _listeningGenerator = true;
    }

    private void DetachGenerator()
    {
        if (!_listeningGenerator) return;
        ItemContainerGenerator.StatusChanged -= OnGeneratorStatusChanged;
        _listeningGenerator = false;
    }

    private void OnGeneratorStatusChanged(object? sender, EventArgs e)
    {
        if (ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
            Rebuild();
    }

    private void OnPrevClick(object sender, RoutedEventArgs e) => MovePrevious();

    private void OnNextClick(object sender, RoutedEventArgs e) => MoveNext();

    private static void OnShowChromeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        ((CardCarousel)d).UpdateChromeVisibility();

    private void UpdateChromeVisibility()
    {
        _prevButton?.Visibility = ShowArrows ? Visibility.Visible : Visibility.Collapsed;
        _nextButton?.Visibility = ShowArrows ? Visibility.Visible : Visibility.Collapsed;
    }

    // --- Build ---------------------------------------------------------------

    private void Rebuild()
    {
        _contentDock = ResolveItemsCanvas();
        if (_contentDock == null)
        {
            Dispatcher.BeginInvoke(Rebuild, DispatcherPriority.Loaded);
            return;
        }

        _storyboard?.Stop();
        _isAnimating = false;
        _slots.Clear();
        _buffer.Clear();
        _elements.Clear();

        _count = Items.Count;
        if (_count == 0)
        {
            NotifyNavSlidesChanged();
            return;
        }

        if (ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
            return;

        if (!TryMeasure())
        {
            Dispatcher.BeginInvoke(Rebuild, DispatcherPriority.Loaded);
            return;
        }

        double height = _contentDock.ActualHeight > 0 ? _contentDock.ActualHeight : ActualHeight;
        for (int i = 0; i < _count; i++)
        {
            if (ItemContainerGenerator.ContainerFromIndex(i) is not FrameworkElement element)
                continue;

            element.Width = _elementWidth;
            element.Height = height;
            element.RenderTransformOrigin = new Point(0.5, 1);
            if (element.RenderTransform is not TransformGroup)
                element.RenderTransform = CreateTransform(ScaleSide);

            _elements[i] = element;
        }

        if (_elements.Count == 0)
        {
            Dispatcher.BeginInvoke(Rebuild, DispatcherPriority.Loaded);
            return;
        }

        int desired = SelectedIndex >= 0 ? SelectedIndex : 0;
        ArrangeToIndexImmediate(desired);
        NotifyNavSlidesChanged();
    }

    private Canvas? ResolveItemsCanvas()
    {
        if (_contentDock != null)
            return _contentDock;

        if (_itemsPresenter == null)
            return null;

        // ItemsPanel Canvas is the visual child of ItemsPresenter once generated.
        if (VisualTreeHelper.GetChildrenCount(_itemsPresenter) > 0
            && VisualTreeHelper.GetChild(_itemsPresenter, 0) is Canvas canvas)
        {
            _contentDock = canvas;
            return canvas;
        }

        return null;
    }

    private bool TryMeasure()
    {
        if (_contentDock == null) return false;
        double width = _contentDock.ActualWidth;
        double height = _contentDock.ActualHeight;
        if (width <= 0 || height <= 0)
        {
            // ItemsPanel Canvas has fixed design size; fall back when not measured yet.
            width = _contentDock.Width;
            height = _contentDock.Height;
        }

        if (width <= 0 || height <= 0 || double.IsNaN(width) || double.IsNaN(height))
            return false;

        _shellWidth = width;
        _elementWidth = _shellWidth * ElementScale;
        _leftDock = 0;
        _centerDock = _shellWidth * DockOffset;
        _rightDock = _shellWidth - _elementWidth;
        return true;
    }

    private static TransformGroup CreateTransform(double scaleY)
    {
        return new TransformGroup
        {
            Children =
            {
                new ScaleTransform { ScaleY = scaleY },
                new SkewTransform(),
                new RotateTransform(),
                new TranslateTransform(),
            },
        };
    }

    private static void SetScale(FrameworkElement element, double scaleY)
    {
        if (element.RenderTransform is TransformGroup group
            && group.Children.Count > 0
            && group.Children[0] is ScaleTransform scale)
        {
            scale.ScaleY = scaleY;
        }
    }

    private static void SetActive(FrameworkElement element, bool active)
    {
        if (element is CardCarouselItem card)
            card.IsActive = active;
    }

    private void ArrangeToIndexImmediate(int index)
    {
        if (index < 0 || index >= _count) return;

        int prev = index - 1;
        if (prev < 0) prev = _count - 1;
        int next = index + 1;
        if (next >= _count) next = 0;

        _slots.Clear();
        _buffer.Clear();

        for (int i = 0; i < _count; i++)
        {
            if (!_elements.TryGetValue(i, out FrameworkElement? element) || element == null)
                continue;

            if (i == prev && _count > 1)
            {
                Canvas.SetLeft(element, _leftDock);
                Panel.SetZIndex(element, (int)SlotZ.Left);
                SetScale(element, ScaleSide);
                SetActive(element, false);
                _slots[Slot.Left] = i;
            }
            else if (i == index)
            {
                Canvas.SetLeft(element, _centerDock);
                Panel.SetZIndex(element, (int)SlotZ.Center);
                SetScale(element, ScaleCenter);
                SetActive(element, true);
                _slots[Slot.Center] = i;
            }
            else if (i == next && _count > 1 && next != prev)
            {
                Canvas.SetLeft(element, _rightDock);
                Panel.SetZIndex(element, (int)SlotZ.Right);
                SetScale(element, ScaleSide);
                SetActive(element, false);
                _slots[Slot.Right] = i;
            }
            else
            {
                _buffer.AddLast(i);
                Canvas.SetLeft(element, _centerDock);
                Panel.SetZIndex(element, i);
                SetScale(element, ScaleSide);
                SetActive(element, false);
            }
        }

        SetSelectedIndexSilent(index);
    }

    // --- Animation -----------------------------------------------------------

    private void PlayToIndex(int index)
    {
        if (index < 0 || index >= _count || _isAnimating)
            return;

        int center = GetSlotIndex(Slot.Center);
        if (center == index)
            return;

        if (GetSlotIndex(Slot.Left) == index)
        {
            PlayLeftToRight();
            return;
        }

        if (GetSlotIndex(Slot.Right) == index)
        {
            PlayRightToLeft();
            return;
        }

        PlayJumpTo(index);
    }

    private bool PlayRightToLeft()
    {
        if (!BeginStoryboard())
            return false;

        int nextIndex = -1;

        // Left → buffer (behind center)
        int left = GetSlotIndex(Slot.Left);
        if (TryGetElement(left, out FrameworkElement? leftEl) && leftEl != null)
        {
            Animate(leftEl, left + 1, _centerDock, ScaleSide);
            _buffer.AddLast(left);
        }
        _slots[Slot.Left] = -1;

        // Center → left
        int center = GetSlotIndex(Slot.Center);
        if (TryGetElement(center, out FrameworkElement? centerEl) && centerEl != null)
        {
            Animate(centerEl, (int)SlotZ.Left, _leftDock, ScaleSide);
            SetActive(centerEl, false);
            _slots[Slot.Left] = center;
        }
        _slots[Slot.Center] = -1;

        // Right → center
        int right = GetSlotIndex(Slot.Right);
        if (TryGetElement(right, out FrameworkElement? rightEl) && rightEl != null)
        {
            Animate(rightEl, (int)SlotZ.Center, _centerDock, ScaleCenter);
            SetActive(rightEl, true);
            _slots[Slot.Center] = right;
            SetSelectedIndexSilent(right);
            nextIndex = right + 1;
            if (nextIndex >= _count) nextIndex = 0;
        }
        _slots[Slot.Right] = -1;

        PromoteToSlot(Slot.Right, nextIndex, preferFirst: true);
        _storyboard!.Begin();
        return true;
    }

    private bool PlayLeftToRight()
    {
        if (!BeginStoryboard())
            return false;

        int nextIndex = -1;

        // Right → buffer
        int right = GetSlotIndex(Slot.Right);
        if (TryGetElement(right, out FrameworkElement? rightEl) && rightEl != null)
        {
            Animate(rightEl, right + 1, _centerDock, ScaleSide);
            _buffer.AddFirst(right);
        }
        _slots[Slot.Right] = -1;

        // Center → right
        int center = GetSlotIndex(Slot.Center);
        if (TryGetElement(center, out FrameworkElement? centerEl) && centerEl != null)
        {
            Animate(centerEl, (int)SlotZ.Right, _rightDock, ScaleSide);
            SetActive(centerEl, false);
            _slots[Slot.Right] = center;
        }
        _slots[Slot.Center] = -1;

        // Left → center
        int left = GetSlotIndex(Slot.Left);
        if (TryGetElement(left, out FrameworkElement? leftEl) && leftEl != null)
        {
            Animate(leftEl, (int)SlotZ.Center, _centerDock, ScaleCenter);
            SetActive(leftEl, true);
            _slots[Slot.Center] = left;
            SetSelectedIndexSilent(left);
            nextIndex = left - 1;
            if (nextIndex < 0) nextIndex = _count - 1;
        }
        _slots[Slot.Left] = -1;

        PromoteToSlot(Slot.Left, nextIndex, preferFirst: false);
        _storyboard!.Begin();
        return true;
    }

    private bool PlayJumpTo(int index)
    {
        if (index < 0 || index >= _count)
            return false;
        if (!BeginStoryboard())
            return false;

        int prev = index - 1;
        if (prev < 0) prev = _count - 1;
        int next = index + 1;
        if (next >= _count) next = 0;

        _buffer.Clear();

        // Collapse current left/center/right into the stack first.
        CollapseSlot(Slot.Left);
        CollapseSlot(Slot.Center);
        CollapseSlot(Slot.Right);

        for (int i = 0; i < _count; i++)
        {
            if (!_elements.TryGetValue(i, out FrameworkElement? element) || element == null)
                continue;

            if (i == prev && _count > 1)
            {
                Animate(element, (int)SlotZ.Left, _leftDock, ScaleSide, delayZIndex: true);
                SetActive(element, false);
                _slots[Slot.Left] = i;
            }
            else if (i == index)
            {
                Animate(element, (int)SlotZ.Center, _centerDock, ScaleCenter, delayZIndex: true);
                SetActive(element, true);
                _slots[Slot.Center] = i;
            }
            else if (i == next && _count > 1 && next != prev)
            {
                Animate(element, (int)SlotZ.Right, _rightDock, ScaleSide, delayZIndex: true);
                SetActive(element, false);
                _slots[Slot.Right] = i;
            }
            else
            {
                _buffer.AddLast(i);
            }
        }

        SetSelectedIndexSilent(index);
        _storyboard!.Begin();
        return true;
    }

    private void CollapseSlot(Slot slot)
    {
        int index = GetSlotIndex(slot);
        if (!TryGetElement(index, out FrameworkElement? element) || element == null)
        {
            _slots[slot] = -1;
            return;
        }

        Animate(element, index + 1, _centerDock, ScaleSide);
        SetActive(element, false);
        _slots[slot] = -1;
    }

    private void PromoteToSlot(Slot slot, int preferredIndex, bool preferFirst)
    {
        int index = preferredIndex;
        if (index >= 0 && _buffer.Contains(index))
            _buffer.Remove(index);
        else if (_buffer.Count > 0)
        {
            if (preferFirst)
            {
                index = _buffer.First!.Value;
                _buffer.RemoveFirst();
            }
            else
            {
                index = _buffer.Last!.Value;
                _buffer.RemoveLast();
            }
        }
        else
        {
            _slots[slot] = -1;
            return;
        }

        if (!TryGetElement(index, out FrameworkElement? element) || element == null)
        {
            _slots[slot] = -1;
            return;
        }

        double left = slot switch
        {
            Slot.Left => _leftDock,
            Slot.Right => _rightDock,
            _ => _centerDock,
        };
        int z = slot switch
        {
            Slot.Left => (int)SlotZ.Left,
            Slot.Right => (int)SlotZ.Right,
            _ => (int)SlotZ.Center,
        };

        Animate(element, z, left, ScaleSide);
        SetActive(element, false);
        _slots[slot] = index;
    }

    private bool BeginStoryboard()
    {
        if (_isAnimating)
            return false;

        _storyboard ??= new Storyboard();
        _storyboard.Completed -= OnStoryboardCompleted;
        _storyboard.Completed += OnStoryboardCompleted;
        _storyboard.Stop();
        _storyboard.Children.Clear();
        _isAnimating = true;
        return true;
    }

    private void OnStoryboardCompleted(object? sender, EventArgs e) =>
        _isAnimating = false;

    private void Animate(
        FrameworkElement element,
        int zIndex,
        double left,
        double scaleY,
        bool delayZIndex = false)
    {
        if (_storyboard == null) return;

        TimeSpan duration = MotionDuration.HasTimeSpan
            ? MotionDuration.TimeSpan
            : TimeSpan.FromSeconds(AnimationSeconds);
        TimeSpan zDuration = TimeSpan.FromSeconds(Math.Max(duration.TotalSeconds, ZIndexSeconds));

        var zAnim = new Int32Animation
        {
            To = zIndex,
            Duration = zDuration,
            EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut },
        };
        Storyboard.SetTarget(zAnim, element);
        Storyboard.SetTargetProperty(zAnim, new PropertyPath("(Panel.ZIndex)"));
        _storyboard.Children.Add(zAnim);

        var leftAnim = new DoubleAnimation
        {
            To = left,
            Duration = duration,
            BeginTime = delayZIndex ? TimeSpan.FromSeconds(ZIndexSeconds) : TimeSpan.Zero,
            EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut },
        };
        Storyboard.SetTarget(leftAnim, element);
        Storyboard.SetTargetProperty(leftAnim, new PropertyPath("(Canvas.Left)"));
        _storyboard.Children.Add(leftAnim);

        var scaleAnim = new DoubleAnimation
        {
            To = scaleY,
            Duration = duration,
            EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut },
        };
        Storyboard.SetTarget(scaleAnim, element);
        Storyboard.SetTargetProperty(
            scaleAnim,
            new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleY)"));
        _storyboard.Children.Add(scaleAnim);
    }

    private int GetSlotIndex(Slot slot) =>
        _slots.TryGetValue(slot, out int index) ? index : -1;

    private bool TryGetElement(int index, out FrameworkElement? element)
    {
        if (index < 0)
        {
            element = null;
            return false;
        }

        return _elements.TryGetValue(index, out element);
    }

    private void SetSelectedIndexSilent(int index)
    {
        if (SelectedIndex == index) return;
        int oldIndex = _reportedActiveIndex;
        _suppressSelection = true;
        try
        {
            SelectedIndex = index;
        }
        finally
        {
            _suppressSelection = false;
        }

        _reportedActiveIndex = index;
        if (oldIndex != index)
            RaiseEvent(new RoutedPropertyChangedEventArgs<int>(oldIndex, index, ActiveIndexChangedEvent));
        NotifyNavSlidesChanged();
    }

    private void HandleClick(Point pos)
    {
        if (_contentDock == null || _count == 0) return;

        IInputElement? hit = _contentDock.InputHitTest(pos);
        DependencyObject? current = hit as DependencyObject;
        while (current != null)
        {
            foreach (KeyValuePair<int, FrameworkElement> pair in _elements)
            {
                if (ReferenceEquals(pair.Value, current))
                {
                    SelectedIndex = pair.Key;
                    return;
                }
            }

            current = VisualTreeHelper.GetParent(current);
        }
    }

    // --- Autoplay ------------------------------------------------------------

    private static void OnAutoplaySettingsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        ((CardCarousel)d).UpdateAutoplayTimer();

    private void UpdateAutoplayTimer()
    {
        if (!IsAutoplayEnabled || Items.Count <= 1 || AutoplayInterval <= 0)
        {
            _autoplayTimer?.Stop();
            return;
        }

        _autoplayTimer ??= new DispatcherTimer();
        _autoplayTimer.Tick -= OnAutoplayTick;
        _autoplayTimer.Tick += OnAutoplayTick;
        _autoplayTimer.Interval = TimeSpan.FromMilliseconds(AutoplayInterval);
        _autoplayTimer.Stop();
        _autoplayTimer.Start();
    }

    private void OnAutoplayTick(object? sender, EventArgs e)
    {
        if (_autoplayPaused || _isAnimating || IsMouseOver) return;
        MoveNext();
    }
}
