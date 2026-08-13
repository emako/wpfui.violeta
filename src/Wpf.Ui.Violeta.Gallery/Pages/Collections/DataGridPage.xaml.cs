using System;
using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Violeta.Controls;
using LiteObservableLanguages;
using Wpf.Ui.Violeta.Gallery.Globalization;

namespace Wpf.Ui.Violeta.Gallery.Pages.Collections;

public record SampleEmployee(string Name, string Department, int Score, string Status);

public partial class DataGridPage : Wpf.Ui.Violeta.Controls.Page
{
    public DataGridPage()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            var items = new[]
            {
                new SampleEmployee(LangKeys.Sample_305be79653.Tr(), LangKeys.Sample_9176a628cc.Tr(), 92, LangKeys.Sample_b5509be4c5.Tr()),
                new SampleEmployee(LangKeys.Sample_b1db96a48b.Tr(), LangKeys.Sample_829ec9c321.Tr(), 88, LangKeys.Sample_b5509be4c5.Tr()),
                new SampleEmployee(LangKeys.Sample_1039106987.Tr(), LangKeys.Sample_c5d34b60ac.Tr(), 95, LangKeys.Sample_b5509be4c5.Tr()),
                new SampleEmployee(LangKeys.Sample_49c531db9a.Tr(), LangKeys.Sample_b890b34994.Tr(), 76, LangKeys.Sample_b5509be4c5.Tr()),
                new SampleEmployee(LangKeys.Sample_57a0dec9ef.Tr(), LangKeys.Sample_36e41c1627.Tr(), 83, LangKeys.Sample_62a8cf0af7.Tr()),
                new SampleEmployee(LangKeys.Sample_ded8c0a3ed.Tr(), LangKeys.Sample_9176a628cc.Tr(), 91, LangKeys.Sample_b5509be4c5.Tr()),
                new SampleEmployee(LangKeys.Sample_e90b708ea5.Tr(), LangKeys.Sample_73b1110542.Tr(), 79, LangKeys.Sample_583e7924e9.Tr()),
                new SampleEmployee(LangKeys.Sample_9a201c3894.Tr(), LangKeys.Sample_9176a628cc.Tr(), 97, LangKeys.Sample_b5509be4c5.Tr()),
            };

            SampleDataGrid.ItemsSource = items;
            HorizontalGridLinesDataGrid.ItemsSource = items;
            VerticalGridLinesDataGrid.ItemsSource = items;
            AllGridLinesDataGrid.ItemsSource = items;
        };
    }
}
