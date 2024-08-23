using System.Windows;

namespace Wpf.Ui.Violeta.Controls;

internal interface IControlProtected
{
    public DependencyObject GetTemplateChild(string childName);
}
