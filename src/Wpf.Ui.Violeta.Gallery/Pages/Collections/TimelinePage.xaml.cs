using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Violeta.Controls;

namespace Wpf.Ui.Violeta.Gallery.Pages.Collections;

public record TimelineEntry(string Header, string Description, global::System.DateTime Time);

public partial class TimelinePage : Wpf.Ui.Violeta.Controls.Page
{
    public ObservableCollection<TimelineEntry> TimelineItems { get; } =
    [
        new("立项", "完成需求确认", new global::System.DateTime(2024, 1, 1)),
        new("设计", "UI 评审通过", new global::System.DateTime(2024, 2, 15)),
        new("开发", "核心功能合入", new global::System.DateTime(2024, 5, 1)),
        new("发布", "v1.0 上线", new global::System.DateTime(2024, 6, 1)),
    ];

    public TimelinePage()
    {
        InitializeComponent();
        DataContext = this;
    }
}
