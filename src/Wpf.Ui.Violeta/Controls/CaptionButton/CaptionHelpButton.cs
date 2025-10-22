using System.Windows;

namespace Wpf.Ui.Violeta.Controls;

public partial class CaptionHelpButton : CaptionButton
{
    static CaptionHelpButton()
        => DefaultStyleKeyProperty.OverrideMetadata(typeof(CaptionHelpButton), new FrameworkPropertyMetadata(typeof(CaptionHelpButton)));

    public CaptionHelpButton()
    {
        Kind = CaptionButtonKind.Help;
    }
}
