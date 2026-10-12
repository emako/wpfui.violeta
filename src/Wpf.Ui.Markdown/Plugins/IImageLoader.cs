using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Media.Imaging;

namespace Wpf.Ui.Markdown.Plugins;

public interface IImageLoader
{
    public BitmapImage? Load(Stream stream);
}
