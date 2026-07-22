using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;

namespace Wpf.Ui.Violeta.Win32;

internal static class TaskDialogThemeHost
{
    private const int TdlgPrimaryPanel = 1;
    private const int TdlgMainInstructionPane = 2;
    private const int TdlgContentPane = 4;
    private const int TdlgExpInfoPane = 6;
    private const int TdlgSecondaryPanel = 8;
    private const int TdlgExpandoText = 12;
    private const int TdlgExpandoButton = 13;
    private const int TdlgVerificationText = 14;
    private const int TdlgFootnotePane = 15;
    private const int TdlgExpandedFooterArea = 18;
    private const int TdlgRadioButtonPane = 21;

    private const int TdlgebsNormal = 1;
    private const int TdlgebsHover = 2;
    private const int TdlgebsPressed = 3;
    private const int TdlgebsExpandedNormal = 4;
    private const int TdlgebsExpandedHover = 5;
    private const int TdlgebsExpandedPressed = 6;

    private const int BpCheckbox = 3;
    private const int BpRadioButton = 2;
    private const int CbsUncheckedNormal = 1;
    private const int CbsUncheckedHot = 2;
    private const int CbsUncheckedPressed = 3;
    private const int CbsCheckedNormal = 5;
    private const int CbsCheckedHot = 6;
    private const int CbsCheckedPressed = 7;
    private const int RbsUncheckedNormal = 1;

    private const int TmtTextColor = 3803;
    private const int TmtFillColor = 3802;
    private const int TmtContentMargins = 3602;
    private const int TmtFont = 210;
    private const int TsTrue = 1;
    private const int TsDraw = 2;

    private const int DttTextColor = 1;
    private const int DttComposited = 8192;
    private const int DtLeft = 0;
    private const int DtVCenter = 4;
    private const int DtWordBreak = 16;
    private const int DtNoPrefix = 2048;
    private const int DtEndEllipsis = 32768;

    private const int WmEraseBackground = 0x0014;
    private const int WmPaint = 0x000F;
    private const int WmPrintClient = 0x0318;
    private const int WmDestroy = 0x0002;
    private const int WmNcDestroy = 0x0082;
    private const int WmMouseMove = 0x0200;
    private const int WmMouseLeave = 0x02A3;
    private const int WmLButtonDown = 0x0201;
    private const int WmLButtonUp = 0x0202;
    private const int WmThemeChanged = 0x031A;
    private const int WmSettingChange = 0x001A;
    private const int WmSysColorChange = 0x0015;
    private const int WmUser = 0x0400;
    private const int WmCtlColorMsgBox = 0x0132;
    private const int WmCtlColorEdit = 0x0133;
    private const int WmCtlColorListBox = 0x0134;
    private const int WmCtlColorButton = 0x0135;
    private const int WmCtlColorDialog = 0x0136;
    private const int WmCtlColorScrollBar = 0x0137;
    private const int WmCtlColorStatic = 0x0138;
    private const int WmAppApplyTheme = 0x8000 + 0x04B;
    private const int PrfClient = 4;
    private const int PbmSetBkColor = WmUser + 1;
    private const int PbmSetBarColor = WmUser + 9;
    private const int BpbfTopDownDib = 2;
    private const int TmeLeave = 2;
    private const int RdwInvalidate = 1;
    private const int RdwUpdateNow = 256;
    private const int RdwAllChildren = 128;
    private const int Transparent = 1;
    private const int DwmwaUseImmersiveDarkMode = 20;
    private const int DwmwaUseImmersiveDarkModeBefore20H1 = 19;
    private const int SwpNoSize = 1;
    private const int SwpNoMove = 2;
    private const int SwpNoZOrder = 4;
    private const int SwpNoActivate = 16;
    private const int SwpFrameChanged = 32;
    private const int GclpHbrBackground = -10;
    private const int ColorWindow = 5;
    private const int ColorBtnFace = 15;
    private const int StateSystemChecked = 0x10;
    private const int LegacyIAccessibleStatePropertyId = 30090;

    private const int RrfRtRegDword = 0x00000018;
    private static readonly nint HkeyCurrentUser = unchecked((nint)0x80000001);

    private const int UiaButtonControlTypeId = 50000;
    private const int UiaHyperlinkControlTypeId = 50005;
    private const int UiaProgressBarControlTypeId = 50012;
    private const int UiaRadioButtonControlTypeId = 50013;
    private const int UiaScrollBarControlTypeId = 50014;
    private const int UiaPaneControlTypeId = 50033;

    private const int Primary = 0x202020;
    private const int Secondary = 0x2C2C2C;
    private const int Footnote = 0x2C2C2C;
    private const int Separator = 0x4D4D4D;
    private const int TextNormal = 0xFFFFFF;
    private const int TextInstruction = 0xFFEB99;
    private const int TextFootnote = 0xE0E0E0;

    private static readonly object SyncRoot = new();
    private static readonly HashSet<IntPtr> Dialogs = [];
    private static readonly Dictionary<IntPtr, DirectUIState> DirectUIStates = [];
    private static readonly Dictionary<IntPtr, IntPtr> OwnedBrushes = [];
    private static readonly UIntPtr MainSubclassId = new(0xDEADBEEF);
    private static readonly UIntPtr DirectUISubclassId = new(0x0BADF00D);
    private static readonly UIntPtr CtlColorSubclassId = new(0xC0FFEE01);
    private static readonly SubclassProc MainSubclassCallback = TaskDialogMainSubclassProc;
    private static readonly SubclassProc DirectUISubclassCallback = DirectUISubclassProc;
    private static readonly SubclassProc CtlColorSubclassCallback = WmCtlColorSubclassProc;
    private static readonly SubclassProc RadioSubclassCallback = RadioSubclassProc;
    private static readonly EnumChildProc ApplyChildCallback = ApplyChildWindowProc;
    private static readonly EnumChildProc RemoveChildCallback = RemoveChildWindowProc;
    private static readonly IntPtr PrimaryBrush = CreateSolidBrush(Primary);
    private static readonly IntPtr SecondaryBrush = CreateSolidBrush(Secondary);
    private static readonly IntPtr FootnoteBrush = CreateSolidBrush(Footnote);
    private static TaskDialogTheme _theme = TaskDialogTheme.System;
    private static bool _hasNativeTheme;

    internal static void SetTheme(TaskDialogTheme theme)
    {
        IntPtr[] dialogs;
        lock (SyncRoot)
        {
            if (_theme == theme)
                return;

            _theme = theme;
            dialogs = new IntPtr[Dialogs.Count];
            Dialogs.CopyTo(dialogs);
        }

        foreach (IntPtr dialog in dialogs)
        {
            if (dialog != IntPtr.Zero && IsWindow(dialog))
                PostMessage(dialog, WmAppApplyTheme, IntPtr.Zero, IntPtr.Zero);
        }
    }

    internal static void RegisterDialog(IntPtr hwnd)
    {
        if (hwnd == IntPtr.Zero)
            return;

        lock (SyncRoot)
        {
            Dialogs.Add(hwnd);
        }

        AllowForTaskDialog(hwnd);
        PostMessage(hwnd, WmAppApplyTheme, IntPtr.Zero, IntPtr.Zero);
    }

    internal static void UnregisterDialog(IntPtr hwnd)
    {
        if (hwnd == IntPtr.Zero)
            return;

        lock (SyncRoot)
        {
            Dialogs.Remove(hwnd);
        }

        RemoveFromTaskDialog(hwnd);
    }

    internal static void ApplyTheme(IntPtr hwnd)
    {
        if (hwnd != IntPtr.Zero && IsWindow(hwnd))
            AllowForTaskDialog(hwnd);
    }

    private static TaskDialogTheme CurrentTheme
    {
        get
        {
            lock (SyncRoot)
            {
                return _theme;
            }
        }
    }

    private static bool IsDark => ResolveDark(CurrentTheme);

    private static bool ResolveDark(TaskDialogTheme theme)
    {
        if (!TaskDialogNativeMethods.SupportsTaskDialogDarkMode)
            return false;

        return theme switch
        {
            TaskDialogTheme.Dark => true,
            TaskDialogTheme.Light => false,
            _ => ReadOsDarkMode(),
        };
    }

