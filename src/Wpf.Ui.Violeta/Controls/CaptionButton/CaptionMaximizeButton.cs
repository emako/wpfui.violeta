using System.Windows;

namespace Wpf.Ui.Violeta.Controls;

public partial class CaptionMaximizeButton : CaptionButton
{
    static CaptionMaximizeButton()
        => DefaultStyleKeyProperty.OverrideMetadata(typeof(CaptionMaximizeButton), new FrameworkPropertyMetadata(typeof(CaptionMaximizeButton)));

    public CaptionMaximizeButton()
    {
        Kind = CaptionButtonKind.Maximize;
    }

    public static readonly DependencyProperty IsRestoreButtonProperty =
    DependencyProperty.Register(
        nameof(IsRestoreButton),
        typeof(bool),
        typeof(CaptionMaximizeButton),
        new PropertyMetadata(false));

    public bool IsRestoreButton
    {
        get => (bool)GetValue(IsRestoreButtonProperty);
        set => SetValue(IsRestoreButtonProperty, value);
    }
}
