using System.Windows;
using System.Windows.Controls;
using LiteObservableLanguages;
using Wpf.Ui.Violeta.Gallery.Globalization;

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
        MultiComboBoxDemo.ItemsSource = new[] { LangKeys.Sample_e6803e21b9.Tr(), LangKeys.Sample_b7c03bbf2b.Tr(), LangKeys.Sample_0905182530.Tr(), LangKeys.Sample_4e9244f80e.Tr(), LangKeys.Sample_a96ab3d9cc.Tr(), LangKeys.Sample_b9af3fd5d3.Tr() };
    }

    private void MultiComboBoxDemo_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (MultiComboBoxDemo.MultiSelectedItems.Count == 0)
        {
            MultiComboBoxResultText.Text = LangKeys.Sample_b5c92782c9.Tr();
        }
        else
        {
            MultiComboBoxResultText.Text = LangKeys.Sample_986ffb30e2.Tr() + string.Join("、", MultiComboBoxDemo.MultiSelectedItems);
        }
    }
}
