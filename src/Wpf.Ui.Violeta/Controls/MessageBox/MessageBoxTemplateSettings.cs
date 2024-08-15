using System.Windows;
using Wpf.Ui.Controls;

namespace Wpf.Ui.Violeta.Controls;

public class MessageBoxTemplateSettings : DependencyObject
{
    internal MessageBoxTemplateSettings()
    {
    }

    private static readonly DependencyPropertyKey IconElementPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(IconElement), typeof(IconElement), typeof(MessageBoxTemplateSettings), null);

    public static readonly DependencyProperty IconElementProperty = IconElementPropertyKey.DependencyProperty;

    public IconElement IconElement
    {
        get => (IconElement)GetValue(IconElementProperty);
        internal set => SetValue(IconElementPropertyKey, value);
    }

    public static readonly DependencyProperty OKButtonTextProperty =
        DependencyProperty.Register(nameof(OKButtonText), typeof(string), typeof(MessageBoxTemplateSettings), new PropertyMetadata("OK"));

    public string OKButtonText
    {
        get => (string)GetValue(OKButtonTextProperty);
        set => SetValue(OKButtonTextProperty, value);
    }

    public static readonly DependencyProperty YesButtonTextProperty =
        DependencyProperty.Register(nameof(YesButtonText), typeof(string), typeof(MessageBoxTemplateSettings), new PropertyMetadata("YES"));

    public string YesButtonText
    {
        get => (string)GetValue(YesButtonTextProperty);
        set => SetValue(YesButtonTextProperty, value);
    }

    public static readonly DependencyProperty NoButtonTextProperty =
        DependencyProperty.Register(nameof(NoButtonText), typeof(string), typeof(MessageBoxTemplateSettings), new PropertyMetadata("NO"));

    public string NoButtonText
    {
        get => (string)GetValue(NoButtonTextProperty);
        set => SetValue(NoButtonTextProperty, value);
    }

    public static readonly DependencyProperty CancelButtonTextProperty =
        DependencyProperty.Register(nameof(CancelButtonText), typeof(string), typeof(MessageBoxTemplateSettings), new PropertyMetadata("CANCEL"));

    public string CancelButtonText
    {
        get => (string)GetValue(CancelButtonTextProperty);
        set => SetValue(CancelButtonTextProperty, value);
    }
}
