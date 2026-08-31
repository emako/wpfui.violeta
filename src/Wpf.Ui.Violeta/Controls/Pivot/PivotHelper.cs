using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Wpf.Ui.Violeta.Controls.Compat;

namespace Wpf.Ui.Violeta.Controls;

public static class PivotHelper
{
    #region Title

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.RegisterAttached(
            "Title",
            typeof(object),
            typeof(PivotHelper));

    public static object GetTitle(TabControl tabControl)
    {
        return tabControl.GetValue(TitleProperty);
    }

    public static void SetTitle(TabControl tabControl, object value)
    {
        tabControl.SetValue(TitleProperty, value);
    }

    #endregion Title

    #region TitleTemplate

    public static readonly DependencyProperty TitleTemplateProperty =
        DependencyProperty.RegisterAttached(
            "TitleTemplate",
            typeof(DataTemplate),
            typeof(PivotHelper));

    public static DataTemplate GetTitleTemplate(TabControl tabControl)
    {
        return (DataTemplate)tabControl.GetValue(TitleTemplateProperty);
    }

    public static void SetTitleTemplate(TabControl tabControl, DataTemplate value)
    {
        tabControl.SetValue(TitleTemplateProperty, value);
    }

    #endregion TitleTemplate

    #region LeftHeader

    public static readonly DependencyProperty LeftHeaderProperty =
        DependencyProperty.RegisterAttached(
            "LeftHeader",
            typeof(object),
            typeof(PivotHelper));

    public static object GetLeftHeader(TabControl tabControl)
    {
        return tabControl.GetValue(LeftHeaderProperty);
    }

    public static void SetLeftHeader(TabControl tabControl, object value)
    {
        tabControl.SetValue(LeftHeaderProperty, value);
    }

    #endregion LeftHeader

    #region LeftHeaderTemplate

    public static readonly DependencyProperty LeftHeaderTemplateProperty =
        DependencyProperty.RegisterAttached(
            "LeftHeaderTemplate",
            typeof(DataTemplate),
            typeof(PivotHelper));

    public static DataTemplate GetLeftHeaderTemplate(TabControl tabControl)
    {
        return (DataTemplate)tabControl.GetValue(LeftHeaderTemplateProperty);
    }

    public static void SetLeftHeaderTemplate(TabControl tabControl, DataTemplate value)
    {
        tabControl.SetValue(LeftHeaderTemplateProperty, value);
    }

    #endregion LeftHeaderTemplate

    #region RightHeader

    public static readonly DependencyProperty RightHeaderProperty =
        DependencyProperty.RegisterAttached(
            "RightHeader",
            typeof(object),
            typeof(PivotHelper));

    public static object GetRightHeader(TabControl tabControl)
    {
        return tabControl.GetValue(RightHeaderProperty);
    }

    public static void SetRightHeader(TabControl tabControl, object value)
    {
        tabControl.SetValue(RightHeaderProperty, value);
    }

    #endregion RightHeader

    #region RightHeaderTemplate

    public static readonly DependencyProperty RightHeaderTemplateProperty =
        DependencyProperty.RegisterAttached(
            "RightHeaderTemplate",
            typeof(DataTemplate),
            typeof(PivotHelper));

    public static DataTemplate GetRightHeaderTemplate(TabControl tabControl)
    {
        return (DataTemplate)tabControl.GetValue(RightHeaderTemplateProperty);
    }

    public static void SetRightHeaderTemplate(TabControl tabControl, DataTemplate value)
    {
        tabControl.SetValue(RightHeaderTemplateProperty, value);
    }

    #endregion RightHeaderTemplate
}

/// <summary>
/// TabViewItem Properties
/// </summary>
public static class TabItemHelper
{
    private static readonly ResourceAccessor ResourceAccessor = new(typeof(TabItemHelper));

    #region IsEnabled

    public static bool GetIsEnabled(TabItem element)
    {
        return (bool)element.GetValue(IsEnabledProperty);
    }

    public static void SetIsEnabled(TabItem element, bool value)
    {
        element.SetValue(IsEnabledProperty, value);
    }

