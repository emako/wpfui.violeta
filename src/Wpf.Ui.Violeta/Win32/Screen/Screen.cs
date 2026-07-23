using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using Microsoft.Win32;

namespace Wpf.Ui.Violeta.Win32;

/// <summary>
/// Represents a display device or multiple display devices on a single system.
/// A WPF-friendly reimplementation of <c>System.Windows.Forms.Screen</c> that does not
/// require Windows Forms.
/// </summary>
/// <remarks>
/// Coordinate values use Win32 virtual-desktop device pixels (same numeric semantics as
/// WinForms <c>Screen</c>), exposed through WPF <see cref="Point"/> and <see cref="Rect"/>.
/// </remarks>
public sealed class Screen
{
    private static readonly object SyncLock = new();
    private static Screen[]? s_screens;
    private static int s_desktopChangedCount = -1;
    private static bool s_displaySettingsHooked;
    private static bool s_userPreferenceHooked;

    private readonly nint _hmonitor;
    private readonly Rect _bounds;
    private readonly bool _primary;
    private readonly string _deviceName;
    private readonly int _bitDepth;

    private Rect _workingArea = Rect.Empty;
    private int _currentDesktopChangedCount = -1;

    private Screen(nint monitor)
        : this(monitor, IntPtr.Zero)
    {
    }

    private Screen(nint monitor, nint hdc)
    {
        nint screenDc = hdc;

        if (!MultiMonitorSupport || monitor == ScreenNativeMethods.PrimaryMonitor)
        {
            _bounds = GetVirtualScreenBounds();
            _primary = true;
            _deviceName = "DISPLAY";
        }
        else
        {
            ScreenNativeMethods.MONITORINFOEX info = CreateMonitorInfoEx();
            if (!ScreenNativeMethods.GetMonitorInfo(monitor, ref info))
            {
                _bounds = GetVirtualScreenBounds();
                _primary = true;
                _deviceName = "DISPLAY";
            }
            else
            {
                _bounds = ToRect(info.rcMonitor);
                _primary = (info.dwFlags & ScreenNativeMethods.MONITORINFOF_PRIMARY) != 0;
                _deviceName = string.IsNullOrEmpty(info.szDevice) ? "DISPLAY" : info.szDevice;

                if (hdc == IntPtr.Zero)
                {
                    // WinForms passes the monitor device name as the CreateDC driver name.
                    screenDc = ScreenNativeMethods.CreateDC(_deviceName, null, null, IntPtr.Zero);
                }
            }
        }

        _hmonitor = monitor;

        if (screenDc != IntPtr.Zero)
        {
            _bitDepth = ScreenNativeMethods.GetDeviceCaps(screenDc, ScreenNativeMethods.BITSPIXEL);
            _bitDepth *= ScreenNativeMethods.GetDeviceCaps(screenDc, ScreenNativeMethods.PLANES);

            if (hdc != screenDc)
            {
                _ = ScreenNativeMethods.DeleteDC(screenDc);
            }
        }
        else
        {
            // Fallback when CreateDC fails: query the desktop DC.
            nint desktopDc = User32.GetDC(IntPtr.Zero);
            if (desktopDc != IntPtr.Zero)
            {
                _bitDepth = ScreenNativeMethods.GetDeviceCaps(desktopDc, ScreenNativeMethods.BITSPIXEL);
                _bitDepth *= ScreenNativeMethods.GetDeviceCaps(desktopDc, ScreenNativeMethods.PLANES);
                _ = User32.ReleaseDC(IntPtr.Zero, desktopDc);
            }
        }
    }

    /// <summary>
    /// Gets an array of all of the displays on the system.
    /// </summary>
    public static Screen[] AllScreens
    {
        get
        {
            Screen[] screens = EnsureScreens();
            Screen[] copy = new Screen[screens.Length];
            Array.Copy(screens, copy, screens.Length);
            return copy;
        }
    }

    /// <summary>
    /// Gets the bits-per-pixel value for this display.
    /// </summary>
    public int BitsPerPixel => _bitDepth;

    /// <summary>
    /// Gets the bounds of the display in virtual-desktop device pixels.
    /// </summary>
    public Rect Bounds => _bounds;

    /// <summary>
    /// Gets the device name associated with a display.
    /// </summary>
    public string DeviceName => _deviceName;

    /// <summary>
    /// Gets a value indicating whether a particular display is the primary device.
    /// </summary>
    public bool Primary => _primary;

    /// <summary>
    /// Gets the primary display, or <see langword="null"/> when multi-monitor support is
    /// available but no primary monitor flag is reported.
    /// </summary>
    public static Screen? PrimaryScreen
    {
        get
        {
            if (MultiMonitorSupport)
            {
                Screen[] screens = EnsureScreens();
                for (int i = 0; i < screens.Length; i++)
                {
                    if (screens[i]._primary)
                    {
                        return screens[i];
                    }
                }

                return null;
            }

            return new Screen(ScreenNativeMethods.PrimaryMonitor);
        }
    }

