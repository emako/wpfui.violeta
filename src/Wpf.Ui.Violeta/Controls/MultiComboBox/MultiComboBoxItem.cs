using System.Windows;
using System.Windows.Controls;

namespace Wpf.Ui.Violeta.Controls;

public class MultiComboBoxItem : ComboBoxItem
{
    public static readonly RoutedEvent IsCheckedChangedEvent =
        EventManager.RegisterRoutedEvent(
            nameof(IsCheckedChanged),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(MultiComboBoxItem));

    public static readonly DependencyProperty IsItemCheckedProperty =
        DependencyProperty.Register(
            nameof(IsItemChecked),
            typeof(bool),
            typeof(MultiComboBoxItem),
            new PropertyMetadata(false, OnIsItemCheckedChanged));

    public event RoutedEventHandler IsCheckedChanged
    {
        add => AddHandler(IsCheckedChangedEvent, value);
        remove => RemoveHandler(IsCheckedChangedEvent, value);
    }

    public bool IsItemChecked
    {
        get => (bool)GetValue(IsItemCheckedProperty);
        set => SetValue(IsItemCheckedProperty, value);
    }

    static MultiComboBoxItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(MultiComboBoxItem),
            new FrameworkPropertyMetadata(typeof(MultiComboBoxItem)));
    }

    private static void OnIsItemCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MultiComboBoxItem item)
        {
            item.RaiseEvent(new RoutedEventArgs(IsCheckedChangedEvent, item));
        }
    }

    protected override void OnPreviewMouseLeftButtonUp(System.Windows.Input.MouseButtonEventArgs e)
    {
        // Toggle the checked state without closing the drop-down box
        IsItemChecked = !IsItemChecked;
        e.Handled = true;
    }
}