    public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
        "IsEnabled",
        typeof(bool),
        typeof(TabItemHelper),
        new PropertyMetadata(OnIsEnabledChanged));

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var item = (TabItem)d;
        if ((bool)e.NewValue)
        {
            item.Loaded += OnLoaded;
            item.SizeChanged += OnSizeChanged;
        }
        else
        {
            item.Loaded -= OnLoaded;
            item.SizeChanged -= OnSizeChanged;
            BindingOperations.ClearBinding(item, FrameworkElement.ToolTipProperty);
        }
    }

    #endregion IsEnabled

    #region Icon

    /// <summary>
    /// Sets the value for the Icon to be displayed within the tab.
    /// </summary>
    /// <param name="tabItem">The element from which to read the property value.</param>
    /// <returns>The Icon to be displayed within the tab.</returns>
    public static object GetIcon(TabItem tabItem)
    {
        return tabItem.GetValue(IconProperty);
    }

    /// <summary>
    /// Gets the value for the Icon to be displayed within the tab.
    /// </summary>
    /// <param name="tabItem">The element from which to read the property value.</param>
    /// <param name="value">The Icon to be displayed within the tab.</param>
    public static void SetIcon(TabItem tabItem, object value)
    {
        tabItem.SetValue(IconProperty, value);
    }

    /// <summary>
    /// Identifies the Icon dependency property.
    /// </summary>
    public static readonly DependencyProperty IconProperty =
        DependencyProperty.RegisterAttached(
            "Icon",
            typeof(object),
            typeof(TabItemHelper));

    #endregion Icon

    #region TabGeometry

    private static void SetTabGeometry(TabItem tabItem, object value)
    {
        tabItem.SetValue(TabGeometryProperty, value);
    }

    public static readonly DependencyProperty TabGeometryProperty =
        DependencyProperty.RegisterAttached(
            "TabGeometry",
            typeof(Geometry),
            typeof(TabItemHelper));

    #endregion TabGeometry

    #region CloseTabButtonCommand

    internal static readonly DependencyProperty CloseTabButtonCommandProperty = DependencyProperty.RegisterAttached(
        "CloseTabButtonCommand",
        typeof(ICommand),
        typeof(TabItemHelper),
        null);

    internal static void SetCloseTabButtonCommand(TabItem tabItem, ICommand value)
    {
        tabItem.SetValue(CloseTabButtonCommandProperty, value);
    }

    #endregion CloseTabButtonCommand

    #region CloseButtonOverlayMode

    public static readonly DependencyProperty CloseButtonOverlayModeProperty = DependencyProperty.RegisterAttached(
        "CloseButtonOverlayMode",
        typeof(TabViewCloseButtonOverlayMode),
        typeof(TabItemHelper),
        null);

    #endregion CloseButtonOverlayMode

    #region CloseRequestedEvent

    public static readonly RoutedEvent CloseRequestedEvent = EventManager.RegisterRoutedEvent(
        "CloseRequested",
        RoutingStrategy.Bubble,
        typeof(EventHandler<TabViewTabCloseRequestedEventArgs>),
        typeof(TabItemHelper));

    #endregion CloseRequestedEvent

    private static void OnLoaded(object? sender, RoutedEventArgs e)
    {
        TabItem TabItem = (sender as TabItem)!;
        UpdateTabGeometry(TabItem);
        UpdateHeaderTooltip(TabItem);
        UpdateCloseButtonTooltip(TabItem);
        UpdateCloseButtonEvents(TabItem);

        TabControl TabControl = TabItem.FindAscendant<TabControl>();

        if (TabControl != null)
        {
            TabItem.SetBinding(CloseButtonOverlayModeProperty, new Binding
            {
                Source = TabControl,
                Mode = BindingMode.OneWay,
                Path = new PropertyPath(TabControlHelper.CloseButtonOverlayModeProperty)
            });
        }
    }

    private static void UpdateHeaderTooltip(TabItem TabItem)
    {
        if (TabItem.ToolTip is null && TabItem.GetTemplateChild<FrameworkElement>("TabContainer") is { } headerContainer)
        {
            headerContainer.SetBinding(
                FrameworkElement.ToolTipProperty,
                new Binding
                {
                    Path = new PropertyPath(HeaderedContentControl.HeaderProperty),
                    RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent),
                    Mode = BindingMode.OneWay,
                    Converter = TabItem.TryFindResource("TabItemHeaderConverter") as IValueConverter
                });
        }
    }

    private static readonly RoutedCommand CloseTabButtonCommand = new()
    {
        InputGestures = { new KeyGesture(Key.F4, ModifierKeys.Control) }
    };

    private static void UpdateCloseButtonEvents(TabItem item)
    {
        TabControl tabControl = item.FindAscendant<TabControl>();

        void ExecutedCustomCommand(object? sender, ExecutedRoutedEventArgs e)
        {
            var eargs = new TabViewTabCloseRequestedEventArgs(TabControlHelper.TabCloseRequestedEvent, item.Content, item);
            tabControl.RaiseEvent(eargs);

            // According to WinUI 3 behavior, the TabView's CloseRequested will be fired first,
            // then the TabItem's CloseRequested will be fired after that.
            // Since WinUI 3 does not have a 'routed' event for TabItem CloseRequested, we may apply
            // the same logic here, but adopting a handled check for TabItem CloseRequested event.
            // If this is inappropriate, feel free to propose a change.
            if (!eargs.Handled)
            {
                item.RaiseEvent(new TabViewTabCloseRequestedEventArgs(CloseRequestedEvent, item.Content, item));
            }

            e.Handled = true;
        }

        void CanExecuteCustomCommand(object? sender, CanExecuteRoutedEventArgs e)
        {
            if (tabControl != null)
            {
                e.CanExecute = true;
            }
            else
            {
                e.CanExecute = false;
            }
            e.Handled = true;
        }

        CommandBinding closeTabButtonCommandBinding = new(CloseTabButtonCommand, ExecutedCustomCommand, CanExecuteCustomCommand);
        item.CommandBindings.Add(closeTabButtonCommandBinding);
        SetCloseTabButtonCommand(item, CloseTabButtonCommand);

        // Cleanup previous bindings
        foreach (var binding in item.CommandBindings)
        {
            if (binding is CommandBinding cmb && cmb.Command == CloseTabButtonCommand
                && cmb != closeTabButtonCommandBinding)
            {
                item.CommandBindings.Remove(cmb);
                break;
            }
        }
    }

    private static void UpdateCloseButtonTooltip(TabItem item)
    {
        if (item?.GetTemplateChild<FrameworkElement>("CloseButton") is not { } closeButton)
        {
            return;
        }

        closeButton.ToolTip =
            ResourceAccessor.GetLocalizedStringResource(ResourceAccessor.SR_TabViewCloseButtonTooltipWithKA);
    }

    private static void OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        UpdateTabGeometry((sender as TabItem)!);
    }

    private static void UpdateTabGeometry(TabItem tabItem)
    {
        try
        {
            var scaleFactor = 1.5;
#if NET462_OR_NEWER
            scaleFactor = VisualTreeHelper.GetDpi(tabItem).DpiScaleX;
#else
            HwndSource hwnd = (HwndSource)PresentationSource.FromVisual(tabItem);
            Matrix transformToDevice = hwnd.CompositionTarget.TransformToDevice;
            scaleFactor = transformToDevice.M11;
#endif
            var height = tabItem.ActualHeight;
            var popupRadius = (CornerRadius)tabItem.GetValue(Border.CornerRadiusProperty);
            var leftCorner = popupRadius.TopLeft;
            var rightCorner = popupRadius.TopRight;

            // Assumes 4px curving-out corners, which are hardcoded in the markup
            //var data = $"F1 M0,{height - 1f / scaleFactor}  a 4,4 0 0 0 4,-4  L 4,{leftCorner}  a {leftCorner},{leftCorner} 0 0 1 {leftCorner},-{leftCorner}  l {tabItem.ActualWidth - (leftCorner + rightCorner + 1.0f / scaleFactor)},0  a {rightCorner},{rightCorner} 0 0 1 {rightCorner},{rightCorner}  l 0,{height - (4 + rightCorner + 1.0f / scaleFactor)}  a 4,4 0 0 0 4,4 Z";
            var data = $"F1 M0,{Math.Round(height - 1f / scaleFactor)}  a 4,4 0 0 0 4,-4  L 4,{leftCorner}  a {leftCorner},{leftCorner} 0 0 1 {leftCorner},-{leftCorner}  l {Math.Round(tabItem.ActualWidth - (leftCorner + rightCorner + 1.0f / scaleFactor))},0  a {rightCorner},{rightCorner} 0 0 1 {rightCorner},{rightCorner}  l 0,{Math.Round(height - (4 + rightCorner + 1.0f / scaleFactor))}  a 4,4 0 0 0 4,4 Z";

            var geometry = Geometry.Parse(data);

            SetTabGeometry(tabItem, geometry);
        }
        catch { }
    }
}

