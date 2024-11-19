using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using Wpf.Ui.Violeta.Resources.Localization;
using Wpf.Ui.Violeta.Win32;

namespace Wpf.Ui.Violeta.Controls;

public static class PendingBox
{
    public static IPendingHandler Show(string? message = null, string? title = null, bool isShowCancel = false, bool closeOnCanceled = true)
    {
        return Show(null, message, title, isShowCancel, closeOnCanceled);
    }

    public static IPendingHandler Show(Window? owner, string? message, string? title = null, bool isShowCancel = false, bool closeOnCanceled = true)
    {
        return ShowCore(owner, message, title, isShowCancel, closeOnCanceled);
    }

    [SuppressMessage("Performance", "CA1859:Use concrete types when possible for improved performance")]
    [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression")]
    private static IPendingHandler ShowCore(Window? owner, string? message, string? title, bool isShowCancel, bool closeOnCanceled = true)
    {
        owner
            ??= Application.Current.Windows.OfType<Window>().FirstOrDefault(window => window.IsActive)
            ?? Application.Current.MainWindow;

        Dispatcher dispatcher = owner?.Dispatcher ?? Application.Current.Dispatcher;

        return dispatcher.Invoke(() =>
        {
            PendingBoxDialog dialog = new()
            {
                Owner = owner,
                Title = title ?? string.Empty,
                Message = message ?? (SH.Loading + "..."),
                IsShowCancel = isShowCancel,
                WindowStartupLocation = owner is null ? WindowStartupLocation.CenterScreen : WindowStartupLocation.CenterOwner
            };
            PendingHandler pending = new(dialog)
            {
                CloseOnCanceled = closeOnCanceled,
            };

            dialog.Owner = owner;
            dialog.Closing += (_, e) =>
            {
                if (!e.Cancel)
                {
                    _ = User32.SetForegroundWindow(new WindowInteropHelper(owner).Handle);
                }
            };
            dialog.Closed += (s, e) =>
            {
                pending.RaiseClosedEvent(s, e);
                _ = User32.EnableWindow(new WindowInteropHelper(owner).Handle, true);
                _ = User32.SetForegroundWindow(new WindowInteropHelper(owner).Handle);
            };
            dialog.Canceled += (s, e) =>
            {
                pending.RaiseCanceledEvent(s, e);
                _ = User32.EnableWindow(new WindowInteropHelper(owner).Handle, true);
            };

            _ = User32.EnableWindow(new WindowInteropHelper(owner).Handle, false);
            dialog.Show();
            return pending;
        });
    }
}
