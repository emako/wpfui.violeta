using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;

namespace Wpf.Ui.Violeta.Automation;

public static class ButtonExtension
{
    /// <summary>
    /// Invokes the button as if it were clicked by the user.
    /// </summary>
    /// <remarks>
    /// This method uses UI Automation (<see cref="ButtonAutomationPeer"/>) to invoke the button,
    /// which will trigger the full click behavior, including:
    /// <list type="bullet">
    /// <item><description>Raising the <see cref="Button.Click"/> event</description></item>
    /// <item><description>Executing the bound <see cref="System.Windows.Controls.Primitives.ButtonBase.Command"/></description></item>
    /// <item><description>Respecting <c>CanExecute</c> logic</description></item>
    /// </list>
    ///
    /// Unlike directly raising the <see cref="Button.Click"/> event, this approach more closely
    /// simulates an actual user interaction.
    /// </remarks>
    /// <param name="button">The target <see cref="Button"/> to invoke.</param>
    public static void InvokeClick(this Button self)
    {
        // Keywords: InvokeClick, RaiseClick, PerformClick, SimulateClick
        if (new ButtonAutomationPeer(self)
            .GetPattern(PatternInterface.Invoke) is IInvokeProvider provider)
        {
            provider?.Invoke();
        }
    }
}
