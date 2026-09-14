using System.Windows;
using Wpf.Ui.Violeta.Controls;

namespace Wpf.Ui.Violeta.Gallery.Pages.Status;

public partial class StorageBarPage : Page
{
    public StorageBarPage()
    {
        InitializeComponent();
        Loaded += (_, _) => ApplyValue(ValueSlider.Value);
    }

    private void OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!IsLoaded)
        {
            return;
        }

        ApplyValue(e.NewValue);
    }

    private void ApplyValue(double value)
    {
        DemoStorageBar.Value = value;
        ValueLabel.Text = $"{value:F0}%";
    }
}
