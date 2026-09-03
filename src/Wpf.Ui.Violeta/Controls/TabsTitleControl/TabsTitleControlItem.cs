using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Wpf.Ui.Controls;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// A headered tab item used inside <see cref="TabsTitleControl"/>.
/// Supports an optional <see cref="IconElement"/> icon and close button.
/// </summary>
[TemplatePart(Name = PartCloseButton, Type = typeof(System.Windows.Controls.Button))]
public class TabsTitleControlItem : HeaderedContentControl
{
    private const string PartCloseButton = "PART_CloseButton";

    private System.Windows.Controls.Button? _closeButton;

    /// <summary>Identifies the <see cref="IsSelected"/> dependency property.</summary>
    public static readonly DependencyProperty IsSelectedProperty =
        Selector.IsSelectedProperty.AddOwner(
            typeof(TabsTitleControlItem),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal));

    /// <summary>Gets or sets whether this item is selected.</summary>
    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    /// <summary>Identifies the <see cref="Icon"/> dependency property.</summary>
    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
        nameof(Icon),
        typeof(IconElement),
        typeof(TabsTitleControlItem),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure));

    /// <summary>
    /// Gets or sets the optional icon shown before the header.
    /// Accepts <see cref="IconElement"/> (e.g. <c>SymbolIcon</c>) or a glyph string via type converter.
    /// </summary>
    public IconElement? Icon
    {
        get => (IconElement?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>Identifies the <see cref="IsClosable"/> dependency property.</summary>
    public static readonly DependencyProperty IsClosableProperty = DependencyProperty.Register(
        nameof(IsClosable),
        typeof(bool),
        typeof(TabsTitleControlItem),
        new PropertyMetadata(true));

    /// <summary>Gets or sets whether the close button is shown.</summary>
    public bool IsClosable
    {
        get => (bool)GetValue(IsClosableProperty);
        set => SetValue(IsClosableProperty, value);
    }

    /// <summary>Identifies the <see cref="IsBrand"/> dependency property.</summary>
    public static readonly DependencyProperty IsBrandProperty = DependencyProperty.Register(
        nameof(IsBrand),
        typeof(bool),
        typeof(TabsTitleControlItem),
        new PropertyMetadata(false));

    /// <summary>Gets or sets whether this item is treated as a brand / pinned tab.</summary>
    public bool IsBrand
    {
        get => (bool)GetValue(IsBrandProperty);
        set => SetValue(IsBrandProperty, value);
    }

    /// <summary>Identifies the <see cref="CloseTab"/> routed event.</summary>
    public static readonly RoutedEvent CloseTabEvent = EventManager.RegisterRoutedEvent(
        nameof(CloseTab),
        RoutingStrategy.Bubble,
        typeof(RoutedEventHandler),
        typeof(TabsTitleControlItem));

    /// <summary>Occurs when the user requests to close this tab.</summary>
    public event RoutedEventHandler CloseTab
    {
        add => AddHandler(CloseTabEvent, value);
        remove => RemoveHandler(CloseTabEvent, value);
    }

    static TabsTitleControlItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(TabsTitleControlItem),
            new FrameworkPropertyMetadata(typeof(TabsTitleControlItem)));
    }

    public TabsTitleControlItem()
    {
        Background = Brushes.Transparent;
        BorderThickness = new Thickness(0);
    }

    public override void OnApplyTemplate()
    {
        _closeButton?.Click -= OnCloseButtonClick;

        base.OnApplyTemplate();

        _closeButton = GetTemplateChild(PartCloseButton) as System.Windows.Controls.Button;
        _closeButton?.Click += OnCloseButtonClick;
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);

        if (ItemsControl.ItemsControlFromItemContainer(this) is TabsTitleControl parentTab)
        {
            var index = parentTab.ItemContainerGenerator.IndexFromContainer(this);
            if (index >= 0)
            {
                parentTab.SelectedIndex = index;
            }
        }

        e.Handled = true;
    }

    private void OnCloseButtonClick(object sender, RoutedEventArgs e)
    {
        RaiseEvent(new RoutedEventArgs(CloseTabEvent, this));
        e.Handled = true;
    }
}
