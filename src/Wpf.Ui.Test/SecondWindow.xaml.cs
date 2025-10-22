using System;
using System.Diagnostics;
using System.Windows;
using Wpf.Ui.Violeta.Controls;

namespace Wpf.Ui.Test;

public partial class SecondWindow : ShellWindow
{
    public SecondWindow()
    {
        InitializeComponent();
    }

    private void PaneToggleButtonClick(object sender, EventArgs e)
    {
    }

    private void HelpButtonClick(object sender, EventArgs e)
    {
        using Process process = new()
        {
            StartInfo = new ProcessStartInfo
            {
                UseShellExecute = true,
                FileName = @"https://github.com/SuGar0218/NativeLikeCaptionButton-WPF"
            }
        };
        process.Start();
    }

    private void FullScreenButtonClick(object sender, RoutedEventArgs e)
    {
        //_fullScreenHandler.ToggleFullScreen();
    }
}
