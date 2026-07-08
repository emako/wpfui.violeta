using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using Wpf.Ui.Controls;

namespace Wpf.Ui.Violeta.Gallery.Pages;

public record SampleEmployee(string Name, string Department, int Score, string Status);

public partial class DataPage : Wpf.Ui.Violeta.Controls.Page
{
    public ObservableCollection<Staff> StaffList { get; set; } = [];
    public TreeModelCollection<TreeTestModel> TreeTestModel { get; set; } = CreateTestModel();

    public DataPage()
    {
        InitializeComponent();
        DataContext = this;
        Loaded += (_, _) => LoadData();
    }

    private void LoadData()
    {
        // DataGrid 数据
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

        // TreeListView 数据
        InitStaffData();
    }

    private void InitStaffData()
    {
        var manager = new Staff
        {
            Name = "张三",
            Age = 35,
            Sex = "男",
            Duty = "部门经理",
            IsChecked = true,
        };

        manager.StaffList.Add(new Staff
        {
            Name = "李四",
            Age = 28,
            Sex = "男",
            Duty = "高级工程师",
            IsChecked = true,
        });

        manager.StaffList.Add(new Staff
        {
            Name = "王五",
            Age = 26,
            Sex = "女",
            Duty = "工程师",
            IsChecked = true,
        });

        var manager2 = new Staff
        {
            Name = "赵六",
            Age = 38,
            Sex = "女",
            Duty = "技术总监",
            IsChecked = true,
        };

        manager2.StaffList.Add(new Staff
        {
            Name = "孙七",
            Age = 30,
            Sex = "男",
            Duty = "架构师",
            IsChecked = true,
        });

        StaffList.Add(manager);
        StaffList.Add(manager2);
    }

    private void AddStaffNode_Click(object sender, RoutedEventArgs e)
    {
        if (StaffList.Count > 0)
        {
            StaffList[0].StaffList.Add(new Staff
            {
                Name = "新员工 " + DateTime.Now.ToString("HH:mm:ss"),
                Age = 25,
                Sex = "男",
                Duty = "初级工程师",
                IsChecked = true,
            });
        }
    }

    private void ChangeStaffNode_Click(object sender, RoutedEventArgs e)
    {
        foreach (var staff in StaffList)
        {
            staff.Age++;
        }
    }

    private void AddTreeModelRow_Click(object sender, RoutedEventArgs e)
    {
        TreeTestModel.Add(new TreeTestModel
        {
            Column1 = "新增 " + DateTime.Now.ToString("HH:mm:ss"),
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
public partial class Staff
{
    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;

    [ObservableProperty]
    public partial int Age { get; set; }

    [ObservableProperty]
    public partial string Sex { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Duty { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsChecked { get; set; } = true;

    [ObservableProperty]
    public partial ObservableCollection<Staff> StaffList { get; set; } = [];
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
