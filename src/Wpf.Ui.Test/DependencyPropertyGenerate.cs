using System.Windows;
using Wpf.Ui.DependencyPropertyGenerator;

namespace Wpf.Ui.Test;

internal sealed partial class DependencyPropertyGenerate : DependencyObject
{
    [DependencyProperty(DefaultValue = 42, PropertyChanged = nameof(OnCountChanged))]
    public int Count { get; set; }

    private static void OnCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // ...
    }
}
