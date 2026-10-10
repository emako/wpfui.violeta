using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;

namespace Wpf.Ui.Violeta.Win32;

/// <summary>
/// Displays a dialog that prompts the user to open a folder.
/// Member names follow <see cref="Microsoft.Win32.OpenFolderDialog"/> from .NET Core.
/// </summary>
public sealed class OpenFolderDialog
{
    private const int ErrorCancelled = unchecked((int)0x800704C7);

    private string? _title;
    private string? _initialDirectory;
    private string[]? _folderNames;
    private FileOpenDialogOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenFolderDialog"/> class.
    /// </summary>
    public OpenFolderDialog()
    {
        Initialize();
    }

    /// <summary>
    /// Gets a value indicating whether the current operating system supports the folder dialog.
    /// </summary>
    public static bool IsFolderDialogSupported =>
        Environment.OSVersion.Platform == PlatformID.Win32NT
        && Environment.OSVersion.Version >= new Version(6, 0);

    /// <summary>
    /// Gets or sets the initial directory displayed by the dialog.
    /// </summary>
    public string InitialDirectory
    {
        get => _initialDirectory ?? string.Empty;
        set => _initialDirectory = value;
    }

    /// <summary>
    /// Gets or sets the text that appears in the title bar of the dialog.
    /// </summary>
    public string Title
    {
        get => _title ?? string.Empty;
        set => _title = value;
    }

    /// <summary>
    /// Gets or sets the full path of the folder selected in the dialog.
    /// </summary>
    public string FolderName
    {
        get => _folderNames is { Length: > 0 } ? _folderNames[0] : string.Empty;
        set => _folderNames = value is null ? null : [value];
    }

    /// <summary>
    /// Gets the folder names of all selected folders in the dialog.
    /// </summary>
    public string[] FolderNames => CloneFolderNames();

    /// <summary>
    /// Gets the folder name component of <see cref="FolderName"/>.
    /// </summary>
    public string SafeFolderName
    {
        get
        {
            string? name = Path.GetFileName(FolderName);
            return name ?? string.Empty;
        }
    }

