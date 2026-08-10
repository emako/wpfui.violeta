using System;
using Wpf.Ui.Violeta.Controls;

namespace Wpf.Ui.Violeta.Gallery.Pages.BasicInput;

public partial class VirtualJoystickPage : Page
{
    public VirtualJoystickPage()
    {
        InitializeComponent();
    }

    private void OnJoystickMoved(object? sender, JoystickMoveEventArgs e)
    {
        VectorTextBlock.Text = $"Vector: X {e.NormalizedX:F2}, Y {e.NormalizedY:F2}";
    }

    private void OnDiameterChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
    {
        if (ResizableJoystick != null)
        {
            ResizableJoystick.PadDiameter = e.NewValue;
        }
    }
}
