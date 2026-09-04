using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Wpf.Ui.Violeta.Resources.Localization;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// A button that opens a confirmation flyout before running <see cref="ConfirmedCommand"/>.
/// Clicking the button only shows the flyout; the command runs after the user confirms.
/// </summary>
/// <example>
/// <code lang="xml">
/// &lt;vio:PopConfirmButton
///     Content="Delete"
///     Title="Delete this item?"
///     Message="This action cannot be undone."
///     MessageBoxIcon="Warning"
///     ConfirmedCommand="{Binding DeleteCommand}"
///     CancelledCommand="{Binding CancelDeleteCommand}" /&gt;
/// </code>
/// </example>
public class PopConfirmButton : Wpf.Ui.Controls.Button
{
    private const string FlyoutTemplateKey = "DefaultPopConfirmFlyoutTemplate";

    private FluentPopup? _popup;
    private ContentControl? _flyoutHost;
    private bool _suppressIsOpenCallback;
    private bool _confirming;
    private bool _skipCancelledOnClose;

    private readonly ICommand _internalConfirmCommand;
    private readonly ICommand _internalCancelCommand;

    /// <summary>Identifies the <see cref="Title"/> dependency property.</summary>
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
        nameof(Title),
        typeof(string),
        typeof(PopConfirmButton),
        new PropertyMetadata(null));

    /// <summary>Identifies the <see cref="Message"/> dependency property.</summary>
    public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(
        nameof(Message),
        typeof(string),
        typeof(PopConfirmButton),
        new PropertyMetadata(null));

    /// <summary>Identifies the <see cref="ConfirmButtonText"/> dependency property.</summary>
    public static readonly DependencyProperty ConfirmButtonTextProperty = DependencyProperty.Register(
        nameof(ConfirmButtonText),
        typeof(string),
        typeof(PopConfirmButton),
        new PropertyMetadata(null));

    /// <summary>Identifies the <see cref="CancelButtonText"/> dependency property.</summary>
    public static readonly DependencyProperty CancelButtonTextProperty = DependencyProperty.Register(
        nameof(CancelButtonText),
        typeof(string),
        typeof(PopConfirmButton),
        new PropertyMetadata(null));

    /// <summary>Identifies the <see cref="MessageBoxIcon"/> dependency property.</summary>
    public static readonly DependencyProperty MessageBoxIconProperty = DependencyProperty.Register(
        nameof(MessageBoxIcon),
        typeof(MessageBoxIcon),
        typeof(PopConfirmButton),
        new PropertyMetadata(MessageBoxIcon.Warning));

    /// <summary>Identifies the <see cref="IsCloseButtonVisible"/> dependency property.</summary>
    public static readonly DependencyProperty IsCloseButtonVisibleProperty = DependencyProperty.Register(
        nameof(IsCloseButtonVisible),
        typeof(bool),
        typeof(PopConfirmButton),
        new PropertyMetadata(true));

    /// <summary>Identifies the <see cref="IsConfirmOpen"/> dependency property.</summary>
    public static readonly DependencyProperty IsConfirmOpenProperty = DependencyProperty.Register(
        nameof(IsConfirmOpen),
        typeof(bool),
        typeof(PopConfirmButton),
        new PropertyMetadata(false, OnIsConfirmOpenChanged));

    /// <summary>Identifies the <see cref="Placement"/> dependency property.</summary>
    public static readonly DependencyProperty PlacementProperty = DependencyProperty.Register(
        nameof(Placement),
        typeof(PlacementMode),
        typeof(PopConfirmButton),
        new PropertyMetadata(PlacementMode.Bottom, OnPlacementChanged));

    /// <summary>Identifies the <see cref="ConfirmedCommand"/> dependency property.</summary>
    public static readonly DependencyProperty ConfirmedCommandProperty = DependencyProperty.Register(
        nameof(ConfirmedCommand),
        typeof(ICommand),
        typeof(PopConfirmButton),
        new PropertyMetadata(null));

    /// <summary>Identifies the <see cref="ConfirmedCommandParameter"/> dependency property.</summary>
    public static readonly DependencyProperty ConfirmedCommandParameterProperty = DependencyProperty.Register(
        nameof(ConfirmedCommandParameter),
        typeof(object),
        typeof(PopConfirmButton),
        new PropertyMetadata(null));

    /// <summary>Identifies the <see cref="CancelledCommand"/> dependency property.</summary>
    public static readonly DependencyProperty CancelledCommandProperty = DependencyProperty.Register(
        nameof(CancelledCommand),
        typeof(ICommand),
        typeof(PopConfirmButton),
        new PropertyMetadata(null));

    /// <summary>Identifies the <see cref="CancelledCommandParameter"/> dependency property.</summary>
    public static readonly DependencyProperty CancelledCommandParameterProperty = DependencyProperty.Register(
        nameof(CancelledCommandParameter),
        typeof(object),
        typeof(PopConfirmButton),
        new PropertyMetadata(null));

    private static readonly DependencyPropertyKey DisplayConfirmButtonTextPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(DisplayConfirmButtonText),
            typeof(string),
            typeof(PopConfirmButton),
            new PropertyMetadata(string.Empty));

    /// <summary>Identifies the <see cref="DisplayConfirmButtonText"/> dependency property.</summary>
    public static readonly DependencyProperty DisplayConfirmButtonTextProperty =
        DisplayConfirmButtonTextPropertyKey.DependencyProperty;

    private static readonly DependencyPropertyKey DisplayCancelButtonTextPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(DisplayCancelButtonText),
            typeof(string),
            typeof(PopConfirmButton),
            new PropertyMetadata(string.Empty));

    /// <summary>Identifies the <see cref="DisplayCancelButtonText"/> dependency property.</summary>
    public static readonly DependencyProperty DisplayCancelButtonTextProperty =
        DisplayCancelButtonTextPropertyKey.DependencyProperty;

    /// <summary>Identifies the <see cref="ConfirmedEvent"/> routed event.</summary>
    public static readonly RoutedEvent ConfirmedEvent = EventManager.RegisterRoutedEvent(
        nameof(Confirmed),
        RoutingStrategy.Bubble,
        typeof(RoutedEventHandler),
        typeof(PopConfirmButton));

    /// <summary>Identifies the <see cref="CancelledEvent"/> routed event.</summary>
    public static readonly RoutedEvent CancelledEvent = EventManager.RegisterRoutedEvent(
        nameof(Cancelled),
        RoutingStrategy.Bubble,
        typeof(RoutedEventHandler),
        typeof(PopConfirmButton));

    static PopConfirmButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(PopConfirmButton),
            new FrameworkPropertyMetadata(typeof(PopConfirmButton)));
    }

    public PopConfirmButton()
    {
        _internalConfirmCommand = new PopConfirmActionCommand(ExecuteConfirm);
        _internalCancelCommand = new PopConfirmActionCommand(ExecuteCancel);
        Unloaded += OnUnloaded;
    }

    /// <summary>Gets or sets the confirmation title.</summary>
    public string? Title
    {
        get => (string?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>Gets or sets the secondary message under the title.</summary>
    public string? Message
    {
        get => (string?)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    /// <summary>Gets or sets the confirm button label. Defaults to the localized OK string when null or empty.</summary>
    public string? ConfirmButtonText
    {
        get => (string?)GetValue(ConfirmButtonTextProperty);
        set => SetValue(ConfirmButtonTextProperty, value);
    }

    /// <summary>Gets or sets the cancel button label. Defaults to the localized Cancel string when null or empty.</summary>
    public string? CancelButtonText
    {
        get => (string?)GetValue(CancelButtonTextProperty);
        set => SetValue(CancelButtonTextProperty, value);
    }

    /// <summary>Gets or sets the icon shown beside the title (same glyphs/colors as <see cref="MessageBox"/>).</summary>
    public MessageBoxIcon MessageBoxIcon
    {
        get => (MessageBoxIcon)GetValue(MessageBoxIconProperty);
        set => SetValue(MessageBoxIconProperty, value);
    }

    /// <summary>Gets or sets whether the close (X) button is visible.</summary>
    public bool IsCloseButtonVisible
    {
        get => (bool)GetValue(IsCloseButtonVisibleProperty);
        set => SetValue(IsCloseButtonVisibleProperty, value);
    }

    /// <summary>Gets or sets whether the confirmation flyout is open.</summary>
    public bool IsConfirmOpen
    {
        get => (bool)GetValue(IsConfirmOpenProperty);
        set => SetValue(IsConfirmOpenProperty, value);
    }

    /// <summary>Gets or sets the flyout placement relative to the button.</summary>
    public PlacementMode Placement
    {
        get => (PlacementMode)GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }

    /// <summary>Gets or sets the command executed when the user confirms.</summary>
    public ICommand? ConfirmedCommand
    {
        get => (ICommand?)GetValue(ConfirmedCommandProperty);
        set => SetValue(ConfirmedCommandProperty, value);
    }

    /// <summary>Gets or sets the parameter for <see cref="ConfirmedCommand"/>.</summary>
    public object? ConfirmedCommandParameter
    {
        get => GetValue(ConfirmedCommandParameterProperty);
        set => SetValue(ConfirmedCommandParameterProperty, value);
    }

    /// <summary>Gets or sets the command executed when the user cancels or dismisses the flyout.</summary>
    public ICommand? CancelledCommand
    {
        get => (ICommand?)GetValue(CancelledCommandProperty);
        set => SetValue(CancelledCommandProperty, value);
    }

    /// <summary>Gets or sets the parameter for <see cref="CancelledCommand"/>.</summary>
    public object? CancelledCommandParameter
    {
        get => GetValue(CancelledCommandParameterProperty);
        set => SetValue(CancelledCommandParameterProperty, value);
    }

    /// <summary>Raised after the user confirms (after <see cref="ConfirmedCommand"/> is executed).</summary>
    public event RoutedEventHandler Confirmed
    {
        add => AddHandler(ConfirmedEvent, value);
        remove => RemoveHandler(ConfirmedEvent, value);
    }

    /// <summary>Raised when the user cancels or light-dismisses the flyout.</summary>
    public event RoutedEventHandler Cancelled
    {
        add => AddHandler(CancelledEvent, value);
        remove => RemoveHandler(CancelledEvent, value);
    }

    /// <summary>Command bound by the flyout Confirm button.</summary>
    public ICommand InternalConfirmCommand => _internalConfirmCommand;

    /// <summary>Command bound by the flyout Cancel / Close buttons.</summary>
    public ICommand InternalCancelCommand => _internalCancelCommand;

    /// <summary>Localized confirm label shown in the flyout.</summary>
    public string DisplayConfirmButtonText => (string)GetValue(DisplayConfirmButtonTextProperty);

    /// <summary>Localized cancel label shown in the flyout.</summary>
    public string DisplayCancelButtonText => (string)GetValue(DisplayCancelButtonTextProperty);

    /// <inheritdoc />
    protected override void OnClick()
    {
        // Opening / toggling the flyout must not run ConfirmedCommand / Button.Command.
        if (IsConfirmOpen)
        {
            CloseConfirm(raiseCancelled: true);
            return;
        }

        OpenConfirm();
    }

    /// <summary>Opens the confirmation flyout.</summary>
    public void OpenConfirm()
    {
        EnsurePopup();
        RefreshDisplayButtonTexts();
        SetCurrentValue(IsConfirmOpenProperty, true);
    }

    /// <summary>Closes the confirmation flyout without confirming.</summary>
    public void CloseConfirm() => CloseConfirm(raiseCancelled: false);

    private void CloseConfirm(bool raiseCancelled)
    {
        _skipCancelledOnClose = true;
        SetCurrentValue(IsConfirmOpenProperty, false);

        if (raiseCancelled)
        {
            RaiseCancelled();
        }
    }

    private void ExecuteConfirm()
    {
        if (_confirming)
        {
            return;
        }

        _confirming = true;
        _skipCancelledOnClose = true;
        try
        {
            SetCurrentValue(IsConfirmOpenProperty, false);
            TryExecuteCommand(ConfirmedCommand, ConfirmedCommandParameter);
            RaiseEvent(new RoutedEventArgs(ConfirmedEvent, this));
        }
        finally
        {
            _confirming = false;
        }
    }

    private void ExecuteCancel() => CloseConfirm(raiseCancelled: true);

    private void RaiseCancelled()
    {
        TryExecuteCommand(CancelledCommand, CancelledCommandParameter);
        RaiseEvent(new RoutedEventArgs(CancelledEvent, this));
    }

    private void EnsurePopup()
    {
        if (_popup is not null)
        {
            return;
        }

        // Do NOT set Content = this: Popup would add the button as a logical child and
        // create a cycle (button already has a parent in the page tree). Bindings use DataContext.
        _flyoutHost = new ContentControl
        {
            DataContext = this,
            Focusable = false,
        };

        if (TryFindResource(FlyoutTemplateKey) is ControlTemplate template)
        {
            _flyoutHost.Template = template;
        }

        _popup = new FluentPopup
        {
            AllowsTransparency = true,
            StaysOpen = false,
            Placement = Placement,
            PlacementTarget = this,
            VerticalOffset = 4,
            PopupAnimation = PopupAnimation.Fade,
            ExtPopupAnimation = FluentPopup.FluentPopupAnimation.Fade,
            Child = _flyoutHost,
        };

        _popup.Opened += OnPopupOpened;
        _popup.Closed += OnPopupClosed;
    }

    private void RefreshDisplayButtonTexts()
    {
        SetValue(
            DisplayConfirmButtonTextPropertyKey,
            string.IsNullOrEmpty(ConfirmButtonText) ? SH.ButtonOK : ConfirmButtonText);
        SetValue(
            DisplayCancelButtonTextPropertyKey,
            string.IsNullOrEmpty(CancelButtonText) ? SH.ButtonCancel : CancelButtonText);
    }

    private static void OnIsConfirmOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var owner = (PopConfirmButton)d;
        if (owner._suppressIsOpenCallback)
        {
            return;
        }

        owner.EnsurePopup();

        var open = (bool)e.NewValue!;
        if (owner._popup is null)
        {
            return;
        }

        if (open)
        {
            owner.RefreshDisplayButtonTexts();
            owner._popup.Placement = owner.Placement;
            owner._popup.PlacementTarget = owner;
            owner._popup.IsOpen = true;
        }
        else
        {
            owner._popup.IsOpen = false;
        }
    }

    private static void OnPlacementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var owner = (PopConfirmButton)d;
        if (owner._popup is not null)
        {
            owner._popup.Placement = (PlacementMode)e.NewValue!;
        }
    }

    private void OnPopupOpened(object? sender, EventArgs e)
    {
        _suppressIsOpenCallback = true;
        try
        {
            SetCurrentValue(IsConfirmOpenProperty, true);
        }
        finally
        {
            _suppressIsOpenCallback = false;
        }
    }

    private void OnPopupClosed(object? sender, EventArgs e)
    {
        _suppressIsOpenCallback = true;
        try
        {
            SetCurrentValue(IsConfirmOpenProperty, false);
        }
        finally
        {
            _suppressIsOpenCallback = false;
        }

        if (_skipCancelledOnClose)
        {
            _skipCancelledOnClose = false;
            return;
        }

        if (!_confirming)
        {
            RaiseCancelled();
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        if (_popup is null)
        {
            return;
        }

        _popup.Opened -= OnPopupOpened;
        _popup.Closed -= OnPopupClosed;
        _popup.IsOpen = false;
        _popup.Child = null;
        _popup = null;
        _flyoutHost = null;
    }

    private static void TryExecuteCommand(ICommand? command, object? parameter)
    {
        if (command?.CanExecute(parameter) == true)
        {
            command.Execute(parameter);
        }
    }

    private sealed class PopConfirmActionCommand(Action execute) : ICommand
    {
        private readonly Action _execute = execute;

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter) => _execute();

#pragma warning disable CS0067 // Event never used — required by ICommand.
        public event EventHandler? CanExecuteChanged;
#pragma warning restore CS0067
    }
}
