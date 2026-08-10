using System;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Provides the joystick position normalized to the range from -1 to 1.
/// </summary>
public sealed class JoystickMoveEventArgs(double normalizedX, double normalizedY) : EventArgs
{
    public double NormalizedX { get; } = normalizedX;

    public double NormalizedY { get; } = normalizedY;
}
