using System.Windows.Media;

namespace Wpf.Ui.Violeta.Controls;

public class SemiColorLightPalette : IColorPalette
{
    private static readonly Color[,] PaletteColors = new[,]
    {
        {
            //Red
            Color.FromArgb(255, 254, 242, 237),
            Color.FromArgb(255, 254, 221, 210),
            Color.FromArgb(255, 253, 183, 165),
            Color.FromArgb(255, 251, 144, 120),
            Color.FromArgb(255, 250, 102, 76),
            Color.FromArgb(255, 249, 57, 32),
            Color.FromArgb(255, 213, 37, 21),
            Color.FromArgb(255, 178, 20, 12),
            Color.FromArgb(255, 142, 8, 5),
            Color.FromArgb(255, 106, 1, 3),
        },
        {
            //Pink
            Color.FromArgb(255, 253, 236, 239),
            Color.FromArgb(255, 251, 207, 216),
            Color.FromArgb(255, 246, 160, 181),
            Color.FromArgb(255, 242, 115, 150),
            Color.FromArgb(255, 237, 72, 123),
            Color.FromArgb(255, 233, 30, 99),
            Color.FromArgb(255, 197, 19, 86),
            Color.FromArgb(255, 162, 11, 72),
            Color.FromArgb(255, 126, 5, 58),
            Color.FromArgb(255, 90, 1, 43),
        },
        {
            //Purple
            Color.FromArgb(255, 247, 233, 247),
            Color.FromArgb(255, 239, 202, 240),
            Color.FromArgb(255, 221, 155, 224),
            Color.FromArgb(255, 201, 111, 209),
            Color.FromArgb(255, 180, 73, 194),
            Color.FromArgb(255, 158, 40, 179),
            Color.FromArgb(255, 135, 30, 158),
            Color.FromArgb(255, 113, 22, 138),
            Color.FromArgb(255, 92, 15, 117),
            Color.FromArgb(255, 73, 10, 97),
        },
        {
            //Violet
            Color.FromArgb(255, 243, 237, 249),
            Color.FromArgb(255, 226, 209, 244),
            Color.FromArgb(255, 196, 167, 233),
            Color.FromArgb(255, 166, 127, 221),
            Color.FromArgb(255, 136, 91, 210),
            Color.FromArgb(255, 106, 58, 199),
            Color.FromArgb(255, 87, 47, 179),
            Color.FromArgb(255, 70, 37, 158),
            Color.FromArgb(255, 54, 28, 138),
            Color.FromArgb(255, 40, 20, 117),
        },
        {
            //Indigo
            Color.FromArgb(255, 236, 239, 248),
            Color.FromArgb(255, 209, 216, 240),
            Color.FromArgb(255, 167, 179, 225),
            Color.FromArgb(255, 128, 144, 211),
            Color.FromArgb(255, 94, 111, 196),
            Color.FromArgb(255, 63, 81, 181),
            Color.FromArgb(255, 51, 66, 161),
            Color.FromArgb(255, 40, 52, 140),
            Color.FromArgb(255, 31, 40, 120),
            Color.FromArgb(255, 23, 29, 99),
        },
        {
            //Blue
            Color.FromArgb(255, 234, 245, 255),
            Color.FromArgb(255, 203, 231, 254),
            Color.FromArgb(255, 152, 205, 253),
            Color.FromArgb(255, 101, 178, 252),
            Color.FromArgb(255, 50, 149, 251),
            Color.FromArgb(255, 0, 100, 250),
            Color.FromArgb(255, 0, 98, 214),
            Color.FromArgb(255, 0, 79, 179),
            Color.FromArgb(255, 0, 61, 143),
            Color.FromArgb(255, 0, 44, 107),
        },
        {
            //LightBlue
            Color.FromArgb(255, 233, 247, 253),
            Color.FromArgb(255, 201, 236, 252),
            Color.FromArgb(255, 149, 216, 248),
            Color.FromArgb(255, 98, 195, 245),
            Color.FromArgb(255, 48, 172, 241),
            Color.FromArgb(255, 0, 149, 238),
            Color.FromArgb(255, 0, 123, 202),
            Color.FromArgb(255, 0, 99, 167),
            Color.FromArgb(255, 0, 75, 131),
            Color.FromArgb(255, 0, 53, 95),
        },
        {
            //Cyan
            Color.FromArgb(255, 229, 247, 248),
            Color.FromArgb(255, 194, 239, 240),
            Color.FromArgb(255, 138, 221, 226),
            Color.FromArgb(255, 88, 203, 211),
            Color.FromArgb(255, 44, 184, 197),
            Color.FromArgb(255, 5, 164, 182),
            Color.FromArgb(255, 3, 134, 152),
            Color.FromArgb(255, 1, 105, 121),
            Color.FromArgb(255, 0, 77, 91),
            Color.FromArgb(255, 0, 50, 61),
        },
        {
            //Teal
            Color.FromArgb(255, 228, 247, 244),
            Color.FromArgb(255, 192, 240, 232),
            Color.FromArgb(255, 135, 224, 211),
            Color.FromArgb(255, 84, 209, 193),
            Color.FromArgb(255, 39, 194, 176),
            Color.FromArgb(255, 0, 179, 161),
            Color.FromArgb(255, 0, 149, 137),
            Color.FromArgb(255, 0, 119, 111),
            Color.FromArgb(255, 0, 89, 85),
            Color.FromArgb(255, 0, 60, 58),
        },
        {
            //Green
            Color.FromArgb(255, 236, 247, 236),
            Color.FromArgb(255, 208, 240, 209),
            Color.FromArgb(255, 164, 224, 167),
            Color.FromArgb(255, 125, 209, 130),
            Color.FromArgb(255, 90, 194, 98),
            Color.FromArgb(255, 59, 179, 70),
            Color.FromArgb(255, 48, 149, 59),
            Color.FromArgb(255, 37, 119, 47),
            Color.FromArgb(255, 27, 89, 36),
            Color.FromArgb(255, 17, 60, 24),
        },
        {
            //LightGreen
            Color.FromArgb(255, 243, 248, 236),
            Color.FromArgb(255, 227, 240, 208),
            Color.FromArgb(255, 200, 226, 165),
            Color.FromArgb(255, 173, 211, 126),
            Color.FromArgb(255, 147, 197, 91),
            Color.FromArgb(255, 123, 182, 60),
            Color.FromArgb(255, 100, 152, 48),
            Color.FromArgb(255, 78, 121, 38),
            Color.FromArgb(255, 57, 91, 27),
            Color.FromArgb(255, 37, 61, 18),
        },
        {
            //Lime
            Color.FromArgb(255, 242, 250, 230),
            Color.FromArgb(255, 227, 246, 197),
            Color.FromArgb(255, 203, 237, 142),
            Color.FromArgb(255, 183, 227, 91),
            Color.FromArgb(255, 167, 218, 44),
            Color.FromArgb(255, 155, 209, 0),
            Color.FromArgb(255, 126, 174, 0),
            Color.FromArgb(255, 99, 139, 0),
            Color.FromArgb(255, 72, 104, 0),
            Color.FromArgb(255, 47, 70, 0),
        },
        {
            //Yellow
            Color.FromArgb(255, 255, 253, 234),
            Color.FromArgb(255, 254, 251, 203),
            Color.FromArgb(255, 253, 243, 152),
            Color.FromArgb(255, 252, 232, 101),
            Color.FromArgb(255, 251, 218, 50),
            Color.FromArgb(255, 250, 200, 0),
            Color.FromArgb(255, 208, 170, 0),
            Color.FromArgb(255, 167, 139, 0),
            Color.FromArgb(255, 125, 106, 0),
            Color.FromArgb(255, 83, 72, 0),
        },
        {
            //Amber
            Color.FromArgb(255, 254, 251, 235),
            Color.FromArgb(255, 252, 245, 206),
            Color.FromArgb(255, 249, 232, 158),
            Color.FromArgb(255, 246, 216, 111),
            Color.FromArgb(255, 243, 198, 65),
            Color.FromArgb(255, 240, 177, 20),
            Color.FromArgb(255, 200, 138, 15),
            Color.FromArgb(255, 160, 102, 10),
            Color.FromArgb(255, 120, 70, 6),
            Color.FromArgb(255, 80, 43, 3),
        },
        {
            //Orange
            Color.FromArgb(255, 255, 248, 234),
            Color.FromArgb(255, 254, 238, 204),
            Color.FromArgb(255, 254, 217, 152),
            Color.FromArgb(255, 253, 193, 101),
            Color.FromArgb(255, 253, 166, 51),
            Color.FromArgb(255, 252, 136, 0),
            Color.FromArgb(255, 210, 103, 0),
            Color.FromArgb(255, 168, 74, 0),
            Color.FromArgb(255, 126, 49, 0),
            Color.FromArgb(255, 84, 29, 0),
        },
        {
            //Grey
            Color.FromArgb(255, 249, 249, 249),
            Color.FromArgb(255, 230, 232, 234),
            Color.FromArgb(255, 198, 202, 205),
            Color.FromArgb(255, 167, 171, 176),
            Color.FromArgb(255, 136, 141, 146),
            Color.FromArgb(255, 107, 112, 117),
            Color.FromArgb(255, 85, 91, 97),
            Color.FromArgb(255, 65, 70, 76),
            Color.FromArgb(255, 46, 50, 56),
            Color.FromArgb(255, 28, 31, 35),
        },
        {
            //AIPurple
            Color.FromArgb(255, 248, 237, 255),
            Color.FromArgb(255, 242, 218, 255),
            Color.FromArgb(255, 227, 181, 255),
            Color.FromArgb(255, 209, 145, 255),
            Color.FromArgb(255, 189, 108, 255),
            Color.FromArgb(255, 166, 71, 255),
            Color.FromArgb(255, 134, 54, 219),
            Color.FromArgb(255, 105, 40, 184),
            Color.FromArgb(255, 78, 28, 148),
            Color.FromArgb(255, 54, 18, 112),
        },
    };

    public Color GetColor(int colorIndex, int shadeIndex)
    {
        return PaletteColors[
            Clamp(colorIndex, 0, ColorCount - 1),
            Clamp(shadeIndex, 0, ShadeCount - 1)
        ];
    }

    public int ColorCount => PaletteColors.GetLength(0);

    public int ShadeCount => PaletteColors.GetLength(1);

    private static int Clamp(int value, int min, int max) =>
        value < min ? min : value > max ? max : value;
}
