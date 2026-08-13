using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Pagination tablist for a <see cref="Carousel"/> or <see cref="CardCarousel"/>.
/// Mirrors Fluent UI React <c>CarouselNav</c> (<c>role="tablist"</c>).
/// </summary>
[TemplatePart(Name = PART_ItemsHost, Type = typeof(Panel))]
public class CarouselNav : Control
{
    public const string PART_ItemsHost = "PART_ItemsHost";

    private Panel? _itemsHost;
    private ICarouselNavHost? _attachedHost;
    private bool _syncingFromHost;

    static CarouselNav()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(CarouselNav),
            new FrameworkPropertyMetadata(typeof(CarouselNav)));
    }

    public CarouselNav()
    {
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    // --- Dependency Properties ------------------------------------------------

    public static readonly DependencyProperty AppearanceProperty =
        DependencyProperty.Register(
            nameof(Appearance),
            typeof(CarouselNavAppearance),
            typeof(CarouselNav),
            new PropertyMetadata(CarouselNavAppearance.Default, OnAppearanceChanged));

    public static readonly DependencyProperty TotalSlidesProperty =
        DependencyProperty.Register(
            nameof(TotalSlides),
            typeof(int),
            typeof(CarouselNav),
            new PropertyMetadata(0, OnTotalSlidesChanged));

    public static readonly DependencyProperty SelectedIndexProperty =
        DependencyProperty.Register(
            nameof(SelectedIndex),
            typeof(int),
            typeof(CarouselNav),
            new FrameworkPropertyMetadata(
                -1,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedIndexChanged));

    public static readonly DependencyProperty CarouselProperty =
        DependencyProperty.Register(
            nameof(Carousel),
            typeof(ICarouselNavHost),
            typeof(CarouselNav),
            new PropertyMetadata(null, OnCarouselChanged));

    /// <summary>Enables brand styling on selected nav buttons.</summary>
    public CarouselNavAppearance Appearance
    {
        get => (CarouselNavAppearance)GetValue(AppearanceProperty);
        set => SetValue(AppearanceProperty, value);
    }

    /// <summary>
    /// Total slides to render. Usually subscribed from a parent host;
    /// can be set manually when used standalone.
    /// </summary>
    public int TotalSlides
    {
        get => (int)GetValue(TotalSlidesProperty);
        set => SetValue(TotalSlidesProperty, value);
    }

    /// <summary>Currently selected page index.</summary>
    public int SelectedIndex
    {
        get => (int)GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    /// <summary>
    /// Explicit host association. When null, the nearest ancestor
    /// <see cref="ICarouselNavHost"/> (<see cref="Carousel"/> / <see cref="CardCarousel"/>) is used.
    /// </summary>
    public ICarouselNavHost? Carousel
    {
        get => (ICarouselNavHost?)GetValue(CarouselProperty);
        set => SetValue(CarouselProperty, value);
    }

    // --- Template ------------------------------------------------------------

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _itemsHost = GetTemplateChild(PART_ItemsHost) as Panel;
        RebuildButtons();
        UpdateSelection();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (TotalSlides <= 0) return;

        if (e.Key is Key.Left or Key.Up)
        {
            SelectIndex(SelectedIndex <= 0 ? 0 : SelectedIndex - 1);
            e.Handled = true;
        }
        else if (e.Key is Key.Right or Key.Down)
        {
            SelectIndex(SelectedIndex >= TotalSlides - 1 ? TotalSlides - 1 : SelectedIndex + 1);
            e.Handled = true;
        }
        else if (e.Key == Key.Home)
        {
            SelectIndex(0);
            e.Handled = true;
        }
        else if (e.Key == Key.End)
        {
            SelectIndex(TotalSlides - 1);
            e.Handled = true;
        }
    }

    // --- Host association -----------------------------------------------------

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        AttachToHost(ResolveHost());
        // Items may already exist before Nav loads; force a sync + rebuild.
        SyncFromHost();
        RebuildButtons();
        UpdateSelection();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e) => DetachFromHost();

    private static void OnCarouselChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var nav = (CarouselNav)d;
        nav.AttachToHost(nav.ResolveHost());
        nav.RebuildButtons();
        nav.UpdateSelection();
    }

    private ICarouselNavHost? ResolveHost()
    {
        if (Carousel != null) return Carousel;
        return FindAncestorHost(this);
    }

    private static ICarouselNavHost? FindAncestorHost(DependencyObject? start)
    {
        DependencyObject? current = start;
        while (current != null)
        {
            if (current is ICarouselNavHost host)
                return host;

            if (current is FrameworkElement { TemplatedParent: ICarouselNavHost templated })
                return templated;

            current = VisualTreeHelper.GetParent(current)
                ?? (current as FrameworkElement)?.Parent;
        }
        return null;
    }

    private void AttachToHost(ICarouselNavHost? host)
    {
        if (ReferenceEquals(_attachedHost, host))
        {
            SyncFromHost();
            return;
        }

        DetachFromHost();
        _attachedHost = host;
        if (_attachedHost == null) return;

        _attachedHost.ActiveIndexChanged += OnHostActiveIndexChanged;
        _attachedHost.ItemContainerGenerator.ItemsChanged += OnHostItemsChanged;
        SyncFromHost();
    }

    private void DetachFromHost()
    {
        if (_attachedHost == null) return;
        _attachedHost.ActiveIndexChanged -= OnHostActiveIndexChanged;
        _attachedHost.ItemContainerGenerator.ItemsChanged -= OnHostItemsChanged;
        _attachedHost = null;
    }

    private void SyncFromHost()
    {
        if (_attachedHost == null) return;
        _syncingFromHost = true;
        try
        {
            SetCurrentValue(TotalSlidesProperty, _attachedHost.TotalSlides);
            SetCurrentValue(SelectedIndexProperty, _attachedHost.ActiveIndex);
        }
        finally
        {
            _syncingFromHost = false;
        }
    }

    private void OnHostItemsChanged(object sender, ItemsChangedEventArgs e)
    {
        SyncFromHost();
        RebuildButtons();
        UpdateSelection();
    }

    private void OnHostActiveIndexChanged(object sender, RoutedPropertyChangedEventArgs<int> e)
    {
        if (SelectedIndex == e.NewValue) return;
        _syncingFromHost = true;
        try
        {
            SetCurrentValue(SelectedIndexProperty, e.NewValue);
        }
        finally
        {
            _syncingFromHost = false;
        }
        UpdateSelection();
    }

    // --- Buttons --------------------------------------------------------------

    private static void OnTotalSlidesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var nav = (CarouselNav)d;
        nav.RebuildButtons();
        nav.UpdateSelection();
    }

    private static void OnSelectedIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var nav = (CarouselNav)d;
        nav.UpdateSelection();
        if (nav._syncingFromHost) return;
        if (nav._attachedHost != null && nav._attachedHost.ActiveIndex != nav.SelectedIndex)
            nav._attachedHost.SelectPageByIndex(nav.SelectedIndex);
    }

    private static void OnAppearanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((CarouselNav)d).ApplyAppearanceToButtons();
    }

    private void RebuildButtons()
    {
        if (_itemsHost == null) return;

        foreach (UIElement child in _itemsHost.Children)
        {
            if (child is CarouselNavButton button)
                button.Click -= OnNavButtonClick;
        }

        _itemsHost.Children.Clear();
        int count = Math.Max(0, TotalSlides);
        for (int i = 0; i < count; i++)
        {
            var button = new CarouselNavButton
            {
                Index = i,
                Appearance = Appearance,
                ToolTip = $"Page {i + 1}",
            };
            button.Click += OnNavButtonClick;
            _itemsHost.Children.Add(button);
        }
    }

    private void OnNavButtonClick(object sender, RoutedEventArgs e)
    {
        if (sender is CarouselNavButton button)
            SelectIndex(button.Index);
    }

    private void SelectIndex(int index)
    {
        if (index < 0 || index >= TotalSlides) return;
        SelectedIndex = index;
        _attachedHost?.SelectPageByIndex(index);
        _attachedHost?.ResetAutoplay();
    }

    private void UpdateSelection()
    {
        if (_itemsHost == null) return;
        for (int i = 0; i < _itemsHost.Children.Count; i++)
        {
            if (_itemsHost.Children[i] is CarouselNavButton button)
                button.IsSelected = i == SelectedIndex;
        }
    }

    private void ApplyAppearanceToButtons()
    {
        if (_itemsHost == null) return;
        foreach (UIElement child in _itemsHost.Children)
        {
            if (child is CarouselNavButton button)
                button.Appearance = Appearance;
        }
    }
}
