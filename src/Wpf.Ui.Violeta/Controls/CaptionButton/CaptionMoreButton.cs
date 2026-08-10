using System.Windows;

namespace Wpf.Ui.Violeta.Controls;

public partial class CaptionMoreButton : CaptionButton
{
    static CaptionMoreButton()
        => DefaultStyleKeyProperty.OverrideMetadata(typeof(CaptionMoreButton), new FrameworkPropertyMetadata(typeof(CaptionMoreButton)));

    public CaptionMoreButton()
    {
        Kind = CaptionButtonKind.More;
    }
}
