using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Wpf.Ui.Controls;

/// <summary>
/// Automatically clips content to the inner rounded edge of the parent border.
/// </summary>
public class BorderContentAdapter : Decorator
{
    static BorderContentAdapter()
    {
        ClipToBoundsProperty.OverrideMetadata(typeof(BorderContentAdapter), new PropertyMetadata(true));
    }

    private static Geometry? CalculateBorderContentClip(System.Windows.Controls.Border border, Size contentSize)
    {
        if (contentSize.Width <= 0 ||
            contentSize.Height <= 0)
        {
            return null;
        }

        var rect = new Rect(0, 0, contentSize.Width, contentSize.Height);
        var radii = new Border.Radii(border.CornerRadius, border.BorderThickness, false);

        var contentGeometry = new StreamGeometry();
        using StreamGeometryContext ctx = contentGeometry.Open();
        Border.GenerateGeometry(ctx, rect, radii);

        contentGeometry.Freeze();
        return contentGeometry;
    }

    protected override Geometry? GetLayoutClip(Size layoutSlotSize)
    {
        if (!ClipToBounds)
        {
            return null;
        }

        if (Parent is Border border)
        {
            return border.ContentClip;
        }

        if (Parent is System.Windows.Controls.Border nativeBorder &&
            CalculateBorderContentClip(nativeBorder, layoutSlotSize) is { } nativeBorderContentClip)
        {
            return nativeBorderContentClip;
        }

        var rect = new RectangleGeometry(new Rect(RenderSize));
        rect.Freeze();
        return rect;
    }
}