    /// <summary>
    /// Reads the documented registry value written by Settings / Explorer.
    /// Returns <see langword="true"/> when the OS apps-use-dark-mode preference is on.
    /// </summary>
    private static bool ReadOsDarkMode()
    {
        int value = 1;
        int size = sizeof(int);
        _ = RegGetValue(
            HkeyCurrentUser,
            @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize",
            "AppsUseLightTheme",
            RrfRtRegDword,
            IntPtr.Zero,
            ref value,
            ref size);
        return value == 0;
    }

    private static void AllowForTaskDialog(nint hwnd)
    {
        _hasNativeTheme = IsDarkThemeActive("DarkMode_Explorer::TaskDialog", "TaskDialog")
            || IsDarkThemeActive("DarkMode_DarkTheme::TaskDialog", "TaskDialog");

        if (!IsDark)
        {
            RemoveDarkTheme(hwnd, keepMainSubclass: true);
            SetImmersiveDarkMode(hwnd, false);
            return;
        }

        EnumChildWindows(hwnd, ApplyChildCallback, hwnd);
        EnumChildWindows(hwnd, ApplyDeepChildCallback, IntPtr.Zero);
        if (_hasNativeTheme)
            _ = SetWindowTheme(hwnd, "DarkMode_Explorer", null!);

        SetImmersiveDarkMode(hwnd, true);
        if (!GetWindowSubclass(hwnd, MainSubclassCallback, MainSubclassId, out _))
            SetWindowSubclass(hwnd, MainSubclassCallback, MainSubclassId, UIntPtr.Zero);

        EnumChildWindows(hwnd, SendSysColorChangeProc, IntPtr.Zero);
        SendMessage(hwnd, WmThemeChanged, IntPtr.Zero, IntPtr.Zero);
        Redraw(hwnd);
    }

    private static void RemoveFromTaskDialog(nint hwnd)
    {
        RemoveWindowSubclass(hwnd, MainSubclassCallback, MainSubclassId);
        RemoveDarkTheme(hwnd, keepMainSubclass: false);
    }

    private static void RemoveDarkTheme(nint hwnd, bool keepMainSubclass)
    {
        _ = SetWindowTheme(hwnd, null!, null!);
        EnumChildWindows(hwnd, RemoveChildCallback, IntPtr.Zero);
        if (!keepMainSubclass)
            RemoveWindowSubclass(hwnd, MainSubclassCallback, MainSubclassId);
    }

    private static bool ApplyChildWindowProc(nint hwndChild, nint rootDialog)
    {
        using UiaElement element = UiaElement.FromHandle(hwndChild);
        ApplyClassThemeFallback(hwndChild);

        if (element == null)
            return true;

        string className = element.ClassName;
        string automationId = element.AutomationId;

        if (string.Equals(className, "CCSysLink", StringComparison.OrdinalIgnoreCase))
        {
            nint link = element.NativeWindowHandle;
            nint parent = GetParent(link);
            if (parent != IntPtr.Zero)
                ApplyLinkParentBackground(parent, automationId);
            return true;
        }

        if (!string.Equals(className, "TaskDialog", StringComparison.OrdinalIgnoreCase))
            return true;

        nint directUI = element.NativeWindowHandle;
        if (directUI == IntPtr.Zero)
            return true;

        SetClassLongPtr(directUI, GclpHbrBackground, SecondaryBrush);
        ApplyTaskPageChildren(element);
        _ = SetWindowTheme(directUI, "DarkMode_Explorer", null!);

        DirectUIState state = GetState(directUI);
        state.Theme = CurrentTheme;
        state.RefreshElements(directUI);

        if (!GetWindowSubclass(directUI, DirectUISubclassCallback, DirectUISubclassId, out _))
            SetWindowSubclass(directUI, DirectUISubclassCallback, DirectUISubclassId, UIntPtr.Zero);

        return true;
    }

    private static void ApplyTaskPageChildren(UiaElement taskPage)
    {
        ApplyTaskPageChildrenRecursive(taskPage, depth: 0);
    }

    private static void ApplyTaskPageChildrenRecursive(UiaElement taskPage, int depth)
    {
        foreach (UiaElement child in taskPage.Children())
        {
            using (child)
            {
                if (depth < 8)
                    ApplyTaskPageChildrenRecursive(child, depth + 1);

                int controlType = child.ControlType;
                if (controlType != UiaButtonControlTypeId
                    && controlType != UiaRadioButtonControlTypeId
                    && controlType != UiaProgressBarControlTypeId
                    && controlType != UiaHyperlinkControlTypeId
                    && controlType != UiaScrollBarControlTypeId
                    && controlType != UiaPaneControlTypeId)
                    continue;

                nint hwnd = child.NativeWindowHandle;
                if (hwnd == IntPtr.Zero)
                    continue;

                string id = child.AutomationId;
                nint parent = GetParent(hwnd);

                if (controlType == UiaProgressBarControlTypeId)
                {
                    ApplyProgressTheme(hwnd);
                    if (parent != IntPtr.Zero)
                        ApplyPanelParentBackground(parent);
                }
                else if (controlType == UiaRadioButtonControlTypeId
                    || id.StartsWith("RadioButton_", StringComparison.Ordinal)
                    || controlType == UiaHyperlinkControlTypeId)
                {
                    if (controlType == UiaRadioButtonControlTypeId || id.StartsWith("RadioButton_", StringComparison.Ordinal))
                    {
                        _ = SetWindowTheme(hwnd, "DarkMode_Explorer", null!);
                        if (!GetWindowSubclass(hwnd, RadioSubclassCallback, CtlColorSubclassId, out _))
                            SetWindowSubclass(hwnd, RadioSubclassCallback, CtlColorSubclassId, UIntPtr.Zero);
                    }
                    else if (IsDarkThemeActive("DarkMode_DarkTheme::TaskDialog", "TaskDialog"))
                    {
                        _ = SetWindowTheme(hwnd, "DarkMode_DarkTheme", null!);
                    }

                    if (parent != IntPtr.Zero)
                    {
                        if (controlType == UiaHyperlinkControlTypeId)
                            ApplyLinkParentBackground(parent, id);
                        else
                            ApplyPanelParentBackground(parent);
                    }
                }
                else if (id.StartsWith("CommandLink_", StringComparison.Ordinal))
                {
                    _ = SetWindowTheme(hwnd, "DarkMode_Explorer", null!);
                    if (parent != IntPtr.Zero)
                        ApplyPanelParentBackground(parent);
                }
                else if (id.StartsWith("CommandButton_", StringComparison.Ordinal))
                {
                    _ = SetWindowTheme(hwnd, "DarkMode_Explorer", null!);
                    if (parent != IntPtr.Zero)
                        ApplyPanelParentBackground(parent, alwaysSecondary: true);
                }
                else
                {
                    _ = SetWindowTheme(hwnd, "DarkMode_Explorer", null!);
                }
            }
        }
    }

    private static bool RemoveChildWindowProc(nint hwndChild, nint lParam)
    {
        if (GetWindowSubclass(hwndChild, DirectUISubclassCallback, DirectUISubclassId, out _))
        {
            RemoveWindowSubclass(hwndChild, DirectUISubclassCallback, DirectUISubclassId);
            DestroyState(hwndChild);
        }

        if (GetWindowSubclass(hwndChild, RadioSubclassCallback, CtlColorSubclassId, out _))
            RemoveWindowSubclass(hwndChild, RadioSubclassCallback, CtlColorSubclassId);

        if (GetWindowSubclass(hwndChild, CtlColorSubclassCallback, CtlColorSubclassId, out _))
        {
            RemoveWindowSubclass(hwndChild, CtlColorSubclassCallback, CtlColorSubclassId);
            FreeOwnedBrush(hwndChild);
        }

        _ = SetWindowTheme(hwndChild, null!, null!);
        RedrawWindow(hwndChild, IntPtr.Zero, IntPtr.Zero, RdwInvalidate | RdwUpdateNow | RdwAllChildren);
        return true;
    }

