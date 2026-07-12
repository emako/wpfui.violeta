#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8618, CS8619, CS8625

using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Wpf.Ui.Violeta.Controls.Compat;

public static class ValidationHelper
{
    #region IsTemplateValidationAdornerSite

    public static readonly DependencyProperty IsTemplateValidationAdornerSiteProperty =
        DependencyProperty.RegisterAttached(
            "IsTemplateValidationAdornerSite",
            typeof(bool),
            typeof(ValidationHelper),
            new PropertyMetadata(OnIsTemplateValidationAdornerSiteChanged));

    public static bool GetIsTemplateValidationAdornerSite(FrameworkElement element)
    {
        return (bool)element.GetValue(IsTemplateValidationAdornerSiteProperty);
    }

    public static void SetIsTemplateValidationAdornerSite(FrameworkElement element, bool value)
    {
        element.SetValue(IsTemplateValidationAdornerSiteProperty, value);
    }

    private static void OnIsTemplateValidationAdornerSiteChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var element = (FrameworkElement)d;
        if ((bool)e.NewValue)
        {
            Debug.Assert(element.TemplatedParent != null);
            Validation.SetErrorTemplate(element, null);
            Validation.SetValidationAdornerSiteFor(element, element.TemplatedParent);
        }
        else
        {
            element.ClearValue(Validation.ErrorTemplateProperty);
            element.ClearValue(Validation.ValidationAdornerSiteForProperty);
        }
    }

    #endregion IsTemplateValidationAdornerSite
}
