using System.Windows;

namespace Wpf.Ui.Violeta.Controls;

public partial class CaptionMinimizeButton : CaptionButton
{
    static CaptionMinimizeButton()
        => DefaultStyleKeyProperty.OverrideMetadata(typeof(CaptionMinimizeButton), new FrameworkPropertyMetadata(typeof(CaptionMinimizeButton)));

    public CaptionMinimizeButton()
    {
        Kind = CaptionButtonKind.Minimize;
    }
}
