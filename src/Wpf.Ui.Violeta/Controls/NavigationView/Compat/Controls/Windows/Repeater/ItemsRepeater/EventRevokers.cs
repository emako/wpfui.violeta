#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8618, CS8619, CS8625

namespace Wpf.Ui.Violeta.Controls.Compat;

internal class ItemsRepeaterElementPreparedRevoker : EventRevoker<ItemsRepeater, TypedEventHandler<ItemsRepeater, ItemsRepeaterElementPreparedEventArgs>>
{
    public ItemsRepeaterElementPreparedRevoker(ItemsRepeater source, TypedEventHandler<ItemsRepeater, ItemsRepeaterElementPreparedEventArgs> handler) : base(source, handler)
    {
    }

    protected override void AddHandler(ItemsRepeater source, TypedEventHandler<ItemsRepeater, ItemsRepeaterElementPreparedEventArgs> handler)
    {
        source.ElementPrepared += handler;
    }

    protected override void RemoveHandler(ItemsRepeater source, TypedEventHandler<ItemsRepeater, ItemsRepeaterElementPreparedEventArgs> handler)
    {
        source.ElementPrepared -= handler;
    }
}

internal class ItemsRepeaterElementClearingRevoker : EventRevoker<ItemsRepeater, TypedEventHandler<ItemsRepeater, ItemsRepeaterElementClearingEventArgs>>
{
    public ItemsRepeaterElementClearingRevoker(ItemsRepeater source, TypedEventHandler<ItemsRepeater, ItemsRepeaterElementClearingEventArgs> handler) : base(source, handler)
    {
    }

    protected override void AddHandler(ItemsRepeater source, TypedEventHandler<ItemsRepeater, ItemsRepeaterElementClearingEventArgs> handler)
    {
        source.ElementClearing += handler;
    }

    protected override void RemoveHandler(ItemsRepeater source, TypedEventHandler<ItemsRepeater, ItemsRepeaterElementClearingEventArgs> handler)
    {
        source.ElementClearing -= handler;
    }
}
