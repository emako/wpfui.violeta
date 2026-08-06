using System.Threading.Tasks;
using System.Windows;
using LiteObservableLanguages;
using Wpf.Ui.Violeta.Gallery.Globalization;

namespace Wpf.Ui.Violeta.Gallery.Pages.OpSystem;

public partial class ClipboardPage : Wpf.Ui.Violeta.Controls.Page
{
    public ClipboardPage()
    {
        InitializeComponent();
    }

    private async void CopyTextToClipboard_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Clipboard.Clear();
            Clipboard.SetText(TextToCopyTextBox.Text);
        }
        catch
        {
            return;
        }

        if (TextCopiedNotice.Visibility == Visibility.Visible)
        {
            return;
        }

        TextCopiedNotice.Visibility = Visibility.Visible;

        await Task.Delay(5000);

        TextCopiedNotice.Visibility = Visibility.Collapsed;
    }

    private void PasteTextFromClipboard_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            ClipboardContentText.Text = Clipboard.GetText();
        }
        catch
        {
            ClipboardContentText.Text = LangKeys.Sample_ccdded2782.Tr();
        }
    }
}
