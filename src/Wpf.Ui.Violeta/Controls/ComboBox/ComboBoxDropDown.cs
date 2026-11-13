using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Keeps the combo dropdown sized to its items and scrolled to the top after the item list changes.
/// </summary>
public static class ComboBoxDropDown
{
    public static readonly DependencyProperty ReflowOnOpenProperty =
        DependencyProperty.RegisterAttached(
            "ReflowOnOpen",
            typeof(bool),
            typeof(ComboBoxDropDown),
            new PropertyMetadata(false, OnReflowOnOpenChanged));

    public static bool GetReflowOnOpen(DependencyObject element)
    {
        return (bool)element.GetValue(ReflowOnOpenProperty);
    }

    public static void SetReflowOnOpen(DependencyObject element, bool value)
    {
        element.SetValue(ReflowOnOpenProperty, value);
    }

    private static void OnReflowOnOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
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
            new Action(() => Reflow(comboBox, allowRefresh: true)));
    }

    private static void Reflow(ComboBox comboBox, bool allowRefresh)
    {
        if (!comboBox.IsDropDownOpen
            || comboBox.Template?.FindName("Popup", comboBox) is not Popup popup
            || popup.Child is not DependencyObject popupChild)
        {
            return;
        }

        ScrollViewer? scrollViewer = FindScrollViewer(popupChild);
        if (scrollViewer is null)
        {
            return;
        }

        scrollViewer.ScrollToVerticalOffset(0);
        scrollViewer.VerticalAlignment = VerticalAlignment.Top;
        scrollViewer.ClearValue(FrameworkElement.HeightProperty);

        int itemCount = comboBox.Items.Count;
        int realized = 0;
        double contentHeight = 0;
        for (int index = 0; index < itemCount; index++)
        {
            if (comboBox.ItemContainerGenerator.ContainerFromIndex(index) is not FrameworkElement container)
            {
                continue;
            }

            realized++;
            double height = container.ActualHeight;
            if (height <= 0)
            {
                height = container.DesiredSize.Height;
            }

            contentHeight += height;
        }

        if (itemCount > 0 && realized == 0 && allowRefresh)
        {
            comboBox.Items.Refresh();
            comboBox.Dispatcher.BeginInvoke(
                DispatcherPriority.ContextIdle,
                new Action(() => Reflow(comboBox, allowRefresh: false)));
            return;
        }

        if (itemCount == 0 || realized == 0 || contentHeight <= 0)
        {
            scrollViewer.ClearValue(FrameworkElement.MaxHeightProperty);
            scrollViewer.ClearValue(ScrollViewer.VerticalScrollBarVisibilityProperty);
            return;
        }

        double maxHeight = comboBox.MaxDropDownHeight;
        if (double.IsNaN(maxHeight) || maxHeight <= 0)
        {
            maxHeight = contentHeight;
        }

        if (contentHeight <= maxHeight + 1)
        {
            scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
            scrollViewer.MaxHeight = contentHeight;
        }
        else
        {
            scrollViewer.ClearValue(ScrollViewer.VerticalScrollBarVisibilityProperty);
            scrollViewer.MaxHeight = maxHeight;
        }

        if (popup.Child is FrameworkElement popupRoot)
        {
            popupRoot.VerticalAlignment = VerticalAlignment.Top;
            popupRoot.InvalidateMeasure();
            popupRoot.UpdateLayout();
        }
    }

    private static ScrollViewer? FindScrollViewer(DependencyObject parent)
    {
        if (parent is ScrollViewer scrollViewer)
        {
            return scrollViewer;
        }

        int count = VisualTreeHelper.GetChildrenCount(parent);
        for (int index = 0; index < count; index++)
        {
            ScrollViewer? found = FindScrollViewer(VisualTreeHelper.GetChild(parent, index));
            if (found is not null)
            {
                return found;
            }
        }

        return null;
    }
}
