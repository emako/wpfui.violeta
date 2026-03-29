using System.Windows;

namespace iNKORE.UI.WPF.Modern.Common
{
    internal interface IControlProtected
    {
        DependencyObject GetTemplateChild(string childName);
    }
}