    /// <summary>
    /// Gets the working area of the screen in virtual-desktop device pixels.
    /// </summary>
    public Rect WorkingArea
    {
        get
        {
            if (_currentDesktopChangedCount != DesktopChangedCount)
            {
                Interlocked.Exchange(ref _currentDesktopChangedCount, DesktopChangedCount);

                if (!MultiMonitorSupport || _hmonitor == ScreenNativeMethods.PrimaryMonitor)
                {
                    _workingArea = GetSystemWorkingArea();
                }
                else
                {
                    ScreenNativeMethods.MONITORINFOEX info = CreateMonitorInfoEx();
                    if (ScreenNativeMethods.GetMonitorInfo(_hmonitor, ref info))
                    {
                        _workingArea = ToRect(info.rcWork);
                    }
                    else
                    {
                        _workingArea = _bounds;
                    }
                }
            }

            return _workingArea;
        }
    }

    private static bool MultiMonitorSupport =>
        ScreenNativeMethods.GetSystemMetrics(ScreenNativeMethods.SM_CMONITORS) != 0;

    private static int DesktopChangedCount
    {
        get
        {
            if (s_desktopChangedCount == -1)
            {
                lock (SyncLock)
                {
                    if (s_desktopChangedCount == -1)
                    {
                        if (!s_userPreferenceHooked)
                        {
                            SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
                            s_userPreferenceHooked = true;
                        }

                        s_desktopChangedCount = 0;
                    }
                }
            }

            return s_desktopChangedCount;
        }
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) =>
        obj is Screen other && _hmonitor == other._hmonitor;

    /// <summary>
    /// Retrieves a <see cref="Screen"/> for the monitor that contains the specified point.
    /// </summary>
    public static Screen FromPoint(Point point)
    {
        if (!MultiMonitorSupport)
        {
            return new Screen(ScreenNativeMethods.PrimaryMonitor);
        }

        POINT nativePoint = new(ToDeviceCoordinate(point.X), ToDeviceCoordinate(point.Y));
        nint monitor = ScreenNativeMethods.MonitorFromPoint(nativePoint, ScreenNativeMethods.MONITOR_DEFAULTTONEAREST);
        return new Screen(monitor);
    }

    /// <summary>
    /// Retrieves a <see cref="Screen"/> for the monitor that contains the largest region of the rectangle.
    /// </summary>
    public static Screen FromRectangle(Rect rect)
    {
        if (!MultiMonitorSupport)
        {
            return new Screen(ScreenNativeMethods.PrimaryMonitor);
        }

        RECT nativeRect = ToNativeRect(rect);
        nint monitor = ScreenNativeMethods.MonitorFromRect(ref nativeRect, ScreenNativeMethods.MONITOR_DEFAULTTONEAREST);
        return new Screen(monitor);
    }

    /// <summary>
    /// Retrieves a <see cref="Screen"/> for the monitor that contains the largest region of the window.
    /// </summary>
    public static Screen FromHandle(nint hwnd)
    {
        if (!MultiMonitorSupport)
        {
            return new Screen(ScreenNativeMethods.PrimaryMonitor);
        }

        nint monitor = ScreenNativeMethods.MonitorFromWindow(hwnd, ScreenNativeMethods.MONITOR_DEFAULTTONEAREST);
        return new Screen(monitor);
    }

    /// <summary>
    /// Retrieves a <see cref="Screen"/> for the monitor that contains the largest region of the window.
    /// </summary>
    /// <remarks>
    /// This is the WPF counterpart of WinForms <c>Screen.FromControl</c>. Accessing the handle may
    /// create the underlying HWND when it does not yet exist.
    /// </remarks>
    public static Screen FromWindow(Window window)
    {
        if (window is null)
        {
            throw new ArgumentNullException(nameof(window));
        }

        nint hwnd = new WindowInteropHelper(window).EnsureHandle();
        return FromHandle(hwnd);
    }

    /// <summary>
    /// Retrieves the working area for the monitor that is closest to the specified point.
    /// </summary>
    public static Rect GetWorkingArea(Point point) => FromPoint(point).WorkingArea;

    /// <summary>
    /// Retrieves the working area for the monitor that contains the largest region of the specified rectangle.
    /// </summary>
    public static Rect GetWorkingArea(Rect rect) => FromRectangle(rect).WorkingArea;

    /// <summary>
    /// Retrieves the working area for the monitor that contains the largest region of the specified window.
    /// </summary>
    public static Rect GetWorkingArea(Window window) => FromWindow(window).WorkingArea;

    /// <summary>
    /// Retrieves the bounds of the monitor that is closest to the specified point.
    /// </summary>
    public static Rect GetBounds(Point point) => FromPoint(point).Bounds;

    /// <summary>
    /// Retrieves the bounds of the monitor that contains the largest region of the specified rectangle.
    /// </summary>
    public static Rect GetBounds(Rect rect) => FromRectangle(rect).Bounds;

