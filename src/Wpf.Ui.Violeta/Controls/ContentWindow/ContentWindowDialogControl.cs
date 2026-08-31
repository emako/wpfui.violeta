using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Wpf.Ui.Violeta.Resources.Localization;

namespace Wpf.Ui.Violeta.Controls;

[TemplatePart(Name = nameof(PrimaryButton), Type = typeof(Button))]
[TemplatePart(Name = nameof(SecondaryButton), Type = typeof(Button))]
public class ContentWindowDialogControl : ContentWindowControl
{
    static ContentWindowDialogControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ContentWindowDialogControl), new FrameworkPropertyMetadata(typeof(ContentWindowDialogControl)));
    }

    public static readonly DependencyProperty PrimaryButtonTextProperty = DependencyProperty.Register(
        nameof(PrimaryButtonText), typeof(string), typeof(ContentWindowDialogControl),
        new PropertyMetadata(SH.ButtonOK));

    public string? PrimaryButtonText
    {
        get => (string?)GetValue(PrimaryButtonTextProperty);
        set => SetValue(PrimaryButtonTextProperty, value);
    }

    public static readonly DependencyProperty SecondaryButtonTextProperty = DependencyProperty.Register(
        nameof(SecondaryButtonText), typeof(string), typeof(ContentWindowDialogControl),
        new PropertyMetadata(SH.ButtonCancel));

    public string? SecondaryButtonText
    {
        get => (string?)GetValue(SecondaryButtonTextProperty);
        set => SetValue(SecondaryButtonTextProperty, value);
    }

    public static readonly DependencyProperty PrimaryButtonCommandProperty = DependencyProperty.Register(
        nameof(PrimaryButtonCommand), typeof(ICommand), typeof(ContentWindowDialogControl), new PropertyMetadata(null));

    public ICommand? PrimaryButtonCommand
    {
        get => (ICommand?)GetValue(PrimaryButtonCommandProperty);
        set => SetValue(PrimaryButtonCommandProperty, value);
    }

    public static readonly DependencyProperty SecondaryButtonCommandProperty = DependencyProperty.Register(
        nameof(SecondaryButtonCommand), typeof(ICommand), typeof(ContentWindowDialogControl), new PropertyMetadata(null));

    public ICommand? SecondaryButtonCommand
    {
        get => (ICommand?)GetValue(SecondaryButtonCommandProperty);
        set => SetValue(SecondaryButtonCommandProperty, value);
    }

    public static readonly DependencyProperty ShowPrimaryButtonProperty = DependencyProperty.Register(
        nameof(ShowPrimaryButton), typeof(bool), typeof(ContentWindowDialogControl), new PropertyMetadata(true, OnButtonVisibilityChanged));

    public bool ShowPrimaryButton
    {
        get => (bool)GetValue(ShowPrimaryButtonProperty);
        set => SetValue(ShowPrimaryButtonProperty, value);
    }

    public static readonly DependencyProperty ShowSecondaryButtonProperty = DependencyProperty.Register(
        nameof(ShowSecondaryButton), typeof(bool), typeof(ContentWindowDialogControl), new PropertyMetadata(true, OnButtonVisibilityChanged));

    public bool ShowSecondaryButton
    {
        get => (bool)GetValue(ShowSecondaryButtonProperty);
        set => SetValue(ShowSecondaryButtonProperty, value);
    }

    public event Wpf.Ui.Controls.TypedEventHandler<ContentWindowDialogControl, ContentDialogButtonClickEventArgs>? PrimaryButtonClick;

    public event Wpf.Ui.Controls.TypedEventHandler<ContentWindowDialogControl, ContentDialogButtonClickEventArgs>? SecondaryButtonClick;

    private Button PrimaryButton { get; set; } = null!;

    private Button SecondaryButton { get; set; } = null!;

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

        PrimaryButton = (Button)GetTemplateChild(nameof(PrimaryButton));
        SecondaryButton = (Button)GetTemplateChild(nameof(SecondaryButton));

        if (PrimaryButton != null)
        {
            PrimaryButton.Click += OnButtonClick;
        }

        if (SecondaryButton != null)
        {
            SecondaryButton.Click += OnButtonClick;
        }

        UpdateButtonsVisibility();

        if (Owner is { } owner)
        {
            owner.CanKeyDownResult = true;
        }
    }

    private void OnButtonClick(object? sender, RoutedEventArgs e)
    {
        if (sender == PrimaryButton)
        {
            HandleButtonClick(PrimaryButtonClick, PrimaryButtonCommand, ContentWindowResult.OK);
        }
        else if (sender == SecondaryButton)
        {
            HandleButtonClick(SecondaryButtonClick, SecondaryButtonCommand, ContentWindowResult.Cancel);
        }
    }

    private void HandleButtonClick(
        Wpf.Ui.Controls.TypedEventHandler<ContentWindowDialogControl, ContentDialogButtonClickEventArgs>? handler,
        ICommand? command,
        ContentWindowResult result)
    {
        if (handler != null)
        {
            var args = new ContentDialogButtonClickEventArgs();

            var deferral = new ContentDialogButtonClickDeferral(() =>
            {
                if (!args.Cancel)
                {
                    TryExecuteCommand(command);
                    Owner?.OnResultCommandExecuted(result);
                }
            });

            args.SetDeferral(deferral);
            args.IncrementDeferralCount();
            handler(this, args);
            args.DecrementDeferralCount();
        }
        else
        {
            TryExecuteCommand(command);
            Owner?.OnResultCommandExecuted(result);
        }
    }

    private void UpdateButtonsVisibility()
    {
        if (PrimaryButton != null)
        {
            PrimaryButton.Visibility = ShowPrimaryButton ? Visibility.Visible : Visibility.Collapsed;
        }

        if (SecondaryButton != null)
        {
            SecondaryButton.Visibility = ShowSecondaryButton ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private static void OnButtonVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((ContentWindowDialogControl)d).UpdateButtonsVisibility();
    }

    private static void TryExecuteCommand(ICommand? command)
    {
        if (command != null && command.CanExecute(null))
        {
            command.Execute(null);
        }
    }
}
