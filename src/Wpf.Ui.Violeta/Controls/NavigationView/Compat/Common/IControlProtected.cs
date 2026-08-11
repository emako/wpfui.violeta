using System.Windows;

namespace Wpf.Ui.Violeta.Controls.Compat;

internal interface IControlProtected
{
    public DependencyObject GetTemplateChild(string childName);
}
