using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Wpf.Ui.Controls;

/// <summary>
/// A <see cref="Decorator"/> that paints a drop shadow beneath its child element
/// using 9-grid gradient brushes, matching the appearance of
/// <c>Microsoft.Windows.Themes.SystemDropShadowChrome</c>.
/// <para>
/// The total render size is the child's desired size plus <see cref="ShadowDepth"/> pixels
/// on the right and bottom edges.  The child is positioned at (0, 0); the shadow appears
/// offset by <c>ShadowDepth</c> in both X and Y directions and fades outward to transparent
/// over <c>ShadowDepth</c> pixels.
/// </para>
/// https://github.com/dotnet/wpf/blob/main/src/Microsoft.DotNet.Wpf/src/Themes/Shared/Microsoft/Windows/Themes/SystemDropShadowChrome.cs
/// </summary>
public sealed class DropShadowChrome : Decorator
{
    /// <summary>
    /// Width (in device-independent pixels) of the shadow blur region on each side.
    /// </summary>
    public const double ShadowDepth = 5.0;

    /// <summary>Backing dependency property for <see cref="Color"/>.</summary>
    public static readonly DependencyProperty ColorProperty =
        DependencyProperty.Register(
            nameof(Color),
            typeof(Color),
            typeof(DropShadowChrome),
            new FrameworkPropertyMetadata(
                Color.FromArgb(0x71, 0x00, 0x00, 0x00),
                FrameworkPropertyMetadataOptions.AffectsRender,
                ClearBrushes));

    /// <summary>
    /// Gets or sets the color used to fill the shadow region.
    /// The default is a semi-transparent black (<c>#71000000</c>), which
    /// matches the system default.
    /// </summary>
    public Color Color
    {
        get => (Color)GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    /// <summary>Backing dependency property for <see cref="CornerRadius"/>.</summary>
    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(DropShadowChrome),
            new FrameworkPropertyMetadata(
                new CornerRadius(),
                FrameworkPropertyMetadataOptions.AffectsRender,
                ClearBrushes),
            IsCornerRadiusValid);

    /// <summary>
    /// Gets or sets the corner radius of the element that casts the shadow.
    /// The shadow corners are rounded to match.  All values must be finite and non-negative.
    /// </summary>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    // A single set of brushes is shared across all instances that use the same
    // Color + CornerRadius, avoiding redundant allocations.
    private static Brush[]? s_commonBrushes;

    private static CornerRadius s_commonCornerRadius;
    private static readonly object s_brushLock = new();

    // Per-instance brush set used when Color/CornerRadius differs from the common cache.
    private Brush[]? _brushes;

    private static void ClearBrushes(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((DropShadowChrome)d)._brushes = null;

    private static bool IsCornerRadiusValid(object value)
    {
        var cr = (CornerRadius)value;
        return !(cr.TopLeft < 0 || cr.TopRight < 0 || cr.BottomLeft < 0 || cr.BottomRight < 0
              || double.IsNaN(cr.TopLeft) || double.IsNaN(cr.TopRight)
              || double.IsNaN(cr.BottomLeft) || double.IsNaN(cr.BottomRight)
              || double.IsInfinity(cr.TopLeft) || double.IsInfinity(cr.TopRight)
              || double.IsInfinity(cr.BottomLeft) || double.IsInfinity(cr.BottomRight));
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size constraint)
    {
        // Reserve ShadowDepth extra pixels on the right and bottom for the shadow.
        Size childConstraint = new(
            Math.Max(0, constraint.Width - ShadowDepth),
            Math.Max(0, constraint.Height - ShadowDepth));

        Child?.Measure(childConstraint);

        Size desired = Child?.DesiredSize ?? default;
        return new Size(desired.Width + ShadowDepth, desired.Height + ShadowDepth);
    }

    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size arrangeSize)
    {
        // Child occupies the top-left region; shadow spills right/bottom.
        Child?.Arrange(new Rect(
            0, 0,
            Math.Max(0, arrangeSize.Width - ShadowDepth),
            Math.Max(0, arrangeSize.Height - ShadowDepth)));
        return arrangeSize;
    }

