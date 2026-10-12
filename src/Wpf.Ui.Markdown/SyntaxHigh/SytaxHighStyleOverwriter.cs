using Wpf.Ui.Markdown.Plugins;
using System.Windows;

namespace Wpf.Ui.Markdown.SyntaxHigh;

internal class SytaxHighStyleOverwriter : IStyleOverwriter
{
    public Style Overwrite(Style style)
    {
        return SyntaxHighStyle.OverwriteStyle(style);
    }
}