using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Violeta.Resources.Localization;

namespace Wpf.Ui.Violeta.Controls;

[TemplatePart(Name = nameof(PART_SelectAllCheckBox), Type = typeof(CheckBox))]
[TemplatePart(Name = nameof(PART_ItemsPresenter), Type = typeof(ItemsPresenter))]
[TemplatePart(Name = nameof(PART_SelectedText), Type = typeof(TextBlock))]
public class MultiComboBox : ComboBox
{
    private CheckBox? PART_SelectAllCheckBox;
    private ItemsPresenter? PART_ItemsPresenter;
    private TextBlock? PART_SelectedText;

    private bool _suppressSelectAllUpdate = false;
    private bool _suppressItemUpdate = false;

    public static readonly DependencyProperty MultiSelectedItemsProperty =
        DependencyProperty.Register(
            nameof(MultiSelectedItems),
            typeof(ObservableCollection<object>),
            typeof(MultiComboBox),
            new PropertyMetadata(null));

    public static readonly DependencyProperty SelectAllCheckStateProperty =
        DependencyProperty.Register(
            nameof(SelectAllCheckState),
            typeof(bool?),
            typeof(MultiComboBox),
            new PropertyMetadata(false));

    public static readonly DependencyProperty SeparatorProperty =
        DependencyProperty.Register(
            nameof(Separator),
            typeof(string),
            typeof(MultiComboBox),
            new PropertyMetadata(", "));

    public static readonly DependencyProperty PlaceholderTextProperty =
        DependencyProperty.Register(
            nameof(PlaceholderText),
            typeof(string),
            typeof(MultiComboBox),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty SelectAllTextProperty =
        DependencyProperty.Register(
            nameof(SelectAllText),
            typeof(string),
            typeof(MultiComboBox),
            new PropertyMetadata(SH.MultiComboBoxSelectAll));

    public static readonly DependencyProperty IsSelectAllEnabledProperty =
        DependencyProperty.Register(
            nameof(IsSelectAllEnabled),
            typeof(bool),
            typeof(MultiComboBox),
            new PropertyMetadata(true));

    public static readonly DependencyProperty ShowAllSelectedTextProperty =
        DependencyProperty.Register(
            nameof(ShowAllSelectedText),
            typeof(bool),
            typeof(MultiComboBox),
            new PropertyMetadata(true));

    public static readonly DependencyProperty AllSelectedTextProperty =
        DependencyProperty.Register(
            nameof(AllSelectedText),
            typeof(string),
            typeof(MultiComboBox),
            new PropertyMetadata(null));

    public ObservableCollection<object> MultiSelectedItems
    {
        get => (ObservableCollection<object>)GetValue(MultiSelectedItemsProperty);
        private set => SetValue(MultiSelectedItemsProperty, value);
    }

    public bool? SelectAllCheckState
    {
        get => (bool?)GetValue(SelectAllCheckStateProperty);
        set => SetValue(SelectAllCheckStateProperty, value);
    }

    public string Separator
    {
        get => (string)GetValue(SeparatorProperty);
        set => SetValue(SeparatorProperty, value);
    }

    public string PlaceholderText
    {
        get => (string)GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    public string SelectAllText
    {
        get => (string)GetValue(SelectAllTextProperty);
        set => SetValue(SelectAllTextProperty, value);
    }

    public bool IsSelectAllEnabled
    {
        get => (bool)GetValue(IsSelectAllEnabledProperty);
        set => SetValue(IsSelectAllEnabledProperty, value);
    }

    public bool ShowAllSelectedText

    {
        get => (bool)GetValue(ShowAllSelectedTextProperty);
        set => SetValue(ShowAllSelectedTextProperty, value);
    }

    public string? AllSelectedText
    {
        get => (string?)GetValue(AllSelectedTextProperty);
        set => SetValue(AllSelectedTextProperty, value);
    }

    static MultiComboBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(MultiComboBox),
            new FrameworkPropertyMetadata(typeof(MultiComboBox)));
    }

    public MultiComboBox()
    {
        MultiSelectedItems = [];
        MultiSelectedItems.CollectionChanged += OnSelectedItemsCollectionChanged;

        if (ReadLocalValue(SelectAllTextProperty) == DependencyProperty.UnsetValue)
        {
            SetCurrentValue(SelectAllTextProperty, SH.MultiComboBoxSelectAll);
        }
        if (ReadLocalValue(AllSelectedTextProperty) == DependencyProperty.UnsetValue)
        {
            SetCurrentValue(AllSelectedTextProperty, SH.MultiComboBoxAllSelected);
        }
        if (ReadLocalValue(PlaceholderTextProperty) == DependencyProperty.UnsetValue)
        {
            SetCurrentValue(PlaceholderTextProperty, SH.PleaseSelect);
        }
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        PART_SelectAllCheckBox?.Click -= OnSelectAllCheckBoxClick;

        PART_SelectAllCheckBox = GetTemplateChild(nameof(PART_SelectAllCheckBox)) as CheckBox;
        PART_ItemsPresenter = GetTemplateChild(nameof(PART_ItemsPresenter)) as ItemsPresenter;
        PART_SelectedText = GetTemplateChild(nameof(PART_SelectedText)) as TextBlock;

        PART_SelectAllCheckBox?.Click += OnSelectAllCheckBoxClick;

        UpdateSelectedText();
        UpdateSelectAllState();
    }

    protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
    {
        base.OnItemsChanged(e);
        UpdateSelectAllState();
    }

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is MultiComboBoxItem;
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new MultiComboBoxItem();
    }

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);

        if (element is MultiComboBoxItem container)
        {
            container.IsCheckedChanged -= OnItemCheckedChanged;
            container.IsCheckedChanged += OnItemCheckedChanged;

            if (MultiSelectedItems.Contains(item))
            {
                container.IsItemChecked = true;
            }
        }
    }

    protected override void ClearContainerForItemOverride(DependencyObject element, object item)
    {
        base.ClearContainerForItemOverride(element, item);

        if (element is MultiComboBoxItem container)
        {
            container.IsCheckedChanged -= OnItemCheckedChanged;
        }
    }

    private void OnItemCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (_suppressItemUpdate)
            return;

        if (sender is not MultiComboBoxItem item)
            return;

        object? dataItem = ItemContainerGenerator.ItemFromContainer(item);
        if (dataItem == null || dataItem == DependencyProperty.UnsetValue)
            return;

        if (item.IsItemChecked)
        {
            if (!MultiSelectedItems.Contains(dataItem))
                MultiSelectedItems.Add(dataItem);
        }
        else
        {
            MultiSelectedItems.Remove(dataItem);
        }

        UpdateSelectAllState();
        UpdateSelectedText();
    }

    private void OnSelectAllCheckBoxClick(object sender, RoutedEventArgs e)
    {
        if (_suppressSelectAllUpdate)
            return;

        bool shouldSelectAll = MultiSelectedItems.Count < Items.Count;

        _suppressItemUpdate = true;

        try
        {
            if (shouldSelectAll)
            {
                MultiSelectedItems.Clear();
                foreach (object item in Items)
                {
                    MultiSelectedItems.Add(item);
                }
                SetAllItemsChecked(true);
            }
            else
            {
                MultiSelectedItems.Clear();
                SetAllItemsChecked(false);
            }
        }
        finally
        {
            _suppressItemUpdate = false;
        }

        SelectAllCheckState = shouldSelectAll;
        PART_SelectAllCheckBox?.IsChecked = shouldSelectAll;
        UpdateSelectedText();
        UpdateSelectAllState();
    }

    private void SetAllItemsChecked(bool isChecked)
    {
        for (int i = 0; i < Items.Count; i++)
        {
            if (ItemContainerGenerator.ContainerFromIndex(i) is MultiComboBoxItem container)
            {
                container.IsItemChecked = isChecked;
            }
        }
    }

    private void OnSelectedItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateSelectedText();
        UpdateSelectAllState();
    }

    private void UpdateSelectAllState()
    {
        if (_suppressSelectAllUpdate)
            return;

        int total = Items.Count;
        int selected = MultiSelectedItems.Count;

        bool? newState;
        if (selected == 0)
            newState = false;
        else if (selected == total)
            newState = true;
        else
            newState = null; // Indeterminate

        _suppressSelectAllUpdate = true;
        try
        {
            SelectAllCheckState = newState;
            PART_SelectAllCheckBox?.IsChecked = newState;
        }
        finally
        {
            _suppressSelectAllUpdate = false;
        }
    }

    private void UpdateSelectedText()
    {
        if (PART_SelectedText == null)
            return;

        if (MultiSelectedItems.Count == 0)
        {
            PART_SelectedText.Text = PlaceholderText;
            return;
        }

        if (ShowAllSelectedText && Items.Count > 0 && MultiSelectedItems.Count == Items.Count)
        {
            PART_SelectedText.Text = AllSelectedText ?? SH.MultiComboBoxAllSelected;
            return;
        }

        var parts = MultiSelectedItems.Select(item =>
        {
            if (item is MultiComboBoxItem container)
                return container.Content?.ToString() ?? string.Empty;

            // Try to resolve display text from the item container
            if (ItemContainerGenerator.ContainerFromItem(item) is MultiComboBoxItem c)
                return c.Content?.ToString() ?? item?.ToString() ?? string.Empty;

            return item?.ToString() ?? string.Empty;
        });

        PART_SelectedText.Text = string.Join(Separator, parts);
    }

    protected override void OnDropDownClosed(EventArgs e)
    {
        base.OnDropDownClosed(e);
        // Prevent ComboBox from altering SelectedItem after dropdown closes
        SelectedItem = null;
    }

    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        // Do not call base — prevents ComboBox from overwriting the display text
    }
}