    /// <inheritdoc/>
    protected override void OnRender(DrawingContext dc)
    {
        CornerRadius cornerRadius = CornerRadius;
        Color color = Color;

        // The shadow rect is the full render area shifted right/down by ShadowDepth.
        Rect shadowBounds = new(
            new Point(ShadowDepth, ShadowDepth),
            new Size(RenderSize.Width, RenderSize.Height));

        if (shadowBounds.Width <= 0 || shadowBounds.Height <= 0 || color.A == 0)
            return;

        // The "center" of the shadow (the fully opaque region) is the shadow bounds
        // deflated by ShadowDepth on every side.
        double centerWidth = shadowBounds.Right - shadowBounds.Left - 2 * ShadowDepth;
        double centerHeight = shadowBounds.Bottom - shadowBounds.Top - 2 * ShadowDepth;

        // Clamp corner radii so they never exceed half the inner shadow region.
        double maxRadius = Math.Min(centerWidth * 0.5, centerHeight * 0.5);
        cornerRadius.TopLeft = Math.Min(cornerRadius.TopLeft, maxRadius);
        cornerRadius.TopRight = Math.Min(cornerRadius.TopRight, maxRadius);
        cornerRadius.BottomLeft = Math.Min(cornerRadius.BottomLeft, maxRadius);
        cornerRadius.BottomRight = Math.Min(cornerRadius.BottomRight, maxRadius);

        Brush[] brushes = GetBrushes(color, cornerRadius);

        // Reference coordinates for the inner (opaque) shadow rectangle.
        double centerTop = shadowBounds.Top + ShadowDepth;
        double centerLeft = shadowBounds.Left + ShadowDepth;
        double centerRight = shadowBounds.Right - ShadowDepth;
        double centerBottom = shadowBounds.Bottom - ShadowDepth;

        // Six X snap lines (left edge of each distinct column segment).
        double[] gX =
        [
            centerLeft,
            centerLeft  + cornerRadius.TopLeft,
            centerRight - cornerRadius.TopRight,
            centerLeft  + cornerRadius.BottomLeft,
            centerRight - cornerRadius.BottomRight,
            centerRight,
        ];

        // Six Y snap lines (top edge of each distinct row segment).
        double[] gY =
        [
            centerTop,
            centerTop    + cornerRadius.TopLeft,
            centerTop    + cornerRadius.TopRight,
            centerBottom - cornerRadius.BottomLeft,
            centerBottom - cornerRadius.BottomRight,
            centerBottom,
        ];

        dc.PushGuidelineSet(new GuidelineSet(gX, gY));

        // Outer corner rect sizes include ShadowDepth to account for the blur fade.
        cornerRadius.TopLeft += ShadowDepth;
        cornerRadius.TopRight += ShadowDepth;
        cornerRadius.BottomLeft += ShadowDepth;
        cornerRadius.BottomRight += ShadowDepth;

        // -- Top row -----------------------------------------------------------
        dc.DrawRectangle(brushes[TopLeft], null,
            new Rect(shadowBounds.Left, shadowBounds.Top, cornerRadius.TopLeft, cornerRadius.TopLeft));

        double topEdgeWidth = gX[2] - gX[1];
        if (topEdgeWidth > 0)
            dc.DrawRectangle(brushes[Top], null,
                new Rect(gX[1], shadowBounds.Top, topEdgeWidth, ShadowDepth));

        dc.DrawRectangle(brushes[TopRight], null,
            new Rect(gX[2], shadowBounds.Top, cornerRadius.TopRight, cornerRadius.TopRight));

        // -- Middle row --------------------------------------------------------
        double leftEdgeHeight = gY[3] - gY[1];
        if (leftEdgeHeight > 0)
            dc.DrawRectangle(brushes[Left], null,
                new Rect(shadowBounds.Left, gY[1], ShadowDepth, leftEdgeHeight));

        double rightEdgeHeight = gY[4] - gY[2];
        if (rightEdgeHeight > 0)
            dc.DrawRectangle(brushes[Right], null,
                new Rect(gX[5], gY[2], ShadowDepth, rightEdgeHeight));

        // -- Bottom row --------------------------------------------------------
        dc.DrawRectangle(brushes[BottomLeft], null,
            new Rect(shadowBounds.Left, gY[3], cornerRadius.BottomLeft, cornerRadius.BottomLeft));

        double bottomEdgeWidth = gX[4] - gX[3];
        if (bottomEdgeWidth > 0)
            dc.DrawRectangle(brushes[Bottom], null,
                new Rect(gX[3], gY[5], bottomEdgeWidth, ShadowDepth));

        dc.DrawRectangle(brushes[BottomRight], null,
            new Rect(gX[4], gY[4], cornerRadius.BottomRight, cornerRadius.BottomRight));

        // -- Center fill -------------------------------------------------------
        // When all corners are zero (i.e. equal to ShadowDepth after the addition above),
        // a single rectangle covers the opaque center.  Otherwise a polygon path is needed
        // because the 9-grid segments leave non-rectangular gaps at mixed corner radii.
        if (cornerRadius.TopLeft == ShadowDepth
            && cornerRadius.TopLeft == cornerRadius.TopRight
            && cornerRadius.TopLeft == cornerRadius.BottomLeft
            && cornerRadius.TopLeft == cornerRadius.BottomRight)
        {
            dc.DrawRectangle(brushes[Center], null,
                new Rect(gX[0], gY[0], centerWidth, centerHeight));
        }
        else
        {
            // Build a counter-clockwise polygon that covers the opaque center area,
            // stepping around whichever corners have non-zero radii.
            PathFigure figure = new();

            if (cornerRadius.TopLeft > ShadowDepth)
            {
                figure.StartPoint = new Point(gX[1], gY[0]);
                figure.Segments.Add(new LineSegment(new Point(gX[1], gY[1]), isStroked: true));
                figure.Segments.Add(new LineSegment(new Point(gX[0], gY[1]), isStroked: true));
            }
            else
            {
                figure.StartPoint = new Point(gX[0], gY[0]);
            }

            if (cornerRadius.BottomLeft > ShadowDepth)
            {
                figure.Segments.Add(new LineSegment(new Point(gX[0], gY[3]), isStroked: true));
                figure.Segments.Add(new LineSegment(new Point(gX[3], gY[3]), isStroked: true));
                figure.Segments.Add(new LineSegment(new Point(gX[3], gY[5]), isStroked: true));
            }
            else
            {
                figure.Segments.Add(new LineSegment(new Point(gX[0], gY[5]), isStroked: true));
            }

            if (cornerRadius.BottomRight > ShadowDepth)
            {
                figure.Segments.Add(new LineSegment(new Point(gX[4], gY[5]), isStroked: true));
                figure.Segments.Add(new LineSegment(new Point(gX[4], gY[4]), isStroked: true));
                figure.Segments.Add(new LineSegment(new Point(gX[5], gY[4]), isStroked: true));
            }
            else
            {
                figure.Segments.Add(new LineSegment(new Point(gX[5], gY[5]), isStroked: true));
            }

            if (cornerRadius.TopRight > ShadowDepth)
            {
                figure.Segments.Add(new LineSegment(new Point(gX[5], gY[2]), isStroked: true));
                figure.Segments.Add(new LineSegment(new Point(gX[2], gY[2]), isStroked: true));
                figure.Segments.Add(new LineSegment(new Point(gX[2], gY[0]), isStroked: true));
            }
            else
            {
                figure.Segments.Add(new LineSegment(new Point(gX[5], gY[0]), isStroked: true));
            }

            figure.IsClosed = true;
            figure.Freeze();

            PathGeometry geometry = new();
            geometry.Figures.Add(figure);
            geometry.Freeze();

            dc.DrawGeometry(brushes[Center], null, geometry);
        }

        dc.Pop(); // GuidelineSet
    }

