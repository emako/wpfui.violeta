using System.Windows;
using System.Windows.Controls.Primitives;

namespace Wpf.Ui.Violeta.Controls;

public partial class TitleBarButton : ButtonBase
{
    static TitleBarButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(TitleBarButton), new FrameworkPropertyMetadata(typeof(TitleBarButton)));
    }

    public static readonly DependencyProperty IsActiveProperty =
        DependencyProperty.Register(nameof(IsActive), typeof(bool), typeof(TitleBarButton), new PropertyMetadata(true));

    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }
}
