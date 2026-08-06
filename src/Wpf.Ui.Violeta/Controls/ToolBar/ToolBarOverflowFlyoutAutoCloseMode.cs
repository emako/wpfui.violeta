namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Controls when the <see cref="ToolBar"/> overflow flyout dismisses after interaction.
/// </summary>
public enum ToolBarOverflowFlyoutAutoCloseMode
{
    /// <summary>
    /// Like a ComboBox dropdown: clicking outside closes the flyout.
    /// Clicking an overflow item that matches <see cref="ToolBarOverflowFlyoutAutoCloseTypes"/>
    /// (whitelist, not blacklist; whitelist defaults to <see cref="System.Windows.Controls.Primitives.ButtonBase"/>) also closes it.
    /// Nested buttons inside ComboBox / other hosts do not dismiss the flyout.
    /// </summary>
    Default = 0,

    /// <summary>
    /// Clicking outside closes the flyout; clicking any overflow item also closes it.
    /// </summary>
    Always = 1,

    /// <summary>
    /// Does not auto-close from outside clicks or item clicks; close only via the overflow button
    /// or <see cref="ToolBar.IsOverflowOpen"/>.
    /// </summary>
    Never = 2,
}
