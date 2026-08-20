using System;
using Wpf.Ui.Violeta.Controls;

namespace Wpf.Ui.Violeta.Gallery.Pages.Status;

public partial class BusyMaskPage : Wpf.Ui.Violeta.Controls.Page
{
    public IndicatorType[] IndicatorTypes { get; } = Enum.GetValues<IndicatorType>();

    public BusyMaskPage()
    {
        InitializeComponent();
    }
}
