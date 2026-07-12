#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8618, CS8619, CS8625

using System.Windows;
using System.Windows.Controls.Primitives;

namespace Wpf.Ui.Violeta.Controls.Compat;

public static class ScrollBarThumbHelper
{
    public static readonly DependencyProperty IsExpandedProperty =
        DependencyProperty.RegisterAttached(
            "IsExpanded",
            typeof(bool),
            typeof(ScrollBarThumbHelper),
            new PropertyMetadata(false));

    public static bool GetIsExpanded(Thumb thumb)
    {
        return (bool)thumb.GetValue(IsExpandedProperty);
    }

    public static void SetIsExpanded(Thumb thumb, bool value)
    {
        thumb.SetValue(IsExpandedProperty, value);
    }
}
