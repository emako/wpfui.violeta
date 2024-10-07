using System;
using System.IO;
using System.IO.Packaging;
using System.Text;
using System.Windows;
using System.Windows.Resources;

namespace Wpf.Ui.Violeta.Resources;

public static class ResourcesProvider
{
    static ResourcesProvider()
    {
        if (!UriParser.IsKnownScheme("pack"))
        {
            _ = PackUriHelper.UriSchemePack;
        }
    }

    /// <summary>
    /// Low performance method to check if a resource exists.
    /// </summary>
    /// <param name="uri"></param>
    /// <returns></returns>
    public static bool HasResource(Uri uri)
    {
        try
        {
            StreamResourceInfo info = Application.GetResourceStream(uri);
            using Stream? stream = info?.Stream;
            _ = stream;
            return true;
        }
        catch
        {
        }
        return false;
    }

    public static bool HasResource(string uriString)
    {
        return HasResource(new Uri(uriString));
    }

    public static Stream GetStream(Uri uri)
    {
        StreamResourceInfo info = Application.GetResourceStream(uri);
        return info?.Stream!;
    }

    public static Stream GetStream(string uriString)
    {
        return GetStream(new Uri(uriString));
    }

    public static Stream? TryFindStream(Uri uri)
    {
        try
        {
            return GetStream(uri);
        }
        catch
        {
        }
        return null;
    }

    public static Stream? TryFindStream(string uriString)
    {
        return TryFindStream(new Uri(uriString));
    }

    public static bool TryGetStream(Uri uri, out Stream? stream)
    {
        try
        {
            stream = GetStream(uri);
            return true;
        }
        catch
        {
        }
        stream = null;
        return false;
    }

    public static bool TryGetStream(string uriString, out Stream? stream)
    {
        return TryGetStream(new Uri(uriString), out stream);
    }

    public static string GetString(Uri uri, Encoding? encoding = null)
    {
        using Stream stream = GetStream(uri);
        using StreamReader streamReader = new(stream, encoding ?? Encoding.UTF8);
        return streamReader.ReadToEnd();
    }

    public static string GetString(string uriString, Encoding? encoding = null)
    {
        return GetString(new Uri(uriString), encoding);
    }

    public static byte[] GetBytes(Uri uri)
    {
        using Stream stream = GetStream(uri);
        using BinaryReader streamReader = new(stream);
        return streamReader.ReadBytes((int)stream.Length);
    }

    public static byte[] GetBytes(string uriString)
    {
        return GetBytes(new Uri(uriString));
    }
}
