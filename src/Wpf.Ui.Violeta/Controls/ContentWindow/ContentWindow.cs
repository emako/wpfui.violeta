using System.Windows;

namespace Wpf.Ui.Violeta.Controls;

public class ContentWindow : ShellWindow
{
    static ContentWindow()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ContentWindow), new FrameworkPropertyMetadata(typeof(ContentWindow)));
    }

    public ContentWindow()
    {
    }
}
