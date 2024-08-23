using System.Windows.Automation.Peers;

namespace Wpf.Ui.Violeta.Controls;

public class PersonPictureAutomationPeer(PersonPicture owner) : FrameworkElementAutomationPeer(owner)
{
    protected override AutomationControlType GetAutomationControlTypeCore()
    {
        return AutomationControlType.Text;
    }

    protected override string GetClassNameCore()
    {
        return nameof(PersonPicture);
    }
}
