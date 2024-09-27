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

    public static bool HasResource(string uriString)
    {
        try
        {
            Uri uri = new(uriString);
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

    public static Stream GetStream(string uriString)
    {
        Uri uri = new(uriString);
        StreamResourceInfo info = Application.GetResourceStream(uri);
        return info?.Stream!;
    }

    public static bool TryGetStream(string uriString, out Stream? stream)
    {
        try
        {
            stream = GetStream(uriString);
            return true;
        }
        catch
        {
        }
        stream = null;
        return false;
    }

    public static Stream? TryFindStream(string uriString)
    {
        try
        {
            return GetStream(uriString);
        }
        catch
        {
        }
        return null;
    }

    public static string GetString(string uriString, Encoding? encoding = null)
    {
        using Stream stream = GetStream(uriString);
        using StreamReader streamReader = new(stream, encoding ?? Encoding.UTF8);
        return streamReader.ReadToEnd();
    }

    public static byte[] GetBytes(string uriString)
    {
        using Stream stream = GetStream(uriString);
        using BinaryReader streamReader = new(stream);
        return streamReader.ReadBytes((int)stream.Length);
    }
}
