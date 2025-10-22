using System.Windows;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// 系统标题栏按钮，特指 Windows 窗口上的帮助、最小化、最大化/还原/关闭按钮，如果不是这些功能请不要使用 CaptionButton 而使用 TitleBarButton。
/// </summary>
public partial class CaptionButton : TitleBarButton
{
    static CaptionButton()
        => DefaultStyleKeyProperty.OverrideMetadata(typeof(CaptionButton), new FrameworkPropertyMetadata(typeof(CaptionButton)));

    /// <summary>
    /// 用于窗口消息处理时判断，此属性本身并不起作用。
    /// </summary>
    public static readonly DependencyProperty KindProperty =
        DependencyProperty.Register(
            nameof(Kind),
            typeof(CaptionButtonKind),
            typeof(CaptionButton),
            new PropertyMetadata(CaptionButtonKind.None)
        );

    public CaptionButtonKind Kind
    {
        get => (CaptionButtonKind)GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }

    public static readonly DependencyProperty IsMouseOverInTitleBarProperty =
        DependencyProperty.Register(
            nameof(IsMouseOverInTitleBar),
            typeof(bool),
            typeof(CaptionButton),
            new PropertyMetadata(false)
        );

    public bool IsMouseOverInTitleBar
    {
        get => (bool)GetValue(IsMouseOverInTitleBarProperty);
        set => SetValue(IsMouseOverInTitleBarProperty, value);
    }

    public static readonly DependencyProperty IsPressedInTitleBarProperty =
        DependencyProperty.Register(
            nameof(IsPressedInTitleBar),
            typeof(bool),
            typeof(CaptionButton),
            new PropertyMetadata(false)
        );

    public bool IsPressedInTitleBar
    {
        get => (bool)GetValue(IsPressedInTitleBarProperty);
        set => SetValue(IsPressedInTitleBarProperty, value);
    }
}
