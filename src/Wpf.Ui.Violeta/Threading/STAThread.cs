using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Windows.Threading;

namespace Wpf.Ui.Violeta.Threading;

public sealed class STAThread(Action<STAThread<object>> start) : STAThread<object>(start);

public class STAThread<T> : STADispatcherObject, IDisposable where T : class
{
    public Thread Value { get; set; } = null!;
    public T Result { get; set; } = default!;

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

    [SuppressMessage("Usage", "CA1816:Dispose methods should call SuppressFinalize")]
    [SuppressMessage("CodeQuality", "IDE0079:Unnecessary SuppressMessage attribute suppression")]
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
                ///
            }
        }
        Dispatcher?.InvokeShutdown();
    }
}

public class STADispatcherObject(Dispatcher dispatcher = null!)
{
    public Dispatcher Dispatcher
    {
        get => dispatcher;
        set => dispatcher = value;
    }
}
