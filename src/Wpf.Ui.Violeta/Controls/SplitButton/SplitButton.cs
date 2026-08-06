using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// A button with a primary action area and a separate chevron that opens a flyout.
/// Primary click uses <see cref="System.Windows.Controls.Primitives.ButtonBase.Command"/>;
/// double-click on the primary area uses <see cref="DoubleCommand"/>.
/// </summary>
[TemplatePart(Name = TemplateElementToggle, Type = typeof(Border))]
[TemplatePart(Name = TemplateElementToggleButton, Type = typeof(ToggleButton))]
public class SplitButton : Wpf.Ui.Controls.Button
{
    private const string TemplateElementToggle = "PART_Toggle";
    private const string TemplateElementToggleButton = "PART_ToggleButton";
    private const string FlyoutContextMenuStyleKey = "DefaultDropDownFlyoutContextMenuStyle";

    private ContextMenu? _contextMenu;
    private Border? _splitButtonToggleBorder;

    /// <summary>Gets or sets the control responsible for toggling the drop-down.</summary>
    protected ToggleButton? SplitButtonToggleButton { get; set; }

    static SplitButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(SplitButton),
            new FrameworkPropertyMetadata(typeof(SplitButton)));
    }

    public SplitButton()
    {
        Unloaded += static (sender, _) => ((SplitButton)sender).ReleaseTemplateResources();
        Loaded += static (sender, _) =>
        {
            var self = (SplitButton)sender;
            if (self.SplitButtonToggleButton is not null)
            {
                self.AttachToggleButtonClick();
            }
        };
    }

    /// <summary>Identifies the <see cref="Flyout"/> dependency property.</summary>
    public static readonly DependencyProperty FlyoutProperty = DependencyProperty.Register(
        nameof(Flyout),
        typeof(object),
        typeof(SplitButton),
        new PropertyMetadata(null, OnFlyoutChanged));

    /// <summary>Identifies the <see cref="IsDropDownOpen"/> dependency property.</summary>
    public static readonly DependencyProperty IsDropDownOpenProperty = DependencyProperty.Register(
        nameof(IsDropDownOpen),
        typeof(bool),
        typeof(SplitButton),
        new PropertyMetadata(false, OnIsDropDownOpenChanged));

    /// <summary>Identifies the <see cref="DoubleCommand"/> dependency property.</summary>
    public static readonly DependencyProperty DoubleCommandProperty = DependencyProperty.Register(
        nameof(DoubleCommand),
        typeof(ICommand),
        typeof(SplitButton),
        new PropertyMetadata(null));

    /// <summary>Identifies the <see cref="DoubleCommandParameter"/> dependency property.</summary>
    public static readonly DependencyProperty DoubleCommandParameterProperty = DependencyProperty.Register(
        nameof(DoubleCommandParameter),
        typeof(object),
        typeof(SplitButton),
        new PropertyMetadata(null));

    /// <summary>Gets or sets the flyout associated with this button.</summary>
    [Bindable(true)]
    public object? Flyout
    {
        get => GetValue(FlyoutProperty);
        set => SetValue(FlyoutProperty, value);
    }

    /// <summary>Gets or sets a value indicating whether the drop-down is currently open.</summary>
    [Bindable(true)]
    [Browsable(false)]
    [Category("Appearance")]
    public bool IsDropDownOpen
    {
        get => (bool)GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }

    /// <summary>
    /// Gets or sets the command invoked when the primary content area is double-clicked.
    /// Single-click continues to use <see cref="System.Windows.Controls.Primitives.ButtonBase.Command"/>.
    /// </summary>
    [Bindable(true)]
    [Category("Action")]
    public ICommand? DoubleCommand
    {
        get => (ICommand?)GetValue(DoubleCommandProperty);
        set => SetValue(DoubleCommandProperty, value);
    }

    /// <summary>Gets or sets the parameter passed to <see cref="DoubleCommand"/>.</summary>
    [Bindable(true)]
    [Category("Action")]
    public object? DoubleCommandParameter
    {
        get => GetValue(DoubleCommandParameterProperty);
        set => SetValue(DoubleCommandParameterProperty, value);
    }

    private static void OnFlyoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((SplitButton)d).OnFlyoutChanged(e.OldValue, e.NewValue);
    }

    /// <summary>Invoked when <see cref="Flyout"/> changes.</summary>
    protected virtual void OnFlyoutChanged(object? oldValue, object? newValue)
    {
        if (oldValue is ContextMenu oldMenu)
        {
            oldMenu.Opened -= OnContextMenuOpened;
            oldMenu.Closed -= OnContextMenuClosed;
        }

        _contextMenu = null;

        if (newValue is ContextMenu contextMenu)
        {
            _contextMenu = contextMenu;
            ApplyFlyoutContextMenuStyle(contextMenu);
            contextMenu.Opened += OnContextMenuOpened;
            contextMenu.Closed += OnContextMenuClosed;
        }
    }

    private static void OnIsDropDownOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((SplitButton)d).OnIsDropDownOpenChanged(e.NewValue is true);
    }

    protected virtual void OnContextMenuClosed(object sender, RoutedEventArgs e)
    {
        SetCurrentValue(IsDropDownOpenProperty, false);
    }

    protected virtual void OnContextMenuOpened(object sender, RoutedEventArgs e)
    {
        SetCurrentValue(IsDropDownOpenProperty, true);
    }

    /// <summary>Invoked when <see cref="IsDropDownOpen"/> changes.</summary>
    protected virtual void OnIsDropDownOpenChanged(bool currentValue) { }

    /// <inheritdoc />
    public override void OnApplyTemplate()
    {
        ReleaseTemplateResources();

        base.OnApplyTemplate();

        SplitButtonToggleButton = GetTemplateChild(TemplateElementToggleButton) as ToggleButton
            ?? throw new NullReferenceException(
                $"Element {TemplateElementToggleButton} of type {typeof(ToggleButton)} not found in {typeof(SplitButton)}");

        _splitButtonToggleBorder = GetTemplateChild(TemplateElementToggle) as Border;

        AttachToggleButtonClick();
    }

    /// <inheritdoc />
    protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
    {
        base.OnMouseDoubleClick(e);

        if (e.ChangedButton != MouseButton.Left || e.Handled)
        {
            return;
        }

        if (IsOverToggle(e.GetPosition(this)))
        {
            return;
        }

        var parameter = DoubleCommandParameter;
        var command = DoubleCommand;
        if (command?.CanExecute(parameter) == true)
        {
            command.Execute(parameter);
            e.Handled = true;
        }
    }

    /// <summary>Releases template event handlers.</summary>
    protected virtual void ReleaseTemplateResources()
    {
        if (SplitButtonToggleButton is not null)
        {
            SplitButtonToggleButton.PreviewMouseLeftButtonUp -= OnSplitButtonToggleButtonOnPreviewMouseLeftButtonUp;
        }
    }

    private void AttachToggleButtonClick()
    {
        if (SplitButtonToggleButton is null)
        {
            return;
        }

        SplitButtonToggleButton.PreviewMouseLeftButtonUp -= OnSplitButtonToggleButtonOnPreviewMouseLeftButtonUp;
        SplitButtonToggleButton.PreviewMouseLeftButtonUp += OnSplitButtonToggleButtonOnPreviewMouseLeftButtonUp;
    }

    private void OnSplitButtonToggleButtonOnPreviewMouseLeftButtonUp(object sender, MouseEventArgs e)
    {
        if (sender is not ToggleButton || _contextMenu is null || _splitButtonToggleBorder is null)
        {
            return;
        }

        var position = e.GetPosition(_splitButtonToggleBorder);
        if (VisualTreeHelper.HitTest(_splitButtonToggleBorder, position)?.VisualHit is null)
        {
            return;
        }

        _contextMenu.SetCurrentValue(MinWidthProperty, ActualWidth);
        _contextMenu.SetCurrentValue(ContextMenu.PlacementTargetProperty, this);
        _contextMenu.SetCurrentValue(ContextMenu.PlacementProperty, PlacementMode.Bottom);
        _contextMenu.SetCurrentValue(ContextMenu.IsOpenProperty, true);
    }

    private bool IsOverToggle(Point positionRelativeToThis)
    {
        if (_splitButtonToggleBorder is null)
        {
            return false;
        }

        var toggleOrigin = _splitButtonToggleBorder.TranslatePoint(new Point(0, 0), this);
        var bounds = new Rect(toggleOrigin, _splitButtonToggleBorder.RenderSize);
        return bounds.Contains(positionRelativeToThis);
    }

    private void ApplyFlyoutContextMenuStyle(ContextMenu contextMenu)
    {
        if (contextMenu.ReadLocalValue(StyleProperty) != DependencyProperty.UnsetValue)
        {
            return;
        }

        var style =
            TryFindResource(FlyoutContextMenuStyleKey) as Style
            ?? Application.Current?.TryFindResource(FlyoutContextMenuStyleKey) as Style;

        if (style is not null)
        {
            contextMenu.Style = style;
        }
    }
}