    private static bool ApplyDeepChildCallback(nint hwndChild, nint lParam)
    {
        ApplyClassThemeFallback(hwndChild);
        EnumChildWindows(hwndChild, ApplyDeepChildCallback, IntPtr.Zero);
        return true;
    }

    private static void ApplyClassThemeFallback(nint hwnd)
    {
        string className = GetWindowClassName(hwnd);
        if (string.Equals(className, "msctls_progress32", StringComparison.OrdinalIgnoreCase))
        {
            ApplyProgressTheme(hwnd);
        }
        else if (string.Equals(className, "Button", StringComparison.OrdinalIgnoreCase)
            || string.Equals(className, "ScrollBar", StringComparison.OrdinalIgnoreCase)
            || string.Equals(className, "SysLink", StringComparison.OrdinalIgnoreCase)
            || string.Equals(className, "DirectUIHWND", StringComparison.OrdinalIgnoreCase))
        {
            if (!string.Equals(className, "Button", StringComparison.OrdinalIgnoreCase))
                _ = SetWindowTheme(hwnd, "DarkMode_Explorer", null!);
        }
    }

    private static void ApplyProgressTheme(nint hwnd)
    {
        bool hasCopyEngine = IsDarkThemeActive("DarkMode_CopyEngine::Progress", "Progress");
        _ = SetWindowTheme(hwnd, hasCopyEngine
            ? "DarkMode_CopyEngine" : "DarkMode_Explorer", null!);

        if (!_hasNativeTheme)
        {
            SendMessage(hwnd, PbmSetBkColor, IntPtr.Zero, Secondary);
            SendMessage(hwnd, PbmSetBarColor, IntPtr.Zero, 0xC56A00);
        }
    }

    private static bool SendSysColorChangeProc(nint hwndChild, nint lParam)
    {
        SendMessage(hwndChild, WmSysColorChange, IntPtr.Zero, IntPtr.Zero);
        return true;
    }

    private static nint TaskDialogMainSubclassProc(nint hwnd, int message, nint wParam, nint lParam, nuint idSubclass, nuint refData)
    {
        switch (message)
        {
            case WmAppApplyTheme:
                AllowForTaskDialog(hwnd);
                return IntPtr.Zero;

            case WmCtlColorDialog:
                if (IsDark)
                {
                    PrepareDarkDeviceContext(wParam, Secondary);
                    return SecondaryBrush;
                }

                break;

            case WmSettingChange:
                if (CurrentTheme == TaskDialogTheme.System)
                    AllowForTaskDialog(hwnd);
                break;

            case WmDestroy:
            case WmNcDestroy:
                RemoveFromTaskDialog(hwnd);
                break;
        }

        return DefSubclassProc(hwnd, message, wParam, lParam);
    }

    private static nint DirectUISubclassProc(nint hwnd, int message, nint wParam, nint lParam, nuint idSubclass, nuint refData)
    {
        DirectUIState state = GetState(hwnd);
        switch (message)
        {
            case WmEraseBackground:
                return 1;

            case WmPaint:
                PaintDirectUI(hwnd, state);
                return IntPtr.Zero;

            case WmMouseMove:
                if (!state.Tracking)
                {
                    TRACKMOUSEEVENT trackMouse = new()
                    {
                        cbSize = Marshal.SizeOf<TRACKMOUSEEVENT>(),
                        dwFlags = TmeLeave,
                        hwndTrack = hwnd,
                    };
                    TrackMouseEvent(ref trackMouse);
                    state.Tracking = true;
                }

                int hot = state.HitTest(GET_X_LPARAM(lParam), GET_Y_LPARAM(lParam));
                if (hot != state.HotIndex)
                {
                    state.HotIndex = hot;
                    InvalidateRect(hwnd, IntPtr.Zero, false);
                }

                break;

            case WmMouseLeave:
                state.Tracking = false;
                state.Pressing = false;
                if (state.HotIndex != -1)
                {
                    state.HotIndex = -1;
                    InvalidateRect(hwnd, IntPtr.Zero, false);
                }

                break;

            case WmLButtonDown:
                state.Pressing = true;
                state.RefreshElements(hwnd);
                break;

            case WmLButtonUp:
                state.Pressing = false;
                state.RefreshElements(hwnd);
                break;

            case WmDestroy:
            case WmNcDestroy:
                DestroyState(hwnd);
                RemoveWindowSubclass(hwnd, DirectUISubclassCallback, idSubclass);
                break;
        }

        return DefSubclassProc(hwnd, message, wParam, lParam);
    }

    private static nint WmCtlColorSubclassProc(nint hwnd, int message, nint wParam, nint lParam, nuint idSubclass, nuint refData)
    {
        switch (message)
        {
            case WmEraseBackground:
                if (wParam != IntPtr.Zero && GetClientRect(hwnd, out RECT rect))
                {
                    _ = FillRect(wParam, ref rect, refData == UIntPtr.Zero ? SecondaryBrush : ToIntPtr(refData));
                    _ = SetTextColor(wParam, TextNormal);
                    return 1;
                }

                break;

            case WmCtlColorMsgBox:
            case WmCtlColorEdit:
            case WmCtlColorListBox:
            case WmCtlColorButton:
            case WmCtlColorDialog:
            case WmCtlColorScrollBar:
            case WmCtlColorStatic:
                nint brush = refData == UIntPtr.Zero ? SecondaryBrush : ToIntPtr(refData);
                PrepareDarkDeviceContext(wParam, GetBrushColor(brush));
                return brush;

            case WmDestroy:
            case WmNcDestroy:
                FreeOwnedBrush(hwnd);
                RemoveWindowSubclass(hwnd, CtlColorSubclassCallback, idSubclass);
                break;
        }

        return DefSubclassProc(hwnd, message, wParam, lParam);
    }

    private static nint RadioSubclassProc(nint hwnd, int message, nint wParam, nint lParam, nuint idSubclass, nuint refData)
    {
        if (message == WmPaint)
        {
            PaintRadioButton(hwnd);
            return 0;
        }

        if (message == WmDestroy || message == WmNcDestroy)
            RemoveWindowSubclass(hwnd, RadioSubclassCallback, idSubclass);

        return DefSubclassProc(hwnd, message, wParam, lParam);
    }

    private static void PaintDirectUI(nint hwnd, DirectUIState state)
    {
        if (!GetClientRect(hwnd, out RECT rect))
            return;

        nint paintHandle = BeginPaint(hwnd, out PAINTSTRUCT paint);
        try
        {
            if (paintHandle == IntPtr.Zero)
                return;

            nint targetDc = paintHandle;
            nint paintBuffer = BeginBufferedPaint(paintHandle, ref rect, BpbfTopDownDib, IntPtr.Zero, out targetDc);
            if (paintBuffer == IntPtr.Zero)
            {
                DefSubclassProc(hwnd, WmPrintClient, paintHandle, PrfClient);
                return;
            }

            try
            {
                DefSubclassProc(hwnd, WmPrintClient, targetDc, PrfClient);
                if (!_hasNativeTheme)
                    PixelSwapPanels(paintBuffer);

                state.RefreshElements(hwnd);
                PaintTaskDialogElements(hwnd, targetDc, state);
            }
            finally
            {
                _ = EndBufferedPaint(paintBuffer, true);
            }
        }
        finally
        {
            EndPaint(hwnd, ref paint);
        }
    }

    private static void PixelSwapPanels(IntPtr paintBuffer)
    {
        if (GetBufferedPaintBits(paintBuffer, out IntPtr bits, out int rowWidth) != 0 || bits == IntPtr.Zero)
            return;

        if (GetBufferedPaintTargetRect(paintBuffer, out RECT target) != 0)
            return;

        int width = target.Right - target.Left;
        int height = target.Bottom - target.Top;
        int primaryLight = GetThemeFillColor(TdlgPrimaryPanel, 0xFFFFFF);
        int secondaryLight = GetThemeFillColor(TdlgSecondaryPanel, 0xF0F0F0);
        int footnoteLight = GetThemeFillColor(TdlgFootnotePane, 0xF0F0F0);
        unsafe
        {
            RGBQUAD* pixels = (RGBQUAD*)bits;
            for (int y = 0; y < height; y++)
            {
                RGBQUAD* row = pixels + (y * rowWidth);
                for (int x = 0; x < width; x++)
                {
                    int color = ToColorRef(row[x]);
                    if (SameColor(color, primaryLight))
                        SetRgb(ref row[x], Primary);
                    else if (SameColor(color, footnoteLight))
                        SetRgb(ref row[x], Footnote);
                    else if (SameColor(color, secondaryLight) || SameColor(color, 0x808080) || SameColor(color, 0xDFDFDF))
                        SetRgb(ref row[x], SameColor(color, secondaryLight) ? Secondary : Separator);
                }
            }
        }
    }

