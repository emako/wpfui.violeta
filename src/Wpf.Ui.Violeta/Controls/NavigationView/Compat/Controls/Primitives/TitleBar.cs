using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Controls.Compat;

internal static class TitleBar
{
    public static readonly DependencyProperty ExtendViewIntoTitleBarProperty =
        DependencyProperty.RegisterAttached(
            "ExtendViewIntoTitleBar",
            typeof(bool),
            typeof(TitleBar),
            new PropertyMetadata(false));

    public static readonly DependencyProperty HeightProperty =
        DependencyProperty.RegisterAttached(
            "Height",
            typeof(double),
            typeof(TitleBar),
            new PropertyMetadata(32d));

    public static readonly DependencyProperty SystemOverlayLeftInsetProperty =
        DependencyProperty.RegisterAttached(
            "SystemOverlayLeftInset",
            typeof(double),
            typeof(TitleBar),
            new PropertyMetadata(0d));

    public static readonly DependencyProperty SystemOverlayRightInsetProperty =
        DependencyProperty.RegisterAttached(
            "SystemOverlayRightInset",
            typeof(double),
            typeof(TitleBar),
            new PropertyMetadata(0d));

    public static readonly RoutedEvent BackRequestedEvent =
        EventManager.RegisterRoutedEvent(
            "BackRequested",
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(TitleBar));

    public static bool GetExtendViewIntoTitleBar(DependencyObject element) => (bool)element.GetValue(ExtendViewIntoTitleBarProperty);

    public static void SetExtendViewIntoTitleBar(DependencyObject element, bool value) => element.SetValue(ExtendViewIntoTitleBarProperty, value);

    public static double GetHeight(DependencyObject element) => (double)element.GetValue(HeightProperty);

    public static void SetHeight(DependencyObject element, double value) => element.SetValue(HeightProperty, value);

    public static double GetSystemOverlayLeftInset(DependencyObject element) => (double)element.GetValue(SystemOverlayLeftInsetProperty);

    public static void SetSystemOverlayLeftInset(DependencyObject element, double value) => element.SetValue(SystemOverlayLeftInsetProperty, value);

    public static double GetSystemOverlayRightInset(DependencyObject element) => (double)element.GetValue(SystemOverlayRightInsetProperty);

    public static void SetSystemOverlayRightInset(DependencyObject element, double value) => element.SetValue(SystemOverlayRightInsetProperty, value);

    public static readonly DependencyProperty ResizeBorderThicknessProperty =
        DependencyProperty.RegisterAttached("ResizeBorderThickness", typeof(Thickness), typeof(TitleBar), new PropertyMetadata(default(Thickness)));

    public static readonly DependencyProperty InactiveBackgroundProperty =
        DependencyProperty.RegisterAttached("InactiveBackground", typeof(Brush), typeof(TitleBar), new PropertyMetadata(default(Brush)));

    public static readonly DependencyProperty InactiveForegroundProperty =
        DependencyProperty.RegisterAttached("InactiveForeground", typeof(Brush), typeof(TitleBar), new PropertyMetadata(default(Brush)));

    public static readonly DependencyProperty ButtonStyleProperty =
        DependencyProperty.RegisterAttached("ButtonStyle", typeof(Style), typeof(TitleBar), new PropertyMetadata(default(Style)));

    public static readonly DependencyProperty IsIconVisibleProperty =
        DependencyProperty.RegisterAttached("IsIconVisible", typeof(bool), typeof(TitleBar), new PropertyMetadata(true));

    public static readonly DependencyProperty IsBackButtonVisibleProperty =
        DependencyProperty.RegisterAttached("IsBackButtonVisible", typeof(bool), typeof(TitleBar), new PropertyMetadata(false));

    public static readonly DependencyProperty IsBackEnabledProperty =
        DependencyProperty.RegisterAttached("IsBackEnabled", typeof(bool), typeof(TitleBar), new PropertyMetadata(false));

    public static readonly DependencyProperty BackButtonCommandProperty =
        DependencyProperty.RegisterAttached("BackButtonCommand", typeof(ICommand), typeof(TitleBar), new PropertyMetadata(default(ICommand)));

    public static readonly DependencyProperty BackButtonCommandParameterProperty =
        DependencyProperty.RegisterAttached("BackButtonCommandParameter", typeof(object), typeof(TitleBar), new PropertyMetadata(null));

    public static readonly DependencyProperty BackButtonCommandTargetProperty =
        DependencyProperty.RegisterAttached("BackButtonCommandTarget", typeof(IInputElement), typeof(TitleBar), new PropertyMetadata(default(IInputElement)));

    public static readonly DependencyProperty BackButtonStyleProperty =
        DependencyProperty.RegisterAttached("BackButtonStyle", typeof(Style), typeof(TitleBar), new PropertyMetadata(default(Style)));

    public static readonly DependencyProperty CloseButtonAvailabilityProperty =
        DependencyProperty.RegisterAttached("CloseButtonAvailability", typeof(TitleBarButtonAvailability), typeof(TitleBar), new PropertyMetadata(TitleBarButtonAvailability.Auto));

    public static readonly DependencyProperty MinimizeButtonAvailabilityProperty =
        DependencyProperty.RegisterAttached("MinimizeButtonAvailability", typeof(TitleBarButtonAvailability), typeof(TitleBar), new PropertyMetadata(TitleBarButtonAvailability.Auto));

    public static readonly DependencyProperty MaximizeButtonAvailabilityProperty =
        DependencyProperty.RegisterAttached("MaximizeButtonAvailability", typeof(TitleBarButtonAvailability), typeof(TitleBar), new PropertyMetadata(TitleBarButtonAvailability.Auto));

    public static readonly DependencyProperty ButtonGlyphStyleProperty =
        DependencyProperty.RegisterAttached("ButtonGlyphStyle", typeof(TitleBarButtonGlyphStyle?), typeof(TitleBar), new PropertyMetadata(null));
}

