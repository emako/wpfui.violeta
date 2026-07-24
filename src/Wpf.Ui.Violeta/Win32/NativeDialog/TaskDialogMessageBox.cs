using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using Wpf.Ui.Violeta.Resources;

namespace Wpf.Ui.Violeta.Win32;

/// <summary>
/// Displays a message box using a <see cref="TaskDialog"/>.
/// </summary>
/// <remarks>
/// This class provides a WPF-style message box API without depending on WPF. Owner windows are supplied
/// using a Win32 window handle (<see cref="nint"/>).
/// </remarks>
public sealed class TaskDialogMessageBox : TaskDialog
{
    private const NativeMessageBoxOptions ValidOptions =
        NativeMessageBoxOptions.DefaultDesktopOnly |
        NativeMessageBoxOptions.RightAlign |
        NativeMessageBoxOptions.RtlReading |
        NativeMessageBoxOptions.ServiceNotification;

    private const int IDI_QUESTION = 32514;

    private const int MinimumMessageBoxWidth = 300;
    private const int MaximumMessageBoxWidth = 540;
    private const int MessageBoxWidthStep = 20;
    private const int TargetMessageLineCount = 4;
    private const int AverageCharacterDialogUnits = 4;
    private const int IconDialogWidth = 36;
    private const int ContentMarginDialogWidth = 48;

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskDialogMessageBox"/> class.
    /// </summary>
    public TaskDialogMessageBox()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskDialogMessageBox"/> class with the specified container.
    /// </summary>
    /// <param name="container">The <see cref="IContainer"/> to add the <see cref="TaskDialogMessageBox"/> to.</param>
    public TaskDialogMessageBox(IContainer container)
        : base(container)
    {
    }

    /// <summary>
    /// Gets or sets a value that indicates whether the message box should automatically adjust its width based on the message text.
    /// </summary>
    /// <value>
    /// <see langword="true" /> to automatically adjust the width; otherwise, <see langword="false" /> to let the task dialog calculate
    /// the width. The default value is <see langword="false" />.
    /// </value>
    public static bool AutoAdjustWidth { get; set; }

    /// <summary>
    /// Displays a message box that has a message and returns a result.
    /// </summary>
    /// <param name="messageBoxText">The text to display.</param>
    /// <returns>A <see cref="NativeMessageBoxResult"/> value that specifies which message box button is clicked by the user.</returns>
    public static NativeMessageBoxResult Show(string messageBoxText)
    {
        return ShowCore(0, messageBoxText, string.Empty, NativeMessageBoxButton.OK, NativeMessageBoxImage.None, NativeMessageBoxResult.None, NativeMessageBoxOptions.None);
    }

    /// <summary>
    /// Displays a message box that has a message and title bar caption, and returns a result.
    /// </summary>
    /// <param name="messageBoxText">The text to display.</param>
    /// <param name="caption">The title bar caption to display.</param>
    /// <returns>A <see cref="NativeMessageBoxResult"/> value that specifies which message box button is clicked by the user.</returns>
    public static NativeMessageBoxResult Show(string messageBoxText, string caption)
    {
        return ShowCore(0, messageBoxText, caption, NativeMessageBoxButton.OK, NativeMessageBoxImage.None, NativeMessageBoxResult.None, NativeMessageBoxOptions.None);
    }

    /// <summary>
    /// Displays a message box that has a message, title bar caption, and button; and returns a result.
    /// </summary>
    /// <param name="messageBoxText">The text to display.</param>
    /// <param name="caption">The title bar caption to display.</param>
    /// <param name="button">A value that specifies which button or buttons to display.</param>
    /// <returns>A <see cref="NativeMessageBoxResult"/> value that specifies which message box button is clicked by the user.</returns>
    public static NativeMessageBoxResult Show(string messageBoxText, string caption, NativeMessageBoxButton button)
    {
        return ShowCore(0, messageBoxText, caption, button, NativeMessageBoxImage.None, NativeMessageBoxResult.None, NativeMessageBoxOptions.None);
    }