    private static void PaintTaskDialogElements(nint hwnd, nint hdc, DirectUIState state)
    {
        nint taskDialogTheme = OpenTaskDialogTheme(hwnd);
        nint taskDialogStyleTheme = _hasNativeTheme ? OpenTheme(hwnd, "DarkMode_Explorer::TaskDialog") : IntPtr.Zero;
        if (taskDialogStyleTheme == IntPtr.Zero)
            taskDialogStyleTheme = OpenTheme(hwnd, "TaskDialog");
        nint buttonTheme = OpenTheme(hwnd, "Button");

        try
        {
            foreach (UIAElementInfo element in state.Elements)
            {
                if (element.Rect.IsEmpty)
                    continue;

                bool hot = state.HotIndex >= 0 && state.HotIndex < state.Elements.Count && state.Elements[state.HotIndex] == element;
                bool pressed = hot && state.Pressing;

                if (element.AutomationId == "ExpandoButton" && taskDialogTheme != IntPtr.Zero)
                    PaintExpandoGlyph(hdc, taskDialogTheme, element.Rect, hot, pressed, state.IsExpanded);
                else if (element.AutomationId == "VerificationCheckBox" && buttonTheme != IntPtr.Zero)
                    PaintCheckboxGlyph(hdc, buttonTheme, element.Rect, hot, pressed, state.IsChecked);
            }

            foreach (UIAElementInfo element in state.Elements)
            {
                if (element.Rect.IsEmpty)
                    continue;

                if (element.AutomationId == "MainIcon" || element.AutomationId == "FootnoteIcon")
                    continue;

                PaintTextElement(hdc, taskDialogTheme, taskDialogStyleTheme, buttonTheme, element);
            }
        }
        finally
        {
            if (taskDialogTheme != IntPtr.Zero)
                _ = CloseThemeData(taskDialogTheme);
            if (taskDialogStyleTheme != IntPtr.Zero)
                _ = CloseThemeData(taskDialogStyleTheme);
            if (buttonTheme != IntPtr.Zero)
                _ = CloseThemeData(buttonTheme);
        }
    }

    private static void PaintExpandoGlyph(nint hdc, nint theme, RECT rect, bool hot, bool pressed, bool expanded)
    {
        int state = pressed && expanded ? TdlgebsExpandedPressed :
            pressed ? TdlgebsPressed :
            hot && expanded ? TdlgebsExpandedHover :
            hot ? TdlgebsHover :
            expanded ? TdlgebsExpandedNormal :
            TdlgebsNormal;

        RECT glyph = rect;
        if (GetThemePartSize(theme, hdc, TdlgExpandoButton, TdlgebsNormal, IntPtr.Zero, TsTrue, out SIZE size) == 0)
            glyph.Right = glyph.Left + size.CX + 3;

        if (!_hasNativeTheme)
        {
            _ = FillRect(hdc, ref glyph, SecondaryBrush);
            _ = DrawThemeBackground(theme, hdc, TdlgExpandoButton, state, ref glyph, ref rect);
        }
    }

    private static void PaintCheckboxGlyph(nint hdc, nint buttonTheme, RECT rect, bool hot, bool pressed, bool check)
    {
        int state = pressed && check ? CbsCheckedPressed :
            pressed ? CbsUncheckedPressed :
            hot && check ? CbsCheckedHot :
            hot ? CbsUncheckedHot :
            check ? CbsCheckedNormal :
            CbsUncheckedNormal;

        if (GetThemePartSize(buttonTheme, hdc, BpCheckbox, CbsUncheckedNormal, IntPtr.Zero, TsDraw, out SIZE size) != 0)
            return;

        int margin = (rect.Bottom - rect.Top - size.CY) / 3;
        RECT glyph = new()
        {
            Left = rect.Left + margin + 1,
            Top = rect.Top + margin + 1,
            Right = rect.Left + margin + 1 + size.CX,
            Bottom = rect.Bottom,
        };

        if (!_hasNativeTheme)
        {
            _ = FillRect(hdc, ref glyph, SecondaryBrush);
            _ = DrawThemeBackground(buttonTheme, hdc, BpCheckbox, state, ref glyph, IntPtr.Zero);
        }
    }

    private static void PaintTextElement(nint hdc, nint taskDialogTheme, nint textTheme, nint buttonTheme, UIAElementInfo element)
    {
        RECT rect = element.Rect;
        int part = 0;
        nint background = PrimaryBrush;
        int flags = DtLeft | DtVCenter | DtWordBreak | DtNoPrefix;

        if (element.AutomationId == "MainInstruction")
        {
            part = TdlgMainInstructionPane;
            background = PrimaryBrush;
        }
        else if (element.AutomationId == "ContentText")
        {
            part = TdlgContentPane;
        }
        else if (element.AutomationId == "ExpandedInformationText")
        {
            part = TdlgExpInfoPane;
        }
        else if (element.AutomationId == "ExpandedFooterText")
        {
            part = TdlgExpandedFooterArea;
        }
        else if (element.AutomationId == "FootnoteText")
        {
            part = TdlgFootnotePane;
        }
        else if (element.AutomationId == "ExpandoTextExpanded"
            || element.AutomationId == "ExpandoTextCollapsed")
        {
            part = TdlgExpandoText;
            background = PrimaryBrush;
            flags = DtLeft | DtVCenter | DtNoPrefix;
        }
        else if (element.AutomationId == "ExpandoButton")
        {
            nint expandoTheme = taskDialogTheme != IntPtr.Zero ? taskDialogTheme : textTheme;
            if (expandoTheme != IntPtr.Zero
                && GetThemePartSize(expandoTheme, hdc, TdlgExpandoButton, TdlgebsNormal, IntPtr.Zero, TsTrue, out SIZE size) == 0)
                rect.Left += size.CX + 6;
            rect.Top += 1;
            part = TdlgExpandoText;
            background = PrimaryBrush;
            flags = DtLeft | DtVCenter | DtNoPrefix;
        }
        else if (element.AutomationId == "VerificationText")
        {
            part = TdlgVerificationText;
            background = SecondaryBrush;
            flags = DtLeft | DtVCenter | DtNoPrefix;
        }
        else if (element.AutomationId == "VerificationCheckBox" && buttonTheme != IntPtr.Zero)
        {
            if (GetThemePartSize(buttonTheme, hdc, BpCheckbox, CbsUncheckedNormal, IntPtr.Zero, TsDraw, out SIZE size) == 0)
                rect.Left += size.CX + 7;
            rect.Top += 5;
            part = TdlgVerificationText;
            background = SecondaryBrush;
            flags = DtLeft | DtVCenter | DtNoPrefix;
        }

        if (part == 0 || string.IsNullOrEmpty(element.Name))
            return;

        background = GetTextBackgroundBrush(element.AutomationId, part, background);

        DTTOPTS options = new()
        {
            dwSize = Marshal.SizeOf<DTTOPTS>(),
            dwFlags = DttComposited | DttTextColor,
            crText = GetTextColor(textTheme, part),
        };

        if (!_hasNativeTheme)
            _ = FillRect(hdc, ref rect, background);

        _ = DrawThemeTextEx(textTheme, hdc, part, 0, element.Name, -1, flags, ref rect, ref options);
    }

