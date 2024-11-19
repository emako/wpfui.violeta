using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media.Animation;

namespace Wpf.Ui.Violeta.Controls;

public partial class SplashWindow : Window, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public Uri ImageUri { get; }

    protected string hint = null!;

    public string Hint
    {
        get => hint;
        set => SetProperty(ref hint, value);
    }

    public bool AutoEnd { get; set; } = false;

    public DateTime TimeOfCtor = DateTime.Now;

    public SplashWindow(Uri imageUri)
    {
        DataContext = this;
        ImageUri = imageUri;
        InitializeComponent();

        MouseLeftButtonDown += (sender, _) =>
        {
            if (sender is DependencyObject depObject)
            {
                GetWindow(depObject)?.DragMove();
            }
        };
    }

    protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        PropertyChanged?.Invoke(this, e);
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, newValue))
        {
            return false;
        }

        field = newValue;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void Start_Completed(object? sender, EventArgs e)
    {
        if (AutoEnd)
        {
            StartEnd();
        }
    }

    private void End_Completed(object? sender, EventArgs e)
    {
        Shutdown();
    }

    public void StartEnd()
    {
        Dispatcher.Invoke(() =>
        {
            Storyboard storyboard = (Storyboard)FindResource("End");
            storyboard.Begin();
        });
    }

    public void Shutdown()
    {
        Dispatcher.Invoke(() =>
        {
            Close();
            Dispatcher?.InvokeShutdown();
        });
    }
}
