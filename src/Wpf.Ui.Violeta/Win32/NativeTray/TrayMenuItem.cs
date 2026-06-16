using System.Windows.Input;

namespace Wpf.Ui.Violeta.Win32;

public class TrayMenuItem : ITrayMenuItemBase
{
    public TrayMenu? Menu { get; set; }

    public object? Icon { get; set; }

    public string? Header { get; set; }

    public bool IsChecked { get; set; }

    public bool IsEnabled { get; set; } = true;

    public bool IsVisible { get; set; } = true;

    public bool IsBold { get; set; }

    public object? Tag { get; set; } = null;

    public ICommand? Command { get; set; }

    public object? CommandParameter { get; set; }
}

public interface ITrayMenuItemBase
{
    TrayMenu? Menu { get; set; }

    object? Icon { get; set; }

    string? Header { get; set; }

    bool IsVisible { get; set; }

    bool IsChecked { get; set; }

    bool IsEnabled { get; set; }

    bool IsBold { get; set; }

    object? Tag { get; set; }

    ICommand? Command { get; set; }

    object? CommandParameter { get; set; }
}
