using System.Windows;

namespace Wpf.Ui.Violeta.Controls;

public partial class CaptionCloseButton : CaptionButton
{
    static CaptionCloseButton()
        => DefaultStyleKeyProperty.OverrideMetadata(typeof(CaptionCloseButton), new FrameworkPropertyMetadata(typeof(CaptionCloseButton)));

    public CaptionCloseButton()
    {
        Kind = CaptionButtonKind.Close;
    }
}
