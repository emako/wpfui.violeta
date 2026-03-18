using System;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Wpf.Ui.Violeta.Threading;

public static class DispatcherExtension
{
    public static void Delay(this Dispatcher dispatcher, int delay, Action<object> action, object? param = null)
    {
        _ = Task.Delay(delay).ContinueWith(t =>
        {
            dispatcher?.Invoke(action, param);
        });
    }
}
