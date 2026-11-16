using ICSharpCode.AvalonEdit;
using System;
using System.Windows.Input;

namespace Wpf.Ui.Markdown.SyntaxHigh.Menus;

public class TextEditorSelectAllCommand(TextEditor editor) : ICommand
{
#pragma warning disable CS0067

    /// <summary>
    /// ICommand requires the event; CanExecute is always true
    /// </summary>
    public event EventHandler? CanExecuteChanged;

#pragma warning restore CS0067

    public bool CanExecute(object? parameter)
    {
        return true;
    }

    public void Execute(object? parameter)
    {
        editor.SelectAll();
    }
}
