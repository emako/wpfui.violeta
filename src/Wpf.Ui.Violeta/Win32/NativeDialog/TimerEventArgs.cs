using System;

namespace Wpf.Ui.Violeta.Win32;

/// <summary>
/// Provides data for the <see cref="TaskDialog.Timer"/> event.
/// </summary>
/// <threadsafety instance="false" static="true" />
/// <remarks>
/// Initializes a new instance of the <see cref="TimerEventArgs"/> class with the specified tick count.
/// </remarks>
/// <param name="tickCount">The tick count.</param>
public class TimerEventArgs(int tickCount) : EventArgs
{
    private bool _resetTickCount;

    /// <summary>
    /// Gets or sets a value that indicates whether the tick count should be reset.
    /// </summary>
    /// <value>
    /// <see langword="true" /> to reset the tick count after the event handler returns; otherwise, <see langword="false" />.
    /// The default value is <see langword="false" />.
    /// </value>
    public bool ResetTickCount
    {
        get => _resetTickCount;
        set => _resetTickCount = value;
    }

    /// <summary>
    /// Gets the current tick count of the timer.
    /// </summary>
    /// <value>
    /// The number of milliseconds that has elapsed since the dialog was created or since the last time the event handler returned
    /// with the <see cref="ResetTickCount"/> property set to <see langword="true" />.
    /// </value>
    public int TickCount => tickCount;
}
