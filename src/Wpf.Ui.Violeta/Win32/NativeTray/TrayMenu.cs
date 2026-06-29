using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace Wpf.Ui.Violeta.Win32;

public class TrayMenu : IEnumerable<ITrayMenuItemBase>, IList<ITrayMenuItemBase>
{
    public TrayMenuItem? Parent { get; internal set; }

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

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int IndexOf(ITrayMenuItemBase item) => _items.IndexOf(item);

    public void Insert(int index, ITrayMenuItemBase item) => _items.Insert(index, item);

    public void RemoveAt(int index) => _items.RemoveAt(index);

    public void Add(ITrayMenuItemBase item) => _items.Add(item);

    public void Clear() => _items.Clear();

    public bool Contains(ITrayMenuItemBase item) => _items.Contains(item);

    public void CopyTo(ITrayMenuItemBase[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);

    public bool Remove(ITrayMenuItemBase item) => _items.Remove(item);

    public void Open(
        nint hWnd,
        TrayContextMenuHorizontalAlignment horizontalAlignment = TrayContextMenuHorizontalAlignment.Left,
        TrayContextMenuVerticalAlignment verticalAlignment = TrayContextMenuVerticalAlignment.Top)
    {
        if (_items.Count == 0)
            return;

        Opening?.Invoke(this, EventArgs.Empty);

        Dictionary<uint, ITrayMenuItemBase> idToItem = [];
        List<nint> allMenus = [];
        List<nint> allBitmaps = [];
        uint currentId = 1000;

        nint hMenu = BuildMenu(_items, idToItem, allMenus, allBitmaps, ref currentId);
        if (hMenu == IntPtr.Zero)
            return;

        _ = User32.GetCursorPos(out POINT pt);

        User32.TrackPopupMenuFlags flag =
            User32.TrackPopupMenuFlags.TPM_RETURNCMD |
            User32.TrackPopupMenuFlags.TPM_VERTICAL |
            GetHorizontalAlignmentFlag(horizontalAlignment) |
            GetVerticalAlignmentFlag(verticalAlignment);

        _ = User32.SetForegroundWindow(hWnd);
        uint selected = User32.TrackPopupMenuEx(hMenu, (uint)flag, pt.X, pt.Y, hWnd, IntPtr.Zero);
        _ = User32.PostMessage(hWnd, 0, IntPtr.Zero, IntPtr.Zero);

        if (selected != 0 && idToItem.TryGetValue(selected, out ITrayMenuItemBase? clickedItem))
        {
            if (CanExecuteCommand(clickedItem.Command, clickedItem.CommandParameter))
                clickedItem.Command?.Execute(clickedItem.CommandParameter);
        }

        foreach (nint menu in allMenus)
            _ = User32.DestroyMenu(menu);

        foreach (nint bitmap in allBitmaps)
            _ = Gdi32.DeleteObject(bitmap);

        Closed?.Invoke(this, EventArgs.Empty);
    }

    private static bool CanExecuteCommand(ICommand? command, object? parameter)
    {
        return command switch
        {
            null => false,
            ITrayCommand trayCommand => trayCommand.CanExecute(),
            _ => command.CanExecute(parameter),
        };
    }

    private static User32.TrackPopupMenuFlags GetHorizontalAlignmentFlag(TrayContextMenuHorizontalAlignment alignment)
    {
        return alignment switch
        {
            TrayContextMenuHorizontalAlignment.Center => User32.TrackPopupMenuFlags.TPM_CENTERALIGN,
            TrayContextMenuHorizontalAlignment.Right => User32.TrackPopupMenuFlags.TPM_RIGHTALIGN,
            _ => User32.TrackPopupMenuFlags.TPM_LEFTALIGN,
        };
    }

    private static User32.TrackPopupMenuFlags GetVerticalAlignmentFlag(TrayContextMenuVerticalAlignment alignment)
    {
        return alignment switch
        {
            TrayContextMenuVerticalAlignment.Center => User32.TrackPopupMenuFlags.TPM_VCENTERALIGN,
            TrayContextMenuVerticalAlignment.Bottom => User32.TrackPopupMenuFlags.TPM_BOTTOMALIGN,
            _ => User32.TrackPopupMenuFlags.TPM_TOPALIGN,
        };
    }

    private static nint BuildMenu(IList<ITrayMenuItemBase> items, Dictionary<uint, ITrayMenuItemBase> idToItem, List<nint> allMenus, List<nint> allBitmaps, ref uint currentId)
    {
        nint hMenu = User32.CreatePopupMenu();
        if (hMenu == IntPtr.Zero)
            return IntPtr.Zero;

        allMenus.Add(hMenu);
        uint menuPosition = 0;

        foreach (ITrayMenuItemBase item in items)
        {
            if (!item.IsVisible)
                continue;

            if (item.Header == "-" || item is TraySeparator)
            {
                _ = User32.AppendMenu(hMenu, (uint)User32.MenuFlags.MF_SEPARATOR, 0, string.Empty);
                menuPosition++;
            }
            else
            {
                TrayMenu? submenu = item.Menu;
                bool hasSubmenu = submenu != null && submenu.Count > 0;
                bool canExecute = item.Command switch
                {
                    null => true,
                    ITrayCommand trayCommand => trayCommand.CanExecute(),
                    _ => item.Command!.CanExecute(item.CommandParameter),
                };

                if (hasSubmenu)
                {
                    nint hSubMenu = BuildMenu(submenu!.Items, idToItem, allMenus, allBitmaps, ref currentId);
                    if (hSubMenu != IntPtr.Zero)
                    {
                        var flags = User32.MenuFlags.MF_STRING | User32.MenuFlags.MF_POPUP;
                        if (!item.IsEnabled || !canExecute)
                            flags |= User32.MenuFlags.MF_DISABLED | User32.MenuFlags.MF_GRAYED;

                        _ = User32.AppendMenu(hMenu, (uint)flags, hSubMenu, item.Header!);
                        ApplyMenuItemBitmap(hMenu, menuPosition, item, allBitmaps);
                        menuPosition++;
                    }
                }
                else
                {
                    var flags = User32.MenuFlags.MF_STRING;
                    if (!item.IsEnabled || !canExecute)
                        flags |= User32.MenuFlags.MF_DISABLED | User32.MenuFlags.MF_GRAYED;

                    if (item.IsChecked)
                        flags |= User32.MenuFlags.MF_CHECKED;

                    _ = User32.AppendMenu(hMenu, (uint)flags, currentId, item.Header!);
                    ApplyMenuItemBitmap(hMenu, menuPosition, item, allBitmaps);

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

                        if (!item.IsEnabled || !canExecute)
                            menuItemInfo.fState |= (uint)User32.MenuItemState.MFS_DISABLED;

                        _ = User32.SetMenuItemInfo(hMenu, currentId, false, ref menuItemInfo);
                    }

                    idToItem[currentId] = item;
                    currentId++;
                    menuPosition++;
                }
            }
        }

        return hMenu;
    }

