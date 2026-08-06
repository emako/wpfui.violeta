using Wpf.Ui.Violeta.Controls;
using LiteObservableLanguages;
using Wpf.Ui.Violeta.Gallery.Globalization;

namespace Wpf.Ui.Violeta.Gallery.Pages.Text;

public partial class SearchBoxPage : Wpf.Ui.Violeta.Controls.Page
{
    public SearchBoxPage()
    {
        InitializeComponent();
    }

    private void OnQueryChanged(SearchBox sender, SearchBoxQueryChangedEventArgs args)
    {
        QueryChangedResult.Text = string.IsNullOrEmpty(args.QueryText)
            ? LangKeys.Sample_24a24070fd.Tr()
            : $"QueryChanged：{args.QueryText}";
    }

    private void OnQuerySubmitted(SearchBox sender, SearchBoxQuerySubmittedEventArgs args)
    {
        QuerySubmittedResult.Text = string.IsNullOrEmpty(args.QueryText)
            ? LangKeys.Sample_efc15e78f4.Tr()
            : $"QuerySubmitted：{args.QueryText}";
    }
}
