using System;
using System.Diagnostics;
using System.IO.Packaging;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using Wpf.Ui.Violeta.Win32;

namespace Wpf.Ui.Violeta.Controls;

public static class Splash
{
    private static STAThread<SplashWindow>? Current { get; set; }

    static Splash()
    {
        if (!UriParser.IsKnownScheme("pack"))
        {
            // Ensure the Pack URI scheme is registered.
            _ = PackUriHelper.UriSchemePack;
        }
    }

    public static void ShowAsync(string imageUriString, double opacity = 1d, Action? completed = null)
    {
        ShowAsync(new Uri(imageUriString), opacity, completed);
    }

    public static void ShowAsync(Uri imageUri, double opacity = 1d, Action? completed = null)
    {
        Current = new(sta =>
        {
            if (string.IsNullOrWhiteSpace(sta.Value.Name))
            {
                sta.Value.Name = "Splash Thread";
            }
            sta.Result = new SplashWindow(imageUri)
            {
                Opacity = opacity,
            };
            sta.Result.Show();
        });
        Current.Start();

        if (completed != null)
        {
            _ = Task.Run(() =>
            {
                try
                {
                    Current.Value.Join();
                    completed?.Invoke();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            });
        }
    }

    public static void CloseAsync(Window? owner = null, bool forced = false)
    {
        try
        {
            SplashWindow? current = Current?.Result;

            if (current == null)
            {
                return;
            }
            current.Closing += (_, _) =>
            {
                owner?.Dispatcher.Invoke(() =>
                {
                    nint hwnd = new WindowInteropHelper(owner).Handle;

                    _ = User32.SetForegroundWindow(hwnd);
                    _ = User32.BringWindowToTop(hwnd);
                });
            };
            if (forced)
            {
                current.Shutdown();
            }
            else
            {
                if (!current.AutoEnd)
                {
                    current.StartEnd();
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
    }

    public static void CloseOnLoaded(Window? owner = null, int? minimumMilliseconds = null, bool forced = false)
    {
        if (owner == null)
        {
            throw new ArgumentNullException(nameof(owner));
        }

        owner.Loaded += OnLoaded;

        async void OnLoaded(object sender, RoutedEventArgs e)
        {
            owner.Loaded -= OnLoaded;

            if (minimumMilliseconds != null)
            {
                SplashWindow? current = Current?.Result;

                if (current != null)
                {
                    double survivalMilliseconds = (DateTime.Now - current.TimeOfCtor).TotalMilliseconds;
                    if (survivalMilliseconds < minimumMilliseconds)
                    {
                        await Task.Delay((int)(minimumMilliseconds.Value - survivalMilliseconds));
                    }
                }
            }

            CloseAsync(owner, forced);
        }
    }

    private sealed class STAThread(Action<STAThread<object>> start) : STAThread<object>(start)
    {
    }

    private class STAThread<T> : STADispatcherObject, IDisposable where T : class
    {
        public Thread Value { get; set; } = null!;
        public T Result { get; set; } = null!;

        public STAThread(Action<STAThread<T>> start)
        {
            Value = new(() =>
            {
                Dispatcher = Dispatcher.CurrentDispatcher;
                start?.Invoke(this);
                Dispatcher.Run();
            })
            {
                IsBackground = true,
                Name = $"STAThread<{typeof(T)}>",
            };
            Value.SetApartmentState(ApartmentState.STA);
        }

        public void Start()
        {
            Value?.Start();
        }

        public void Forget()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (typeof(IDisposable).IsAssignableFrom(typeof(T)))
            {
                try
                {
                    ((IDisposable?)Result)?.Dispose();
                }
                catch
                {
                }
            }
            Dispatcher?.InvokeShutdown();
        }
    }

    private class STADispatcherObject(Dispatcher dispatcher = null!)
    {
        public Dispatcher Dispatcher { get; set; } = dispatcher;
    }
}
