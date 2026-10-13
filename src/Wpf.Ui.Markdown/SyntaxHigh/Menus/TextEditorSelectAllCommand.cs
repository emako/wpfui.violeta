using ICSharpCode.AvalonEdit;
using System;
using System.Windows.Input;

namespace Wpf.Ui.Markdown.SyntaxHigh.Menus;

public class TextEditorSelectAllCommand(TextEditor editor) : ICommand
{
    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter)
    {
        return true;
    }

    public void Execute(object? parameter)
    {
        editor.SelectAll();
    }
}
