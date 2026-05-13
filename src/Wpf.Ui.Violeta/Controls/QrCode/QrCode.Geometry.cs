using Gma.QrCodeNet.Encoding;
using System.Windows;
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Controls;

public partial class QrCode
{
    private void ProcessSymbol(
        PathGeometry geometry,
        BitMatrix bitMatrix,
        int row,
        int column,
        Size symbolSize,
        double cornerRatio,
        Thickness padding)
    {
        var symbolBounds = new Rect(
            (column + QuietZoneCount) * symbolSize.Width + padding.Left,
            (row + QuietZoneCount) * symbolSize.Height + padding.Top,
            symbolSize.Width,
            symbolSize.Height
        );

        if (IsValidBit(bitMatrix, column, row))
        {
            ProcessSymbolIfSet(geometry, bitMatrix, row, column, symbolBounds, cornerRatio);
        }
        else
        {
            ProcessSymbolIfUnset(geometry, bitMatrix, row, column, symbolBounds, cornerRatio);
        }
    }

    private void AddPositionDetectionPattern(PathGeometry geometry, Rect bounds, Size symbolSize, double cornerRatio)
    {
        var padding = Padding;
        var dataBounds = DeflateRect(DeflateRect(bounds, padding),
            new Thickness(
                symbolSize.Width * QuietZoneCount,
                symbolSize.Height * QuietZoneCount,
                symbolSize.Width * QuietZoneCount,
                symbolSize.Height * QuietZoneCount));

        var twiceSymbolSize = new Size(symbolSize.Width * 2, symbolSize.Height * 2);

        for (var i = 0; i < 3; i++)
        {
            var markerSize = new Size(symbolSize.Width * 7, symbolSize.Height * 7);
            var markerLeftTopPosition = new Point(
                i == 1 ? dataBounds.Right - markerSize.Width : dataBounds.Left,
                i == 2 ? dataBounds.Bottom - markerSize.Height : dataBounds.Top
            );
            var arcSize = new Size(markerSize.Width * SymbolCornerRatio, markerSize.Height * SymbolCornerRatio);

            for (var x = 0; x < 3; x++)
            {
                var markerBounds = new Rect(markerLeftTopPosition, markerSize);

                var startPoint = new Point(
                    markerLeftTopPosition.X,
                    markerLeftTopPosition.Y + arcSize.Height
                );

                var figure = new PathFigure
                {
                    StartPoint = startPoint,
                    IsClosed = true,
                    IsFilled = true,
                };
                figure.Segments.Add(new ArcSegment(new Point(markerBounds.Left + arcSize.Width, markerBounds.Top), arcSize, 0, false, SweepDirection.Clockwise, false));
                figure.Segments.Add(new LineSegment(new Point(markerBounds.Right - arcSize.Width, markerBounds.Top), false));
                figure.Segments.Add(new ArcSegment(new Point(markerBounds.Right, markerBounds.Top + arcSize.Height), arcSize, 0, false, SweepDirection.Clockwise, false));
                figure.Segments.Add(new LineSegment(new Point(markerBounds.Right, markerBounds.Bottom - arcSize.Height), false));
                figure.Segments.Add(new ArcSegment(new Point(markerBounds.Right - arcSize.Width, markerBounds.Bottom), arcSize, 0, false, SweepDirection.Clockwise, false));
                figure.Segments.Add(new LineSegment(new Point(markerBounds.Left + arcSize.Width, markerBounds.Bottom), false));
                figure.Segments.Add(new ArcSegment(new Point(markerBounds.Left, markerBounds.Bottom - arcSize.Height), arcSize, 0, false, SweepDirection.Clockwise, false));
                figure.Segments.Add(new LineSegment(new Point(markerBounds.Left, markerBounds.Top + arcSize.Height), false));
                geometry.Figures.Add(figure);

                markerLeftTopPosition = new Point(
                    markerLeftTopPosition.X + symbolSize.Width,
                    markerLeftTopPosition.Y + symbolSize.Height);
                markerSize = new Size(markerSize.Width - twiceSymbolSize.Width, markerSize.Height - twiceSymbolSize.Height);
                arcSize = new Size(markerSize.Width * SymbolCornerRatio, markerSize.Height * SymbolCornerRatio);
            }
        }
    }

