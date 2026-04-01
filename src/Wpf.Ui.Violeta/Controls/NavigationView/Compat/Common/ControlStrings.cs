using System;
using System.Reflection;

namespace Wpf.Ui.Violeta.Controls.Compat;

internal class ControlStrings : ResourceAccessor
{
    public ControlStrings(Type controlType, ModernControlCategory category) : base(GetControlBaseName(controlType, category), GetControlAssembly(controlType))
    {
    }


    internal static string GetControlBaseName(Type controlType, ModernControlCategory category)
    {
        _ = category;

        // Resource manifest names follow the control's CLR namespace,
        // e.g. Wpf.Ui.Violeta.Controls.NavigationView.Strings.Resources.
        string controlFullName = controlType.FullName ?? controlType.Name;
        return $"{controlFullName}.Strings.Resources";
    }

    internal static Assembly GetControlAssembly(Type controlType)
    {
        return controlType.Assembly;
    }
}

internal enum ModernControlCategory
{
    Windows,
    Community,
    Extended,
}
