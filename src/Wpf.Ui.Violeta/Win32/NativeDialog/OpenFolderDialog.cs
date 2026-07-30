using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Wpf.Ui.Violeta.Win32;

/// <summary>
/// Provides the Windows folder selection dialog.
/// </summary>
public sealed class OpenFolderDialog
{
    private const int ErrorCancelled = unchecked((int)0x800704C7);

    private string? _description;
    private string? _selectedPath;
    private string[]? _selectedPaths;
    private FileOpenDialogOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenFolderDialog"/> class.
    /// </summary>
    public OpenFolderDialog()
    {
        Reset();
    }

    /// <summary>
    /// Gets a value indicating whether the current operating system supports the folder dialog.
    /// </summary>
    public static bool IsFolderDialogSupported =>
        Environment.OSVersion.Platform == PlatformID.Win32NT
        && Environment.OSVersion.Version >= new Version(6, 0);

    /// <summary>
    /// Gets or sets descriptive text displayed by the dialog.
    /// </summary>
    public string Description
    {
        get => _description ?? string.Empty;
        set => _description = value;
    }

    /// <summary>
    /// Gets or sets the folder selected when the dialog opens, or the selected folder after it closes.
    /// </summary>
    public string SelectedPath
    {
        get => _selectedPath ?? (_selectedPaths is { Length: > 0 } ? _selectedPaths[0] : string.Empty);
        set => _selectedPath = value;
    }

    /// <summary>
    /// Gets or sets whether multiple folders can be selected.
    /// </summary>
    public bool Multiselect
    {
        get => (_options & FileOpenDialogOptions.AllowMultiSelect) != 0;
        set => SetOption(FileOpenDialogOptions.AllowMultiSelect, value);
    }

    /// <summary>
    /// Gets the paths of all folders selected in the dialog.
    /// </summary>
    public string[] SelectedPaths
    {
        get
        {
            if (_selectedPaths is not null)
            {
                return (string[])_selectedPaths.Clone();
            }

            return string.IsNullOrWhiteSpace(_selectedPath) ? Array.Empty<string>() : new[] { _selectedPath! };
        }
        set => _selectedPaths = value;
    }

    /// <summary>
    /// Gets or sets whether <see cref="Description"/> is used as the dialog title.
    /// </summary>
    public bool UseDescriptionForTitle { get; set; }

    /// <summary>
    /// Gets or sets the root folder for legacy folder dialogs.
    /// </summary>
    /// <remarks>The Windows common folder dialog does not use this property.</remarks>
    public Environment.SpecialFolder RootFolder { get; set; }

    /// <summary>
    /// Gets or sets whether a legacy folder dialog shows its New Folder button.
    /// </summary>
    /// <remarks>The Windows common folder dialog always provides folder creation.</remarks>
    public bool ShowNewFolderButton { get; set; }

    /// <summary>
    /// Resets all properties to their default values.
    /// </summary>
    public void Reset()
    {
        _description = string.Empty;
        _selectedPath = string.Empty;
        _selectedPaths = null;
        _options = FileOpenDialogOptions.None;
        UseDescriptionForTitle = false;
        RootFolder = Environment.SpecialFolder.Desktop;
        ShowNewFolderButton = true;
    }

    /// <summary>
    /// Displays the folder selection dialog.
    /// </summary>
    /// <returns><see langword="true"/> when the user selects a folder; otherwise, <see langword="false"/>.</returns>
    public bool? ShowDialog()
    {
        return ShowDialog(0);
    }

    /// <summary>
    /// Displays the folder selection dialog with the specified owner window.
    /// </summary>
    /// <param name="owner">The owner window handle, or zero to use the active window.</param>
    /// <returns><see langword="true"/> when the user selects a folder; otherwise, <see langword="false"/>.</returns>
    public bool? ShowDialog(nint owner)
    {
        if (!IsFolderDialogSupported)
        {
            throw new PlatformNotSupportedException("The Windows common folder dialog requires Windows Vista or later.");
        }

        var ownerHandle = owner == 0 ? User32.GetActiveWindow() : owner;
        IFileOpenDialog? dialog = null;

        try
        {
            dialog = (IFileOpenDialog)new FileOpenDialogComObject();
            ConfigureDialog(dialog);

            var result = dialog.Show(ownerHandle);
            if (result == ErrorCancelled)
            {
                return false;
            }

            Marshal.ThrowExceptionForHR(result);
            ReadResult(dialog);
            return true;
        }
        finally
        {
            if (dialog is not null)
            {
                Marshal.FinalReleaseComObject(dialog);
            }
        }
    }

