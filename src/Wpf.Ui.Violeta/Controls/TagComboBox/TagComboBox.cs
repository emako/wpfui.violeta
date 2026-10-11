using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Wpf.Ui.Violeta.Resources.Localization;

#pragma warning disable CS0067

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// A multi-select ComboBox that displays selected items as removable <see cref="ClosableTag"/>
/// chips in the header area. Drop-down items are individually checkable without closing the
/// popup. This control is a WPF port of Ursa.Avalonia's <c>MultiComboBox</c>.
/// </summary>
[TemplatePart(Name = PART_HeaderPanel, Type = typeof(Panel))]
[TemplatePart(Name = PART_Placeholder, Type = typeof(UIElement))]
[TemplatePart(Name = PART_HeaderScrollViewer, Type = typeof(ScrollViewer))]
public class TagComboBox : ComboBox
{
    public const string PART_HeaderPanel = "PART_HeaderPanel";
    public const string PART_Placeholder = "PART_Placeholder";
    public const string PART_HeaderScrollViewer = "PART_HeaderScrollViewer";

    private Panel? _headerPanel;
    private UIElement? _placeholder;
    private ScrollViewer? _headerScrollViewer;
    private readonly ICommand _removeItemCommand;
    private readonly List<object> _publishedSelection = [];

    // -- Dependency Properties ----------------------------------------------

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

    public static readonly DependencyProperty SelectedItemTemplateProperty =
        DependencyProperty.Register(
            nameof(SelectedItemTemplate),
            typeof(DataTemplate),
            typeof(TagComboBox),
            new PropertyMetadata(null, OnSelectedItemTemplateChanged));

    public ObservableCollection<object>? SelectedItems
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

    /// <summary>
    /// Template used to display selected items as chips.
    /// When null, <see cref="ItemsControl.ItemTemplate"/> is used as a fallback.
    /// </summary>
    public DataTemplate? SelectedItemTemplate
    {
        get => (DataTemplate?)GetValue(SelectedItemTemplateProperty);
        set => SetValue(SelectedItemTemplateProperty, value);
    }

    // -- Static ctor & ctor ------------------------------------------------

    static TagComboBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(TagComboBox),
            new FrameworkPropertyMetadata(typeof(TagComboBox)));
    }

    public TagComboBox()
    {
        _removeItemCommand = new RemoveItemCommandImpl(this);
        SetCurrentValue(SelectedItemsProperty, new ObservableCollection<object>());

        if (ReadLocalValue(PlaceholderTextProperty) == DependencyProperty.UnsetValue)
        {
            SetCurrentValue(PlaceholderTextProperty, SH.PleaseSelect);
        }
    }

    // -- DP change handlers ------------------------------------------------

    private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TagComboBox box) return;

        if (e.OldValue is INotifyCollectionChanged old)
            old.CollectionChanged -= box.OnSelectedItemsCollectionChanged;
        if (e.NewValue is INotifyCollectionChanged @new)
            @new.CollectionChanged += box.OnSelectedItemsCollectionChanged;

        box.RebuildChips();
        box.CheckPlaceholderVisibility();
        box.PublishSelectionChanged();
    }

    private static void OnSelectedItemTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TagComboBox box)
            box.RebuildChips();
    }

    private void OnSelectedItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RebuildChips();
        CheckPlaceholderVisibility();
        SyncDropDownCheckedStates();
        PublishSelectionChanged();
    }

    // -- Template ----------------------------------------------------------

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _headerPanel = GetTemplateChild(PART_HeaderPanel) as Panel;
        _placeholder = GetTemplateChild(PART_Placeholder) as UIElement;
        _headerScrollViewer = GetTemplateChild(PART_HeaderScrollViewer) as ScrollViewer;
        RebuildChips();
        CheckPlaceholderVisibility();
    }

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.Property == ItemTemplateProperty || e.Property == DisplayMemberPathProperty)
            RebuildChips();
    }

    // -- Item container overrides ------------------------------------------

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

    // -- Item check-change handler -----------------------------------------

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

    // -- Chip management ---------------------------------------------------

    private void RebuildChips()
    {
        if (_headerPanel is null) return;
        _headerPanel.Children.Clear();
        if (SelectedItems is null) return;

        var template = SelectedItemTemplate ?? ItemTemplate;

        foreach (object item in SelectedItems)
        {
            var chip = new ClosableTag
            {
                Command = _removeItemCommand,
                CommandParameter = item,
                Margin = new Thickness(2),
                VerticalAlignment = VerticalAlignment.Center,
            };

            if (template is not null)
            {
                chip.Content = ResolveSelectedContent(item);
                chip.ContentTemplate = template;
            }
            else
            {
                chip.Content = GetDisplayText(item);
            }

            _headerPanel.Children.Add(chip);
        }
    }

    private static object? ResolveSelectedContent(object item) =>
        item is TagComboBoxItem container ? container.Content : item;

    private void CheckPlaceholderVisibility()
    {
        bool isEmpty = SelectedItems is null || SelectedItems.Count == 0;

        if (_placeholder is not null)
        {
            _placeholder.Visibility = isEmpty ? Visibility.Visible : Visibility.Collapsed;
        }

        if (_headerScrollViewer is not null)
        {
            _headerScrollViewer.Visibility = isEmpty ? Visibility.Collapsed : Visibility.Visible;
        }
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
        if (item is TagComboBoxItem container)
            return container.Content?.ToString() ?? string.Empty;

        if (!string.IsNullOrEmpty(DisplayMemberPath))
        {
            var prop = item?.GetType().GetProperty(DisplayMemberPath);
            if (prop is not null)
                return prop.GetValue(item)?.ToString() ?? item?.ToString() ?? string.Empty;
        }

        return item?.ToString() ?? string.Empty;
    }

    private void PublishSelectionChanged()
    {
        var current = SelectedItems?.ToList() ?? [];
        var previous = _publishedSelection.ToList();
        _publishedSelection.Clear();
        _publishedSelection.AddRange(current);

        var removed = new List<object>();
        var added = new List<object>();

        foreach (var item in previous)
        {
            if (!current.Contains(item))
                removed.Add(item);
        }

        foreach (var item in current)
        {
            if (!previous.Contains(item))
                added.Add(item);
        }

        if (removed.Count == 0 && added.Count == 0)
            return;

        RaiseEvent(new SelectionChangedEventArgs(SelectionChangedEvent, removed, added));
    }

    // -- ComboBox overrides ------------------------------------------------

    protected override void OnDropDownClosed(EventArgs e)
    {
        base.OnDropDownClosed(e);
        // Prevent ComboBox base from altering SelectedItem after dropdown closes.
        SelectedItem = null;
    }

    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        // ComboBox.OnSelectionChanged rewrites the header from SelectedItem.
        // Multi-select notifications are raised from SelectedItems instead.
    }

    // -- Remove command (inner) --------------------------------------------

    private sealed class RemoveItemCommandImpl(TagComboBox owner) : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            if (parameter is null) return;
            owner.SelectedItems?.Remove(parameter);

            // Also uncheck the corresponding container in the open dropdown.
            var item = owner.Items.Cast<object>().FirstOrDefault(i => ReferenceEquals(i, parameter));
            if (item is not null &&
                owner.ItemContainerGenerator.ContainerFromItem(item) is TagComboBoxItem container)
            {
                container.IsItemChecked = false;
            }
        }
    }
}
