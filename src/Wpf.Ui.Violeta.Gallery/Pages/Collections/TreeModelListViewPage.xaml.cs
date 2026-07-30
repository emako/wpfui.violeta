using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Windows;
using Wpf.Ui.Controls;
using Wpf.Ui.Violeta.Controls;

namespace Wpf.Ui.Violeta.Gallery.Pages.Collections;

public partial class TreeModelListViewPage : Wpf.Ui.Violeta.Controls.Page
{
    public TreeModelCollection<TreeTestModel> TreeTestModel { get; set; } = CreateTestModel();

    public TreeModelListViewPage()
    {
        InitializeComponent();
        DataContext = this;
    }

    private void AddTreeModelRow_Click(object sender, RoutedEventArgs e)
    {
        TreeTestModel.Add(new TreeTestModel
        {
            Column1 = "新增 " + global::System.DateTime.Now.ToString("HH:mm:ss"),
            Column2 = "数据2",
            Column3 = "数据3",
        });
    }

    private void RemoveTreeModelRow_Click(object sender, RoutedEventArgs e)
    {
        if (TreeTestModel.Count > 0)
        {
            TreeTestModel.RemoveAt(0);
        }
    }

    private void ClearTreeModel_Click(object sender, RoutedEventArgs e)
    {
        TreeTestModel.Clear();
    }

    private static TreeModelCollection<TreeTestModel> CreateTestModel()
    {
        return new TreeModelCollection<TreeTestModel>
        {
            Children =
            [
                new()
                {
                    Column1 = "项目 A",
                    Column2 = "进行中",
                    Column3 = "2024-01-01",
                    Children =
                    [
                        new()
                        {
                            Column1 = "任务 A-1",
                            Column2 = "已完成",
                            Column3 = "2024-02-01",
                        },
                        new()
                        {
                            Column1 = "任务 A-2",
                            Column2 = "进行中",
                            Column3 = "2024-02-15",
                        },
                    ],
                },
                new()
                {
                    Column1 = "项目 B",
                    Column2 = "计划中",
                    Column3 = "2024-03-01",
                },
            ],
        };
    }
}

[ObservableObject]
public partial class TreeTestModel : TreeModelObject<TreeTestModel>
{
    [ObservableProperty]
    public partial string? Column1 { get; set; }

    [ObservableProperty]
    public partial string? Column2 { get; set; }

    [ObservableProperty]
    public partial string? Column3 { get; set; }

    [ObservableProperty]
    public partial bool IsChecked { get; set; } = false;
}
