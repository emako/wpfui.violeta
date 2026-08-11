namespace Wpf.Ui.Violeta.Controls.Compat;

internal class FlyoutBaseClosingRevoker(FlyoutBase source, TypedEventHandler<FlyoutBase, FlyoutBaseClosingEventArgs> handler) : EventRevoker<FlyoutBase, TypedEventHandler<FlyoutBase, FlyoutBaseClosingEventArgs>>(source, handler)
{
    protected override void AddHandler(FlyoutBase source, TypedEventHandler<FlyoutBase, FlyoutBaseClosingEventArgs> handler)
    {
        source.Closing += handler;
    }

    protected override void RemoveHandler(FlyoutBase source, TypedEventHandler<FlyoutBase, FlyoutBaseClosingEventArgs> handler)
    {
        source.Closing -= handler;
    }
}
