#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8618, CS8619, CS8625

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
