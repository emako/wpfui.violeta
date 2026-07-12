using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// An item in a <see cref="TreeComboBox"/>. Supports hierarchical tree structure
/// with expand/collapse and optional selection.
/// </summary>
[TemplatePart(Name = PART_Header, Type = typeof(FrameworkElement))]
public class TreeComboBoxItem : HeaderedItemsControl
{
    public const string PART_Header = "PART_Header";

    private TreeComboBox? _treeComboBox;

    /// <summary>Gets the owning <see cref="TreeComboBox"/>.</summary>
    public TreeComboBox? Owner => _treeComboBox;

    public static readonly DependencyProperty IsSelectedProperty =
        DependencyProperty.Register(
            nameof(IsSelected), typeof(bool), typeof(TreeComboBoxItem),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    public static readonly DependencyProperty IsExpandedProperty =
        DependencyProperty.Register(
            nameof(IsExpanded), typeof(bool), typeof(TreeComboBoxItem),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    /// <summary>
    /// When <see langword="false"/>, clicking on this item does not select it
    /// (the popup stays open). Useful for non-leaf parent nodes.
    /// </summary>
    public static readonly DependencyProperty IsSelectableProperty =
        DependencyProperty.Register(
            nameof(IsSelectable), typeof(bool), typeof(TreeComboBoxItem),
            new PropertyMetadata(true));

    public bool IsSelectable
    {
        get => (bool)GetValue(IsSelectableProperty);
        set => SetValue(IsSelectableProperty, value);
    }

    private static readonly DependencyPropertyKey LevelPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(Level), typeof(int), typeof(TreeComboBoxItem),
            new PropertyMetadata(0));

    public static readonly DependencyProperty LevelProperty = LevelPropertyKey.DependencyProperty;

    /// <summary>Gets the nesting depth (0 = top-level).</summary>
    public int Level => (int)GetValue(LevelProperty);

    static TreeComboBoxItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(TreeComboBoxItem),
            new FrameworkPropertyMetadata(typeof(TreeComboBoxItem)));
    }

    protected override void OnVisualParentChanged(DependencyObject oldParent)
    {
        base.OnVisualParentChanged(oldParent);
        _treeComboBox = FindAncestorOfType<TreeComboBox>(this);
        UpdateLevel();

        if (_treeComboBox is not null && ItemTemplate is null && _treeComboBox.ItemTemplate is not null)
        {
            SetCurrentValue(ItemTemplateProperty, _treeComboBox.ItemTemplate);
        }
    }

    private void UpdateLevel()
    {
        // Count the number of TreeComboBoxItem ancestors (ignoring all intermediate
        // visual elements such as Border/Grid/StackPanel/ItemsPresenter).
        // This is robust for both declarative XAML items and data-bound ItemsSource items.
        int level = 0;
        DependencyObject? current = LogicalTreeHelper.GetParent(this);
        current ??= VisualTreeHelper.GetParent(this);
        while (current is not null)
        {
            if (current is TreeComboBox)
                break;
            if (current is TreeComboBoxItem)
                level++;
            DependencyObject? next = LogicalTreeHelper.GetParent(current);
            next ??= VisualTreeHelper.GetParent(current);
            current = next;
        }
        SetValue(LevelPropertyKey, level);
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        if (!e.Handled && IsSelectable && _treeComboBox is not null)
        {
            _treeComboBox.NotifyItemClicked(this);
            e.Handled = true;
        }
    }

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is TreeComboBoxItem;
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
        return _treeComboBox?.CreateContainerForItemInternal() ?? new TreeComboBoxItem();
    }

    private static T? FindAncestorOfType<T>(DependencyObject? element) where T : DependencyObject
    {
        DependencyObject? current = element;
        while (current is not null)
        {
            DependencyObject? parent = LogicalTreeHelper.GetParent(current);
            parent ??= VisualTreeHelper.GetParent(current);
            if (parent is T result)
                return result;
            current = parent;
        }
        return null;
    }
}
