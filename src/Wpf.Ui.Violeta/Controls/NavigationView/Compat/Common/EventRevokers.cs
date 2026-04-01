using System.Windows;

namespace Wpf.Ui.Violeta.Controls.Compat;

internal class FrameworkElementSizeChangedRevoker : EventRevoker<FrameworkElement, SizeChangedEventHandler>
{
    public FrameworkElementSizeChangedRevoker(FrameworkElement source, SizeChangedEventHandler handler) : base(source, handler)
    {
    }

    protected override void AddHandler(FrameworkElement source, SizeChangedEventHandler handler)
    {
        source.SizeChanged += handler;
    }

    protected override void RemoveHandler(FrameworkElement source, SizeChangedEventHandler handler)
    {
        source.SizeChanged -= handler;
    }
}
