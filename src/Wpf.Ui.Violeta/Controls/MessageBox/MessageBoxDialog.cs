using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Wpf.Ui.Controls;
using Wpf.Ui.Violeta.Win32;
using Button = System.Windows.Controls.Button;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxResult = System.Windows.MessageBoxResult;

namespace Wpf.Ui.Violeta.Controls;

[TemplatePart(Name = "PART_OKButton", Type = typeof(Button))]
[TemplatePart(Name = "PART_YesButton", Type = typeof(Button))]
[TemplatePart(Name = "PART_NoButton", Type = typeof(Button))]
[TemplatePart(Name = "PART_CancelButton", Type = typeof(Button))]
public partial class MessageBoxDialog : Window
{
    private const string OKVisibleState = "OKVisible";
    private const string OKCancelVisibleState = "OKCancelVisible";
    private const string YesNoCancelVisibleState = "YesNoCancelVisible";
    private const string YesNoVisibleState = "YesNoVisible";

    private const string OKAsDefaultButtonState = "OKAsDefaultButton";
    private const string YesAsDefaultButtonState = "YesAsDefaultButton";

    private const string IconVisibleState = "IconVisible";
    private const string IconCollapsedState = "IconCollapsed";

    private const string TitleVisibleState = "TitleVisible";
    private const string TitleCollapsedState = "TitleCollapsed";

    public MessageBoxResult Result;

    private Button OKButton { get; set; } = null!;
    private Button YesButton { get; set; } = null!;
    private Button NoButton { get; set; } = null!;
    private Button CancelButton { get; set; } = null!;

