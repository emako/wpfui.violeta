using System.Windows;
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Controls;

public sealed class PersonPictureTemplateSettings : DependencyObject
{
    #region ActualImageBrush

    private static readonly DependencyPropertyKey ActualImageBrushPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(ActualImageBrush),
            typeof(ImageBrush),
            typeof(PersonPictureTemplateSettings),
            null);

    public static readonly DependencyProperty ActualImageBrushProperty =
        ActualImageBrushPropertyKey.DependencyProperty;

    public ImageBrush ActualImageBrush
    {
        get => (ImageBrush)GetValue(ActualImageBrushProperty);
        internal set => SetValue(ActualImageBrushPropertyKey, value);
    }

    #endregion ActualImageBrush

    #region ActualInitials

    private static readonly DependencyPropertyKey ActualInitialsPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(ActualInitials),
            typeof(string),
            typeof(PersonPictureTemplateSettings),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty ActualInitialsProperty =
        ActualInitialsPropertyKey.DependencyProperty;

    public string ActualInitials
    {
        get => (string)GetValue(ActualInitialsProperty);
        internal set => SetValue(ActualInitialsPropertyKey, value);
    }

    #endregion ActualInitials
}
