using System.Windows;
using Wpf.Ui.Violeta.Controls;

namespace Wpf.Ui.Violeta.Gallery.Pages.Status;

public partial class StorageRingPage : Page
{
    public StorageRingPage()
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
        DemoStorageRing.Value = value;
        string text = $"{value:F0}%";
        ValueLabel.Text = text;
        RingPercentText.Text = text;
    }
}