    private static void ProcessSymbolIfSet(
        PathGeometry geometry,
        BitMatrix bitMatrix,
        int row,
        int column,
        Rect symbolBounds,
        double cornerRatio)
    {
        if (cornerRatio == 0)
        {
            var simpleFigure = new PathFigure { StartPoint = symbolBounds.TopLeft, IsClosed = true, IsFilled = true };
            simpleFigure.Segments.Add(new LineSegment(symbolBounds.TopRight, false));
            simpleFigure.Segments.Add(new LineSegment(symbolBounds.BottomRight, false));
            simpleFigure.Segments.Add(new LineSegment(symbolBounds.BottomLeft, false));
            geometry.Figures.Add(simpleFigure);
            return;
        }

        var cornerRadius = new Size(symbolBounds.Width * cornerRatio, symbolBounds.Height * cornerRatio);
        var cornerFlags = GetSetSymbolCornerFlags(bitMatrix, row, column);

        var figure = new PathFigure
        {
            StartPoint = new Point(symbolBounds.Left, symbolBounds.Top + cornerRadius.Height),
            IsClosed = true,
            IsFilled = true,
        };

        // Top Left
        if ((cornerFlags & CornerFlags.TopLeft) != 0)
        {
            figure.Segments.Add(new LineSegment(symbolBounds.TopLeft, false));
            figure.Segments.Add(new LineSegment(new Point(symbolBounds.Right - cornerRadius.Width, symbolBounds.Top), false));
        }
        else
        {
            figure.Segments.Add(new ArcSegment(new Point(symbolBounds.Left + cornerRadius.Width, symbolBounds.Top), cornerRadius, 0, false, SweepDirection.Clockwise, false));
            figure.Segments.Add(new LineSegment(new Point(symbolBounds.Right - cornerRadius.Width, symbolBounds.Top), false));
        }

        // Top Right
        if ((cornerFlags & CornerFlags.TopRight) != 0)
        {
            figure.Segments.Add(new LineSegment(symbolBounds.TopRight, false));
            figure.Segments.Add(new LineSegment(new Point(symbolBounds.Right, symbolBounds.Bottom - cornerRadius.Height), false));
        }
        else
        {
            figure.Segments.Add(new ArcSegment(new Point(symbolBounds.Right, symbolBounds.Top + cornerRadius.Height), cornerRadius, 0, false, SweepDirection.Clockwise, false));
            figure.Segments.Add(new LineSegment(new Point(symbolBounds.Right, symbolBounds.Bottom - cornerRadius.Height), false));
        }

        // Bottom Right
        if ((cornerFlags & CornerFlags.BottomRight) != 0)
        {
            figure.Segments.Add(new LineSegment(symbolBounds.BottomRight, false));
            figure.Segments.Add(new LineSegment(new Point(symbolBounds.Left + cornerRadius.Width, symbolBounds.Bottom), false));
        }
        else
        {
            figure.Segments.Add(new ArcSegment(new Point(symbolBounds.Right - cornerRadius.Width, symbolBounds.Bottom), cornerRadius, 0, false, SweepDirection.Clockwise, false));
            figure.Segments.Add(new LineSegment(new Point(symbolBounds.Left + cornerRadius.Width, symbolBounds.Bottom), false));
        }

        // Bottom Left
        if ((cornerFlags & CornerFlags.BottomLeft) != 0)
        {
            figure.Segments.Add(new LineSegment(symbolBounds.BottomLeft, false));
            figure.Segments.Add(new LineSegment(figure.StartPoint, false));
        }
        else
        {
            figure.Segments.Add(new ArcSegment(new Point(symbolBounds.Left, symbolBounds.Bottom - cornerRadius.Height), cornerRadius, 0, false, SweepDirection.Clockwise, false));
            figure.Segments.Add(new LineSegment(figure.StartPoint, false));
        }

        geometry.Figures.Add(figure);
    }

    private static CornerFlags GetSetSymbolCornerFlags(BitMatrix bitMatrix, int row, int column)
    {
        var flags = CornerFlags.None;

        if (!IsValidBit(bitMatrix, column, row))
            return flags;

        if (IsValidBit(bitMatrix, column, row - 1) || IsValidBit(bitMatrix, column - 1, row))
            flags |= CornerFlags.TopLeft;
        if (IsValidBit(bitMatrix, column, row - 1) || IsValidBit(bitMatrix, column + 1, row))
            flags |= CornerFlags.TopRight;
        if (IsValidBit(bitMatrix, column, row + 1) || IsValidBit(bitMatrix, column + 1, row))
            flags |= CornerFlags.BottomRight;
        if (IsValidBit(bitMatrix, column, row + 1) || IsValidBit(bitMatrix, column - 1, row))
            flags |= CornerFlags.BottomLeft;

        return flags;
    }

