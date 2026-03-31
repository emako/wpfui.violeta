using System;
using System.Windows.Controls;

namespace Wpf.Ui.Test.NavigationView.Pages;

public partial class ReportsDemoPage : Page
{
    public ReportsDemoPage(string tag)
    {
        InitializeComponent();

        var section = "overview";
        if (tag.StartsWith("reports/", StringComparison.OrdinalIgnoreCase))
        {
            section = tag["reports/".Length..];
        }

        SectionTextBlock.Text = $"当前报表分组: {section}";
    }
}
