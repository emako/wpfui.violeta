using System.Collections.Generic;
using Wpf.Ui.Violeta.Controls;

namespace Wpf.Ui.Violeta.Gallery.Pages.Status;

public partial class MeterBarPage : Page
{
    public MeterBarPage()
    {
        InitializeComponent();
        DataContext = this;
    }

    public double DataPlanTotal { get; } = 50;

    public IReadOnlyList<DataPlanUsage> DataPlanUsages { get; } =
    [
        new("Video", 21.5, "21.5 GB"),
        new("Social", 8.2, "8.2 GB"),
        new("Browsing", 4.6, "4.6 GB"),
    ];
}

public sealed record DataPlanUsage(string Name, double Used, string UsedText);
