using System.Windows;
using System.Windows.Controls.Primitives;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Placement and appearance helpers for <c>CalloutToolTipStyle</c>.
/// </summary>
/// <example>
/// <code lang="xml">
/// &lt;ToolTip
///     Style="{StaticResource CalloutToolTipStyle}"
///     vio:CalloutToolTipHelper.ForceDark="True"
///     Content="Always dark" /&gt;
/// </code>
/// </example>
public static class CalloutToolTipHelper
{
    public static readonly CustomPopupPlacementCallback TopCenterPlacementCallback = PlaceTopCenter;

    public static CustomPopupPlacement[] PlaceTopCenter(Size popupSize, Size targetSize, Point offset)
    {
        // Placement=Top left-aligns the popup; center so the bottom triangle points at the target.
        var point = new Point(
            ((targetSize.Width - popupSize.Width) / 2) + offset.X,
            -popupSize.Height + offset.Y);

        return [new CustomPopupPlacement(point, PopupPrimaryAxis.Horizontal)];
    }

    /// <summary>
    /// Identifies the <see cref="ForceDarkProperty"/> attached property.
    /// </summary>
    public static readonly DependencyProperty ForceDarkProperty =
        DependencyProperty.RegisterAttached(
            "ForceDark",
            typeof(bool),
            typeof(CalloutToolTipHelper),
            new PropertyMetadata(false));

    /// <summary>
    /// Gets whether the callout tooltip uses the dark appearance regardless of the app theme.
    /// </summary>
    public static bool GetForceDark(DependencyObject element) =>
        (bool)element.GetValue(ForceDarkProperty);

    /// <summary>
    /// Sets whether the callout tooltip uses the dark appearance regardless of the app theme.
    /// </summary>
    public static void SetForceDark(DependencyObject element, bool value) =>
        element.SetValue(ForceDarkProperty, value);
}
