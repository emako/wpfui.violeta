using System.Windows;
using Wpf.Ui.Violeta.Controls.Compat;

static class CppWinRTHelpers
{
    public static WinRTReturn GetTemplateChildT<WinRTReturn>(string childName, IControlProtected controlProtected) where WinRTReturn : DependencyObject
    {
        DependencyObject childAsDO = controlProtected.GetTemplateChild(childName);

        if (childAsDO != null)
        {
            return childAsDO as WinRTReturn;
        }
        return null;
    }
}
