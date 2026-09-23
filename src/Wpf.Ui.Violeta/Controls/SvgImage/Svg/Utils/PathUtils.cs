using System;
using System.IO;
using System.Reflection;

namespace Wpf.Ui.Violeta.Controls.Svg.Utils;

/// <summary>
/// Extends functionality of <see cref="Path"/> and provides additional methods for manipulating file or directory path information.
/// </summary>
internal static class PathUtils
{
    /// <summary>
    /// Combines an assembly location and an array of strings into a path.
    /// </summary>
    /// <param name="assembly">An <see cref="Assembly"/> which is taken as the base path.</param>
    /// <param name="paths">Path segments which are appended to the assembly location.</param>
    /// <returns>A string containing the combined path.</returns>
    public static string Combine(Assembly assembly, params string[] paths)
    {
#pragma warning disable IL3000 // Location is empty for single-file; fall back to BaseDirectory
        var location = assembly.Location;
#pragma warning restore IL3000
        var basePath = string.IsNullOrEmpty(location)
            ? GetBaseDirectory()
            : Path.GetDirectoryName(location);

        if (paths.Length == 0)
            return basePath!;

        var newPaths = new string[paths.Length + 1];
        Array.Copy(paths, 0, newPaths, 1, paths.Length);
        newPaths[0] = basePath!;

        return Path.Combine(newPaths);
    }

    /// <summary>
    /// Gets the full path to the assembly file.
    /// </summary>
    /// <param name="assembly">An <see cref="Assembly"/> which is taken as the base path.</param>
    /// <returns>A string containing the full path to the assembly file.</returns>
    public static string GetAssemblyPath(Assembly assembly)
    {
#pragma warning disable IL3000 // Location is empty for single-file; fall back to BaseDirectory
        var location = assembly.Location;
#pragma warning restore IL3000
        if (!string.IsNullOrEmpty(location))
            return location;

        return Path.Combine(GetBaseDirectory(), GetAssemblyFileName(assembly));
    }

    /// <summary>
    /// Gets the file name if the assembly.
    /// </summary>
    /// <param name="assembly">An <see cref="Assembly"/> which is taken as the base path.</param>
    /// <returns>A string containing the file name of the assembly.</returns>
    public static string GetAssemblyFileName(Assembly assembly)
    {
        return assembly.ManifestModule.ScopeName;
    }

    private static string GetBaseDirectory()
    {
#if NETCORE
        return AppContext.BaseDirectory;
#else
		return AppDomain.CurrentDomain.BaseDirectory;
#endif
    }
}