    private static void ApplyMenuItemBitmap(nint hMenu, uint menuPosition, ITrayMenuItemBase item, List<nint> tempBitmaps)
    {
        if (!TryGetMenuBitmap(item.Icon, out nint hBitmap, out bool shouldDisposeBitmap))
            return;

        var menuItemInfo = new User32.MENUITEMINFO
        {
            cbSize = (uint)Marshal.SizeOf<User32.MENUITEMINFO>(),
            fMask = (uint)User32.MenuItemMask.MIIM_BITMAP,
            hbmpItem = hBitmap,
        };

        bool setResult = User32.SetMenuItemInfo(hMenu, menuPosition, true, ref menuItemInfo);

        if (!setResult && shouldDisposeBitmap)
        {
            _ = Gdi32.DeleteObject(hBitmap);
            return;
        }

        if (setResult && shouldDisposeBitmap)
            tempBitmaps.Add(hBitmap);
    }

    private static bool TryGetMenuBitmap(object? icon, out nint hBitmap, out bool shouldDisposeBitmap)
    {
        hBitmap = IntPtr.Zero;
        shouldDisposeBitmap = false;

        if (icon is null)
            return false;

        if (icon is nint directBitmap && directBitmap != IntPtr.Zero)
        {
            hBitmap = directBitmap;
            return true;
        }

        if (icon is Win32Image win32Image)
            return win32Image.TryCreateMenuBitmap(out hBitmap, out shouldDisposeBitmap);

        if (icon is Win32Icon win32Icon)
            return win32Icon.TryCreateMenuBitmap(out hBitmap, out shouldDisposeBitmap);

        return false;
    }
}
