using System;
using System.ComponentModel;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Default scroll physics using velocity-based friction decay.
/// Provides natural momentum scrolling where the total scroll distance equals the input delta.
/// </summary>
public class DefaultScrollPhysics : IScrollPhysics
{
    // Friction coefficient range: 0.85 (stops quickly) ~ 0.96 (smooth deceleration)
    private const double MinFriction = 0.85d;

    private const double MaxFriction = 0.96d;
    private const double PreciseModeFriction = 0.88d;

    // Reference frame time for frame-rate–independent physics (normalised to 144 Hz)
    private const double ReferenceFrameTime = 1d / 144d;

    private double _friction;
    private double _smoothness = 0.78d;
    private double _velocity;
    private bool _isStable = true;
    private bool _isPreciseMode;

    public DefaultScrollPhysics()
    {
        _friction = MinFriction + (MaxFriction - MinFriction) * _smoothness;
    }

    /// <summary>
    /// Scroll smoothness in [0, 1]. 0 = stops quickly; 1 = very smooth long deceleration.
    /// </summary>
    [Category("Scroll Physics")]
    [Description("The larger the value, the smoother and longer the rolling; The smaller the value, the faster it stops. Take the value 0~1")]
    public double Smoothness
    {
        get => _smoothness;
        set
        {
            _smoothness = value < 0.0 ? 0.0 : value > 1.0 ? 1.0 : value;
            _friction = MinFriction + (MaxFriction - MinFriction) * _smoothness;
        }
    }

    /// <summary>
    /// Stop threshold in DIPs. Scrolling stops when remaining velocity drops below this.
    /// </summary>
    [Category("Scroll Physics")]
    [Description("Stop threshold, stop scrolling when the remaining distance is less than this value. Take the value of 0.1~5")]
    public double StopThreshold { get; set; } = 0.5;

    /// <inheritdoc/>
    public bool IsStable => _isStable;

    /// <inheritdoc/>
    public bool IsPreciseMode
    {
        get => _isPreciseMode;
        set => _isPreciseMode = value;
    }

    /// <inheritdoc/>
    public void OnScroll(double delta)
    {
        _isStable = false;
        // Accumulate velocity; total displacement = velocity when k = (1 - f).
        _velocity -= delta;
    }

    /// <inheritdoc/>
    public double Update(double currentOffset, double dt)
    {
        if (_isStable) return currentOffset;

        if (Math.Abs(_velocity) < StopThreshold)
        {
            _velocity = 0;
            _isStable = true;
            return currentOffset;
        }

        // Frame-rate–independent: scale friction exponent by (dt / referenceFrameTime).
        double timeFactor = dt / ReferenceFrameTime;
        double f = Math.Pow(_isPreciseMode ? PreciseModeFriction : _friction, timeFactor);
        double displacement = _velocity * (1.0 - f);
        _velocity -= displacement;
        return currentOffset + displacement;
    }
}
