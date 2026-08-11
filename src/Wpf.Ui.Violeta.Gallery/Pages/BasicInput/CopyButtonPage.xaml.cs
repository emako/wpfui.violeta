using System.Windows;

namespace Wpf.Ui.Violeta.Gallery.Pages.BasicInput;

public partial class CopyButtonPage : Wpf.Ui.Violeta.Controls.Page
{
    public CopyButtonPage()
    {
        InitializeComponent();
    }

    private void CustomCopyButton_Click(object sender, RoutedEventArgs e)
    {
        Win32.Clipboard.SetText(CustomTextBox.Text);
    }
}
