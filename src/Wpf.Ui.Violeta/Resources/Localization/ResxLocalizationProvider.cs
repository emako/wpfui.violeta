using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Windows;
using System.Windows.Resources;
using System.Xml;

namespace Wpf.Ui.Violeta.Resources.Localization;

/// <summary>
/// Localizes by reading .resx XML exposed as WPF pack resources (Resource/Content build actions).
/// </summary>
/// <param name="baseResourcePath">Relative path under the assembly (e.g., <c>Resources/Localization/</c>).</param>
/// <param name="assemblyName">Assembly name used in <c>pack://application</c> URIs (e.g., <c>Wpf.Ui.Violeta</c>).</param>
internal sealed class ResxLocalizationProvider(string baseResourcePath, string assemblyName) : ILocalizationProvider
{
    private readonly string _baseResourcePath = baseResourcePath;
    private readonly string _assemblyName = assemblyName;
    private readonly Dictionary<string, Dictionary<string, string>?> _cache = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Dictionary<string, string>> _resolvedCache = new(StringComparer.OrdinalIgnoreCase);

    // Optional prewarm list to reduce I/O when first accessed for invariant culture.
    private static readonly string[] KnownCultures =
    [
        string.Empty, "fr", "id", "ja", "ko", "pt", "ru", "vi", "de", "zh-Hans", "zh-Hant"
    ];

    /// <summary>
    /// Gets or sets the culture used when callers pass null to <see cref="Get"/>.
    /// </summary>
    public CultureInfo Culture { get; set; } = CultureInfo.CurrentUICulture;

    /// <summary>
    /// Returns a localized value for the given key and culture.
    /// </summary>
    /// <param name="key">Resource key.</param>
    /// <param name="culture">Culture to resolve; null uses <see cref="Culture"/>.</param>
    /// <returns>The localized string or null if not found.</returns>
    public string? Get(string key, CultureInfo? culture = null)
    {
        culture ??= Culture;
        Dictionary<string, string> resolved = GetResolvedMap(culture);
        return resolved.TryGetValue(key, out string? value) ? value : null;
    }

    /// <summary>
    /// Clears all caches (per-culture maps and merged maps).
    /// </summary>
    public void Invalidate()
    {
        _cache.Clear();
        _resolvedCache.Clear();
    }

    private static IEnumerable<CultureInfo> EnumerateFallback(CultureInfo culture)
    {
        CultureInfo current = culture;
        while (!string.IsNullOrEmpty(current.Name))
        {
            yield return current;
            current = current.Parent;
        }

        yield return CultureInfo.InvariantCulture;
    }

    /// <summary>
    /// Loads and parses the resx for a specific culture (no fallback), then caches it.
    /// </summary>
    private Dictionary<string, string>? LoadCultureMap(CultureInfo culture)
    {
        if (_cache.TryGetValue(culture.Name, out Dictionary<string, string>? cached))
        {
            return cached;
        }

        string resourceName = (string.IsNullOrEmpty(culture.Name) || culture.Name is "en")
            ? $"{_baseResourcePath}SH.resx"
            : $"{_baseResourcePath}SH.{culture.Name}.resx";

        using Stream? stream = OpenResxStream(resourceName, _assemblyName);
        if (stream == null)
        {
            _cache[culture.Name] = null;
            return null;
        }

        using XmlReader reader = XmlReader.Create(stream, new XmlReaderSettings { IgnoreComments = true, IgnoreWhitespace = true });
        Dictionary<string, string> dict = new(StringComparer.Ordinal);

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

    /// <summary>
    /// Gets a merged map for a culture including its fallback chain (child wins), cached per culture.
    /// </summary>
    private Dictionary<string, string> GetResolvedMap(CultureInfo culture)
    {
        if (_resolvedCache.TryGetValue(culture.Name, out Dictionary<string, string>? resolved))
        {
            return resolved;
        }

        // Attempt prewarm for known cultures when asked for invariant.
        if (string.IsNullOrEmpty(culture.Name))
        {
            foreach (var name in KnownCultures)
            {
                _ = LoadCultureMap(new CultureInfo(name == string.Empty ? string.Empty : name));
            }
        }

        Dictionary<string, string> merged = new(StringComparer.Ordinal);
        foreach (CultureInfo c in EnumerateFallback(culture))
        {
            Dictionary<string, string>? map = LoadCultureMap(c);
            if (map == null)
            {
                continue;
            }

            foreach (KeyValuePair<string, string> pair in map)
            {
                // Child culture wins; only fill missing entries.
                if (!merged.ContainsKey(pair.Key))
                {
                    merged[pair.Key] = pair.Value;
                }
            }
        }

        _resolvedCache[culture.Name] = merged;
        return merged;
    }

    /// <summary>
    /// Opens a resx stream via pack URIs or file fallback.
    /// </summary>
    private static Stream? OpenResxStream(string resourcePath, string assemblyName)
    {
        // Try application-scope pack URI (for Resource build action).
        Uri appUri = new($"pack://application:,,,/{assemblyName};component/{resourcePath}", UriKind.Absolute);

        Stream? stream = TryGet(appUri);
        if (stream != null)
        {
            return stream;
        }

        // Try site-of-origin (for Content copied to output/publish).
        Uri sooUri = new($"pack://siteoforigin:,,,/{resourcePath}", UriKind.Absolute);
        stream = TryGet(sooUri);
        if (stream != null)
        {
            return stream;
        }

        // Final fallback: relative file path next to app base (useful for tests).
        string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, resourcePath.Replace('/', System.IO.Path.DirectorySeparatorChar));
        return File.Exists(filePath) ? File.OpenRead(filePath) : null;

        static Stream? TryGet(Uri uri)
        {
            try
            {
                StreamResourceInfo? info = Application.GetResourceStream(uri);
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

    /// <inheritdoc />
    public override string? GetString(string name, CultureInfo? culture)
    {
        return _provider.Get(name, culture) ?? string.Empty;
    }
}

/// <summary>
/// Abstraction for localization providers used by the strongly typed SH class.
/// </summary>
internal interface ILocalizationProvider
{
    /// <summary>Gets a localized value for the given key and culture.</summary>
    public string? Get(string key, CultureInfo? culture = null);

    /// <summary>Gets or sets the default culture used when no culture is supplied.</summary>
    public CultureInfo Culture { get; set; }

    /// <summary>Clears any cached data.</summary>
    public void Invalidate();
}
