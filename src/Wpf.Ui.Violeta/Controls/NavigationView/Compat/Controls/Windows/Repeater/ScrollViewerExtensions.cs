#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8618, CS8619, CS8625

using System.Windows;
using System.Windows.Controls;

namespace Wpf.Ui.Violeta.Controls.Compat;

internal static class ScrollViewerExtensions
{
    public static UIElement GetContentTemplateRoot(this ScrollViewer scrollViewer)
    {
        return scrollViewer.Content as UIElement;
    }

    public static bool ChangeView(this ScrollViewer scrollViewer,
        double? horizontalOffset,
        double? verticalOffset,
        float? zoomFactor)
    {
        return scrollViewer.ChangeView(horizontalOffset, verticalOffset, zoomFactor, false);
    }

    public static bool ChangeView(this ScrollViewer scrollViewer,
        double? horizontalOffset,
        double? verticalOffset,
        float? zoomFactor,
        bool disableAnimation)
    {
        if (horizontalOffset.HasValue)
        {
            scrollViewer.ScrollToHorizontalOffset(horizontalOffset.Value);
        }

        if (verticalOffset.HasValue)
        {
            scrollViewer.ScrollToVerticalOffset(verticalOffset.Value);
        }

        return true; // TODO
    }
}
