using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using LiteObservableLanguages;
using Wpf.Ui.Violeta.Gallery.Globalization;

namespace Wpf.Ui.Violeta.Gallery.Pages.Collections;

public record StepBarEntry(string Header, string Content);

public partial class StepBarPage : Wpf.Ui.Violeta.Controls.Page
{
    public static readonly DependencyProperty StepIndexProperty =
        DependencyProperty.Register(
            nameof(StepIndex),
            typeof(int),
            typeof(StepBarPage),
            new PropertyMetadata(0));

    public int StepIndex
    {
        get => (int)GetValue(StepIndexProperty);
        set => SetValue(StepIndexProperty, value);
    }

    public ObservableCollection<StepBarEntry> StepItems { get; } =
    [
        new(LangKeys.Sample_e8f1a0b2c3.Tr(), LangKeys.Sample_51a5a9cd6d.Tr()),
        new(LangKeys.Sample_e8f1a0b2c3.Tr(), LangKeys.Sample_2da752eef4.Tr()),
        new(LangKeys.Sample_e8f1a0b2c3.Tr(), LangKeys.Sample_1fac3c1795.Tr()),
        new(LangKeys.Sample_e8f1a0b2c3.Tr(), LangKeys.Sample_3b5365276d.Tr()),
    ];

    public ICommand PrevCommand { get; }
    public ICommand NextCommand { get; }

    public StepBarPage()
    {
        PrevCommand = new RelayCommand(() =>
        {
            if (StepIndex > 0)
            {
                StepIndex--;
            }
        });
        NextCommand = new RelayCommand(() => StepIndex++);

        InitializeComponent();
        DataContext = this;
    }
}
