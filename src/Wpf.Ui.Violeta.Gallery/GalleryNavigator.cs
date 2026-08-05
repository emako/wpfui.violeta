using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace Wpf.Ui.Violeta.Gallery;

/// <summary>
/// Lightweight static navigator used by overview cards to request page navigation.
/// </summary>
public static class GalleryNavigator
{
    public static Action<string?>? NavigateRequested { get; set; }

    public static ICommand NavigateCommand { get; } = new RelayCommand<string>(Navigate);

    public static void Navigate(string? tag)
    {
        if (!string.IsNullOrWhiteSpace(tag))
        {
            NavigateRequested?.Invoke(tag);
        }
    }
}
