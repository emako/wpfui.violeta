using System;
using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Violeta.Controls;

namespace Wpf.Ui.Violeta.Gallery.Pages.Collections;

public record SampleEmployee(string Name, string Department, int Score, string Status);

public partial class DataGridPage : Wpf.Ui.Violeta.Controls.Page
{
    public DataGridPage()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            SampleDataGrid.ItemsSource = new[]
            {
                new SampleEmployee("张伟", "研发部", 92, "在职"),
                new SampleEmployee("李娜", "设计部", 88, "在职"),
                new SampleEmployee("王芳", "产品部", 95, "在职"),
                new SampleEmployee("刘洋", "运营部", 76, "在职"),
                new SampleEmployee("陈静", "测试部", 83, "休假"),
                new SampleEmployee("赵磊", "研发部", 91, "在职"),
                new SampleEmployee("周梅", "市场部", 79, "离职"),
                new SampleEmployee("吴鑫", "研发部", 97, "在职"),
            };
        };
    }
}