    /// <summary>
    /// Retrieves the bounds of the monitor that contains the largest region of the specified window.
    /// </summary>
    public static Rect GetBounds(Window window) => FromWindow(window).Bounds;

    /// <inheritdoc />
    public override int GetHashCode() => unchecked((int)_hmonitor);

    /// <inheritdoc />
    public override string ToString() =>
        $"{GetType().Name}[Bounds={FormatRect(_bounds)} WorkingArea={FormatRect(WorkingArea)} Primary={_primary} DeviceName={_deviceName}]";

    private static Screen[] EnsureScreens()
    {
        Screen[]? screens = Volatile.Read(ref s_screens);
        if (screens is not null)
        {
            return screens;
        }

        lock (SyncLock)
        {
            screens = s_screens;
            if (screens is not null)
            {
                return screens;
            }

            screens = EnumerateScreens();
            Volatile.Write(ref s_screens, screens);

            if (!s_displaySettingsHooked)
            {
                SystemEvents.DisplaySettingsChanging += OnDisplaySettingsChanging;
                s_displaySettingsHooked = true;
            }

            return screens;
        }
    }

    private static Screen[] EnumerateScreens()
    {
        if (!MultiMonitorSupport)
        {
            return [new Screen(ScreenNativeMethods.PrimaryMonitor)];
        }

        List<Screen> screens = [];
        ScreenNativeMethods.MonitorEnumProc callback = (nint hMonitor, nint hdcMonitor, ref RECT _, nint _) =>
        {
            screens.Add(new Screen(hMonitor, hdcMonitor));
            return true;
        };

        if (!ScreenNativeMethods.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, callback, IntPtr.Zero) || screens.Count == 0)
        {
            return [new Screen(ScreenNativeMethods.PrimaryMonitor)];
        }

        return [.. screens];
    }

    private static void OnDisplaySettingsChanging(object? sender, EventArgs e)
    {
        lock (SyncLock)
        {
            if (s_displaySettingsHooked)
            {
                SystemEvents.DisplaySettingsChanging -= OnDisplaySettingsChanging;
                s_displaySettingsHooked = false;
            }

            Volatile.Write(ref s_screens, null);
        }
    }

    private static void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
    {
        if (e.Category == UserPreferenceCategory.Desktop)
        {
            Interlocked.Increment(ref s_desktopChangedCount);
        }
    }

    private static ScreenNativeMethods.MONITORINFOEX CreateMonitorInfoEx() =>
        new()
        {
            cbSize = MarshalSizeOfMonitorInfoEx(),
            szDevice = string.Empty,
        };

    private static int MarshalSizeOfMonitorInfoEx() =>
        System.Runtime.InteropServices.Marshal.SizeOf<ScreenNativeMethods.MONITORINFOEX>();

    private static Rect GetVirtualScreenBounds()
    {
        if (MultiMonitorSupport)
        {
            int x = ScreenNativeMethods.GetSystemMetrics(ScreenNativeMethods.SM_XVIRTUALSCREEN);
            int y = ScreenNativeMethods.GetSystemMetrics(ScreenNativeMethods.SM_YVIRTUALSCREEN);
            int width = ScreenNativeMethods.GetSystemMetrics(ScreenNativeMethods.SM_CXVIRTUALSCREEN);
            int height = ScreenNativeMethods.GetSystemMetrics(ScreenNativeMethods.SM_CYVIRTUALSCREEN);
            return new Rect(x, y, width, height);
        }

        int cx = ScreenNativeMethods.GetSystemMetrics(ScreenNativeMethods.SM_CXSCREEN);
        int cy = ScreenNativeMethods.GetSystemMetrics(ScreenNativeMethods.SM_CYSCREEN);
        return new Rect(0, 0, cx, cy);
    }

    private static Rect GetSystemWorkingArea()
    {
        RECT workArea = default;
        if (ScreenNativeMethods.SystemParametersInfo(ScreenNativeMethods.SPI_GETWORKAREA, 0, ref workArea, 0))
        {
            return ToRect(workArea);
        }

        return GetVirtualScreenBounds();
    }

    private static Rect ToRect(RECT rect) =>
        new(rect.Left, rect.Top, Math.Max(0, rect.Width), Math.Max(0, rect.Height));

    private static RECT ToNativeRect(Rect rect)
    {
        int left = ToDeviceCoordinate(rect.X);
        int top = ToDeviceCoordinate(rect.Y);
        int right = ToDeviceCoordinate(rect.X + rect.Width);
        int bottom = ToDeviceCoordinate(rect.Y + rect.Height);
        return new RECT(left, top, right, bottom);
    }

    private static int ToDeviceCoordinate(double value) =>
        (int)Math.Round(value, MidpointRounding.AwayFromZero);

    private static string FormatRect(Rect rect) =>
        $"{{X={rect.X},Y={rect.Y},Width={rect.Width},Height={rect.Height}}}";
}
