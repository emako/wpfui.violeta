using System.Windows;
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Options for a single toast notification (appearance, placement, lifetime, stacking).
/// </summary>
/// <remarks>
/// Use per-instance <see cref="IsStacked"/> or the global default <see cref="Toast.IsStacked"/>.
/// Defaults: 2 s lifetime, top-center, no icon, 15 px edge inset, not stacked.
/// </remarks>
public sealed class ToastConfig()
{
    /// <summary>Short auto-close duration (1500 ms).</summary>
    public const int FastTime = 1500;

    /// <summary>Default auto-close duration (2000 ms).</summary>
    public const int NormalTime = 2000;

    /// <summary>Long auto-close duration (3000 ms).</summary>
    public const int SlowTime = 3000;

    /// <summary>Auto-close delay in milliseconds. Default: <see cref="NormalTime"/>.</summary>
    public int Time { get; set; } = NormalTime;

    /// <summary>Leading icon. Default: <see cref="ToastIcon.None"/> (hidden).</summary>
    public ToastIcon ToastIcon { get; set; } = ToastIcon.None;

    /// <summary>Window anchor. Default: <see cref="ToastLocation.TopCenter"/>.</summary>
    public ToastLocation Location { get; set; } = ToastLocation.TopCenter;

    /// <summary>Message font style. Default: system message font style.</summary>
    public FontStyle FontStyle { get; set; } = SystemFonts.MessageFontStyle;

    /// <summary>Message font stretch. Default: <see cref="FontStretches.Normal"/>.</summary>
    public FontStretch FontStretch { get; set; } = FontStretches.Normal;

    /// <summary>Message font size (DIP). Default: system message font size.</summary>
    public double FontSize { get; set; } = SystemFonts.MessageFontSize;

    /// <summary>Message font weight. Default: system menu font weight.</summary>
    public FontWeight FontWeight { get; set; } = SystemFonts.MenuFontWeight;

    /// <summary>Icon size (DIP). Default: 16.</summary>
    public double IconSize { get; set; } = 16d;

    /// <summary>Border corner radius. Default: 3 on all corners.</summary>
    public CornerRadius CornerRadius { get; set; } = new CornerRadius(3d);

    /// <summary>Border brush. Default: #1B1B1B.</summary>
    public Brush BorderBrush { get; set; } = (Brush)new BrushConverter().ConvertFromString("#1B1B1B")!;

    /// <summary>Border thickness. Default: 1 px on all sides.</summary>
    public Thickness BorderThickness { get; set; } = new Thickness(1d);

    /// <summary>Content horizontal alignment. Default: <see cref="HorizontalAlignment.Left"/>.</summary>
    public HorizontalAlignment HorizontalContentAlignment { get; set; } = HorizontalAlignment.Left;

    /// <summary>Content vertical alignment. Default: <see cref="VerticalAlignment.Center"/>.</summary>
    public VerticalAlignment VerticalContentAlignment { get; set; } = VerticalAlignment.Center;

    /// <summary>
    /// Inset from the owner window edge (DIP). Default: 15 on all sides.
    /// The active edge depends on <see cref="Location"/> (Top, Bottom, Left, or Right).
    /// </summary>
    public Thickness OffsetMargin { get; set; } = new Thickness(15d);

    /// <summary>
    /// Stack with other toasts at the same <see cref="Location"/>. Default: false.
    /// The parameterized constructor copies <see cref="Toast.IsStacked"/> unless overridden.
    /// </summary>
    public bool IsStacked { get; set; }

    /// <summary>
    /// Max visible stack depth per location. Default: 25; extra toasts overlap the last slot.
    /// </summary>
    public static int MaxStacked { get; set; } = 25;

    /// <summary>Creates a config with common show parameters.</summary>
    /// <param name="icon">Toast icon.</param>
    /// <param name="location">Window anchor.</param>
    /// <param name="offsetMargin">Edge inset; pass default to keep 15 px.</param>
    /// <param name="time">Auto-close duration in milliseconds.</param>
    public ToastConfig(ToastIcon icon, ToastLocation location, Thickness offsetMargin, int time) : this()
    {
        ToastIcon = icon;
        Location = location;
        if (offsetMargin != default)
        {
            OffsetMargin = offsetMargin;
        }
        Time = time;
        IsStacked = Toast.IsStacked;
    }
}
