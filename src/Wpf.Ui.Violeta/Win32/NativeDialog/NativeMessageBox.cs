using System;
using System.Runtime.InteropServices;

namespace Wpf.Ui.Violeta.Win32;

/// <summary>
/// Provides functionality to display a native message box using the Windows API.
/// </summary>
public static class NativeMessageBox
{
    private const uint MB_DEFBUTTON2 = 0x00000100;
    private const uint MB_DEFBUTTON3 = 0x00000200;

    /// <summary>
    /// Displays a native message box.
    /// </summary>
    /// <param name="owner">The handle of the owner window.</param>
    /// <param name="text">The text to display in the message box.</param>
    /// <param name="caption">The caption of the message box.</param>
    /// <param name="buttons">The buttons to display in the message box.</param>
    /// <param name="icon">The icon to display in the message box.</param>
    /// <param name="defaultResult">The default result of the message box.</param>
    /// <param name="options">The options for the message box.</param>
    /// <returns>The result of the message box.</returns>
    public static NativeMessageBoxResult Show(
        nint owner,
        string text,
        string caption,
        NativeMessageBoxButton buttons,
        NativeMessageBoxImage icon,
        NativeMessageBoxResult defaultResult = NativeMessageBoxResult.None,
        NativeMessageBoxOptions options = NativeMessageBoxOptions.None)
    {
        uint type = (uint)buttons | (uint)icon | (uint)options;
        type |= GetDefaultButtonFlag(buttons, defaultResult);
        return (NativeMessageBoxResult)MessageBoxW(owner, text, caption, type);
    }

    private static uint GetDefaultButtonFlag(NativeMessageBoxButton buttons, NativeMessageBoxResult defaultResult)
    {
        return buttons switch
        {
            NativeMessageBoxButton.OKCancel when defaultResult == NativeMessageBoxResult.Cancel => MB_DEFBUTTON2,
            NativeMessageBoxButton.YesNo when defaultResult == NativeMessageBoxResult.No => MB_DEFBUTTON2,
            NativeMessageBoxButton.YesNoCancel when defaultResult == NativeMessageBoxResult.No => MB_DEFBUTTON2,
            NativeMessageBoxButton.YesNoCancel when defaultResult == NativeMessageBoxResult.Cancel => MB_DEFBUTTON3,
            _ => 0,
        };
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    private static extern int MessageBoxW(nint hWnd, string text, string caption, uint type);
}

/// <summary>
/// Specifies the buttons displayed on a message box.
/// </summary>
public enum NativeMessageBoxButton
{
    /// <summary>
    /// The message box displays an OK button.
    /// </summary>
    OK = 0,

    /// <summary>
    /// The message box displays OK and Cancel buttons.
    /// </summary>
    OKCancel = 1,

    /// <summary>
    /// The message box displays Yes, No, and Cancel buttons.
    /// </summary>
    YesNoCancel = 3,

    /// <summary>
    /// The message box displays Yes and No buttons.
    /// </summary>
    YesNo = 4,
}

/// <summary>
/// Specifies the icon displayed on a message box.
/// </summary>
public enum NativeMessageBoxImage
{
    /// <summary>
    /// No icon is displayed.
    /// </summary>
    None = 0,

    /// <summary>
    /// The message box displays an error icon.
    /// </summary>
    Hand = 0x10,

    /// <summary>
    /// The message box displays an error icon.
    /// </summary>
    Stop = Hand,

    /// <summary>
    /// The message box displays an error icon.
    /// </summary>
    Error = Hand,

    /// <summary>
    /// The message box displays a question icon.
    /// </summary>
    Question = 0x20,

    /// <summary>
    /// The message box displays a warning icon.
    /// </summary>
    Exclamation = 0x30,

    /// <summary>
    /// The message box displays a warning icon.
    /// </summary>
    Warning = Exclamation,

    /// <summary>
    /// The message box displays an information icon.
    /// </summary>
    Asterisk = 0x40,

    /// <summary>
    /// The message box displays an information icon.
    /// </summary>
    Information = Asterisk,
}

/// <summary>
/// Specifies the result of a message box.
/// </summary>
public enum NativeMessageBoxResult
{
    /// <summary>
    /// No result.
    /// </summary>
    None = 0,

    /// <summary>
    /// The OK button.
    /// </summary>
    OK = 1,

    /// <summary>
    /// The Cancel button.
    /// </summary>
    Cancel = 2,

    /// <summary>
    /// The Yes button.
    /// </summary>
    Yes = 6,

    /// <summary>
    /// The No button.
    /// </summary>
    No = 7,
}

/// <summary>
/// Specifies display and association options for a message box.
/// </summary>
[Flags]
public enum NativeMessageBoxOptions
{
    /// <summary>
    /// No options.
    /// </summary>
    None = 0,

    /// <summary>
    /// Displays the message box on the default desktop.
    /// </summary>
    DefaultDesktopOnly = 0x20000,

    /// <summary>
    /// Right-aligns the text.
    /// </summary>
    RightAlign = 0x80000,

    /// <summary>
    /// Displays text in right-to-left reading order.
    /// </summary>
    RtlReading = 0x100000,

    /// <summary>
    /// Displays the message box as a service notification.
    /// </summary>
    ServiceNotification = 0x200000,
}