    /// <summary>
    /// Displays a message box that has a message, title bar caption, button, and icon; and returns a result.
    /// </summary>
    /// <param name="messageBoxText">The text to display.</param>
    /// <param name="caption">The title bar caption to display.</param>
    /// <param name="button">A value that specifies which button or buttons to display.</param>
    /// <param name="icon">A value that specifies the icon to display.</param>
    /// <returns>A <see cref="NativeMessageBoxResult"/> value that specifies which message box button is clicked by the user.</returns>
    public static NativeMessageBoxResult Show(string messageBoxText, string caption, NativeMessageBoxButton button, NativeMessageBoxImage icon)
    {
        return ShowCore(0, messageBoxText, caption, button, icon, NativeMessageBoxResult.None, NativeMessageBoxOptions.None);
    }

    /// <summary>
    /// Displays a message box that has a message, title bar caption, button, icon, and default result; and returns a result.
    /// </summary>
    /// <param name="messageBoxText">The text to display.</param>
    /// <param name="caption">The title bar caption to display.</param>
    /// <param name="button">A value that specifies which button or buttons to display.</param>
    /// <param name="icon">A value that specifies the icon to display.</param>
    /// <param name="defaultResult">A value that specifies the default result.</param>
    /// <returns>A <see cref="NativeMessageBoxResult"/> value that specifies which message box button is clicked by the user.</returns>
    public static NativeMessageBoxResult Show(string messageBoxText, string caption, NativeMessageBoxButton button, NativeMessageBoxImage icon, NativeMessageBoxResult defaultResult)
    {
        return ShowCore(0, messageBoxText, caption, button, icon, defaultResult, NativeMessageBoxOptions.None);
    }

    /// <summary>
    /// Displays a message box that has a message, title bar caption, button, icon, default result, and options; and returns a result.
    /// </summary>
    /// <param name="messageBoxText">The text to display.</param>
    /// <param name="caption">The title bar caption to display.</param>
    /// <param name="button">A value that specifies which button or buttons to display.</param>
    /// <param name="icon">A value that specifies the icon to display.</param>
    /// <param name="defaultResult">A value that specifies the default result.</param>
    /// <param name="options">A value that specifies display options.</param>
    /// <returns>A <see cref="NativeMessageBoxResult"/> value that specifies which message box button is clicked by the user.</returns>
    public static NativeMessageBoxResult Show(string messageBoxText, string caption, NativeMessageBoxButton button, NativeMessageBoxImage icon, NativeMessageBoxResult defaultResult, NativeMessageBoxOptions options)
    {
        return ShowCore(0, messageBoxText, caption, button, icon, defaultResult, options);
    }

    /// <summary>
    /// Displays a message box in front of the specified owner window and returns a result.
    /// </summary>
    /// <param name="owner">The owner window handle.</param>
    /// <param name="messageBoxText">The text to display.</param>
    /// <returns>A <see cref="NativeMessageBoxResult"/> value that specifies which message box button is clicked by the user.</returns>
    public static NativeMessageBoxResult Show(nint owner, string messageBoxText)
    {
        return ShowCore(owner, messageBoxText, string.Empty, NativeMessageBoxButton.OK, NativeMessageBoxImage.None, NativeMessageBoxResult.None, NativeMessageBoxOptions.None);
    }

    /// <summary>
    /// Displays a message box in front of the specified owner window and returns a result.
    /// </summary>
    /// <param name="owner">The owner window handle.</param>
    /// <param name="messageBoxText">The text to display.</param>
    /// <param name="caption">The title bar caption to display.</param>
    /// <returns>A <see cref="NativeMessageBoxResult"/> value that specifies which message box button is clicked by the user.</returns>
    public static NativeMessageBoxResult Show(nint owner, string messageBoxText, string caption)
    {
        return ShowCore(owner, messageBoxText, caption, NativeMessageBoxButton.OK, NativeMessageBoxImage.None, NativeMessageBoxResult.None, NativeMessageBoxOptions.None);
    }

    /// <summary>
    /// Displays a message box in front of the specified owner window and returns a result.
    /// </summary>
    /// <param name="owner">The owner window handle.</param>
    /// <param name="messageBoxText">The text to display.</param>
    /// <param name="caption">The title bar caption to display.</param>
    /// <param name="button">A value that specifies which button or buttons to display.</param>
    /// <returns>A <see cref="NativeMessageBoxResult"/> value that specifies which message box button is clicked by the user.</returns>
    public static NativeMessageBoxResult Show(nint owner, string messageBoxText, string caption, NativeMessageBoxButton button)
    {
        return ShowCore(owner, messageBoxText, caption, button, NativeMessageBoxImage.None, NativeMessageBoxResult.None, NativeMessageBoxOptions.None);
    }

