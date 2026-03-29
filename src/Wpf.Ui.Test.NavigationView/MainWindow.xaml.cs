using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Wpf.Ui.Violeta.Controls;
using Wpf.Ui.Violeta.Controls.Compat;
using NavigationViewControl = Wpf.Ui.Violeta.Controls.NavigationView;

namespace Wpf.Ui.Test.NavigationView;

public partial class MainWindow : ShellWindow
{
    private readonly ObservableCollection<string> _eventLog = new();
    private readonly List<NavigationViewItem> _dynamicItems = new();
    private int _dynamicItemIndex = 1;

    public MainWindow()
    {
        InitializeComponent();

        EventLogListBox.ItemsSource = _eventLog;
        Loaded += MainWindow_OnLoaded;
    }

    private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        TestNavigationView.SelectedItem = HomeItem;
        UpdatePaneState();
        UpdateDisplayModeState();
        UpdateSelectionState("Home", "初始选中项为 Home。");
        PushEvent("Window loaded");
    }

    private void PaneDisplayModeComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (PaneDisplayModeComboBox.SelectedItem is ComboBoxItem { Tag: NavigationViewPaneDisplayMode mode })
        {
            TestNavigationView.PaneDisplayMode = mode;
            PushEvent($"PaneDisplayMode => {mode}");
        }
    }

    private void BackButtonVisibilityComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (BackButtonVisibilityComboBox.SelectedItem is ComboBoxItem { Tag: NavigationViewBackButtonVisible visibility })
        {
            TestNavigationView.IsBackButtonVisible = visibility;
            PushEvent($"BackButton => {visibility}");
        }
    }

    private void TogglePaneButton_OnClick(object sender, RoutedEventArgs e)
    {
        TestNavigationView.IsPaneOpen = !TestNavigationView.IsPaneOpen;
        UpdatePaneState();
        PushEvent($"IsPaneOpen => {TestNavigationView.IsPaneOpen}");
    }

    private void TogglePaneVisibilityButton_OnClick(object sender, RoutedEventArgs e)
    {
        TestNavigationView.IsPaneVisible = !TestNavigationView.IsPaneVisible;
        UpdatePaneState();
        PushEvent($"IsPaneVisible => {TestNavigationView.IsPaneVisible}");
    }

    private void ToggleSettingsButton_OnClick(object sender, RoutedEventArgs e)
    {
        TestNavigationView.IsSettingsVisible = !TestNavigationView.IsSettingsVisible;
        PushEvent($"IsSettingsVisible => {TestNavigationView.IsSettingsVisible}");
    }

    private void SelectSettingsButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (TestNavigationView.SettingsItem is not null)
        {
            TestNavigationView.SelectedItem = TestNavigationView.SettingsItem;
            PushEvent("SelectedItem => Settings");
        }
    }

    private void AddDynamicItemButton_OnClick(object sender, RoutedEventArgs e)
    {
        NavigationViewItem item = CreateDynamicItem(_dynamicItemIndex++);
        _dynamicItems.Add(item);
        TestNavigationView.MenuItems.Add(item);
        PushEvent($"Added dynamic item => {item.Content}");
    }

    private void RemoveDynamicItemButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (_dynamicItems.Count == 0)
        {
            PushEvent("No dynamic items to remove");
            return;
        }

        NavigationViewItem item = _dynamicItems[^1];
        _dynamicItems.RemoveAt(_dynamicItems.Count - 1);
        TestNavigationView.MenuItems.Remove(item);
        PushEvent($"Removed dynamic item => {item.Content}");
    }

    private void ExpandReportsButton_OnClick(object sender, RoutedEventArgs e)
    {
        ReportsItem.IsExpanded = !ReportsItem.IsExpanded;
        PushEvent($"Reports.IsExpanded => {ReportsItem.IsExpanded}");
    }

    private void SelectHomeButton_OnClick(object sender, RoutedEventArgs e)
    {
        TestNavigationView.SelectedItem = HomeItem;
        PushEvent("SelectedItem => Home");
    }

    private void SelectReportsChildButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (ReportsItem.MenuItems.Count > 0)
        {
            ReportsItem.IsExpanded = true;
            TestNavigationView.SelectedItem = ReportsItem.MenuItems[0];
            PushEvent("SelectedItem => Daily report");
        }
    }

    private void ClearLogButton_OnClick(object sender, RoutedEventArgs e)
    {
        _eventLog.Clear();
        PushEvent("Log cleared");
    }

    private void TestNavigationView_OnSelectionChanged(NavigationViewControl sender, NavigationViewSelectionChangedEventArgs args)
    {
        string title = args.IsSettingsSelected ? "Settings" : GetItemLabel(args.SelectedItemContainer ?? args.SelectedItem);
        string detail = args.IsSettingsSelected
            ? "设置项被选中。"
            : $"SelectedItem = {GetItemLabel(args.SelectedItem)}, Container = {GetItemLabel(args.SelectedItemContainer)}";

        UpdateSelectionState(title, detail);
        PushEvent($"SelectionChanged => {title}");
    }

    private void TestNavigationView_OnItemInvoked(NavigationViewControl sender, NavigationViewItemInvokedEventArgs args)
    {
        string label = args.IsSettingsInvoked ? "Settings" : GetItemLabel(args.InvokedItemContainer ?? args.InvokedItem);
        PushEvent($"ItemInvoked => {label}");
    }

    private void TestNavigationView_OnDisplayModeChanged(NavigationViewControl sender, NavigationViewDisplayModeChangedEventArgs args)
    {
        UpdateDisplayModeState();
        PushEvent($"DisplayModeChanged => {sender.DisplayMode}");
    }

    private void TestNavigationView_OnBackRequested(NavigationViewControl sender, NavigationViewBackRequestedEventArgs args)
    {
        PushEvent("BackRequested");
    }

    private void TestNavigationView_OnPaneOpened(NavigationViewControl sender, object args)
    {
        UpdatePaneState();
        PushEvent("PaneOpened");
    }

    private void TestNavigationView_OnPaneOpening(NavigationViewControl sender, object args)
    {
        UpdatePaneState();
        PushEvent("PaneOpening");
    }

    private void TestNavigationView_OnPaneClosing(NavigationViewControl sender, NavigationViewPaneClosingEventArgs args)
    {
        UpdatePaneState();
        PushEvent("PaneClosing");
    }

    private void TestNavigationView_OnPaneClosed(NavigationViewControl sender, object args)
    {
        UpdatePaneState();
        PushEvent("PaneClosed");
    }

    private void TestNavigationView_OnExpanding(NavigationViewControl sender, NavigationViewItemExpandingEventArgs args)
    {
        PushEvent($"Expanding => {GetItemLabel(args.ExpandingItemContainer ?? args.ExpandingItem)}");
    }

    private void TestNavigationView_OnCollapsed(NavigationViewControl sender, NavigationViewItemCollapsedEventArgs args)
    {
        PushEvent($"Collapsed => {GetItemLabel(args.CollapsedItemContainer ?? args.CollapsedItem)}");
    }

    private void UpdateDisplayModeState()
    {
        CurrentDisplayModeTextBlock.Text = $"{TestNavigationView.DisplayMode} / {TestNavigationView.PaneDisplayMode}";
    }

    private void UpdatePaneState()
    {
        PaneStateTextBlock.Text = $"IsPaneOpen = {TestNavigationView.IsPaneOpen}, IsPaneVisible = {TestNavigationView.IsPaneVisible}";
    }

    private void UpdateSelectionState(string title, string detail)
    {
        CurrentSelectionTextBlock.Text = title;
        CurrentSelectionDetailTextBlock.Text = detail;
    }

    private void PushEvent(string message)
    {
        string line = $"{DateTime.Now:HH:mm:ss.fff}  {message}";
        _eventLog.Insert(0, line);

        while (_eventLog.Count > 100)
        {
            _eventLog.RemoveAt(_eventLog.Count - 1);
        }

        LastEventTextBlock.Text = message;
        EventLogListBox.ScrollIntoView(line);
    }

    private static NavigationViewItem CreateDynamicItem(int index)
    {
        return new NavigationViewItem
        {
            Content = $"Dynamic {index}",
            Tag = $"dynamic/{index}",
            Icon = new FontIcon("\uE8C8", new FontFamily("Segoe Fluent Icons,Segoe MDL2 Assets,Segoe UI Symbol"))
        };
    }

    private static string GetItemLabel(object? item)
    {
        return item switch
        {
            null => "<null>",
            NavigationViewItem navigationViewItem => navigationViewItem.Content?.ToString() ?? "NavigationViewItem",
            NavigationViewItemHeader navigationViewItemHeader => navigationViewItemHeader.Content?.ToString() ?? "Header",
            NavigationViewItemSeparator => "Separator",
            _ => item.ToString() ?? item.GetType().Name,
        };
    }
}