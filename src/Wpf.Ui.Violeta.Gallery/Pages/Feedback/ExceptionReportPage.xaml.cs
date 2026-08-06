using System;
using System.Windows;
using Wpf.Ui.Violeta.Controls;
using LiteObservableLanguages;
using Wpf.Ui.Violeta.Gallery.Globalization;

namespace Wpf.Ui.Violeta.Gallery.Pages.Feedback;

public partial class ExceptionReportPage : Wpf.Ui.Violeta.Controls.Page
{
    public ExceptionReportPage()
    {
        InitializeComponent();
    }

    private void ShowException_Click(object sender, RoutedEventArgs e)
    {
        ExceptionReport.Show(new InvalidOperationException(LangKeys.Sample_280b260594.Tr()));
    }
}
