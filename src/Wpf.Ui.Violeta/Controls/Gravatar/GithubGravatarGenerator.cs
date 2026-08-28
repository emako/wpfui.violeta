using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// GitHub-style 5×5 mirrored identicon generator (adapted from HandyControl / identicon.js).
/// </summary>
public class GithubGravatarGenerator : IGravatarGenerator
{
    private const int RenderDataMaxLength = 15;

    public object GetGravatar(string id)
    {
        var hashcode = GetHashCode(id ?? string.Empty);
        var renderData = GetRenderData(hashcode);
        var renderBrush = GetRenderBrush(hashcode);
        var geometryGroup = new GeometryGroup();

        void AddRec(int i, int j, bool hidden = false)
        {
            var rec = new RectangleGeometry(new Rect(new Point(i, j), hidden ? new Size() : new Size(1, 1)));
            geometryGroup.Children.Add(rec);
        }

        var index = 0;
        for (var i = 0; i < 2; i++)
        {
            for (var j = 0; j < 5; j++, index++)
            {
                AddRec(i, j, renderData[index] == 0);
            }
        }

        for (var j = 0; j < 5; j++, index++)
        {
            AddRec(2, j, renderData[index] == 0);
        }

        index -= 10;
        for (var i = 3; i < 5; i++)
        {
            for (var j = 0; j < 5; j++, index++)
            {
                AddRec(i, j, renderData[index] == 0);
            }

            index -= 10;
        }

        var path = new Path
        {
            Data = geometryGroup,
            Fill = renderBrush,
            VerticalAlignment = VerticalAlignment.Top,
            Stretch = Stretch.Uniform,
        };

        RenderOptions.SetEdgeMode(path, EdgeMode.Aliased);

        return path;
    }

    [SuppressMessage("Performance", "CA1850:Prefer static 'HashData' method over 'ComputeHash'")]
    private static string GetHashCode(string id)
    {
        var bytes = System.Text.Encoding.ASCII.GetBytes(id);
        using var md5 = MD5.Create();
        var hash = md5.ComputeHash(bytes);
        var stringBuilder = new StringBuilder(hash.Length * 2);
        foreach (var item in hash)
        {
            stringBuilder.Append(item.ToString("X2", CultureInfo.InvariantCulture));
        }

        return stringBuilder.ToString();
    }

    private static int[] GetRenderData(string hashcode)
    {
        var arr = new int[RenderDataMaxLength];
        for (var i = 0; i < RenderDataMaxLength; i++)
        {
            var c = hashcode[i];
            var v = (int)c;
            arr[i] = v % 2;
        }

        return arr;
    }

    [SuppressMessage("Performance", "CA1846:Prefer 'AsSpan' over 'Substring'")]
    [SuppressMessage("Performance", "CA1859:Use concrete types when possible for improved performance")]
    [SuppressMessage("Style", "IDE0057:Use range operator")]
    private static Brush GetRenderBrush(string hashcode)
    {
        var v = (double)int.Parse(hashcode.Substring(hashcode.Length - 7), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        var scale = v / 0xfffffff;
        var color = Hsl2Rgb(scale);
        return new SolidColorBrush(color);
    }

    // Adapted from https://github.com/stewartlord/identicon.js
    private static Color Hsl2Rgb(double h, double s = 0.7, double b = 0.5)
    {
        h *= 6;
        var arr = new[]
        {
            b += s *= b < .5 ? b : 1 - b,
            b - h % 1 * s * 2,
            b -= s *= 2,
            b,
            b + h % 1 * s,
            b + s,
        };

        var hValue = (int)Math.Floor(h);

        return Color.FromRgb(
            (byte)(arr[hValue % 6] * 255),
            (byte)(arr[(hValue | 16) % 6] * 255),
            (byte)(arr[(hValue | 8) % 6] * 255));
    }
}