    /// <summary>
    /// Displays a message box in front of the specified owner window and returns a result.
    /// </summary>
    /// <param name="owner">The owner window handle.</param>
    /// <param name="messageBoxText">The text to display.</param>
    /// <param name="caption">The title bar caption to display.</param>
    /// <param name="button">A value that specifies which button or buttons to display.</param>
    /// <param name="icon">A value that specifies the icon to display.</param>
    /// <returns>A <see cref="NativeMessageBoxResult"/> value that specifies which message box button is clicked by the user.</returns>
    public static NativeMessageBoxResult Show(nint owner, string messageBoxText, string caption, NativeMessageBoxButton button, NativeMessageBoxImage icon)
    {
        return ShowCore(owner, messageBoxText, caption, button, icon, NativeMessageBoxResult.None, NativeMessageBoxOptions.None);
    }

    /// <summary>
    /// Displays a message box in front of the specified owner window and returns a result.
    /// </summary>
    /// <param name="owner">The owner window handle.</param>
    /// <param name="messageBoxText">The text to display.</param>
    /// <param name="caption">The title bar caption to display.</param>
    /// <param name="button">A value that specifies which button or buttons to display.</param>
    /// <param name="icon">A value that specifies the icon to display.</param>
    /// <param name="defaultResult">A value that specifies the default result.</param>
    /// <returns>A <see cref="NativeMessageBoxResult"/> value that specifies which message box button is clicked by the user.</returns>
    public static NativeMessageBoxResult Show(nint owner, string messageBoxText, string caption, NativeMessageBoxButton button, NativeMessageBoxImage icon, NativeMessageBoxResult defaultResult)
    {
        return ShowCore(owner, messageBoxText, caption, button, icon, defaultResult, NativeMessageBoxOptions.None);
    }

    /// <summary>
    /// Displays a message box in front of the specified owner window and returns a result.
    /// </summary>
    /// <param name="owner">The owner window handle.</param>
    /// <param name="messageBoxText">The text to display.</param>
    /// <param name="caption">The title bar caption to display.</param>
    /// <param name="button">A value that specifies which button or buttons to display.</param>
    /// <param name="icon">A value that specifies the icon to display.</param>
    /// <param name="defaultResult">A value that specifies the default result.</param>
    /// <param name="options">A value that specifies display options.</param>
    /// <returns>A <see cref="NativeMessageBoxResult"/> value that specifies which message box button is clicked by the user.</returns>
    public static NativeMessageBoxResult Show(nint owner, string messageBoxText, string caption, NativeMessageBoxButton button, NativeMessageBoxImage icon, NativeMessageBoxResult defaultResult, NativeMessageBoxOptions options)
    {
        return ShowCore(owner, messageBoxText, caption, button, icon, defaultResult, options);
    }

    private static NativeMessageBoxResult ShowCore(nint owner, string messageBoxText, string caption, NativeMessageBoxButton button, NativeMessageBoxImage icon, NativeMessageBoxResult defaultResult, NativeMessageBoxOptions options)
    {
        ValidateParameters(owner, button, icon, defaultResult, options);

        if ((options & (NativeMessageBoxOptions.DefaultDesktopOnly | NativeMessageBoxOptions.ServiceNotification)) != 0)
            return NativeMessageBox.Show(owner, messageBoxText ?? string.Empty, caption ?? string.Empty, button, icon, defaultResult, options);

        using TaskDialogMessageBox dialog = new()
        {
            WindowTitle = caption ?? string.Empty,
            Content = messageBoxText ?? string.Empty,
            Width = AutoAdjustWidth ? DetectMessageBoxWidth(messageBoxText!, button, icon) : 0,
            CenterParent = owner != 0,
            PreferParent = owner,
            RightToLeft = (options & (NativeMessageBoxOptions.RightAlign | NativeMessageBoxOptions.RtlReading)) != 0,
            AllowDialogCancellation = button == NativeMessageBoxButton.OK,
        };

        SetIcon(dialog, icon);
        AddButtons(dialog, button, defaultResult);

        TaskDialogButton result = dialog.ShowDialog(owner);
        return result == null ? GetCancellationResult(button) : ToMessageBoxResult(result.ButtonType);
    }

