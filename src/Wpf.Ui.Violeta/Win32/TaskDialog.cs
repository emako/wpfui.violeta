using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Wpf.Ui.Violeta.Win32;

/// <summary>
/// Win32 TaskDialog wrapper with optional dark-mode support.
/// Call <see cref="SetTheme"/> before displaying any TaskDialog to choose dark or light.
/// Ported from https://github.com/SFTRS/DarkTaskDialog
/// Pure C# re-implementation using window subclassing instead of Microsoft Detours.
/// Dark mode is applied via:
///   * DwmSetWindowAttribute (DWMWA_USE_IMMERSIVE_DARK_MODE) for the title bar
///   * SetWindowTheme("DarkMode_Explorer") on buttons, scrollbars, DirectUIHWND
///   * Window subclassing to handle WM_ERASEBKGND / WM_CTLCOLORDLG
///   * SysLink color override via LM_SETITEM / LIS_DEFAULTCOLORS
/// </summary>
public static class TaskDialog
{
    // -- Standard icon "MAKEINTRESOURCE" handles ------------------------------
    public static readonly nint IconWarning = 0xFFFF; // TD_WARNING_ICON

    public static readonly nint IconError = 0xFFFE; // TD_ERROR_ICON
    public static readonly nint IconInformation = 0xFFFD; // TD_INFORMATION_ICON
    public static readonly nint IconShield = 0xFFFC; // TD_SHIELD_ICON
    public static readonly nint IconShieldBlue = 0xFFFB;
    public static readonly nint IconShieldGray = 0xFFFA;
    public static readonly nint IconShieldWarningYellow = 0xFFF9;
    public static readonly nint IconShieldErrorRed = 0xFFF8;
    public static readonly nint IconShieldSuccessGreen = 0xFFF7;

    // -- Theme ----------------------------------------------------------------
    public enum Theme { Dark, Light }

    private static volatile Theme _theme = Theme.Light;
    private static readonly object _lock = new();

    // GDI brushes (created once, never destroyed – process-lifetime)
    private static nint _darkBrush = IntPtr.Zero; // RGB(36,36,36)

    private static nint _notSoDarkBrush = IntPtr.Zero; // RGB(54,54,54)

    // Static delegate fields – MUST be kept alive (prevent GC collection)
    private static TaskDialogCallbackProc? _callbackProc;

    private static SubclassProcDelegate? _subclassProc;
    private static EnumChildProcDelegate? _enumChildProc;

    // Currently visible dialogs
    private static readonly HashSet<nint> _activeDialogs = [];

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    /// <summary>Set the dark/light theme for all future (and existing) TaskDialogs.</summary>
    public static void SetTheme(Theme theme)
    {
        Theme old;
        lock (_lock) { old = _theme; _theme = theme; }
        if (old == theme) return;

        lock (_lock)
        {
            foreach (nint hwnd in _activeDialogs)
                ApplyThemeToWindow(hwnd, isThemeSwitch: true);
        }
    }

    /// <summary>Show a TaskDialog using the provided <paramref name="config"/>.</summary>
    /// <returns>The button ID that closed the dialog (IDOK = 1, IDCANCEL = 2, etc.),
    /// or the custom button ID.</returns>
    public static int ShowIndirect(TaskDialogConfig config) =>
        ShowIndirect(config, out _, out _);

    /// <inheritdoc cref="ShowIndirect(TaskDialogConfig)"/>
    public static int ShowIndirect(
        TaskDialogConfig config,
        out int radioButtonId,
        out bool verificationChecked)
    {
        EnsureStaticDelegates();
        EnsureBrushes();

        var wrapper = new CallbackWrapper
        {
            UserCallback = config.Callback,
            UserData = config.CallbackData,
        };

        var allocations = new List<nint>();
        GCHandle wrapperHandle = default;
        try
        {
            wrapperHandle = GCHandle.Alloc(wrapper);
            NativeTaskDialogConfig nc = BuildNativeConfig(config, wrapperHandle, allocations);

            int hresult = TaskDialogIndirect(
                ref nc,
                out int btn,
                out int radio,
                out int verifChecked);

            radioButtonId = radio;
            verificationChecked = verifChecked != 0;

            if (hresult != 0)
                Marshal.ThrowExceptionForHR(hresult);

            return btn;
        }
        finally
        {
            if (wrapperHandle.IsAllocated && !wrapper.HandleFreedByCallback)
                wrapperHandle.Free();

            foreach (nint p in allocations)
                if (p != IntPtr.Zero)
                    Marshal.FreeHGlobal(p);
        }
    }

