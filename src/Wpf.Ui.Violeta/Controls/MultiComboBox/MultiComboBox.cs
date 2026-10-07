using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Wpf.Ui.Violeta.Resources.Localization;

namespace Wpf.Ui.Violeta.Controls;

[TemplatePart(Name = nameof(PART_SelectAllCheckBox), Type = typeof(CheckBox))]
[TemplatePart(Name = nameof(PART_ItemsPresenter), Type = typeof(ItemsPresenter))]
[TemplatePart(Name = nameof(PART_SelectedText), Type = typeof(TextBlock))]
[TemplatePart(Name = nameof(PART_SelectedItems), Type = typeof(ItemsControl))]
public class MultiComboBox : ComboBox
{
    private CheckBox? PART_SelectAllCheckBox;
    private ItemsPresenter? PART_ItemsPresenter;
    private TextBlock? PART_SelectedText;
    private ItemsControl? PART_SelectedItems;

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
            new PropertyMetadata(", ", OnDisplayPropertyChanged));

    public static readonly DependencyProperty PlaceholderTextProperty =
        DependencyProperty.Register(
            nameof(PlaceholderText),
            typeof(string),
            typeof(MultiComboBox),
            new PropertyMetadata(string.Empty, OnDisplayPropertyChanged));

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
            new PropertyMetadata(true, OnDisplayPropertyChanged));

    public static readonly DependencyProperty AllSelectedTextProperty =
        DependencyProperty.Register(
            nameof(AllSelectedText),
            typeof(string),
            typeof(MultiComboBox),
            new PropertyMetadata(null, OnDisplayPropertyChanged));

    public static readonly DependencyProperty SelectedItemTemplateProperty =
        DependencyProperty.Register(
            nameof(SelectedItemTemplate),
            typeof(DataTemplate),
            typeof(MultiComboBox),
            new PropertyMetadata(null, OnDisplayPropertyChanged));

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

    /// <summary>
    /// Template used to display selected items in the closed combo box.
    /// When null, <see cref="ItemsControl.ItemTemplate"/> is used as a fallback.
    /// </summary>
    public DataTemplate? SelectedItemTemplate
    {
        get => (DataTemplate?)GetValue(SelectedItemTemplateProperty);
        set => SetValue(SelectedItemTemplateProperty, value);
    }

    private static void OnDisplayPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MultiComboBox box)
            box.UpdateSelectedDisplay();
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
        PART_SelectedItems = GetTemplateChild(nameof(PART_SelectedItems)) as ItemsControl;

        PART_SelectAllCheckBox?.Click += OnSelectAllCheckBoxClick;

        UpdateSelectedDisplay();
        UpdateSelectAllState();
    }

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.Property == ItemTemplateProperty || e.Property == DisplayMemberPathProperty)
            UpdateSelectedDisplay();
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
        UpdateSelectedDisplay();
    }

    private void OnSelectAllCheckBoxClick(object? sender, RoutedEventArgs e)
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
        UpdateSelectedDisplay();
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
        UpdateSelectedDisplay();
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

    private DataTemplate? GetEffectiveSelectedItemTemplate() =>
        SelectedItemTemplate ?? ItemTemplate;

    private void UpdateSelectedDisplay()
    {
        if (PART_SelectedText is null)
            return;

        if (MultiSelectedItems.Count == 0)
        {
            ShowSelectedText(PlaceholderText);
            return;
        }

        if (ShowAllSelectedText && Items.Count > 0 && MultiSelectedItems.Count == Items.Count)
        {
            ShowSelectedText(AllSelectedText ?? SH.MultiComboBoxAllSelected);
            return;
        }

        var template = GetEffectiveSelectedItemTemplate();
        if (template is not null && PART_SelectedItems is not null)
        {
            ShowSelectedItems(template);
            return;
        }

        var parts = MultiSelectedItems.Select(GetItemDisplayText);
        ShowSelectedText(string.Join(Separator, parts));
    }

    private void ShowSelectedText(string text)
    {
        PART_SelectedText!.Text = text;
        PART_SelectedText.Visibility = Visibility.Visible;

        if (PART_SelectedItems is null)
            return;

        PART_SelectedItems.Visibility = Visibility.Collapsed;
        PART_SelectedItems.ItemsSource = null;
        PART_SelectedItems.Items.Clear();
        PART_SelectedItems.ItemTemplate = null;
    }

    private void ShowSelectedItems(DataTemplate template)
    {
        PART_SelectedText!.Visibility = Visibility.Collapsed;

        PART_SelectedItems!.Visibility = Visibility.Visible;
        PART_SelectedItems.ItemsSource = null;
        PART_SelectedItems.ItemTemplate = null;
        PART_SelectedItems.Items.Clear();

        for (var i = 0; i < MultiSelectedItems.Count; i++)
        {
            if (i > 0 && !string.IsNullOrEmpty(Separator))
            {
                PART_SelectedItems.Items.Add(new TextBlock
                {
                    Text = Separator,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = Foreground
                });
            }

            PART_SelectedItems.Items.Add(new ContentPresenter
            {
                Content = ResolveSelectedContent(MultiSelectedItems[i]),
                ContentTemplate = template,
                VerticalAlignment = VerticalAlignment.Center
            });
        }
    }

    private static object? ResolveSelectedContent(object item) =>
        item is MultiComboBoxItem container ? container.Content : item;

    private string GetItemDisplayText(object item)
    {
        if (item is MultiComboBoxItem container)
            return container.Content?.ToString() ?? string.Empty;

        if (!string.IsNullOrEmpty(DisplayMemberPath))
        {
            var value = GetDisplayMemberValue(item, DisplayMemberPath);
            if (value is not null)
                return value;
        }

        if (ItemContainerGenerator.ContainerFromItem(item) is MultiComboBoxItem c)
            return c.Content?.ToString() ?? item.ToString() ?? string.Empty;

        return item.ToString() ?? string.Empty;
    }

    private static string? GetDisplayMemberValue(object item, string path)
    {
        try
        {
            var binding = new Binding(path) { Source = item };
            var evaluator = new BindingEvaluator();
            BindingOperations.SetBinding(evaluator, BindingEvaluator.ValueProperty, binding);
            var result = evaluator.Value?.ToString();
            BindingOperations.ClearBinding(evaluator, BindingEvaluator.ValueProperty);
            return result;
        }
        catch (Exception)
        {
            var property = TypeDescriptor.GetProperties(item)[path];
            return property?.GetValue(item)?.ToString();
        }
    }

    private sealed class BindingEvaluator : DependencyObject
    {
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                nameof(Value),
                typeof(object),
                typeof(BindingEvaluator));

        public object? Value
        {
            get => GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }
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
