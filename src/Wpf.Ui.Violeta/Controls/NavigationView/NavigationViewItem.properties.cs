using System.Collections;
using System.Windows;
using ModernWpf.Controls.Primitives;
using Wpf.Ui.Controls;

namespace Wpf.Ui.Violeta.Controls;

partial class NavigationViewItem
{
    #region Icon

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(
            nameof(Icon),
            typeof(IconElement),
            typeof(NavigationViewItem),
            new PropertyMetadata(OnIconPropertyChanged));

    public IconElement Icon
    {
        get => (IconElement)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    private static void OnIconPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var owner = (NavigationViewItem)sender;
        owner.OnIconPropertyChanged(args);
    }

    #endregion Icon

    #region CompactPaneLength

    private static readonly DependencyPropertyKey CompactPaneLengthPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(CompactPaneLength),
            typeof(double),
            typeof(NavigationViewItem),
            new PropertyMetadata(48.0));

    public static readonly DependencyProperty CompactPaneLengthProperty =
        CompactPaneLengthPropertyKey.DependencyProperty;

    public double CompactPaneLength
    {
        get => (double)GetValue(CompactPaneLengthProperty);
        private set => SetValue(CompactPaneLengthPropertyKey, value);
    }

    #endregion CompactPaneLength

    #region SelectsOnInvoked

    public static readonly DependencyProperty SelectsOnInvokedProperty =
        DependencyProperty.Register(
            nameof(SelectsOnInvoked),
            typeof(bool),
            typeof(NavigationViewItem),
            new PropertyMetadata(true));

    public bool SelectsOnInvoked
    {
        get => (bool)GetValue(SelectsOnInvokedProperty);
        set => SetValue(SelectsOnInvokedProperty, value);
    }

    #endregion SelectsOnInvoked

    #region IsExpanded

    public static readonly DependencyProperty IsExpandedProperty =
        DependencyProperty.Register(
            nameof(IsExpanded),
            typeof(bool),
            typeof(NavigationViewItem),
            new PropertyMetadata(false, OnIsExpandedPropertyChanged));

    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    internal event DependencyPropertyChangedCallback IsExpandedChanged;

    private static void OnIsExpandedPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var owner = (NavigationViewItem)sender;
        owner.OnIsExpandedPropertyChanged(args);
        owner.IsExpandedChanged?.Invoke(owner, args.Property);
    }

    #endregion IsExpanded

    #region HasUnrealizedChildren

    public static readonly DependencyProperty HasUnrealizedChildrenProperty =
        DependencyProperty.Register(
            nameof(HasUnrealizedChildren),
            typeof(bool),
            typeof(NavigationViewItem),
            new PropertyMetadata(false, OnHasUnrealizedChildrenPropertyChanged));

    public bool HasUnrealizedChildren
    {
        get => (bool)GetValue(HasUnrealizedChildrenProperty);
        set => SetValue(HasUnrealizedChildrenProperty, value);
    }

    private static void OnHasUnrealizedChildrenPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var owner = (NavigationViewItem)sender;
        owner.OnHasUnrealizedChildrenPropertyChanged(args);
    }

    #endregion HasUnrealizedChildren

    #region IsChildSelected

    public static readonly DependencyProperty IsChildSelectedProperty =
        DependencyProperty.Register(
            nameof(IsChildSelected),
            typeof(bool),
            typeof(NavigationViewItem),
            new PropertyMetadata(false));

    public bool IsChildSelected
    {
        get => (bool)GetValue(IsChildSelectedProperty);
        set => SetValue(IsChildSelectedProperty, value);
    }

    #endregion IsChildSelected

    #region MenuItems

    private static readonly DependencyPropertyKey MenuItemsPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(MenuItems),
            typeof(IList),
            typeof(NavigationViewItem),
            new PropertyMetadata(OnMenuItemsPropertyChanged));

    public static readonly DependencyProperty MenuItemsProperty =
        MenuItemsPropertyKey.DependencyProperty;

    public IList MenuItems
    {
        get => (IList)GetValue(MenuItemsProperty);
    }

    private static void OnMenuItemsPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var owner = (NavigationViewItem)sender;
        owner.OnMenuItemsPropertyChanged(args);
    }

    #endregion MenuItems

    #region MenuItemsSource

    public static readonly DependencyProperty MenuItemsSourceProperty =
        DependencyProperty.Register(
            nameof(MenuItemsSource),
            typeof(object),
            typeof(NavigationViewItem),
            new PropertyMetadata(OnMenuItemsSourcePropertyChanged));

    public object MenuItemsSource
    {
        get => GetValue(MenuItemsSourceProperty);
        set => SetValue(MenuItemsSourceProperty, value);
    }

    private static void OnMenuItemsSourcePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var owner = (NavigationViewItem)sender;
        owner.OnMenuItemsSourcePropertyChanged(args);
    }

    #endregion MenuItemsSource

    #region CornerRadius

    public static readonly DependencyProperty CornerRadiusProperty =
        ControlHelper.CornerRadiusProperty.AddOwner(typeof(NavigationViewItem));

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    #endregion CornerRadius
}
