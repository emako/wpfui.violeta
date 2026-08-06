using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// A button that opens a flyout of choices. Flyout ContextMenu uses a Violeta style
/// without the upstream slide-from-above animation that covers the button.
/// </summary>
public class DropDownButton : Wpf.Ui.Controls.Button
{
    private const string FlyoutContextMenuStyleKey = "DefaultDropDownFlyoutContextMenuStyle";

    private ContextMenu? _contextMenu;

    static DropDownButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(DropDownButton),
            new FrameworkPropertyMetadata(typeof(DropDownButton)));
    }

    public DropDownButton()
    {
        PreviewMouseLeftButtonUp += OnPreviewMouseLeftButtonUp;
    }

    /// <summary>Identifies the <see cref="Flyout"/> dependency property.</summary>
    public static readonly DependencyProperty FlyoutProperty = DependencyProperty.Register(
        nameof(Flyout),
        typeof(object),
        typeof(DropDownButton),
        new PropertyMetadata(null, OnFlyoutChanged));

    /// <summary>Identifies the <see cref="IsDropDownOpen"/> dependency property.</summary>
    public static readonly DependencyProperty IsDropDownOpenProperty = DependencyProperty.Register(
        nameof(IsDropDownOpen),
        typeof(bool),
        typeof(DropDownButton),
        new PropertyMetadata(false));

    /// <summary>Gets or sets the flyout associated with this button.</summary>
    [Bindable(true)]
    public object? Flyout
    {
        get => GetValue(FlyoutProperty);
        set => SetValue(FlyoutProperty, value);
    }

    /// <summary>Gets or sets a value indicating whether the drop-down is currently open.</summary>
    [Bindable(true)]
    [Browsable(false)]
    [Category("Appearance")]
    public bool IsDropDownOpen
    {
        get => (bool)GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }

    private static void OnFlyoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((DropDownButton)d).OnFlyoutChanged(e.OldValue, e.NewValue);
    }

    /// <summary>Invoked when <see cref="Flyout"/> changes.</summary>
    protected virtual void OnFlyoutChanged(object? oldValue, object? newValue)
    {
        if (oldValue is ContextMenu oldMenu)
        {
            oldMenu.Opened -= OnContextMenuOpened;
            oldMenu.Closed -= OnContextMenuClosed;
        }

        _contextMenu = null;

        if (newValue is ContextMenu contextMenu)
        {
            _contextMenu = contextMenu;
            ApplyFlyoutContextMenuStyle(contextMenu);
            contextMenu.Opened += OnContextMenuOpened;
            contextMenu.Closed += OnContextMenuClosed;
        }
    }

    protected virtual void OnContextMenuClosed(object sender, RoutedEventArgs e)
    {
        SetCurrentValue(IsDropDownOpenProperty, false);
    }

    protected virtual void OnContextMenuOpened(object sender, RoutedEventArgs e)
    {
        SetCurrentValue(IsDropDownOpenProperty, true);
    }

    private void OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (_contextMenu is null)
        {
            return;
        }

        _contextMenu.SetCurrentValue(MinWidthProperty, ActualWidth);
        _contextMenu.SetCurrentValue(ContextMenu.PlacementTargetProperty, this);
        _contextMenu.SetCurrentValue(ContextMenu.PlacementProperty, PlacementMode.Bottom);
        _contextMenu.SetCurrentValue(ContextMenu.IsOpenProperty, true);
    }

    private void ApplyFlyoutContextMenuStyle(ContextMenu contextMenu)
    {
        if (contextMenu.ReadLocalValue(StyleProperty) != DependencyProperty.UnsetValue)
        {
            return;
        }

        var style =
            TryFindResource(FlyoutContextMenuStyleKey) as Style
            ?? Application.Current?.TryFindResource(FlyoutContextMenuStyleKey) as Style;

        if (style is not null)
        {
            contextMenu.Style = style;
        }
    }
}
