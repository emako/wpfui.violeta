using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Wpf.Ui.Violeta.Resources.Localization;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// A button that opens a confirmation flyout before running footer commands.
/// API mirrors <see cref="ContentDialog"/>: Primary / Secondary / Close.
/// By default Primary (OK) and Close (Cancel) are shown; set <see cref="SecondaryButtonText"/> to show a third button.
/// Empty string on Primary/Close text hides that button; <c>null</c> uses the localized default and keeps it visible.
/// </summary>
/// <example>
/// <code lang="xml">
/// &lt;vio:PopConfirmButton
///     Content="Delete"
///     Title="Delete this item?"
///     Message="This action cannot be undone."
///     MessageBoxIcon="Warning"
///     PrimaryButtonCommand="{Binding DeleteCommand}"
///     CloseButtonCommand="{Binding CancelCommand}" /&gt;
/// </code>
/// </example>
public class PopConfirmButton : Wpf.Ui.Controls.Button
{
    private const string FlyoutTemplateKey = "DefaultPopConfirmFlyoutTemplate";

    private FluentPopup? _popup;
    private ContentControl? _flyoutHost;
    private bool _suppressIsOpenCallback;
    private bool _executingFooter;
    private bool _skipCloseOnPopupClosed;

    private readonly ICommand _internalPrimaryCommand;
    private readonly ICommand _internalSecondaryCommand;
    private readonly ICommand _internalCloseCommand;

