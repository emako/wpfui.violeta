using System.Windows;
using System.Windows.Input;

namespace Wpf.Ui.Violeta.Controls.Compat
{
    internal static class FocusManagerEx
    {
        public static UIElement FindNextFocusableElement(FocusNavigationDirection focusNavigationDirection)
        {
            if (Keyboard.FocusedElement is UIElement focusedElement)
            {
                return focusedElement.PredictFocus(focusNavigationDirection) as UIElement;
            }

            return null;
        }
    }
}

