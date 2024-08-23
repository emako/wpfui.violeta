using System.Windows;

namespace Wpf.Ui.Violeta.Controls;

public class NavigationViewItemPresenterTemplateSettings : DependencyObject
{
    public NavigationViewItemPresenterTemplateSettings()
    {
    }

    #region IconWidth

    private static readonly DependencyPropertyKey IconWidthPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(IconWidth),
            typeof(double),
            typeof(NavigationViewItemPresenterTemplateSettings),
            null);

    public static readonly DependencyProperty IconWidthProperty =
        IconWidthPropertyKey.DependencyProperty;

    public double IconWidth
    {
        get => (double)GetValue(IconWidthProperty);
        internal set => SetValue(IconWidthPropertyKey, value);
    }

    #endregion IconWidth

    #region SmallerIconWidth

    private static readonly DependencyPropertyKey SmallerIconWidthPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(SmallerIconWidth),
            typeof(double),
            typeof(NavigationViewItemPresenterTemplateSettings),
            null);

    public static readonly DependencyProperty SmallerIconWidthProperty =
        SmallerIconWidthPropertyKey.DependencyProperty;

    public double SmallerIconWidth
    {
        get => (double)GetValue(SmallerIconWidthProperty);
        internal set => SetValue(SmallerIconWidthPropertyKey, value);
    }

    #endregion SmallerIconWidth
}
