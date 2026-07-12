using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Controls;

[SuppressMessage("Style", "IDE0001:Simplify name")]
public class Page : Wpf.Ui.Violeta.Controls.Compat.Page
{
    static Page()
    {
        // Not overwriting will prevent rendering
        // DefaultStyleKeyProperty.OverrideMetadata(typeof(Page), new FrameworkPropertyMetadata(typeof(Page)));

        BackgroundProperty.OverrideMetadata(typeof(Page), new FrameworkPropertyMetadata(Brushes.Transparent));
        FontSizeProperty.OverrideMetadata(typeof(Page), new FrameworkPropertyMetadata(13d));
    }
}
