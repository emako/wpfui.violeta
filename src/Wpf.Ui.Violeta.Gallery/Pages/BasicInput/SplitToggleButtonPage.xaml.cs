using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.Input;
using LiteObservableLanguages;
using Wpf.Ui.Violeta.Gallery.Globalization;

namespace Wpf.Ui.Violeta.Gallery.Pages.BasicInput;

public partial class SplitToggleButtonPage : Wpf.Ui.Violeta.Controls.Page
{
    public ObservableCollection<ToolItem> Tools { get; } =
    [
        new("Brush", "Paint"),
        new("Eraser", "Erase"),
        new("Fill", "Bucket"),
        new("Eyedropper", "Pick"),
    ];

    public ObservableCollection<ColorItem> Colors { get; } =
    [
        new("Crimson", Brushes.Crimson),
        new("SeaGreen", Brushes.SeaGreen),
        new("DodgerBlue", Brushes.DodgerBlue),
        new("Gold", Brushes.Gold),
    ];

    public ICommand DoubleCommand { get; }

    public SplitToggleButtonPage()
    {
        DoubleCommand = new RelayCommand(OnDoubleCommand);
        DataContext = this;
        InitializeComponent();

        BasicSplitToggle.Checked += (_, _) => UpdateBasicStatus();
        BasicSplitToggle.Unchecked += (_, _) => UpdateBasicStatus();
        UpdateBasicStatus();
    }

    private void BasicSplitToggle_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateBasicStatus();
    }

    private void CommandSplitToggle_OnClick(object sender, RoutedEventArgs e)
    {
        CommandStatusText.Text = LangKeys.Format_SplitToggleCommandClick.Tr(CommandSplitToggle.IsChecked == true);
    }

    private void OnDoubleCommand()
    {
        CommandStatusText.Text = LangKeys.Format_SplitToggleDoubleCommand.Tr(CommandSplitToggle.IsChecked == true);
    }

    private void UpdateBasicStatus()
    {
        var selected = BasicSplitToggle.SelectedItem?.ToString() ?? LangKeys.Sample_97139627c1.Tr();
        BasicStatusText.Text = LangKeys.Format_SplitToggleBasicStatus.Tr(BasicSplitToggle.IsChecked == true, selected);
    }

    public sealed record ToolItem(string Name, string Description);

    public sealed record ColorItem(string Name, Brush Brush);
}
