using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// A title-style tab control with closable tabs, an optional add button,
/// and a sliding content transition — suitable for document / browser-like headers.
/// </summary>
/// <remarks>
/// Styles use theme <c>DynamicResource</c> brushes so light and dark modes are supported
/// without rebuilding templates in code.
/// </remarks>
[TemplatePart(Name = PartContentPresenter, Type = typeof(ContentPresenter))]
[TemplatePart(Name = PartContentBorder, Type = typeof(Border))]
[TemplatePart(Name = PartAddButton, Type = typeof(Button))]
[TemplatePart(Name = PartTabsScrollViewer, Type = typeof(ScrollViewer))]
[TemplatePart(Name = PartTabsScrollBar, Type = typeof(ScrollBar))]
public class TabsTitleControl : Selector
{
    private const string PartContentPresenter = "PART_ContentPresenter";
    private const string PartContentBorder = "PART_ContentBorder";
    private const string PartAddButton = "PART_AddButton";
    private const string PartTabsScrollViewer = "PART_TabsScrollViewer";
    private const string PartTabsScrollBar = "PART_TabsScrollBar";

    private ContentPresenter? _contentPresenter;
    private Border? _contentBorder;
    private Button? _addButton;
    private ScrollViewer? _tabsScrollViewer;
    private ScrollBar? _tabsScrollBar;
    private int _previousIndex;
    private bool _isAnimating;
    private bool _isSyncingScrollBar;
    private bool _tabsScrollBarDesiredVisible;
    private bool _tabsScrollBarHitTestVisible;

    private static readonly TimeSpan TabsScrollBarFadeInDuration = TimeSpan.FromMilliseconds(100);
    private static readonly TimeSpan TabsScrollBarFadeOutDuration = TimeSpan.FromMilliseconds(800);

