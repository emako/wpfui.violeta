#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8618, CS8619, CS8625

using System.Windows;
using System.Windows.Input;

namespace Wpf.Ui.Violeta.Controls.Compat;

internal static class FocusManagerEx
{
    public static UIElement FindNextFocusableElement(FocusNavigationDirection focusNavigationDirection)
    {
        if (Keyboard.FocusedElement is UIElement focusedElement)
        {
            return focusedElement.PredictFocus(focusNavigationDirection) as UIElement;
        }

        return null!;
    }
}