    /// <summary>Convenience overload matching the C++ sample usage.</summary>
    public static int Show(
        nint ownerHwnd,
        string? title,
        string? mainInstruction,
        string? content,
        TaskDialogCommonButton commonButtons = TaskDialogCommonButton.OK,
        nint mainIcon = default,
        TaskDialogFlags flags = TaskDialogFlags.None,
        TaskDialogCallbackDelegate? callback = null,
        object? callbackData = null)
    {
        return ShowIndirect(new TaskDialogConfig
        {
            ParentWindow = ownerHwnd,
            Title = title,
            MainInstruction = mainInstruction,
            Content = content,
            CommonButtons = commonButtons,
            MainIcon = mainIcon,
            Flags = flags,
            Callback = callback,
            CallbackData = callbackData,
        });
    }

    // -------------------------------------------------------------------------
    // Internals – delegate lifetime helpers
    // -------------------------------------------------------------------------

    private static void EnsureStaticDelegates()
    {
        if (_callbackProc is not null) return;
        _callbackProc = InternalTaskDialogCallback;
        _subclassProc = SubclassProcImpl;
        _enumChildProc = EnumChildProcImpl;
        EnsureNativeExports();
    }

    // Native-export availability checks to avoid EntryPointNotFoundException
    private static bool _nativeExportsChecked = false;

    private static bool _hasComCtlSubclassApi = false;
    private static bool _hasDefSubclassProcApi = false;
    private static bool _hasSetWindowThemeApi = false;
    private static bool _hasDwmSetWindowAttributeApi = false;

    private static void EnsureNativeExports()
    {
        if (_nativeExportsChecked) return;
        _nativeExportsChecked = true;

        try
        {
            nint hComCtl = GetModuleHandle("comctl32.dll");
            if (hComCtl == IntPtr.Zero)
                hComCtl = LoadLibrary("comctl32.dll");

            _hasComCtlSubclassApi = hComCtl != IntPtr.Zero && GetProcAddress(hComCtl, "SetWindowSubclass") != IntPtr.Zero;
            _hasDefSubclassProcApi = hComCtl != IntPtr.Zero && GetProcAddress(hComCtl, "DefSubclassProc") != IntPtr.Zero;

            nint hUx = GetModuleHandle("uxtheme.dll");
            if (hUx == IntPtr.Zero) hUx = LoadLibrary("uxtheme.dll");
            _hasSetWindowThemeApi = hUx != IntPtr.Zero && GetProcAddress(hUx, "SetWindowTheme") != IntPtr.Zero;

            nint hDwm = GetModuleHandle("dwmapi.dll");
            if (hDwm == IntPtr.Zero) hDwm = LoadLibrary("dwmapi.dll");
            _hasDwmSetWindowAttributeApi = hDwm != IntPtr.Zero && GetProcAddress(hDwm, "DwmSetWindowAttribute") != IntPtr.Zero;

            Log("Native exports: comctl32.SetWindowSubclass={0}, comctl32.DefSubclassProc={1}, uxtheme.SetWindowTheme={2}, dwmapi.DwmSetWindowAttribute={3}",
                _hasComCtlSubclassApi, _hasDefSubclassProcApi, _hasSetWindowThemeApi, _hasDwmSetWindowAttributeApi);
        }
        catch (Exception ex)
        {
            Log("EnsureNativeExports failed: {0}\n{1}", ex, ex.StackTrace);
        }
    }

    private static void EnsureBrushes()
    {
        if (_darkBrush != IntPtr.Zero) return;
        _darkBrush = CreateSolidBrush(0x00242424u); // RGB(36,36,36)
        _notSoDarkBrush = CreateSolidBrush(0x00363636u); // RGB(54,54,54)
    }

    // -------------------------------------------------------------------------
    // Native config builder
    // -------------------------------------------------------------------------

    private static NativeTaskDialogConfig BuildNativeConfig(
        TaskDialogConfig config,
        GCHandle wrapperHandle,
        List<nint> allocations)
    {
        var nc = new NativeTaskDialogConfig
        {
            cbSize = (uint)Marshal.SizeOf<NativeTaskDialogConfig>(),
            hwndParent = config.ParentWindow,
            hInstance = IntPtr.Zero,
            dwFlags = (uint)config.Flags,
            dwCommonButtons = (uint)config.CommonButtons,
            pszWindowTitle = AllocString(config.Title, allocations),
            mainIconUnion = config.MainIcon,
            pszMainInstruction = AllocString(config.MainInstruction, allocations),
            pszContent = AllocString(config.Content, allocations),
            nDefaultButton = config.DefaultButton,
            nDefaultRadioButton = config.DefaultRadioButton,
            pszVerificationText = AllocString(config.VerificationText, allocations),
            pszExpandedInformation = AllocString(config.ExpandedInformation, allocations),
            pszExpandedControlText = AllocString(config.ExpandedControlText, allocations),
            pszCollapsedControlText = AllocString(config.CollapsedControlText, allocations),
            footerIconUnion = config.FooterIcon,
            pszFooter = AllocString(config.Footer, allocations),
            pfCallback = Marshal.GetFunctionPointerForDelegate(_callbackProc!),
            lpCallbackData = GCHandle.ToIntPtr(wrapperHandle),
            cxWidth = config.Width,
        };

        if (config.Buttons is { Count: > 0 })
        {
            nc.cButtons = (uint)config.Buttons.Count;
            nc.pButtons = AllocButtons(config.Buttons, allocations);
        }

        if (config.RadioButtons is { Count: > 0 })
        {
            nc.cRadioButtons = (uint)config.RadioButtons.Count;
            nc.pRadioButtons = AllocButtons(config.RadioButtons, allocations);
        }

        return nc;
    }

