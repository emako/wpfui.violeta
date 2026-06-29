using System;
using System.Collections;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using DispatcherDictionary = System.Collections.Concurrent.ConcurrentDictionary<System.Windows.Threading.Dispatcher, Wpf.Ui.Violeta.Controls.DispatcherAsyncOperation<System.Windows.Threading.Dispatcher>>;

namespace Wpf.Ui.Violeta.Controls;

[ContentProperty(nameof(Child))]
public class AsyncBox : FrameworkElement
{
    /// <summary>
    /// 保存外部 UI 线程和与其关联的异步 UI 线程。
    /// 例如主 UI 线程对应一个 AsyncBox 专用的 UI 线程；外面可能有另一个 UI 线程，那么对应另一个 AsyncBox 专用的 UI 线程。
    /// </summary>
    private static readonly DispatcherDictionary RelatedAsyncDispatchers = new();

    private UIElement? _child;

    private readonly HostVisual _hostVisual;

    private VisualTargetPresentationSource? _targetSource;

    private UIElement? _loadingView;

    private readonly ContentPresenter _contentPresenter;

    private bool _isChildReadyToLoad;

    private bool _isReadyToStartLoading;

    private bool _isLoadingStarted;

    public static readonly DependencyProperty LoadingDelayProperty =
        DependencyProperty.Register(nameof(LoadingDelay), typeof(int), typeof(AsyncBox), new PropertyMetadata(0));

    public static readonly DependencyProperty IsLoadingProperty =
        DependencyProperty.Register(nameof(IsLoading), typeof(bool), typeof(AsyncBox), new PropertyMetadata(false, OnLoadingTriggerChanged));

    public static readonly DependencyProperty AutoLoadingProperty =
        DependencyProperty.Register(nameof(AutoLoading), typeof(bool), typeof(AsyncBox), new PropertyMetadata(true, OnLoadingTriggerChanged));

    public int LoadingDelay
    {
        get => (int)GetValue(LoadingDelayProperty);
        set => SetValue(LoadingDelayProperty, value);
    }

    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    public bool AutoLoading
    {
        get => (bool)GetValue(AutoLoadingProperty);
        set => SetValue(AutoLoadingProperty, value);
    }

    public UIElement? Child
    {
        get => _child;
        set
        {
            if (Equals(_child, value)) return;

            if (value != null)
            {
                RemoveLogicalChild(value);

                if (value is FrameworkElement element)
                {
                    void ChildLoaded(object sender, RoutedEventArgs e)
                    {
                        element.Loaded -= ChildLoaded;

                        RemoveLoadingView();
                    }

                    element.Loaded += ChildLoaded;
                }
                else
                {
                    throw new InvalidOperationException($"The {nameof(Child)} must be typeof {nameof(FrameworkElement)}.");
                }
            }

            _child = value!;

            if (_isChildReadyToLoad)
            {
                ActivateChild();
            }
        }
    }

    public Type LoadingViewType
    {
        get
        {
            if (field == null)
            {
                throw new InvalidOperationException($"Before {nameof(AsyncBox)} displayed, you must first set {nameof(LoadingViewType)} to a typeof {nameof(UIElement)} Loading UIElement.");
            }

            return field;
        }
        set
        {
#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable CA1510 // Use ArgumentNullException throw helper
#pragma warning disable IDE0016 // Null check can be simplified
            if (value == null)
            {
                throw new ArgumentNullException(nameof(LoadingViewType));
            }
#pragma warning restore IDE0016 // Null check can be simplified
#pragma warning restore CA1510 // Use ArgumentNullException throw helper
#pragma warning restore IDE0079 // Remove unnecessary suppression

            if (field != null)
            {
                throw new ArgumentException($"{nameof(LoadingViewType)} is only allowed to be set once.", nameof(value));
            }

            field = value;
        }
    }

    [Obsolete("This property is only for internal use. Please set LoadingViewType instead.", true)]
    public UIElement? LoadingView
    {
        get => _loadingView;
        set => _loadingView = value;
    }

    public AsyncBox()
    {
        _hostVisual = new HostVisual();
        _contentPresenter = new ContentPresenter();
        Loaded += OnLoaded;
    }