    /// <summary>
    /// Gets the folder name component of each selected folder.
    /// </summary>
    public string[] SafeFolderNames
    {
        get
        {
            string[] names = CloneFolderNames();
            for (int i = 0; i < names.Length; i++)
            {
                names[i] = Path.GetFileName(names[i]) ?? string.Empty;
            }

            return names;
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the dialog allows users to select multiple folders.
    /// </summary>
    public bool Multiselect
    {
        get => (_options & FileOpenDialogOptions.AllowMultiSelect) != 0;
        set => SetOption(FileOpenDialogOptions.AllowMultiSelect, value);
    }

    /// <summary>
    /// Resets all properties to their default values.
    /// </summary>
    public void Reset()
    {
        _title = string.Empty;
        _initialDirectory = string.Empty;
        _folderNames = null;
        _options = FileOpenDialogOptions.None;
        Initialize();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return base.ToString() + ", FolderName: " + FolderName;
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

    private void Initialize()
    {
        SetOption(FileOpenDialogOptions.FileMustExist, value: true);
        SetOption(FileOpenDialogOptions.PickFolders, value: true);
    }

    private void ConfigureDialog(IFileOpenDialog dialog)
    {
        Marshal.ThrowExceptionForHR(dialog.SetOptions(
            FileOpenDialogOptions.ForceFileSystem
            | _options));

        if (!string.IsNullOrEmpty(_title))
        {
            Marshal.ThrowExceptionForHR(dialog.SetTitle(_title!));
        }

        var initialDirectory = InitialDirectory;
        if (!string.IsNullOrWhiteSpace(initialDirectory) && Directory.Exists(initialDirectory))
        {
            var folder = CreateShellItem(initialDirectory);
            try
            {
                Marshal.ThrowExceptionForHR(dialog.SetFolder(folder));
            }
            finally
            {
                Marshal.FinalReleaseComObject(folder);
            }
        }

        if (string.IsNullOrWhiteSpace(FolderName))
        {
            return;
        }

        var selectedPath = FolderName;
        if (string.IsNullOrWhiteSpace(initialDirectory))
        {
            var parent = Path.GetDirectoryName(selectedPath);
            if (!string.IsNullOrWhiteSpace(parent) && Directory.Exists(parent))
            {
                var folder = CreateShellItem(parent);
                try
                {
                    Marshal.ThrowExceptionForHR(dialog.SetFolder(folder));
                    Marshal.ThrowExceptionForHR(dialog.SetFileName(Path.GetFileName(selectedPath)));
                }
                finally
                {
                    Marshal.FinalReleaseComObject(folder);
                }

                return;
            }
        }

        Marshal.ThrowExceptionForHR(dialog.SetFileName(Path.GetFileName(selectedPath)));
    }

    private void ReadResult(IFileOpenDialog dialog)
    {
        if (Multiselect)
        {
            Marshal.ThrowExceptionForHR(dialog.GetResults(out var results));
            try
            {
                Marshal.ThrowExceptionForHR(results.GetCount(out var count));
                var paths = new string[count];
                for (uint index = 0; index < count; index++)
                {
                    Marshal.ThrowExceptionForHR(results.GetItemAt(index, out var item));
                    try
                    {
                        Marshal.ThrowExceptionForHR(item.GetDisplayName(ShellItemDisplayName.FileSystemPath, out paths[index]));
                    }
                    finally
                    {
                        Marshal.FinalReleaseComObject(item);
                    }
                }

                _folderNames = paths;
            }
            finally
            {
                Marshal.FinalReleaseComObject(results);
            }

            return;
        }

        Marshal.ThrowExceptionForHR(dialog.GetResult(out var result));
        try
        {
            Marshal.ThrowExceptionForHR(result.GetDisplayName(ShellItemDisplayName.FileSystemPath, out var path));
            _folderNames = [path];
        }
        finally
        {
            Marshal.FinalReleaseComObject(result);
        }
    }

    private string[] CloneFolderNames()
    {
        if (_folderNames is null)
        {
            return [];
        }

        return (string[])_folderNames.Clone();
    }

    private void SetOption(FileOpenDialogOptions option, bool value)
    {
        _options = value ? _options | option : _options & ~option;
    }

    private static IShellItem CreateShellItem(string path)
    {
        var interfaceId = typeof(IShellItem).GUID;
        Marshal.ThrowExceptionForHR(SHCreateItemFromParsingName(path, 0, ref interfaceId, out var item));
        return item;
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
    [Guid("D57C7288-D4AD-4768-BE02-9D969532D960")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [SuppressMessage("Interoperability", "SYSLIB1096:Convert to 'GeneratedComInterface'")]
    private interface IFileOpenDialog
    {
        // ComImport does not append methods inherited from IFileDialog, so the base
        // slots are listed here. Otherwise GetResults calls the wrong native method.
        [PreserveSig] public int Show(nint parent);

        [PreserveSig] public int SetFileTypes(uint fileTypeCount, nint fileTypes);

        [PreserveSig] public int SetFileTypeIndex(uint fileTypeIndex);

        [PreserveSig] public int GetFileTypeIndex(out uint fileTypeIndex);

        [PreserveSig] public int Advise(nint events, out uint cookie);

        [PreserveSig] public int Unadvise(uint cookie);

        [PreserveSig] public int SetOptions(FileOpenDialogOptions options);

        [PreserveSig] public int GetOptions(out FileOpenDialogOptions options);

        [PreserveSig] public int SetDefaultFolder(IShellItem folder);

        [PreserveSig] public int SetFolder(IShellItem folder);

        [PreserveSig] public int GetFolder(out IShellItem folder);

        [PreserveSig] public int GetCurrentSelection(out IShellItem item);

        [PreserveSig] public int SetFileName([MarshalAs(UnmanagedType.LPWStr)] string name);

        [PreserveSig] public int GetFileName([MarshalAs(UnmanagedType.LPWStr)] out string name);

        [PreserveSig] public int SetTitle([MarshalAs(UnmanagedType.LPWStr)] string title);

        [PreserveSig] public int SetOkButtonLabel([MarshalAs(UnmanagedType.LPWStr)] string text);

        [PreserveSig] public int SetFileNameLabel([MarshalAs(UnmanagedType.LPWStr)] string label);

        [PreserveSig] public int GetResult(out IShellItem item);

        [PreserveSig] public int AddPlace(IShellItem item, int alignment);

        [PreserveSig] public int SetDefaultExtension([MarshalAs(UnmanagedType.LPWStr)] string defaultExtension);

        [PreserveSig] public int Close(int hResult);

        [PreserveSig] public int SetClientGuid(ref Guid clientGuid);

        [PreserveSig] public int ClearClientData();

        [PreserveSig] public int SetFilter(nint filter);

        [PreserveSig] public int GetResults(out IShellItemArray results);

        [PreserveSig] public int GetSelectedItems(out IShellItemArray items);
    }

    [ComImport]
    [Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [SuppressMessage("Interoperability", "SYSLIB1096:Convert to 'GeneratedComInterface'")]
    private interface IShellItem
    {
        [PreserveSig] public int BindToHandler(nint bindContext, ref Guid handlerId, ref Guid interfaceId, out nint result);

        [PreserveSig] public int GetParent(out IShellItem parent);

        [PreserveSig] public int GetDisplayName(ShellItemDisplayName displayName, [MarshalAs(UnmanagedType.LPWStr)] out string name);

        [PreserveSig] public int GetAttributes(uint attributes, out uint result);

        [PreserveSig] public int Compare(IShellItem other, uint hint, out int order);
    }

    [ComImport]
    [Guid("B63EA76D-1F85-456F-A19C-48159EFA858B")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [SuppressMessage("Interoperability", "SYSLIB1096:Convert to 'GeneratedComInterface'")]
    private interface IShellItemArray
    {
        [PreserveSig] public int BindToHandler(nint bindContext, ref Guid handlerId, ref Guid interfaceId, out nint result);

        [PreserveSig] public int GetPropertyStore(int flags, ref Guid interfaceId, out nint result);

        [PreserveSig] public int GetPropertyDescriptionList(ref nint keyType, ref Guid interfaceId, out nint result);

        [PreserveSig] public int GetAttributes(uint attributes, uint mask, out uint result);

        [PreserveSig] public int GetCount(out uint count);

        [PreserveSig] public int GetItemAt(uint index, out IShellItem item);

        [PreserveSig] public int EnumItems(out nint enumShellItems);
    }

    [Flags]
    private enum FileOpenDialogOptions : uint
    {
        None = 0,
        PickFolders = 0x00000020,
        ForceFileSystem = 0x00000040,
        AllowMultiSelect = 0x00000200,
        FileMustExist = 0x00001000,
    }

    private enum ShellItemDisplayName : uint
    {
        FileSystemPath = 0x80058000,
    }
}
