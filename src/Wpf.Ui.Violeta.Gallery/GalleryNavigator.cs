using System;

namespace Wpf.Ui.Violeta.Gallery;

/// <summary>
/// Lightweight static navigator used by overview cards to request page navigation.
/// </summary>
public static class GalleryNavigator
{
    public static Action<string?>? NavigateRequested { get; set; }

    public static void Navigate(string tag) => NavigateRequested?.Invoke(tag);
}
