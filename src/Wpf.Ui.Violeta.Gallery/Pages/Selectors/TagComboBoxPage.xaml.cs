using System.Windows;

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
        TagComboBoxDemo.ItemsSource = new[] { "前端", "后端", "DevOps", "UI/UX", "移动端", "数据库" };
    }
}
