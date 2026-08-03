using System.Windows.Media;

namespace Wpf.Ui.Violeta.Controls.ColorPickerHelpers;

/// <summary>
/// Mutable HSV components optimized for color-picker calculations (no alpha, no clamping).
/// </summary>
internal struct Hsv
{
    public double H;
    public double S;
    public double V;

    public Hsv(double h, double s, double v)
    {
        H = h;
        S = s;
        V = v;
    }

    public Hsv(HsvColor hsvColor)
    {
        H = hsvColor.H;
        S = hsvColor.S;
        V = hsvColor.V;
    }

    public readonly HsvColor ToHsvColor(double alpha = 1.0) => HsvColor.FromAhsv(alpha, H, S, V);

    public readonly Rgb ToRgb()
    {
        Color color = HsvColor.ToRgb(H, S, V);
        return new Rgb(color);
    }
}
