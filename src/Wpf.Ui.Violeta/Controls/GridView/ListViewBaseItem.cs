using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Wpf.Ui.Violeta.Controls.Compat;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Item container base for WinUI-style list/grid views (click + multi-select visual states).
/// </summary>
public class ListViewBaseItem : ListBoxItem
{
    protected ListViewBaseItem()
    {
    }

    #region UseSystemFocusVisuals

    public static readonly DependencyProperty UseSystemFocusVisualsProperty =
        FocusVisualHelper.UseSystemFocusVisualsProperty.AddOwner(typeof(ListViewBaseItem));

    public bool UseSystemFocusVisuals
    {
        get => (bool)GetValue(UseSystemFocusVisualsProperty);
        set => SetValue(UseSystemFocusVisualsProperty, value);
    }

    #endregion

    #region FocusVisualMargin

    public static readonly DependencyProperty FocusVisualMarginProperty =
        FocusVisualHelper.FocusVisualMarginProperty.AddOwner(typeof(ListViewBaseItem));

    public Thickness FocusVisualMargin
    {
        get => (Thickness)GetValue(FocusVisualMarginProperty);
        set => SetValue(FocusVisualMarginProperty, value);
    }

    #endregion

    #region CornerRadius

    public static readonly DependencyProperty CornerRadiusProperty =
        Border.CornerRadiusProperty.AddOwner(typeof(ListViewBaseItem));

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    #endregion

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        UpdateMultiSelectStates(ParentListViewBase, false);
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        if (!e.Handled)
        {
            _isPressed = true;
        }

        base.OnMouseLeftButtonDown(e);
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        if (!e.Handled)
        {
            HandleMouseUp(e);
            _isPressed = false;
        }

        base.OnMouseLeftButtonUp(e);
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
        if (!e.Handled)
        {
            _isPressed = false;
        }

        base.OnMouseLeave(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Key == Key.Enter)
        {
            OnClick();
            e.Handled = true;
        }
    }

    internal void SubscribeToMultiSelectEnabledChanged(ListViewBase parent)
    {
        parent.MultiSelectEnabledChanged += OnMultiSelectEnabledChanged;
        UpdateMultiSelectStates(parent);
    }

    internal void UnsubscribeFromMultiSelectEnabledChanged(ListViewBase parent)
    {
        parent.MultiSelectEnabledChanged -= OnMultiSelectEnabledChanged;
        UpdateMultiSelectStates(parent);
    }

    private void OnMultiSelectEnabledChanged(object? sender, EventArgs e)
    {
        UpdateMultiSelectStates((ListViewBase)sender!);
    }

    private void UpdateMultiSelectStates(ListViewBase? parent, bool useTransitions = true)
    {
        if (parent is null)
        {
            return;
        }

        bool enabled = parent.MultiSelectEnabled && parent.IsMultiSelectCheckBoxEnabled;
        VisualStateManager.GoToState(this, enabled ? "MultiSelectEnabled" : "MultiSelectDisabled", useTransitions);
    }

    private void HandleMouseUp(MouseButtonEventArgs e)
    {
        if (!_isPressed)
        {
            return;
        }

        var bounds = new Rect(new Point(), RenderSize);
        if (bounds.Contains(e.GetPosition(this)))
        {
            OnClick();
        }
    }

    private void OnClick()
    {
        ParentListViewBase?.NotifyListItemClicked(this);
    }

    private ListViewBase? ParentListViewBase =>
        ItemsControl.ItemsControlFromItemContainer(this) as ListViewBase;

    private bool _isPressed;
}
