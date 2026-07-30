using System.Windows;
using Wpf.Ui.Violeta.Controls;

namespace Wpf.Ui.Violeta.Gallery.Pages.Navigation;

public partial class BreadcrumbBarPage : Page
{
    private static readonly string[] DefaultFolders = ["This PC", "Documents", "Projects", "Wpf.Ui.Violeta"];

    public BreadcrumbBarPage()
    {
        InitializeComponent();

        StringBreadcrumbBar.ItemsSource = new[] { "Home", "Documents", "Project", "File.txt" };
        FolderBreadcrumbBar.ItemsSource = DefaultFolders;
    }

    private void ResetFolders_Click(object sender, RoutedEventArgs e)
    {
        FolderBreadcrumbBar.ItemsSource = DefaultFolders;
    }
}
