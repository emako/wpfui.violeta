using System;

namespace Wpf.Ui.Violeta.Win32;

public partial class TrayIconHost
{
    private const nint TwinkTimerId = 1;

    private bool _isTwink;
    private TimeSpan _twinkInterval = TimeSpan.FromMilliseconds(500);
    private bool _twinkTimerRunning;
    private bool _twinkShowingIcon = true;

    /// <summary>
    /// Gets or sets whether the tray icon twinkles by alternating between the icon and an empty icon.
    /// </summary>
    public bool IsTwink
    {
        get => _isTwink;
        set
        {
            if (_isTwink == value)
                return;

            _isTwink = value;

            if (value)
                StartTwink();
            else
                StopTwink(restoreIcon: true);
        }
    }

    /// <summary>
    /// Gets or sets the interval between twinkle toggles. Default is 500 milliseconds.
    /// </summary>
    public TimeSpan TwinkInterval
    {
        get => _twinkInterval;
        set
        {
            if (value <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(value), value, "TwinkInterval must be greater than zero.");

            if (_twinkInterval == value)
                return;

            _twinkInterval = value;

            if (_twinkTimerRunning)
                RestartTwinkTimer();
        }
    }

    private void StartTwink()
    {
        if (!IsVisible || hWnd == IntPtr.Zero)
            return;

        _twinkShowingIcon = true;
        ApplyTrayIcon(_iconHandle);
        RestartTwinkTimer();
    }

    private void StopTwink(bool restoreIcon)
    {
        if (_twinkTimerRunning && hWnd != IntPtr.Zero)
        {
            _ = User32.KillTimer(hWnd, TwinkTimerId);
            _twinkTimerRunning = false;
        }

        _twinkShowingIcon = true;

        if (restoreIcon)
            ApplyTrayIcon(_iconHandle);
    }

    private void RestartTwinkTimer()
    {
        if (hWnd == IntPtr.Zero)
            return;

        if (_twinkTimerRunning)
            _ = User32.KillTimer(hWnd, TwinkTimerId);

        uint intervalMs = (uint)Math.Max(1, (int)_twinkInterval.TotalMilliseconds);
        _ = User32.SetTimer(hWnd, TwinkTimerId, intervalMs, IntPtr.Zero);
        _twinkTimerRunning = true;
    }

    private void OnTwinkTimer()
    {
        if (!_isTwink || !IsVisible)
            return;

        _twinkShowingIcon = !_twinkShowingIcon;
        ApplyTrayIcon(_twinkShowingIcon ? _iconHandle : IntPtr.Zero);
    }

    private void ApplyTrayIcon(nint hIcon)
    {
        notifyIconData.hIcon = hIcon;
        notifyIconData.uFlags |= (int)Shell32.NotifyIconFlags.NIF_ICON;
        _ = Shell32.Shell_NotifyIcon((int)Shell32.NOTIFY_COMMAND.NIM_MODIFY, ref notifyIconData);
    }

    private void SyncTwinkWithVisibility()
    {
        if (!_isTwink)
            return;

        if (IsVisible)
            StartTwink();
        else
            StopTwink(restoreIcon: false);
    }
}
