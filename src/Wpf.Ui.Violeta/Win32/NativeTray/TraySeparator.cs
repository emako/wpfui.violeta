using System;
using System.Windows.Input;

namespace Wpf.Ui.Violeta.Win32;

public sealed class TraySeparator : ITrayMenuItemBase
{
    public TrayMenu? Menu
    {
        get => null;
        set => throw new NotImplementedException();
    }

    public object? Icon
    {
        get => null;
        set => throw new NotImplementedException();
    }

    public string? Header
    {
        get => "-";
        set => throw new NotImplementedException();
    }

    public bool IsVisible
    {
        get => true;
        set => throw new NotImplementedException();
    }

    public bool IsChecked
    {
        get => false;
        set => throw new NotImplementedException();
    }

    public bool IsEnabled
    {
        get => false;
        set => throw new NotImplementedException();
    }

    public bool IsBold
    {
        get => false;
        set => throw new NotImplementedException();
    }

    public ICommand? Command
    {
        get => null;
        set => throw new NotImplementedException();
    }

    public object? CommandParameter
    {
        get => null;
        set => throw new NotImplementedException();
    }

    public object? Tag { get; set; } = null;
}
