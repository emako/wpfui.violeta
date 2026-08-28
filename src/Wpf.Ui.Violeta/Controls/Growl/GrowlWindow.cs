using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Desktop-level transparent topmost window that hosts global growls.
/// </summary>
public sealed class GrowlWindow : Window
{
    internal Panel GrowlPanel { get; }

    static GrowlWindow()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(GrowlWindow),
            new FrameworkPropertyMetadata(typeof(GrowlWindow)));
    }

    internal GrowlWindow()
    {
        WindowStyle = WindowStyle.None;
        AllowsTransparency = true;
        Background = System.Windows.Media.Brushes.Transparent;
        ShowActivated = false;
        ShowInTaskbar = false;
        Topmost = true;
        Width = 340;
        MaxWidth = 340;

        GrowlPanel = new StackPanel();
        Content = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Hidden,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Content = GrowlPanel,
        };
    }

    internal void UpdatePosition(GrowlTransitionMode transitionMode)
    {
        var desktopWorkingArea = SystemParameters.WorkArea;
        Height = desktopWorkingArea.Height;
        Top = desktopWorkingArea.Top;

        var panelHorizontalAlignment = Growl.GetPanelHorizontalAlignment(transitionMode);
        Left = panelHorizontalAlignment switch
        {
            HorizontalAlignment.Right => desktopWorkingArea.Right - Width,
            HorizontalAlignment.Left => desktopWorkingArea.Left,
            HorizontalAlignment.Center => desktopWorkingArea.Left + (desktopWorkingArea.Width - Width) * 0.5,
            _ => desktopWorkingArea.Right - Width,
        };

        Growl.SetTransitionMode(this, transitionMode);
        GrowlPanel.VerticalAlignment = Growl.GetPanelVerticalAlignment(transitionMode);
        GrowlPanel.HorizontalAlignment = panelHorizontalAlignment;
    }

    protected override void OnSourceInitialized(System.EventArgs e)
    {
        base.OnSourceInitialized(e);

        if (PresentationSource.FromVisual(this) is HwndSource hwndSource)
        {
            // Remove system menu so Alt+Space / Alt+F4 do not interfere with a chrome-less toast window.
            var hwnd = hwndSource.Handle;
            var style = NativeMethods.GetWindowLong(hwnd, NativeMethods.GWL_STYLE);
            NativeMethods.SetWindowLong(hwnd, NativeMethods.GWL_STYLE, style & ~NativeMethods.WS_SYSMENU);
        }
    }

    private static class NativeMethods
    {
        public const int GWL_STYLE = -16;
        public const int WS_SYSMENU = 0x80000;

        [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        private static extern int GetWindowLong32(System.IntPtr hWnd, int nIndex);

        [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
        private static extern System.IntPtr GetWindowLongPtr64(System.IntPtr hWnd, int nIndex);

        [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        private static extern int SetWindowLong32(System.IntPtr hWnd, int nIndex, int dwNewLong);

        [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern System.IntPtr SetWindowLongPtr64(System.IntPtr hWnd, int nIndex, System.IntPtr dwNewLong);

        public static int GetWindowLong(System.IntPtr hWnd, int nIndex)
            => System.IntPtr.Size == 8
                ? (int)GetWindowLongPtr64(hWnd, nIndex)
                : GetWindowLong32(hWnd, nIndex);

        public static int SetWindowLong(System.IntPtr hWnd, int nIndex, int dwNewLong)
            => System.IntPtr.Size == 8
                ? (int)SetWindowLongPtr64(hWnd, nIndex, (System.IntPtr)dwNewLong)
                : SetWindowLong32(hWnd, nIndex, dwNewLong);
    }
}
