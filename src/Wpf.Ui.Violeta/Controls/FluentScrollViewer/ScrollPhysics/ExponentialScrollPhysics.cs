using System;
using System.ComponentModel;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Exponential decay scroll physics: offset = target + (start - target) * e^(-k*t).
/// Characteristics: fast start, slow end, natural feel.
/// </summary>
public class ExponentialScrollPhysics : IScrollPhysics
{
    private double _decayRate = 8.0;
    private double _remainingDistance;
    private bool _isStable = true;

    /// <summary>
    /// Decay rate in [1, 20]. Higher values reach the target faster.
    /// </summary>
    [Category("Scroll Physics")]
    [Description("The higher the decay rate, the faster the roll reaches the target position. Take the value of 1~20")]
    public double DecayRate
    {
        get => _decayRate;
        set => _decayRate = value < 1.0 ? 1.0 : value > 20.0 ? 20.0 : value;
    }

    /// <summary>
    /// Stop threshold in DIPs. Scrolling stops when the remaining distance drops below this.
    /// </summary>
    [Category("Scroll Physics")]
    [Description("Stop threshold, stop scrolling when the remaining distance is less than this value. Take the value of 0.1~5")]
    public double StopThreshold { get; set; } = 0.5;

    /// <inheritdoc/>
    public bool IsStable => _isStable;

    /// <inheritdoc/>
    public bool IsPreciseMode { get; set; }

    /// <inheritdoc/>
    public void OnScroll(double delta)
    {
        _isStable = false;
        _remainingDistance -= delta;
    }

    /// <inheritdoc/>
    public double Update(double currentOffset, double dt)
    {
        if (_isStable) return currentOffset;

        if (Math.Abs(_remainingDistance) < StopThreshold)
        {
            double last = _remainingDistance;
            _remainingDistance = 0;
            _isStable = true;
            return currentOffset + last;
        }

        // factor = 1 - e^(-k*dt): fraction of remaining distance consumed this frame.
        double factor = 1.0 - Math.Exp(-_decayRate * dt);
        double displacement = _remainingDistance * factor;
        _remainingDistance -= displacement;
        return currentOffset + displacement;
    }
}
