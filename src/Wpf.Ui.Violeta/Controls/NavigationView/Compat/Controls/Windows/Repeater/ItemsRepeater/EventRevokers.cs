namespace Wpf.Ui.Violeta.Controls.Compat;

internal class ItemsRepeaterElementPreparedRevoker(ItemsRepeater source, TypedEventHandler<ItemsRepeater, ItemsRepeaterElementPreparedEventArgs> handler) : EventRevoker<ItemsRepeater, TypedEventHandler<ItemsRepeater, ItemsRepeaterElementPreparedEventArgs>>(source, handler)
{
    protected override void AddHandler(ItemsRepeater source, TypedEventHandler<ItemsRepeater, ItemsRepeaterElementPreparedEventArgs> handler)
    {
        source.ElementPrepared += handler;
    }

    protected override void RemoveHandler(ItemsRepeater source, TypedEventHandler<ItemsRepeater, ItemsRepeaterElementPreparedEventArgs> handler)
    {
        source.ElementPrepared -= handler;
    }
}

internal class ItemsRepeaterElementClearingRevoker(ItemsRepeater source, TypedEventHandler<ItemsRepeater, ItemsRepeaterElementClearingEventArgs> handler) : EventRevoker<ItemsRepeater, TypedEventHandler<ItemsRepeater, ItemsRepeaterElementClearingEventArgs>>(source, handler)
{
    protected override void AddHandler(ItemsRepeater source, TypedEventHandler<ItemsRepeater, ItemsRepeaterElementClearingEventArgs> handler)
    {
        source.ElementClearing += handler;
    }

    protected override void RemoveHandler(ItemsRepeater source, TypedEventHandler<ItemsRepeater, ItemsRepeaterElementClearingEventArgs> handler)
    {
        source.ElementClearing -= handler;
    }
}
