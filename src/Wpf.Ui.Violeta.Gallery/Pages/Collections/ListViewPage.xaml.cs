using LiteObservableLanguages;
using Wpf.Ui.Violeta.Gallery.Globalization;

namespace Wpf.Ui.Violeta.Gallery.Pages.Collections;

public record SampleFileItem(string Name, string Type, string Status);

public partial class ListViewPage : Wpf.Ui.Violeta.Controls.Page
{
    public ListViewPage()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            SampleListView.ItemsSource = new[]
            {
                new SampleFileItem(LangKeys.Sample_6bbc20b3ec.Tr(), "DOCX", LangKeys.Sample_fad5222ca0.Tr()),
                new SampleFileItem(LangKeys.Sample_22554bfe34.Tr(), "FIG", LangKeys.Sample_fb852fc6cc.Tr()),
                new SampleFileItem(LangKeys.Sample_75aee2c1f7.Tr(), "MD", LangKeys.Sample_fad5222ca0.Tr()),
                new SampleFileItem(LangKeys.Sample_7617ac7261.Tr(), "PNG", LangKeys.Sample_fb852fc6cc.Tr()),
            };
        };
    }
}
