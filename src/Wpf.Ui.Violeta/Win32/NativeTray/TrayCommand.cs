using System;
using System.Windows.Input;

namespace Wpf.Ui.Violeta.Win32;

public class TrayCommand : ITrayCommand
{
    private readonly Action<object?> _execute;
    private readonly Func<object?, bool>? _canExecute;

    public TrayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute()
    {
        return CanExecute(null);
    }

    public bool CanExecute(object? parameter)
    {
        return _canExecute?.Invoke(parameter) ?? true;
    }

    public void Execute(object? parameter)
    {
        if (CanExecute(parameter))
            _execute(parameter);
    }

    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public static implicit operator TrayCommand(Action<object?> execute)
    {
        return new TrayCommand(execute);
    }
}

public interface ITrayCommand : ICommand
{
    bool CanExecute();
}
