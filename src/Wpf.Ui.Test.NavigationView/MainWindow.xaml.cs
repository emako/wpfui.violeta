using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Wpf.Ui.Test.NavigationView.Pages;
using Wpf.Ui.Violeta.Controls;
using Wpf.Ui.Violeta.Controls.Compat;
using NavigationViewControl = Wpf.Ui.Violeta.Controls.NavigationView;

namespace Wpf.Ui.Test.NavigationView;

public partial class MainWindow : ShellWindow
{
    private readonly ObservableCollection<string> _eventLog = new();
    private readonly List<NavigationViewItem> _dynamicItems = new();
    private readonly List<NavigationViewItem> _runtimeMenuItems = new();
    private int _dynamicItemIndex = 1;
    private string? _currentPageKey;
    private NavigationViewItem? _homeItem;
    private NavigationViewItem? _reportsItem;

    public MainWindow()
    {
        InitializeComponent();

        EventLogListBox.ItemsSource = _eventLog;
        Loaded += MainWindow_OnLoaded;
    }

    private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        BuildNavigationItems();
        TestNavigationView.PaneTitle = $"Navigation Lab ({TestNavigationView.MenuItems.Count})";
        TestNavigationView.Header = $"NavigationView 手动测试台 / Items={TestNavigationView.MenuItems.Count}";
        TestNavigationView.SelectedItem = _homeItem;
        UpdatePaneState();
        UpdateDisplayModeState();
        UpdateSelectionState("Home", "初始选中项为 Home。");
        NavigateToTag("home", "Home");
        PushEvent($"Window loaded, MenuItems={TestNavigationView.MenuItems.Count}, FooterMenuItems={TestNavigationView.FooterMenuItems.Count}");
        Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(DumpNavigationDiagnostics));
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
        if (_reportsItem is null)
        {
            return;
        }

        _reportsItem.IsExpanded = !_reportsItem.IsExpanded;
        PushEvent($"Reports.IsExpanded => {_reportsItem.IsExpanded}");
    }

    private void SelectHomeButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (_homeItem is not null)
        {
            TestNavigationView.SelectedItem = _homeItem;
        }

        PushEvent("SelectedItem => Home");
    }

    private void SelectReportsChildButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (_reportsItem?.MenuItems.Count > 0)
        {
            _reportsItem.IsExpanded = true;
            TestNavigationView.SelectedItem = _reportsItem.MenuItems[0];
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
        string pageTag = args.IsSettingsSelected
            ? "settings"
            : ResolveTag(args.SelectedItemContainer ?? args.SelectedItem);

        UpdateSelectionState(title, detail);
        NavigateToTag(pageTag, title);
        PlayContentTransition();
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

    private void BuildNavigationItems()
    {
        var menuItems = new ObservableCollection<object>();

        TestNavigationView.FooterMenuItems.Clear();
        _runtimeMenuItems.Clear();

        _homeItem = CreateNavigationItem("Home", "home", "\uE80F", infoBadgeValue: 3);
        _reportsItem = CreateNavigationItem("Reports", "reports", "\uE9D2");
        _reportsItem.MenuItems.Add(CreateNavigationItem("Daily report", "reports/daily", "\uE8FD"));
        _reportsItem.MenuItems.Add(CreateNavigationItem("Weekly review", "reports/weekly", "\uE823"));
        _reportsItem.MenuItems.Add(CreateNavigationItem("Executive summary", "reports/executive", "\uE7C3"));

        _runtimeMenuItems.Add(_homeItem);
        _runtimeMenuItems.Add(_reportsItem);
        _runtimeMenuItems.Add(CreateNavigationItem("Approvals", "approvals", "\uE8FB"));
        _runtimeMenuItems.Add(CreateNavigationItem("Tasks", "tasks", "\uE823"));
        _runtimeMenuItems.Add(CreateNavigationItem("Analytics", "analytics", "\uE9D9"));
        _runtimeMenuItems.Add(CreateNavigationItem("Customers", "customers", "\uE716"));
        _runtimeMenuItems.Add(CreateNavigationItem("Orders", "orders", "\uE8FE"));
        _runtimeMenuItems.Add(CreateNavigationItem("Billing", "billing", "\uE8C7"));
        _runtimeMenuItems.Add(CreateNavigationItem("Inventory", "inventory", "\uE7BF"));
        _runtimeMenuItems.Add(CreateNavigationItem("Studio", "studio", "\uE714"));
        _runtimeMenuItems.Add(CreateNavigationItem("Timeline", "timeline", "\uE823"));
        _runtimeMenuItems.Add(CreateNavigationItem("Experiments", "experiments", "\uE7BE"));

        menuItems.Add(_homeItem);
        menuItems.Add(new NavigationViewItemHeader { Content = "Operations" });
        menuItems.Add(_reportsItem);
        menuItems.Add(_runtimeMenuItems[2]);
        menuItems.Add(_runtimeMenuItems[3]);
        menuItems.Add(new NavigationViewItemSeparator());

        for (int index = 4; index < _runtimeMenuItems.Count; index++)
        {
            menuItems.Add(_runtimeMenuItems[index]);
        }

        TestNavigationView.MenuItems = menuItems;

        TestNavigationView.FooterMenuItems.Add(CreateNavigationItem("Support", "support", "\uE897"));
        TestNavigationView.FooterMenuItems.Add(CreateNavigationItem("Archive", "archive", "\uE7B8"));
    }

    private static NavigationViewItem CreateNavigationItem(string content, string tag, string glyph, int? infoBadgeValue = null)
    {
        var item = new NavigationViewItem
        {
            Content = content,
            Tag = tag,
            Icon = new FontIcon(glyph, new FontFamily("Segoe Fluent Icons,Segoe MDL2 Assets,Segoe UI Symbol"))
        };

        if (infoBadgeValue.HasValue)
        {
            item.InfoBadge = new InfoBadge { Value = infoBadgeValue.Value };
        }

        return item;
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

    private void NavigateToTag(string? tag, string title)
    {
        string normalized = NormalizeTag(tag);
        string pageKey = normalized.StartsWith("reports/", StringComparison.OrdinalIgnoreCase) ? "reports" : normalized;

        if (_currentPageKey == pageKey && pageKey != "reports")
        {
            return;
        }

        System.Windows.Controls.Page page = pageKey switch
        {
            "home" => new HomeDemoPage(),
            "reports" => new ReportsDemoPage(normalized),
            "settings" => new SettingsDemoPage(),
            _ => new GenericDemoPage(title, normalized),
        };

        DemoFrame.Navigate(page);
        _currentPageKey = pageKey;
    }

    private static string ResolveTag(object? item)
    {
        return item is NavigationViewItem navigationViewItem && navigationViewItem.Tag is string tag
            ? tag
            : string.Empty;
    }

    private static string NormalizeTag(string? tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
        {
            return "home";
        }

        return tag.Trim().ToLowerInvariant();
    }

    private void PlayContentTransition()
    {
        if (!IsLoaded)
        {
            return;
        }

        ContentAreaHost.BeginAnimation(OpacityProperty, null);
        ContentAreaTranslate.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, null);

        var fade = new DoubleAnimation
        {
            From = 0.55,
            To = 1.0,
            Duration = TimeSpan.FromMilliseconds(220),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        var slide = new DoubleAnimation
        {
            From = 26,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(220),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        ContentAreaHost.BeginAnimation(OpacityProperty, fade, HandoffBehavior.SnapshotAndReplace);
        ContentAreaTranslate.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, slide, HandoffBehavior.SnapshotAndReplace);
    }

    private void DumpNavigationDiagnostics()
    {
        try
        {
            var lines = new List<string>
            {
                $"MenuItems.Count={TestNavigationView.MenuItems.Count}",
                $"FooterMenuItems.Count={TestNavigationView.FooterMenuItems.Count}",
                $"DisplayMode={TestNavigationView.DisplayMode}",
                $"PaneDisplayMode={TestNavigationView.PaneDisplayMode}",
                $"IsPaneOpen={TestNavigationView.IsPaneOpen}"
            };

            lines.Add($"Template={TestNavigationView.Template?.GetType().FullName ?? "<null>"}");
            lines.Add($"m_leftNavRepeater={DescribePrivateField(TestNavigationView, "m_leftNavRepeater")}");
            lines.Add($"m_topNavRepeater={DescribePrivateField(TestNavigationView, "m_topNavRepeater")}");
            lines.Add($"m_rootSplitView={DescribePrivateField(TestNavigationView, "m_rootSplitView")}");

            if (FindDescendantByName<ItemsRepeater>(TestNavigationView, "MenuItemsHost") is { } menuItemsHost)
            {
                lines.Add($"MenuItemsHost.Children.Count={menuItemsHost.Children.Count}");
                lines.Add($"MenuItemsHost.ActualWidth={menuItemsHost.ActualWidth}");
                lines.Add($"MenuItemsHost.ActualHeight={menuItemsHost.ActualHeight}");

                for (int i = 0; i < menuItemsHost.Children.Count; i++)
                {
                    if (menuItemsHost.Children[i] is not FrameworkElement child)
                    {
                        continue;
                    }

                    lines.Add($"Child[{i}]={child.GetType().FullName}; Name={child.Name}; Actual={child.ActualWidth}x{child.ActualHeight}; Visibility={child.Visibility}");

                    if (FindDescendantByName<FrameworkElement>(child, "NavigationViewItemPresenter") is { } presenter)
                    {
                        lines.Add($"Child[{i}].Presenter={presenter.GetType().FullName}; Actual={presenter.ActualWidth}x{presenter.ActualHeight}; Visibility={presenter.Visibility}");
                    }

                    if (FindDescendantByName<ContentPresenter>(child, "ContentPresenter") is { } contentPresenter)
                    {
                        lines.Add($"Child[{i}].ContentPresenter.Content={contentPresenter.Content}");
                        lines.Add($"Child[{i}].ContentPresenter.Actual={contentPresenter.ActualWidth}x{contentPresenter.ActualHeight}; Visibility={contentPresenter.Visibility}");
                    }
                }
            }
            else
            {
                lines.Add("MenuItemsHost=<null>");
            }

            string path = Path.Combine(Path.GetTempPath(), "wpfui-violeta-navigationview-diagnostics.txt");
            File.WriteAllText(path, string.Join(Environment.NewLine, lines), Encoding.UTF8);
            PushEvent($"Diagnostics saved => {path}");
        }
        catch (Exception ex)
        {
            PushEvent($"Diagnostics failed => {ex.Message}");
        }
    }

    private static T? FindDescendantByName<T>(DependencyObject root, string name) where T : FrameworkElement
    {
        int childrenCount = VisualTreeHelper.GetChildrenCount(root);

        for (int i = 0; i < childrenCount; i++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(root, i);

            if (child is T element && element.Name == name)
            {
                return element;
            }

            if (FindDescendantByName<T>(child, name) is { } match)
            {
                return match;
            }
        }

        return null;
    }

    private static string DescribePrivateField(object instance, string fieldName)
    {
        FieldInfo? field = instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        object? value = field?.GetValue(instance);

        if (value is FrameworkElement element)
        {
            return $"{element.GetType().FullName}; Name={element.Name}; Actual={element.ActualWidth}x{element.ActualHeight}; Visibility={element.Visibility}";
        }

        return value?.GetType().FullName ?? "<null>";
    }
}