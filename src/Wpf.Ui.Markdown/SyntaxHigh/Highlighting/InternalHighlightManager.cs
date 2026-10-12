using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using Wpf.Ui.Markdown.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#if NETFRAMEWORK
using System.Net;
#else
using System.Net.Http;
#endif
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace Wpf.Ui.Markdown.Highlighting;

internal class InternalHighlightManager
{
    private static Dictionary<string, IHighlightingDefinition> s_builtin;
    private static Dictionary<Uri, IHighlightingDefinition> s_cache;

    static InternalHighlightManager()
    {
        s_builtin = new();
        s_cache = new();

        Register("ASP/XHTML", "asp/xhtml");
        Register("Boo", "boo");
        Register("Coco", "coco");
        Register("C++", "c++");
        Register("C#", "csharp", "c#");
        Register("JavaScript", "javascript");
        Register("HTML", "html");
        Register("MarkDown", "markdown");
        Register("PowerShell", "powershell", "posh", "microsoftshell", "msshel");
        Register("Python", "python");
        Register("TSQL", "tsql");

        void Register(string basename, params string[] aliases)
        {
            if (HighlightingManager.Instance.GetDefinition(basename) is { } def)
            {
                foreach (var name in aliases)
                    s_builtin[name] = def;
            }
        }
    }

    private static IHighlightingDefinition Load(Uri path)
    {
        if (s_cache.TryGetValue(path, out var def))
            return def;

        using Stream input = Open(path);
        using XmlTextReader reader = new XmlTextReader(input);

        return s_cache[path] = HighlightingLoader.Load(reader, HighlightingManager.Instance);
    }

#if !NETFRAMEWORK
    public async static Task<MemoryStream> DownloadTextAsync(Uri path)
    {
        var httpClient = new HttpClient();
        var data = httpClient.GetByteArrayAsync(path).GetAwaiter().GetResult();
        return new MemoryStream(data);
    }
#endif

    private static Stream Open(Uri path)
    {
        switch (path.Scheme)
        {
#if NETFRAMEWORK
            case "http":
            case "https":
                using (var wc = new WebClient())
                    return new MemoryStream(wc.DownloadData(path));
#else
            case "http":
            case "https":
                return DownloadTextAsync(path).GetAwaiter().GetResult();
#endif
            case "file":
                return File.OpenRead(path.LocalPath);

            case "pack":
                return Application.GetResourceStream(path).Stream;

            default:
                throw new ArgumentException($"unsupport schema {path.Scheme}");
        }
    }


    public static IHighlightingDefinition? Find(string langcode)
    {
        if (MarkdownCustomHighlighting.HighlightingResolver?.Invoke(langcode) is { } custom)
            return custom;

        if (s_builtin.TryGetValue(langcode.ToLower(), out var builtin))
            return builtin;

        return HighlightingManager.Instance.GetDefinition(langcode)
            ?? HighlightingManager.Instance.GetDefinitionByExtension("." + langcode);
    }

    private Dictionary<string, IHighlightingDefinition> _definitions = new(s_builtin);


    public void Register(Definition def)
    {
        if (def.Alias is null)
            throw new ArgumentException("Definition.Alias must not be null.");

        if ((def.Resource is not null && def.RealName is not null))
            throw new ArgumentException("Only one of Definition.Resource and Definition.RealName must be set.");

        IHighlightingDefinition definition;
        if (def.Resource is not null)
            definition = Load(def.Resource);

        else if (def.RealName is not null)
            if (Get(def.RealName) is { } d)
                definition = d;
            else
                throw new ArgumentException($"syntax highlighting `{def.RealName}` is not found.");

        else
            throw new ArgumentException("Only one of Definition.Resource and Definition.RealName must be set.");


        foreach (var alias in def.Alias.Split(','))
        {
            _definitions[alias] = definition;
        }
    }

    public IHighlightingDefinition? Get(string langcode)
    {
        // If provided, try the method of customized syntax highlighting first.
        if (MarkdownCustomHighlighting.HighlightingResolver != null)
        {
            var def = MarkdownCustomHighlighting.HighlightingResolver(langcode);
            if (def != null)
            {
                return def;
            }
        }

        var r = GetHighlight(langcode);
        if(r is not null)
        {
            return r;
        }

        return
            HighlightingManager.Instance.GetDefinitionByExtension("." + langcode);
    }

    private IHighlightingDefinition? GetHighlight(string langcode)
        => _definitions.TryGetValue(langcode.ToLower(), out var reged) ? reged : null;
}
