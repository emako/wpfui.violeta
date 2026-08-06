using System.Windows;
using Wpf.Ui.Violeta.Controls;
using Wpf.Ui.Violeta.Gallery.Globalization;
using LiteObservableLanguages;

namespace Wpf.Ui.Violeta.Gallery.Pages.Selectors;

public partial class ValuePickerPage : Wpf.Ui.Violeta.Controls.Page
{
    public ValuePickerPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        ValuePickerDemo.Columns =
        [
            new ValuePickerColumn
            {
                Placeholder = LangKeys.Sample_966e7cdd84.Tr(),
                Items = [LangKeys.Sample_de28d2720d.Tr(), LangKeys.Sample_3f086416bf.Tr(), LangKeys.Sample_ce3d5f558f.Tr()],
            },
            new ValuePickerColumn
            {
                Placeholder = LangKeys.Sample_fe7d74278a.Tr(),
                Items = ["128 GB", "256 GB", "512 GB", "1 TB"],
            },
            new ValuePickerColumn
            {
                Placeholder = LangKeys.Sample_6b36c6f7ec.Tr(),
                Items = [LangKeys.Sample_9d2d1f62ae.Tr(), LangKeys.Sample_2fc96b2704.Tr(), LangKeys.Sample_9c9aabab3f.Tr(), LangKeys.Sample_454b22f95d.Tr()],
                ShouldLoop = false,
            },
        ];
        ValuePickerDemo.SelectedValuesChanged += (_, _) =>
        {
            ValuePickerResultText.Text = ValuePickerDemo.SelectedValues is { Length: > 0 } values
                ? LangKeys.Format_Selected.Tr(string.Join(" / ", values))
                : LangKeys.Sample_b5c92782c9.Tr();
        };
    }
}
