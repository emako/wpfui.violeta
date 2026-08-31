using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using LiteObservableLanguages;
using Wpf.Ui.Appearance;
using Wpf.Ui.Violeta.Appearance;
using Wpf.Ui.Violeta.Controls;
using Wpf.Ui.Violeta.Controls.Compat;
using Wpf.Ui.Violeta.Gallery.Globalization;
using Wpf.Ui.Violeta.Gallery.Pages.AllSamples;
using Wpf.Ui.Violeta.Gallery.Pages.BasicInput;
using Wpf.Ui.Violeta.Gallery.Pages.Buttons;
using Wpf.Ui.Violeta.Gallery.Pages.Collections;
using Wpf.Ui.Violeta.Gallery.Pages.ComboBoxes;
using Wpf.Ui.Violeta.Gallery.Pages.DateTime;
using Wpf.Ui.Violeta.Gallery.Pages.Design;
using Wpf.Ui.Violeta.Gallery.Pages.Dialogs;
using Wpf.Ui.Violeta.Gallery.Pages.Feedback;
using Wpf.Ui.Violeta.Gallery.Pages.Home;
using Wpf.Ui.Violeta.Gallery.Pages.Layout;
using Wpf.Ui.Violeta.Gallery.Pages.Media;
using Wpf.Ui.Violeta.Gallery.Pages.Navigation;
using Wpf.Ui.Violeta.Gallery.Pages.Notifications;
using Wpf.Ui.Violeta.Gallery.Pages.NumberInput;
using Wpf.Ui.Violeta.Gallery.Pages.OpSystem;
using Wpf.Ui.Violeta.Gallery.Pages.Pickers;
using Wpf.Ui.Violeta.Gallery.Pages.Selection;
using Wpf.Ui.Violeta.Gallery.Pages.Selectors;
using Wpf.Ui.Violeta.Gallery.Pages.Settings;
using Wpf.Ui.Violeta.Gallery.Pages.Sliders;
using Wpf.Ui.Violeta.Gallery.Pages.Status;
using Wpf.Ui.Violeta.Gallery.Pages.TagInput;
using Wpf.Ui.Violeta.Gallery.Pages.Text;
using Wpf.Ui.Violeta.Gallery.Pages.TextDisplay;
using Wpf.Ui.Violeta.Gallery.Pages.TextInput;
using Wpf.Ui.Violeta.Gallery.Pages.Windows;
using Wpf.Ui.Violeta.Win32;

namespace Wpf.Ui.Violeta.Gallery;

public partial class MainWindow : ShellWindow
{
    private readonly Dictionary<string, Wpf.Ui.Violeta.Controls.Page> _pageCache = [];
    private bool _syncingSelection;
    private bool _syncingLanguage;

