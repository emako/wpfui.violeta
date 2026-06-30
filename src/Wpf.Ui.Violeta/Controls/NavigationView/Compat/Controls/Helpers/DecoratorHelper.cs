#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8618, CS8619, CS8625

using System.Windows;
using System.Windows.Controls;

namespace Wpf.Ui.Violeta.Controls.Compat;

public static class DecoratorHelper
{
    #region Child

    public static readonly DependencyProperty ChildProperty =
        DependencyProperty.RegisterAttached(
            "Child",
            typeof(UIElement),
            typeof(DecoratorHelper),
            new PropertyMetadata(default(UIElement), OnChildChanged));

    public static UIElement GetChild(Decorator border)
    {
        return (UIElement)border.GetValue(ChildProperty);
    }

    public static void SetChild(Decorator border, UIElement value)
    {
        border.SetValue(ChildProperty, value);
    }

    private static void OnChildChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((Decorator)d).Child = (UIElement)e.NewValue;
    }

    #endregion Child
}