    static MessageBoxDialog()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(MessageBoxDialog), new FrameworkPropertyMetadata(typeof(MessageBoxDialog)));
    }

    public MessageBoxDialog()
    {
        SetValue(TemplateSettingsPropertyKey, new MessageBoxTemplateSettings());
        Loaded += OnLoaded;

        RoutedCommand copyCommand = new("CopyCommand", typeof(MessageBoxDialog));
        CommandBindings.Add(new CommandBinding(copyCommand, CopyExecuted));
        InputBindings.Add(new KeyBinding(copyCommand, new KeyGesture(Key.C, ModifierKeys.Control)));
    }

    public MessageBoxIcon MessageBoxIcon
    {
        get => (MessageBoxIcon)GetValue(MessageBoxIconProperty);
        set => SetValue(MessageBoxIconProperty, value);
    }

    public static readonly DependencyProperty MessageBoxIconProperty = DependencyProperty.Register("MessageBoxIcon", typeof(MessageBoxIcon), typeof(MessageBoxDialog), new(MessageBoxIcon.Information));

    public static readonly DependencyProperty CaptionProperty =
        DependencyProperty.Register(nameof(Caption), typeof(object), typeof(MessageBoxDialog));

    public object Caption
    {
        get => GetValue(CaptionProperty);
        set => SetValue(CaptionProperty, value);
    }

    public static readonly DependencyProperty CaptionTemplateProperty =
        DependencyProperty.Register(nameof(CaptionTemplate), typeof(DataTemplate), typeof(MessageBoxDialog));

    public DataTemplate CaptionTemplate
    {
        get => (DataTemplate)GetValue(CaptionTemplateProperty);
        set => SetValue(CaptionTemplateProperty, value);
    }

    public static readonly DependencyProperty OKButtonTextProperty =
        DependencyProperty.Register(nameof(OKButtonText), typeof(string), typeof(MessageBoxDialog), new PropertyMetadata(string.Empty, OnButtonTextChanged));

    public string OKButtonText
    {
        get => (string)GetValue(OKButtonTextProperty);
        set => SetValue(OKButtonTextProperty, value);
    }

    public static readonly DependencyProperty OKButtonCommandProperty =
        DependencyProperty.Register(nameof(OKButtonCommand), typeof(ICommand), typeof(MessageBoxDialog), null);

    public ICommand OKButtonCommand
    {
        get => (ICommand)GetValue(OKButtonCommandProperty);
        set => SetValue(OKButtonCommandProperty, value);
    }

    public static readonly DependencyProperty OKButtonCommandParameterProperty =
        DependencyProperty.Register(nameof(OKButtonCommandParameter), typeof(object), typeof(MessageBoxDialog), null);

    public object OKButtonCommandParameter
    {
        get => GetValue(OKButtonCommandParameterProperty);
        set => SetValue(OKButtonCommandParameterProperty, value);
    }

    public static readonly DependencyProperty OKButtonStyleProperty =
        DependencyProperty.Register(nameof(OKButtonStyle), typeof(Style), typeof(MessageBoxDialog), null);

    public Style OKButtonStyle
    {
        get => (Style)GetValue(OKButtonStyleProperty);
        set => SetValue(OKButtonStyleProperty, value);
    }

    public static readonly DependencyProperty YesButtonTextProperty =
        DependencyProperty.Register(nameof(YesButtonText), typeof(string), typeof(MessageBoxDialog), new PropertyMetadata(string.Empty, OnButtonTextChanged));

    public string YesButtonText
    {
        get => (string)GetValue(YesButtonTextProperty);
        set => SetValue(YesButtonTextProperty, value);
    }

    public static readonly DependencyProperty YesButtonCommandProperty =
        DependencyProperty.Register(nameof(YesButtonCommand), typeof(ICommand), typeof(MessageBoxDialog), null);

    public ICommand YesButtonCommand
    {
        get => (ICommand)GetValue(YesButtonCommandProperty);
        set => SetValue(YesButtonCommandProperty, value);
    }

    public static readonly DependencyProperty YesButtonCommandParameterProperty =
        DependencyProperty.Register(nameof(YesButtonCommandParameter), typeof(object), typeof(MessageBoxDialog), null);

    public object YesButtonCommandParameter
    {
        get => GetValue(YesButtonCommandParameterProperty);
        set => SetValue(YesButtonCommandParameterProperty, value);
    }

    public static readonly DependencyProperty YesButtonStyleProperty =
        DependencyProperty.Register(nameof(YesButtonStyle), typeof(Style), typeof(MessageBoxDialog), null);

    public Style YesButtonStyle
    {
        get => (Style)GetValue(YesButtonStyleProperty);
        set => SetValue(YesButtonStyleProperty, value);
    }

    public static readonly DependencyProperty NoButtonTextProperty =
        DependencyProperty.Register(nameof(NoButtonText), typeof(string), typeof(MessageBoxDialog), new PropertyMetadata(string.Empty, OnButtonTextChanged));

    public string NoButtonText
    {
        get => (string)GetValue(NoButtonTextProperty);
        set => SetValue(NoButtonTextProperty, value);
    }

    public static readonly DependencyProperty NoButtonCommandProperty =
        DependencyProperty.Register(nameof(NoButtonCommand), typeof(ICommand), typeof(MessageBoxDialog), null);

    public ICommand NoButtonCommand
    {
        get => (ICommand)GetValue(NoButtonCommandProperty);
        set => SetValue(NoButtonCommandProperty, value);
    }

    public static readonly DependencyProperty NoButtonCommandParameterProperty =
        DependencyProperty.Register(nameof(NoButtonCommandParameter), typeof(object), typeof(MessageBoxDialog), null);

    public object NoButtonCommandParameter
    {
        get => GetValue(NoButtonCommandParameterProperty);
        set => SetValue(NoButtonCommandParameterProperty, value);
    }

    public static readonly DependencyProperty NoButtonStyleProperty =
        DependencyProperty.Register(nameof(NoButtonStyle), typeof(Style), typeof(MessageBoxDialog), null);

    public Style NoButtonStyle
    {
        get => (Style)GetValue(NoButtonStyleProperty);
        set => SetValue(NoButtonStyleProperty, value);
    }

    public static readonly DependencyProperty CancelButtonTextProperty =
        DependencyProperty.Register(nameof(CancelButtonText), typeof(string), typeof(MessageBoxDialog), new PropertyMetadata(string.Empty, OnButtonTextChanged));

    public string CancelButtonText
    {
        get => (string)GetValue(CancelButtonTextProperty);
        set => SetValue(CancelButtonTextProperty, value);
    }

    public static readonly DependencyProperty CancelButtonCommandProperty =
        DependencyProperty.Register(nameof(CancelButtonCommand), typeof(ICommand), typeof(MessageBoxDialog), null);

    public ICommand CancelButtonCommand
    {
        get => (ICommand)GetValue(CancelButtonCommandProperty);
        set => SetValue(CancelButtonCommandProperty, value);
    }

    public static readonly DependencyProperty CancelButtonCommandParameterProperty =
        DependencyProperty.Register(nameof(CancelButtonCommandParameter), typeof(object), typeof(MessageBoxDialog), null);

    public object CancelButtonCommandParameter
    {
        get => GetValue(CancelButtonCommandParameterProperty);
        set => SetValue(CancelButtonCommandParameterProperty, value);
    }

    public static readonly DependencyProperty CancelButtonStyleProperty =
        DependencyProperty.Register(nameof(CancelButtonStyle), typeof(Style), typeof(MessageBoxDialog), null);

    public Style CancelButtonStyle
    {
        get => (Style)GetValue(CancelButtonStyleProperty);
        set => SetValue(CancelButtonStyleProperty, value);
    }

    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(MessageBoxDialog));

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public IconSource IconSource
    {
        get => (IconSource)GetValue(IconSourceProperty);
        set => SetValue(IconSourceProperty, value);
    }

    public static readonly DependencyProperty IconSourceProperty =
        DependencyProperty.Register(nameof(IconSource), typeof(IconSource), typeof(MessageBoxDialog), new PropertyMetadata(OnIconSourcePropertyChanged));

    private static void OnIconSourcePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        ((MessageBoxDialog)sender).OnIconSourcePropertyChanged(args);
    }

    public MessageBoxButton MessageBoxButtons
    {
        get => (MessageBoxButton)GetValue(MessageBoxButtonsProperty);
        set => SetValue(MessageBoxButtonsProperty, value);
    }

    public static readonly DependencyProperty MessageBoxButtonsProperty =
        DependencyProperty.Register(nameof(MessageBoxButtons), typeof(MessageBoxButton), typeof(MessageBoxDialog), new PropertyMetadata(OnMessageBoxButtonsPropertyChanged));

    private static void OnMessageBoxButtonsPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        ((MessageBoxDialog)sender).UpdateMessageBoxButtonState();
    }

    public MessageBoxResult DefaultResult
    {
        get => (MessageBoxResult)GetValue(DefaultResultProperty);
        set => SetValue(DefaultResultProperty, value);
    }

    public static readonly DependencyProperty DefaultResultProperty =
        DependencyProperty.Register(nameof(DefaultResult), typeof(MessageBoxResult), typeof(MessageBoxDialog));

    private static readonly DependencyPropertyKey TemplateSettingsPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(TemplateSettings), typeof(MessageBoxTemplateSettings), typeof(MessageBoxDialog), null);

    public static readonly DependencyProperty TemplateSettingsProperty =
        TemplateSettingsPropertyKey.DependencyProperty;

    public MessageBoxTemplateSettings TemplateSettings
    {
        get => (MessageBoxTemplateSettings)GetValue(TemplateSettingsProperty);
    }

    public event TypedEventHandler<MessageBoxDialog, MessageBoxOpenedEventArgs>? Opened;

    public new event TypedEventHandler<MessageBoxDialog, MessageBoxClosingEventArgs>? Closing;

    public new event TypedEventHandler<MessageBoxDialog, MessageBoxClosedEventArgs>? Closed;

    public event TypedEventHandler<MessageBoxDialog, MessageBoxButtonClickEventArgs>? OKButtonClick;

    public event TypedEventHandler<MessageBoxDialog, MessageBoxButtonClickEventArgs>? YesButtonClick;

    public event TypedEventHandler<MessageBoxDialog, MessageBoxButtonClickEventArgs>? NoButtonClick;

    public event TypedEventHandler<MessageBoxDialog, MessageBoxButtonClickEventArgs>? CancelButtonClick;

    private static void OnButtonTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((MessageBoxDialog)d).UpdateButtonTextState();
    }

    private void OnIconSourcePropertyChanged(DependencyPropertyChangedEventArgs args)
    {
        if (args.NewValue is IconSource iconSource)
        {
            TemplateSettings.IconElement = iconSource.MakeIconElementFrom();
        }
        else
        {
            TemplateSettings.ClearValue(MessageBoxTemplateSettings.IconElementProperty);
        }
        UpdateIconState();
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (OKButton != null)
        {
            OKButton.Click -= OnButtonClick;
        }

        if (YesButton != null)
        {
            YesButton.Click -= OnButtonClick;
        }

        if (NoButton != null)
        {
            NoButton.Click -= OnButtonClick;
        }

        if (CancelButton != null)
        {
            CancelButton.Click -= OnButtonClick;
        }

        OKButton = (Button)GetTemplateChild("PART_OKButton");
        YesButton = (Button)GetTemplateChild("PART_YesButton");
        NoButton = (Button)GetTemplateChild("PART_NoButton");
        CancelButton = (Button)GetTemplateChild("PART_CancelButton");

        if (OKButton != null)
        {
            OKButton.Click += OnButtonClick;
        }

        if (YesButton != null)
        {
            YesButton.Click += OnButtonClick;
        }

        if (NoButton != null)
        {
            NoButton.Click += OnButtonClick;
        }

        if (CancelButton != null)
        {
            CancelButton.Click += OnButtonClick;
            CancelButton.IsCancel = true;
        }

        UpdateIconState();
        UpdateMessageState();
        UpdateButtonTextState();
        UpdateMessageBoxButtonState();
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        InvalidateMeasure();

        if (WindowBackdrop.IsSupported(WindowBackdropType.Mica))
        {
            Background = new SolidColorBrush(Colors.Transparent);
            WindowBackdrop.ApplyBackdrop(this, WindowBackdropType.Mica);
        }
    }

    private void OnButtonClick(object? sender, RoutedEventArgs e)
    {
        if (sender == OKButton)
        {
            HandleButtonClick(
                OKButtonClick,
                OKButtonCommand,
                OKButtonCommandParameter,
                MessageBoxResult.OK);
        }
        else if (sender == YesButton)
        {
            HandleButtonClick(
                YesButtonClick,
                YesButtonCommand,
                YesButtonCommandParameter,
                MessageBoxResult.Yes);
        }
        else if (sender == NoButton)
        {
            HandleButtonClick(
                NoButtonClick,
                NoButtonCommand,
                NoButtonCommandParameter,
                MessageBoxResult.No);
        }
        else if (sender == CancelButton)
        {
            HandleButtonClick(
                CancelButtonClick,
                CancelButtonCommand,
                CancelButtonCommandParameter,
                MessageBoxResult.Cancel);
        }
    }

    private void HandleButtonClick(
        TypedEventHandler<MessageBoxDialog, MessageBoxButtonClickEventArgs>? handler,
        ICommand command,
        object commandParameter,
        MessageBoxResult result)
    {
        if (handler != null)
        {
            var args = new MessageBoxButtonClickEventArgs();

            var deferral = new MessageBoxButtonClickDeferral(() =>
            {
                if (!args.Cancel)
                {
                    TryExecuteCommand(command, commandParameter);
                    Close(result);
                }
            });

            args.SetDeferral(deferral);

            args.IncrementDeferralCount();
            handler(this, args);
            args.DecrementDeferralCount();
        }
        else
        {
            TryExecuteCommand(command, commandParameter);
            Close(result);
        }
    }

    private void UpdateButtonTextState()
    {
        MessageBoxTemplateSettings templateSettings = TemplateSettings;
        templateSettings.OKButtonText = string.IsNullOrEmpty(OKButtonText) ? GetString(User32.DialogBoxCommand.IDOK) : OKButtonText;
        templateSettings.YesButtonText = string.IsNullOrEmpty(YesButtonText) ? GetString(User32.DialogBoxCommand.IDYES) : YesButtonText;
        templateSettings.NoButtonText = string.IsNullOrEmpty(NoButtonText) ? GetString(User32.DialogBoxCommand.IDNO) : NoButtonText;
        templateSettings.CancelButtonText = string.IsNullOrEmpty(CancelButtonText) ? GetString(User32.DialogBoxCommand.IDCANCEL) : CancelButtonText;
    }

    [SuppressMessage("Performance", "SYSLIB1045:Convert to 'GeneratedRegexAttribute'.")]
    [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression")]
    private static string GetString(User32.DialogBoxCommand wBtn)
    {
        nint strPtr = User32.MB_GetString((uint)wBtn);
        StringBuilder sb = new(Marshal.PtrToStringAuto(strPtr));
        return Regex.Replace(sb.Replace("&", string.Empty).ToString(), @"\([^)]*\)", string.Empty);
    }

    private void UpdateMessageState()
    {
        string stateName = Caption == null || (Caption is string && string.IsNullOrEmpty((string)Caption)) ? TitleCollapsedState : TitleVisibleState;
        VisualStateManager.GoToState(this, stateName, true);
    }

    private void UpdateIconState()
    {
        string stateName = TemplateSettings.IconElement == null ? IconCollapsedState : IconVisibleState;
        VisualStateManager.GoToState(this, stateName, true);
    }

    private void UpdateMessageBoxButtonState()
    {
        string stateName;

        MessageBoxButton button = MessageBoxButtons;

        switch (button)
        {
            case MessageBoxButton.OK:
                stateName = OKVisibleState;
                OKButton?.Focus();
                break;

            case MessageBoxButton.OKCancel:
                stateName = OKCancelVisibleState;
                OKButton?.Focus();
                break;

            case MessageBoxButton.YesNoCancel:
                stateName = YesNoCancelVisibleState;
                YesButton?.Focus();
                break;

            case MessageBoxButton.YesNo:
                stateName = YesNoVisibleState;
                YesButton?.Focus();
                break;

            default:
                stateName = OKVisibleState;
                OKButton?.Focus();
                break;
        }

        VisualStateManager.GoToState(this, stateName, true);

        stateName = button switch
        {
            MessageBoxButton.OK => OKAsDefaultButtonState,
            MessageBoxButton.OKCancel => OKAsDefaultButtonState,
            MessageBoxButton.YesNoCancel => YesAsDefaultButtonState,
            MessageBoxButton.YesNo => YesAsDefaultButtonState,
            _ => OKAsDefaultButtonState,
        };
        VisualStateManager.GoToState(this, stateName, true);
    }

    /// <summary>
    /// Opens a Message Box and returns only when the newly opened window is closed.
    /// </summary>
    /// <returns>A <see cref="MessageBoxResult"/> value that specifies which message box button is clicked by the user.</returns>
    public new MessageBoxResult ShowDialog()
    {
        if (Owner != null)
        {
            // Inherit the topmost state from the owner window
            Topmost = Owner.Topmost;
        }

        base.ShowDialog();
        return Result;
    }

    public void Close(MessageBoxResult result)
    {
        var closing = Closing;
        if (closing != null)
        {
            var args = new MessageBoxClosingEventArgs(result);

            var deferral = new MessageBoxClosingDeferral(() =>
            {
                if (!args.Cancel)
                {
                    Result = result;
                    Close();
                    Closed?.Invoke(this, new MessageBoxClosedEventArgs(result));
                }
            });

            args.SetDeferral(deferral);

            args.IncrementDeferralCount();
            closing(this, args);
            args.DecrementDeferralCount();
        }
        else
        {
            Result = result;
            Close();
            Closed?.Invoke(this, new MessageBoxClosedEventArgs(result));
        }
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        Loaded -= OnLoaded;
        Opened?.Invoke(this, new MessageBoxOpenedEventArgs());
    }

    private static void TryExecuteCommand(ICommand command, object parameter)
    {
        if (command != null && command.CanExecute(parameter))
        {
            command.Execute(parameter);
        }
    }

    private void CopyExecuted(object? sender, ExecutedRoutedEventArgs e)
    {
        try
        {
            Clipboard.SetText(Content?.ToString() ?? string.Empty);
        }
        catch
        {
            ///
        }
    }
}

file static class UIExtensions
{
    public static IconElement MakeIconElementFrom(this IconSource iconSource)
    {
        if (iconSource is FontIconSource fontIconSource)
        {
            FontIcon fontIcon = new()
            {
                Glyph = fontIconSource.Glyph,
                FontSize = fontIconSource.FontSize,
            };
            var newForeground = fontIconSource.Foreground;
            if (newForeground != null)
            {
                fontIcon.Foreground = newForeground;
            }

            if (fontIconSource.FontFamily != null)
            {
                fontIcon.FontFamily = fontIconSource.FontFamily;
            }

            fontIcon.FontWeight = fontIconSource.FontWeight;
            fontIcon.FontStyle = fontIconSource.FontStyle;

            return fontIcon;
        }
        else if (iconSource is SymbolIconSource symbolIconSource)
        {
            SymbolIcon symbolIcon = new()
            {
                Symbol = symbolIconSource.Symbol,
            };
            var newForeground = symbolIconSource.Foreground;
            if (newForeground != null)
            {
                symbolIcon.Foreground = newForeground;
            }
            return symbolIcon;
        }
        return null!;
    }
}
