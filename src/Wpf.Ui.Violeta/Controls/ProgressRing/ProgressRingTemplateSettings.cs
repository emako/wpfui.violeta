using System.Windows;

namespace Wpf.Ui.Violeta.Controls.Primitives;

/// <summary>
/// Provides calculated values that can be referenced as <c>TemplatedParent</c> sources
/// when defining templates for a ProgressRing control. Not intended for general use.
/// </summary>
public sealed class ProgressRingTemplateSettings : DependencyObject
{
    internal ProgressRingTemplateSettings()
    {
    }

    #region NormalizedRange

    private static readonly DependencyPropertyKey NormalizedRangePropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(NormalizedRange),
            typeof(double),
            typeof(ProgressRingTemplateSettings),
            null);

    public static readonly DependencyProperty NormalizedRangeProperty =
        NormalizedRangePropertyKey.DependencyProperty;

    public double NormalizedRange
    {
        get => (double)GetValue(NormalizedRangeProperty);
        internal set => SetValue(NormalizedRangePropertyKey, value);
    }

    #endregion
}
