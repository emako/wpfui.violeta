using System.Windows;

namespace Wpf.Ui.Violeta.Controls.Compat;

internal interface IControlProtected
{
    DependencyObject GetTemplateChild(string childName);
}