/// <summary>
/// TabView Properties
/// </summary>
public static class TabControlHelper
{
    #region IsEnabled

    public static bool GetIsEnabled(TabControl element)
    {
        return (bool)element.GetValue(IsEnabledProperty);
    }

    public static void SetIsEnabled(TabControl element, bool value)
    {
        element.SetValue(IsEnabledProperty, value);
    }

    public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
        "IsEnabled",
        typeof(bool),
        typeof(TabControlHelper),
        new PropertyMetadata(OnIsEnabledChanged));

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var item = (TabControl)d;
        if ((bool)e.NewValue)
        {
            item.Loaded += OnLoaded;
        }
        else
        {
            item.Loaded -= OnLoaded;
        }
    }

    #endregion IsEnabled

    #region CloseButtonOverlayMode

    /// <summary>
    /// Identifies the CloseButtonOverlayMode dependency property.
    /// </summary>
    public static readonly DependencyProperty CloseButtonOverlayModeProperty = DependencyProperty.RegisterAttached(
        "CloseButtonOverlayMode",
        typeof(TabViewCloseButtonOverlayMode),
        typeof(TabControlHelper),
        new PropertyMetadata(TabViewCloseButtonOverlayMode.Auto));

    #endregion CloseButtonOverlayMode

    #region TabCloseRequestedEvent

    /// <summary>
    /// The event is raised when a tab's close button is clicked.
    ///
    /// </summary>
    public static readonly RoutedEvent TabCloseRequestedEvent = EventManager.RegisterRoutedEvent(
        "TabCloseRequested",
        RoutingStrategy.Direct,
        typeof(EventHandler<TabViewTabCloseRequestedEventArgs>),
        typeof(TabControlHelper));

    #endregion TabCloseRequestedEvent

    #region AddTabButtonClickEvent

    public static readonly RoutedEvent AddTabButtonClickEvent = EventManager.RegisterRoutedEvent(
        "AddTabButtonClick",
        RoutingStrategy.Direct,
        typeof(RoutedEventHandler),
        typeof(TabControlHelper));

    #endregion AddTabButtonClickEvent

    private static void OnLoaded(object? sender, RoutedEventArgs e)
    {
        TabControl? TabControl = sender as TabControl;
        Button? AddButton = (Button?)TabControl?.FindDescendantByName("AddButton");

        if (AddButton != null)
        {
            void OnAddButtonClick(object? sender, RoutedEventArgs e)
            {
                RoutedEventArgs args = new(AddTabButtonClickEvent, TabControl);
                TabControl?.RaiseEvent(args);
            }
            AddButton.Click += OnAddButtonClick;
        }
    }
}

