using System;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Wpf.Ui.Violeta.Controls;

internal class BitmapImageProvider(Uri path) : AnimationProvider(path)
{
    public override void Dispose()
    {
        ///
    }

    public override Task<BitmapSource> GetRenderedFrame(int index)
    {
        return Task.FromResult<BitmapSource>(new BitmapImage(Path));
    }
}