    private static void PaintRadioButton(nint hwnd)
    {
        if (!GetClientRect(hwnd, out RECT client))
            return;

        nint paintHandle = BeginPaint(hwnd, out PAINTSTRUCT paint);
        if (paintHandle == IntPtr.Zero)
        {
            EndPaint(hwnd, ref paint);
            return;
        }

        nint buttonTheme = OpenTheme(hwnd, "Button");
        nint textTheme = OpenTheme(hwnd, "TaskDialogStyle");
        nint paintBuffer = BeginBufferedPaint(paintHandle, ref client, BpbfTopDownDib, IntPtr.Zero, out nint bufferDc);
        try
        {
            if (paintBuffer == IntPtr.Zero)
                bufferDc = paintHandle;

            DefSubclassProc(hwnd, WmPrintClient, bufferDc, PrfClient);
            string text = GetWindowText(hwnd);
            RECT textRect = client;
            if (buttonTheme != IntPtr.Zero && GetThemePartSize(buttonTheme, bufferDc, BpRadioButton, RbsUncheckedNormal, ref client, TsTrue, out SIZE glyphSize) == 0)
                textRect.Left += glyphSize.CX + 2;

            DTTOPTS options = new()
            {
                dwSize = Marshal.SizeOf<DTTOPTS>(),
                dwFlags = DttComposited | DttTextColor,
                crText = TextNormal,
            };

            if (!_hasNativeTheme)
                _ = FillRect(bufferDc, ref textRect, SecondaryBrush);

            _ = DrawThemeTextEx(textTheme == IntPtr.Zero ? buttonTheme : textTheme, bufferDc, TdlgRadioButtonPane, 0, text, -1, DtLeft | DtVCenter | DtEndEllipsis, ref textRect, ref options);

            if (paintBuffer != IntPtr.Zero)
                _ = EndBufferedPaint(paintBuffer, true);
        }
        finally
        {
            if (paintBuffer == IntPtr.Zero)
            {
                // No buffered paint was active.
            }
            if (buttonTheme != IntPtr.Zero)
                _ = CloseThemeData(buttonTheme);
            if (textTheme != IntPtr.Zero)
                _ = CloseThemeData(textTheme);
            EndPaint(hwnd, ref paint);
        }
    }

    private static DirectUIState GetState(nint hwnd)
    {
        lock (SyncRoot)
        {
            if (!DirectUIStates.TryGetValue(hwnd, out DirectUIState? state))
            {
                state = new DirectUIState();
                DirectUIStates.Add(hwnd, state);
            }

            return state;
        }
    }

    private static void DestroyState(nint hwnd)
    {
        lock (SyncRoot)
        {
            DirectUIStates.Remove(hwnd);
        }
    }

    private static bool IsFootnoteAutomationId(string automationId)
    {
#pragma warning disable CA2249 // Consider using 'string.Contains' instead of 'string.IndexOf'
        return automationId.IndexOf("Footnote", StringComparison.OrdinalIgnoreCase) >= 0
            || automationId.IndexOf("ExpandedFooter", StringComparison.OrdinalIgnoreCase) >= 0;
#pragma warning restore CA2249 // Consider using 'string.Contains' instead of 'string.IndexOf'
    }

    private static int GetLinkParentBackground(string automationId)
    {
        if (string.Equals(automationId, "ContentLink", StringComparison.OrdinalIgnoreCase))
            return _hasNativeTheme ? Footnote : Primary;

        if (IsFootnoteAutomationId(automationId))
            return _hasNativeTheme ? Primary : Footnote;

        return _hasNativeTheme ? Secondary : Primary;
    }

    private static void ApplyPanelParentBackground(IntPtr parent, bool alwaysSecondary = false)
    {
        int background = alwaysSecondary
            ? Secondary
            : _hasNativeTheme ? Secondary : Primary;

        SubclassCtlColorParent(parent, background);
    }

    private static void ApplyLinkParentBackground(IntPtr parent, string automationId)
    {
        SubclassCtlColorParent(parent, GetLinkParentBackground(automationId));
    }

    private static IntPtr GetTextBackgroundBrush(string automationId, int part, IntPtr defaultBackground)
    {
        if (IsFootnoteAutomationId(automationId)
            || part == TdlgFootnotePane
            || part == TdlgExpandedFooterArea)
            return FootnoteBrush;

        if (part == TdlgContentPane
            || part == TdlgExpInfoPane
            || part == TdlgVerificationText
            || part == TdlgExpandoText)
            return SecondaryBrush;

        return defaultBackground;
    }

    private static void SubclassCtlColorParent(IntPtr hwnd, int background)
    {
        if (GetWindowSubclass(hwnd, CtlColorSubclassCallback, CtlColorSubclassId, out _))
            return;

        IntPtr brush = CreateSolidBrush(background);
        lock (SyncRoot)
        {
            OwnedBrushes[hwnd] = brush;
        }

        SetWindowSubclass(hwnd, CtlColorSubclassCallback, CtlColorSubclassId, ToUIntPtr(brush));
    }

    private static void FreeOwnedBrush(IntPtr hwnd)
    {
        IntPtr brush;
        lock (SyncRoot)
        {
            if (!OwnedBrushes.TryGetValue(hwnd, out brush))
                return;
            OwnedBrushes.Remove(hwnd);
        }

        if (brush != IntPtr.Zero)
            DeleteObject(brush);
    }

    private static bool IsDarkThemeActive(string dark, string baseTheme)
    {
        nint darkTheme = OpenThemeData(IntPtr.Zero, dark);
        nint baseThemeHandle = OpenThemeData(IntPtr.Zero, baseTheme);
        bool active = darkTheme != IntPtr.Zero && darkTheme != baseThemeHandle;
        if (darkTheme != IntPtr.Zero)
            _ = CloseThemeData(darkTheme);
        if (baseThemeHandle != IntPtr.Zero)
            _ = CloseThemeData(baseThemeHandle);
        return active;
    }

    private static nint OpenTaskDialogTheme(nint hwnd)
    {
        if (IsDarkThemeActive("DarkMode_DarkTheme::TaskDialog", "TaskDialog"))
            return OpenTheme(hwnd, "DarkMode_DarkTheme::TaskDialog");
        if (_hasNativeTheme)
            return OpenTheme(hwnd, "DarkMode_Explorer::TaskDialog");
        return OpenTheme(hwnd, "TaskDialog");
    }

    private static nint OpenTheme(nint hwnd, string classList)
    {
        uint dpi = GetDpiForWindow(hwnd);
        nint theme = OpenThemeDataForDpi(hwnd, classList, dpi);
        return theme == IntPtr.Zero ? OpenThemeData(hwnd, classList) : theme;
    }

    private static int GetTextColor(nint theme, int part)
    {
        // TaskDialogStyle part 12 (ExpandoText) and several other parts still resolve to
        // light-mode black even when opened via DarkMode_*::TaskDialog.
        return part switch
        {
            TdlgMainInstructionPane => TextInstruction,
            TdlgFootnotePane => TextFootnote,
            TdlgExpandedFooterArea => TextFootnote,
            TdlgExpandoText => TextNormal,
            TdlgVerificationText => TextNormal,
            TdlgExpInfoPane => TextNormal,
            TdlgContentPane => TextNormal,
            _ when _hasNativeTheme
                && theme != IntPtr.Zero
                && GetThemeColor(theme, part, 0, TmtTextColor, out int color) == 0
                && IsReadableOnDarkBackground(color) => color,
            _ => TextNormal,
        };

        static bool IsReadableOnDarkBackground(int color)
        {
            int r = color & 0xFF;
            int g = (color >> 8) & 0xFF;
            int b = (color >> 16) & 0xFF;
            return r + g + b > 0x180;
        }
    }

    private static int GetThemeFillColor(int part, int fallback)
    {
        nint theme = OpenThemeData(IntPtr.Zero, "TaskDialog");
        try
        {
            return theme != IntPtr.Zero && GetThemeColor(theme, part, 0, TmtFillColor, out int color) == 0
                ? color
                : fallback;
        }
        finally
        {
            if (theme != IntPtr.Zero)
                _ = CloseThemeData(theme);
        }
    }

