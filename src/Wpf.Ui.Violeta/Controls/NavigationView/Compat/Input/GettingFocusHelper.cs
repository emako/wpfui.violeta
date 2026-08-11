using System;
using System.Windows;
using System.Windows.Input;

namespace Wpf.Ui.Violeta.Controls.Compat;

internal class GettingFocusHelper : IDisposable
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public GettingFocusHelper(UIElement owner)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        _owner = owner;
        _owner.PreviewGotKeyboardFocus += OnPreviewGotKeyboardFocus;
    }

    public void Dispose()
    {
        _owner?.PreviewGotKeyboardFocus -= OnPreviewGotKeyboardFocus;
        _owner = null!;
    }

    public event TypedEventHandler<UIElement, GettingFocusEventArgs> GettingFocus;

    private void OnPreviewGotKeyboardFocus(object? sender, KeyboardFocusChangedEventArgs e)
    {
        if (_ignoreGotFocus)
        {
            return;
        }

        var gettingFocus = GettingFocus;
        if (gettingFocus != null)
        {
            try
            {
                _ignoreGotFocus = true;

                var args = new GettingFocusEventArgs(e);

                gettingFocus((sender as UIElement)!, args);

                if (args.Cancel)
                {
                    e.Handled = true;
                }
            }
            finally
            {
                _ignoreGotFocus = false;
            }
        }
    }

    private UIElement _owner;
    private bool _ignoreGotFocus;
}