    // -- Brush helpers ---------------------------------------------------------

    // Brush-array index constants — 9-grid layout:
    //   0 TopLeft    1 Top      2 TopRight
    //   3 Left       4 Center   5 Right
    //   6 BottomLeft 7 Bottom   8 BottomRight
    private const int TopLeft = 0;

    private const int Top = 1;
    private const int TopRight = 2;
    private const int Left = 3;
    private const int Center = 4;
    private const int Right = 5;
    private const int BottomLeft = 6;
    private const int Bottom = 7;
    private const int BottomRight = 8;

    /// <summary>
    /// Returns a cached or newly created array of 9 brushes for the given
    /// <paramref name="color"/> and <paramref name="cornerRadius"/>.
    /// The first call caches a "common" set; subsequent calls with different
    /// parameters create a local set on the instance.
    /// </summary>
    private Brush[] GetBrushes(Color color, CornerRadius cornerRadius)
    {
        if (s_commonBrushes is null)
        {
            lock (s_brushLock)
            {
                if (s_commonBrushes is null)
                {
                    // Assume the first render represents the most-used style.
                    s_commonBrushes = CreateBrushes(color, cornerRadius);
                    s_commonCornerRadius = cornerRadius;
                }
            }
        }

        // Reuse the shared set when both color and corner radii match.
        if (color == ((SolidColorBrush)s_commonBrushes[Center]).Color
            && cornerRadius == s_commonCornerRadius)
        {
            _brushes = null; // release any instance-local set
            return s_commonBrushes;
        }

        // Otherwise, fall back to (or create) an instance-local set.
        return _brushes ??= CreateBrushes(color, cornerRadius);
    }