    #region Title / Message / Icon / Placement

    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
        nameof(Title), typeof(string), typeof(PopConfirmButton), new PropertyMetadata(null));

    public string? Title
    {
        get => (string?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(
        nameof(Message), typeof(string), typeof(PopConfirmButton), new PropertyMetadata(null));

    public string? Message
    {
        get => (string?)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public static readonly DependencyProperty MessageBoxIconProperty = DependencyProperty.Register(
        nameof(MessageBoxIcon), typeof(MessageBoxIcon), typeof(PopConfirmButton),
        new PropertyMetadata(MessageBoxIcon.Warning));

    public MessageBoxIcon MessageBoxIcon
    {
        get => (MessageBoxIcon)GetValue(MessageBoxIconProperty);
        set => SetValue(MessageBoxIconProperty, value);
    }

    /// <summary>Chrome (header) dismiss X visibility — not the footer Close button.</summary>
    public static readonly DependencyProperty IsCloseButtonVisibleProperty = DependencyProperty.Register(
        nameof(IsCloseButtonVisible), typeof(bool), typeof(PopConfirmButton), new PropertyMetadata(true));

    public bool IsCloseButtonVisible
    {
        get => (bool)GetValue(IsCloseButtonVisibleProperty);
        set => SetValue(IsCloseButtonVisibleProperty, value);
    }

    public static readonly DependencyProperty IsConfirmOpenProperty = DependencyProperty.Register(
        nameof(IsConfirmOpen), typeof(bool), typeof(PopConfirmButton),
        new PropertyMetadata(false, OnIsConfirmOpenChanged));

    public bool IsConfirmOpen
    {
        get => (bool)GetValue(IsConfirmOpenProperty);
        set => SetValue(IsConfirmOpenProperty, value);
    }

    public static readonly DependencyProperty PlacementProperty = DependencyProperty.Register(
        nameof(Placement), typeof(PlacementMode), typeof(PopConfirmButton),
        new PropertyMetadata(PlacementMode.Bottom, OnPlacementChanged));

    public PlacementMode Placement
    {
        get => (PlacementMode)GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }

    #endregion

    #region Primary

    public static readonly DependencyProperty PrimaryButtonTextProperty = DependencyProperty.Register(
        nameof(PrimaryButtonText), typeof(string), typeof(PopConfirmButton),
        new PropertyMetadata(null, OnFooterButtonTextChanged));

    /// <summary>Primary footer label. <c>null</c> → localized OK; <c>""</c> → hide.</summary>
    public string? PrimaryButtonText
    {
        get => (string?)GetValue(PrimaryButtonTextProperty);
        set => SetValue(PrimaryButtonTextProperty, value);
    }

    public static readonly DependencyProperty PrimaryButtonCommandProperty = DependencyProperty.Register(
        nameof(PrimaryButtonCommand), typeof(ICommand), typeof(PopConfirmButton), new PropertyMetadata(null));

    public ICommand? PrimaryButtonCommand
    {
        get => (ICommand?)GetValue(PrimaryButtonCommandProperty);
        set => SetValue(PrimaryButtonCommandProperty, value);
    }

    public static readonly DependencyProperty PrimaryButtonCommandParameterProperty = DependencyProperty.Register(
        nameof(PrimaryButtonCommandParameter), typeof(object), typeof(PopConfirmButton), new PropertyMetadata(null));

    public object? PrimaryButtonCommandParameter
    {
        get => GetValue(PrimaryButtonCommandParameterProperty);
        set => SetValue(PrimaryButtonCommandParameterProperty, value);
    }

    public static readonly DependencyProperty IsPrimaryButtonEnabledProperty = DependencyProperty.Register(
        nameof(IsPrimaryButtonEnabled), typeof(bool), typeof(PopConfirmButton), new PropertyMetadata(true));

    public bool IsPrimaryButtonEnabled
    {
        get => (bool)GetValue(IsPrimaryButtonEnabledProperty);
        set => SetValue(IsPrimaryButtonEnabledProperty, value);
    }

    #endregion

    #region Secondary

    public static readonly DependencyProperty SecondaryButtonTextProperty = DependencyProperty.Register(
        nameof(SecondaryButtonText), typeof(string), typeof(PopConfirmButton),
        new PropertyMetadata(null, OnFooterButtonTextChanged));

    /// <summary>Secondary footer label. Shown only when non-null and non-empty (third button).</summary>
    public string? SecondaryButtonText
    {
        get => (string?)GetValue(SecondaryButtonTextProperty);
        set => SetValue(SecondaryButtonTextProperty, value);
    }

    public static readonly DependencyProperty SecondaryButtonCommandProperty = DependencyProperty.Register(
        nameof(SecondaryButtonCommand), typeof(ICommand), typeof(PopConfirmButton), new PropertyMetadata(null));

    public ICommand? SecondaryButtonCommand
    {
        get => (ICommand?)GetValue(SecondaryButtonCommandProperty);
        set => SetValue(SecondaryButtonCommandProperty, value);
    }

    public static readonly DependencyProperty SecondaryButtonCommandParameterProperty = DependencyProperty.Register(
        nameof(SecondaryButtonCommandParameter), typeof(object), typeof(PopConfirmButton), new PropertyMetadata(null));

    public object? SecondaryButtonCommandParameter
    {
        get => GetValue(SecondaryButtonCommandParameterProperty);
        set => SetValue(SecondaryButtonCommandParameterProperty, value);
    }

    public static readonly DependencyProperty IsSecondaryButtonEnabledProperty = DependencyProperty.Register(
        nameof(IsSecondaryButtonEnabled), typeof(bool), typeof(PopConfirmButton), new PropertyMetadata(true));

    public bool IsSecondaryButtonEnabled
    {
        get => (bool)GetValue(IsSecondaryButtonEnabledProperty);
        set => SetValue(IsSecondaryButtonEnabledProperty, value);
    }

    #endregion

    #region Close (footer)

    public static readonly DependencyProperty CloseButtonTextProperty = DependencyProperty.Register(
        nameof(CloseButtonText), typeof(string), typeof(PopConfirmButton),
        new PropertyMetadata(null, OnFooterButtonTextChanged));

    /// <summary>Close/cancel footer label. <c>null</c> → localized Cancel; <c>""</c> → hide.</summary>
    public string? CloseButtonText
    {
        get => (string?)GetValue(CloseButtonTextProperty);
        set => SetValue(CloseButtonTextProperty, value);
    }

    public static readonly DependencyProperty CloseButtonCommandProperty = DependencyProperty.Register(
        nameof(CloseButtonCommand), typeof(ICommand), typeof(PopConfirmButton), new PropertyMetadata(null));

    public ICommand? CloseButtonCommand
    {
        get => (ICommand?)GetValue(CloseButtonCommandProperty);
        set => SetValue(CloseButtonCommandProperty, value);
    }

    public static readonly DependencyProperty CloseButtonCommandParameterProperty = DependencyProperty.Register(
        nameof(CloseButtonCommandParameter), typeof(object), typeof(PopConfirmButton), new PropertyMetadata(null));

    public object? CloseButtonCommandParameter
    {
        get => GetValue(CloseButtonCommandParameterProperty);
        set => SetValue(CloseButtonCommandParameterProperty, value);
    }

    #endregion

    #region Display / visibility (template)

    private static readonly DependencyPropertyKey DisplayPrimaryButtonTextPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(DisplayPrimaryButtonText), typeof(string), typeof(PopConfirmButton),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty DisplayPrimaryButtonTextProperty =
        DisplayPrimaryButtonTextPropertyKey.DependencyProperty;

    public string DisplayPrimaryButtonText => (string)GetValue(DisplayPrimaryButtonTextProperty);

    private static readonly DependencyPropertyKey DisplaySecondaryButtonTextPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(DisplaySecondaryButtonText), typeof(string), typeof(PopConfirmButton),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty DisplaySecondaryButtonTextProperty =
        DisplaySecondaryButtonTextPropertyKey.DependencyProperty;

    public string DisplaySecondaryButtonText => (string)GetValue(DisplaySecondaryButtonTextProperty);

    private static readonly DependencyPropertyKey DisplayCloseButtonTextPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(DisplayCloseButtonText), typeof(string), typeof(PopConfirmButton),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty DisplayCloseButtonTextProperty =
        DisplayCloseButtonTextPropertyKey.DependencyProperty;

    public string DisplayCloseButtonText => (string)GetValue(DisplayCloseButtonTextProperty);

    private static readonly DependencyPropertyKey IsPrimaryButtonVisiblePropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(IsPrimaryButtonVisible), typeof(bool), typeof(PopConfirmButton),
            new PropertyMetadata(true));

    public static readonly DependencyProperty IsPrimaryButtonVisibleProperty =
        IsPrimaryButtonVisiblePropertyKey.DependencyProperty;

    public bool IsPrimaryButtonVisible => (bool)GetValue(IsPrimaryButtonVisibleProperty);

    private static readonly DependencyPropertyKey IsSecondaryButtonVisiblePropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(IsSecondaryButtonVisible), typeof(bool), typeof(PopConfirmButton),
            new PropertyMetadata(false));

    public static readonly DependencyProperty IsSecondaryButtonVisibleProperty =
        IsSecondaryButtonVisiblePropertyKey.DependencyProperty;

    public bool IsSecondaryButtonVisible => (bool)GetValue(IsSecondaryButtonVisibleProperty);

    private static readonly DependencyPropertyKey IsCloseFooterButtonVisiblePropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(IsCloseFooterButtonVisible), typeof(bool), typeof(PopConfirmButton),
            new PropertyMetadata(true));

    public static readonly DependencyProperty IsCloseFooterButtonVisibleProperty =
        IsCloseFooterButtonVisiblePropertyKey.DependencyProperty;

    /// <summary>Whether the footer Close button is visible (independent of chrome X).</summary>
    public bool IsCloseFooterButtonVisible => (bool)GetValue(IsCloseFooterButtonVisibleProperty);

    #endregion

    #region Events

    public static readonly RoutedEvent PrimaryButtonClickEvent = EventManager.RegisterRoutedEvent(
        nameof(PrimaryButtonClick), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PopConfirmButton));

    public event RoutedEventHandler PrimaryButtonClick
    {
        add => AddHandler(PrimaryButtonClickEvent, value);
        remove => RemoveHandler(PrimaryButtonClickEvent, value);
    }

    public static readonly RoutedEvent SecondaryButtonClickEvent = EventManager.RegisterRoutedEvent(
        nameof(SecondaryButtonClick), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PopConfirmButton));

    public event RoutedEventHandler SecondaryButtonClick
    {
        add => AddHandler(SecondaryButtonClickEvent, value);
        remove => RemoveHandler(SecondaryButtonClickEvent, value);
    }

    public static readonly RoutedEvent CloseButtonClickEvent = EventManager.RegisterRoutedEvent(
        nameof(CloseButtonClick), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PopConfirmButton));

    public event RoutedEventHandler CloseButtonClick
    {
        add => AddHandler(CloseButtonClickEvent, value);
        remove => RemoveHandler(CloseButtonClickEvent, value);
    }

    #endregion

    public ICommand InternalPrimaryCommand => _internalPrimaryCommand;

    public ICommand InternalSecondaryCommand => _internalSecondaryCommand;

    public ICommand InternalCloseCommand => _internalCloseCommand;

    static PopConfirmButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(PopConfirmButton),
            new FrameworkPropertyMetadata(typeof(PopConfirmButton)));
    }

    public PopConfirmButton()
    {
        _internalPrimaryCommand = new PopConfirmActionCommand(ExecutePrimary);
        _internalSecondaryCommand = new PopConfirmActionCommand(ExecuteSecondary);
        _internalCloseCommand = new PopConfirmActionCommand(ExecuteClose);
        Unloaded += OnUnloaded;
        RefreshFooterButtonState();
    }

    /// <inheritdoc />
    protected override void OnClick()
    {
        if (IsConfirmOpen)
        {
            CloseFlyout(raiseClose: true);
            return;
        }

        OpenConfirm();
    }

    public void OpenConfirm()
    {
        EnsurePopup();
        RefreshFooterButtonState();
        SetCurrentValue(IsConfirmOpenProperty, true);
    }

    public void CloseConfirm() => CloseFlyout(raiseClose: false);

    private void CloseFlyout(bool raiseClose)
    {
        _skipCloseOnPopupClosed = true;
        SetCurrentValue(IsConfirmOpenProperty, false);

        if (raiseClose)
        {
            RaiseClose();
        }
    }

    private void ExecutePrimary()
    {
        if (_executingFooter)
        {
            return;
        }

        _executingFooter = true;
        _skipCloseOnPopupClosed = true;
        try
        {
            SetCurrentValue(IsConfirmOpenProperty, false);
            TryExecuteCommand(PrimaryButtonCommand, PrimaryButtonCommandParameter);
            RaiseEvent(new RoutedEventArgs(PrimaryButtonClickEvent, this));
        }
        finally
        {
            _executingFooter = false;
        }
    }

    private void ExecuteSecondary()
    {
        if (_executingFooter)
        {
            return;
        }

        _executingFooter = true;
        _skipCloseOnPopupClosed = true;
        try
        {
            SetCurrentValue(IsConfirmOpenProperty, false);
            TryExecuteCommand(SecondaryButtonCommand, SecondaryButtonCommandParameter);
            RaiseEvent(new RoutedEventArgs(SecondaryButtonClickEvent, this));
        }
        finally
        {
            _executingFooter = false;
        }
    }

    private void ExecuteClose() => CloseFlyout(raiseClose: true);

    private void RaiseClose()
    {
        TryExecuteCommand(CloseButtonCommand, CloseButtonCommandParameter);
        RaiseEvent(new RoutedEventArgs(CloseButtonClickEvent, this));
    }

    private static void OnFooterButtonTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((PopConfirmButton)d).RefreshFooterButtonState();
    }

    private void RefreshFooterButtonState()
    {
        // Primary: null → default OK + visible; "" → hide; otherwise custom + visible
        var primaryHidden = PrimaryButtonText == string.Empty;
        SetValue(IsPrimaryButtonVisiblePropertyKey, !primaryHidden);
        SetValue(
            DisplayPrimaryButtonTextPropertyKey,
            primaryHidden
                ? string.Empty
                : (string.IsNullOrEmpty(PrimaryButtonText) ? SH.ButtonOK : PrimaryButtonText));

        // Secondary: only when explicitly set to non-empty
        var secondaryVisible = !string.IsNullOrEmpty(SecondaryButtonText);
        SetValue(IsSecondaryButtonVisiblePropertyKey, secondaryVisible);
        SetValue(DisplaySecondaryButtonTextPropertyKey, secondaryVisible ? SecondaryButtonText! : string.Empty);

        // Close footer: null → default Cancel + visible; "" → hide
        var closeHidden = CloseButtonText == string.Empty;
        SetValue(IsCloseFooterButtonVisiblePropertyKey, !closeHidden);
        SetValue(
            DisplayCloseButtonTextPropertyKey,
            closeHidden
                ? string.Empty
                : (string.IsNullOrEmpty(CloseButtonText) ? SH.ButtonCancel : CloseButtonText));
    }

    private void EnsurePopup()
    {
        if (_popup is not null)
        {
            return;
        }

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

    private static void OnIsConfirmOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var owner = (PopConfirmButton)d;
        if (owner._suppressIsOpenCallback)
        {
            return;
        }

        owner.EnsurePopup();
        if (owner._popup is null)
        {
            return;
        }

        if ((bool)e.NewValue!)
        {
            owner.RefreshFooterButtonState();
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

        if (_skipCloseOnPopupClosed)
        {
            _skipCloseOnPopupClosed = false;
            return;
        }

        if (!_executingFooter)
        {
            RaiseClose();
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

#pragma warning disable CS0067
        public event EventHandler? CanExecuteChanged;
#pragma warning restore CS0067
    }
}
