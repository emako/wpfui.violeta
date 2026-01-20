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

/// <summary>
/// Manages a Win32 tray icon and its context menu for WPF applications.
/// </summary>
public class TrayIconHost : IDisposable
{
    /// <summary>
    /// Handle to the hidden window hosting the tray icon.
    /// </summary>
    private readonly nint hWnd = IntPtr.Zero;

    /// <summary>
    /// Delegate for window procedure.
    /// </summary>
    private readonly User32.WndProcDelegate wndProcDelegate = null!;

    /// <summary>
    /// Data structure for the tray icon.
    /// </summary>
    private Shell32.NotifyIconData notifyIconData = default;

    /// <summary>
    /// Unique ID for this tray icon.
    /// </summary>
    private readonly int id = default;

    /// <summary>
    /// Static counter for unique IDs.
    /// </summary>
    private static int nextId = 0;

    /// <summary>
    /// Windows message ID for TaskbarCreated broadcast.
    /// </summary>
    private readonly uint taskbarCreatedMessageId = 0;

    /// <summary>
    /// Tooltip text for the tray icon.
    /// </summary>
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

    /// <summary>
    /// Handle to the icon image.
    /// </summary>
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

    /// <summary>
    /// Whether the tray icon is visible.
    /// </summary>
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

    /// <summary>
    /// Balloon tip text.
    /// </summary>
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

    /// <summary>
    /// Balloon tip icon type.
    /// </summary>
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

    /// <summary>
    /// Balloon tip title.
    /// </summary>
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

    /// <summary>
    /// User-defined tag.
    /// </summary>
    public object? Tag { get; set; } = null;

    /// <summary>
    /// Context menu for the tray icon.
    /// </summary>
    public TrayMenu Menu { get; set; } = null!;

    /// <summary>
    /// Occurs when the balloon tip is clicked.
    /// </summary>
    public event EventHandler<EventArgs>? BalloonTipClicked = null;

    /// <summary>
    /// Occurs when the balloon tip is closed.
    /// </summary>
    public event EventHandler<EventArgs>? BalloonTipClosed = null;

    /// <summary>
    /// Occurs when the balloon tip is shown.
    /// </summary>
    public event EventHandler<EventArgs>? BalloonTipShown = null;

    /// <summary>
    /// Occurs when the tray icon is clicked.
    /// </summary>
    public event EventHandler<EventArgs>? Click = null;

    /// <summary>
    /// Occurs when the right mouse button is pressed down on the tray icon.
    /// </summary>
    public event EventHandler<EventArgs>? RightDown = null;

    /// <summary>
    /// Occurs when the right mouse button is clicked on the tray icon.
    /// </summary>
    public event EventHandler<EventArgs>? RightClick = null;

    /// <summary>
    /// Occurs when the right mouse button is double-clicked on the tray icon.
    /// </summary>
    public event EventHandler<EventArgs>? RightDoubleClick = null;

    /// <summary>
    /// Occurs when the left mouse button is pressed down on the tray icon.
    /// </summary>
    public event EventHandler<EventArgs>? LeftDown = null;

    /// <summary>
    /// Occurs when the left mouse button is clicked on the tray icon.
    /// </summary>
    public event EventHandler<EventArgs>? LeftClick = null;

    /// <summary>
    /// Occurs when the left mouse button is double-clicked on the tray icon.
    /// </summary>
    public event EventHandler<EventArgs>? LeftDoubleClick = null;

    /// <summary>
    /// Occurs when the middle mouse button is pressed down on the tray icon.
    /// </summary>
    public event EventHandler<EventArgs>? MiddleDown = null;

    /// <summary>
    /// Occurs when the middle mouse button is clicked on the tray icon.
    /// </summary>
    public event EventHandler<EventArgs>? MiddleClick = null;

    /// <summary>
    /// Occurs when the middle mouse button is double-clicked on the tray icon.
    /// </summary>
    public event EventHandler<EventArgs>? MiddleDoubleClick = null;

