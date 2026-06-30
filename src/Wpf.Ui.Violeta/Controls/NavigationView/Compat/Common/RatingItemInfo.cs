#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8618, CS8619, CS8625

using System.Windows;

namespace Wpf.Ui.Violeta.Controls.Compat;

/// <summary>
/// Represents information about the visual states of the elements that represent
/// a rating.
/// </summary>
public class RatingItemInfo : Freezable
{
    /// <summary>
    /// Initializes a new instance of the RatingItemInfo class.
    /// </summary>
    public RatingItemInfo()
    {
    }

    protected override Freezable CreateInstanceCore()
    {
        return new RatingItemInfo();
    }
}
