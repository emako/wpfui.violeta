using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;

namespace Wpf.Ui.Violeta.Win32;

public class TrayIconHost
{
    private readonly nint hWnd = IntPtr.Zero;
    private readonly User32.WndProcDelegate wndProcDelegate = null!;
    private Shell32.NotifyIconData notifyIconData = default;
    private readonly int id = default;
    private static int nextId = 0;

    public string ToolTipText
    {
        get => notifyIconData.szTip;
        set
        {
            notifyIconData.szTip = value;
            notifyIconData.uFlags |= (int)Shell32.NotifyIconFlags.NIF_TIP;
            _ = Shell32.Shell_NotifyIcon((int)Shell32.NOTIFY_COMMAND.NIM_MODIFY, ref notifyIconData);
        }
    }

    public nint Icon
    {
        get => notifyIconData.hIcon;
        set
        {
            if (notifyIconData.hIcon != IntPtr.Zero)
                _ = User32.DestroyIcon(notifyIconData.hIcon);
            notifyIconData.hIcon = User32.CopyIcon(value);
            notifyIconData.uFlags |= (int)Shell32.NotifyIconFlags.NIF_ICON;
            _ = Shell32.Shell_NotifyIcon((int)Shell32.NOTIFY_COMMAND.NIM_MODIFY, ref notifyIconData);
        }
    }

    public bool IsVisible
    {
        get => notifyIconData.dwState != (uint)Shell32.NotifyIconState.NIS_HIDDEN;
        set
        {
            notifyIconData.dwState = value ? 0 : (uint)Shell32.NotifyIconState.NIS_HIDDEN;
            notifyIconData.dwStateMask = (uint)(Shell32.NotifyIconState.NIS_HIDDEN | Shell32.NotifyIconState.NIS_SHAREDICON);
            notifyIconData.uFlags |= (int)Shell32.NotifyIconFlags.NIF_STATE;
            _ = Shell32.Shell_NotifyIcon((int)Shell32.NOTIFY_COMMAND.NIM_MODIFY, ref notifyIconData);
        }
    }

    public string BalloonTipText
    {
        get => field;
        set
        {
            if (value != field)
            {
                field = value;
            }
        }
    } = string.Empty;

