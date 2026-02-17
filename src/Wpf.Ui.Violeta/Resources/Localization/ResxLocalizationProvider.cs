using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Windows;
using System.Xml;

namespace Wpf.Ui.Violeta.Resources.Localization;

// Reads .resx XML via WPF pack:// resources (Build Action: Content/Resource).
internal sealed class ResxLocalizationProvider(string baseResourcePath, string assemblyName) : ILocalizationProvider
{
    private readonly string _baseResourcePath = baseResourcePath;
    private readonly string _assemblyName = assemblyName;
    private readonly Dictionary<string, Dictionary<string, string>?> _cache = new(StringComparer.OrdinalIgnoreCase);

    public CultureInfo Culture { get; set; } = CultureInfo.CurrentUICulture;

    public string? Get(string key, CultureInfo? culture = null)
    {
        culture ??= Culture;
        foreach (var c in EnumerateFallback(culture))
        {
            var map = LoadCultureMap(c);
            if (map != null && map.TryGetValue(key, out var value))
            {
                return value;
            }
        }

        return null;
    }

    public void Invalidate()
    {
        _cache.Clear();
    }

    private static IEnumerable<CultureInfo> EnumerateFallback(CultureInfo culture)
    {
        var current = culture;
        while (!string.IsNullOrEmpty(current.Name))
        {
            yield return current;
            current = current.Parent;
        }

        yield return CultureInfo.InvariantCulture;
    }

    private Dictionary<string, string>? LoadCultureMap(CultureInfo culture)
    {
        if (_cache.TryGetValue(culture.Name, out var cached))
        {
            return cached;
        }

        var resourceName = (string.IsNullOrEmpty(culture.Name) || culture.Name is "en")
            ? $"{_baseResourcePath}SH.resx"
            : $"{_baseResourcePath}SH.{culture.Name}.resx";

        using var stream = OpenResxStream(resourceName, _assemblyName);
        if (stream == null)
        {
            _cache[culture.Name] = null;
            return null;
        }

        using var reader = XmlReader.Create(stream, new XmlReaderSettings { IgnoreComments = true, IgnoreWhitespace = true });
        var dict = new Dictionary<string, string>(StringComparer.Ordinal);

        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element && reader.Name == "data")
            {
                var key = reader.GetAttribute("name");
                if (string.IsNullOrEmpty(key))
                {
                    continue;
                }

                string? value = null;
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "value")
                    {
                        value = reader.ReadElementContentAsString();
                        break;
                    }

                    if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "data")
                    {
                        break;
                    }
                }

                if (value != null)
                {
                    dict[key] = value;
                }
            }
        }

        _cache[culture.Name] = dict;
        return dict;
    }

    private static Stream? OpenResxStream(string resourcePath, string assemblyName)
    {
        // Try application-scope pack URI (for Resource build action).
        var appUri = new Uri($"pack://application:,,,/{assemblyName};component/{resourcePath}", UriKind.Absolute);

        var stream = TryGet(appUri);
        if (stream != null)
        {
            return stream;
        }

        // Try site-of-origin (for Content copied to output/publish).
        var sooUri = new Uri($"pack://siteoforigin:,,,/{resourcePath}", UriKind.Absolute);
        stream = TryGet(sooUri);
        if (stream != null)
        {
            return stream;
        }

        // Final fallback: relative file path next to app base (useful for tests).
        var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, resourcePath.Replace('/', System.IO.Path.DirectorySeparatorChar));
        return File.Exists(filePath) ? File.OpenRead(filePath) : null;

        static Stream? TryGet(Uri uri)
        {
            try
            {
                var info = Application.GetResourceStream(uri);
                return info?.Stream;
            }
            catch
            {
            }
            return null;
        }
    }
}

// Simple ResourceManager wrapper to preserve SH API surface.
internal sealed class ResxResourceManager(ILocalizationProvider provider) : ResourceManager
{
    private readonly ILocalizationProvider _provider = provider;

    public override string? GetString(string name, CultureInfo? culture)
    {
        return _provider.Get(name, culture) ?? string.Empty;
    }
}

internal interface ILocalizationProvider
{
    public string? Get(string key, CultureInfo? culture = null);

    public CultureInfo Culture { get; set; }

    public void Invalidate();
}
