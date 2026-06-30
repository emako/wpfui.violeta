#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8618, CS8619, CS8625

using System.Windows;
using System.Windows.Controls;

namespace Wpf.Ui.Violeta.Controls.Compat;

public static class TextControlContentHostHelper
{
    #region ContentPresenterMargin

    public static readonly DependencyProperty ContentPresenterMarginProperty =
        DependencyProperty.RegisterAttached(
            "ContentPresenterMargin",
            typeof(Thickness),
            typeof(TextControlContentHostHelper));

    public static Thickness GetContentPresenterMargin(ScrollViewer contentHost)
    {
        return (Thickness)contentHost.GetValue(ContentPresenterMarginProperty);
    }

    public static void SetContentPresenterMargin(ScrollViewer contentHost, Thickness value)
    {
        contentHost.SetValue(ContentPresenterMarginProperty, value);
    }

    #endregion ContentPresenterMargin
}
