using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Wpf.Ui.Appearance;
using Wpf.Ui.Violeta.Controls;
using Wpf.Ui.Violeta.Controls.Compat;
using Wpf.Ui.Violeta.Gallery.Pages;
using Page = Wpf.Ui.Violeta.Controls.Compat.Page;

namespace Wpf.Ui.Violeta.Gallery;

public partial class MainWindow : ShellWindow
{
    private readonly Dictionary<string, Page> _pageCache = [];

    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_OnLoaded;
    }

    private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        GalleryNav.SelectedItem = HomeItem;
    }

    private void GalleryNav_OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is not NavigationViewItem item)
        {
            return;
        }

        var tag = item.Tag as string;
        var header = item.Content?.ToString() ?? string.Empty;
        GalleryNav.Header = header;
        GalleryNav.IsBackEnabled = ContentFrame.CanGoBack;
        NavigateTo(tag);
    }

    private void GalleryNav_OnBackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
    {
        if (ContentFrame.CanGoBack)
        {
            ContentFrame.GoBack();
            GalleryNav.IsBackEnabled = ContentFrame.CanGoBack;
        }
    }

    private void NavigateTo(string? tag)
    {
        Page page = tag switch
        {
            "home" => GetOrCreate("home", () => new HomePage()),
            "toast" => GetOrCreate("toast", () => new ToastPage()),
            "dialog" => GetOrCreate("dialog", () => new DialogPage()),
            "input" => GetOrCreate("input", () => new InputPage()),
            "data" => GetOrCreate("data", () => new DataPage()),
            "feedback" => GetOrCreate("feedback", () => new FeedbackPage()),
            "layout" => GetOrCreate("layout", () => new LayoutPage()),
            _ => GetOrCreate("home", () => new HomePage()),
        };

        ContentFrame.Navigate(page, new EntranceNavigationTransitionInfo());
    }

    private Page GetOrCreate(string key, Func<Page> factory)
    {
        if (!_pageCache.TryGetValue(key, out var page))
        {
            page = factory();
            _pageCache[key] = page;
        }

        return page;
    }

    private void ThemeComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var theme = ThemeComboBox.SelectedIndex switch
        {
            0 => ApplicationTheme.Unknown,
            1 => ApplicationTheme.Dark,
            2 => ApplicationTheme.Light,
            _ => ApplicationTheme.Dark,
        };

        if (theme == ApplicationTheme.Unknown)
        {
            ApplicationThemeManager.ApplySystemTheme();
        }
        else
        {
            ApplicationThemeManager.Apply(theme);
        }
    }
}
