using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Wpf.Ui.Violeta.Win32;

public static class IconManager
{
    private static ImageSource SmallDirIcon = null!;
    private static ImageSource LargeDirIcon = null!;
    private static readonly Dictionary<string, ImageSource> SmallIconCache = [];
    private static readonly Dictionary<string, ImageSource> LargeIconCache = [];

    /// <summary>
    /// May [".exe", ".png", ".jpg", ".jpeg", ".bmp", ".gif", ".tiff", ".ico"]
    /// </summary>
    public static string[] ExcludeExtensions { get; set; } = null!;

    public static void ClearCache()
    {
        SmallDirIcon = LargeDirIcon = null!;

        SmallIconCache.Clear();
        LargeIconCache.Clear();
    }

    /// <summary>
    /// Get the icon of a directory
    /// </summary>
    /// <param name="large">16x16 or 32x32 icon</param>
    /// <returns>an icon</returns>
    public static ImageSource FindIconForDir(bool large = false)
    {
        ImageSource icon = large ? LargeDirIcon : SmallDirIcon;

        if (icon != null)
        {
            return icon;
        }

        nint hIcon = IconReader.GetFolderIcon(large ? IconReader.IconSize.Large : IconReader.IconSize.Small, false);

        icon = hIcon.ToImageSource();
        _ = User32.DestroyIcon(hIcon);

        if (large)
        {
            LargeDirIcon = icon;
        }
        else
        {
            SmallDirIcon = icon;
        }

        return icon;
    }

    /// <summary>
    /// Get an icon for a given filename
    /// </summary>
    /// <param name="fileName">any filename</param>
    /// <param name="large">16x16 or 32x32 icon</param>
    /// <returns>null if path is null, otherwise - an icon</returns>
    public static ImageSource FindIconForFilename(string fileName, bool large = false)
    {
        string extension = Path.GetExtension(fileName);
        if (extension == null)
        {
            return null!;
        }

        bool needCache = !(ExcludeExtensions?.Contains(extension) ?? false);

        Dictionary<string, ImageSource> cache = large ? LargeIconCache : SmallIconCache;
        if (needCache && cache.TryGetValue(extension, out ImageSource? icon))
        {
            return icon;
        }

        nint hIcon = IconReader.GetFileIcon(fileName, large ? IconReader.IconSize.Large : IconReader.IconSize.Small, false);

        icon = hIcon.ToImageSource();
        _ = User32.DestroyIcon(hIcon);

        if (needCache)
        {
            cache.Add(extension, icon);
        }

        return icon;
    }

    private static ImageSource ToImageSource(this nint icon)
    {
        ImageSource imageSource = Imaging.CreateBitmapSourceFromHIcon(icon, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        return imageSource;
    }

    /// <summary>
    /// Provides static methods to read system icons for both folders and files.
    /// </summary>
    private static class IconReader
    {
        /// <summary>
        /// Options to specify the size of icons to return.
        /// </summary>
        public enum IconSize
        {
            /// <summary>
            /// Specify large icon - 32 pixels by 32 pixels.
            /// </summary>
            Large = 0,

            /// <summary>
            /// Specify small icon - 16 pixels by 16 pixels.
            /// </summary>
            Small = 1
        }

        /// <summary>
        /// Returns the icon of a folder.
        /// </summary>
        /// <param name="size">Large or small</param>
        /// <param name="linkOverlay">Whether to include the link icon</param>
        /// <returns>hIcon</returns>
        public static nint GetFolderIcon(IconSize size, bool linkOverlay)
        {
            Shell32.Shfileinfo shfi = new();
            uint flags = Shell32.ShgfiIcon | Shell32.ShgfiUsefileattributes;

            if (linkOverlay)
            {
                flags |= Shell32.ShgfiLinkoverlay;
            }

            if (IconSize.Small == size)
            {
                flags |= Shell32.ShgfiSmallicon;
            }
            else
            {
                flags |= Shell32.ShgfiLargeicon;
            }

            _ = Shell32.SHGetFileInfo("placeholder", Shell32.FileAttributeDirectory, ref shfi, (uint)Marshal.SizeOf(shfi), flags);
            return shfi.hIcon;
        }

        /// <summary>
        /// Returns an icon for a given file - indicated by the name parameter.
        /// </summary>
        /// <param name="name">Pathname for file.</param>
        /// <param name="size">Large or small</param>
        /// <param name="linkOverlay">Whether to include the link icon</param>
        /// <returns>hIcon</returns>
        public static nint GetFileIcon(string name, IconSize size, bool linkOverlay)
        {
            Shell32.Shfileinfo shfi = new();
            uint flags = Shell32.ShgfiIcon | Shell32.ShgfiUsefileattributes;

            if (linkOverlay)
            {
                flags |= Shell32.ShgfiLinkoverlay;
            }

            if (IconSize.Small == size)
            {
                flags |= Shell32.ShgfiSmallicon;
            }
            else
            {
                flags |= Shell32.ShgfiLargeicon;
            }

            _ = Shell32.SHGetFileInfo(name, Shell32.FileAttributeNormal, ref shfi, (uint)Marshal.SizeOf(shfi), flags);
            return shfi.hIcon;
        }
    }
}