    private void ConfigureDialog(IFileOpenDialog dialog)
    {
        ThrowIfFailed(dialog.SetOptions(
            FileOpenDialogOptions.PickFolders
            | FileOpenDialogOptions.ForceFileSystem
            | FileOpenDialogOptions.FileMustExist
            | _options));

        if (!string.IsNullOrEmpty(_description))
        {
            var description = _description;
            if (UseDescriptionForTitle)
            {
                ThrowIfFailed(dialog.SetTitle(description!));
            }
            else
            {
                var customize = (IFileDialogCustomize)dialog;
                ThrowIfFailed(customize.AddText(0, description!));
            }
        }

        if (string.IsNullOrWhiteSpace(_selectedPath))
        {
            return;
        }

        var selectedPath = _selectedPath;
        var parent = Path.GetDirectoryName(selectedPath);
        if (string.IsNullOrWhiteSpace(parent) || !Directory.Exists(parent))
        {
            ThrowIfFailed(dialog.SetFileName(selectedPath!));
            return;
        }

        var folder = CreateShellItem(parent);
        try
        {
            ThrowIfFailed(dialog.SetFolder(folder));
            ThrowIfFailed(dialog.SetFileName(Path.GetFileName(selectedPath)));
        }
        finally
        {
            Marshal.FinalReleaseComObject(folder);
        }
    }

    private void ReadResult(IFileOpenDialog dialog)
    {
        if (Multiselect)
        {
            ThrowIfFailed(dialog.GetResults(out var results));
            try
            {
                ThrowIfFailed(results.GetCount(out var count));
                var paths = new string[count];
                for (uint index = 0; index < count; index++)
                {
                    ThrowIfFailed(results.GetItemAt(index, out var item));
                    try
                    {
                        ThrowIfFailed(item.GetDisplayName(ShellItemDisplayName.FileSystemPath, out paths[index]));
                    }
                    finally
                    {
                        Marshal.FinalReleaseComObject(item);
                    }
                }

                _selectedPaths = paths;
                _selectedPath = paths.Length > 0 ? paths[0] : string.Empty;
            }
            finally
            {
                Marshal.FinalReleaseComObject(results);
            }

            return;
        }

        ThrowIfFailed(dialog.GetResult(out var result));
        try
        {
            ThrowIfFailed(result.GetDisplayName(ShellItemDisplayName.FileSystemPath, out var path));
            _selectedPath = path;
            _selectedPaths = null;
        }
        finally
        {
            Marshal.FinalReleaseComObject(result);
        }
    }

    private void SetOption(FileOpenDialogOptions option, bool value)
    {
        _options = value ? _options | option : _options & ~option;
    }

    private static IShellItem CreateShellItem(string path)
    {
        var interfaceId = typeof(IShellItem).GUID;
        ThrowIfFailed(SHCreateItemFromParsingName(path, 0, ref interfaceId, out var item));
        return item;
    }

    private static void ThrowIfFailed(int hResult)
    {
        if (hResult < 0)
        {
            Marshal.ThrowExceptionForHR(hResult);
        }
    }

    [DllImport("shell32.dll", CharSet = CharSet.Unicode, PreserveSig = true)]
    private static extern int SHCreateItemFromParsingName(
        string path,
        nint bindContext,
        ref Guid interfaceId,
        [MarshalAs(UnmanagedType.Interface)] out IShellItem shellItem);

    [ComImport]
    [Guid("DC1C5A9C-E88A-4DDE-A5A1-60F82A20AEF7")]
    private class FileOpenDialogComObject;

