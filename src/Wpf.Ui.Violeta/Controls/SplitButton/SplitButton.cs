using System.Windows;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Violeta wrapper around <see cref="Wpf.Ui.Controls.SplitButton"/> that applies
/// <c>DropDownFlyoutContextMenuStyle</c> to the Flyout ContextMenu so the popup does not
/// cover the button (upstream UiContextMenu slides in from Y=-90).
/// </summary>
/// <remarks>
/// Existing <c>ui:SplitButton</c> usage is covered by Hotfix +
/// <see cref="DropDownFlyoutAssist"/>; prefer this type for new <c>vio:</c> markup.
/// </remarks>
public class SplitButton : Wpf.Ui.Controls.SplitButton
{
    static SplitButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(SplitButton),
            new FrameworkPropertyMetadata(typeof(Wpf.Ui.Controls.SplitButton)));
    }

    /// <inheritdoc />
    protected override void OnFlyoutChanged(object value)
    {
        base.OnFlyoutChanged(value);
        DropDownFlyoutHelper.ApplyFlyoutContextMenuStyle(this, value);
    }
}