    public ToolTipIcon BalloonTipIcon
    {
        get => field;
        set
        {
            if ((int)value < 0 || (int)value > 3)
            {
                throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(ToolTipIcon));
            }

            if (value != field)
            {
                field = value;
            }
        }
    }

    public string BalloonTipTitle
    {
        get => field;
        set
        {
            if (value != field)
            {
                field = value;
            }
        }
    } = string.Empty;

    public object? Tag { get; set; } = null;

    public TrayMenu Menu { get; set; } = null!;

    public event EventHandler<EventArgs>? BalloonTipClicked = null;

    public event EventHandler<EventArgs>? BalloonTipClosed = null;

    public event EventHandler<EventArgs>? BalloonTipShown = null;

    public event EventHandler<EventArgs>? Click = null;

    public event EventHandler<EventArgs>? RightDown = null;

    public event EventHandler<EventArgs>? RightClick = null;

    public event EventHandler<EventArgs>? RightDoubleClick = null;

    public event EventHandler<EventArgs>? LeftDown = null;

    public event EventHandler<EventArgs>? LeftClick = null;

    public event EventHandler<EventArgs>? LeftDoubleClick = null;

    public event EventHandler<EventArgs>? MiddleDown = null;

    public event EventHandler<EventArgs>? MiddleClick = null;

    public event EventHandler<EventArgs>? MiddleDoubleClick = null;

    public TrayIconHost()
    {
        id = ++nextId;

        wndProcDelegate = new User32.WndProcDelegate(WndProc);

        User32.WNDCLASS wc = new()
        {
            lpszClassName = "TrayIconHostWindowClass",
            lpfnWndProc = Marshal.GetFunctionPointerForDelegate(wndProcDelegate)
        };
        User32.RegisterClass(ref wc);

        hWnd = User32.CreateWindowEx(0, "TrayIconHostWindowClass", "TrayIconHostWindow", 0, 0, 0, 0, 0,
            IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

        notifyIconData = new Shell32.NotifyIconData()
        {
            cbSize = Marshal.SizeOf<Shell32.NotifyIconData>(),
            hWnd = hWnd,
            uID = id,
            uFlags = (int)(Shell32.NotifyIconFlags.NIF_ICON | Shell32.NotifyIconFlags.NIF_MESSAGE | Shell32.NotifyIconFlags.NIF_TIP),
            uCallbackMessage = (int)User32.WindowMessage.WM_TRAYICON,
            hIcon = IntPtr.Zero,
            szTip = null!,
        };

        _ = Shell32.Shell_NotifyIcon((int)Shell32.NOTIFY_COMMAND.NIM_ADD, ref notifyIconData);
    }

    protected virtual nint WndProc(nint hWnd, uint msg, nint wParam, nint lParam)
    {
        if (msg == (uint)User32.WindowMessage.WM_TRAYICON)
        {
            if ((int)wParam == id)
            {
                User32.WindowMessage mouseMsg = (User32.WindowMessage)lParam;

                switch (mouseMsg)
                {
                    case User32.WindowMessage.WM_QUERYENDSESSION:
                    case User32.WindowMessage.WM_ENDSESSION:
                        _ = Shell32.Shell_NotifyIcon((int)Shell32.NOTIFY_COMMAND.NIM_DELETE, ref notifyIconData);
                        break;

                    case User32.WindowMessage.WM_LBUTTONDOWN:
                        LeftDown?.Invoke(this, EventArgs.Empty);
                        break;

                    case User32.WindowMessage.WM_LBUTTONUP:
                        LeftClick?.Invoke(this, EventArgs.Empty);
                        Click?.Invoke(this, EventArgs.Empty);
                        break;

                    case User32.WindowMessage.WM_LBUTTONDBLCLK:
                        LeftDoubleClick?.Invoke(this, EventArgs.Empty);
                        break;

                    case User32.WindowMessage.WM_RBUTTONDOWN:
                        RightDown?.Invoke(this, EventArgs.Empty);
                        break;

                    case User32.WindowMessage.WM_RBUTTONUP:
                        RightClick?.Invoke(this, EventArgs.Empty);
                        ShowContextMenu();
                        break;

                    case User32.WindowMessage.WM_RBUTTONDBLCLK:
                        RightDoubleClick?.Invoke(this, EventArgs.Empty);
                        break;

                    case User32.WindowMessage.WM_MBUTTONDOWN:
                        MiddleDown?.Invoke(this, EventArgs.Empty);
                        break;

                    case User32.WindowMessage.WM_MBUTTONUP:
                        MiddleClick?.Invoke(this, EventArgs.Empty);
                        Click?.Invoke(this, EventArgs.Empty);
                        break;

                    case User32.WindowMessage.WM_MBUTTONDBLCLK:
                        MiddleDoubleClick?.Invoke(this, EventArgs.Empty);
                        break;

                    case User32.WindowMessage.WM_NOTIFYICON_BALLOONSHOW:
                        BalloonTipShown?.Invoke(this, EventArgs.Empty);
                        break;

                    case User32.WindowMessage.WM_NOTIFYICON_BALLOONHIDE:
                        BalloonTipClosed?.Invoke(this, EventArgs.Empty);
                        break;

                    case User32.WindowMessage.WM_NOTIFYICON_BALLOONTIMEOUT:
                        BalloonTipClosed?.Invoke(this, EventArgs.Empty);
                        break;

                    case User32.WindowMessage.WM_NOTIFYICON_BALLOONUSERCLICK:
                        BalloonTipClicked?.Invoke(this, EventArgs.Empty);
                        break;
                }
            }
        }
        return User32.DefWindowProc(hWnd, msg, wParam, lParam);
    }

    public virtual void ShowContextMenu()
    {
        Menu?.Open(hWnd);
    }

    public virtual void ShowBalloonTip(int timeout)
    {
        ShowBalloonTip(timeout, BalloonTipTitle, BalloonTipText, BalloonTipIcon);
    }

    public virtual void ShowBalloonTip(int timeout, string tipTitle, string tipText, ToolTipIcon tipIcon)
    {
        if (timeout < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(timeout));
        }

        if (string.IsNullOrEmpty(tipText))
        {
            throw new ArgumentException("NotifyIconEmptyOrNullTipText");
        }

        if ((int)tipIcon < 0 || (int)tipIcon > 3)
        {
            throw new InvalidEnumArgumentException(nameof(tipIcon), (int)tipIcon, typeof(ToolTipIcon));
        }

        var notifyIconData = new Shell32.NotifyIconData()
        {
            cbSize = Marshal.SizeOf<Shell32.NotifyIconData>(),
            hWnd = hWnd,
            uID = id,
            uFlags = (int)Shell32.NotifyIconFlags.NIF_INFO,
            uTimeoutOrVersion = (uint)timeout,
            szInfoTitle = tipTitle,
            szInfo = tipText,
            dwInfoFlags = tipIcon switch
            {
                ToolTipIcon.Info => 1,
                ToolTipIcon.Warning => 2,
                ToolTipIcon.Error => 3,
                ToolTipIcon.None or _ => 0,
            },
        };

        _ = Shell32.Shell_NotifyIcon((int)Shell32.NOTIFY_COMMAND.NIM_MODIFY, ref notifyIconData);
    }
}

