using System;
using System.Windows.Media;
using System.Windows.Threading;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Options used when showing a <see cref="Growl"/> notification.
/// </summary>
public class GrowlInfo
{
    public string? Message { get; set; }

    public bool ShowDateTime { get; set; } = true;

    /// <summary>Auto-close delay in seconds (minimum 2). Ignored when <see cref="StaysOpen"/> is true.</summary>
    public int WaitTime { get; set; } = 6;

    public string CancelStr { get; set; } = "Cancel";

    public string ConfirmStr { get; set; } = "Confirm";

    /// <summary>
    /// Invoked before close. Parameter is <c>true</c> for Confirm, otherwise Cancel/Close.
    /// Return <c>false</c> to abort closing.
    /// </summary>
    public Func<bool, bool>? ActionBeforeClose { get; set; }

    public bool StaysOpen { get; set; }

    /// <summary>When true, caller-supplied icon / brush are kept unless null.</summary>
    public bool IsCustom { get; set; }

    public GrowlType Type { get; set; }

    /// <summary>Segoe Fluent Icons glyph (see <see cref="FontSymbols"/>).</summary>
    public string? Icon { get; set; }

    public Brush? IconBrush { get; set; }

    public bool ShowCloseButton { get; set; } = true;

    public string? Token { get; set; }

    public Dispatcher? Dispatcher { get; set; }
}
