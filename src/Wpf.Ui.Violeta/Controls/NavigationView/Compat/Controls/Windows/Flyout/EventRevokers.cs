#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8618, CS8619, CS8625

namespace Wpf.Ui.Violeta.Controls.Compat;

internal class FlyoutBaseClosingRevoker : EventRevoker<FlyoutBase, TypedEventHandler<FlyoutBase, FlyoutBaseClosingEventArgs>>
{
    public FlyoutBaseClosingRevoker(FlyoutBase source, TypedEventHandler<FlyoutBase, FlyoutBaseClosingEventArgs> handler) : base(source, handler)
    {
    }

    protected override void AddHandler(FlyoutBase source, TypedEventHandler<FlyoutBase, FlyoutBaseClosingEventArgs> handler)
    {
        source.Closing += handler;
    }

    protected override void RemoveHandler(FlyoutBase source, TypedEventHandler<FlyoutBase, FlyoutBaseClosingEventArgs> handler)
    {
        source.Closing -= handler;
    }
}
