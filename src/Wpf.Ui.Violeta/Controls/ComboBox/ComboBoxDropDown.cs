using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;

namespace Wpf.Ui.Violeta.Controls;

public static class ComboBoxDropDown
{
    public static readonly DependencyProperty RemeasureOnOpenProperty =
        DependencyProperty.RegisterAttached(
            "RemeasureOnOpen",
            typeof(bool),
            typeof(ComboBoxDropDown),
            new PropertyMetadata(false, OnRemeasureOnOpenChanged));

    public static bool GetRemeasureOnOpen(DependencyObject element) =>
        (bool)element.GetValue(RemeasureOnOpenProperty);

    public static void SetRemeasureOnOpen(DependencyObject element, bool value) =>
        element.SetValue(RemeasureOnOpenProperty, value);

    private static void OnRemeasureOnOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ComboBox comboBox)
        {
            return;
        }

        if ((bool)e.NewValue)
        {
            comboBox.DropDownOpened += OnDropDownOpened;
        }
        else
        {
            comboBox.DropDownOpened -= OnDropDownOpened;
        }
    }

    private static void OnDropDownOpened(object? sender, EventArgs e)
    {
        if (sender is not ComboBox comboBox)
        {
            return;
        }

        comboBox.Dispatcher.BeginInvoke(
            DispatcherPriority.Loaded,
            () =>
            {
                if (!comboBox.IsDropDownOpen
                    || comboBox.Template?.FindName("Popup", comboBox) is not Popup popup
                    || popup.Child is not UIElement child)
                {
                    return;
                }

                popup.Child = null;
                popup.Child = child;
            });
    }
}
