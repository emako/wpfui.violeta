using System.Windows;
using System.Windows.Controls;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Represents an individual item in a <see cref="FlipView"/>.
/// </summary>
public class FlipViewItem : ContentControl
{
    static FlipViewItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(FlipViewItem),
            new FrameworkPropertyMetadata(typeof(FlipViewItem)));
    }
}
