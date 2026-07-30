using System;
using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Violeta.Controls;

namespace Wpf.Ui.Violeta.Gallery.Pages.Text;

public partial class AutoSuggestBoxPage : Wpf.Ui.Violeta.Controls.Page
{
    public AutoSuggestBoxPage()
    {
        InitializeComponent();

        SuggestBox.OriginalItemsSource = new[]
        {
            "Document",
            "Picture",
            "Music",
            "Video",
            "Downloads",
            "Desktop",
            "Library",
            "Recent",
        };
    }
}
