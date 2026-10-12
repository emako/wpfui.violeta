using System;
using System.Windows.Input;

namespace Wpf.Ui.Markdown.LinkActions;

public class HighlightOnlyCommand : ICommand
{
    public event EventHandler? CanExecuteChanged;

    private bool _isExecutable = true;
    public bool IsExecutable
    {
        get => _isExecutable;
        set
        {
            if (_isExecutable != value)
            {
                _isExecutable = value;
                CanExecuteChanged?.Invoke(this, new EventArgs());
            }
        }
    }

    public bool CanExecute(object? parameter) => _isExecutable;

    public void Execute(object? parameter) {}
}
