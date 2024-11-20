using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Wpf.Ui.Controls;
using Wpf.Ui.Violeta.Win32;
using Button = System.Windows.Controls.Button;
using TextBlock = System.Windows.Controls.TextBlock;

namespace Wpf.Ui.Violeta.Controls;

[TemplatePart(Name = "PART_Title", Type = typeof(TextBlock))]
[TemplatePart(Name = "PART_Loading", Type = typeof(Loading))]
[TemplatePart(Name = "PART_Message", Type = typeof(TextBlock))]
[TemplatePart(Name = "PART_CancelButton", Type = typeof(Button))]
public partial class PendingBoxDialog : Window
{
    public event EventHandler? Canceled;

    public bool IsClosedByHandler { get; internal set; } = false;

    protected TextBlock TitleTextBlock { get; set; } = null!;
    protected Button CancelButton { get; set; } = null!;

    public bool IsShowCancel
    {
        get => (bool)GetValue(IsShowCancelProperty);
        set => SetValue(IsShowCancelProperty, value);
    }

    public static readonly DependencyProperty IsShowCancelProperty =
        DependencyProperty.Register(nameof(IsShowCancel), typeof(bool), typeof(PendingBoxDialog), new PropertyMetadata());

    protected static void OnIsShowCancelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PendingBoxDialog self)
        {
            if (self.CancelButton != null)
            {
                self.CancelButton.Visibility = self.IsShowCancel ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }

    public string? Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public static readonly DependencyProperty MessageProperty =
        DependencyProperty.Register(nameof(Message), typeof(string), typeof(PendingBoxDialog), new PropertyMetadata(string.Empty));

    public double LoadingSize
    {
        get => (double)GetValue(LoadingSizeProperty);
        set => SetValue(LoadingSizeProperty, value);
    }

    public static readonly DependencyProperty LoadingSizeProperty =
        DependencyProperty.Register(nameof(LoadingSize), typeof(double), typeof(PendingBoxDialog), new PropertyMetadata(38d));

    public PendingBoxDialog()
    {
        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        MinHeight = 200d;
        MinWidth = 450d;
        MaxHeight = 200d;
        MaxWidth = 450d;

        MouseLeftButtonDown += OnMouseLeftButtonDown;
        Closing += OnClosing;
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        if (WindowBackdrop.IsSupported(WindowBackdropType.Mica))
        {
            Background = new SolidColorBrush(Colors.Transparent);
            WindowBackdrop.ApplyBackdrop(this, WindowBackdropType.Mica);
        }

        // Hide the all window control buttons
        {
            nint hwnd = new WindowInteropHelper(this).Handle;
            int currentStyle = User32.GetWindowLong(hwnd, User32.GWL_STYLE);
            _ = User32.SetWindowLong(hwnd, User32.GWL_STYLE, currentStyle & ~User32.WS_SYSMENU);
        }
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        TitleTextBlock = (TextBlock)GetTemplateChild("PART_Title");

        if (string.IsNullOrWhiteSpace(Title))
        {
            if (TitleTextBlock != null)
            {
                TitleTextBlock.Visibility = Visibility.Collapsed;
            }
        }

        if (CancelButton != null)
        {
            CancelButton.Click -= OnCancelButtonClick;
        }

        CancelButton = (Button)GetTemplateChild("PART_CancelButton");

        if (CancelButton != null)
        {
            CancelButton.Click += OnCancelButtonClick;
            CancelButton.IsCancel = true;
            CancelButton.Content = GetString(User32.DialogBoxCommand.IDCANCEL);
            CancelButton.Visibility = IsShowCancel ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    [SuppressMessage("Performance", "SYSLIB1045:Convert to 'GeneratedRegexAttribute'.")]
    [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression")]
    private static string GetString(User32.DialogBoxCommand wBtn)
    {
        nint strPtr = User32.MB_GetString((uint)wBtn);
        StringBuilder sb = new(Marshal.PtrToStringAuto(strPtr));
        return Regex.Replace(sb.Replace("&", string.Empty).ToString(), @"\([^)]*\)", string.Empty);
    }

    private void OnCancelButtonClick(object? sender, RoutedEventArgs e)
    {
        Canceled?.Invoke(this, e);

        if (CancelButton != null)
        {
            CancelButton.IsEnabled = false;
        }

        // Don't call Close() here, let the handler do it.
    }

    private void OnMouseLeftButtonDown(object? sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    private void OnClosing(object? sender, CancelEventArgs e)
    {
        // Don't close the dialog if it's not closed by the handler.
        if (!IsClosedByHandler)
        {
            e.Cancel = true;
        }
    }

    public new void Show()
    {
        if (Owner != null)
        {
            // Inherit the topmost state from the owner window
            Topmost = Owner.Topmost;
        }

        base.Show();
    }

    [Obsolete("Use Show instead")]
    public new bool? ShowDialog()
    {
        if (Owner != null)
        {
            // Inherit the topmost state from the owner window
            Topmost = Owner.Topmost;
        }

        base.ShowDialog();
        return DialogResult;
    }
}