    private static nint AllocString(string? s, List<nint> allocations)
    {
        if (s is null) return IntPtr.Zero;
        var ptr = Marshal.StringToHGlobalUni(s);
        allocations.Add(ptr);
        return ptr;
    }

    private static nint AllocButtons(IList<TaskDialogButtonSpec> buttons, List<nint> allocations)
    {
        int btnSize = Marshal.SizeOf<NativeTaskDialogButton>();
        nint array = Marshal.AllocHGlobal(btnSize * buttons.Count);
        allocations.Add(array);

        for (int i = 0; i < buttons.Count; i++)
        {
            var nb = new NativeTaskDialogButton
            {
                nButtonID = buttons[i].Id,
                pszButtonText = AllocString(buttons[i].Text, allocations),
            };
            Marshal.StructureToPtr(nb, IntPtr.Add(array, i * btnSize), false);
        }
        return array;
    }

    // -------------------------------------------------------------------------
    // Internal TaskDialog callback – mirrors taskdialogcallback() in C++
    // -------------------------------------------------------------------------

    private const uint TDN_CREATED = 0;
    private const uint TDN_NAVIGATED = 1;
    private const uint TDN_DESTROYED = 5;
    private const uint TDN_DIALOG_CONSTRUCTED = 7;

    private const uint WM_USER = 0x0400;
    private const uint MY_APPLY_THEME_MSG = WM_USER + 0x100;
    private const uint RDW_INVALIDATE = 0x0001;
    private const uint RDW_ERASE = 0x0004;
    private const uint RDW_ALLCHILDREN = 0x0080;
    private const uint RDW_UPDATENOW = 0x0100;
    private const uint WM_THEMECHANGED = 0x031A;

    private static int InternalTaskDialogCallback(
        nint hwnd,
        uint msg,
        nint wParam,
        nint lParam,
        nint lpRefData)
    {
        try
        {
            Log("InternalTaskDialogCallback msg={0}", msg);
            GCHandle handle = GCHandle.FromIntPtr(lpRefData);
            CallbackWrapper wrapper = (CallbackWrapper)handle.Target!;

            if (msg == TDN_CREATED)
            {
                lock (_lock) _activeDialogs.Add(hwnd);

                // Subclass the dialog itself for WM_ERASEBKGND / WM_CTLCOLORDLG
                // and post a message so the theme application runs on the dialog's
                // message loop instead of inside the TaskDialog callback context.
                try
                {
                    if (_hasComCtlSubclassApi)
                    {
                        SetWindowSubclass(hwnd, _subclassProc!, (nint)1, IntPtr.Zero);
                    }
                    else
                    {
                        Log("SetWindowSubclass not available; skipping subclassing for hwnd={0}", hwnd);
                    }
                    PostMessage(hwnd, MY_APPLY_THEME_MSG, IntPtr.Zero, IntPtr.Zero);
                }
                catch (EntryPointNotFoundException ex)
                {
                    Log("SetWindowSubclass EntryPointNotFound: {0}", ex);
                    _hasComCtlSubclassApi = false;
                }
                catch (Exception ex)
                {
                    Log("SetWindowSubclass/PostMessage failed: {0}", ex);
                }
            }
            else if (msg == TDN_DIALOG_CONSTRUCTED)
            {
                // Called on first show AND after each TDM_NAVIGATE_PAGE – re-apply theming
                // Post message so enumeration and theme changes execute on the
                // dialog thread normally and outside the TaskDialog callback.
                try
                {
                    PostMessage(hwnd, MY_APPLY_THEME_MSG, IntPtr.Zero, IntPtr.Zero);
                }
                catch (Exception ex)
                {
                    Log("PostMessage for DIALOG_CONSTRUCTED failed: {0}", ex);
                }
            }
            else if (msg == TDN_DESTROYED)
            {
                lock (_lock) _activeDialogs.Remove(hwnd);
                wrapper.HandleFreedByCallback = true;
                handle.Free();
            }

            try
            {
                return wrapper.UserCallback?.Invoke(
                    hwnd,
                    (TaskDialogNotification)msg,
                    wParam,
                    lParam,
                    wrapper.UserData) ?? 0;
            }
            catch (Exception ex)
            {
                Log("User callback threw: {0}\n{1}", ex, ex.StackTrace);
                return 0;
            }
        }
        catch (Exception ex)
        {
            Log("Unhandled exception in InternalTaskDialogCallback: {0}\n{1}", ex, ex.StackTrace);
            return 0;
        }
    }

