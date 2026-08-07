using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Wpf.Ui.Violeta.Controls;

namespace Wpf.Ui.Violeta.Gallery.Pages.Selectors;

public partial class SwatchPickerPage : Wpf.Ui.Violeta.Controls.Page
{
    public SwatchPickerPage()
    {
        InitializeComponent();
    }

    private void OnSelectedValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
    }

    private void OnShapeSwatchClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Swatch clicked || VisualTreeHelper.GetParent(clicked) is not Panel row)
        {
            return;
        }

        foreach (Swatch swatch in row.Children.OfType<Swatch>())
        {
            swatch.IsSelected = ReferenceEquals(swatch, clicked);
        }
    }
}
