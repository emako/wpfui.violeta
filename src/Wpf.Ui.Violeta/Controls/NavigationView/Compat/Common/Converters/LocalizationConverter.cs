using System;
using System.Globalization;
using System.Windows.Data;

namespace Wpf.Ui.Violeta.Controls.Compat;

public class LocalizeConverter : IValueConverter
{
    private ResourceAccessor _resourceAccessor = null!;
    private Type _controlType = null!;

    public Type ControlType
    {
        get => _controlType;
        set
        {
            _controlType = value;
            _resourceAccessor = new ResourceAccessor(_controlType);
        }
    }

    public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        return _resourceAccessor?.GetLocalizedStringResource(value as string);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