    private static void SetImmersiveDarkMode(nint hwnd, bool enabled)
    {
        int value = enabled ? 1 : 0;
        if (DwmSetWindowAttribute(hwnd, DwmwaUseImmersiveDarkMode, ref value, sizeof(int)) != 0)
            _ = DwmSetWindowAttribute(hwnd, DwmwaUseImmersiveDarkModeBefore20H1, ref value, sizeof(int));

        SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0, SwpNoMove | SwpNoSize | SwpNoZOrder | SwpNoActivate | SwpFrameChanged);
    }

    private static void PrepareDarkDeviceContext(nint hdc, int background)
    {
        if (hdc == IntPtr.Zero)
            return;

        _ = SetTextColor(hdc, TextNormal);
        _ = SetBkColor(hdc, background);
        _ = SetBkMode(hdc, Transparent);
    }

    private static int GetBrushColor(nint brush)
    {
        return brush == PrimaryBrush ? Primary :
            brush == FootnoteBrush ? Footnote :
            Secondary;
    }

    private static void Redraw(nint hwnd)
    {
        InvalidateRect(hwnd, IntPtr.Zero, true);
        UpdateWindow(hwnd);
    }

    private static int GET_X_LPARAM(nint lParam) => unchecked((short)((long)lParam & 0xffff));

    private static int GET_Y_LPARAM(nint lParam) => unchecked((short)(((long)lParam >> 16) & 0xffff));

    private static nint ToIntPtr(nuint value)
    {
        return unchecked((nint)value);
    }

    private static nuint ToUIntPtr(nint value)
    {
        return unchecked((nuint)value);
    }

    private static bool SameColor(int left, int right)
    {
        return (left & 0x00FFFFFF) == (right & 0x00FFFFFF);
    }

    private static int ToColorRef(RGBQUAD value)
    {
        return value.rgbRed | (value.rgbGreen << 8) | (value.rgbBlue << 16);
    }

    private static void SetRgb(ref RGBQUAD value, int color)
    {
        value.rgbRed = (byte)(color & 0xff);
        value.rgbGreen = (byte)((color >> 8) & 0xff);
        value.rgbBlue = (byte)((color >> 16) & 0xff);
        value.rgbReserved = 55;
    }

    private static string GetWindowText(nint hwnd)
    {
        StringBuilder builder = new(512);
        int length = GetWindowText(hwnd, builder, builder.Capacity);
        return length <= 0 ? string.Empty : builder.ToString(0, length);
    }

    private static string GetWindowClassName(nint hwnd)
    {
        StringBuilder className = new(256);
        int length = GetClassName(hwnd, className, className.Capacity);
        return length <= 0 ? string.Empty : className.ToString(0, length);
    }

    private static RECT ToClientRect(nint hwnd, RECT screenRect)
    {
        POINT topLeft = new()
        {
            X = screenRect.Left,
            Y = screenRect.Top,
        };
        POINT bottomRight = new()
        {
            X = screenRect.Right,
            Y = screenRect.Bottom,
        };

        ScreenToClient(hwnd, ref topLeft);
        ScreenToClient(hwnd, ref bottomRight);

        return new RECT
        {
            Left = topLeft.X,
            Top = topLeft.Y,
            Right = bottomRight.X,
            Bottom = bottomRight.Y,
        };
    }

    private sealed class DirectUIState
    {
        public readonly List<UIAElementInfo> Elements = [];
        public bool Tracking;
        public bool Pressing;
        public int HotIndex = -1;
        public bool IsExpanded;
        public bool IsChecked;
        public TaskDialogTheme Theme;

        public void RefreshElements(nint hwnd)
        {
            Elements.Clear();
            using UiaElement root = UiaElement.FromHandle(hwnd);
            if (root == null)
                return;

            AddOwnerDrawElements(root, hwnd, depth: 0);

            foreach (UIAElementInfo element in Elements)
            {
                if (element.AutomationId == "VerificationCheckBox")
                    IsChecked = (element.LegacyState & StateSystemChecked) != 0;
            }
        }

        private void AddOwnerDrawElements(UiaElement parent, IntPtr hwnd, int depth)
        {
            if (depth > 8)
                return;

            foreach (UiaElement child in parent.Children())
            {
                using (child)
                {
                    UIAElementInfo info = new(child.AutomationId, child.Name, ToClientRect(hwnd, child.BoundingRectangle), child.LegacyState);
                    if (!info.Rect.IsEmpty && !string.IsNullOrEmpty(info.AutomationId))
                    {
                        if (info.AutomationId == "ExpandoButton")
                        {
#pragma warning disable CA2249 // Consider using 'string.Contains' instead of 'string.IndexOf'
                            IsExpanded = info.Name.IndexOf("hide", StringComparison.OrdinalIgnoreCase) >= 0
                                || info.Name.IndexOf("collapse", StringComparison.OrdinalIgnoreCase) >= 0;
#pragma warning restore CA2249 // Consider using 'string.Contains' instead of 'string.IndexOf'
                        }

                        if (ShouldOwnerDraw(info.AutomationId) && !ContainsElement(info))
                            Elements.Add(info);
                    }

                    AddOwnerDrawElements(child, hwnd, depth + 1);
                }
            }
        }

        private bool ContainsElement(UIAElementInfo info)
        {
            foreach (UIAElementInfo existing in Elements)
            {
                if (existing.AutomationId == info.AutomationId
                    && existing.Rect.Left == info.Rect.Left
                    && existing.Rect.Top == info.Rect.Top
                    && existing.Rect.Right == info.Rect.Right
                    && existing.Rect.Bottom == info.Rect.Bottom)
                    return true;
            }

            return false;
        }

        public int HitTest(int x, int y)
        {
            for (int index = 0; index < Elements.Count; index++)
            {
                if (Elements[index].Rect.Contains(x, y))
                    return index;
            }

            return -1;
        }

        private static bool ShouldOwnerDraw(string id)
        {
            return id == "MainIcon"
                || id == "MainInstruction"
                || id == "ContentText"
                || id == "ExpandedInformationText"
                || id == "ExpandedFooterText"
                || id == "ExpandoButton"
                || id == "ExpandoTextExpanded"
                || id == "ExpandoTextCollapsed"
                || id == "VerificationCheckBox"
                || id == "VerificationText"
                || id == "FootnoteText"
                || id == "FootnoteIcon"
                || id.StartsWith("RadioButton_", StringComparison.Ordinal)
                || id.StartsWith("CommandLink_", StringComparison.Ordinal)
                || id.StartsWith("CommandButton_", StringComparison.Ordinal);
        }
    }

    private sealed class UiaElement : IDisposable
    {
        private static readonly Guid CUIAutomationClsid = new("ff48dba4-60ef-4201-aa87-54103eef594e");
        private readonly IUIAutomationElement _element;

        private UiaElement(IUIAutomationElement element)
        {
            _element = element;
        }

        public string AutomationId => GetString(_element.get_CurrentAutomationId);

        public string ClassName => GetString(_element.get_CurrentClassName);

        public string Name => GetString(_element.get_CurrentName);

        public int ControlType => _element.get_CurrentControlType(out int value) == 0 ? value : 0;

        public int LegacyState
        {
            get
            {
                try
                {
                    return _element.GetCurrentPropertyValue(LegacyIAccessibleStatePropertyId, out object value) == 0 && value is int state
                        ? state
                        : 0;
                }
                catch (Exception)
                {
                    return 0;
                }
            }
        }

        public nint NativeWindowHandle => _element.get_CurrentNativeWindowHandle(out nint value) == 0 ? value : IntPtr.Zero;

        public RECT BoundingRectangle => _element.get_CurrentBoundingRectangle(out RECT value) == 0 ? value : default;

        public static UiaElement FromHandle(nint hwnd)
        {
            if (hwnd == IntPtr.Zero)
                return null!;

            IUIAutomation automation = CreateAutomation();
            if (automation == null)
                return null!;

            try
            {
                return automation.ElementFromHandle(hwnd, out IUIAutomationElement element) == 0 && element != null
                    ? new UiaElement(element)
                    : null!;
            }
            finally
            {
                Marshal.ReleaseComObject(automation);
            }
        }

        public IEnumerable<UiaElement> Children()
        {
            IUIAutomation automation = CreateAutomation();
            if (automation == null)
                yield break;

            IUIAutomationTreeWalker walker = null!;
            IUIAutomationElement child = null!;
            try
            {
                if (automation.get_RawViewWalker(out walker) != 0 || walker == null)
                    yield break;

                if (walker.GetFirstChildElement(_element, out child) != 0 || child == null)
                    yield break;

                while (child != null)
                {
                    IUIAutomationElement current = child;
                    child = null!;
                    walker.GetNextSiblingElement(current, out child);
                    yield return new UiaElement(current);
                }
            }
            finally
            {
                if (child != null)
                    Marshal.ReleaseComObject(child);
                if (walker != null)
                    Marshal.ReleaseComObject(walker);
                Marshal.ReleaseComObject(automation);
            }
        }

        public void Dispose()
        {
            if (_element != null)
                Marshal.ReleaseComObject(_element);
        }

        private static IUIAutomation CreateAutomation()
        {
            try
            {
                Type? type = Type.GetTypeFromCLSID(CUIAutomationClsid);
                return type == null ? null! : (IUIAutomation)Activator.CreateInstance(type)!;
            }
            catch (Exception)
            {
                return null!;
            }
        }

        private delegate int StringGetter(out string value);

        private static string GetString(StringGetter getter)
        {
            return getter(out string value) == 0 ? value ?? string.Empty : string.Empty;
        }
    }

    private sealed class UIAElementInfo(string automationId, string name, TaskDialogThemeHost.RECT rect, int legacyState)
    {
        public string AutomationId { get; } = automationId;

        public string Name { get; } = name;

        public RECT Rect { get; } = rect;

        public int LegacyState { get; } = legacyState;
    }

    [SuppressMessage("Style", "IDE1006:Naming Styles")]
    [SuppressMessage("Interoperability", "SYSLIB1096:Convert to 'GeneratedComInterface'")]
    [ComImport, Guid("30CBE57D-D9D0-452A-AB13-7AC5AC4825EE"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IUIAutomation
    {
        [PreserveSig] int CompareElements(nint el1, nint el2, out bool areSame);

        [PreserveSig] int CompareRuntimeIds(nint runtimeId1, nint runtimeId2, out bool areSame);

        [PreserveSig] int GetRootElement(out IUIAutomationElement root);

        [PreserveSig] int ElementFromHandle(nint hwnd, out IUIAutomationElement element);

        [PreserveSig] int ElementFromPoint(POINT pt, out IUIAutomationElement element);

        [PreserveSig] int GetFocusedElement(out IUIAutomationElement element);

        [PreserveSig] int GetRootElementBuildCache(nint cacheRequest, out IUIAutomationElement root);

        [PreserveSig] int ElementFromHandleBuildCache(nint hwnd, nint cacheRequest, out IUIAutomationElement element);

        [PreserveSig] int ElementFromPointBuildCache(POINT pt, nint cacheRequest, out IUIAutomationElement element);

        [PreserveSig] int GetFocusedElementBuildCache(nint cacheRequest, out IUIAutomationElement element);

        [PreserveSig] int CreateTreeWalker(nint condition, out IUIAutomationTreeWalker walker);

        [PreserveSig] int get_ControlViewWalker(out IUIAutomationTreeWalker walker);

        [PreserveSig] int get_ContentViewWalker(out IUIAutomationTreeWalker walker);

        [PreserveSig] int get_RawViewWalker(out IUIAutomationTreeWalker walker);
    }

    [SuppressMessage("Style", "IDE1006:Naming Styles")]
    [SuppressMessage("Interoperability", "SYSLIB1096:Convert to 'GeneratedComInterface'")]
    [ComImport, Guid("D22108AA-8AC5-49A5-837B-37BBB3D7591E"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IUIAutomationElement
    {
        [PreserveSig] int SetFocus();

        [PreserveSig] int GetRuntimeId(out nint runtimeId);

        [PreserveSig] int FindFirst(int scope, nint condition, out IUIAutomationElement found);

        [PreserveSig] int FindAll(int scope, nint condition, out nint found);

        [PreserveSig] int FindFirstBuildCache(int scope, nint condition, nint cacheRequest, out IUIAutomationElement found);

        [PreserveSig] int FindAllBuildCache(int scope, nint condition, nint cacheRequest, out nint found);

        [PreserveSig] int BuildUpdatedCache(nint cacheRequest, out IUIAutomationElement updatedElement);

        [PreserveSig] int GetCurrentPropertyValue(int propertyId, [MarshalAs(UnmanagedType.Struct)] out object retVal);

        [PreserveSig] int GetCurrentPropertyValueEx(int propertyId, bool ignoreDefaultValue, [MarshalAs(UnmanagedType.Struct)] out object retVal);

        [PreserveSig] int GetCachedPropertyValue(int propertyId, [MarshalAs(UnmanagedType.Struct)] out object retVal);

        [PreserveSig] int GetCachedPropertyValueEx(int propertyId, bool ignoreDefaultValue, [MarshalAs(UnmanagedType.Struct)] out object retVal);

        [PreserveSig] int GetCurrentPatternAs(int patternId, ref Guid riid, out nint patternObject);

        [PreserveSig] int GetCachedPatternAs(int patternId, ref Guid riid, out nint patternObject);

        [PreserveSig] int GetCurrentPattern(int patternId, out nint patternObject);

        [PreserveSig] int GetCachedPattern(int patternId, out nint patternObject);

        [PreserveSig] int GetCachedParent(out IUIAutomationElement parent);

        [PreserveSig] int GetCachedChildren(out nint children);

        [PreserveSig] int get_CurrentProcessId(out int retVal);

        [PreserveSig] int get_CurrentControlType(out int retVal);

        [PreserveSig] int get_CurrentLocalizedControlType([MarshalAs(UnmanagedType.BStr)] out string retVal);

        [PreserveSig] int get_CurrentName([MarshalAs(UnmanagedType.BStr)] out string retVal);

        [PreserveSig] int get_CurrentAcceleratorKey([MarshalAs(UnmanagedType.BStr)] out string retVal);

        [PreserveSig] int get_CurrentAccessKey([MarshalAs(UnmanagedType.BStr)] out string retVal);

        [PreserveSig] int get_CurrentHasKeyboardFocus(out bool retVal);

        [PreserveSig] int get_CurrentIsKeyboardFocusable(out bool retVal);

        [PreserveSig] int get_CurrentIsEnabled(out bool retVal);

        [PreserveSig] int get_CurrentAutomationId([MarshalAs(UnmanagedType.BStr)] out string retVal);

        [PreserveSig] int get_CurrentClassName([MarshalAs(UnmanagedType.BStr)] out string retVal);

        [PreserveSig] int get_CurrentHelpText([MarshalAs(UnmanagedType.BStr)] out string retVal);

        [PreserveSig] int get_CurrentCulture(out int retVal);

        [PreserveSig] int get_CurrentIsControlElement(out bool retVal);

        [PreserveSig] int get_CurrentIsContentElement(out bool retVal);

        [PreserveSig] int get_CurrentIsPassword(out bool retVal);

        [PreserveSig] int get_CurrentNativeWindowHandle(out nint retVal);

        [PreserveSig] int get_CurrentItemType([MarshalAs(UnmanagedType.BStr)] out string retVal);

        [PreserveSig] int get_CurrentIsOffscreen(out bool retVal);

        [PreserveSig] int get_CurrentOrientation(out int retVal);

        [PreserveSig] int get_CurrentFrameworkId([MarshalAs(UnmanagedType.BStr)] out string retVal);

        [PreserveSig] int get_CurrentIsRequiredForForm(out bool retVal);

        [PreserveSig] int get_CurrentItemStatus([MarshalAs(UnmanagedType.BStr)] out string retVal);

        [PreserveSig] int get_CurrentBoundingRectangle(out RECT retVal);
    }

    [SuppressMessage("Interoperability", "SYSLIB1096:Convert to 'GeneratedComInterface'")]
    [ComImport, Guid("4042C624-389C-4AFC-A630-9DF854A541FC"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IUIAutomationTreeWalker
    {
        [PreserveSig] int GetParentElement(IUIAutomationElement element, out IUIAutomationElement parent);

        [PreserveSig] int GetFirstChildElement(IUIAutomationElement element, out IUIAutomationElement first);

        [PreserveSig] int GetLastChildElement(IUIAutomationElement element, out IUIAutomationElement last);

        [PreserveSig] int GetNextSiblingElement(IUIAutomationElement element, out IUIAutomationElement next);
    }

    private delegate nint SubclassProc(nint hwnd, int message, nint wParam, nint lParam, nuint idSubclass, nuint refData);

    private delegate bool EnumChildProc(nint hwnd, nint lParam);

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct SIZE
    {
        public int CX;
        public int CY;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public readonly bool IsEmpty => Right <= Left || Bottom <= Top;

        public readonly bool Contains(int x, int y)
        {
            return x >= Left && x < Right && y >= Top && y < Bottom;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RGBQUAD
    {
        public byte rgbBlue;
        public byte rgbGreen;
        public byte rgbRed;
        public byte rgbReserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct TRACKMOUSEEVENT
    {
        public int cbSize;
        public int dwFlags;
        public nint hwndTrack;
        public int dwHoverTime;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct PAINTSTRUCT
    {
        public nint hdc;
        public bool fErase;
        public RECT rcPaint;
        public bool fRestore;
        public bool fIncUpdate;
        public byte rgbReserved1;
        public byte rgbReserved2;
        public byte rgbReserved3;
        public byte rgbReserved4;
        public byte rgbReserved5;
        public byte rgbReserved6;
        public byte rgbReserved7;
        public byte rgbReserved8;
        public byte rgbReserved9;
        public byte rgbReserved10;
        public byte rgbReserved11;
        public byte rgbReserved12;
        public byte rgbReserved13;
        public byte rgbReserved14;
        public byte rgbReserved15;
        public byte rgbReserved16;
        public byte rgbReserved17;
        public byte rgbReserved18;
        public byte rgbReserved19;
        public byte rgbReserved20;
        public byte rgbReserved21;
        public byte rgbReserved22;
        public byte rgbReserved23;
        public byte rgbReserved24;
        public byte rgbReserved25;
        public byte rgbReserved26;
        public byte rgbReserved27;
        public byte rgbReserved28;
        public byte rgbReserved29;
        public byte rgbReserved30;
        public byte rgbReserved31;
        public byte rgbReserved32;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct DTTOPTS
    {
        public int dwSize;
        public int dwFlags;
        public int crText;
        public int crBorder;
        public int crShadow;
        public int iTextShadowType;
        public POINT ptShadowOffset;
        public int iBorderSize;
        public int iFontPropId;
        public int iColorPropId;
        public int iStateId;
        public bool fApplyOverlay;
        public int iGlowSize;
        public nint pfnDrawTextCallback;
        public nint lParam;
    }

    [DllImport("comctl32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWindowSubclass(nint hwnd, SubclassProc subclassProc, nuint idSubclass, nuint refData);

    [DllImport("comctl32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetWindowSubclass(nint hwnd, SubclassProc subclassProc, nuint idSubclass, out nuint refData);

    [DllImport("comctl32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool RemoveWindowSubclass(nint hwnd, SubclassProc subclassProc, nuint idSubclass);

    [DllImport("comctl32.dll")]
    private static extern nint DefSubclassProc(nint hwnd, int message, nint wParam, nint lParam);

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(nint hwnd, int attribute, ref int attributeValue, int attributeSize);

    [DllImport("gdi32.dll")]
    private static extern nint CreateSolidBrush(int color);

    [DllImport("gdi32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DeleteObject(nint value);

    [DllImport("gdi32.dll")]
    private static extern int SetTextColor(nint hdc, int color);

    [DllImport("gdi32.dll")]
    private static extern int SetBkColor(nint hdc, int color);

    [DllImport("gdi32.dll")]
    private static extern int SetBkMode(nint hdc, int mode);

    [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
    private static extern int SetWindowTheme(nint hwnd, string subAppName, string subIdList);

    [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
    private static extern nint OpenThemeData(nint hwnd, string classList);

    [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
    private static extern nint OpenThemeDataForDpi(nint hwnd, string classList, uint dpi);

    [DllImport("uxtheme.dll")]
    private static extern int CloseThemeData(nint theme);

    [DllImport("uxtheme.dll")]
    private static extern int GetThemeColor(nint theme, int partId, int stateId, int propId, out int color);

    [DllImport("uxtheme.dll")]
    private static extern int GetThemePartSize(nint theme, nint hdc, int partId, int stateId, nint rect, int sizeType, out SIZE size);

    [DllImport("uxtheme.dll")]
    private static extern int GetThemePartSize(nint theme, nint hdc, int partId, int stateId, ref RECT rect, int sizeType, out SIZE size);

    [DllImport("uxtheme.dll")]
    private static extern int DrawThemeBackground(nint theme, nint hdc, int partId, int stateId, ref RECT rect, nint clipRect);

    [DllImport("uxtheme.dll")]
    private static extern int DrawThemeBackground(nint theme, nint hdc, int partId, int stateId, ref RECT rect, ref RECT clipRect);

    [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
    private static extern int DrawThemeTextEx(nint theme, nint hdc, int partId, int stateId, string text, int charCount, int flags, ref RECT rect, ref DTTOPTS options);

    [DllImport("uxtheme.dll")]
    private static extern nint BeginBufferedPaint(nint targetDC, ref RECT targetRect, int format, nint paintParams, out nint paintDC);

    [DllImport("uxtheme.dll")]
    private static extern int EndBufferedPaint(nint bufferedPaint, [MarshalAs(UnmanagedType.Bool)] bool updateTarget);

    [DllImport("uxtheme.dll")]
    private static extern int GetBufferedPaintBits(nint bufferedPaint, out nint bits, out int rowWidth);

    [DllImport("uxtheme.dll")]
    private static extern int GetBufferedPaintTargetRect(nint bufferedPaint, out RECT targetRect);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EnumChildWindows(nint hwndParent, EnumChildProc enumFunc, nint lParam);

    [DllImport("user32.dll")]
    private static extern nint GetParent(nint hwnd);

    [DllImport("user32.dll")]
    private static extern nint SendMessage(nint hwnd, int message, nint wParam, nint lParam);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool PostMessage(nint hwnd, int message, nint wParam, nint lParam);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetClientRect(nint hwnd, out RECT rect);

    [DllImport("user32.dll")]
    private static extern int FillRect(nint hdc, ref RECT rect, nint brush);

    [DllImport("user32.dll")]
    private static extern nint BeginPaint(nint hwnd, out PAINTSTRUCT paint);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EndPaint(nint hwnd, ref PAINTSTRUCT paint);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool InvalidateRect(nint hwnd, nint rect, [MarshalAs(UnmanagedType.Bool)] bool erase);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UpdateWindow(nint hwnd);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool RedrawWindow(nint hwnd, nint rect, nint region, int flags);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsWindow(nint hwnd);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWindowPos(nint hwnd, nint insertAfter, int x, int y, int cx, int cy, int flags);

    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "RegGetValueW")]
    private static extern int RegGetValue(
        nint hkey,
        string lpSubKey,
        string lpValue,
        int dwFlags,
        nint pdwType,
        ref int pvData,
        ref int pcbData);

    [DllImport("user32.dll", EntryPoint = "SetClassLongPtrW")]
    private static extern nint SetClassLongPtr64(nint hwnd, int index, nint newLong);

    [DllImport("user32.dll", EntryPoint = "SetClassLongW")]
    private static extern nint SetClassLong32(nint hwnd, int index, nint newLong);

    [DllImport("user32.dll")]
    private static extern uint GetDpiForWindow(nint hwnd);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool TrackMouseEvent(ref TRACKMOUSEEVENT eventTrack);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(nint hwnd, StringBuilder text, int maxCount);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetClassName(nint hwnd, StringBuilder className, int maxCount);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ScreenToClient(nint hwnd, ref POINT point);

    private static nint SetClassLongPtr(nint hwnd, int index, nint newLong)
    {
        return IntPtr.Size == 8
            ? SetClassLongPtr64(hwnd, index, newLong)
            : SetClassLong32(hwnd, index, newLong);
    }
}
