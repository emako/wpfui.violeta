using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Violeta.Controls;

namespace Wpf.Ui.Violeta.Gallery.Pages.BasicInput;

public partial class LoadingButtonPage : Wpf.Ui.Violeta.Controls.Page
{
    public LoadingButtonPage()
    {
        InitializeComponent();
    }

    private async void LoadingButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not LoadingButton button || button.IsLoading)
        {
            return;
        }

        button.IsLoading = true;
        try
        {
            await Task.Delay(2000);
        }
        finally
        {
            button.IsLoading = false;
        }
    }
}
