using System.Windows;
using Wpf.Ui.Violeta.Controls;

namespace Wpf.Ui.Violeta.Gallery.Pages.Selectors;

public partial class CascadingComboBoxPage : Wpf.Ui.Violeta.Controls.Page
{
    public CascadingComboBoxPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        CascadingComboBoxDemo.ItemsSource = new ICascadingItem[]
        {
            new CascadingItem("广东",
            [
                new CascadingItem("广州"),
                new CascadingItem("深圳"),
                new CascadingItem("东莞"),
                new CascadingItem("佛山"),
            ]),
            new CascadingItem("浙江",
            [
                new CascadingItem("杭州"),
                new CascadingItem("宁波"),
                new CascadingItem("温州"),
                new CascadingItem("嘉兴"),
            ]),
            new CascadingItem("江苏",
            [
                new CascadingItem("南京"),
                new CascadingItem("苏州"),
                new CascadingItem("无锡"),
                new CascadingItem("常州"),
            ]),
        };
    }
}
