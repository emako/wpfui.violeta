using Wpf.Ui.Violeta.Controls;

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
            ? "QueryChanged：（已清除）"
            : $"QueryChanged：{args.QueryText}";
    }

    private void OnQuerySubmitted(SearchBox sender, SearchBoxQuerySubmittedEventArgs args)
    {
        QuerySubmittedResult.Text = string.IsNullOrEmpty(args.QueryText)
            ? "QuerySubmitted：（空查询）"
            : $"QuerySubmitted：{args.QueryText}";
    }
}
