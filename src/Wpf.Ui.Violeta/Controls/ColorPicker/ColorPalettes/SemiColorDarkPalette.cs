// Adapted from Semi.Avalonia.ColorPicker SemiColorDarkPalette.
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Controls;

public class SemiColorDarkPalette : IColorPalette
{
    private static readonly Color[,] PaletteColors = new[,]
    {
        {
            //Red
            Color.FromArgb(255, 108, 9, 11),
            Color.FromArgb(255, 144, 17, 16),
            Color.FromArgb(255, 180, 32, 25),
            Color.FromArgb(255, 215, 51, 36),
            Color.FromArgb(255, 251, 73, 50),
            Color.FromArgb(255, 252, 114, 90),
            Color.FromArgb(255, 253, 153, 131),
            Color.FromArgb(255, 253, 190, 172),
            Color.FromArgb(255, 254, 224, 213),
            Color.FromArgb(255, 255, 243, 239),
        },
        {
            //Pink
            Color.FromArgb(255, 92, 7, 48),
            Color.FromArgb(255, 128, 14, 65),
            Color.FromArgb(255, 164, 23, 81),
            Color.FromArgb(255, 199, 34, 97),
            Color.FromArgb(255, 235, 47, 113),
            Color.FromArgb(255, 239, 86, 134),
            Color.FromArgb(255, 243, 126, 159),
            Color.FromArgb(255, 247, 168, 188),
            Color.FromArgb(255, 251, 211, 220),
            Color.FromArgb(255, 253, 238, 241),
        },
        {
            //Purple
            Color.FromArgb(255, 74, 16, 97),
            Color.FromArgb(255, 94, 23, 118),
            Color.FromArgb(255, 115, 31, 138),
            Color.FromArgb(255, 137, 40, 159),
            Color.FromArgb(255, 160, 51, 179),
            Color.FromArgb(255, 181, 83, 194),
            Color.FromArgb(255, 202, 120, 209),
            Color.FromArgb(255, 221, 160, 225),
            Color.FromArgb(255, 239, 206, 240),
            Color.FromArgb(255, 247, 235, 247),
        },
        {
            //Violet
            Color.FromArgb(255, 64, 27, 119),
            Color.FromArgb(255, 76, 36, 140),
            Color.FromArgb(255, 88, 46, 160),
            Color.FromArgb(255, 100, 57, 181),
            Color.FromArgb(255, 114, 70, 201),
            Color.FromArgb(255, 136, 101, 212),
            Color.FromArgb(255, 162, 136, 223),
            Color.FromArgb(255, 190, 173, 233),
            Color.FromArgb(255, 221, 212, 244),
            Color.FromArgb(255, 241, 238, 250),
        },
        {
            //Indigo
            Color.FromArgb(255, 23, 30, 101),
            Color.FromArgb(255, 32, 41, 122),
            Color.FromArgb(255, 41, 54, 142),
            Color.FromArgb(255, 52, 68, 163),
            Color.FromArgb(255, 64, 83, 183),
            Color.FromArgb(255, 95, 113, 197),
            Color.FromArgb(255, 129, 145, 212),
            Color.FromArgb(255, 167, 180, 226),
            Color.FromArgb(255, 209, 216, 241),
            Color.FromArgb(255, 237, 239, 248),
        },
        {
            //Blue
            Color.FromArgb(255, 5, 49, 112),
            Color.FromArgb(255, 10, 70, 148),
            Color.FromArgb(255, 19, 92, 184),
            Color.FromArgb(255, 29, 117, 219),
            Color.FromArgb(255, 41, 144, 255),
            Color.FromArgb(255, 84, 169, 255),
            Color.FromArgb(255, 127, 193, 255),
            Color.FromArgb(255, 169, 215, 255),
            Color.FromArgb(255, 212, 236, 255),
            Color.FromArgb(255, 239, 248, 255),
        },
        {
            //LightBlue
            Color.FromArgb(255, 0, 55, 97),
            Color.FromArgb(255, 0, 77, 133),
            Color.FromArgb(255, 3, 102, 169),
            Color.FromArgb(255, 10, 129, 204),
            Color.FromArgb(255, 19, 159, 240),
            Color.FromArgb(255, 64, 180, 243),
            Color.FromArgb(255, 110, 200, 246),
            Color.FromArgb(255, 157, 220, 249),
            Color.FromArgb(255, 206, 238, 252),
            Color.FromArgb(255, 235, 248, 254),
        },
        {
            //Cyan
            Color.FromArgb(255, 4, 52, 61),
            Color.FromArgb(255, 7, 79, 92),
            Color.FromArgb(255, 10, 108, 123),
            Color.FromArgb(255, 14, 137, 153),
            Color.FromArgb(255, 19, 168, 184),
            Color.FromArgb(255, 56, 187, 198),
            Color.FromArgb(255, 98, 205, 212),
            Color.FromArgb(255, 145, 223, 227),
            Color.FromArgb(255, 198, 239, 241),
            Color.FromArgb(255, 231, 247, 248),
        },
        {
            //Teal
            Color.FromArgb(255, 2, 60, 57),
            Color.FromArgb(255, 4, 90, 85),
            Color.FromArgb(255, 7, 119, 111),
            Color.FromArgb(255, 10, 149, 136),
            Color.FromArgb(255, 14, 179, 161),
            Color.FromArgb(255, 51, 194, 176),
            Color.FromArgb(255, 94, 209, 193),
            Color.FromArgb(255, 142, 225, 211),
            Color.FromArgb(255, 196, 240, 232),
            Color.FromArgb(255, 230, 247, 244),
        },
        {
            //Green
            Color.FromArgb(255, 18, 60, 25),
            Color.FromArgb(255, 28, 90, 37),
            Color.FromArgb(255, 39, 119, 49),
            Color.FromArgb(255, 50, 149, 61),
            Color.FromArgb(255, 62, 179, 73),
            Color.FromArgb(255, 93, 194, 100),
            Color.FromArgb(255, 127, 209, 132),
            Color.FromArgb(255, 166, 225, 168),
            Color.FromArgb(255, 208, 240, 209),
            Color.FromArgb(255, 236, 247, 236),
        },
        {
            //LightGreen
            Color.FromArgb(255, 38, 61, 19),
            Color.FromArgb(255, 59, 92, 29),
            Color.FromArgb(255, 81, 123, 40),
            Color.FromArgb(255, 103, 153, 52),
            Color.FromArgb(255, 127, 184, 64),
            Color.FromArgb(255, 151, 198, 95),
            Color.FromArgb(255, 176, 212, 129),
            Color.FromArgb(255, 201, 227, 167),
            Color.FromArgb(255, 228, 241, 209),
            Color.FromArgb(255, 243, 248, 237),
        },
        {
            //Lime
            Color.FromArgb(255, 49, 70, 3),
            Color.FromArgb(255, 75, 105, 5),
            Color.FromArgb(255, 103, 141, 9),
            Color.FromArgb(255, 132, 176, 12),
            Color.FromArgb(255, 162, 211, 17),
            Color.FromArgb(255, 174, 220, 58),
            Color.FromArgb(255, 189, 229, 102),
            Color.FromArgb(255, 207, 237, 150),
            Color.FromArgb(255, 229, 246, 201),
            Color.FromArgb(255, 243, 251, 233),
        },
        {
            //Yellow
            Color.FromArgb(255, 84, 73, 3),
            Color.FromArgb(255, 126, 108, 6),
            Color.FromArgb(255, 168, 142, 10),
            Color.FromArgb(255, 210, 175, 15),
            Color.FromArgb(255, 252, 206, 20),
            Color.FromArgb(255, 253, 222, 67),
            Color.FromArgb(255, 253, 235, 113),
            Color.FromArgb(255, 254, 245, 160),
            Color.FromArgb(255, 254, 251, 208),
            Color.FromArgb(255, 255, 254, 236),
        },
        {
            //Amber
            Color.FromArgb(255, 81, 46, 9),
            Color.FromArgb(255, 121, 75, 15),
            Color.FromArgb(255, 161, 107, 22),
            Color.FromArgb(255, 202, 143, 30),
            Color.FromArgb(255, 242, 183, 38),
            Color.FromArgb(255, 245, 202, 80),
            Color.FromArgb(255, 247, 219, 122),
            Color.FromArgb(255, 250, 234, 166),
            Color.FromArgb(255, 252, 246, 210),
            Color.FromArgb(255, 254, 251, 237),
        },
        {
            //Orange
            Color.FromArgb(255, 85, 31, 3),
            Color.FromArgb(255, 128, 53, 6),
            Color.FromArgb(255, 170, 80, 10),
            Color.FromArgb(255, 213, 111, 15),
            Color.FromArgb(255, 255, 146, 20),
            Color.FromArgb(255, 255, 174, 67),
            Color.FromArgb(255, 255, 199, 114),
            Color.FromArgb(255, 255, 221, 161),
            Color.FromArgb(255, 255, 239, 208),
            Color.FromArgb(255, 255, 249, 237),
        },
        {
            //Grey
            Color.FromArgb(255, 28, 31, 35),
            Color.FromArgb(255, 46, 50, 56),
            Color.FromArgb(255, 65, 70, 76),
            Color.FromArgb(255, 85, 91, 97),
            Color.FromArgb(255, 107, 112, 117),
            Color.FromArgb(255, 136, 141, 146),
            Color.FromArgb(255, 167, 171, 176),
            Color.FromArgb(255, 198, 202, 205),
            Color.FromArgb(255, 230, 232, 234),
            Color.FromArgb(255, 249, 249, 249),
        },
        {
            //AIPurple
            Color.FromArgb(255, 58, 23, 112),
            Color.FromArgb(255, 83, 35, 148),
            Color.FromArgb(255, 111, 49, 184),
            Color.FromArgb(255, 141, 65, 219),
            Color.FromArgb(255, 167, 68, 255),
            Color.FromArgb(255, 195, 117, 255),
            Color.FromArgb(255, 213, 152, 255),
            Color.FromArgb(255, 229, 186, 255),
            Color.FromArgb(255, 243, 221, 255),
            Color.FromArgb(255, 251, 243, 255),
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
