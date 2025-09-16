using System.Windows;
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Controls;

public sealed class ToastConfig
{
    public const int FastTime = 1500;
    public const int NormalTime = 2000;
    public const int SlowTime = 3000;

    public int Time { get; set; } = NormalTime;

    public ToastIcon ToastIcon { get; set; } = ToastIcon.None;

    public ToastLocation Location { get; set; } = ToastLocation.TopCenter;

    public FontStyle FontStyle { get; set; } = SystemFonts.MessageFontStyle;

    public FontStretch FontStretch { get; set; } = FontStretches.Normal;

    public double FontSize { get; set; } = SystemFonts.MessageFontSize;

    public FontWeight FontWeight { get; set; } = SystemFonts.MenuFontWeight;

    public double IconSize { get; set; } = 16d;

    public CornerRadius CornerRadius { get; set; } = new CornerRadius(3d);

    public Brush BorderBrush { get; set; } = (Brush)new BrushConverter().ConvertFromString("#1B1B1B")!;

    public Thickness BorderThickness { get; set; } = new Thickness(1d);

    public HorizontalAlignment HorizontalContentAlignment { get; set; } = HorizontalAlignment.Left;

    public VerticalAlignment VerticalContentAlignment { get; set; } = VerticalAlignment.Center;

    public Thickness OffsetMargin { get; set; } = new Thickness(15d);

    public static bool IsStacked { get; set; } = true;

    public static int MaxStacked { get; set; } = 5;

    public ToastConfig()
    {
    }

    public ToastConfig(ToastIcon icon, ToastLocation location, Thickness offsetMargin, int time) : this()
    {
        ToastIcon = icon;
        Location = location;
        if (offsetMargin != default)
        {
            OffsetMargin = offsetMargin;
        }
        Time = time;
    }
}
