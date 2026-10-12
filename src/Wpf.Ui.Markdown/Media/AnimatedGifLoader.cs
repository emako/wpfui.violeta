using System.IO;
using System.Windows;
using Wpf.Ui.Markdown.Plugins;
using Wpf.Ui.Violeta.Controls;

namespace Wpf.Ui.Markdown.Media;

/// <summary>
/// Plays GIF images through a Violeta <see cref="GifImage"/>.
/// </summary>
public class AnimatedGifLoader : IElementLoader, IPreferredLoader
{
    public FrameworkElement? Load(Stream stream)
    {
        if (!IsGif(stream))
            return null;

        stream.Position = 0;
        return new GifImage(stream);
    }

    private static bool IsGif(Stream stream)
    {
        var header = new byte[6];
        var read = 0;
        while (read < header.Length)
        {
            var count = stream.Read(header, read, header.Length - read);
            if (count == 0)
                return false;
            read += count;
        }

        return header[0] == 'G' && header[1] == 'I' && header[2] == 'F'
            && header[3] == '8' && (header[4] == '7' || header[4] == '9') && header[5] == 'a';
    }
}
