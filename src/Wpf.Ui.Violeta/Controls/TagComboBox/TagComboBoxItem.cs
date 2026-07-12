using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// A selectable item inside <see cref="TagComboBox"/>'s dropdown. Clicking toggles
/// its checked state without closing the drop-down.
/// </summary>
public class TagComboBoxItem : ComboBoxItem
{
    public static readonly RoutedEvent IsCheckedChangedEvent =
        EventManager.RegisterRoutedEvent(
            nameof(IsCheckedChanged),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(TagComboBoxItem));

    public static readonly DependencyProperty IsItemCheckedProperty =
        DependencyProperty.Register(
            nameof(IsItemChecked),
            typeof(bool),
            typeof(TagComboBoxItem),
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

    static TagComboBoxItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(TagComboBoxItem),
            new FrameworkPropertyMetadata(typeof(TagComboBoxItem)));
    }

    private static void OnIsItemCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TagComboBoxItem item)
            item.RaiseEvent(new RoutedEventArgs(IsCheckedChangedEvent, item));
    }

    protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        IsItemChecked = !IsItemChecked;
        e.Handled = true;
    }
}