    private static readonly Dictionary<string, Func<Wpf.Ui.Violeta.Controls.Page>> PageFactories = new()
    {
        ["home"] = static () => new HomePage(),
        ["all"] = static () => new AllSamplesPage(),
        ["all-samples"] = static () => new AllSamplesPage(),

        ["design"] = static () => new DesignPage(),
        ["design/typography"] = static () => new TypographyPage(),
        ["design/icons"] = static () => new IconsPage(),
        ["design/colors"] = static () => new ColorsPage(),
        ["design/theme-color-approach"] = static () => new ThemeColorApproachPage(),
        ["design/theme-refresh-converter"] = static () => new ThemeRefreshConverterPage(),

        // Group overview pages (by control family)
        ["buttons"] = static () => new ButtonsPage(),
        ["selection"] = static () => new SelectionPage(),
        ["sliders"] = static () => new SlidersPage(),
        ["number-input"] = static () => new NumberInputPage(),
        ["text-input"] = static () => new TextInputPage(),
        ["text-display"] = static () => new TextDisplayPage(),
        ["combo-box"] = static () => new ComboBoxesPage(),
        ["pickers"] = static () => new PickersPage(),

        // Legacy group tags → new overview pages
        ["basic-input"] = static () => new ButtonsPage(),
        ["text"] = static () => new TextInputPage(),
        ["selectors"] = static () => new ComboBoxesPage(),
        ["date-time"] = static () => new PickersPage(),

        ["basic-input/button"] = static () => new ButtonPage(),
        ["basic-input/repeat-button"] = static () => new RepeatButtonPage(),
        ["basic-input/drop-down-button"] = static () => new DropDownButtonPage(),
        ["basic-input/hyperlink-button"] = static () => new HyperlinkButtonPage(),
        ["basic-input/toggle-button"] = static () => new ToggleButtonPage(),
        ["basic-input/check-box"] = static () => new CheckBoxPage(),
        ["basic-input/combo-box"] = static () => new ComboBoxPage(),
        ["basic-input/radio-button"] = static () => new RadioButtonPage(),
        ["basic-input/rating"] = static () => new RatingPage(),
        ["basic-input/thumb-rate"] = static () => new ThumbRatePage(),
        ["basic-input/split-button"] = static () => new SplitButtonPage(),
        ["basic-input/toggle-combo-box"] = static () => new ToggleComboBoxPage(),
        ["basic-input/slider"] = static () => new SliderPage(),
        ["basic-input/loading-button"] = static () => new LoadingButtonPage(),
        ["basic-input/copy-button"] = static () => new CopyButtonPage(),
        ["basic-input/go-back-button"] = static () => new GoBackButtonPage(),
        ["basic-input/animated-symbol-button"] = static () => new AnimatedSymbolButtonPage(),
        ["basic-input/icon-toggle-button"] = static () => new IconToggleButtonPage(),
        ["basic-input/toggle-switch"] = static () => new ToggleSwitchPage(),
        ["basic-input/numeric-up-down"] = static () => new NumericUpDownPage(),
        ["basic-input/number-combo-box"] = static () => new NumberComboBoxPage(),
        ["basic-input/button-spinner"] = static () => new ButtonSpinnerPage(),
        ["basic-input/range-slider"] = static () => new RangeSliderPage(),
        ["basic-input/virtual-joystick"] = static () => new VirtualJoystickPage(),
        ["basic-input/key-gesture-input"] = static () => new KeyGestureInputPage(),
        ["basic-input/pin-code"] = static () => new PinCodePage(),
        ["basic-input/toggle-button-group"] = static () => new ToggleButtonGroupPage(),
        ["basic-input/ipv4-box"] = static () => new IPv4BoxPage(),
        ["basic-input/ipv4-port-box"] = static () => new IPv4PortBoxPage(),

        ["text/textbox"] = static () => new TextBoxPage(),
        ["text/auto-suggest-box"] = static () => new AutoSuggestBoxPage(),
        ["text/search-box"] = static () => new SearchBoxPage(),
        ["text/number-box"] = static () => new NumberBoxPage(),
        ["text/password-box"] = static () => new PasswordBoxPage(),
        ["text/rich-text-box"] = static () => new RichTextBoxPage(),
        ["text/label"] = static () => new LabelPage(),
        ["text/text-block"] = static () => new TextBlockPage(),
        ["text/selectable-text-block"] = static () => new SelectableTextBlockPage(),
        ["text/number-displayer"] = static () => new NumberDisplayerPage(),
        ["text/hyperlink"] = static () => new HyperlinkPage(),
        ["text/bool-state-text-block"] = static () => new BoolStateTextBlockPage(),
        ["text/emoji"] = static () => new EmojiPage(),

        ["selectors/multi-combo-box"] = static () => new MultiComboBoxPage(),
        ["selectors/cascading-combo-box"] = static () => new CascadingComboBoxPage(),
        ["selectors/tag-combo-box"] = static () => new TagComboBoxPage(),
        ["selectors/tree-combo-box"] = static () => new TreeComboBoxPage(),
        ["selectors/value-picker"] = static () => new ValuePickerPage(),
        ["selectors/color-picker"] = static () => new ColorPickerPage(),
        ["selectors/swatch-picker"] = static () => new SwatchPickerPage(),

        ["tag-input"] = static () => new TagInputPage(),

        ["date-time/date-picker"] = static () => new DatePickerPage(),
        ["date-time/time-picker"] = static () => new TimePickerPage(),
        ["date-time/calendar-date-picker"] = static () => new CalendarDatePickerPage(),
        ["date-time/calendar-date-time-picker"] = static () => new CalendarDateTimePickerPage(),
        ["date-time/time-box-picker"] = static () => new TimeBoxPickerPage(),
        ["date-time/time-box"] = static () => new TimeBoxPage(),
        ["date-time/calendar"] = static () => new CalendarPage(),

        ["dialogs"] = static () => new DialogsPage(),
        ["dialogs/content-dialog"] = static () => new ContentDialogPage(),
        ["dialogs/content-window-dialog"] = static () => new ContentWindowDialogControlPage(),
        ["dialogs/message-box"] = static () => new MessageBoxPage(),
        ["dialogs/pending-box"] = static () => new PendingBoxPage(),
        ["dialogs/task-dialog"] = static () => new TaskDialogPage(),
        ["dialogs/native-message-box"] = static () => new NativeMessageBoxPage(),
        ["dialogs/credential-dialog"] = static () => new CredentialDialogPage(),
        ["dialogs/open-folder-dialog"] = static () => new OpenFolderDialogPage(),
        ["dialogs/flyout"] = static () => new FlyoutPage(),
        ["dialogs/fluent-popup"] = static () => new FluentPopupPage(),
        ["dialogs/teaching-tip"] = static () => new TeachingTipPage(),

        ["notifications"] = static () => new NotificationsPage(),
        ["notifications/snackbar"] = static () => new SnackbarPage(),
        ["notifications/toast"] = static () => new ToastPage(),
        ["notifications/growl"] = static () => new GrowlPage(),
        ["notifications/banner"] = static () => new BannerPage(),
        ["notifications/notification"] = static () => new NotificationPage(),
        ["notifications/tray-icon"] = static () => new TrayIconPage(),

        ["collections"] = static () => new CollectionsPage(),
        ["collections/data-grid"] = static () => new DataGridPage(),
        ["collections/list-box"] = static () => new ListBoxPage(),
        ["collections/list-view"] = static () => new ListViewPage(),
        ["collections/grid-view"] = static () => new GridViewPage(),
        ["collections/tree-view"] = static () => new TreeViewPage(),
        ["collections/tree-list-view"] = static () => new TreeListViewPage(),
        ["collections/tree-model-list-view"] = static () => new TreeModelListViewPage(),
        ["collections/flip-view"] = static () => new FlipViewPage(),
        ["collections/carousel"] = static () => new CarouselPage(),
        ["collections/card-carousel"] = static () => new CardCarouselPage(),
        ["collections/pagination"] = static () => new PaginationPage(),
        ["collections/timeline"] = static () => new TimelinePage(),
        ["collections/drag-drop"] = static () => new DragDropPage(),

        ["navigation"] = static () => new NavigationPage(),
        ["navigation/navigation-view"] = static () => new NavigationViewPage(),
        ["navigation/breadcrumb-bar"] = static () => new BreadcrumbBarPage(),
        ["navigation/menu"] = static () => new MenuPage(),
        ["navigation/tool-bar"] = static () => new ToolBarPage(),
        ["navigation/tab-control"] = static () => new TabControlPage(),
        ["navigation/tab-view"] = static () => new TabViewPage(),
        ["navigation/tab-strip"] = static () => new TabStripPage(),
        ["navigation/segmented"] = static () => new SegmentedPage(),
        ["basic-input/segmented"] = static () => new SegmentedPage(),
        ["navigation/pivot"] = static () => new PivotPage(),
        ["navigation/anchor"] = static () => new AnchorPage(),

        ["layout"] = static () => new LayoutPage(),
        ["layout/card-control"] = static () => new CardControlPage(),
        ["layout/card-border"] = static () => new CardBorderPage(),
        ["layout/card-action"] = static () => new CardActionPage(),
        ["layout/card-progress"] = static () => new CardProgressPage(),
        ["layout/border-beam"] = static () => new BorderBeamPage(),
        ["layout/tile-press-feedback"] = static () => new TilePressFeedbackPage(),
        ["layout/grid"] = static () => new GridPage(),
        ["layout/auto-grid"] = static () => new AutoGridPage(),
        ["layout/flex-panel"] = static () => new FlexPanelPage(),
        ["layout/border"] = static () => new BorderPage(),
        ["layout/drop-shadow-chrome"] = static () => new DropShadowChromePage(),
        ["layout/aspect-ratio-layout"] = static () => new AspectRatioLayoutPage(),
        ["layout/form"] = static () => new FormPage(),
        ["layout/descriptions"] = static () => new DescriptionsPage(),
        ["layout/grid-splitter"] = static () => new GridSplitterPage(),
        ["layout/divider"] = static () => new DividerPage(),
        ["layout/drawer"] = static () => new DrawerPage(),
        ["layout/expander"] = static () => new ExpanderPage(),
        ["layout/fluent-scroll-viewer"] = static () => new FluentScrollViewerPage(),

        ["status"] = static () => new StatusPage(),
        ["status/info-badge"] = static () => new InfoBadgePage(),
        ["status/info-bar"] = static () => new InfoBarPage(),
        ["status/progress-bar"] = static () => new ProgressBarPage(),
        ["status/progress-ring"] = static () => new ProgressRingPage(),
        ["status/badge"] = static () => new BadgePage(),
        ["status/shield"] = static () => new ShieldPage(),
        ["status/volume-view"] = static () => new VolumeViewPage(),
        ["status/skeleton"] = static () => new SkeletonPage(),
        ["status/busy-mask"] = static () => new BusyMaskPage(),
        ["status/bool-state-content-control"] = static () => new BoolStateContentControlPage(),
        ["status/tool-tip"] = static () => new ToolTipPage(),
        ["status/status-bar"] = static () => new StatusBarPage(),
        ["status/empty-simple-logo"] = static () => new EmptySimpleLogoPage(),

        ["media"] = static () => new MediaPage(),
        ["media/image"] = static () => new ImagePage(),
        ["media/image-view"] = static () => new ImageViewPage(),
        ["media/canvas"] = static () => new CanvasPage(),
        ["media/web-browser"] = static () => new WebBrowserPage(),
        ["media/web-view2"] = static () => new WebView2Page(),
        ["media/person-picture"] = static () => new PersonPicturePage(),
        ["media/gravatar"] = static () => new GravatarPage(),
        ["media/bitmap-icon"] = static () => new BitmapIconPage(),
        ["media/qr-code"] = static () => new QrCodePage(),
        ["media/cached-image"] = static () => new CachedImagePage(),

        ["feedback"] = static () => new FeedbackPage(),
        ["feedback/transitioning-content-control"] = static () => new TransitioningContentControlPage(),
        ["feedback/running-block"] = static () => new RunningBlockPage(),
        ["feedback/async-box"] = static () => new AsyncBoxPage(),
        ["feedback/splash"] = static () => new SplashPage(),
        ["feedback/exception-report"] = static () => new ExceptionReportPage(),

        ["windows"] = static () => new WindowsPage(),
        ["windows/shell-window"] = static () => new ShellWindowPage(),
        ["windows/content-window"] = static () => new ContentWindowPage(),
        ["windows/title-bar"] = static () => new TitleBarPage(),
        ["windows/caption-button-bar"] = static () => new CaptionButtonBarPage(),

        ["op-system"] = static () => new OpSystemPage(),
        ["op-system/clipboard"] = static () => new ClipboardPage(),
        ["op-system/file-picker"] = static () => new FilePickerPage(),

        ["settings"] = static () => new SettingsPage(),
    };

