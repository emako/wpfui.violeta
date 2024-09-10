using System;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Wpf.Ui.Controls;
using Wpf.Ui.Violeta.Resources.Localization;
using Wpf.Ui.Violeta.Win32;

namespace Wpf.Ui.Violeta.Controls;

public partial class ExceptionWindow : FluentWindow
{
    public Window? owner = null;

    public new Window? Owner
    {
        get => owner;
        set
        {
            owner = value;

            if (owner != null)
            {
                try
                {
                    // Try get title but be care the owner had been disposed.
                    AppName ??= owner.Dispatcher.Invoke(() => owner.Title);
                }
                catch
                {
                    ///
                }

                try
                {
                    // Try get assembly version but be care the owner class had been unmounted.
                    AppVersion ??= "v" + owner.GetType().Assembly.GetName().Version?.ToString();
                }
                catch
                {
                    ///
                }
            }
        }
    }

    public Exception? ExceptionObject { get; set; }
    public string? ExceptionType => ExceptionObject?.GetType().ToString();

    public string? AppName
    {
        get => (string)GetValue(AppNameProperty);
        set => SetValue(AppNameProperty, value);
    }

    public static readonly DependencyProperty AppNameProperty =
        DependencyProperty.Register(nameof(AppName), typeof(string), typeof(ExceptionWindow), new PropertyMetadata(null));

    public string? AppVersion
    {
        get => (string)GetValue(AppVersionProperty);
        set => SetValue(AppVersionProperty, value);
    }

    public static readonly DependencyProperty AppVersionProperty =
        DependencyProperty.Register(nameof(AppVersion), typeof(string), typeof(ExceptionWindow), new PropertyMetadata(null));

    public string ErrorTime { get; } = DateTime.Now.ToString();

    public string OSVersion { get; } = "Windows " + NTdll.GetOSVersion().ToString();

    public ExceptionWindow(Exception e, string? appName = null, string? appVersion = null)
    {
        ExceptionObject = e ?? new Exception("<NULL>");
        AppName = appName;
        AppVersion = appVersion;

        DataContext = this;
        InitializeComponent();

        Title = SH.ExceptionWindowTitle;
        Hint1TextBlock.Text = SH.ExceptionWindowHint1;
        Hint2TextBlock.Text = SH.ExceptionWindowHint2;
        CopyTextBlock.Text = SH.ButtonCopy;
        IgnoreTextBlock.Text = SH.ButtonIgnore;
        ExitTextBlock.Text = SH.ButtonExit;

        CopyButton.Click += (_, _) => CopyInfo();
        TryIgnoreButton.Click += (_, _) => IgnoreAndTry();
        ExitButton.Click += (_, _) => ExitApp();

        KeyDown += (_, e) =>
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.C)
            {
                CopyInfo();
            }
        };
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        if (WindowBackdrop.IsSupported(WindowBackdropType.Mica))
        {
            Background = new SolidColorBrush(Colors.Transparent);
            WindowBackdrop.ApplyBackdrop(this, WindowBackdropType.Mica);
        }
    }

    public string GetInfo()
    {
        StringBuilder sb = new();

        sb.AppendLine($"{AppName} {AppVersion}")
          .AppendLine($"{ErrorTime} {OSVersion}")
          .AppendLine("```")
          .AppendLine(ExceptionObject?.ToString())
          .AppendLine("```");

        return sb.ToString();
    }

    public void CopyInfo()
    {
        try
        {
            Clipboard.SetText(GetInfo());
        }
        catch
        {
            ///
        }
    }

    public void IgnoreAndTry()
    {
        Close();
    }

    public void ExitApp()
    {
        IgnoreAndTry();
        Environment.Exit('e' + 'x' + 'c' + 'e' + 'p' + 't' + 'i' + 'o' + 'n');
    }
}
