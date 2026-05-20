using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// A multi-select ComboBox that displays selected items as removable <see cref="ClosableTag"/>
/// chips in the header area. Drop-down items are individually checkable without closing the
/// popup. This control is a WPF port of Ursa.Avalonia's <c>MultiComboBox</c>.
/// </summary>
[TemplatePart(Name = PART_HeaderPanel, Type = typeof(Panel))]
[TemplatePart(Name = PART_Placeholder, Type = typeof(UIElement))]
public class TagComboBox : ComboBox
{
    public const string PART_HeaderPanel = "PART_HeaderPanel";
    public const string PART_Placeholder = "PART_Placeholder";

    private Panel? _headerPanel;
    private UIElement? _placeholder;
    private readonly ICommand _removeItemCommand;

    // ── Dependency Properties ──────────────────────────────────────────────

    public static readonly DependencyProperty SelectedItemsProperty =
        DependencyProperty.Register(
            nameof(SelectedItems),
            typeof(ObservableCollection<object>),
            typeof(TagComboBox),
            new PropertyMetadata(null, OnSelectedItemsChanged));

    public static readonly DependencyProperty PlaceholderTextProperty =
        DependencyProperty.Register(
            nameof(PlaceholderText),
            typeof(string),
            typeof(TagComboBox),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty MaxSelectionBoxHeightProperty =
        DependencyProperty.Register(
            nameof(MaxSelectionBoxHeight),
            typeof(double),
            typeof(TagComboBox),
            new PropertyMetadata(120.0));

    public new ObservableCollection<object>? SelectedItems
    {
        get => (ObservableCollection<object>?)GetValue(SelectedItemsProperty);
        set => SetValue(SelectedItemsProperty, value);
    }

    public string PlaceholderText
    {
        get => (string)GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    public double MaxSelectionBoxHeight
    {
        get => (double)GetValue(MaxSelectionBoxHeightProperty);
        set => SetValue(MaxSelectionBoxHeightProperty, value);
    }

    // ── Static ctor & ctor ────────────────────────────────────────────────

    static TagComboBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(TagComboBox),
            new FrameworkPropertyMetadata(typeof(TagComboBox)));
    }

    public TagComboBox()
    {
        SetCurrentValue(SelectedItemsProperty, new ObservableCollection<object>());
        _removeItemCommand = new RemoveItemCommandImpl(this);
    }

    // ── DP change handlers ────────────────────────────────────────────────

    private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TagComboBox box) return;

        if (e.OldValue is INotifyCollectionChanged old)
            old.CollectionChanged -= box.OnSelectedItemsCollectionChanged;
        if (e.NewValue is INotifyCollectionChanged @new)
            @new.CollectionChanged += box.OnSelectedItemsCollectionChanged;

        box.RebuildChips();
        box.CheckPlaceholderVisibility();
    }

    private void OnSelectedItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RebuildChips();
        CheckPlaceholderVisibility();
        SyncDropDownCheckedStates();
    }

    // ── Template ──────────────────────────────────────────────────────────

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _headerPanel = GetTemplateChild(PART_HeaderPanel) as Panel;
        _placeholder = GetTemplateChild(PART_Placeholder) as UIElement;
        RebuildChips();
        CheckPlaceholderVisibility();
    }

    // ── Item container overrides ──────────────────────────────────────────

    protected override bool IsItemItsOwnContainerOverride(object item)
        => item is TagComboBoxItem;

    protected override DependencyObject GetContainerForItemOverride()
        => new TagComboBoxItem();

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);
        if (element is TagComboBoxItem container)
        {
            container.IsCheckedChanged -= OnItemCheckedChanged;
            container.IsCheckedChanged += OnItemCheckedChanged;
            container.IsItemChecked = SelectedItems?.Contains(item) == true;
        }
    }

    protected override void ClearContainerForItemOverride(DependencyObject element, object item)
    {
        base.ClearContainerForItemOverride(element, item);
        if (element is TagComboBoxItem container)
            container.IsCheckedChanged -= OnItemCheckedChanged;
    }

    // ── Item check-change handler ─────────────────────────────────────────

    private void OnItemCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is not TagComboBoxItem item) return;

        object? dataItem = ItemContainerGenerator.ItemFromContainer(item);
        if (dataItem is null || dataItem == DependencyProperty.UnsetValue) return;

        if (item.IsItemChecked)
        {
            if (SelectedItems?.Contains(dataItem) != true)
                SelectedItems?.Add(dataItem);
        }
        else
        {
            SelectedItems?.Remove(dataItem);
        }
    }

    // ── Chip management ───────────────────────────────────────────────────

    private void RebuildChips()
    {
        if (_headerPanel is null) return;
        _headerPanel.Children.Clear();
        if (SelectedItems is null) return;

        foreach (object item in SelectedItems)
        {
            var chip = new ClosableTag
            {
                Content = GetDisplayText(item),
                Command = _removeItemCommand,
                CommandParameter = item,
                Margin = new Thickness(2),
                VerticalAlignment = VerticalAlignment.Center,
            };
            _headerPanel.Children.Add(chip);
        }
    }

    private void CheckPlaceholderVisibility()
    {
        if (_placeholder is null) return;
        _placeholder.Visibility = SelectedItems is null || SelectedItems.Count == 0
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    private void SyncDropDownCheckedStates()
    {
        for (int i = 0; i < Items.Count; i++)
        {
            if (ItemContainerGenerator.ContainerFromIndex(i) is TagComboBoxItem container)
            {
                object? dataItem = ItemContainerGenerator.ItemFromContainer(container);
                if (dataItem is not null && dataItem != DependencyProperty.UnsetValue)
                    container.IsItemChecked = SelectedItems?.Contains(dataItem) == true;
            }
        }
    }

    private string GetDisplayText(object item)
    {
        if (!string.IsNullOrEmpty(DisplayMemberPath))
        {
            var prop = item?.GetType().GetProperty(DisplayMemberPath);
            if (prop is not null)
                return prop.GetValue(item)?.ToString() ?? item?.ToString() ?? string.Empty;
        }
        return item?.ToString() ?? string.Empty;
    }

    // ── ComboBox overrides ────────────────────────────────────────────────

    protected override void OnDropDownClosed(EventArgs e)
    {
        base.OnDropDownClosed(e);
        // Prevent ComboBox base from altering SelectedItem after dropdown closes.
        SelectedItem = null;
    }

    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        // Suppress: multi-select is driven by IsItemChecked, not the base single-select mechanism.
    }

    // ── Remove command (inner) ────────────────────────────────────────────

    private sealed class RemoveItemCommandImpl : ICommand
    {
        private readonly TagComboBox _owner;

        public RemoveItemCommandImpl(TagComboBox owner) => _owner = owner;

        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            if (parameter is null) return;
            _owner.SelectedItems?.Remove(parameter);

            // Also uncheck the corresponding container in the open dropdown.
            var item = _owner.Items.Cast<object>().FirstOrDefault(i => ReferenceEquals(i, parameter));
            if (item is not null &&
                _owner.ItemContainerGenerator.ContainerFromItem(item) is TagComboBoxItem container)
            {
                container.IsItemChecked = false;
            }
        }
    }
}