    // -------------------------------------------------------------------------
    // Dark mode application – mirrors update() + EnumChildProc in C++
    // -------------------------------------------------------------------------

    private static void ApplyThemeToWindow(nint hwnd, bool isThemeSwitch)
    {
        int dark = _theme == Theme.Dark ? 1 : 0;
        DwmSetWindowAttribute(hwnd, 20 /*DWMWA_USE_IMMERSIVE_DARK_MODE*/, ref dark, sizeof(int));
        // Attempt to set theme on the dialog window itself first
        try
        {
            TrySetWindowTheme(hwnd, _theme == Theme.Dark);
        }
        catch (Exception ex)
        {
            Log("TrySetWindowTheme on dialog hwnd={0} failed: {1}", hwnd, ex);
        }

        EnumChildWindows(hwnd, _enumChildProc!, isThemeSwitch ? (nint)1 : IntPtr.Zero);
        try
        {
            // Force redraw of the dialog and all children to apply visual changes
            uint flags = RDW_INVALIDATE | RDW_ERASE | RDW_ALLCHILDREN | RDW_UPDATENOW;
            RedrawWindow(hwnd, IntPtr.Zero, IntPtr.Zero, flags);
        }
        catch (Exception ex)
        {
            Log("RedrawWindow failed for hwnd={0}: {1}", hwnd, ex);
        }
        try
        {
            // Notify theme change in case controls respond to WM_THEMECHANGED
            SendMessage(hwnd, WM_THEMECHANGED, IntPtr.Zero, IntPtr.Zero);
        }
        catch (Exception ex)
        {
            Log("SendMessage(WM_THEMECHANGED) failed for hwnd={0}: {1}", hwnd, ex);
        }
    }

    private static bool EnumChildProcImpl(nint hwnd, nint lParam)
    {
        try
        {
            bool isDark = _theme == Theme.Dark;
            string cls = GetWindowClass(hwnd);

            // Diagnostic: log each enumerated child to help find which window hosts content
            Log("EnumChild: hwnd={0}, class={1}, isDark={2}", hwnd, cls, isDark);

            // Recursively enumerate grandchildren first (same order as C++ original)
            EnumChildWindows(hwnd, _enumChildProc!, lParam);

            // Special-case DirectUIHWND: apply theme but DO NOT subclass it,
            // because subclassing DirectUIHWND can prevent its internal renderer
            // from receiving paint/erase messages and lead to a blank area.
            if (cls == "DirectUIHWND")
            {
                if (isDark)
                {
                    nint parent = GetParent(hwnd);
                    GetWindowPlacement(parent, out NATIVE_WINDOWPLACEMENT placement);
                    TrySetWindowTheme(hwnd, true);
                    SetWindowPlacement(parent, ref placement);
                    try
                    {
                        RedrawWindow(hwnd, IntPtr.Zero, IntPtr.Zero, RDW_INVALIDATE | RDW_ERASE | RDW_UPDATENOW);
                    }
                    catch (Exception ex)
                    {
                        Log("RedrawWindow for DirectUIHWND failed for hwnd={0}: {1}", hwnd, ex);
                    }
                    try
                    {
                        SendMessage(hwnd, WM_THEMECHANGED, IntPtr.Zero, IntPtr.Zero);
                    }
                    catch (Exception ex)
                    {
                        Log("SendMessage(WM_THEMECHANGED) for DirectUIHWND failed for hwnd={0}: {1}", hwnd, ex);
                    }
                }
                else
                {
                    SetWindowTheme(hwnd, null, null);
                    try
                    {
                        RedrawWindow(hwnd, IntPtr.Zero, IntPtr.Zero, RDW_INVALIDATE | RDW_ERASE | RDW_UPDATENOW);
                    }
                    catch (Exception ex)
                    {
                        Log("RedrawWindow for DirectUIHWND failed for hwnd={0}: {1}", hwnd, ex);
                    }
                }

                // Skip subclassing and other changes for DirectUIHWND.
                return true;
            }

            if (cls == "SysLink")
            {
                // Control link default colors: clear them in dark mode so text stays white
                try { ApplySysLinkColors(hwnd, isDark); }
                catch (Exception ex) { Log("ApplySysLinkColors failed for {0}: {1}", hwnd, ex); }
            }

            if (_hasComCtlSubclassApi)
            {
                try
                {
                    if (isDark)
                    {
                        // Only subclass if not already subclassed
                        if (!GetWindowSubclass(hwnd, _subclassProc!, (nint)1, out _))
                            SetWindowSubclass(hwnd, _subclassProc!, (nint)1, IntPtr.Zero);
                    }
                    else
                    {
                        RemoveWindowSubclass(hwnd, _subclassProc!, (nint)1);
                    }
                }
                catch (EntryPointNotFoundException ex)
                {
                    Log("Subclass API EntryPointNotFound during use: {0}", ex);
                    _hasComCtlSubclassApi = false;
                }
                catch (Exception ex)
                {
                    Log("Subclass API call failed: {0}", ex);
                }
            }

            if (cls is "Button" or "ScrollBar")
            {
                try { TrySetWindowTheme(hwnd, isDark); }
                catch (Exception ex) { Log("TrySetWindowTheme failed for {0} ({1}): {2}", cls, hwnd, ex); }
            }

            return true;
        }
        catch (Exception ex)
        {
            Log("Unhandled exception in EnumChildProcImpl for hwnd={0}: {1}\n{2}", hwnd, ex, ex.StackTrace!);
            // Returning true continues enumeration despite the error.
            return true;
        }
    }

