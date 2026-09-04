using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Wpf.Ui.Violeta.Controls;

[TemplatePart(Name = nameof(PrimaryButton), Type = typeof(Button))]
[TemplatePart(Name = nameof(SecondaryButton), Type = typeof(Button))]
[TemplatePart(Name = nameof(CloseButton), Type = typeof(Button))]
public class ContentWindowDialogControl : ContentWindowControl
{
    static ContentWindowDialogControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ContentWindowDialogControl), new FrameworkPropertyMetadata(typeof(ContentWindowDialogControl)));
    }

    public static readonly DependencyProperty PrimaryButtonTextProperty = DependencyProperty.Register(
        nameof(PrimaryButtonText), typeof(string), typeof(ContentWindowDialogControl),
        new PropertyMetadata(null, OnButtonTextChanged));

    public string? PrimaryButtonText
    {
        get => (string?)GetValue(PrimaryButtonTextProperty);
        set => SetValue(PrimaryButtonTextProperty, value);
    }

    public static readonly DependencyProperty PrimaryButtonCommandProperty = DependencyProperty.Register(
        nameof(PrimaryButtonCommand), typeof(ICommand), typeof(ContentWindowDialogControl), new PropertyMetadata(null));

    public ICommand? PrimaryButtonCommand
    {
        get => (ICommand?)GetValue(PrimaryButtonCommandProperty);
        set => SetValue(PrimaryButtonCommandProperty, value);
    }

    public static readonly DependencyProperty PrimaryButtonCommandParameterProperty = DependencyProperty.Register(
        nameof(PrimaryButtonCommandParameter), typeof(object), typeof(ContentWindowDialogControl), new PropertyMetadata(null));

    public object? PrimaryButtonCommandParameter
    {
        get => GetValue(PrimaryButtonCommandParameterProperty);
        set => SetValue(PrimaryButtonCommandParameterProperty, value);
    }

    public static readonly DependencyProperty IsPrimaryButtonEnabledProperty = DependencyProperty.Register(
        nameof(IsPrimaryButtonEnabled), typeof(bool), typeof(ContentWindowDialogControl), new PropertyMetadata(true));

    public bool IsPrimaryButtonEnabled
    {
        get => (bool)GetValue(IsPrimaryButtonEnabledProperty);
        set => SetValue(IsPrimaryButtonEnabledProperty, value);
    }

    public static readonly DependencyProperty SecondaryButtonTextProperty = DependencyProperty.Register(
        nameof(SecondaryButtonText), typeof(string), typeof(ContentWindowDialogControl),
        new PropertyMetadata(null, OnButtonTextChanged));

    public string? SecondaryButtonText
    {
        get => (string?)GetValue(SecondaryButtonTextProperty);
        set => SetValue(SecondaryButtonTextProperty, value);
    }

    public static readonly DependencyProperty SecondaryButtonCommandProperty = DependencyProperty.Register(
        nameof(SecondaryButtonCommand), typeof(ICommand), typeof(ContentWindowDialogControl), new PropertyMetadata(null));

    public ICommand? SecondaryButtonCommand
    {
        get => (ICommand?)GetValue(SecondaryButtonCommandProperty);
        set => SetValue(SecondaryButtonCommandProperty, value);
    }

    public static readonly DependencyProperty SecondaryButtonCommandParameterProperty = DependencyProperty.Register(
        nameof(SecondaryButtonCommandParameter), typeof(object), typeof(ContentWindowDialogControl), new PropertyMetadata(null));

    public object? SecondaryButtonCommandParameter
    {
        get => GetValue(SecondaryButtonCommandParameterProperty);
        set => SetValue(SecondaryButtonCommandParameterProperty, value);
    }

    public static readonly DependencyProperty IsSecondaryButtonEnabledProperty = DependencyProperty.Register(
        nameof(IsSecondaryButtonEnabled), typeof(bool), typeof(ContentWindowDialogControl), new PropertyMetadata(true));

    public bool IsSecondaryButtonEnabled
    {
        get => (bool)GetValue(IsSecondaryButtonEnabledProperty);
        set => SetValue(IsSecondaryButtonEnabledProperty, value);
    }

    public static readonly DependencyProperty CloseButtonTextProperty = DependencyProperty.Register(
        nameof(CloseButtonText), typeof(string), typeof(ContentWindowDialogControl),
        new PropertyMetadata(null, OnButtonTextChanged));

    public string? CloseButtonText
    {
        get => (string?)GetValue(CloseButtonTextProperty);
        set => SetValue(CloseButtonTextProperty, value);
    }

    public static readonly DependencyProperty CloseButtonCommandProperty = DependencyProperty.Register(
        nameof(CloseButtonCommand), typeof(ICommand), typeof(ContentWindowDialogControl), new PropertyMetadata(null));

    public ICommand? CloseButtonCommand
    {
        get => (ICommand?)GetValue(CloseButtonCommandProperty);
        set => SetValue(CloseButtonCommandProperty, value);
    }

    public static readonly DependencyProperty CloseButtonCommandParameterProperty = DependencyProperty.Register(
        nameof(CloseButtonCommandParameter), typeof(object), typeof(ContentWindowDialogControl), new PropertyMetadata(null));

    public object? CloseButtonCommandParameter
    {
        get => GetValue(CloseButtonCommandParameterProperty);
        set => SetValue(CloseButtonCommandParameterProperty, value);
    }

    public static readonly DependencyProperty DefaultButtonProperty = DependencyProperty.Register(
        nameof(DefaultButton), typeof(ContentDialogButton), typeof(ContentWindowDialogControl), new PropertyMetadata(OnDefaultButtonChanged));

    public ContentDialogButton DefaultButton
    {
        get => (ContentDialogButton)GetValue(DefaultButtonProperty);
        set => SetValue(DefaultButtonProperty, value);
    }

    public static readonly DependencyProperty ButtonAlignmentProperty = DependencyProperty.Register(
        nameof(ButtonAlignment), typeof(ContentWindowButtonAlignment), typeof(ContentWindowDialogControl),
        new PropertyMetadata(ContentWindowButtonAlignment.Stretch, OnButtonAlignmentChanged));

    public ContentWindowButtonAlignment ButtonAlignment
    {
        get => (ContentWindowButtonAlignment)GetValue(ButtonAlignmentProperty);
        set => SetValue(ButtonAlignmentProperty, value);
    }

    public event Wpf.Ui.Controls.TypedEventHandler<ContentWindowDialogControl, ContentWindowOpenedEventArgs>? Opened;

    public event Wpf.Ui.Controls.TypedEventHandler<ContentWindowDialogControl, ContentWindowClosingEventArgs>? Closing;

    public event Wpf.Ui.Controls.TypedEventHandler<ContentWindowDialogControl, ContentWindowClosedEventArgs>? Closed;

    public event Wpf.Ui.Controls.TypedEventHandler<ContentWindowDialogControl, ContentDialogButtonClickEventArgs>? PrimaryButtonClick;

    public event Wpf.Ui.Controls.TypedEventHandler<ContentWindowDialogControl, ContentDialogButtonClickEventArgs>? SecondaryButtonClick;

    public event Wpf.Ui.Controls.TypedEventHandler<ContentWindowDialogControl, ContentDialogButtonClickEventArgs>? CloseButtonClick;

    private Wpf.Ui.Controls.Button PrimaryButton { get; set; } = null!;

    private Wpf.Ui.Controls.Button SecondaryButton { get; set; } = null!;

    private Wpf.Ui.Controls.Button CloseButton { get; set; } = null!;

    private Grid? CommandArea { get; set; }

    private ContentWindow? m_subscribedOwner;

    private bool m_closingInProgress;

    private bool m_openedRaised;

    public ContentWindowDialogControl()
    {
        Loaded += (_, _) => SubscribeOwnerEvents();
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (PrimaryButton != null)
        {
            PrimaryButton.Click -= OnButtonClick;
        }

        if (SecondaryButton != null)
        {
            SecondaryButton.Click -= OnButtonClick;
        }

        if (CloseButton != null)
        {
            CloseButton.Click -= OnButtonClick;
        }

        PrimaryButton = (Wpf.Ui.Controls.Button)GetTemplateChild(nameof(PrimaryButton));
        SecondaryButton = (Wpf.Ui.Controls.Button)GetTemplateChild(nameof(SecondaryButton));
        CloseButton = (Wpf.Ui.Controls.Button)GetTemplateChild(nameof(CloseButton));
        CommandArea = GetTemplateChild(nameof(CommandArea)) as Grid;

        if (PrimaryButton != null)
        {
            PrimaryButton.Click += OnButtonClick;
        }

        if (SecondaryButton != null)
        {
            SecondaryButton.Click += OnButtonClick;
        }

        if (CloseButton != null)
        {
            CloseButton.Click += OnButtonClick;
        }

        UpdateCommandArea();
        SubscribeOwnerEvents();
    }

    protected override void ResultCommandExecuted(object? sender, ContentWindowResultEventArgs e)
    {
        base.ResultCommandExecuted(sender, e);

        if (e.Handled)
        {
            return;
        }

        if (m_closingInProgress)
        {
            // The close was already requested by one of our buttons or the Esc key,
            // and the Closing event was already raised in RequestClose.
            m_closingInProgress = false;
            return;
        }

        // System-initiated close (title bar X, Alt+F4, ...). Give subscribers a chance to cancel.
        RaiseClosingAndBlockIfCanceled(e.DialogResult, e);
    }

    private void OnOwnerKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Handled)
        {
            return;
        }

        switch (e.Key)
        {
            case Key.Enter:
                var button = GetDefaultButtonForEnter();
                if (button != null && button.IsVisible && button.IsEnabled)
                {
                    OnButtonClick(button, null!);
                    e.Handled = true;
                }

                break;

            case Key.Escape:
                e.Handled = true;
                RequestClose(Owner?.CancelResult ?? ContentWindowResult.Cancel);
                break;
        }
    }

    private void OnOwnerLoaded(object? sender, RoutedEventArgs e)
    {
        if (!m_openedRaised)
        {
            m_openedRaised = true;
            Opened?.Invoke(this, new ContentWindowOpenedEventArgs());
        }
    }

    private void OnOwnerClosed(object? sender, EventArgs e)
    {
        if (Owner is { } owner)
        {
            Closed?.Invoke(this, new ContentWindowClosedEventArgs(owner.Result));
        }
    }

    private void OnButtonClick(object? sender, RoutedEventArgs e)
    {
        if (sender == PrimaryButton)
        {
            HandleButtonClick(
                PrimaryButtonClick,
                PrimaryButtonCommand,
                PrimaryButtonCommandParameter,
                ContentWindowResult.OK);
        }
        else if (sender == SecondaryButton)
        {
            HandleButtonClick(
                SecondaryButtonClick,
                SecondaryButtonCommand,
                SecondaryButtonCommandParameter,
                ContentWindowResult.Cancel);
        }
        else if (sender == CloseButton)
        {
            HandleButtonClick(
                CloseButtonClick,
                CloseButtonCommand,
                CloseButtonCommandParameter,
                ContentWindowResult.None);
        }
    }

    private void HandleButtonClick(
        Wpf.Ui.Controls.TypedEventHandler<ContentWindowDialogControl, ContentDialogButtonClickEventArgs>? handler,
        ICommand? command,
        object? commandParameter,
        ContentWindowResult result)
    {
        if (handler != null)
        {
            var args = new ContentDialogButtonClickEventArgs();

            var deferral = new ContentDialogButtonClickDeferral(() =>
            {
                if (!args.Cancel)
                {
                    TryExecuteCommand(command, commandParameter);
                    RequestClose(result);
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
            RequestClose(result);
        }
    }

    private void RequestClose(ContentWindowResult result)
    {
        if (Closing is { } closing)
        {
            var args = new ContentWindowClosingEventArgs(result);

            var deferral = new ContentWindowClosingDeferral(() =>
            {
                if (!args.Cancel)
                {
                    CloseWithResult(result);
                }
            });

            args.SetDeferral(deferral);
            args.IncrementDeferralCount();
            closing(this, args);
            args.DecrementDeferralCount();
        }
        else
        {
            CloseWithResult(result);
        }
    }

    private void CloseWithResult(ContentWindowResult result)
    {
        m_closingInProgress = true;
        Owner?.OnResultCommandExecuted(result);
    }

    private void RaiseClosingAndBlockIfCanceled(ContentWindowResult result, ContentWindowResultEventArgs resultArgs)
    {
        if (Closing is { } closing)
        {
            var args = new ContentWindowClosingEventArgs(result);

            var deferral = new ContentWindowClosingDeferral(() =>
            {
            });

            args.SetDeferral(deferral);
            args.IncrementDeferralCount();
            closing(this, args);
            args.DecrementDeferralCount();

            if (args.Cancel)
            {
                resultArgs.Handled = true;
            }
        }
    }

    private Wpf.Ui.Controls.Button? GetDefaultButtonForEnter()
    {
        if (DefaultButton == ContentDialogButton.None)
        {
            return AllButtons().FirstOrDefault(b => b.IsVisible && b.IsEnabled);
        }

        return DefaultButton switch
        {
            ContentDialogButton.Primary => PrimaryButton,
            ContentDialogButton.Secondary => SecondaryButton,
            ContentDialogButton.Close => CloseButton,
            _ => null,
        };
    }

    private void UpdateCommandArea()
    {
        var buttons = AllButtons();
        if (buttons.Count == 0 || CommandArea is not { } commandArea)
        {
            return;
        }

        var visible = buttons.Where(b => !string.IsNullOrEmpty(GetButtonText(b))).ToList();
        bool isStretch = ButtonAlignment == ContentWindowButtonAlignment.Stretch;

        // 无任何底部按钮时收起整块 CommandArea，避免仍占着 Margin 空白
        commandArea.Visibility = visible.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
        if (visible.Count == 0)
        {
            foreach (var button in buttons)
            {
                button.Visibility = Visibility.Collapsed;
            }

            return;
        }

        commandArea.HorizontalAlignment = isStretch
            ? HorizontalAlignment.Stretch
            : ButtonAlignment switch
            {
                ContentWindowButtonAlignment.Left => HorizontalAlignment.Left,
                ContentWindowButtonAlignment.Center => HorizontalAlignment.Center,
                _ => HorizontalAlignment.Right,
            };

        commandArea.ColumnDefinitions.Clear();

        int columnIndex = 0;
        foreach (var button in buttons)
        {
            if (!visible.Contains(button))
            {
                button.Visibility = Visibility.Collapsed;
                continue;
            }

            button.Visibility = Visibility.Visible;
            Grid.SetColumn(button, columnIndex);
            button.HorizontalAlignment = isStretch ? HorizontalAlignment.Stretch : HorizontalAlignment.Left;
            commandArea.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = isStretch ? new GridLength(1, GridUnitType.Star) : GridLength.Auto,
            });
            columnIndex++;
        }

        var defaultButton = DefaultButton switch
        {
            ContentDialogButton.Primary => PrimaryButton,
            ContentDialogButton.Secondary => SecondaryButton,
            ContentDialogButton.Close => CloseButton,
            _ => null,
        };

        foreach (var button in buttons)
        {
            button.Appearance = button == defaultButton
                ? Wpf.Ui.Controls.ControlAppearance.Primary
                : Wpf.Ui.Controls.ControlAppearance.Secondary;
        }
    }

    private List<Wpf.Ui.Controls.Button> AllButtons()
    {
        var buttons = new List<Wpf.Ui.Controls.Button>(3);

        if (PrimaryButton != null)
        {
            buttons.Add(PrimaryButton);
        }

        if (SecondaryButton != null)
        {
            buttons.Add(SecondaryButton);
        }

        if (CloseButton != null)
        {
            buttons.Add(CloseButton);
        }

        return buttons;
    }

    private string? GetButtonText(Wpf.Ui.Controls.Button button)
    {
        if (button == PrimaryButton)
        {
            return PrimaryButtonText;
        }

        if (button == SecondaryButton)
        {
            return SecondaryButtonText;
        }

        if (button == CloseButton)
        {
            return CloseButtonText;
        }

        return null;
    }

    private void SubscribeOwnerEvents()
    {
        if (m_subscribedOwner != null)
        {
            m_subscribedOwner.KeyDown -= OnOwnerKeyDown;
            m_subscribedOwner.Loaded -= OnOwnerLoaded;
            m_subscribedOwner.Closed -= OnOwnerClosed;
            m_subscribedOwner = null;
        }

        if (Owner is { } owner)
        {
            owner.KeyDown += OnOwnerKeyDown;
            owner.Loaded += OnOwnerLoaded;
            owner.Closed += OnOwnerClosed;
            m_subscribedOwner = owner;
        }
    }

    private static void OnButtonTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((ContentWindowDialogControl)d).UpdateCommandArea();
    }

    private static void OnButtonAlignmentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((ContentWindowDialogControl)d).UpdateCommandArea();
    }

    private static void OnDefaultButtonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((ContentWindowDialogControl)d).UpdateCommandArea();
    }

    private static void TryExecuteCommand(ICommand? command, object? parameter)
    {
        if (command != null && command.CanExecute(parameter))
        {
            command.Execute(parameter);
        }
    }
}
