using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// A single navigation item inside an <see cref="Anchor"/> control.
/// </summary>
[TemplatePart(Name = PART_Header, Type = typeof(ContentPresenter))]
public class AnchorItem : HeaderedItemsControl
{
    public const string PART_Header = nameof(PART_Header);

    // ---- Dependency Properties -----------------------------------------------

    public static readonly DependencyProperty AnchorIdProperty =
        DependencyProperty.Register(nameof(AnchorId), typeof(string), typeof(AnchorItem),
            new PropertyMetadata(null));

    /// <summary>Gets or sets the id that corresponds to a marked element inside the target ScrollViewer.</summary>
    public string? AnchorId
    {
        get => (string?)GetValue(AnchorIdProperty);
        set => SetValue(AnchorIdProperty, value);
    }

    public static readonly DependencyProperty IsSelectedProperty =
        DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(AnchorItem),
            new PropertyMetadata(false, OnIsSelectedChanged));

    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    internal static readonly DependencyProperty LevelProperty =
        DependencyProperty.Register(nameof(Level), typeof(int), typeof(AnchorItem),
            new PropertyMetadata(0));

    /// <summary>Nesting depth (0 = root), used for left-margin indentation.</summary>
    public int Level
    {
        get => (int)GetValue(LevelProperty);
        internal set => SetValue(LevelProperty, value);
    }

    // ---- Static constructor --------------------------------------------------

    static AnchorItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(AnchorItem),
            new FrameworkPropertyMetadata(typeof(AnchorItem)));

        // AnchorItems use a StackPanel for sub-items by default.
        ItemsPanelProperty.OverrideMetadata(typeof(AnchorItem), new FrameworkPropertyMetadata(
            new ItemsPanelTemplate(new FrameworkElementFactory(typeof(StackPanel)))));
    }

    // ---- Overrides -----------------------------------------------------------

    protected override DependencyObject GetContainerForItemOverride() => new AnchorItem();

    protected override bool IsItemItsOwnContainerOverride(object item) => item is AnchorItem;

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);

        if (element is AnchorItem child)
        {
            // Inherit ItemTemplate / ItemContainerStyle from the root Anchor.
            var root = FindAnchor();
            if (root is not null)
            {
                if (child.ItemTemplate is null && root.ItemTemplate is not null)
                    child.SetCurrentValue(ItemTemplateProperty, root.ItemTemplate);
                if (child.ItemContainerStyle is null && root.ItemContainerStyle is not null)
                    child.SetCurrentValue(ItemContainerStyleProperty, root.ItemContainerStyle);
            }

            // Level = parent level + 1
            child.Level = Level + 1;
        }
    }

    protected override void OnVisualParentChanged(DependencyObject oldParent)
    {
        base.OnVisualParentChanged(oldParent);
        // Recalculate depth whenever the item is attached.
        Level = CalculateLevel();
    }

    // ---- Helpers -------------------------------------------------------------

    private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((AnchorItem)d).UpdateVisualState();
    }

    private void UpdateVisualState()
    {
        VisualStateManager.GoToState(this, IsSelected ? "Selected" : "Normal", true);
    }

    private Anchor? FindAnchor()
    {
        DependencyObject? current = VisualParent;
        while (current is not null)
        {
            if (current is Anchor a)
                return a;
            current = LogicalTreeHelper.GetParent(current) ??
                      (current is Visual v ? System.Windows.Media.VisualTreeHelper.GetParent(v) : null);
        }
        return null;
    }

    private int CalculateLevel()
    {
        int level = 0;
        DependencyObject? current = LogicalTreeHelper.GetParent(this);
        while (current is not null)
        {
            if (current is Anchor)
                break;
            if (current is AnchorItem)
                level++;
            current = LogicalTreeHelper.GetParent(current);
        }
        return level;
    }
}
