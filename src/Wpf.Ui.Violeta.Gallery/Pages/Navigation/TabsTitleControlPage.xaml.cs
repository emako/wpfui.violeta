using System.Windows;
using LiteObservableLanguages;
using Wpf.Ui.Controls;
using Wpf.Ui.Violeta.Controls;
using Wpf.Ui.Violeta.Gallery.Globalization;

namespace Wpf.Ui.Violeta.Gallery.Pages.Navigation;

public partial class TabsTitleControlPage : Wpf.Ui.Violeta.Controls.Page
{
    private int _newTabCount;

    public TabsTitleControlPage()
    {
        InitializeComponent();
    }

    private void OnAddTab(object sender, RoutedEventArgs e)
    {
        _newTabCount++;
        var title = $"{LangKeys.Sample_c8a19ea6c7.Tr()} {_newTabCount}";
        var item = new TabsTitleControlItem
        {
            Header = title,
            Icon = new Wpf.Ui.Controls.SymbolIcon { Symbol = SymbolRegular.Document24 },
            Content = new System.Windows.Controls.TextBlock
            {
                Margin = new Thickness(16),
                Text = $"{title} — {LangKeys.Sample_1bedbb9cef.Tr()}",
            },
        };
        DemoTabs.Items.Add(item);
        DemoTabs.SelectedItem = item;
    }
}
