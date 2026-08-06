using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Violeta.Controls;
using ContentDialog = Wpf.Ui.Violeta.Controls.ContentDialog;
using ContentDialogButton = Wpf.Ui.Violeta.Controls.ContentDialogButton;
using Wpf.Ui.Violeta.Gallery.Globalization;
using LiteObservableLanguages;

namespace Wpf.Ui.Violeta.Gallery.Pages.Dialogs;

public partial class ContentDialogPage : Wpf.Ui.Violeta.Controls.Page
{
    public ContentDialogPage()
    {
        InitializeComponent();
    }

    private async void ShowContentDialog_Click(object sender, RoutedEventArgs e)
    {
        ContentDialog dialog = new()
        {
            Title = LangKeys.Sample_ae05e054a4.Tr(),
            Content = LangKeys.Sample_f330915173.Tr(),
            CloseButtonText = LangKeys.Sample_b15d91274e.Tr(),
            PrimaryButtonText = LangKeys.Sample_e83a256e4f.Tr(),
            SecondaryButtonText = LangKeys.Sample_bb86dd5c6e.Tr(),
            DefaultButton = ContentDialogButton.Primary,
        };

        var result = await dialog.ShowAsync();
        ContentDialogResultText.Text = LangKeys.Format_Result.Tr(result);
    }

    private async void ShowContentDialogCustom_Click(object sender, RoutedEventArgs e)
    {
        var stack = new StackPanel { Margin = new Thickness(0, 8, 0, 0) };
        stack.Children.Add(new TextBlock { Text = LangKeys.Sample_b15e5d18e7.Tr(), Margin = new Thickness(0, 0, 0, 8) });
        stack.Children.Add(new Wpf.Ui.Controls.TextBox { PlaceholderText = LangKeys.Sample_f5a481d0ae.Tr(), MinWidth = 200 });

        ContentDialog dialog = new()
        {
            Title = LangKeys.Sample_acf041436a.Tr(),
            Content = stack,
            CloseButtonText = LangKeys.Sample_625fb26b4b.Tr(),
            PrimaryButtonText = LangKeys.Sample_38cf16f220.Tr(),
            DefaultButton = ContentDialogButton.Primary,
        };

        var result = await dialog.ShowAsync();
        ContentDialogResultText.Text = LangKeys.Format_Result.Tr(result);
    }
}
