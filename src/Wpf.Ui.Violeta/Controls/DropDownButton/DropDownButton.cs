using System.Windows;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Violeta wrapper around <see cref="Wpf.Ui.Controls.DropDownButton"/> that applies
/// <c>DropDownFlyoutContextMenuStyle</c> to the Flyout ContextMenu so the popup does not
/// cover the button (upstream UiContextMenu slides in from Y=-90).
/// </summary>
/// <remarks>
/// Existing <c>ui:DropDownButton</c> usage is covered by Hotfix +
/// <see cref="DropDownFlyoutAssist"/>; prefer this type for new <c>vio:</c> markup.
/// </remarks>
public class DropDownButton : Wpf.Ui.Controls.DropDownButton
{
    static DropDownButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(DropDownButton),
            new FrameworkPropertyMetadata(typeof(Wpf.Ui.Controls.DropDownButton)));
    }

    /// <inheritdoc />
    protected override void OnFlyoutChanged(object value)
    {
        base.OnFlyoutChanged(value);
        DropDownFlyoutHelper.ApplyFlyoutContextMenuStyle(this, value);
    }
}
