using System.Windows.Media;

namespace Wpf.Ui.Violeta.Controls.ColorPickerHelpers;

/// <summary>
/// Mutable RGB components in 0..1 range (no alpha, no clamping).
/// </summary>
internal struct Rgb
{
    public double R;
    public double G;
    public double B;

    public Rgb(double r, double g, double b)
    {
        R = r;
        G = g;
        B = b;
    }

    public Rgb(Color color)
    {
        R = color.R / 255.0;
        G = color.G / 255.0;
        B = color.B / 255.0;
    }

    public readonly Color ToColor(double alpha = 1.0)
    {
        return Color.FromArgb(
            (byte)Clamp(alpha * 255.0, 0x00, 0xFF),
            (byte)Clamp(R * 255.0, 0x00, 0xFF),
            (byte)Clamp(G * 255.0, 0x00, 0xFF),
            (byte)Clamp(B * 255.0, 0x00, 0xFF));
    }

    public readonly Hsv ToHsv()
    {
        HsvColor hsvColor = ColorExtensions.ToHsv(
            Clamp(R, 0.0, 1.0),
            Clamp(G, 0.0, 1.0),
            Clamp(B, 0.0, 1.0));
        return new Hsv(hsvColor);
    }

    private static double Clamp(double value, double min, double max) =>
        value < min ? min : value > max ? max : value;
}