    private static void ValidateParameters(nint owner, NativeMessageBoxButton button, NativeMessageBoxImage icon, NativeMessageBoxResult defaultResult, NativeMessageBoxOptions options)
    {
        switch (button)
        {
            case NativeMessageBoxButton.OK:
            case NativeMessageBoxButton.OKCancel:
            case NativeMessageBoxButton.YesNoCancel:
            case NativeMessageBoxButton.YesNo:
                break;

            default:
                throw new InvalidEnumArgumentException(nameof(button), (int)button, typeof(NativeMessageBoxButton));
        }

        switch (icon)
        {
            case NativeMessageBoxImage.None:
            case NativeMessageBoxImage.Hand:
            case NativeMessageBoxImage.Question:
            case NativeMessageBoxImage.Exclamation:
            case NativeMessageBoxImage.Asterisk:
                break;

            default:
                throw new InvalidEnumArgumentException(nameof(icon), (int)icon, typeof(NativeMessageBoxImage));
        }

        switch (defaultResult)
        {
            case NativeMessageBoxResult.None:
            case NativeMessageBoxResult.OK:
            case NativeMessageBoxResult.Cancel:
            case NativeMessageBoxResult.Yes:
            case NativeMessageBoxResult.No:
                break;

            default:
                throw new InvalidEnumArgumentException(nameof(defaultResult), (int)defaultResult, typeof(NativeMessageBoxResult));
        }

        if ((options & ~ValidOptions) != 0)
            throw new InvalidEnumArgumentException(nameof(options), (int)options, typeof(NativeMessageBoxOptions));

        if (!IsValidDefaultResult(button, defaultResult))
            throw new ArgumentException("The default result must be one of the buttons displayed by the message box.", nameof(defaultResult));

        if ((options & (NativeMessageBoxOptions.DefaultDesktopOnly | NativeMessageBoxOptions.ServiceNotification)) != 0 && owner != 0)
            throw new ArgumentException("Desktop notification options cannot be used with an owner window.", nameof(options));
    }

    private static bool IsValidDefaultResult(NativeMessageBoxButton button, NativeMessageBoxResult defaultResult)
    {
        if (defaultResult == NativeMessageBoxResult.None)
            return true;

        return button switch
        {
            NativeMessageBoxButton.OK => defaultResult == NativeMessageBoxResult.OK,
            NativeMessageBoxButton.OKCancel => defaultResult == NativeMessageBoxResult.OK || defaultResult == NativeMessageBoxResult.Cancel,
            NativeMessageBoxButton.YesNoCancel => defaultResult == NativeMessageBoxResult.Yes || defaultResult == NativeMessageBoxResult.No || defaultResult == NativeMessageBoxResult.Cancel,
            NativeMessageBoxButton.YesNo => defaultResult == NativeMessageBoxResult.Yes || defaultResult == NativeMessageBoxResult.No,
            _ => false,
        };
    }

    private static void AddButtons(TaskDialogMessageBox dialog, NativeMessageBoxButton button, NativeMessageBoxResult defaultResult)
    {
        switch (button)
        {
            case NativeMessageBoxButton.OK:
                dialog.Buttons.Add(CreateButton(ButtonType.Ok, defaultResult));
                break;

            case NativeMessageBoxButton.OKCancel:
                dialog.Buttons.Add(CreateButton(ButtonType.Ok, defaultResult));
                dialog.Buttons.Add(CreateButton(ButtonType.Cancel, defaultResult));
                break;

            case NativeMessageBoxButton.YesNoCancel:
                dialog.Buttons.Add(CreateButton(ButtonType.Yes, defaultResult));
                dialog.Buttons.Add(CreateButton(ButtonType.No, defaultResult));
                dialog.Buttons.Add(CreateButton(ButtonType.Cancel, defaultResult));
                break;

            case NativeMessageBoxButton.YesNo:
                dialog.Buttons.Add(CreateButton(ButtonType.Yes, defaultResult));
                dialog.Buttons.Add(CreateButton(ButtonType.No, defaultResult));
                break;
        }
    }

    private static TaskDialogButton CreateButton(ButtonType type, NativeMessageBoxResult defaultResult)
    {
        return new TaskDialogButton(type)
        {
            Default = ToMessageBoxResult(type) == defaultResult,
        };
    }

