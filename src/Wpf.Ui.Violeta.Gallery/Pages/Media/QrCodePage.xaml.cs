using System.Windows;
using System.Windows.Controls;

namespace Wpf.Ui.Violeta.Gallery.Pages.Media;

public partial class QrCodePage : Wpf.Ui.Violeta.Controls.Page
{
    public QrCodePage()
    {
        InitializeComponent();
    }

    private void QrDataTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        if (!IsLoaded)
        {
            return;
        }

        DemoQrCode.Data = QrDataTextBox.Text;
    }

    private void CornerRatioSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!IsLoaded)
        {
            return;
        }

        DemoQrCode.SymbolCornerRatio = CornerRatioSlider.Value;
    }
}
