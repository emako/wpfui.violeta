using System.Diagnostics;
using System.Windows.Navigation;
using Wpf.Ui.Violeta.Controls;

namespace Wpf.Ui.Violeta.Gallery.Pages.Text;

public partial class FlowDocumentPage : Page
{
    public FlowDocumentPage()
    {
        InitializeComponent();
    }

    private void OnHyperlinkRequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
        e.Handled = true;
    }
}
