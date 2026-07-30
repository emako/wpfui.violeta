using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using Wpf.Ui.Violeta.Controls;

namespace Wpf.Ui.Violeta.Gallery.Pages.Collections;

public partial class TreeListViewPage : Wpf.Ui.Violeta.Controls.Page
{
    public ObservableCollection<Staff> StaffList { get; set; } = [];

    public TreeListViewPage()
    {
        InitializeComponent();
        DataContext = this;
        Loaded += (_, _) => InitStaffData();
    }

    private void InitStaffData()
    {
        if (StaffList.Count > 0)
        {
            return;
        }

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
                Name = "新员工 " + global::System.DateTime.Now.ToString("HH:mm:ss"),
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
}

public partial class Staff : ObservableObject
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
