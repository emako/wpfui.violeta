namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Status of a step in <see cref="StepBar"/>.
/// </summary>
public enum StepStatus
{
    /// <summary>The step has been completed.</summary>
    Complete,

    /// <summary>The step is currently in progress.</summary>
    UnderWay,

    /// <summary>The step is waiting to start.</summary>
    Waiting
}
