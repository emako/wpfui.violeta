using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Wpf.Ui.Violeta.Controls.Compat;

internal class NavigationAnimation
{
    static NavigationAnimation()
    {
        _defaultBitmapCache = new BitmapCache();
        _defaultBitmapCache.Freeze();
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public NavigationAnimation(FrameworkElement element, Storyboard storyboard)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        _element = element;
        _storyboard = storyboard;
        _storyboard.CurrentStateInvalidated += OnCurrentStateInvalidated;
        _storyboard.Completed += OnCompleted;
    }

    public event EventHandler Completed;

    public void Begin()
    {
        if (ShadowAssist.UseBitmapCache && _element.CacheMode is not BitmapCache)
        {
            _element.SetCurrentValue(UIElement.CacheModeProperty, GetBitmapCache());
        }
        _storyboard.Begin(_element, true);
    }

    public void Stop()
    {
        if (_currentState != ClockState.Stopped)
        {
            _storyboard.Stop(_element);
        }
        if (ShadowAssist.UseBitmapCache)
        {
            _element.InvalidateProperty(UIElement.CacheModeProperty);
        }
        _element.InvalidateProperty(UIElement.RenderTransformProperty);
        _element.InvalidateProperty(UIElement.RenderTransformOriginProperty);
    }

    private void OnCurrentStateInvalidated(object? sender, EventArgs e)
    {
        if (sender is Clock clock)
        {
            _currentState = clock.CurrentState;
        }
    }

    private void OnCompleted(object? sender, EventArgs e)
    {
        Completed?.Invoke(this, EventArgs.Empty);
    }

    [SuppressMessage("Performance", "CA1822:Mark members as static")]
    private BitmapCache GetBitmapCache()
    {
#if NET462_OR_NEWER
        return new BitmapCache(VisualTreeHelper.GetDpi(_element).PixelsPerDip);
#else
        return _defaultBitmapCache;
#endif
    }

    private static readonly BitmapCache _defaultBitmapCache;

    private readonly FrameworkElement _element;
    private readonly Storyboard _storyboard;

    private ClockState _currentState = ClockState.Stopped;
}
