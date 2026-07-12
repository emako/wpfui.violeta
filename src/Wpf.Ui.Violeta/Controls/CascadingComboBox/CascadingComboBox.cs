using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Violeta.Resources.Localization;

namespace Wpf.Ui.Violeta.Controls;

[TemplatePart(Name = PART_SelectedText, Type = typeof(TextBlock))]
public class CascadingComboBox : ComboBox
{
    public const string PART_SelectedText = "PART_SelectedText";

    private bool _isAutoExpanding;

    public static readonly DependencyProperty PlaceholderTextProperty =
        DependencyProperty.Register(nameof(PlaceholderText), typeof(string), typeof(CascadingComboBox), new PropertyMetadata(string.Empty));

    private static readonly DependencyPropertyKey LevelsPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(Levels), typeof(int), typeof(CascadingComboBox), new PropertyMetadata(0));

    public static readonly DependencyProperty LevelsProperty = LevelsPropertyKey.DependencyProperty;

    public static readonly DependencyProperty SelectedCascadingItemProperty =
        DependencyProperty.Register(nameof(SelectedCascadingItem), typeof(ICascadingItem), typeof(CascadingComboBox),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedCascadingItemChanged));

    static CascadingComboBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(CascadingComboBox), new FrameworkPropertyMetadata(typeof(CascadingComboBox)));
        ItemsSourceProperty.OverrideMetadata(typeof(CascadingComboBox), new FrameworkPropertyMetadata(OnItemsSourceChanged));
    }

    public CascadingComboBox()
    {
        if (ReadLocalValue(PlaceholderTextProperty) == DependencyProperty.UnsetValue)
        {
            SetCurrentValue(PlaceholderTextProperty, SH.PleaseSelect);
        }
    }

    public string PlaceholderText
    {
        get => (string)GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    /// <summary>
    /// Number of columns currently visible in the dropdown. Auto-updates as the user makes selections.
    /// </summary>
    public int Levels => (int)GetValue(LevelsProperty);

    /// <summary>
    /// The leaf node currently selected by the user. Two-way bindable.
    /// </summary>
    public ICascadingItem? SelectedCascadingItem
    {
        get => (ICascadingItem?)GetValue(SelectedCascadingItemProperty);
        set => SetValue(SelectedCascadingItemProperty, value);
    }

    /// <summary>
    /// Internal columns collection that drives the dynamic popup layout.
    /// </summary>
    public ObservableCollection<CascadingColumnData> Columns { get; } = [];

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is not null and not IEnumerable<ICascadingItem>)
        {
            throw new ArgumentException($"{nameof(ItemsSource)} must be of type IEnumerable<ICascadingItem>.", nameof(ItemsSource));
        }
        ((CascadingComboBox)d).ResetColumns();
    }

    private static void OnSelectedCascadingItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is ICascadingItem)
        {
            ((CascadingComboBox)d).IsDropDownOpen = false;
        }
    }

    private void ResetColumns()
    {
        foreach (var col in Columns)
        {
            col.PropertyChanged -= OnColumnSelectionChanged;
        }
        Columns.Clear();
        SetCurrentValue(SelectedCascadingItemProperty, null);

        var root = (ItemsSource as IEnumerable<ICascadingItem>)?.ToList();
        if (root is { Count: > 0 })
        {
            AddColumn(root);
        }
    }

    /// <summary>
    /// Automatically selects the first item in the first column only,
    /// expanding to show 2 columns (first + second level) when the dropdown opens.
    /// </summary>
    private void AutoExpandFirstPath()
    {
        _isAutoExpanding = true;
        try
        {
            int colIdx = 0;
            while (colIdx < Columns.Count && colIdx < 1)
            {
                var col = Columns[colIdx];
                if (col.Items.Count == 0 || col.SelectedItem != null)
                    break;
                var firstItem = col.Items[0];
                // Only auto-select non-leaf nodes to expand the next column.
                // Never select a leaf so it doesn't appear highlighted.
                var children = firstItem.Children?.ToList();
                if (children is not { Count: > 0 })
                    break;
                col.SelectedItem = firstItem;
                colIdx++;
            }
        }
        finally
        {
            _isAutoExpanding = false;
        }
    }

    protected override void OnDropDownOpened(EventArgs e)
    {
        base.OnDropDownOpened(e);
        AutoExpandFirstPath();
    }

    private void AddColumn(IEnumerable<ICascadingItem> items)
    {
        var col = new CascadingColumnData(Columns.Count, items);
        col.PropertyChanged += OnColumnSelectionChanged;
        Columns.Add(col);
        SetValue(LevelsPropertyKey, Columns.Count);
    }

    private void OnColumnSelectionChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not CascadingColumnData column || e.PropertyName != nameof(CascadingColumnData.SelectedItem))
            return;

        // Remove all columns after this one
        int idx = column.ColumnIndex;
        while (Columns.Count > idx + 1)
        {
            Columns[Columns.Count - 1].PropertyChanged -= OnColumnSelectionChanged;
            Columns.RemoveAt(Columns.Count - 1);
        }
        SetValue(LevelsPropertyKey, Columns.Count);

        var selected = column.SelectedItem;
        if (selected is null)
        {
            SetCurrentValue(SelectedCascadingItemProperty, null);
            return;
        }

        var children = selected.Children?.ToList();
        if (children is { Count: > 0 })
        {
            // Non-leaf: expand to next column
            SetCurrentValue(SelectedCascadingItemProperty, null);
            AddColumn(children);
        }
        else if (!_isAutoExpanding)
        {
            // Leaf: commit selection and close (skip during auto-expand)
            SetCurrentValue(SelectedCascadingItemProperty, selected);
        }
    }
}