    public TrayIconHost()
    {
        id = ++nextId;

        // Register for TaskbarCreated message to handle Explorer restarts
        taskbarCreatedMessageId = User32.RegisterWindowMessage("TaskbarCreated");

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
        // Handle TaskbarCreated message to re-register tray icon after Explorer restart
        if (msg == taskbarCreatedMessageId)
        {
            // Re-add the tray icon
            _ = Shell32.Shell_NotifyIcon((int)Shell32.NOTIFY_COMMAND.NIM_ADD, ref notifyIconData);
            return IntPtr.Zero;
        }

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

    /// <summary>
    /// Shows the context menu associated with the tray icon.
    /// </summary>
    public virtual void ShowContextMenu()
    {
        Menu?.Open(hWnd);
    }

    /// <summary>
    /// Shows a balloon tip with the specified timeout.
    /// </summary>
    public virtual void ShowBalloonTip(int timeout)
    {
        ShowBalloonTip(timeout, BalloonTipTitle, BalloonTipText, BalloonTipIcon);
    }

    /// <summary>
    /// Shows a balloon tip with the specified parameters.
    /// </summary>
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

    /// <summary>
    /// Disposes the tray icon and its resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Remove tray icon
            _ = Shell32.Shell_NotifyIcon((int)Shell32.NOTIFY_COMMAND.NIM_DELETE, ref notifyIconData);

            // Clean up icon resources
            if (notifyIconData.hIcon != IntPtr.Zero)
            {
                _ = User32.DestroyIcon(notifyIconData.hIcon);
                notifyIconData.hIcon = IntPtr.Zero;
            }

            // Destroy window
            if (hWnd != IntPtr.Zero)
            {
                _ = User32.DestroyWindow(hWnd);
            }
        }
    }

    ~TrayIconHost()
    {
        Dispose(false);
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
    /// An error icon.
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

        Opening?.Invoke(this, EventArgs.Empty);

        Dictionary<uint, ITrayMenuItemBase> idToItem = [];
        List<nint> allMenus = [];
        uint currentId = 1000;

        nint hMenu = BuildMenu(_items, idToItem, allMenus, ref currentId);
        if (hMenu == IntPtr.Zero) return;

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

        // Destroy all menus (main menu and submenus)
        foreach (nint menu in allMenus)
        {
            User32.DestroyMenu(menu);
        }

        Closed?.Invoke(this, EventArgs.Empty);
    }

    private static nint BuildMenu(IList<ITrayMenuItemBase> items, Dictionary<uint, ITrayMenuItemBase> idToItem, List<nint> allMenus, ref uint currentId)
    {
        nint hMenu = User32.CreatePopupMenu();
        if (hMenu == IntPtr.Zero) return IntPtr.Zero;

        allMenus.Add(hMenu);

        foreach (ITrayMenuItemBase item in items)
        {
            if (!item.IsVisible) continue;

            if (item.Header == "-" || item is TraySeparator)
            {
                _ = User32.AppendMenu(hMenu, (uint)User32.MenuFlags.MF_SEPARATOR, 0, string.Empty);
            }
            else
            {
                // Check if this item has a submenu
                TrayMenu? submenu = item.Menu;
                bool hasSubmenu = submenu != null && submenu.Count > 0;

                if (hasSubmenu)
                {
                    // Recursively build the submenu
                    nint hSubMenu = BuildMenu(submenu!.Items, idToItem, allMenus, ref currentId);
                    if (hSubMenu != IntPtr.Zero)
                    {
                        var flags = User32.MenuFlags.MF_STRING | User32.MenuFlags.MF_POPUP;

                        if (!item.IsEnabled)
                            flags |= User32.MenuFlags.MF_DISABLED | User32.MenuFlags.MF_GRAYED;

                        _ = User32.AppendMenu(hMenu, (uint)flags, hSubMenu, item.Header!);
                    }
                }
                else
                {
                    var flags = User32.MenuFlags.MF_STRING;

                    if (!item.IsEnabled)
                        flags |= User32.MenuFlags.MF_DISABLED | User32.MenuFlags.MF_GRAYED;

                    if (item.IsChecked)
                        flags |= User32.MenuFlags.MF_CHECKED;

                    _ = User32.AppendMenu(hMenu, (uint)flags, currentId, item.Header!);

                    if (item.IsBold)
                    {
                        var menuItemInfo = new User32.MENUITEMINFO
                        {
                            cbSize = (uint)Marshal.SizeOf<User32.MENUITEMINFO>(),
                            fMask = (uint)User32.MenuItemMask.MIIM_STATE,
                            fState = (uint)User32.MenuItemState.MFS_DEFAULT
                        };

                        if (item.IsChecked)
                            menuItemInfo.fState |= (uint)User32.MenuItemState.MFS_CHECKED;

                        if (!item.IsEnabled)
                            menuItemInfo.fState |= (uint)User32.MenuItemState.MFS_DISABLED;

                        _ = User32.SetMenuItemInfo(hMenu, currentId, false, ref menuItemInfo);
                    }

                    idToItem[currentId] = item;
                    currentId++;
                }
            }
        }

        return hMenu;
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

    public bool IsBold { get; set; }

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

    public bool IsBold
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

    public static readonly DependencyProperty IsBoldProperty =
        DependencyProperty.Register(nameof(IsBold), typeof(bool), typeof(TrayMenuItem), new(false));

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

    public bool IsBold
    {
        get => (bool)GetValue(IsBoldProperty);
        set => SetValue(IsBoldProperty, value);
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
