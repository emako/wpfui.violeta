#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8618, CS8619, CS8625

namespace Wpf.Ui.Violeta.Controls.Compat;

/// <summary>
/// Defines constants that specify the password reveal behavior of a PasswordBox.
/// </summary>
public enum PasswordRevealMode
{
    /// <summary>
    /// The password reveal button is visible. The password is not obscured while the
    /// button is pressed.
    /// </summary>
    Peek = 0,

    /// <summary>
    /// The password reveal button is not visible. The password is always obscured.
    /// </summary>
    Hidden = 1,

    /// <summary>
    /// The password reveal button is not visible. The password is not obscured.
    /// </summary>
    Visible = 2,
}
