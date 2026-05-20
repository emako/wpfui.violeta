using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// An anchor navigation control that mirrors a list of section headings inside a
/// <see cref="ScrollViewer"/>.  Clicking an item smooth-scrolls the viewer to the
/// corresponding section; scrolling the viewer highlights the visible item.
/// </summary>
/// <remarks>
/// Mark target elements with the <c>Anchor.Id</c> attached property:
/// <code>
///     &lt;TextBlock vio:Anchor.Id="section1" Text="Section 1" /&gt;
/// </code>
/// Then point the <see cref="Anchor"/> at the <see cref="TargetContainer"/>:
/// <code>
///     &lt;vio:Anchor TargetContainer="{Binding ElementName=MyScrollViewer}"&gt;
///         &lt;vio:AnchorItem Header="Section 1" AnchorId="section1" /&gt;
///     &lt;/vio:Anchor&gt;
/// </code>
/// </remarks>
public class Anchor : ItemsControl
{
    // ---- Attached property: Id -----------------------------------------------

    public static readonly DependencyProperty IdProperty =
        DependencyProperty.RegisterAttached("Id", typeof(string), typeof(Anchor),
            new FrameworkPropertyMetadata(null));

    public static void SetId(DependencyObject obj, string? value) => obj.SetValue(IdProperty, value);

    public static string? GetId(DependencyObject obj) => (string?)obj.GetValue(IdProperty);

    // ---- Dependency Properties -----------------------------------------------

    public static readonly DependencyProperty TargetContainerProperty =
        DependencyProperty.Register(nameof(TargetContainer), typeof(ScrollViewer), typeof(Anchor),
            new PropertyMetadata(null, OnTargetContainerChanged));

    /// <summary>The <see cref="ScrollViewer"/> whose scroll position drives selection.</summary>
    public ScrollViewer? TargetContainer
    {
        get => (ScrollViewer?)GetValue(TargetContainerProperty);
        set => SetValue(TargetContainerProperty, value);
    }

    public static readonly DependencyProperty TopOffsetProperty =
        DependencyProperty.Register(nameof(TopOffset), typeof(double), typeof(Anchor),
            new PropertyMetadata(0d));

    /// <summary>Pixels from the top of the viewport used to decide which anchor is "active".</summary>
    public double TopOffset
    {
        get => (double)GetValue(TopOffsetProperty);
        set => SetValue(TopOffsetProperty, value);
    }

    // ---- Private state -------------------------------------------------------

    private List<(string Id, double Position)> _positions = [];
    private AnchorItem? _selectedContainer;
    private bool _scrollingFromSelection;

    // ---- Static constructor --------------------------------------------------

