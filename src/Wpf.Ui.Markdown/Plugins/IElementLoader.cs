using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Wpf.Ui.Markdown.Plugins;

public interface IElementLoader
{
    public FrameworkElement? Load(Stream stream);
}
