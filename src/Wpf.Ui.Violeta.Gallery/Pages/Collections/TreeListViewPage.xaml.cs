using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using Wpf.Ui.Violeta.Controls;
using LiteObservableLanguages;
using Wpf.Ui.Violeta.Gallery.Globalization;

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
            Name = LangKeys.Sample_615db57aa3.Tr(),
            Age = 35,
            Sex = LangKeys.Sample_36a4908a55.Tr(),
            Duty = LangKeys.Sample_1697f28005.Tr(),
            IsChecked = true,
        };

        manager.StaffList.Add(new Staff
        {
            Name = LangKeys.Sample_36c942351e.Tr(),
            Age = 28,
            Sex = LangKeys.Sample_36a4908a55.Tr(),
            Duty = LangKeys.Sample_132ab8784b.Tr(),
            IsChecked = true,
        });

        manager.StaffList.Add(new Staff
        {
            Name = LangKeys.Sample_3228f322c9.Tr(),
            Age = 26,
            Sex = LangKeys.Sample_87c835a6b1.Tr(),
            Duty = LangKeys.Sample_98bc1a09b5.Tr(),
            IsChecked = true,
        });

        var manager2 = new Staff
        {
            Name = LangKeys.Sample_b43536d046.Tr(),
            Age = 38,
            Sex = LangKeys.Sample_87c835a6b1.Tr(),
            Duty = LangKeys.Sample_e74804e480.Tr(),
            IsChecked = true,
        };

        manager2.StaffList.Add(new Staff
        {
            Name = LangKeys.Sample_52a48bb45a.Tr(),
            Age = 30,
            Sex = LangKeys.Sample_36a4908a55.Tr(),
            Duty = LangKeys.Sample_4647705c65.Tr(),
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
                Name = LangKeys.Sample_4d0e6c6f67.Tr() + global::System.DateTime.Now.ToString("HH:mm:ss"),
                Age = 25,
                Sex = LangKeys.Sample_36a4908a55.Tr(),
                Duty = LangKeys.Sample_8a0f7479e5.Tr(),
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