    public MainWindow()
    {
        InitializeComponent();
        GalleryNavigator.NavigateRequested = OnNavigateRequested;
        Loaded += MainWindow_OnLoaded;
        Closing += MainWindow_OnClosing;
        Locale.Default.CultureChanged += OnCultureChanged;
    }

    private void MainWindow_OnClosing(object? sender, CancelEventArgs e)
    {
        if (TrayIconManager.IsExitRequested || !TrayIconManager.MinimizeToTrayOnClose)
        {
            return;
        }

        e.Cancel = true;
        Hide();
        TrayIconManager.ShowNotification(
            LangKeys.Gallery_AppTitle.Tr(),
            LangKeys.Gallery_Tray_MinimizedMessage.Tr(),
            ToolTipIcon.Info);
    }

    private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        SyncLanguageComboBox(LocaleManager.Language);
        GalleryNav.SelectedItem = HomeItem;
    }

    private void OnCultureChanged(object? sender, CultureInfo culture)
    {
        Dispatcher.Invoke(RefreshNavigationHeader);
    }

    private void RefreshNavigationHeader()
    {
        if (GalleryNav.SelectedItem is NavigationViewItem item)
        {
            GalleryNav.Header = item.Content?.ToString() ?? string.Empty;
        }
        else if (ContentFrame.Content is SettingsPage)
        {
            GalleryNav.Header = LangKeys.Gallery_Settings.Tr();
        }
    }

    private void OnNavigateRequested(string? tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
        {
            return;
        }

        var item = FindMenuItemByTag(GalleryNav.MenuItems, tag);
        if (item is not null)
        {
            _syncingSelection = true;
            try
            {
                GalleryNav.SelectedItem = item;
                GalleryNav.Header = item.Content?.ToString() ?? tag;
            }
            finally
            {
                _syncingSelection = false;
            }
        }
        else
        {
            GalleryNav.Header = tag;
        }

        NavigateTo(tag);
    }

    private void GalleryNav_OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (_syncingSelection)
        {
            return;
        }

        if (args.IsSettingsSelected)
        {
            GalleryNav.Header = LangKeys.Gallery_Settings.Tr();
            NavigateTo("settings");
            return;
        }

        if (args.SelectedItem is not NavigationViewItem item)
        {
            return;
        }

        var tag = item.Tag as string;
        GalleryNav.Header = item.Content?.ToString() ?? string.Empty;
        NavigateTo(tag);
    }

    private void GalleryNav_OnBackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
    {
        if (ContentFrame.CanGoBack)
        {
            ContentFrame.GoBack();
            UpdateBackButtonState();
        }
    }

    private void GalleryTitleBar_OnBackButtonClick(object? sender, EventArgs e)
    {
        if (ContentFrame.CanGoBack)
        {
            ContentFrame.GoBack();
            UpdateBackButtonState();
        }
    }

    private void GalleryTitleBar_OnPaneToggleButtonClick(object? sender, EventArgs e)
    {
        GalleryNav.IsPaneOpen = !GalleryNav.IsPaneOpen;
    }

    private void UpdateBackButtonState()
    {
        GalleryNav.IsBackEnabled = ContentFrame.CanGoBack;
        GalleryTitleBar.IsBackButtonEnabled = ContentFrame.CanGoBack;
    }

    private void NavigateTo(string? tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
        {
            tag = "home";
        }

        if (!PageFactories.TryGetValue(tag, out var factory))
        {
            tag = "home";
            factory = PageFactories["home"];
        }

        var page = GetOrCreate(tag, factory);
        ContentFrame.Navigate(page, new EntranceNavigationTransitionInfo());
        UpdateBackButtonState();
    }

    private Wpf.Ui.Violeta.Controls.Page GetOrCreate(string key, Func<Wpf.Ui.Violeta.Controls.Page> factory)
    {
        if (!_pageCache.TryGetValue(key, out var page))
        {
            page = factory();
            _pageCache[key] = page;
        }

        return page;
    }

    private static NavigationViewItem? FindMenuItemByTag(System.Collections.IEnumerable? items, string tag)
    {
        if (items is null)
        {
            return null;
        }

        foreach (var obj in items)
        {
            if (obj is not NavigationViewItem item)
            {
                continue;
            }

            if (string.Equals(item.Tag as string, tag, StringComparison.OrdinalIgnoreCase))
            {
                return item;
            }

            var child = FindMenuItemByTag(item.MenuItems, tag);
            if (child is not null)
            {
                return child;
            }
        }

        return null;
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
            ThemeManager.ApplySystemTheme();
        }
        else
        {
            ThemeManager.Apply(theme);
        }
    }

    private void LanguageComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded || _syncingLanguage)
        {
            return;
        }

        if (LanguageComboBox.SelectedItem is not ComboBoxItem { Tag: string language })
        {
            return;
        }

        LocaleManager.SetLanguage(language);
    }

    internal int ThemeComboBoxSelectedIndex => ThemeComboBox.SelectedIndex;

    internal string LanguageComboBoxSelectedTag =>
        LanguageComboBox.SelectedItem is ComboBoxItem { Tag: string tag } ? tag : LocaleManager.Language;

    /// <summary>
    /// Keeps the title-bar theme combo in sync when theme is changed from Settings.
    /// </summary>
    internal void SyncThemeComboBox(int selectedIndex)
    {
        if (ThemeComboBox.SelectedIndex == selectedIndex)
        {
            return;
        }

        ThemeComboBox.SelectedIndex = selectedIndex;
    }

    /// <summary>
    /// Keeps the title-bar language combo in sync when language is changed from Settings.
    /// </summary>
    internal void SyncLanguageComboBox(string language)
    {
        _syncingLanguage = true;
        try
        {
            for (var i = 0; i < LanguageComboBox.Items.Count; i++)
            {
                if (LanguageComboBox.Items[i] is ComboBoxItem { Tag: string tag }
                    && string.Equals(tag, language, StringComparison.OrdinalIgnoreCase))
                {
                    LanguageComboBox.SelectedIndex = i;
                    return;
                }
            }
        }
        finally
        {
            _syncingLanguage = false;
        }
    }
}
