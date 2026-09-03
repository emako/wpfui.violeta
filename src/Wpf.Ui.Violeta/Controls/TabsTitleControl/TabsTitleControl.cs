using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
public class TabsTitleControl : Selector
{
    private const string PartContentPresenter = "PART_ContentPresenter";
    private const string PartContentBorder = "PART_ContentBorder";
    private const string PartAddButton = "PART_AddButton";

    private ContentPresenter? _contentPresenter;
    private Border? _contentBorder;
    private Button? _addButton;
    private int _previousIndex;
    private bool _isAnimating;

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
        if (_addButton is not null)
        {
            _addButton.Click -= OnAddButtonClick;
        }

        base.OnApplyTemplate();

        _contentPresenter = GetTemplateChild(PartContentPresenter) as ContentPresenter;
        _contentBorder = GetTemplateChild(PartContentBorder) as Border;
        _addButton = GetTemplateChild(PartAddButton) as Button;

        if (_addButton is not null)
        {
            _addButton.Click += OnAddButtonClick;
        }

        // Template Freezables are immutable — install a fresh transform for slide animation.
        if (_contentBorder is not null)
        {
            _contentBorder.RenderTransform = new TranslateTransform();
        }

        ApplySelection(animate: false);
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