    private static void ApplySysLinkColors(nint hwnd, bool darkMode)
    {
        // LM_SETITEM = WM_USER + 2 = 0x0402
        // LITEM.mask = LIF_ITEMINDEX | LIF_STATE, stateMask = LIS_DEFAULTCOLORS
        // When darkMode: state = 0 (clear LIS_DEFAULTCOLORS so system won't paint blue links)
        // When lightMode: state = LIS_DEFAULTCOLORS (restore)
        const uint LM_SETITEM = 0x0402;
        const uint LIF_ITEMINDEX = 0x0001;
        const uint LIF_STATE = 0x0002;
        const uint LIS_DEFAULTCOLORS = 0x0004;

        // LITEM layout (Pack=1):
        //   mask:      UINT   (4)
        //   iLink:     int    (4)
        //   state:     UINT   (4)
        //   stateMask: UINT   (4)
        //   szID:      WCHAR[48] (96 bytes)
        //   szUrl:     WCHAR[2084] (4168 bytes)
        // Total = 4280 bytes – allocate on heap to avoid stack overflow risk
        int litemSize = 4 + 4 + 4 + 4 + 96 + 4168;
        nint litem = Marshal.AllocHGlobal(litemSize);
        try
        {
            const int MAX_LINKS = 1024; // safety cap to avoid infinite loops
            for (int i = 0; i < MAX_LINKS; i++)
            {
                // Clear the allocation to zero for each iteration
                for (int b = 0; b < litemSize; b++)
                    Marshal.WriteByte(litem, b, 0);

                Marshal.WriteInt32(litem, 0, (int)(LIF_ITEMINDEX | LIF_STATE));
                Marshal.WriteInt32(litem, 4, i);
                Marshal.WriteInt32(litem, 8, darkMode ? 0 : (int)LIS_DEFAULTCOLORS);
                Marshal.WriteInt32(litem, 12, (int)LIS_DEFAULTCOLORS);

                nint result = SendMessage(hwnd, LM_SETITEM, IntPtr.Zero, litem);
                if (result == IntPtr.Zero) break; // no more items
                // If we've reached the cap, log a warning.
                if (i == MAX_LINKS - 1)
                    Log("ApplySysLinkColors: reached MAX_LINKS for hwnd={0}", hwnd);
            }
        }
        finally
        {
            Marshal.FreeHGlobal(litem);
        }
    }

    // -------------------------------------------------------------------------
    // Window subclass procedure – mirrors Subclassproc() in C++
    // -------------------------------------------------------------------------

