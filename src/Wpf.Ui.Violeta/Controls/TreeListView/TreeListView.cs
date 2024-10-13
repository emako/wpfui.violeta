using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace Wpf.Ui.Controls;

public class TreeListView : TreeView
{
    public new object SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public new static readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.Register(nameof(SelectedItem), typeof(object), typeof(TreeListView), new PropertyMetadata(null, OnNewSelectedItemChanged));

    private static void OnNewSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Use the private `TreeView::SelectedItemProperty` to
        // allow writing to the readonly `TreeView::SelectedItemProperty`.
        if (d is TreeListView treeListView)
        {
            object? newValue = e.NewValue;

            if (typeof(TreeView).GetField("SelectedItemPropertyKey", BindingFlags.NonPublic | BindingFlags.Static) is FieldInfo fieldInfo)
            {
                DependencyPropertyKey selectedItemPropertyKey = (DependencyPropertyKey)fieldInfo.GetValue(null)!;
                object? currentValue = treeListView.GetValue(TreeView.SelectedItemProperty);

                if (currentValue != newValue)
                {
                    treeListView.SetValue(selectedItemPropertyKey, newValue);
                }
            }
        }
    }

    public TreeListView()
    {
        SelectedItemChanged += (sender, e) =>
        {
            // Avoid exceptions caused by possible inconsistencies with
            // the handling of readonly `TreeView::SelectedItemProperty`.
            try
            {
                if (SelectedItem != base.SelectedItem)
                {
                    SelectedItem = base.SelectedItem;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        };
    }

    public GridViewColumnCollection Columns
    {
        get => (GridViewColumnCollection)GetValue(ColumnsProperty);
        set => SetValue(ColumnsProperty, value);
    }

    public static readonly DependencyProperty ColumnsProperty =
        DependencyProperty.Register(nameof(Columns), typeof(GridViewColumnCollection), typeof(TreeListView), new PropertyMetadata(null));

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(TreeListView), new PropertyMetadata(new CornerRadius(4d)));

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new TreeListViewItem();
    }

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is TreeListViewItem;
    }
}
