using System;
using System.Windows;
using Wpf.Ui.Violeta.Controls;

namespace Wpf.Ui.Violeta.Gallery.Pages.Feedback;

public partial class ExceptionReportPage : Wpf.Ui.Violeta.Controls.Page
{
    public ExceptionReportPage()
    {
        InitializeComponent();
    }

    private void ShowException_Click(object sender, RoutedEventArgs e)
    {
        ExceptionReport.Show(new InvalidOperationException("这是 Gallery 演示异常：用于预览 ExceptionReport 对话框。"));
    }
}