    /// <summary>
    /// 返回一个可等待的用于显示异步 UI 的后台 UI 线程调度器。
    /// </summary>
    private DispatcherAsyncOperation<Dispatcher> GetAsyncDispatcherAsync() => RelatedAsyncDispatchers.GetOrAdd(
        Dispatcher, dispatcher => AsyncBoxDispatcher.RunNewAsync("AsyncBox"));

    private UIElement CreateLoadingView()
    {
        if (_loadingView == null)
        {
            var instance = Activator.CreateInstance(LoadingViewType);
            if (instance is UIElement element)
            {
                return element;
            }
        }
        else
        {
            return _loadingView;
        }

        throw new InvalidOperationException($"The {LoadingViewType} must be typeof {nameof(UIElement)}.");
    }

    private void RemoveLoadingView()
    {
        _targetSource?.Dispatcher.Invoke(() => _targetSource?.Dispose());
    }

    private static void OnLoadingTriggerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AsyncBox asyncBox)
        {
            asyncBox.TryStartLoading();
        }
    }

    private int GetEffectiveLoadingDelay() => Math.Max(0, LoadingDelay);

    private bool ShouldStartLoading() => AutoLoading || IsLoading;

    private void TryStartLoading()
    {
        if (!_isReadyToStartLoading || _isLoadingStarted || _isChildReadyToLoad)
        {
            return;
        }

        if (!ShouldStartLoading())
        {
            return;
        }

        _isLoadingStarted = true;
        _ = StartLoadingAsync();
    }

    private async Task StartLoadingAsync()
    {
        var loadingDelay = GetEffectiveLoadingDelay();
        if (loadingDelay > 0)
        {
            await Task.Delay(loadingDelay);
        }

        if (_isChildReadyToLoad)
        {
            return;
        }

        _isChildReadyToLoad = true;
        ActivateChild();
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnLoaded;
        if (DesignerProperties.GetIsInDesignMode(this)) return;

        var dispatcher = await GetAsyncDispatcherAsync();
        _loadingView = await dispatcher.InvokeAsync(() =>
        {
            var loadingView = CreateLoadingView();
            _targetSource = new VisualTargetPresentationSource(_hostVisual)
            {
                RootVisual = loadingView
            };
            return loadingView;
        });
        AddVisualChild(_contentPresenter);
        AddVisualChild(_hostVisual);

        await LayoutAsync();
        await Dispatcher.Yield(DispatcherPriority.Background);

        Dispatcher.DoEvents();

        _isReadyToStartLoading = true;
        TryStartLoading();
    }

    private void ActivateChild()
    {
        var child = Child;
        if (child != null)
        {
            _contentPresenter.Content = child;
            AddLogicalChild(child);
            InvalidateMeasure();
        }
    }

    private async Task LayoutAsync()
    {
        var dispatcher = await GetAsyncDispatcherAsync();
        await dispatcher.InvokeAsync(() =>
        {
            if (_loadingView != null)
            {
                _loadingView.Measure(RenderSize);
                _loadingView.Arrange(new Rect(RenderSize));
            }
        });
    }

    protected override int VisualChildrenCount => _loadingView != null ? 2 : 0;

    protected override Visual GetVisualChild(int index)
    {
        return index switch
        {
            0 => _contentPresenter,
            1 => _hostVisual,
            _ => null!,
        };
    }

    protected override IEnumerator LogicalChildren
    {
        get
        {
            if (_isChildReadyToLoad)
            {
                yield return _contentPresenter;
            }
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        if (_isChildReadyToLoad)
        {
            _contentPresenter.Measure(availableSize);
            return _contentPresenter.DesiredSize;
        }

        var size = base.MeasureOverride(availableSize);
        return size;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (_isChildReadyToLoad)
        {
            _contentPresenter.Arrange(new Rect(finalSize));
            var renderSize = _contentPresenter.RenderSize;
            LayoutAsync().ConfigureAwait(false);
            return renderSize;
        }

        var size = base.ArrangeOverride(finalSize);
        LayoutAsync().ConfigureAwait(false);
        return size;
    }
}

file static class DispatcherExtension
{
    public static void DoEvents(this Dispatcher dispatcher)
    {
        DispatcherFrame frame = new();
        dispatcher.BeginInvoke(new Action<object>((obj) =>
        {
            if (obj is DispatcherFrame frm)
            {
                frm.Continue = false;
            }
        }), DispatcherPriority.Background, frame);
    }
}