    private static void ProcessSymbolIfUnset(
        PathGeometry geometry,
        BitMatrix bitMatrix,
        int row,
        int column,
        Rect symbolBounds,
        double cornerRatio)
    {
        if (IsValidBit(bitMatrix, column, row))
            return;
        if (cornerRatio == 0)
            return;

        var cornerFlags = GetUnsetSymbolCornerFlags(bitMatrix, row, column);
        if (cornerFlags == CornerFlags.None)
            return;

        var cornerRadius = new Size(symbolBounds.Width * cornerRatio, symbolBounds.Height * cornerRatio);

        // Top Left
        if ((cornerFlags & CornerFlags.TopLeft) != 0)
        {
            var start = new Point(symbolBounds.Left, symbolBounds.Top + cornerRadius.Height);
            var fig = new PathFigure { StartPoint = start, IsClosed = true, IsFilled = true };
            fig.Segments.Add(new LineSegment(symbolBounds.TopLeft, false));
            fig.Segments.Add(new LineSegment(new Point(symbolBounds.Left + cornerRadius.Width, symbolBounds.Top), false));
            fig.Segments.Add(new ArcSegment(start, cornerRadius, 0, false, SweepDirection.Counterclockwise, false));
            geometry.Figures.Add(fig);
        }

        // Top Right
        if ((cornerFlags & CornerFlags.TopRight) != 0)
        {
            var start = new Point(symbolBounds.Right - cornerRadius.Width, symbolBounds.Top);
            var fig = new PathFigure { StartPoint = start, IsClosed = true, IsFilled = true };
            fig.Segments.Add(new LineSegment(symbolBounds.TopRight, false));
            fig.Segments.Add(new LineSegment(new Point(symbolBounds.Right, symbolBounds.Top + cornerRadius.Height), false));
            fig.Segments.Add(new ArcSegment(start, cornerRadius, 0, false, SweepDirection.Counterclockwise, false));
            geometry.Figures.Add(fig);
        }

        // Bottom Right
        if ((cornerFlags & CornerFlags.BottomRight) != 0)
        {
            var start = new Point(symbolBounds.Right, symbolBounds.Bottom - cornerRadius.Height);
            var fig = new PathFigure { StartPoint = start, IsClosed = true, IsFilled = true };
            fig.Segments.Add(new LineSegment(symbolBounds.BottomRight, false));
            fig.Segments.Add(new LineSegment(new Point(symbolBounds.Right - cornerRadius.Width, symbolBounds.Bottom), false));
            fig.Segments.Add(new ArcSegment(start, cornerRadius, 0, false, SweepDirection.Counterclockwise, false));
            geometry.Figures.Add(fig);
        }

        // Bottom Left
        if ((cornerFlags & CornerFlags.BottomLeft) != 0)
        {
            var start = new Point(symbolBounds.Left + cornerRadius.Width, symbolBounds.Bottom);
            var fig = new PathFigure { StartPoint = start, IsClosed = true, IsFilled = true };
            fig.Segments.Add(new LineSegment(symbolBounds.BottomLeft, false));
            fig.Segments.Add(new LineSegment(new Point(symbolBounds.Left, symbolBounds.Bottom - cornerRadius.Height), false));
            fig.Segments.Add(new ArcSegment(start, cornerRadius, 0, false, SweepDirection.Counterclockwise, false));
            geometry.Figures.Add(fig);
        }
    }

    private static CornerFlags GetUnsetSymbolCornerFlags(BitMatrix bitMatrix, int row, int column)
    {
        var flags = CornerFlags.None;

        if (IsValidBit(bitMatrix, column, row))
            return flags;

        if (IsValidBit(bitMatrix, column, row - 1) && IsValidBit(bitMatrix, column - 1, row - 1) && IsValidBit(bitMatrix, column - 1, row))
            flags |= CornerFlags.TopLeft;
        if (IsValidBit(bitMatrix, column, row - 1) && IsValidBit(bitMatrix, column + 1, row - 1) && IsValidBit(bitMatrix, column + 1, row))
            flags |= CornerFlags.TopRight;
        if (IsValidBit(bitMatrix, column, row + 1) && IsValidBit(bitMatrix, column + 1, row + 1) && IsValidBit(bitMatrix, column + 1, row))
            flags |= CornerFlags.BottomRight;
        if (IsValidBit(bitMatrix, column, row + 1) && IsValidBit(bitMatrix, column - 1, row + 1) && IsValidBit(bitMatrix, column - 1, row))
            flags |= CornerFlags.BottomLeft;

        return flags;
    }

    private static bool IsValidBit(BitMatrix bitMatrix, int x, int y)
    {
        if (x < 0 || y < 0 || x >= bitMatrix.Width || y >= bitMatrix.Height)
            return false;
        if (x < 8 && y < 8) return false;
        if (x > bitMatrix.Width - 9 && y < 8) return false;
        if (x < 8 && y > bitMatrix.Height - 9) return false;
        return bitMatrix[y, x];
    }

    private static Rect DeflateRect(Rect rect, Thickness thickness)
    {
        return new Rect(
            rect.Left + thickness.Left,
            rect.Top + thickness.Top,
            Math.Max(0, rect.Width - thickness.Left - thickness.Right),
            Math.Max(0, rect.Height - thickness.Top - thickness.Bottom));
    }
}