    private static void SetIcon(TaskDialogMessageBox dialog, NativeMessageBoxImage icon)
    {
        switch (icon)
        {
            case NativeMessageBoxImage.None:
                break;

            case NativeMessageBoxImage.Hand:
                dialog.MainIcon = TaskDialogIcon.Error;
                break;

            case NativeMessageBoxImage.Question:
                byte[] icoBytes = ResourcesProvider.GetBytes("pack://application:,,,/Wpf.Ui.Violeta;component/Resources/Images/help.ico");
                uint dpi = GetDpiForWindow(dialog.PreferParent);
                int iconSize = MulDiv(32, (int)dpi, 96);
                nint hicon = iconSize switch
                {
                    256 or 64 or 48 or 40 or 32 or 24 or 20 or 16 => CreateIconHandleFromBytes(icoBytes, iconSize, iconSize),
                    _ => CreateIconHandleFromBytes(icoBytes),
                };
                dialog.CustomMainIcon = hicon != IntPtr.Zero ? hicon : LoadIconW(0, new IntPtr(IDI_QUESTION));
                break;

            case NativeMessageBoxImage.Exclamation:
                dialog.MainIcon = TaskDialogIcon.Warning;
                break;

            case NativeMessageBoxImage.Asterisk:
                dialog.MainIcon = TaskDialogIcon.Information;
                break;
        }
    }

    private static NativeMessageBoxResult ToMessageBoxResult(ButtonType type)
    {
        return type switch
        {
            ButtonType.Ok => NativeMessageBoxResult.OK,
            ButtonType.Cancel => NativeMessageBoxResult.Cancel,
            ButtonType.Yes => NativeMessageBoxResult.Yes,
            ButtonType.No => NativeMessageBoxResult.No,
            _ => NativeMessageBoxResult.None,
        };
    }

    private static NativeMessageBoxResult GetCancellationResult(NativeMessageBoxButton button)
    {
        return button switch
        {
            NativeMessageBoxButton.OK => NativeMessageBoxResult.OK,
            NativeMessageBoxButton.OKCancel => NativeMessageBoxResult.Cancel,
            NativeMessageBoxButton.YesNoCancel => NativeMessageBoxResult.Cancel,
            _ => NativeMessageBoxResult.None,
        };
    }

    private static int DetectMessageBoxWidth(string messageBoxText, NativeMessageBoxButton button, NativeMessageBoxImage icon)
    {
        int minimumWidth = Math.Max(MinimumMessageBoxWidth, GetMinimumWidthForButtons(button));
        int lineCountAtMinimumWidth = EstimateWrappedLineCount(messageBoxText, minimumWidth, icon);
        int maximumWidth = GetMaximumWidthForLineCount(lineCountAtMinimumWidth, minimumWidth);

        for (int width = minimumWidth; width <= maximumWidth; width += MessageBoxWidthStep)
        {
            if (EstimateWrappedLineCount(messageBoxText, width, icon) <= TargetMessageLineCount)
                return width;
        }

        return maximumWidth;
    }

    private static int GetMinimumWidthForButtons(NativeMessageBoxButton button)
    {
        return button switch
        {
            NativeMessageBoxButton.YesNoCancel => 360,
            NativeMessageBoxButton.OKCancel or NativeMessageBoxButton.YesNo => 320,
            _ => MinimumMessageBoxWidth,
        };
    }

    private static int GetMaximumWidthForLineCount(int lineCount, int minimumWidth)
    {
        int maximumWidth = lineCount switch
        {
            <= 2 => minimumWidth,
            <= 4 => 340,
            <= 8 => 420,
            <= 12 => 480,
            _ => MaximumMessageBoxWidth,
        };

        return Math.Max(minimumWidth, maximumWidth);
    }

    private static int EstimateWrappedLineCount(string text, int dialogWidth, NativeMessageBoxImage icon)
    {
        int availableUnits = Math.Max(
            1,
            (dialogWidth - ContentMarginDialogWidth - (icon == NativeMessageBoxImage.None ? 0 : IconDialogWidth)) / AverageCharacterDialogUnits);

        if (string.IsNullOrEmpty(text))
            return 1;

        int lineCount = 0;
        foreach (string line in NormalizeLineBreaks(text).Split('\n'))
        {
            int textUnits = Math.Max(1, MeasureVisualTextUnits(line));
            lineCount += (textUnits + availableUnits - 1) / availableUnits;
        }

        return lineCount;
    }

    private static string NormalizeLineBreaks(string text)
    {
        return text.Replace("\r\n", "\n").Replace('\r', '\n');
    }

