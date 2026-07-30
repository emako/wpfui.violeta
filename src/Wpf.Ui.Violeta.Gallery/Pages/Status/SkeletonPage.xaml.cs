using System.Windows;

namespace Wpf.Ui.Violeta.Gallery.Pages.Status;

public partial class SkeletonPage : Wpf.Ui.Violeta.Controls.Page
{
    public SkeletonPage()
    {
        InitializeComponent();
    }

    private void LoadingState_Changed(object sender, RoutedEventArgs e)
    {
        if (!IsLoaded)
        {
            return;
        }

        var loading = LoadingCheckBox.IsChecked == true;
        var active = ActiveCheckBox.IsChecked == true;

        DemoSkeleton.IsLoading = loading;
        DemoSkeleton.IsActive = active;
        DemoSkeleton2.IsLoading = loading;
        DemoSkeleton2.IsActive = active;
        DemoSkeleton3.IsLoading = loading;
        DemoSkeleton3.IsActive = active;
    }
}