/// <summary>
/// Defines a set of standardized icons that can be associated with a ToolTip.
/// </summary>
public enum ToolTipIcon
{
    /// <summary>
    /// Not a standard icon.
    /// </summary>
    None = 0x00,

    /// <summary>
    /// An information icon.
    /// </summary>
    Info = 0x01,

    /// <summary>
    /// A warning icon.
    /// </summary>
    Warning = 0x02,

    /// <summary>
    /// An error icon
    /// </summary>
    Error = 0x03,
}

public class TrayMenu : DependencyObject, IEnumerable<ITrayMenuItemBase>, IList<ITrayMenuItemBase>
{
    public static readonly DependencyProperty ParentProperty =
        DependencyProperty.Register(nameof(Parent), typeof(TrayMenuItem), typeof(TrayMenu), new(null));

    public TrayMenuItem? Parent
    {
        get => (TrayMenuItem?)GetValue(ParentProperty);
        internal set => SetValue(ParentProperty, value);
    }

    private readonly ObservableCollection<ITrayMenuItemBase> _items = [];

    public IList<ITrayMenuItemBase> Items => _items;

    public int Count => _items.Count;

    public bool IsReadOnly => false;

    public object? Tag { get; set; } = null;

    public ITrayMenuItemBase this[int index]
    {
        get => _items[index];
        set => _items[index] = value;
    }

    public event EventHandler<EventArgs>? Opening;

    public event EventHandler<EventArgs>? Closed;

    public IEnumerator<ITrayMenuItemBase> GetEnumerator() => _items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int IndexOf(ITrayMenuItemBase item) => _items.IndexOf(item);

    public void Insert(int index, ITrayMenuItemBase item) => _items.Insert(index, item);

    public void RemoveAt(int index) => _items.RemoveAt(index);

    public void Add(ITrayMenuItemBase item) => _items.Add(item);

    public void Clear() => _items.Clear();

    public bool Contains(ITrayMenuItemBase item) => _items.Contains(item);

    public void CopyTo(ITrayMenuItemBase[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);

    public bool Remove(ITrayMenuItemBase item) => _items.Contains(item);

    public void Open(nint hWnd)
    {
        if (_items.Count == 0) return;

        nint hMenu = User32.CreatePopupMenu();
        if (hMenu == IntPtr.Zero) return;

        Opening?.Invoke(this, EventArgs.Empty);

        Dictionary<uint, ITrayMenuItemBase> idToItem = [];
        uint currentId = 1000;

        foreach (ITrayMenuItemBase item in _items)
        {
            if (!item.IsVisible) continue;

            if (item.Header == "-" || item is TraySeparator)
            {
                _ = User32.AppendMenu(hMenu, (uint)User32.MenuFlags.MF_SEPARATOR, 0, string.Empty);
            }
            else
            {
                var flags = User32.MenuFlags.MF_STRING;

                if (!item.IsEnabled)
                    flags |= User32.MenuFlags.MF_DISABLED | User32.MenuFlags.MF_GRAYED;

                if (item.IsChecked)
                    flags |= User32.MenuFlags.MF_CHECKED;

                _ = User32.AppendMenu(hMenu, (uint)flags, currentId, item.Header!);
                idToItem[currentId] = item;
                currentId++;
            }
        }

        _ = User32.GetCursorPos(out POINT pt);

        User32.TrackPopupMenuFlags flag =
            User32.TrackPopupMenuFlags.TPM_RETURNCMD |
            User32.TrackPopupMenuFlags.TPM_VERTICAL |
            User32.TrackPopupMenuFlags.TPM_LEFTALIGN;

        _ = User32.SetForegroundWindow(hWnd);
        uint selected = User32.TrackPopupMenuEx(hMenu, (uint)flag, pt.X, pt.Y, hWnd, IntPtr.Zero);
        _ = User32.PostMessage(hWnd, 0, IntPtr.Zero, IntPtr.Zero);

        if (selected != 0 && idToItem.TryGetValue(selected, out ITrayMenuItemBase? clickedItem))
        {
            clickedItem.Command?.Execute(clickedItem.CommandParameter);
        }

        User32.DestroyMenu(hMenu);

        Closed?.Invoke(this, EventArgs.Empty);
    }
}

public interface ITrayMenuItemBase
{
    public TrayMenu? Menu { get; set; }

    /// <summary>
    /// Bitmap
    /// </summary>
    public object? Icon { get; set; }

    public string? Header { get; set; }

