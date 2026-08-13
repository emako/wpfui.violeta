using System;
using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Violeta.Controls.Compat;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Provides the infrastructure for the <see cref="GridView"/> and related list controls
/// (WinUI-style ItemClick, Header/Footer, multi-select checkbox).
/// </summary>
public class ListViewBase : ListBox
{
    static ListViewBase()
    {
        SelectionModeProperty.OverrideMetadata(
            typeof(ListViewBase),
            new FrameworkPropertyMetadata(OnSelectionModePropertyChanged));
    }

    protected ListViewBase()
    {
        UpdateMultiSelectEnabled();
    }

    #region IsItemClickEnabled

    public static readonly DependencyProperty IsItemClickEnabledProperty =
        DependencyProperty.Register(
            nameof(IsItemClickEnabled),
            typeof(bool),
            typeof(ListViewBase),
            new PropertyMetadata(false));

    public bool IsItemClickEnabled
    {
        get => (bool)GetValue(IsItemClickEnabledProperty);
        set => SetValue(IsItemClickEnabledProperty, value);
    }

    #endregion

    #region IsSelectionEnabled

    public static readonly DependencyProperty IsSelectionEnabledProperty =
        DependencyProperty.Register(
            nameof(IsSelectionEnabled),
            typeof(bool),
            typeof(ListViewBase),
            new PropertyMetadata(true, OnIsSelectionEnabledChanged));

    public bool IsSelectionEnabled
    {
        get => (bool)GetValue(IsSelectionEnabledProperty);
        set => SetValue(IsSelectionEnabledProperty, value);
    }

    private static void OnIsSelectionEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var lvb = (ListViewBase)d;
        lvb.UpdateMultiSelectEnabled();
        if (!(bool)e.NewValue && lvb.SelectedItems.Count > 0)
        {
            lvb.UnselectAll();
        }
    }

    #endregion

    #region IsMultiSelectCheckBoxEnabled

    public static readonly DependencyProperty IsMultiSelectCheckBoxEnabledProperty =
        DependencyProperty.Register(
            nameof(IsMultiSelectCheckBoxEnabled),
            typeof(bool),
            typeof(ListViewBase),
            new PropertyMetadata(true, OnIsMultiSelectCheckBoxEnabledChanged));

    public bool IsMultiSelectCheckBoxEnabled
    {
        get => (bool)GetValue(IsMultiSelectCheckBoxEnabledProperty);
        set => SetValue(IsMultiSelectCheckBoxEnabledProperty, value);
    }

    private static void OnIsMultiSelectCheckBoxEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((ListViewBase)d).UpdateMultiSelectEnabled();
    }

    #endregion

    #region UseSystemFocusVisuals

    public static readonly DependencyProperty UseSystemFocusVisualsProperty =
        FocusVisualHelper.UseSystemFocusVisualsProperty.AddOwner(typeof(ListViewBase));

    public bool UseSystemFocusVisuals
    {
        get => (bool)GetValue(UseSystemFocusVisualsProperty);
        set => SetValue(UseSystemFocusVisualsProperty, value);
    }

    #endregion

    #region FocusVisualMargin

    public static readonly DependencyProperty FocusVisualMarginProperty =
        FocusVisualHelper.FocusVisualMarginProperty.AddOwner(typeof(ListViewBase));

    public Thickness FocusVisualMargin
    {
        get => (Thickness)GetValue(FocusVisualMarginProperty);
        set => SetValue(FocusVisualMarginProperty, value);
    }

    #endregion

    #region CornerRadius

    public static readonly DependencyProperty CornerRadiusProperty =
        Border.CornerRadiusProperty.AddOwner(typeof(ListViewBase));

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    #endregion

    #region Header

    public static readonly DependencyProperty HeaderProperty =
        ListViewHelper.HeaderProperty.AddOwner(typeof(ListViewBase));

    public object Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    #endregion

    #region HeaderTemplate

    public static readonly DependencyProperty HeaderTemplateProperty =
        ListViewHelper.HeaderTemplateProperty.AddOwner(typeof(ListViewBase));

    public DataTemplate HeaderTemplate
    {
        get => (DataTemplate)GetValue(HeaderTemplateProperty);
        set => SetValue(HeaderTemplateProperty, value);
    }

    #endregion

    #region Footer

    public static readonly DependencyProperty FooterProperty =
        ListViewHelper.FooterProperty.AddOwner(typeof(ListViewBase));

    public object Footer
    {
        get => GetValue(FooterProperty);
        set => SetValue(FooterProperty, value);
    }

    #endregion

    #region FooterTemplate

    public static readonly DependencyProperty FooterTemplateProperty =
        ListViewHelper.FooterTemplateProperty.AddOwner(typeof(ListViewBase));

    public DataTemplate FooterTemplate
    {
        get => (DataTemplate)GetValue(FooterTemplateProperty);
        set => SetValue(FooterTemplateProperty, value);
    }

    #endregion

    internal bool MultiSelectEnabled
    {
        get => _multiSelectEnabled;
        set
        {
            if (_multiSelectEnabled != value)
            {
                _multiSelectEnabled = value;
                MultiSelectEnabledChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public event ItemClickEventHandler? ItemClick;

    internal event EventHandler? MultiSelectEnabledChanged;

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);

        if (element is ListViewBaseItem lvi)
        {
            lvi.SubscribeToMultiSelectEnabledChanged(this);
        }
    }

    protected override void ClearContainerForItemOverride(DependencyObject element, object item)
    {
        base.ClearContainerForItemOverride(element, item);

        if (element is ListViewBaseItem lvi)
        {
            lvi.UnsubscribeFromMultiSelectEnabledChanged(this);
        }
    }

    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        if (IsSelectionEnabled)
        {
            base.OnSelectionChanged(e);
        }
        else if (SelectedItems.Count > 0)
        {
            UnselectAll();
        }
    }

    internal void NotifyListItemClicked(ListViewBaseItem item)
    {
        if (IsItemClickEnabled)
        {
            var clickedItem = ItemContainerGenerator.ItemFromContainer(item);
            ItemClick?.Invoke(this, new ItemClickEventArgs { ClickedItem = clickedItem });
        }
    }

    private static void OnSelectionModePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((ListViewBase)d).UpdateMultiSelectEnabled();
    }

    private void UpdateMultiSelectEnabled()
    {
        MultiSelectEnabled = IsSelectionEnabled &&
                             SelectionMode == SelectionMode.Multiple &&
                             IsMultiSelectCheckBoxEnabled;
    }

    private bool _multiSelectEnabled;
}
