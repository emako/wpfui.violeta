using System.IO;

namespace Wpf.Ui.Violeta.Controls.Svg.FileLoaders;

public interface IExternalFileLoader
{
    public Stream LoadFile(string hRef, string svgFilename);
}