    private static int MeasureVisualTextUnits(string text)
    {
        int units = 0;
        foreach (char c in text)
        {
            if (c == '\t')
            {
                units += 4;
            }
            else if (!char.IsControl(c))
            {
                units += IsWideCharacter(c) ? 2 : 1;
            }
        }

        return units;
    }

    private static bool IsWideCharacter(char c)
    {
        return c is >= '\u1100' and <= '\u115f'
            or >= '\u2329' and <= '\u232a'
            or >= '\u2e80' and <= '\ua4cf'
            or >= '\uac00' and <= '\ud7a3'
            or >= '\uf900' and <= '\ufaff'
            or >= '\ufe10' and <= '\ufe19'
            or >= '\ufe30' and <= '\ufe6f'
            or >= '\uff00' and <= '\uff60'
            or >= '\uffe0' and <= '\uffe6';
    }

    /// <summary>
    /// Create icon handle from icon bytes with specified size
    /// </summary>
    private static nint CreateIconHandleFromBytes(byte[] bytes, int desiredWidth, int desiredHeight)
    {
        if (bytes.Length == 0) throw new InvalidDataException("Icon stream is empty.");
        // Parse ICONDIR (first 6 bytes)
        if (bytes.Length < 6) throw new InvalidDataException("Stream too short for ICONDIR.");

        ushort reserved = BitConverter.ToUInt16(bytes, 0); // Must be 0
        ushort type = BitConverter.ToUInt16(bytes, 2);     // 1 = ICON
        ushort count = BitConverter.ToUInt16(bytes, 4);    // Number of icons

        if (reserved != 0 || type != 1 || count == 0)
            throw new InvalidDataException("Invalid ICO header.");

        const int entryItemSize = 16;
        int entryOffset = 6;
        uint imageSize = 0;
        uint imageOffset = 0;
        bool found = false;

        // Iterate icon entries to match target size
        for (int i = 0; i < count; i++)
        {
            if (bytes.Length < entryOffset + entryItemSize)
                throw new InvalidDataException("Stream too short for ICONDIRENTRY.");

            // 0 means 256px in ICO standard
            int width = bytes[entryOffset] == 0 ? 256 : bytes[entryOffset];
            int height = bytes[entryOffset + 1] == 0 ? 256 : bytes[entryOffset + 1];

            if (width == desiredWidth && height == desiredHeight)
            {
                imageSize = BitConverter.ToUInt32(bytes, entryOffset + 8);
                imageOffset = BitConverter.ToUInt32(bytes, entryOffset + 12);
                found = true;
                break;
            }

            entryOffset += entryItemSize;
        }

        if (!found)
            throw new InvalidDataException($"Icon size {desiredWidth}x{desiredHeight} not found.");

        if (imageOffset + imageSize > bytes.Length)
            throw new InvalidDataException("Icon image out of bounds.");

        nint hIcon = CreateIconFromResourceEx(
            ref bytes[imageOffset],
            imageSize,
            true,
            0x00030000,
            0,
            0,
            0);

        if (hIcon == IntPtr.Zero)
            throw new InvalidOperationException("CreateIconFromResourceEx failed.");

        return hIcon;
    }

    private static nint CreateIconHandleFromBytes(byte[] bytes)
    {
        if (bytes.Length == 0)
            throw new InvalidDataException("Icon stream is empty.");

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

        nint hIcon = CreateIconFromResourceEx(
            ref bytes[imageOffset],
            imageSize,
            true,
            0x00030000,
            0, 0,
            0);

        if (hIcon == IntPtr.Zero)
            throw new InvalidOperationException("CreateIconFromResourceEx failed.");

        return hIcon;
    }

    private static int MulDiv(int nNumber, int nNumerator, int nDenominator)
    {
        if (nDenominator == 0)
            return 0;

        long product = (long)nNumber * nNumerator;
        return (int)(product / nDenominator);
    }

    [DllImport("user32.dll")]
    private static extern uint GetDpiForWindow(nint hwnd);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    private static extern nint LoadIconW(nint hInstance, nint lpIconName);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern nint CreateIconFromResourceEx(
        ref byte pbIconBits,
        uint cbIconBits,
        bool fIcon,
        uint dwVersion,
        int cxDesired,
        int cyDesired,
        uint uFlags);
}
