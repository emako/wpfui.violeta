#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8618, CS8619, CS8625

using System;

namespace Wpf.Ui.Violeta.Controls.Compat;

internal abstract class EventRevoker<TSource, TDelegate>
    where TSource : class
    where TDelegate : Delegate
{
    private WeakReference<TSource> _source;
    private WeakReference<TDelegate> _handler;

    protected EventRevoker(TSource source, TDelegate handler)
    {
        _source = new WeakReference<TSource>(source);
        _handler = new WeakReference<TDelegate>(handler);
        AddHandler(source, handler);
    }

    protected abstract void AddHandler(TSource source, TDelegate handler);

    protected abstract void RemoveHandler(TSource source, TDelegate handler);

    public void Revoke()
    {
        if (_source != null && _handler != null &&
            _source.TryGetTarget(out var source) &&
            _handler.TryGetTarget(out var handler))
        {
            RemoveHandler(source, handler);
        }

        _source = null;
        _handler = null;
    }
}