    static Anchor()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Anchor),
            new FrameworkPropertyMetadata(typeof(Anchor)));

        ItemsPanelProperty.OverrideMetadata(typeof(Anchor), new FrameworkPropertyMetadata(
            new ItemsPanelTemplate(new FrameworkElementFactory(typeof(StackPanel)))));
    }

    // ---- ItemsControl overrides ---------------------------------------------

    protected override DependencyObject GetContainerForItemOverride() => new AnchorItem();

    protected override bool IsItemItsOwnContainerOverride(object item) => item is AnchorItem;

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);
        if (element is AnchorItem ai)
            ai.Level = 0;
    }

    // ---- Loaded / Unloaded --------------------------------------------------

    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);
        Loaded += OnAnchorLoaded;
        Unloaded += OnAnchorUnloaded;
    }

    private void OnAnchorLoaded(object sender, RoutedEventArgs e)
    {
        AttachScrollViewer(TargetContainer);
        InvalidateAnchorPositions();
        MarkSelectedContainerByPosition();
    }

    private void OnAnchorUnloaded(object sender, RoutedEventArgs e)
    {
        DetachScrollViewer(TargetContainer);
    }

    // ---- Target container changed -------------------------------------------

    private static void OnTargetContainerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var anchor = (Anchor)d;
        anchor.DetachScrollViewer(e.OldValue as ScrollViewer);
        anchor.AttachScrollViewer(e.NewValue as ScrollViewer);
        anchor.InvalidateAnchorPositions();
        anchor.MarkSelectedContainerByPosition();
    }

    private void AttachScrollViewer(ScrollViewer? sv)
    {
        if (sv is null) return;
        sv.ScrollChanged += OnScrollChanged;
        sv.Loaded += OnTargetContainerLoaded;
    }

    private void DetachScrollViewer(ScrollViewer? sv)
    {
        if (sv is null) return;
        sv.ScrollChanged -= OnScrollChanged;
        sv.Loaded -= OnTargetContainerLoaded;
    }

    private void OnTargetContainerLoaded(object sender, RoutedEventArgs e)
    {
        InvalidateAnchorPositions();
        MarkSelectedContainerByPosition();
    }

    // ---- Scroll event --------------------------------------------------------

    private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        if (_scrollingFromSelection) return;
        MarkSelectedContainerByPosition();
    }

    // ---- Mouse interaction ---------------------------------------------------

    protected override void OnMouseLeftButtonUp(System.Windows.Input.MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);

        // Walk up the hit-tested element tree to find the AnchorItem container.
        var source = e.OriginalSource as DependencyObject;
        AnchorItem? item = null;
        while (source is not null)
        {
            if (source is AnchorItem ai)
            {
                item = ai;
                break;
            }
            source = VisualTreeHelper.GetParent(source) ?? LogicalTreeHelper.GetParent(source);
        }

        if (item is null) return;
        MarkSelectedContainer(item);

        if (TargetContainer is not null && item.AnchorId is not null)
        {
            var target = FindElementById(TargetContainer, item.AnchorId);
            if (target is not null)
                ScrollToAnchor(target);
        }
    }

    // ---- Position tracking --------------------------------------------------

    /// <summary>
    /// Re-measures the vertical positions of all Id-marked elements inside the
    /// <see cref="TargetContainer"/>.  Call this after the target content changes.
    /// </summary>
    public void InvalidatePositions()
    {
        InvalidateAnchorPositions();
        MarkSelectedContainerByPosition();
    }

    internal void InvalidateAnchorPositions()
    {
        if (TargetContainer is null) return;

        var results = new List<(string, double)>();
        CollectPositions(TargetContainer, TargetContainer, results);
        results.Sort((a, b) => a.Item2.CompareTo(b.Item2));
        _positions = results;
    }

    private static void CollectPositions(Visual root, Visual container, List<(string, double)> results)
    {
        int count = VisualTreeHelper.GetChildrenCount(root);
        for (int i = 0; i < count; i++)
        {
            if (VisualTreeHelper.GetChild(root, i) is not Visual child) continue;

            if (child is DependencyObject dep)
            {
                var id = GetId(dep);
                if (id is not null)
                {
                    var pt = child.TransformToAncestor(container).Transform(new Point(0, 0));
                    results.Add((id, pt.Y));
                }
            }
            CollectPositions(child, container, results);
        }
    }

    private void MarkSelectedContainerByPosition()
    {
        if (TargetContainer is null || _positions.Count == 0) return;

        double top = TargetContainer.VerticalOffset + TopOffset;

        // Pick the last anchor whose position is <= current scroll top.
        string? activeId = null;
        foreach (var (id, pos) in _positions)
        {
            if (pos <= top)
                activeId = id;
            else
                break;
        }

        activeId ??= _positions[0].Id;

        var container = FindAnchorItemById(this, activeId);
        MarkSelectedContainer(container);
    }

    // ---- Selection management -----------------------------------------------

    internal void MarkSelectedContainer(AnchorItem? item)
    {
        if (_selectedContainer == item) return;
        _selectedContainer?.IsSelected = false;
        _selectedContainer = item;
        _selectedContainer?.IsSelected = true;
    }

    // ---- Smooth scroll -------------------------------------------------------

    private void ScrollToAnchor(Visual target)
    {
        if (TargetContainer is null) return;

        var pt = target.TransformToAncestor(TargetContainer).Transform(new Point(0, 0));
        double from = TargetContainer.VerticalOffset;
        double to = from + pt.Y - TopOffset;
        double maxOffset = TargetContainer.ScrollableHeight;
        if (to > maxOffset) to = maxOffset;
        if (to < 0) to = 0;

        if (Math.Abs(from - to) < 0.5) return;

        _scrollingFromSelection = true;

        var animation = new DoubleAnimation
        {
            From = from,
            To = to,
            Duration = TimeSpan.FromMilliseconds(300),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut },
            FillBehavior = FillBehavior.Stop,
        };
        animation.Completed += (_, _) =>
        {
            TargetContainer.ScrollToVerticalOffset(to);
            _scrollingFromSelection = false;
        };

        // Animate via a helper AnimatableDouble on the ScrollViewer.
        var proxy = new ScrollProxy(TargetContainer);
        proxy.Animate(from, to, animation);
    }

    // ---- Tree helpers --------------------------------------------------------

    private static AnchorItem? FindAnchorItemById(ItemsControl parent, string id)
    {
        foreach (object item in parent.Items)
        {
            if (parent.ItemContainerGenerator.ContainerFromItem(item) is not AnchorItem container) continue;
            if (container.AnchorId == id)
                return container;
            var nested = FindAnchorItemById(container, id);
            if (nested is not null)
                return nested;
        }
        return null;
    }

    private static Visual? FindElementById(Visual root, string id)
    {
        if (root is DependencyObject dep && GetId(dep) == id)
            return root;

        int count = VisualTreeHelper.GetChildrenCount(root);
        for (int i = 0; i < count; i++)
        {
            if (VisualTreeHelper.GetChild(root, i) is Visual child)
            {
                var found = FindElementById(child, id);
                if (found is not null)
                    return found;
            }
        }
        return null;
    }

    // ---- Scroll animation proxy ---------------------------------------------

    /// <summary>Tiny helper that drives a <see cref="ScrollViewer"/> offset via a <see cref="DoubleAnimation"/>.</summary>
    private sealed class ScrollProxy : Animatable
    {
        private readonly ScrollViewer _sv;

        public static readonly DependencyProperty OffsetProperty =
            DependencyProperty.Register("Offset", typeof(double), typeof(ScrollProxy),
                new PropertyMetadata(0d, OnOffsetChanged));

        private static void OnOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            => ((ScrollProxy)d)._sv.ScrollToVerticalOffset((double)e.NewValue);

        protected override Freezable CreateInstanceCore() => new ScrollProxy(_sv);

        public ScrollProxy(ScrollViewer sv)
        {
            _sv = sv;
            SetValue(OffsetProperty, sv.VerticalOffset);
        }

        public void Animate(double from, double to, DoubleAnimation animation)
        {
            animation.From = from;
            animation.To = to;
            BeginAnimation(OffsetProperty, animation);
        }
    }
}