    private static nint SubclassProcImpl(
        nint hWnd,
        uint uMsg,
        nint wParam,
        nint lParam,
        nint uIdSubclass,
        nint dwRefData)
    {
        try
        {
            if (_theme != Theme.Dark)
                return DefSubclassProc(hWnd, uMsg, wParam, lParam);

            const uint WM_ERASEBKGND = 0x0014;
            const uint WM_CTLCOLORDLG = 0x0136;

            switch (uMsg)
            {
                case WM_ERASEBKGND:
                    {
                        // Set white text for SysLink controls (which inherit from parent WM_ERASEBKGND)
                        SetTextColor(wParam, 0x00FFFFFF); // white

                        string cls = GetWindowClass(hWnd);
                        if (cls == "SysLink")
                        {
                            // Don't erase background for links – avoids white flash on page switches
                            return (nint)1;
                        }

                        // Let DirectUIHWND default handling occur – subclassing it may
                        // interfere with its internal renderer and produce blank content.
                        if (cls == "DirectUIHWND")
                        {
                            return DefSubclassProc(hWnd, uMsg, wParam, lParam);
                        }

                        GetClientRect(hWnd, out NATIVE_RECT rect);
                        FillRect(wParam, ref rect, _notSoDarkBrush);
                        return (nint)1;
                    }

                case WM_CTLCOLORDLG:
                    // Window background colour when the expander resizes upward (Win10)
                    return _darkBrush;

                case MY_APPLY_THEME_MSG:
                    {
                        try
                        {
                            ApplyThemeToWindow(hWnd, isThemeSwitch: false);
                        }
                        catch (Exception ex)
                        {
                            Log("MY_APPLY_THEME_MSG ApplyThemeToWindow failed: {0}", ex);
                        }
                        // If DefSubclassProc isn't available fallback to DefWindowProc
                        if (_hasDefSubclassProcApi)
                            return DefSubclassProc(hWnd, uMsg, wParam, lParam);
                        else
                            return DefWindowProc(hWnd, uMsg, wParam, lParam);
                    }
            }

            return DefSubclassProc(hWnd, uMsg, wParam, lParam);
        }
        catch (Exception ex)
        {
            Log("Unhandled exception in SubclassProcImpl for hwnd={0}, msg={1}: {2}\n{3}", hWnd, uMsg, ex, ex.StackTrace);
            return DefSubclassProc(hWnd, uMsg, wParam, lParam);
        }
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static string GetWindowClass(nint hwnd)
    {
        var sb = new StringBuilder(256);
        GetClassName(hwnd, sb, 256);
        return sb.ToString();
    }

    private static readonly string _logFile = Path.Combine(Path.GetTempPath(), "taskdialog-log.txt");

    private static void Log(string format, params object[]? args)
    {
        try
        {
            args ??= [];
#if DEBUG
            string line = DateTime.Now.ToString("o") + " [" + Thread.CurrentThread.ManagedThreadId + "] " + string.Format(format, args);
            File.AppendAllText(_logFile, line + Environment.NewLine);
#else
            // ...
#endif
        }
        catch
        {
            // Swallow logging errors
        }
    }

    // -------------------------------------------------------------------------
    // Native types (Pack=1 matches Windows SDK layout for TASKDIALOGCONFIG)
    // -------------------------------------------------------------------------

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
    private struct NativeTaskDialogConfig
    {
        public uint cbSize;
        public nint hwndParent;
        public nint hInstance;
        public uint dwFlags;
        public uint dwCommonButtons;
        public nint pszWindowTitle;
        public nint mainIconUnion;        // union: HICON or PCWSTR
        public nint pszMainInstruction;
        public nint pszContent;
        public uint cButtons;
        public nint pButtons;
        public int nDefaultButton;
        public uint cRadioButtons;
        public nint pRadioButtons;
        public int nDefaultRadioButton;
        public nint pszVerificationText;
        public nint pszExpandedInformation;
        public nint pszExpandedControlText;
        public nint pszCollapsedControlText;
        public nint footerIconUnion;      // union: HICON or PCWSTR
        public nint pszFooter;
        public nint pfCallback;
        public nint lpCallbackData;
        public uint cxWidth;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
    private struct NativeTaskDialogButton
    {
        public int nButtonID;
        public nint pszButtonText;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NATIVE_RECT
    {
        public int Left, Top, Right, Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NATIVE_POINT
    {
        public int X, Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NATIVE_WINDOWPLACEMENT
    {
        public uint length;
        public uint flags;
        public uint showCmd;
        public NATIVE_POINT ptMinPosition;
        public NATIVE_POINT ptMaxPosition;
        public NATIVE_RECT rcNormalPosition;
    }

    // -------------------------------------------------------------------------
    // Private delegates
    // -------------------------------------------------------------------------

    private delegate int TaskDialogCallbackProc(nint hwnd, uint msg, nint wParam, nint lParam, nint lpRefData);

    private delegate nint SubclassProcDelegate(nint hWnd, uint uMsg, nint wParam, nint lParam, nint uIdSubclass, nint dwRefData);

    private delegate bool EnumChildProcDelegate(nint hwnd, nint lParam);

    // -------------------------------------------------------------------------
    // P/Invoke – comctl32
    // -------------------------------------------------------------------------

    // TaskDialogIndirect exists only in comctl32 v6.  WPF itself loads v6, so we
    // locate it via GetProcAddress on the already-loaded module rather than relying
    // on the OS loader picking up v5 through a plain DllImport.
    private delegate int TaskDialogIndirectDelegate(
        ref NativeTaskDialogConfig pTaskConfig,
        out int pnButton,
        out int pnRadioButton,
        out int pfVerificationFlagChecked);

    private static TaskDialogIndirectDelegate? _taskDialogIndirectDelegate;

    private static int TaskDialogIndirect(
        ref NativeTaskDialogConfig pTaskConfig,
        out int pnButton,
        out int pnRadioButton,
        out int pfVerificationFlagChecked)
    {
        if (_taskDialogIndirectDelegate is null)
        {
            // WPF loads comctl32 v6 via its own activation context.  Use
            // GetModuleHandle first (it is already mapped into the process);
            // fall back to LoadLibrary so the function also works in non-WPF hosts.
            nint hMod = GetModuleHandle("comctl32.dll");
            if (hMod == IntPtr.Zero)
                hMod = LoadLibrary("comctl32.dll");
            if (hMod == IntPtr.Zero)
                throw new DllNotFoundException("comctl32.dll");

            nint proc = GetProcAddress(hMod, "TaskDialogIndirect");
            if (proc == IntPtr.Zero)
                throw new EntryPointNotFoundException(
                    "TaskDialogIndirect was not found in comctl32.dll. "
                    + "Ensure the application manifest references Common Controls v6.");

            _taskDialogIndirectDelegate =
                Marshal.GetDelegateForFunctionPointer<TaskDialogIndirectDelegate>(proc);
        }
        return _taskDialogIndirectDelegate(
            ref pTaskConfig, out pnButton, out pnRadioButton, out pfVerificationFlagChecked);
    }

    [DllImport("comctl32.dll")]
    private static extern bool SetWindowSubclass(
        nint hWnd, SubclassProcDelegate pfnSubclass,
        nint uIdSubclass, nint dwRefData);

    [DllImport("comctl32.dll")]
    private static extern nint DefSubclassProc(
        nint hWnd, uint uMsg, nint wParam, nint lParam);

    [DllImport("user32.dll")]
    private static extern nint DefWindowProc(nint hWnd, uint Msg, nint wParam, nint lParam);

    [DllImport("comctl32.dll")]
    private static extern bool RemoveWindowSubclass(
        nint hWnd, SubclassProcDelegate pfnSubclass, nint uIdSubclass);

    [DllImport("comctl32.dll")]
    private static extern bool GetWindowSubclass(
        nint hWnd, SubclassProcDelegate pfnSubclass,
        nint uIdSubclass, out nint pdwRefData);

    // -------------------------------------------------------------------------
    // P/Invoke – dwmapi
    // -------------------------------------------------------------------------

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(
        nint hwnd, int dwAttribute, ref int pvAttribute, int cbAttribute);

    // -------------------------------------------------------------------------
    // P/Invoke – uxtheme
    // -------------------------------------------------------------------------

    [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
    private static extern int SetWindowTheme(nint hwnd, string? pszSubAppName, string? pszSubIdList);

    private static void TrySetWindowTheme(nint hwnd, bool isDark)
    {
        if (!_hasSetWindowThemeApi) return;
        try
        {
            if (isDark)
            {
                int r = SetWindowTheme(hwnd, "DarkMode_Explorer", null);
                if (r != 0)
                {
                    // Fallback to Explorer if DarkMode_Explorer isn't supported
                    SetWindowTheme(hwnd, "Explorer", null);
                }
            }
            else
            {
                SetWindowTheme(hwnd, null, null);
            }
        }
        catch (EntryPointNotFoundException)
        {
            _hasSetWindowThemeApi = false;
        }
        catch (Exception ex)
        {
            Log("TrySetWindowTheme failed for hwnd={0}: {1}", hwnd, ex);
        }
    }

    // -------------------------------------------------------------------------
    // P/Invoke – user32
    // -------------------------------------------------------------------------

    [DllImport("user32.dll")]
    private static extern bool EnumChildWindows(
        nint hwndParent, EnumChildProcDelegate lpEnumFunc, nint lParam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetClassName(nint hWnd, StringBuilder lpClassName, int nMaxCount);

    [DllImport("user32.dll")]
    private static extern bool GetClientRect(nint hWnd, out NATIVE_RECT lpRect);

    [DllImport("user32.dll")]
    private static extern bool FillRect(nint hDC, ref NATIVE_RECT lprc, nint hbr);

    [DllImport("gdi32.dll")]
    private static extern uint SetTextColor(nint hdc, uint crColor);

    [DllImport("user32.dll")]
    private static extern nint GetParent(nint hWnd);

    [DllImport("user32.dll")]
    private static extern bool GetWindowPlacement(nint hWnd, out NATIVE_WINDOWPLACEMENT lpwndpl);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPlacement(nint hWnd, ref NATIVE_WINDOWPLACEMENT lpwndpl);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern nint SendMessage(nint hWnd, uint Msg, nint wParam, nint lParam);

    [DllImport("user32.dll")]
    private static extern bool PostMessage(nint hWnd, uint Msg, nint wParam, nint lParam);

    [DllImport("user32.dll")]
    private static extern bool RedrawWindow(nint hWnd, nint lprcUpdate, nint hrgnUpdate, uint flags);

    // -------------------------------------------------------------------------
    // P/Invoke – kernel32 (for dynamic comctl32 v6 loading)
    // -------------------------------------------------------------------------

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern nint GetModuleHandle(string lpModuleName);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern nint LoadLibrary(string lpFileName);

    [DllImport("kernel32.dll", CharSet = CharSet.Ansi, BestFitMapping = false)]
    private static extern nint GetProcAddress(nint hModule, string procName);

    // -------------------------------------------------------------------------
    // P/Invoke – gdi32
    // -------------------------------------------------------------------------

    [DllImport("gdi32.dll")]
    private static extern nint CreateSolidBrush(uint crColor);

    // -------------------------------------------------------------------------
    // Helper class
    // -------------------------------------------------------------------------

    private sealed class CallbackWrapper
    {
        public TaskDialogCallbackDelegate? UserCallback;
        public object? UserData;

        /// <summary>Set by TDN_DESTROYED to avoid double-free in ShowIndirect finally.</summary>
        public bool HandleFreedByCallback;
    }
}

/// <summary>
/// Callback raised for TaskDialog notifications.
/// </summary>
/// <returns>Returning a non-zero HRESULT will affect dialog behaviour for some notifications.</returns>
public delegate int TaskDialogCallbackDelegate(
    nint hwnd,
    TaskDialogNotification notification,
    nint wParam,
    nint lParam,
    object? data);

/// <summary>TaskDialog notification identifiers (TDN_*).</summary>
public enum TaskDialogNotification : uint
{
    Created = 0,
    NavigatedPage = 1,
    ButtonClicked = 2,
    HyperlinkClicked = 3,
    Timer = 4,
    Destroyed = 5,
    RadioButtonClicked = 6,
    DialogConstructed = 7,
    VerificationClicked = 8,
    Help = 9,
    ExpandoButtonClicked = 10,
}

/// <summary>TASKDIALOG_FLAGS subset.</summary>
[Flags]
public enum TaskDialogFlags : uint
{
    None = 0x0000,
    EnableHyperlinks = 0x0001,
    UseHiconMain = 0x0002,
    UseHiconFooter = 0x0004,
    AllowDialogCancellation = 0x0008,
    UseCommandLinks = 0x0010,
    UseCommandLinksNoIcon = 0x0020,
    ExpandFooterArea = 0x0040,
    ExpandedByDefault = 0x0080,
    VerificationFlagChecked = 0x0100,
    ShowProgressBar = 0x0200,
    ShowMarqueeProgressBar = 0x0400,
    CallbackTimer = 0x0800,
    PositionRelativeToWindow = 0x1000,
    RtlLayout = 0x2000,
    NoDefaultRadioButton = 0x4000,
    CanBeMinimized = 0x8000,
    NoSetForeground = 0x00010000,
    SizeToContent = 0x01000000,
}

/// <summary>TASKDIALOG_COMMON_BUTTON_FLAGS subset.</summary>
[Flags]
public enum TaskDialogCommonButton : uint
{
    None = 0x0000,
    OK = 0x0001,
    Yes = 0x0002,
    No = 0x0004,
    Cancel = 0x0008,
    Retry = 0x0010,
    Close = 0x0020,
}

/// <summary>Describes a custom button or radio button.</summary>
public sealed class TaskDialogButtonSpec
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;

    public TaskDialogButtonSpec(int id, string text)
    {
        Id = id; Text = text;
    }
}

/// <summary>Managed configuration equivalent to TASKDIALOGCONFIG.</summary>
public sealed class TaskDialogConfig
{
    public nint ParentWindow { get; set; }
    public TaskDialogFlags Flags { get; set; }
    public TaskDialogCommonButton CommonButtons { get; set; }
    public string? Title { get; set; }

    /// <summary>
    /// Standard icon: use <see cref="TaskDialog.IconWarning"/>,
    /// <see cref="TaskDialog.IconError"/> etc., or an HICON handle when
    /// <see cref="TaskDialogFlags.UseHiconMain"/> is set.
    /// </summary>
    public nint MainIcon { get; set; }

    public string? MainInstruction { get; set; }
    public string? Content { get; set; }
    public IList<TaskDialogButtonSpec>? Buttons { get; set; }
    public int DefaultButton { get; set; }
    public IList<TaskDialogButtonSpec>? RadioButtons { get; set; }
    public int DefaultRadioButton { get; set; }
    public string? VerificationText { get; set; }
    public string? ExpandedInformation { get; set; }
    public string? ExpandedControlText { get; set; }
    public string? CollapsedControlText { get; set; }

    /// <summary>See <see cref="MainIcon"/> for icon value conventions.</summary>
    public nint FooterIcon { get; set; }

    public string? Footer { get; set; }
    public TaskDialogCallbackDelegate? Callback { get; set; }
    public object? CallbackData { get; set; }
    public uint Width { get; set; }
}
