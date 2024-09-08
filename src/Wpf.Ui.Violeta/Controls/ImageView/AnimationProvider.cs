using System;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace Wpf.Ui.Violeta.Controls;

internal abstract class AnimationProvider : IDisposable
{
    protected AnimationProvider(Uri path)
    {
        Path = path;
        Animator = new();
        Animator.KeyFrames.Add(new DiscreteInt32KeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.Zero)));
    }

    public Uri Path { get; }

    public Int32AnimationUsingKeyFrames Animator { get; protected set; }

    public abstract void Dispose();

    public abstract Task<BitmapSource> GetRenderedFrame(int index);
}
