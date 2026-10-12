using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace Wpf.Ui.Markdown.Plugins;

public interface ICodeBlockLoader
{
    public void Register(Definition definition);
    FrameworkElement CodeBlocksEvaluator(string? lang, string code, bool disabledContextMenu);
}
