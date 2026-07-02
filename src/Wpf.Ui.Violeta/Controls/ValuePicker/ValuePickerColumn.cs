using System.Collections.Generic;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Defines one scrollable column in <see cref="ValuePicker"/>.
/// </summary>
public class ValuePickerColumn
{
    /// <summary>
    /// Placeholder text shown in the flyout button when nothing is selected.
    /// </summary>
    public string? Placeholder { get; set; }

    /// <summary>
    /// The selectable values for this column.
    /// </summary>
    public IList<string> Items { get; set; } = [];

    /// <summary>
    /// Whether the column wraps from last item back to the first.
    /// </summary>
    public bool ShouldLoop { get; set; } = true;
}
