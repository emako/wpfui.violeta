using System;
using System.Windows.Documents;
using System.Windows.Media;

namespace Wpf.Ui.Emoji;

internal static class Extensions
{
    /// <summary>
    /// Advance a TextPointer to the nth character
    /// </summary>
    public static TextPointer GetPositionAtCharOffset(this TextPointer p, int offset)
    {
        var fallback = offset > 0 ? p.DocumentEnd : p.DocumentStart;
        while (offset != 0 && p != null)
        {
            var dir = offset > 0 ? LogicalDirection.Forward : LogicalDirection.Backward;
            if (p.GetPointerContext(dir) == TextPointerContext.Text)
            {
                var text = p.GetTextInRun(dir);
                if (text.Length >= Math.Abs(offset))
                    return p.GetPositionAtOffset(offset);
                offset -= Math.Sign(offset) * text.Length;
            }
            p = p.GetNextContextPosition(dir);
        }
        return p ?? fallback;
    }

    public static SolidColorBrush ToBrush(this Color c, bool freeze = true)
    {
        var brush = new SolidColorBrush(c);

        if (freeze && brush.CanFreeze)
        {
            brush.Freeze();
        }

        return brush;
    }
}
