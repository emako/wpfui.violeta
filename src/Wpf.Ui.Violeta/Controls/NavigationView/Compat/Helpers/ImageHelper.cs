using System;
using System.Windows.Media.Imaging;

namespace Wpf.Ui.Violeta.Controls.Compat;

public static class ImageHelper
{
    public static BitmapImage? LoadImage(string? uri, int decodeWidth = 512)
    {
        if (uri == null)
            return null;

        BitmapImage bmp = new()
        {
            DecodePixelHeight = 250,
        };
        bmp.BeginInit();
        bmp.CreateOptions = BitmapCreateOptions.DelayCreation;
        bmp.DecodePixelWidth = decodeWidth;
        bmp.CacheOption = BitmapCacheOption.OnLoad;
        bmp.UriSource = new Uri(uri);

        bmp.EndInit();

        if (bmp.CanFreeze)
            bmp.Freeze();

        return bmp;
    }
}
