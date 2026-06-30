using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// A combo-box that shows its items in a hierarchical (vertical) tree popup.
/// Mirrors the logic of Ursa.Avalonia's TreeComboBox.
/// </summary>
[TemplatePart(Name = PART_Popup, Type = typeof(Popup))]
public class TreeComboBox : ItemsControl
{
    public const string PART_Popup = "PART_Popup";

    private Popup? _popup;
    private Window? _parentWindow;
    private bool _windowHandlerRegistered;

    #region Dependency Properties

    public static readonly DependencyProperty MaxDropDownHeightProperty =
        DependencyProperty.Register(
            nameof(MaxDropDownHeight), typeof(double), typeof(TreeComboBox),
            new PropertyMetadata(400.0));

    public double MaxDropDownHeight
    {
        get => (double)GetValue(MaxDropDownHeightProperty);
        set => SetValue(MaxDropDownHeightProperty, value);
    }

    public static readonly DependencyProperty PlaceholderTextProperty =
        DependencyProperty.Register(
            nameof(PlaceholderText), typeof(string), typeof(TreeComboBox),
            new PropertyMetadata(string.Empty));

    public string? PlaceholderText
    {
        get => (string?)GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    public static readonly DependencyProperty PlaceholderForegroundProperty =
        DependencyProperty.Register(
            nameof(PlaceholderForeground), typeof(Brush), typeof(TreeComboBox),
            new PropertyMetadata(null));

    public Brush? PlaceholderForeground
    {
        get => (Brush?)GetValue(PlaceholderForegroundProperty);
        set => SetValue(PlaceholderForegroundProperty, value);
    }

    public static readonly DependencyProperty IsDropDownOpenProperty =
        DependencyProperty.Register(
            nameof(IsDropDownOpen), typeof(bool), typeof(TreeComboBox),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnIsDropDownOpenChanged));

    public bool IsDropDownOpen
    {
        get => (bool)GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }

    public static readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.Register(
            nameof(SelectedItem), typeof(object), typeof(TreeComboBox),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedItemChanged));

    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    private static readonly DependencyPropertyKey SelectionBoxItemPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(SelectionBoxItem), typeof(object), typeof(TreeComboBox),
            new PropertyMetadata(null));

    public static readonly DependencyProperty SelectionBoxItemProperty = SelectionBoxItemPropertyKey.DependencyProperty;

    /// <summary>
    /// The item (or header) currently displayed in the closed/collapsed header area.
    /// </summary>
    public object? SelectionBoxItem
    {
        get => GetValue(SelectionBoxItemProperty);
        private set => SetValue(SelectionBoxItemPropertyKey, value);
    }

    #endregion Dependency Properties

    static TreeComboBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(TreeComboBox),
            new FrameworkPropertyMetadata(typeof(TreeComboBox)));
    }

    public TreeComboBox()
    {
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _popup = GetTemplateChild(PART_Popup) as Popup;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        _parentWindow = Window.GetWindow(this);
    }

    private void OnUnloaded(object? sender, RoutedEventArgs e)
    {
        UnregisterWindowHandler();
    }

    private void RegisterWindowHandler()
    {
        _parentWindow ??= Window.GetWindow(this);

        if (_parentWindow is not null && !_windowHandlerRegistered)
        {
            _parentWindow.PreviewMouseLeftButtonDown += OnWindowPreviewMouseLeftButtonDown;
            _windowHandlerRegistered = true;
        }
    }

    private void UnregisterWindowHandler()
    {
        if (_parentWindow is not null && _windowHandlerRegistered)
        {
            _parentWindow.PreviewMouseLeftButtonDown -= OnWindowPreviewMouseLeftButtonDown;
            _windowHandlerRegistered = false;
        }
    }

    private static void OnIsDropDownOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var combo = (TreeComboBox)d;
        if ((bool)e.NewValue)
            combo.RegisterWindowHandler();
        else
            combo.UnregisterWindowHandler();
    }

    /// <summary>
    /// Window-level preview handler for light-dismiss: closes the popup when
    /// the user clicks outside both the header and the popup content.
    /// </summary>
    private void OnWindowPreviewMouseLeftButtonDown(object? sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is not DependencyObject target)
            return;

        // Click inside our own header area → let the ToggleButton handle toggle
        if (IsVisualDescendantOf(target, this))
            return;

        // Click inside popup content → items handle it themselves
        if (_popup?.Child is UIElement popupChild && IsVisualDescendantOf(target, popupChild))
            return;

        // Click is outside → close
        SetCurrentValue(IsDropDownOpenProperty, false);
    }

    private static bool IsVisualDescendantOf(DependencyObject element, DependencyObject ancestor)
    {
        DependencyObject? current = element;
        while (current is not null)
        {
            if (current == ancestor) return true;
            current = VisualTreeHelper.GetParent(current);
        }
        return false;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Handled) return;

        // F4 or Alt+Down/Up toggles dropdown
        if ((e.Key == Key.F4 && !Keyboard.Modifiers.HasFlag(ModifierKeys.Alt)) ||
            ((e.Key == Key.Down || e.Key == Key.Up) && Keyboard.Modifiers.HasFlag(ModifierKeys.Alt)))
        {
            SetCurrentValue(IsDropDownOpenProperty, !IsDropDownOpen);
            e.Handled = true;
        }
        else if (IsDropDownOpen && e.Key == Key.Escape)
        {
            SetCurrentValue(IsDropDownOpenProperty, false);
            e.Handled = true;
        }
        else if (!IsDropDownOpen && (e.Key == Key.Return || e.Key == Key.Space))
        {
            SetCurrentValue(IsDropDownOpenProperty, true);
            e.Handled = true;
        }
        else if (IsDropDownOpen && e.Key == Key.Tab)
        {
            SetCurrentValue(IsDropDownOpenProperty, false);
            e.Handled = true;
        }
    }

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is TreeComboBoxItem;
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new TreeComboBoxItem();
    }

    internal TreeComboBoxItem CreateContainerForItemInternal()
    {
        return new TreeComboBoxItem();
    }

    /// <summary>
    /// Called by a <see cref="TreeComboBoxItem"/> when it is clicked.
    /// Selects the corresponding data item and closes the popup.
    /// </summary>
    internal void NotifyItemClicked(TreeComboBoxItem container)
    {
        var dataItem = FindItemFromContainer(this, container);
        SelectedItem = dataItem ?? container;
        SetCurrentValue(IsDropDownOpenProperty, false);
    }

    /// <summary>Clears the current selection.</summary>
    public void Clear()
    {
        SelectedItem = null;
    }

    private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((TreeComboBox)d).HandleSelectedItemChanged(e.OldValue, e.NewValue);
    }

    private void HandleSelectedItemChanged(object? oldValue, object? newValue)
    {
        MarkContainerSelection(oldValue, false);
        MarkContainerSelection(newValue, true);
        UpdateSelectionBoxItem(newValue);
    }

    private void MarkContainerSelection(object? item, bool selected)
    {
        if (item is null) return;

        TreeComboBoxItem? container = item as TreeComboBoxItem
            ?? FindContainerFromItem(this, item) as TreeComboBoxItem;

        container?.IsSelected = selected;
    }

    private void UpdateSelectionBoxItem(object? item)
    {
        if (item is null)
        {
            SelectionBoxItem = null;
            return;
        }

        // For declarative TreeComboBoxItem usage, expose its Header as the display value
        if (item is TreeComboBoxItem treeItem)
        {
            SelectionBoxItem = treeItem.Header ?? item;
        }
        else
        {
            // For data-bound usage, expose the data object itself (rendered via ItemTemplate)
            SelectionBoxItem = item;
        }
    }

    /// <summary>
    /// Recursively searches the generated container tree to find the container for <paramref name="item"/>.
    /// </summary>
    private static DependencyObject? FindContainerFromItem(ItemsControl itemsControl, object item)
    {
        var container = itemsControl.ItemContainerGenerator.ContainerFromItem(item);
        if (container is not null && !ReferenceEquals(container, DependencyProperty.UnsetValue))
            return container;

        foreach (object childItem in itemsControl.Items)
        {
            var childContainer = itemsControl.ItemContainerGenerator.ContainerFromItem(childItem);
            if (childContainer is ItemsControl nestedControl)
            {
                var found = FindContainerFromItem(nestedControl, item);
                if (found is not null)
                    return found;
            }
        }
        return null;
    }

    /// <summary>
    /// Recursively searches the generated container tree to find the data item for <paramref name="container"/>.
    /// </summary>
    private static object? FindItemFromContainer(ItemsControl itemsControl, DependencyObject container)
    {
        var item = itemsControl.ItemContainerGenerator.ItemFromContainer(container);
        if (item != DependencyProperty.UnsetValue && item is not null)
            return item;

        foreach (object childItem in itemsControl.Items)
        {
            var childContainer = itemsControl.ItemContainerGenerator.ContainerFromItem(childItem);
            if (childContainer is ItemsControl nestedControl)
            {
                var found = FindItemFromContainer(nestedControl, container);
                if (found is not null)
                    return found;
            }
        }
        return null;
    }
}