/// <summary>
/// Defines constants that describe the behavior of the close button contained within each <see cref="TabItem"/>.
/// </summary>
public enum TabViewCloseButtonOverlayMode
{
    /// <summary>
    /// Behavior is defined by the framework. Default.
    /// This value maps to Always.
    /// </summary>
    Auto = 0,

    /// <summary>
    /// The selected tab always shows the close button if it is closable. Unselected tabs show the close button when the tab is closable and the user has their pointer over the tab.
    /// </summary>
    OnPointerOver = 1,

    /// <summary>
    /// The selected tab always shows the close button if it is closable. Unselected tabs always show the close button if they are closable.
    /// </summary>
    Always = 2,
}

/// <summary>
/// Provides data for a tab close event.
/// </summary>
public class TabViewTabCloseRequestedEventArgs : RoutedEventArgs
{
    /// <summary>
    /// Gets a value that represents the data context for the tab in which a close is being requested.
    /// </summary>
    public object Item { get; private set; }

    /// <summary>
    /// Gets the tab in which a close is being requested.
    /// </summary>
    public TabItem Tab { get; private set; }

    internal TabViewTabCloseRequestedEventArgs(RoutedEvent routedEvent, object item, TabItem tab)
        : base(routedEvent)
    {
        Item = item;
        Tab = tab;
    }
}
