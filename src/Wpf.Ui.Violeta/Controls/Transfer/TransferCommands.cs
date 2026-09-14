using System.Windows.Input;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Commands used by <see cref="Transfer"/> move buttons.
/// </summary>
public static class TransferCommands
{
    /// <summary>Move selected source items to the target list.</summary>
    public static readonly RoutedCommand Selected = new(nameof(Selected), typeof(TransferCommands));

    /// <summary>Move selected target items back to the source list.</summary>
    public static readonly RoutedCommand Cancel = new(nameof(Cancel), typeof(TransferCommands));
}
