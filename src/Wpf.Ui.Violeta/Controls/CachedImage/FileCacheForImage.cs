﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Wpf.Ui.Violeta.Controls;

public static class FileCacheForImage
{
    public enum CacheMode
    {
        WinINet,
        Dedicated
    }

    // Record whether a file is being written.
    private static readonly Dictionary<string, bool> IsWritingFile = [];

    // Timeout for performing the file download request.
    private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(5);

    // HttpClient is intended to be instantiated once per application, rather than per-use.
    private static readonly Lazy<HttpClient> LazyHttpClient = new(() => new HttpClient());

    static FileCacheForImage()
    {
        // default cache directory - can be changed if needed from App.xaml
        AppCacheDirectory = string.Format("{0}\\{1}\\Cache\\",
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            Process.GetCurrentProcess().ProcessName);
        AppCacheMode = CacheMode.WinINet;
    }

    /// <summary>
    /// Gets or sets the path to the folder that stores the cache file. Only works when AppCacheMode is
    /// CacheMode.Dedicated.
    /// </summary>
    public static string AppCacheDirectory { get; set; }

    /// <summary>
    /// Gets or sets the cache mode. WinINet is recommended, it's provided by .Net Framework and uses the Temporary Files
    /// of IE and the same cache policy of IE.
    /// </summary>
    public static CacheMode AppCacheMode { get; set; }

    public static async Task<MemoryStream> HitAsync(string url)
    {
        if (!Directory.Exists(AppCacheDirectory))
        {
            Directory.CreateDirectory(AppCacheDirectory);
        }
        var uri = new Uri(url);
        var fileNameBuilder = new StringBuilder();
        using (SHA1 sha1 = SHA1.Create())
        {
            var canonicalUrl = uri.ToString();
            var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(canonicalUrl));
            fileNameBuilder.Append(BitConverter.ToString(hash).Replace("-", "").ToLower());
            if (Path.HasExtension(canonicalUrl))
            {
                fileNameBuilder.Append(Path.GetExtension(canonicalUrl).Split('?')[0]);
            }
        }

        var fileName = fileNameBuilder.ToString();
        var localFile = string.Format("{0}\\{1}", AppCacheDirectory, fileName);
        var memoryStream = new MemoryStream();

        FileStream fileStream = null!;
        if (!IsWritingFile.ContainsKey(fileName) && File.Exists(localFile))
        {
            using (fileStream = new FileStream(localFile, FileMode.Open, FileAccess.Read))
            {
                await fileStream.CopyToAsync(memoryStream);
            }
            memoryStream.Seek(0, SeekOrigin.Begin);
            return memoryStream;
        }

        var client = LazyHttpClient.Value;
        client.Timeout = RequestTimeout;
        try
        {
            var response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);

            if (response.IsSuccessStatusCode is false)
            {
                return null!;
            }

            var responseStream = await response.Content.ReadAsStreamAsync();

            if (!IsWritingFile.ContainsKey(fileName))
            {
                IsWritingFile[fileName] = true;
                fileStream = new FileStream(localFile, FileMode.Create, FileAccess.Write);
            }

            using (responseStream)
            {
                var bytebuffer = new byte[100];
                int bytesRead;
                do
                {
                    bytesRead = await responseStream.ReadAsync(bytebuffer, 0, 100);
                    if (fileStream != null)
                    {
                        await fileStream.WriteAsync(bytebuffer, 0, bytesRead);
                    }

                    await memoryStream.WriteAsync(bytebuffer, 0, bytesRead);
                } while (bytesRead > 0);
                if (fileStream != null)
                {
                    await fileStream.FlushAsync();
                    fileStream.Dispose();
                    IsWritingFile.Remove(fileName);
                }
            }
            memoryStream.Seek(0, SeekOrigin.Begin);
            return memoryStream;
        }
        catch (WebException)
        {
            return null!;
        }
    }
}
