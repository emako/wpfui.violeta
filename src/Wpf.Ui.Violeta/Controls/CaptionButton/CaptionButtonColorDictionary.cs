using System;
using System.Windows;

namespace Wpf.Ui.Violeta.Controls;

public class CaptionButtonColorDictionary : ResourceDictionary
{
    public CaptionButtonColorDictionary()
    {
        if (Environment.OSVersion.Version.Major >= 10 && Environment.OSVersion.Version.Build >= 22000)
        {
            Source = new Uri("/Wpf.Ui.Violeta;component/Controls/CaptionButton/CaptionButtonColors.Windows11.xaml", UriKind.Relative);
        }
        else
        {
            Source = new Uri("/Wpf.Ui.Violeta;component/Controls/CaptionButton/CaptionButtonColors.Windows10.xaml", UriKind.Relative);
        }
    }
}
