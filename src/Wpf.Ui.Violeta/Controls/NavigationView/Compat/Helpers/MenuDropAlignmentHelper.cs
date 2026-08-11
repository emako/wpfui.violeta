using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows;

namespace Wpf.Ui.Violeta.Controls.Compat;

public static class MenuDropAlignmentHelper
{
    private static readonly FieldInfo _menuDropAlignmentField;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    static MenuDropAlignmentHelper()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        try
        {
            _menuDropAlignmentField = typeof(SystemParameters).GetField("_menuDropAlignment", BindingFlags.NonPublic | BindingFlags.Static)!;
        }
        catch (Exception)
        {
        }

        Debug.Assert(_menuDropAlignmentField != null);
        if (_menuDropAlignmentField != null)
        {
            EnsureStandardPopupAlignment();
            SystemParameters.StaticPropertyChanged += SystemParameters_StaticPropertyChanged;
        }
    }

    public static void EnsureStandardPopupAlignment()
    {
        if (SystemParameters.MenuDropAlignment)
        {
            try
            {
                _menuDropAlignmentField.SetValue(null, false);
            }
            catch (Exception)
            {
            }

            Debug.Assert(!SystemParameters.MenuDropAlignment);
        }
    }

    private static void SystemParameters_StaticPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SystemParameters.MenuDropAlignment))
        {
            EnsureStandardPopupAlignment();
        }
    }
}
