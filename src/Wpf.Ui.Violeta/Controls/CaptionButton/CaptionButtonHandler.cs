using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Controls;

public class CaptionButtonHandler
{
    public CaptionButtonHandler(HwndSource hwndSource)
    {
        _hwndSource = hwndSource;
        _hwndSource.AddHook(OnHwndSourceMessage);
    }

    public void Add(CaptionButton button)
    {
        _buttons.Add(button);

        if (button.IsVisible)
        {
            DependencyObject child = VisualTreeHelper.GetChild(button, 0);
            _cacheChildToButton.Add(child, button);
        }
        else
        {
            button.IsVisibleChanged += OnButtonIsVisibleChanged;
        }
    }

    private void OnButtonIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        CaptionButton button = (CaptionButton)sender;
        if (button.IsVisible)
        {
            DependencyObject child = VisualTreeHelper.GetChild(button, 0);
            _cacheChildToButton.Add(child, button);
            button.IsVisibleChanged -= OnButtonIsVisibleChanged;
        }
    }

    private nint OnHwndSourceMessage(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
    {
        switch (msg)
        {
            case WM_NCHITTEST:
                {
                    CaptionButton? button = GetPointedButton(lParam);
                    if (button is null)  // 鼠标不在任何标题栏按钮上
                    {
                        HoveredButton = null;
                        break;
                    }
                    if (button.IsEnabled)
                    {
                        if (PressedButton is not null && PressedButton != button)  // 已经按下了其他按钮
                        {
                            PressedButton.IsMouseOverInTitleBar = false;
                            PressedButton.IsPressedInTitleBar = false;
                            break;
                        }
                        else if (PressedButton == button)
                        {
                            PressedButton.IsPressedInTitleBar = true;
                        }
                        HoveredButton = button;
                    }
                    handled = true;
                    return (nint)button.Kind;
                }

            case WM_NCLBUTTONDOWN:
                {
                    CaptionButton? button = GetPointedButton(lParam);
                    if (button is null)  // 没点到标题栏按钮上
                    {
                        PressedButton = null;
                        break;
                    }
                    if (button.IsEnabled)
                    {
                        PressedButton = button;
                    }
                    handled = true;
                    break;
                }

            case WM_NCLBUTTONUP:
                {
                    CaptionButton? button = GetPointedButton(lParam);
                    if (button is null)
                    {
                        PressedButton = null;
                        break;
                    }

                    if (button.IsEnabled && button == PressedButton)
                    {
                        button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                    }
                    PressedButton = null;
                    handled = true;
                    break;
                }

            case WM_NCMOUSELEAVE:
                {
                    HoveredButton = null;
                    PressedButton = null;
                    break;
                }
        }
        return 0;
    }

    private CaptionButton? GetPointedButton(nint lParam)
    {
        Point pointerScreenPosition = new(GET_X_LPARAM(lParam), GET_Y_LPARAM(lParam));
        Window ownerWindow = Window.GetWindow(_buttons.First());
        return ownerWindow.InputHitTest(ownerWindow.PointFromScreen(pointerScreenPosition)) is DependencyObject hit && _cacheChildToButton.TryGetValue(hit, out CaptionButton? button) ? button : null;
    }

    private static nint GET_X_LPARAM(nint lParam) => lParam & 0x0000FFFF;

    private static nint GET_Y_LPARAM(nint lParam) => (lParam >> 16) & 0x0000FFFF;

    private readonly HwndSource _hwndSource;
    private readonly HashSet<CaptionButton> _buttons = [];
    private readonly Dictionary<DependencyObject, CaptionButton> _cacheChildToButton = [];

    private CaptionButton? HoveredButton
    {
        get => field;
        set
        {
            field?.IsMouseOverInTitleBar = false;
            field = value;
            field?.IsMouseOverInTitleBar = true;
        }
    }

    private CaptionButton? PressedButton
    {
        get => field;
        set
        {
            field?.IsPressedInTitleBar = false;
            field = value;
            field?.IsPressedInTitleBar = true;
        }
    }

    private const int WM_NCHITTEST = 0x0084;
    private const int WM_NCLBUTTONDOWN = 0x00A1;
    private const int WM_NCLBUTTONUP = 0x00A2;
    private const int WM_NCMOUSELEAVE = 0x02A2;
}