    /// <summary>
    /// Builds the 9-brush array for a given <paramref name="color"/> and
    /// <paramref name="cornerRadius"/>.
    /// </summary>
    private static Brush[] CreateBrushes(Color color, CornerRadius cornerRadius)
    {
        Brush[] brushes = new Brush[9];

        // Center: fully opaque solid fill.
        brushes[Center] = new SolidColorBrush(color);
        brushes[Center].Freeze();

        // Straight edges all share the same gradient stops (corner radius = 0).
        GradientStopCollection sideStops = CreateStops(color, 0);

        LinearGradientBrush top = new(sideStops, new Point(0, 1), new Point(0, 0));
        top.Freeze();
        brushes[Top] = top;

        LinearGradientBrush left = new(sideStops, new Point(1, 0), new Point(0, 0));
        left.Freeze();
        brushes[Left] = left;

        LinearGradientBrush right = new(sideStops, new Point(0, 0), new Point(1, 0));
        right.Freeze();
        brushes[Right] = right;

        LinearGradientBrush bottom = new(sideStops, new Point(0, 0), new Point(0, 1));
        bottom.Freeze();
        brushes[Bottom] = bottom;

        // Corners: radial gradients whose stops are scaled to account for
        // the corner radius' curvature + the ShadowDepth fade.

        GradientStopCollection topLeftStops = cornerRadius.TopLeft == 0
            ? sideStops
            : CreateStops(color, cornerRadius.TopLeft);

        RadialGradientBrush topLeft = new(topLeftStops)
        {
            RadiusX = 1,
            RadiusY = 1,
            Center = new Point(1, 1),
            GradientOrigin = new Point(1, 1),
        };
        topLeft.Freeze();
        brushes[TopLeft] = topLeft;

        GradientStopCollection topRightStops =
            cornerRadius.TopRight == 0 ? sideStops :
            cornerRadius.TopRight == cornerRadius.TopLeft ? topLeftStops :
                                                                  CreateStops(color, cornerRadius.TopRight);

        RadialGradientBrush topRight = new(topRightStops)
        {
            RadiusX = 1,
            RadiusY = 1,
            Center = new Point(0, 1),
            GradientOrigin = new Point(0, 1),
        };
        topRight.Freeze();
        brushes[TopRight] = topRight;

        GradientStopCollection bottomLeftStops =
            cornerRadius.BottomLeft == 0 ? sideStops :
            cornerRadius.BottomLeft == cornerRadius.TopLeft ? topLeftStops :
            cornerRadius.BottomLeft == cornerRadius.TopRight ? topRightStops :
                                                                  CreateStops(color, cornerRadius.BottomLeft);

        RadialGradientBrush bottomLeft = new(bottomLeftStops)
        {
            RadiusX = 1,
            RadiusY = 1,
            Center = new Point(1, 0),
            GradientOrigin = new Point(1, 0),
        };
        bottomLeft.Freeze();
        brushes[BottomLeft] = bottomLeft;

        GradientStopCollection bottomRightStops =
            cornerRadius.BottomRight == 0 ? sideStops :
            cornerRadius.BottomRight == cornerRadius.TopLeft ? topLeftStops :
            cornerRadius.BottomRight == cornerRadius.TopRight ? topRightStops :
            cornerRadius.BottomRight == cornerRadius.BottomLeft ? bottomLeftStops :
                                                                   CreateStops(color, cornerRadius.BottomRight);

        RadialGradientBrush bottomRight = new(bottomRightStops)
        {
            RadiusX = 1,
            RadiusY = 1,
            Center = new Point(0, 0),
            GradientOrigin = new Point(0, 0),
        };
        bottomRight.Freeze();
        brushes[BottomRight] = bottomRight;

        return brushes;
    }

    /// <summary>
    /// Creates the gradient stops for an edge or corner with the given
    /// <paramref name="cornerRadius"/>.
    /// Stop offsets are scaled so the gradient spans from the inner edge of
    /// the shadow center to its outer (transparent) boundary, matching the
    /// Win32 drop-shadow fall-off curve.
    /// </summary>
    private static GradientStopCollection CreateStops(Color color, double cornerRadius)
    {
        // Normalize so the gradient covers exactly [cornerRadius, cornerRadius + ShadowDepth].
        double scale = 1d / (cornerRadius + ShadowDepth);

        GradientStopCollection stops =
        [
            // Full opacity at the inner edge.
            new GradientStop(color, (0.5d + cornerRadius) * scale),
            // Win32 drop-shadow fall-off: ~74 %, ~38 %, ~12 %, ~3 %, 0 %.
            new GradientStop(ColorWithAlpha(color, 0.74336d),  (1.5d + cornerRadius) * scale),
            new GradientStop(ColorWithAlpha(color, 0.38053d),  (2.5d + cornerRadius) * scale),
            new GradientStop(ColorWithAlpha(color, 0.12389d),  (3.5d + cornerRadius) * scale),
            new GradientStop(ColorWithAlpha(color, 0.02654d),  (4.5d + cornerRadius) * scale),
            new GradientStop(ColorWithAlpha(color, 0d),        (5d + cornerRadius) * scale),
        ];
        stops.Freeze();
        return stops;
    }

    /// <summary>Returns a copy of <paramref name="color"/> with alpha scaled by <paramref name="alphaFactor"/>.</summary>
    private static Color ColorWithAlpha(Color color, double alphaFactor)
        => Color.FromArgb((byte)(alphaFactor * color.A), color.R, color.G, color.B);
}
