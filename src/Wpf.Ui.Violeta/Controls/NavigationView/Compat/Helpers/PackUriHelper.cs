using System;

namespace Wpf.Ui.Violeta.Controls.Compat;

internal static class PackUriHelper
{
    public static Uri GetAbsoluteUri(string path)
    {
        return new Uri($"pack://application:,,,/Wpf.Ui.Violeta.Controls.Compat;component/{path}");
    }
}