    [ComImport]
    [Guid("42F85136-DB7E-439C-85F1-E4075D135FC8")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IFileDialog
    {
        [PreserveSig] int Show(nint parent);
        [PreserveSig] int SetFileTypes(uint fileTypeCount, nint fileTypes);
        [PreserveSig] int SetFileTypeIndex(uint fileTypeIndex);
        [PreserveSig] int GetFileTypeIndex(out uint fileTypeIndex);
        [PreserveSig] int Advise(nint events, out uint cookie);
        [PreserveSig] int Unadvise(uint cookie);
        [PreserveSig] int SetOptions(FileOpenDialogOptions options);
        [PreserveSig] int GetOptions(out FileOpenDialogOptions options);
        [PreserveSig] int SetDefaultFolder(IShellItem folder);
        [PreserveSig] int SetFolder(IShellItem folder);
        [PreserveSig] int GetFolder(out IShellItem folder);
        [PreserveSig] int GetCurrentSelection(out IShellItem item);
        [PreserveSig] int SetFileName([MarshalAs(UnmanagedType.LPWStr)] string name);
        [PreserveSig] int GetFileName([MarshalAs(UnmanagedType.LPWStr)] out string name);
        [PreserveSig] int SetTitle([MarshalAs(UnmanagedType.LPWStr)] string title);
        [PreserveSig] int SetOkButtonLabel([MarshalAs(UnmanagedType.LPWStr)] string text);
        [PreserveSig] int SetFileNameLabel([MarshalAs(UnmanagedType.LPWStr)] string label);
        [PreserveSig] int GetResult(out IShellItem item);
        [PreserveSig] int AddPlace(IShellItem item, int alignment);
        [PreserveSig] int SetDefaultExtension([MarshalAs(UnmanagedType.LPWStr)] string defaultExtension);
        [PreserveSig] int Close(int hResult);
        [PreserveSig] int SetClientGuid(ref Guid clientGuid);
        [PreserveSig] int ClearClientData();
        [PreserveSig] int SetFilter(nint filter);
    }

    [ComImport]
    [Guid("D57C7288-D4AD-4768-BE02-9D969532D960")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IFileOpenDialog : IFileDialog
    {
        [PreserveSig] int GetResults(out IShellItemArray results);
        [PreserveSig] int GetSelectedItems(out IShellItemArray items);
    }

    [ComImport]
    [Guid("E6FDD21A-163F-4975-9C8C-A69F1BA37034")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IFileDialogCustomize
    {
        [PreserveSig] int EnableOpenDropDown(uint controlId);
        [PreserveSig] int AddMenu(uint controlId, [MarshalAs(UnmanagedType.LPWStr)] string label);
        [PreserveSig] int AddPushButton(uint controlId, [MarshalAs(UnmanagedType.LPWStr)] string label);
        [PreserveSig] int AddComboBox(uint controlId);
        [PreserveSig] int AddRadioButtonList(uint controlId);
        [PreserveSig] int AddCheckButton(uint controlId, [MarshalAs(UnmanagedType.LPWStr)] string label, [MarshalAs(UnmanagedType.Bool)] bool isChecked);
        [PreserveSig] int AddEditBox(uint controlId, [MarshalAs(UnmanagedType.LPWStr)] string text);
        [PreserveSig] int AddSeparator(uint controlId);
        [PreserveSig] int AddText(uint controlId, [MarshalAs(UnmanagedType.LPWStr)] string text);
    }

    [ComImport]
    [Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IShellItem
    {
        [PreserveSig] int BindToHandler(nint bindContext, ref Guid handlerId, ref Guid interfaceId, out nint result);
        [PreserveSig] int GetParent(out IShellItem parent);
        [PreserveSig] int GetDisplayName(ShellItemDisplayName displayName, [MarshalAs(UnmanagedType.LPWStr)] out string name);
        [PreserveSig] int GetAttributes(uint attributes, out uint result);
        [PreserveSig] int Compare(IShellItem other, uint hint, out int order);
    }

    [ComImport]
    [Guid("B63EA76D-1F85-456F-A19C-48159EFA858B")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IShellItemArray
    {
        [PreserveSig] int BindToHandler(nint bindContext, ref Guid handlerId, ref Guid interfaceId, out nint result);
        [PreserveSig] int GetPropertyStore(int flags, ref Guid interfaceId, out nint result);
        [PreserveSig] int GetPropertyDescriptionList(ref nint keyType, ref Guid interfaceId, out nint result);
        [PreserveSig] int GetAttributes(uint attributes, uint mask, out uint result);
        [PreserveSig] int GetCount(out uint count);
        [PreserveSig] int GetItemAt(uint index, out IShellItem item);
        [PreserveSig] int EnumItems(out nint enumShellItems);
    }

    [Flags]
    private enum FileOpenDialogOptions : uint
    {
        None = 0,
        AllowMultiSelect = 0x00000200,
        ForceFileSystem = 0x00000040,
        FileMustExist = 0x00001000,
        PickFolders = 0x00000020,
    }

    private enum ShellItemDisplayName : uint
    {
        FileSystemPath = 0x80058000,
    }
}
