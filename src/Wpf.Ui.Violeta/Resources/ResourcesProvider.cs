using System;
using System.IO;
using System.IO.Packaging;
using System.Text;
using System.Windows;
using System.Windows.Resources;

namespace Wpf.Ui.Violeta.Resources;

/// <summary>
/// Provides helper utilities for accessing application resources via pack URIs.
/// This class offers methods to check existence, open streams and read
/// resource contents as strings or byte arrays. It wraps WPF's
/// <see cref="Application.GetResourceStream"/> and handles common patterns
/// for resource access.
/// </summary>
public static class ResourcesProvider
{
    static ResourcesProvider()
    {
        // Ensure the "pack" URI scheme is registered before any pack URI
        // operations are attempted. Accessing PackUriHelper.UriSchemePack
        // forces the registration if it hasn't happened yet.
        if (!UriParser.IsKnownScheme("pack"))
        {
            _ = PackUriHelper.UriSchemePack;
        }
    }

    /// <summary>
    /// Low-performance check to determine whether a resource exists for the
    /// specified <paramref name="uri"/>. This method attempts to open the
    /// resource and returns <c>true</c> if successful; otherwise <c>false</c>.
    /// </summary>
    /// <param name="uri">The pack or resource <see cref="Uri"/> to check.</param>
    /// <returns><c>true</c> when the resource can be opened; otherwise <c>false</c>.</returns>
    /// <remarks>
    /// This method can be slow because it tries to open the resource stream.
    /// It swallows exceptions and should be used for best-effort existence checks only.
    /// </remarks>
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

    /// <summary>
    /// Checks whether a resource exists for the specified URI string.
    /// </summary>
    /// <param name="uriString">The pack or resource URI as a string.</param>
    /// <returns><c>true</c> when the resource can be opened; otherwise <c>false</c>.</returns>
    public static bool HasResource(string uriString)
    {
        return HasResource(new Uri(uriString));
    }

    /// <summary>
    /// Opens and returns a <see cref="Stream"/> for the specified resource
    /// <paramref name="uri"/>. Throws an exception if the resource cannot be found.
    /// </summary>
    /// <param name="uri">The resource <see cref="Uri"/> to open.</param>
    /// <returns>A <see cref="Stream"/> for the requested resource.</returns>
    public static Stream GetStream(Uri uri)
    {
        StreamResourceInfo info = Application.GetResourceStream(uri);
        return info?.Stream!;
    }

    /// <summary>
    /// Opens and returns a <see cref="Stream"/> for the specified resource URI string.
    /// Throws an exception if the resource cannot be found.
    /// </summary>
    /// <param name="uriString">The resource URI as a string.</param>
    /// <returns>A <see cref="Stream"/> for the requested resource.</returns>
    public static Stream GetStream(string uriString)
    {
        return GetStream(new Uri(uriString));
    }

    /// <summary>
    /// Attempts to open a <see cref="Stream"/> for the specified resource <paramref name="uri"/>.
    /// Returns <c>null</c> if the resource cannot be found.
    /// </summary>
    /// <param name="uri">The resource <see cref="Uri"/> to open.</param>
    /// <returns>A <see cref="Stream"/> for the resource, or <c>null</c> if not found.</returns>
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

    /// <summary>
    /// Attempts to open a <see cref="Stream"/> for the specified resource URI string.
    /// Returns <c>null</c> if the resource cannot be found.
    /// </summary>
    /// <param name="uriString">The resource URI as a string.</param>
    /// <returns>A <see cref="Stream"/> for the resource, or <c>null</c> if not found.</returns>
    public static Stream? TryFindStream(string uriString)
    {
        return TryFindStream(new Uri(uriString));
    }

    /// <summary>
    /// Attempts to open a <see cref="Stream"/> for the specified resource <paramref name="uri"/>.
    /// Returns <c>true</c> if successful, with the stream in <paramref name="stream"/>;
    /// otherwise returns <c>false</c> and sets <paramref name="stream"/> to <c>null</c>.
    /// </summary>
    /// <param name="uri">The resource <see cref="Uri"/> to open.</param>
    /// <param name="stream">When this method returns, contains the opened stream if successful; otherwise <c>null</c>.</param>
    /// <returns><c>true</c> if the stream was opened; otherwise <c>false</c>.</returns>
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

    /// <summary>
    /// Attempts to open a <see cref="Stream"/> for the specified resource URI string.
    /// Returns <c>true</c> if successful, with the stream in <paramref name="stream"/>;
    /// otherwise returns <c>false</c> and sets <paramref name="stream"/> to <c>null</c>.
    /// </summary>
    /// <param name="uriString">The resource URI as a string.</param>
    /// <param name="stream">When this method returns, contains the opened stream if successful; otherwise <c>null</c>.</param>
    /// <returns><c>true</c> if the stream was opened; otherwise <c>false</c>.</returns>
    public static bool TryGetStream(string uriString, out Stream? stream)
    {
        return TryGetStream(new Uri(uriString), out stream);
    }

    /// <summary>
    /// Reads the resource at <paramref name="uri"/> and returns its text
    /// content. The optional <paramref name="encoding"/> defaults to UTF-8.
    /// </summary>
    /// <param name="uri">The resource <see cref="Uri"/> to read.</param>
    /// <param name="encoding">Optional text encoding; defaults to UTF-8.</param>
    /// <returns>The resource content as a string.</returns>
    public static string GetString(Uri uri, Encoding? encoding = null)
    {
        using Stream stream = GetStream(uri);
        using StreamReader streamReader = new(stream, encoding ?? Encoding.UTF8);
        return streamReader.ReadToEnd();
    }

    /// <summary>
    /// Reads the resource at the specified URI string and returns its text content.
    /// The optional <paramref name="encoding"/> defaults to UTF-8.
    /// </summary>
    /// <param name="uriString">The resource URI as a string.</param>
    /// <param name="encoding">Optional text encoding; defaults to UTF-8.</param>
    /// <returns>The resource content as a string.</returns>
    public static string GetString(string uriString, Encoding? encoding = null)
    {
        return GetString(new Uri(uriString), encoding);
    }

    /// <summary>
    /// Reads the resource at <paramref name="uri"/> and returns its raw
    /// bytes. Be aware that this method reads the entire stream into memory.
    /// </summary>
    /// <param name="uri">The resource <see cref="Uri"/> to read.</param>
    /// <returns>A byte array containing the resource data.</returns>
    public static byte[] GetBytes(Uri uri)
    {
        using Stream stream = GetStream(uri);
        using BinaryReader streamReader = new(stream);
        return streamReader.ReadBytes((int)stream.Length);
    }

    /// <summary>
    /// Reads the resource at the specified URI string and returns its raw bytes.
    /// </summary>
    /// <param name="uriString">The resource URI as a string.</param>
    /// <returns>A byte array containing the resource data.</returns>
    public static byte[] GetBytes(string uriString)
    {
        return GetBytes(new Uri(uriString));
    }
}
