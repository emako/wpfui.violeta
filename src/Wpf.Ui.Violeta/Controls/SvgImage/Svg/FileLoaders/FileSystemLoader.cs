using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Wpf.Ui.Violeta.Controls.Svg.FileLoaders;

public sealed class FileSystemLoader : IExternalFileLoader
{
    static FileSystemLoader()
    {
        Instance = new FileSystemLoader();
    }

    public static FileSystemLoader Instance { get; }

    public Stream LoadFile(string hRef, string svgFilename)
    {
        var path = Environment.CurrentDirectory;
        if (!string.IsNullOrEmpty(svgFilename))
        {
            path = Path.GetDirectoryName(svgFilename);
        }
        string filename = Path.Combine(path!, hRef);
        if (File.Exists(filename))
            return File.OpenRead(filename);

        // For the issue #43 : Environment.CurrentDirectory prevents msix packaging.
        // Prefer entry-assembly directory when available; fall back for single-file (IL3000).
        path = GetAppDirectory();
        filename = Path.Combine(path, hRef);
        if (File.Exists(filename))
            return File.OpenRead(filename);

        Trace.TraceWarning("Unresolved URI: " + hRef);

        return null!;
    }

    private static string GetAppDirectory()
    {
        var entry = Assembly.GetEntryAssembly();
        if (entry != null)
        {
#pragma warning disable IL3000 // Location is empty for single-file; BaseDirectory used below
            var location = entry.Location;
#pragma warning restore IL3000
            if (!string.IsNullOrEmpty(location))
            {
                var directory = Path.GetDirectoryName(location);
                if (!string.IsNullOrEmpty(directory))
                    return directory;
            }
        }

        return AppContext.BaseDirectory;
    }
}
