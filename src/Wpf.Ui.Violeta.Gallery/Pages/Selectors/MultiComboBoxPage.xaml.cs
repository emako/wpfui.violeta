using System.Windows;
using System.Windows.Controls;

namespace Wpf.Ui.Violeta.Gallery.Pages.Selectors;

public partial class MultiComboBoxPage : Wpf.Ui.Violeta.Controls.Page
{
    public MultiComboBoxPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        MultiComboBoxDemo.ItemsSource = new[] { "苹果", "香蕉", "樱桃", "草莓", "蓝莓", "西瓜" };
    }

    private void MultiComboBoxDemo_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (MultiComboBoxDemo.MultiSelectedItems.Count == 0)
        {
            MultiComboBoxResultText.Text = "已选择：(无)";
        }
        else
        {
            MultiComboBoxResultText.Text = "已选择：" + string.Join("、", MultiComboBoxDemo.MultiSelectedItems);
        }
    }
}