    /// <summary>Identifies the <see cref="HeaderBackground"/> dependency property.</summary>
    public static readonly DependencyProperty HeaderBackgroundProperty = DependencyProperty.Register(
        nameof(HeaderBackground),
        typeof(Brush),
        typeof(TabsTitleControl),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Gets or sets the background of the tab header strip.</summary>
    public Brush? HeaderBackground
    {
        get => (Brush?)GetValue(HeaderBackgroundProperty);
        set => SetValue(HeaderBackgroundProperty, value);
    }

    /// <summary>Identifies the <see cref="TabBackground"/> dependency property.</summary>
    public static readonly DependencyProperty TabBackgroundProperty = DependencyProperty.Register(
        nameof(TabBackground),
        typeof(Brush),
        typeof(TabsTitleControl),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Gets or sets the default background of unselected tabs.</summary>
    public Brush? TabBackground
    {
        get => (Brush?)GetValue(TabBackgroundProperty);
        set => SetValue(TabBackgroundProperty, value);
    }

    /// <summary>Identifies the <see cref="TabSelectedBackground"/> dependency property.</summary>
    public static readonly DependencyProperty TabSelectedBackgroundProperty = DependencyProperty.Register(
        nameof(TabSelectedBackground),
        typeof(Brush),
        typeof(TabsTitleControl),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Gets or sets the background of the selected tab.</summary>
    public Brush? TabSelectedBackground
    {
        get => (Brush?)GetValue(TabSelectedBackgroundProperty);
        set => SetValue(TabSelectedBackgroundProperty, value);
    }

    /// <summary>Identifies the <see cref="TabHoverBackground"/> dependency property.</summary>
    public static readonly DependencyProperty TabHoverBackgroundProperty = DependencyProperty.Register(
        nameof(TabHoverBackground),
        typeof(Brush),
        typeof(TabsTitleControl),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Gets or sets the background of a hovered unselected tab.</summary>
    public Brush? TabHoverBackground
    {
        get => (Brush?)GetValue(TabHoverBackgroundProperty);
        set => SetValue(TabHoverBackgroundProperty, value);
    }

    /// <summary>Identifies the <see cref="TabForeground"/> dependency property.</summary>
    public static readonly DependencyProperty TabForegroundProperty = DependencyProperty.Register(
        nameof(TabForeground),
        typeof(Brush),
        typeof(TabsTitleControl),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Gets or sets the foreground of unselected tabs.</summary>
    public Brush? TabForeground
    {
        get => (Brush?)GetValue(TabForegroundProperty);
        set => SetValue(TabForegroundProperty, value);
    }

    /// <summary>Identifies the <see cref="TabSelectedForeground"/> dependency property.</summary>
    public static readonly DependencyProperty TabSelectedForegroundProperty = DependencyProperty.Register(
        nameof(TabSelectedForeground),
        typeof(Brush),
        typeof(TabsTitleControl),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Gets or sets the foreground of the selected tab.</summary>
    public Brush? TabSelectedForeground
    {
        get => (Brush?)GetValue(TabSelectedForegroundProperty);
        set => SetValue(TabSelectedForegroundProperty, value);
    }

    /// <summary>Identifies the <see cref="CloseButtonForeground"/> dependency property.</summary>
    public static readonly DependencyProperty CloseButtonForegroundProperty = DependencyProperty.Register(
        nameof(CloseButtonForeground),
        typeof(Brush),
        typeof(TabsTitleControl),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Gets or sets the foreground of tab close buttons.</summary>
    public Brush? CloseButtonForeground
    {
        get => (Brush?)GetValue(CloseButtonForegroundProperty);
        set => SetValue(CloseButtonForegroundProperty, value);
    }

    /// <summary>Identifies the <see cref="AddButtonForeground"/> dependency property.</summary>
    public static readonly DependencyProperty AddButtonForegroundProperty = DependencyProperty.Register(
        nameof(AddButtonForeground),
        typeof(Brush),
        typeof(TabsTitleControl),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Gets or sets the foreground of the add-tab button.</summary>
    public Brush? AddButtonForeground
    {
        get => (Brush?)GetValue(AddButtonForegroundProperty);
        set => SetValue(AddButtonForegroundProperty, value);
    }

    /// <summary>Identifies the <see cref="TabFontSize"/> dependency property.</summary>
    public static readonly DependencyProperty TabFontSizeProperty = DependencyProperty.Register(
        nameof(TabFontSize),
        typeof(double),
        typeof(TabsTitleControl),
        new FrameworkPropertyMetadata(13.0));

    /// <summary>Gets or sets the font size of tab headers.</summary>
    public double TabFontSize
    {
        get => (double)GetValue(TabFontSizeProperty);
        set => SetValue(TabFontSizeProperty, value);
    }

    /// <summary>Identifies the <see cref="TabPadding"/> dependency property.</summary>
    public static readonly DependencyProperty TabPaddingProperty = DependencyProperty.Register(
        nameof(TabPadding),
        typeof(Thickness),
        typeof(TabsTitleControl),
        new FrameworkPropertyMetadata(new Thickness(12, 8, 8, 8)));

    /// <summary>Gets or sets the padding of each tab item.</summary>
    public Thickness TabPadding
    {
        get => (Thickness)GetValue(TabPaddingProperty);
        set => SetValue(TabPaddingProperty, value);
    }

    /// <summary>Identifies the <see cref="ShowAddButton"/> dependency property.</summary>
    public static readonly DependencyProperty ShowAddButtonProperty = DependencyProperty.Register(
        nameof(ShowAddButton),
        typeof(bool),
        typeof(TabsTitleControl),
        new FrameworkPropertyMetadata(true));

    /// <summary>Gets or sets whether the add-tab button is visible.</summary>
    public bool ShowAddButton
    {
        get => (bool)GetValue(ShowAddButtonProperty);
        set => SetValue(ShowAddButtonProperty, value);
    }

    /// <summary>Delegate for <see cref="CloseTab"/>.</summary>
    public delegate void TabsTitleCloseRoutedEventHandler(object sender, TabsTitleCloseRoutedEventArgs e);

    /// <summary>Identifies the <see cref="AddTab"/> routed event.</summary>
    public static readonly RoutedEvent AddTabEvent = EventManager.RegisterRoutedEvent(
        nameof(AddTab),
        RoutingStrategy.Bubble,
        typeof(RoutedEventHandler),
        typeof(TabsTitleControl));

    /// <summary>Occurs when the add-tab button is clicked.</summary>
    public event RoutedEventHandler AddTab
    {
        add => AddHandler(AddTabEvent, value);
        remove => RemoveHandler(AddTabEvent, value);
    }

    /// <summary>Identifies the <see cref="CloseTab"/> routed event.</summary>
    public static readonly RoutedEvent CloseTabEvent = EventManager.RegisterRoutedEvent(
        nameof(CloseTab),
        RoutingStrategy.Bubble,
        typeof(TabsTitleCloseRoutedEventHandler),
        typeof(TabsTitleControl));

    /// <summary>
    /// Occurs when a tab close is requested. Mark the event as handled to prevent automatic removal.
    /// </summary>
    public event TabsTitleCloseRoutedEventHandler CloseTab
    {
        add => AddHandler(CloseTabEvent, value);
        remove => RemoveHandler(CloseTabEvent, value);
    }

    static TabsTitleControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(TabsTitleControl),
            new FrameworkPropertyMetadata(typeof(TabsTitleControl)));
    }

    public TabsTitleControl()
    {
        Background = Brushes.Transparent;
        BorderThickness = new Thickness(0);
        Loaded += OnLoaded;
        SelectionChanged += OnSelectionChanged;
        AddHandler(TabsTitleControlItem.CloseTabEvent, new RoutedEventHandler(OnItemCloseTab));
    }

    protected override bool IsItemItsOwnContainerOverride(object item) => item is TabsTitleControlItem;

    protected override DependencyObject GetContainerForItemOverride() => new TabsTitleControlItem();

    public override void OnApplyTemplate()
    {
        _addButton?.Click -= OnAddButtonClick;
        DetachTabsScrollChrome();

        base.OnApplyTemplate();

        _contentPresenter = GetTemplateChild(PartContentPresenter) as ContentPresenter;
        _contentBorder = GetTemplateChild(PartContentBorder) as Border;
        _addButton = GetTemplateChild(PartAddButton) as Button;
        _tabsScrollViewer = GetTemplateChild(PartTabsScrollViewer) as ScrollViewer;
        _tabsScrollBar = GetTemplateChild(PartTabsScrollBar) as ScrollBar;

        _addButton?.Click += OnAddButtonClick;
        AttachTabsScrollChrome();

        // Template Freezables are immutable — install a fresh transform for slide animation.
        _contentBorder?.RenderTransform = new TranslateTransform();

        ApplySelection(animate: false);
    }

    private void AttachTabsScrollChrome()
    {
        if (_tabsScrollViewer is null || _tabsScrollBar is null)
        {
            return;
        }

        _tabsScrollViewer.ScrollChanged += OnTabsScrollChanged;
        _tabsScrollBar.Scroll += OnTabsScrollBarScroll;
        SyncTabsScrollBarFromViewer();
        UpdateTabsScrollBarHoverState();
    }

    private void DetachTabsScrollChrome()
    {
        if (_tabsScrollViewer is not null)
        {
            _tabsScrollViewer.ScrollChanged -= OnTabsScrollChanged;
        }

        if (_tabsScrollBar is not null)
        {
            _tabsScrollBar.Scroll -= OnTabsScrollBarScroll;
            _tabsScrollBar.BeginAnimation(OpacityProperty, null);
            _tabsScrollBar.Opacity = 0;
            _tabsScrollBar.IsHitTestVisible = false;
        }

        _tabsScrollBarDesiredVisible = false;
        _tabsScrollBarHitTestVisible = false;
    }

    private void OnTabsScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        if (e.HorizontalChange == 0 && e.ExtentWidthChange == 0 && e.ViewportWidthChange == 0)
        {
            return;
        }

        SyncTabsScrollBarFromViewer();
    }

    private void OnTabsScrollBarScroll(object sender, ScrollEventArgs e)
    {
        if (_tabsScrollViewer is null || _isSyncingScrollBar)
        {
            return;
        }

        _tabsScrollViewer.ScrollToHorizontalOffset(e.NewValue);
    }

    private void SyncTabsScrollBarFromViewer()
    {
        if (_tabsScrollViewer is null || _tabsScrollBar is null)
        {
            return;
        }

        _isSyncingScrollBar = true;
        try
        {
            _tabsScrollBar.Value = _tabsScrollViewer.HorizontalOffset;
        }
        finally
        {
            _isSyncingScrollBar = false;
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        UpdateTabsScrollBarHoverState();
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);
        UpdateTabsScrollBarHoverState();
    }

    /// <summary>
    /// Shows the tab-strip scrollbar only when the pointer is geometrically inside the
    /// scrollable tabs region (or while dragging the thumb). Do not use <see cref="UIElement.IsMouseOver"/>
    /// on the header — tab Content remains a logical child of each item, so WPF can report
    /// IsMouseOver on the strip while the pointer is actually over content.
    /// Fade timings match VS Code monaco scrollbars: 100ms in, 800ms out.
    /// </summary>
    private void UpdateTabsScrollBarHoverState()
    {
        if (_tabsScrollBar is null || _tabsScrollViewer is null)
        {
            return;
        }

        var show = _tabsScrollBar.IsMouseCaptureWithin
            || IsPointerInsideElement(_tabsScrollViewer)
            || (_tabsScrollBarHitTestVisible && IsPointerInsideElement(_tabsScrollBar));

        if (show == _tabsScrollBarDesiredVisible)
        {
            return;
        }

        _tabsScrollBarDesiredVisible = show;
        AnimateTabsScrollBarOpacity(show);
    }

    private void AnimateTabsScrollBarOpacity(bool show)
    {
        if (_tabsScrollBar is null)
        {
            return;
        }

        // Stop any in-flight fade so the next animation starts from the current visual opacity.
        var from = _tabsScrollBar.Opacity;
        _tabsScrollBar.BeginAnimation(OpacityProperty, null);
        _tabsScrollBar.Opacity = from;

        if (show)
        {
            _tabsScrollBarHitTestVisible = true;
            _tabsScrollBar.IsHitTestVisible = true;

            var fadeIn = new DoubleAnimation(from, 1, TabsScrollBarFadeInDuration)
            {
                // VS Code: transition: opacity 100ms linear
                EasingFunction = null,
                FillBehavior = FillBehavior.HoldEnd,
            };
            fadeIn.Completed += (_, _) =>
            {
                if (_tabsScrollBar is null || !_tabsScrollBarDesiredVisible)
                {
                    return;
                }

                _tabsScrollBar.BeginAnimation(OpacityProperty, null);
                _tabsScrollBar.Opacity = 1;
            };
            _tabsScrollBar.BeginAnimation(OpacityProperty, fadeIn);
            return;
        }

        var fadeOut = new DoubleAnimation(from, 0, TabsScrollBarFadeOutDuration)
        {
            // VS Code: .invisible.fade { transition: opacity 800ms linear }
            EasingFunction = null,
            FillBehavior = FillBehavior.HoldEnd,
        };
        // VS Code applies pointer-events: none as soon as the hide class is set.
        _tabsScrollBarHitTestVisible = false;
        _tabsScrollBar.IsHitTestVisible = false;
        fadeOut.Completed += (_, _) =>
        {
            if (_tabsScrollBar is null || _tabsScrollBarDesiredVisible)
            {
                return;
            }

            _tabsScrollBar.BeginAnimation(OpacityProperty, null);
            _tabsScrollBar.Opacity = 0;
        };
        _tabsScrollBar.BeginAnimation(OpacityProperty, fadeOut);
    }

    private static bool IsPointerInsideElement(FrameworkElement element)
    {
        if (!element.IsVisible || element.ActualWidth <= 0 || element.ActualHeight <= 0)
        {
            return false;
        }

        var pos = Mouse.GetPosition(element);
        return pos.X >= 0 && pos.Y >= 0 && pos.X <= element.ActualWidth && pos.Y <= element.ActualHeight;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (Items.Count == 0)
        {
            return;
        }

        if (SelectedIndex < 0)
        {
            SelectedIndex = 0;
        }

        ApplySelection(animate: false);
    }

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ApplySelection(animate: true);
    }

    private void OnAddButtonClick(object sender, RoutedEventArgs e)
    {
        RaiseEvent(new RoutedEventArgs(AddTabEvent, this));
    }

    private void OnItemCloseTab(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is not TabsTitleControlItem tabItem)
        {
            return;
        }

        var args = new TabsTitleCloseRoutedEventArgs(CloseTabEvent, tabItem);
        RaiseEvent(args);

        if (!args.Handled)
        {
            var index = ItemContainerGenerator.IndexFromContainer(tabItem);
            if (index >= 0 && index < Items.Count)
            {
                Items.RemoveAt(index);
            }
        }

        e.Handled = true;
    }

    private void ApplySelection(bool animate)
    {
        if (_contentPresenter is null)
        {
            return;
        }

        var newIndex = SelectedIndex;
        if (newIndex < 0 || newIndex >= Items.Count)
        {
            _contentPresenter.Content = null;
            return;
        }

        if (!animate || newIndex == _previousIndex || _contentPresenter.Content is null || _contentBorder is null || _isAnimating)
        {
            SetContentFromIndex(newIndex);
            _previousIndex = newIndex;
            return;
        }

        var slideRight = newIndex > _previousIndex;
        _previousIndex = newIndex;
        _isAnimating = true;

        var slideOutOffset = slideRight ? -50.0 : 50.0;
        var slideInStart = slideRight ? 50.0 : -50.0;

        // Always use a fresh, unfrozen transform — template Freezables cannot be animated.
        var translate = new TranslateTransform();
        _contentBorder.BeginAnimation(OpacityProperty, null);
        _contentBorder.RenderTransform = translate;

        var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(120))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn },
        };
        var slideOut = new DoubleAnimation(0, slideOutOffset, TimeSpan.FromMilliseconds(180))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn },
        };

        slideOut.Completed += (_, _) =>
        {
            if (_contentBorder is null)
            {
                _isAnimating = false;
                return;
            }

            SetContentFromIndex(newIndex);

            var slideTranslate = new TranslateTransform(slideInStart, 0);
            _contentBorder.RenderTransform = slideTranslate;

            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(120))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
            };
            var slideIn = new DoubleAnimation(slideInStart, 0, TimeSpan.FromMilliseconds(180))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
            };
            slideIn.Completed += (_, _) => _isAnimating = false;

            _contentBorder.BeginAnimation(OpacityProperty, fadeIn);
            slideTranslate.BeginAnimation(TranslateTransform.XProperty, slideIn);
        };

        _contentBorder.BeginAnimation(OpacityProperty, fadeOut);
        translate.BeginAnimation(TranslateTransform.XProperty, slideOut);
    }

    private void SetContentFromIndex(int index)
    {
        if (_contentPresenter is null || index < 0 || index >= Items.Count)
        {
            return;
        }

        if (ItemContainerGenerator.ContainerFromIndex(index) is TabsTitleControlItem container)
        {
            _contentPresenter.Content = container.Content;
            return;
        }

        if (Items[index] is TabsTitleControlItem item)
        {
            _contentPresenter.Content = item.Content;
        }
    }
}
