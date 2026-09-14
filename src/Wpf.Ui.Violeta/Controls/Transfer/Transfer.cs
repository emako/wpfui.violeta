using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Wpf.Ui.Violeta.Collections;
using Wpf.Ui.Violeta.Resources.Localization;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Dual-list transfer control: select items on the left and move them to the right (and back).
/// Ported from HandyControl's Transfer control.
/// </summary>
[TemplatePart(Name = PART_SelectedListBox, Type = typeof(ListBox))]
[DefaultEvent(nameof(TransferredItemsChanged))]
public class Transfer : ListBox
{
    public const string PART_SelectedListBox = "PART_SelectedListBox";

    private ListBox? _selectedListBox;

    static Transfer()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(Transfer),
            new FrameworkPropertyMetadata(typeof(Transfer)));
    }

    public Transfer()
    {
        CommandBindings.Add(new CommandBinding(TransferCommands.Selected, SelectItems));
        CommandBindings.Add(new CommandBinding(TransferCommands.Cancel, DeselectItems));
        Loaded += OnLoaded;
        SetCurrentValue(EmptyTextProperty, SH.EmptyNoData);
    }

    #region TransferredItemsChanged

    public static readonly RoutedEvent TransferredItemsChangedEvent =
        EventManager.RegisterRoutedEvent(
            nameof(TransferredItemsChanged),
            RoutingStrategy.Bubble,
            typeof(SelectionChangedEventHandler),
            typeof(Transfer));

    [Category("Behavior")]
    public event SelectionChangedEventHandler TransferredItemsChanged
    {
        add => AddHandler(TransferredItemsChangedEvent, value);
        remove => RemoveHandler(TransferredItemsChangedEvent, value);
    }

    #endregion TransferredItemsChanged

    #region TransferredItems

    private static readonly DependencyPropertyKey TransferredItemsPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(TransferredItems),
            typeof(IList),
            typeof(Transfer),
            new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty TransferredItemsProperty =
        TransferredItemsPropertyKey.DependencyProperty;

    [Bindable(true)]
    [Category("Appearance")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IList? TransferredItems => (IList?)GetValue(TransferredItemsProperty);

    #endregion TransferredItems

    #region EmptyText

    public static readonly DependencyProperty EmptyTextProperty =
        DependencyProperty.Register(
            nameof(EmptyText),
            typeof(string),
            typeof(Transfer),
            new PropertyMetadata(string.Empty));

    /// <summary>Text shown under the empty logo when the source list has no items.</summary>
    public string EmptyText
    {
        get => (string)GetValue(EmptyTextProperty);
        set => SetValue(EmptyTextProperty, value);
    }

    #endregion EmptyText

    #region CornerRadius

    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(Transfer),
            new FrameworkPropertyMetadata(new CornerRadius(4)));

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    #endregion CornerRadius

    private void OnLoaded(object sender, RoutedEventArgs e) => SelectItems(null!, null!);

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _selectedListBox = GetTemplateChild(PART_SelectedListBox) as ListBox;
    }

    protected virtual void OnTransferredItemsChanged(SelectionChangedEventArgs e) => RaiseEvent(e);

    protected override bool IsItemItsOwnContainerOverride(object item) => item is TransferItem;

    protected override DependencyObject GetContainerForItemOverride() => new TransferItem();

    private void SelectItems(object sender, ExecutedRoutedEventArgs e)
    {
        if (_selectedListBox is null || SelectedItems.Count == 0)
            return;

        foreach (var item in SelectedItems)
        {
            if (ItemContainerGenerator.ContainerFromItem(item) is not TransferItem { IsTransferred: false } selectedItem)
                continue;

            selectedItem.IsTransferred = true;

            var transferItem = new TransferItem { Tag = item };

            if (ItemsSource is not null)
            {
                if (string.IsNullOrEmpty(DisplayMemberPath))
                    transferItem.Content = item;
                else
                    transferItem.SetBinding(ContentControl.ContentProperty, new Binding(DisplayMemberPath) { Source = item });
            }
            else
            {
                transferItem.Content = item is TransferItem container ? container.Content : item;
            }

            _selectedListBox.Items.Add(transferItem);
        }

        SetTransferredItems(_selectedListBox.Items.OfType<TransferItem>().Select(item => item.Tag));
        OnTransferredItemsChanged(new SelectionChangedEventArgs(TransferredItemsChangedEvent, new List<object>(), SelectedItems)
        {
            Source = this
        });
        UnselectAll();
    }

    private void DeselectItems(object sender, ExecutedRoutedEventArgs e)
    {
        if (_selectedListBox is null)
            return;

        var deselectItems = new List<object>();
        foreach (var transferItem in _selectedListBox.Items.OfType<TransferItem>().ToList())
        {
            if (!transferItem.IsSelected)
                continue;

            if (ItemContainerGenerator.ContainerFromItem(transferItem.Tag) is not TransferItem selectedItem)
                continue;

            _selectedListBox.Items.Remove(transferItem);
            if (transferItem.Tag is not null)
                deselectItems.Add(transferItem.Tag);

            selectedItem.SetCurrentValue(TransferItem.IsTransferredProperty, false);
            selectedItem.SetCurrentValue(IsSelectedProperty, false);
        }

        SetTransferredItems(_selectedListBox.Items.OfType<TransferItem>().Select(item => item.Tag));
        OnTransferredItemsChanged(new SelectionChangedEventArgs(TransferredItemsChangedEvent, deselectItems, new List<object>())
        {
            Source = this
        });
    }

    private void SetTransferredItems(IEnumerable selectedItems)
    {
        var transferred = GetValue(TransferredItemsProperty) as ManualObservableCollection<object>;

        if (transferred is null)
        {
            transferred = new ManualObservableCollection<object>();
            SetValue(TransferredItemsPropertyKey, transferred);
        }

        transferred.CanNotify = false;
        transferred.Clear();

        foreach (var selectedItem in selectedItems)
        {
            if (selectedItem is not null)
                transferred.Add(selectedItem);
        }

        transferred.CanNotify = true;
    }
}
