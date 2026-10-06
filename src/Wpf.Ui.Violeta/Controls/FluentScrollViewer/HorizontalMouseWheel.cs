using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Media3D;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Converts native horizontal mouse-wheel messages into a WPF routed event.
/// Ported from FluentWpfCore.Input.HorizontalMouseWheel.
/// </summary>
/// <remarks>
/// All subscribers in the same native window share one message hook. The event is
/// raised on the element under the pointer and bubbles through the WPF element tree.
/// </remarks>
public static class HorizontalMouseWheel
{
    private const int WmMouseHWheel = 0x020E;

    private static readonly object SyncRoot = new();
    private static readonly ConditionalWeakTable<FrameworkElement, ElementRegistration> ElementRegistrations = new();
    private static readonly ConditionalWeakTable<HwndSource, SourceHook> SourceHooks = new();

    /// <summary>
    /// Identifies the horizontal mouse-wheel routed event.
    /// </summary>
    public static readonly RoutedEvent MouseWheelEvent = EventManager.RegisterRoutedEvent(
        "MouseWheel",
        RoutingStrategy.Bubble,
        typeof(MouseWheelEventHandler),
        typeof(HorizontalMouseWheel));

    /// <summary>
    /// Adds a handler for horizontal mouse-wheel input.
    /// </summary>
    public static void AddMouseWheelHandler(DependencyObject element, MouseWheelEventHandler handler)
    {
        _ = element ?? throw new ArgumentNullException(nameof(element));
        _ = handler ?? throw new ArgumentNullException(nameof(handler));
        if (element is not FrameworkElement frameworkElement)
            throw new ArgumentException("The event target must be a FrameworkElement.", nameof(element));

        frameworkElement.AddHandler(MouseWheelEvent, handler);

        lock (SyncRoot)
        {
            ElementRegistration registration = ElementRegistrations.GetValue(
                frameworkElement,
                static registeredElement => new ElementRegistration(registeredElement));
            registration.AddHandler(handler);
        }
    }

    /// <summary>
    /// Removes a handler for horizontal mouse-wheel input.
    /// </summary>
    public static void RemoveMouseWheelHandler(DependencyObject element, MouseWheelEventHandler handler)
    {
        _ = element ?? throw new ArgumentNullException(nameof(element));
        _ = handler ?? throw new ArgumentNullException(nameof(handler));
        if (element is not FrameworkElement frameworkElement)
            throw new ArgumentException("The event target must be a FrameworkElement.", nameof(element));

        lock (SyncRoot)
        {
            if (!ElementRegistrations.TryGetValue(frameworkElement, out ElementRegistration? registration))
                return;

            if (!registration.RemoveHandler(handler, out bool isEmpty))
                return;

            frameworkElement.RemoveHandler(MouseWheelEvent, handler);
            if (isEmpty)
                ElementRegistrations.Remove(frameworkElement);
        }
    }

    private static SourceHook AcquireSourceHook(HwndSource source)
    {
        lock (SyncRoot)
        {
            SourceHook hook = SourceHooks.GetValue(source, static registeredSource => new SourceHook(registeredSource));
            hook.AddReference();
            return hook;
        }
    }

    private static void ReleaseSourceHook(SourceHook hook)
    {
        lock (SyncRoot)
        {
            if (hook.ReleaseReference())
                SourceHooks.Remove(hook.Source);
        }
    }

    private sealed class ElementRegistration
    {
        private readonly FrameworkElement _element;
        private readonly Dictionary<MouseWheelEventHandler, int> _handlers = new();
        private SourceHook? _sourceHook;
        private int _referenceCount;

        internal ElementRegistration(FrameworkElement element)
        {
            _element = element;
            _element.Loaded += OnLoaded;
            _element.Unloaded += OnUnloaded;
        }

        internal void AddHandler(MouseWheelEventHandler handler)
        {
            _handlers.TryGetValue(handler, out int handlerCount);
            _handlers[handler] = handlerCount + 1;

            _referenceCount++;
            if (_referenceCount == 1 && _element.IsLoaded)
                AttachToSource();
        }

        internal bool RemoveHandler(MouseWheelEventHandler handler, out bool isEmpty)
        {
            isEmpty = false;
            if (!_handlers.TryGetValue(handler, out int handlerCount))
                return false;

            if (handlerCount == 1)
                _handlers.Remove(handler);
            else
                _handlers[handler] = handlerCount - 1;

            _referenceCount--;
            if (_referenceCount != 0)
                return true;

            DetachFromSource();
            _element.Loaded -= OnLoaded;
            _element.Unloaded -= OnUnloaded;
            isEmpty = true;
            return true;
        }

        private void OnLoaded(object sender, RoutedEventArgs e) => AttachToSource();

        private void OnUnloaded(object sender, RoutedEventArgs e) => DetachFromSource();

        private void AttachToSource()
        {
            if (_sourceHook != null)
                return;

            if (PresentationSource.FromVisual(_element) is HwndSource source)
                _sourceHook = AcquireSourceHook(source);
        }

        private void DetachFromSource()
        {
            if (_sourceHook == null)
                return;

            ReleaseSourceHook(_sourceHook);
            _sourceHook = null;
        }
    }

    private sealed class SourceHook
    {
        private int _referenceCount;

        internal SourceHook(HwndSource source)
        {
            Source = source;
            Source.AddHook(WndProc);
        }

        internal HwndSource Source { get; }

        internal void AddReference() => _referenceCount++;

        internal bool ReleaseReference()
        {
            if (_referenceCount == 0)
                return false;

            _referenceCount--;
            if (_referenceCount != 0)
                return false;

            Source.RemoveHook(WndProc);
            return true;
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg != WmMouseHWheel || Source.RootVisual is not UIElement root)
                return IntPtr.Zero;

            int delta = (short)((wParam.ToInt64() >> 16) & 0xFFFF);
            if (delta == 0)
                return IntPtr.Zero;

            long packedPosition = lParam.ToInt64();
            var screenPosition = new Point(
                (short)(packedPosition & 0xFFFF),
                (short)((packedPosition >> 16) & 0xFFFF));
            Point position = root.PointFromScreen(screenPosition);
            IInputElement? inputElement = root.InputHitTest(position);

            if (inputElement == null)
                return IntPtr.Zero;

            var eventArgs = new MouseWheelEventArgs(Mouse.PrimaryDevice, Environment.TickCount, delta)
            {
                RoutedEvent = MouseWheelEvent,
                Source = inputElement,
            };

            switch (inputElement)
            {
                case UIElement uiElement:
                    uiElement.RaiseEvent(eventArgs);
                    break;
                case ContentElement contentElement:
                    contentElement.RaiseEvent(eventArgs);
                    break;
                case UIElement3D uiElement3D:
                    uiElement3D.RaiseEvent(eventArgs);
                    break;
                default:
                    return IntPtr.Zero;
            }

            handled = eventArgs.Handled;
            return IntPtr.Zero;
        }
    }
}
