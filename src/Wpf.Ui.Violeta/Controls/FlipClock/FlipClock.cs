using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// A flip-card digital clock that displays the current time with 3D page-flip animations.
/// Ported from HandyControl's FlipClock.
/// </summary>
public class FlipClock : Control, IDisposable
{
    private readonly DispatcherTimer _dispatcherTimer;
    private bool _isDisposed;

    static FlipClock()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(FlipClock),
            new FrameworkPropertyMetadata(typeof(FlipClock)));
    }

    public FlipClock()
    {
        _dispatcherTimer = new DispatcherTimer(DispatcherPriority.Render)
        {
            Interval = TimeSpan.FromMilliseconds(200)
        };

        IsVisibleChanged += OnIsVisibleChanged;
    }

    ~FlipClock() => Dispose();

    #region NumberList

    public static readonly DependencyProperty NumberListProperty = DependencyProperty.Register(
        nameof(NumberList),
        typeof(IList<int>),
        typeof(FlipClock),
        new PropertyMetadata(new List<int> { 0, 0, 0, 0, 0, 0 }));

    /// <summary>
    /// Six digits representing HH:MM:SS (hour tens/units, minute tens/units, second tens/units).
    /// </summary>
    public IList<int> NumberList
    {
        get => (IList<int>)GetValue(NumberListProperty);
        set => SetValue(NumberListProperty, value);
    }

    #endregion

    #region DisplayTime

    public static readonly DependencyProperty DisplayTimeProperty = DependencyProperty.Register(
        nameof(DisplayTime),
        typeof(DateTime),
        typeof(FlipClock),
        new PropertyMetadata(default(DateTime), OnDisplayTimeChanged));

    /// <summary>
    /// The time shown on the clock. When the control is visible, this is updated automatically.
    /// </summary>
    public DateTime DisplayTime
    {
        get => (DateTime)GetValue(DisplayTimeProperty);
        set => SetValue(DisplayTimeProperty, value);
    }

    private static void OnDisplayTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ctl = (FlipClock)d;
        var v = (DateTime)e.NewValue;

        ctl.NumberList =
        [
            v.Hour / 10,
            v.Hour % 10,
            v.Minute / 10,
            v.Minute % 10,
            v.Second / 10,
            v.Second % 10
        ];
    }

    #endregion

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        IsVisibleChanged -= OnIsVisibleChanged;
        _dispatcherTimer.Stop();
        _dispatcherTimer.Tick -= OnDispatcherTimerTick;
        _isDisposed = true;
        GC.SuppressFinalize(this);
    }

    private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (_isDisposed)
        {
            return;
        }

        if (IsVisible)
        {
            _dispatcherTimer.Tick -= OnDispatcherTimerTick;
            _dispatcherTimer.Tick += OnDispatcherTimerTick;
            _dispatcherTimer.Start();
            DisplayTime = DateTime.Now;
        }
        else
        {
            _dispatcherTimer.Stop();
            _dispatcherTimer.Tick -= OnDispatcherTimerTick;
        }
    }

    private void OnDispatcherTimerTick(object? sender, EventArgs e) => DisplayTime = DateTime.Now;
}
