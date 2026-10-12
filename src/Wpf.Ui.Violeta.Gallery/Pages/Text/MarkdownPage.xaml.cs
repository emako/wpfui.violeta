using System;
using System.IO;
using System.Windows;

namespace Wpf.Ui.Violeta.Gallery.Pages.Text;

public partial class MarkdownPage
{
    public MarkdownPage()
    {
        InitializeComponent();
        Viewer.Markdown = LoadSample();
    }

    private static string LoadSample()
    {
        var info = Application.GetResourceStream(new Uri("pack://application:,,,/Wpf.Ui.Violeta.Gallery;component/Resources/Strings/MarkdownSample.md"));
        using var reader = new StreamReader(info.Stream);
        return reader.ReadToEnd();
    }
}
