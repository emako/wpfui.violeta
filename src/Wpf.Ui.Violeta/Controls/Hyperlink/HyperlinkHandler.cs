using System;
using System.Diagnostics;
using System.Windows.Navigation;

namespace Wpf.Ui.Violeta.Controls;

public static class HyperlinkHandler
{
    public static RequestNavigateEventHandler Default { get; } = (_, e) =>
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri)
            {
                UseShellExecute = true,
            });

            _ = process;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }

        e.Handled = true;
    };
}
