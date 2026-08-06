using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Windows;
using Wpf.Ui.Controls;
using Wpf.Ui.Violeta.Controls;
using LiteObservableLanguages;
using Wpf.Ui.Violeta.Gallery.Globalization;

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
            Column1 = LangKeys.Sample_1bb125b89c.Tr() + global::System.DateTime.Now.ToString("HH:mm:ss"),
            Column2 = LangKeys.Sample_eeda260c6a.Tr(),
            Column3 = LangKeys.Sample_4053a05691.Tr(),
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
                    Column1 = LangKeys.Sample_416b31af1e.Tr(),
                    Column2 = LangKeys.Sample_fb852fc6cc.Tr(),
                    Column3 = "2024-01-01",
                    Children =
                    [
                        new()
                        {
                            Column1 = LangKeys.Sample_32ca86d11b.Tr(),
                            Column2 = LangKeys.Sample_fad5222ca0.Tr(),
                            Column3 = "2024-02-01",
                        },
                        new()
                        {
                            Column1 = LangKeys.Sample_602f80825c.Tr(),
                            Column2 = LangKeys.Sample_fb852fc6cc.Tr(),
                            Column3 = "2024-02-15",
                        },
                    ],
                },
                new()
                {
                    Column1 = LangKeys.Sample_048547bbb0.Tr(),
                    Column2 = LangKeys.Sample_4daea5b39d.Tr(),
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
