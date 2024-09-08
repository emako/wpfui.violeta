using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Wpf.Ui.Violeta.Controls;

public class AnimatedImage : Image, IDisposable
{
    public static List<KeyValuePair<string[], Type>> Providers { get; } = [];

    private AnimationProvider _animation = null!;
    private bool _disposing;

    static AnimatedImage()
    {
        Providers.Add(
            new KeyValuePair<string[], Type>(
                [".apng", ".png", ".jpg", ".jpeg", ".bmp"],
                typeof(BitmapImageProvider)));
    }

    public void Dispose()
    {
        _disposing = true;

        BeginAnimation(AnimationFrameIndexProperty, null);
        Source = null;

        _animation?.Dispose();
        _animation = null!;
    }

    public event EventHandler? ImageLoaded;

    public event EventHandler? DoZoomToFit;

    private static AnimationProvider InitAnimationProvider(Uri path)
    {
        var ext = Path.GetExtension(path.LocalPath).ToLower();
        var type = Providers.First(p => p.Key.Contains(ext) || p.Key.Contains("*")).Value;

        var provider = type.CreateInstance<AnimationProvider>(path);

        return provider;
    }

    public static readonly DependencyProperty AnimationFrameIndexProperty =
        DependencyProperty.Register(nameof(AnimationFrameIndex), typeof(int), typeof(AnimatedImage), new UIPropertyMetadata(-1, AnimationFrameIndexChanged));

    public static readonly DependencyProperty AnimationUriProperty =
        DependencyProperty.Register(nameof(AnimationUri), typeof(Uri), typeof(AnimatedImage), new UIPropertyMetadata(null, AnimationUriChanged));

    public int AnimationFrameIndex
    {
        get => (int)GetValue(AnimationFrameIndexProperty);
        set => SetValue(AnimationFrameIndexProperty, value);
    }

    public Uri AnimationUri
    {
        get => (Uri)GetValue(AnimationUriProperty);
        set => SetValue(AnimationUriProperty, value);
    }

    private static void AnimationUriChanged(DependencyObject obj, DependencyPropertyChangedEventArgs ev)
    {
        if (obj is not AnimatedImage instance)
        {
            return;
        }

        instance._animation = InitAnimationProvider((Uri)ev.NewValue);
        ShowThumbnailAndStartAnimation(instance);
    }

    private static void ShowThumbnailAndStartAnimation(AnimatedImage instance)
    {
        instance.Dispatcher.Invoke(() =>
        {
            if (instance._disposing)
            {
                return;
            }

            instance.DoZoomToFit?.Invoke(instance, new EventArgs());
            instance.ImageLoaded?.Invoke(instance, new EventArgs());

            instance.BeginAnimation(AnimationFrameIndexProperty, instance._animation?.Animator);
        });
    }

    private static void AnimationFrameIndexChanged(DependencyObject obj, DependencyPropertyChangedEventArgs ev)
    {
        if (obj is not AnimatedImage instance)
        {
            return;
        }

        if (instance._disposing)
        {
            return;
        }

        var task = instance._animation.GetRenderedFrame((int)ev.NewValue);

        task.ContinueWith(_ => instance.Dispatcher.Invoke(() =>
        {
            if (instance._disposing)
                return;

            var firstLoad = instance.Source == null;

            instance.Source = _.Result;

            if (firstLoad)
            {
                instance.DoZoomToFit?.Invoke(instance, new EventArgs());
                instance.ImageLoaded?.Invoke(instance, new EventArgs());
            }
        }));
    }
}

file static class TypeExtensions
{
    public static T CreateInstance<T>(this Type t, params object[] paramArray)
    {
        return (T)Activator.CreateInstance(t, paramArray)!;
    }
}
