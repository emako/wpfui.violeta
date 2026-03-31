using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;

namespace Wpf.Ui.Violeta.Controls;

[ContentProperty(nameof(Child))]
public sealed class DispatcherContainer : FrameworkElement
{
    private readonly HostVisual? _hostVisual;
    private VisualTargetPresentationSource? _targetSource;

    private bool _isUpdatingChild;
    private UIElement? _child;
    public UIElement? Child => _child;

    public DispatcherContainer()
    {
        _hostVisual = new HostVisual();
    }

    public async Task SetChildAsync<T>(Dispatcher? dispatcher = null) where T : UIElement, new()
    {
        await SetChildAsync(() => new T(), dispatcher);
    }

    public async Task SetChildAsync<T>(Func<T> @new, Dispatcher? dispatcher = null) where T : UIElement
    {
        dispatcher ??= await AsyncBoxDispatcher.RunNewAsync($"{typeof(T).Name}");
        var child = await dispatcher.InvokeAsync(@new);
        await SetChildAsync(child);
    }

    public async Task SetChildAsync(UIElement value)
    {
        if (_isUpdatingChild)
        {
            throw new InvalidOperationException("Child property should not be set during Child updating.");
        }

        _isUpdatingChild = true;
        try
        {
            await SetChildAsync();
        }
        finally
        {
            _isUpdatingChild = false;
        }

        async Task SetChildAsync()
        {
            var oldChild = _child;
            var visualTarget = _targetSource;

            if (Equals(oldChild, value))
                return;

            _targetSource = null!;
            if (visualTarget != null)
            {
                RemoveVisualChild(oldChild);
                await visualTarget.Dispatcher.InvokeAsync(visualTarget.Dispose);
            }

            _child = value;

            if (value == null)
            {
                _targetSource = null!;
            }
            else
            {
                await value.Dispatcher.InvokeAsync(() =>
                {
                    _targetSource = new VisualTargetPresentationSource(_hostVisual!)
                    {
                        RootVisual = value,
                    };
                });
                AddVisualChild(_hostVisual);
            }
            InvalidateMeasure();
        }
    }

    protected override Visual GetVisualChild(int index)
    {
        return index != 0
            ? throw new ArgumentOutOfRangeException(nameof(index))
            : (Visual)_hostVisual!;
    }

    protected override int VisualChildrenCount => _child != null ? 1 : 0;

    protected override Size MeasureOverride(Size availableSize)
    {
        UIElement? child = _child;
        if (child == null)
            return default;

        child.Dispatcher.InvokeAsync(
            () => child.Measure(availableSize),
            DispatcherPriority.Loaded);

        return default;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        UIElement? child = _child;
        if (child == null)
            return finalSize;

        child.Dispatcher.InvokeAsync(
            () => child.Arrange(new Rect(finalSize)),
            DispatcherPriority.Loaded);

        return finalSize;
    }

    protected override HitTestResult HitTestCore(PointHitTestParameters htp)
    {
        var child = _child;

        var element = child?.Dispatcher.Invoke(() =>
        {
            double offsetX = 0d, offsetY = 0d;
            if (child is FrameworkElement fe)
            {
                offsetX = fe.Margin.Left;
                offsetY = fe.Margin.Top;
            }
            return _child?.InputHitTest(new Point(htp.HitPoint.X - offsetX, htp.HitPoint.Y - offsetY));
        }, DispatcherPriority.Normal);
        if (element == null)
        {
            return null!;
        }

        return new PointHitTestResult(this, htp.HitPoint);
    }
}
