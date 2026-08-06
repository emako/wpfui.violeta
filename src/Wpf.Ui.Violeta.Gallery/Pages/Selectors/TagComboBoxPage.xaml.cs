using System.Windows;
using LiteObservableLanguages;
using Wpf.Ui.Violeta.Gallery.Globalization;

namespace Wpf.Ui.Violeta.Gallery.Pages.Selectors;

public partial class TagComboBoxPage : Wpf.Ui.Violeta.Controls.Page
{
    public TagComboBoxPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        TagComboBoxDemo.ItemsSource = new[] { LangKeys.Sample_9abfe4a039.Tr(), LangKeys.Sample_e778d61ae4.Tr(), "DevOps", "UI/UX", LangKeys.Sample_c95e748d58.Tr(), LangKeys.Sample_68051bf4aa.Tr() };
    }
}