    public bool IsVisible { get; set; }

    public bool IsChecked { get; set; }

    public bool IsEnabled { get; set; }

    public object? Tag { get; set; }

    public ICommand? Command { get; set; }

    public object? CommandParameter { get; set; }
}

public sealed class TraySeparator : DependencyObject, ITrayMenuItemBase
{
    public TrayMenu? Menu
    {
        get => null;
        set => throw new NotImplementedException();
    }

    /// <summary>
    /// Bitmap
    /// </summary>
    public object? Icon
    {
        get => null;
        set => throw new NotImplementedException();
    }

    public string? Header
    {
        get => "-";
        set => throw new NotImplementedException();
    }

    public bool IsVisible
    {
        get => true;
        set => throw new NotImplementedException();
    }

    public bool IsChecked
    {
        get => false;
        set => throw new NotImplementedException();
    }

    public bool IsEnabled
    {
        get => false;
        set => throw new NotImplementedException();
    }

    public ICommand? Command
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    public object? CommandParameter
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    public object? Tag { get; set; } = null;
}

public class TrayMenuItem : DependencyObject, ITrayMenuItemBase
{
    public static readonly DependencyProperty MenuProperty =
        DependencyProperty.Register(nameof(Menu), typeof(TrayMenu), typeof(TrayMenuItem), new(null));

    /// <summary>
    /// Bitmap
    /// </summary>
    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(object), typeof(TrayMenuItem), new(null));

    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register(nameof(Header), typeof(string), typeof(TrayMenuItem), new(null));

    public static readonly DependencyProperty IsCheckedProperty =
        DependencyProperty.Register(nameof(IsChecked), typeof(bool), typeof(TrayMenuItem), new(false));

    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(TrayMenuItem), new(null));

    public static readonly DependencyProperty CommandParameterProperty =
        DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(TrayMenuItem), new(null));

    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.Register(nameof(IsEnabled), typeof(bool), typeof(TrayMenuItem), new(true));

    public static readonly DependencyProperty IsVisibleProperty =
        DependencyProperty.Register(nameof(IsVisible), typeof(bool), typeof(TrayMenuItem), new(true));

    public TrayMenu? Menu
    {
        get => (TrayMenu)GetValue(MenuProperty);
        set => SetValue(MenuProperty, value);
    }

    /// <summary>
    /// Bitmap
    /// </summary>
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public string? Header
    {
        get => (string)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public bool IsChecked
    {
        get => (bool)GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }

    public bool IsEnabled
    {
        get => (bool)GetValue(IsEnabledProperty);
        set => SetValue(IsEnabledProperty, value);
    }

    public bool IsVisible
    {
        get => (bool)GetValue(IsVisibleProperty);
        set => SetValue(IsVisibleProperty, value);
    }

    public object? Tag { get; set; } = null;

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }
}

public class Win32Icon : IDisposable
{
    public nint Handle { get; private set; }

    public Win32Icon(Stream stream)
    {
        _ = stream ?? throw new ArgumentNullException(nameof(stream));

        using MemoryStream ms = new();
        stream.CopyTo(ms);
        byte[] bytes = ms.ToArray();

        // Parse ICONDIR (first 6 bytes)
        if (bytes.Length < 6)
            throw new InvalidDataException("Stream too short for ICONDIR.");

        ushort reserved = BitConverter.ToUInt16(bytes, 0); // Must be 0
        ushort type = BitConverter.ToUInt16(bytes, 2);     // 1 = ICON
        ushort count = BitConverter.ToUInt16(bytes, 4);    // Number of icons

        if (reserved != 0 || type != 1 || count == 0)
            throw new InvalidDataException("Invalid ICO header.");

        // Use only the first icon entry
        int entryOffset = 6;
        if (bytes.Length < entryOffset + 16)
            throw new InvalidDataException("Stream too short for ICONDIRENTRY.");

        uint imageSize = BitConverter.ToUInt32(bytes, entryOffset + 8);
        uint imageOffset = BitConverter.ToUInt32(bytes, entryOffset + 12);

        if (imageOffset + imageSize > bytes.Length)
            throw new InvalidDataException("Icon image out of bounds.");

        nint hIcon = User32.CreateIconFromResourceEx(
            ref bytes[imageOffset],
            imageSize,
            true,
            0x00030000,
            0, 0,
            0);

        if (hIcon == IntPtr.Zero)
            throw new InvalidOperationException("CreateIconFromResourceEx failed.");

        Handle = hIcon;
    }

    public void Dispose()
    {
        if (Handle != IntPtr.Zero)
        {
            _ = User32.DestroyIcon(Handle);
            Handle = IntPtr.Zero;
        }
    }
}
