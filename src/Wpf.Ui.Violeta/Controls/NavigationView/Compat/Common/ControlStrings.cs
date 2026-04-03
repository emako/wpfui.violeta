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

        // NavigationView shared strings are now centralized under Resources/Localization.
        _ = controlType;
        return "Wpf.Ui.Violeta.Resources.Localization.Resources";
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
