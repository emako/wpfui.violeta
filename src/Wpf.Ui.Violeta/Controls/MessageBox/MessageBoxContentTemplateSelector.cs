using System;
using System.Windows;
using System.Windows.Controls;

namespace Wpf.Ui.Violeta.Controls;

internal sealed class MessageBoxContentTemplateSelector : DataTemplateSelector
{
    public DataTemplate? StringTemplate { get; set; }
    public DataTemplate? DefaultTemplate { get; set; }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        if (item is string)
        {
            return StringTemplate ?? throw new InvalidOperationException(nameof(StringTemplate));
        }

        return DefaultTemplate ?? throw new InvalidOperationException(nameof(DefaultTemplate));
    }
}
