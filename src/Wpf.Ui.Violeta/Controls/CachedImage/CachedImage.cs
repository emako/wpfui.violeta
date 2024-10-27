using System;
using System.Net.Cache;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Represents a control that is a wrapper on System.Windows.Controls.Image for enabling filesystem-based caching
/// </summary>
public class CachedImage : System.Windows.Controls.Image
{
    static CachedImage()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(CachedImage), new FrameworkPropertyMetadata(typeof(CachedImage)));
    }

    public string ImageUrl
    {
        get => (string)GetValue(ImageUrlProperty);
        set => SetValue(ImageUrlProperty, value);
    }

    public static readonly DependencyProperty ImageUrlProperty =
        DependencyProperty.Register(nameof(ImageUrl), typeof(string), typeof(CachedImage), new(string.Empty, ImageUrlPropertyChanged));

    public BitmapCreateOptions CreateOptions
    {
        get => (BitmapCreateOptions)GetValue(CreateOptionsProperty);
        set => SetValue(CreateOptionsProperty, value);
    }

    public static readonly DependencyProperty CreateOptionsProperty =
        DependencyProperty.Register(nameof(CreateOptions), typeof(BitmapCreateOptions), typeof(CachedImage));

    private static async void ImageUrlPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        var url = e.NewValue as string;

        if (string.IsNullOrEmpty(url))
            return;

        var cachedImage = (CachedImage)obj;
        var bitmapImage = new BitmapImage();

        switch (FileCacheForImage.AppCacheMode)
        {
            case FileCacheForImage.CacheMode.WinINet:
                bitmapImage.BeginInit();
                bitmapImage.CreateOptions = cachedImage.CreateOptions;
                bitmapImage.UriSource = new Uri(url);
                // Enable IE-like cache policy.
                bitmapImage.UriCachePolicy = new RequestCachePolicy(RequestCacheLevel.Default);
                bitmapImage.EndInit();
                cachedImage.Source = bitmapImage;
                break;

            case FileCacheForImage.CacheMode.Dedicated:
                try
                {
                    var memoryStream = await FileCacheForImage.HitAsync(url!);
                    if (memoryStream == null)
                        return;

                    bitmapImage.BeginInit();
                    bitmapImage.CreateOptions = cachedImage.CreateOptions;
                    bitmapImage.StreamSource = memoryStream;
                    bitmapImage.EndInit();
                    cachedImage.Source = bitmapImage;
                }
                catch (Exception)
                {
                    // ignored, in case the downloaded file is a broken or not an image.
                }
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
