using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Controls.Compat;

/// <summary>
/// This is similar to the CornerRadius class in WPF, but it allows different values in X and Y axis for each corner.
/// </summary>
public struct CornerRadiusEx : IEquatable<CornerRadiusEx>
{
    // According to WPF CornerRadius, these properties are mutable, so we can't use readonly fields.
    private double _topLeftX;

    public double TopLeftX
    {
        readonly get => _topLeftX;
        set => _topLeftX = value;
    }

    private double _topLeftY;

    public double TopLeftY
    {
        readonly get => _topLeftY;
        set => _topLeftY = value;
    }

    private double _topRightX;

    public double TopRightX
    {
        readonly get => _topRightX;
        set => _topRightX = value;
    }

    private double _topRightY;

    public double TopRightY
    {
        readonly get => _topRightY;
        set => _topRightY = value;
    }

    private double _bottomLeftX;

    public double BottomLeftX
    {
        readonly get => _bottomLeftX;
        set => _bottomLeftX = value;
    }

    private double _bottomLeftY;

    public double BottomLeftY
    {
        readonly get => _bottomLeftY;
        set => _bottomLeftY = value;
    }

    private double _bottomRightX;

    public double BottomRightX
    {
        readonly get => _bottomRightX;
        set => _bottomRightX = value;
    }

    private double _bottomRightY;

    public double BottomRightY
    {
        readonly get => _bottomRightY;
        set => _bottomRightY = value;
    }

    [SuppressMessage("Style", "IDE0290:Use primary constructor")]
    public CornerRadiusEx(double topLeftX, double topLeftY, double topRightX, double topRightY, double bottomLeftX, double bottomLeftY, double bottomRightX, double bottomRightY)
    {
        _topLeftX = topLeftX;
        _topLeftY = topLeftY;
        _topRightX = topRightX;
        _topRightY = topRightY;
        _bottomLeftX = bottomLeftX;
        _bottomLeftY = bottomLeftY;
        _bottomRightX = bottomRightX;
        _bottomRightY = bottomRightY;
    }

    public CornerRadiusEx(CornerRadius cornerRadius) : this(cornerRadius.TopLeft, cornerRadius.TopLeft, cornerRadius.TopRight, cornerRadius.TopRight, cornerRadius.BottomLeft, cornerRadius.BottomLeft, cornerRadius.BottomRight, cornerRadius.BottomRight)
    {
    }

    public CornerRadiusEx(double uniformRadius) : this(uniformRadius, uniformRadius, uniformRadius, uniformRadius, uniformRadius, uniformRadius, uniformRadius, uniformRadius)
    {
    }

    public static implicit operator CornerRadiusEx(CornerRadius cornerRadius)
    {
        return new CornerRadiusEx(cornerRadius);
    }

    public override readonly string ToString()
    {
        return $"{TopLeftX} {TopLeftY}, {TopRightX} {TopRightY}, {BottomLeftX} {BottomLeftY},{BottomRightX} {BottomRightY}";
    }

    /// <summary>
    /// Converts this CornerRadiusEx to regular WPF CornerRadius. When the values are different in X and Y axis, an average is used.
    /// </summary>
    public readonly CornerRadius ToCornerRadius()
    {
        return new CornerRadius
        (
            (TopLeftX + TopLeftY) / 2,
            (TopRightX + TopRightY) / 2,
            (BottomRightX + BottomRightY) / 2,
            (BottomLeftX + BottomLeftY) / 2
        );
    }

    /// <summary>
    /// Draws a rounded rectangle geometry with the specified rect and corner radius.
    /// </summary>
    public static StreamGeometry CreateRoundedRectangleGeometry(Rect rect, CornerRadiusEx radius)
    {
        var geometry = new StreamGeometry();

        using (var context = geometry.Open())
        {
            // The AI did really a great job! Prompt as follows:
            // c# wpf，我有一个 rect 和 CornerRadiusEx（这个CornerRadiusEx包含 TopLeftX, TopLeftY...每个角都可以自定义 X 和 Y），
            // 现在请你编写一个方法，生成一个 StreamGeometry，其内容就是指定的 rect 加上这个圆角

            context.BeginFigure(new Point(rect.Left + radius.TopLeftX, rect.Top), true, true);

            // Top edge
            context.LineTo(new Point(rect.Right - radius.TopRightX, rect.Top), true, false);
            context.ArcTo(new Point(rect.Right, rect.Top + radius.TopRightY),
                          new Size(radius.TopRightX, radius.TopRightY),
                          0, false, SweepDirection.Clockwise, true, false);

            // Right edge
            context.LineTo(new Point(rect.Right, rect.Bottom - radius.BottomRightY), true, false);
            context.ArcTo(new Point(rect.Right - radius.BottomRightX, rect.Bottom),
                          new Size(radius.BottomRightX, radius.BottomRightY),
                          0, false, SweepDirection.Clockwise, true, false);

            // Bottom edge
            context.LineTo(new Point(rect.Left + radius.BottomLeftX, rect.Bottom), true, false);
            context.ArcTo(new Point(rect.Left, rect.Bottom - radius.BottomLeftY),
                          new Size(radius.BottomLeftX, radius.BottomLeftY),
                          0, false, SweepDirection.Clockwise, true, false);

            // Left edge
            context.LineTo(new Point(rect.Left, rect.Top + radius.TopLeftY), true, false);
            context.ArcTo(new Point(rect.Left + radius.TopLeftX, rect.Top),
                          new Size(radius.TopLeftX, radius.TopLeftY),
                          0, false, SweepDirection.Clockwise, true, false);
        }

        geometry.Freeze();
        return geometry;
    }

    public override readonly bool Equals(object? obj)
    {
        if (obj is CornerRadiusEx other)
        {
            return Equals(other);
        }
        else
        {
            return false;
        }
    }

    public readonly bool Equals(CornerRadiusEx other)
    {
        return TopLeftX == other.TopLeftX && TopLeftY == other.TopLeftY &&
               TopRightX == other.TopRightX && TopRightY == other.TopRightY &&
               BottomLeftX == other.BottomLeftX && BottomLeftY == other.BottomLeftY &&
               BottomRightX == other.BottomRightX && BottomRightY == other.BottomRightY;
    }

    public static bool operator ==(CornerRadiusEx left, CornerRadiusEx right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(CornerRadiusEx left, CornerRadiusEx right)
    {
        return !left.Equals(right);
    }

    public override readonly int GetHashCode()
    {
        return _topLeftX.GetHashCode() ^ _topLeftY.GetHashCode() ^ _topRightX.GetHashCode() ^ _topRightY.GetHashCode()
            ^ _bottomLeftX.GetHashCode() ^ _bottomLeftY.GetHashCode() ^ _bottomRightX.GetHashCode() ^ _bottomRightY.GetHashCode();
    }

    /// <summary>
    /// Scales every asix of every corner by the specified scale.
    /// </summary>
    /// <param name="scale"></param>
    public void Scale(double scale)
    {
        TopLeftX *= scale;
        TopLeftY *= scale;
        TopRightX *= scale;
        TopRightY *= scale;
        BottomLeftX *= scale;
        BottomLeftY *= scale;
        BottomRightX *= scale;
        BottomRightY *= scale;
    }
}
