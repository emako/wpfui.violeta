using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Violeta.Controls;
using LiteObservableLanguages;
using Wpf.Ui.Violeta.Gallery.Globalization;

namespace Wpf.Ui.Violeta.Gallery.Pages.Collections;

public record TimelineEntry(string Header, string Description, global::System.DateTime Time);

public partial class TimelinePage : Wpf.Ui.Violeta.Controls.Page
{
    public ObservableCollection<TimelineEntry> TimelineItems { get; } =
    [
        new(LangKeys.Sample_b420d8f320.Tr(), LangKeys.Sample_600166f742.Tr(), new global::System.DateTime(2024, 1, 1)),
        new(LangKeys.Sample_b08890a6ef.Tr(), LangKeys.Sample_2ac5ec7017.Tr(), new global::System.DateTime(2024, 2, 15)),
        new(LangKeys.Sample_3ff3c3e26a.Tr(), LangKeys.Sample_1ec0c7b465.Tr(), new global::System.DateTime(2024, 5, 1)),
        new(LangKeys.Sample_83611abd5f.Tr(), LangKeys.Sample_29df7ab4fb.Tr(), new global::System.DateTime(2024, 6, 1)),
    ];

    public TimelinePage()
    {
        InitializeComponent();
        DataContext = this;
    }
}
