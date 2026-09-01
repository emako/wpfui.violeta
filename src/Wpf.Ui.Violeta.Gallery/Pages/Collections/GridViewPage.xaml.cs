using CommunityToolkit.Mvvm.ComponentModel;
using LiteObservableLanguages;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Wpf.Ui.Violeta.Controls;
using Wpf.Ui.Violeta.Gallery.Globalization;

namespace Wpf.Ui.Violeta.Gallery.Pages.Collections;

public partial class GridViewPage : Wpf.Ui.Violeta.Controls.Page
{
    public ObservableCollection<GridViewSampleItem> Items { get; } = [];

    public GridViewPage()
    {
        InitializeComponent();
        DataContext = this;

        string[] colors =
        [
            "#2060C0", "#20A060", "#C04020", "#8040C0",
            "#C08020", "#2080A0", "#A02060", "#4060A0",
        ];

        for (int i = 0; i < colors.Length; i++)
        {
            Items.Add(new GridViewSampleItem
            {
                Title = LangKeys.Format_Item.Tr(i + 1),
                Description = LangKeys.Format_SampleCard.Tr(i + 1),
                Accent = (Brush)new BrushConverter().ConvertFromString(colors[i])!,
            });
        }
    }

    private void BasicGridView_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is GridViewSampleItem item)
        {
            ClickOutput.Text = LangKeys.Format_Clicked.Tr(item.Title);
        }
    }

    private void SelectionModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ContentGridView is null || SelectionModeComboBox.SelectedItem is not System.Windows.Controls.ComboBoxItem selected)
        {
            return;
        }

        switch (selected.Tag?.ToString())
        {
            case "None":
                ContentGridView.IsSelectionEnabled = false;
                break;

            case "Single":
                ContentGridView.IsSelectionEnabled = true;
                ContentGridView.SelectionMode = SelectionMode.Single;
                break;

            case "Multiple":
                ContentGridView.IsSelectionEnabled = true;
                ContentGridView.SelectionMode = SelectionMode.Multiple;
                break;

            case "Extended":
                ContentGridView.IsSelectionEnabled = true;
                ContentGridView.SelectionMode = SelectionMode.Extended;
                break;
        }
    }

    private void ContentGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        SelectionOutput.Text = LangKeys.Format_SelectedItemCount.Tr(ContentGridView.SelectedItems.Count);
    }

    private void ItemClickCheckBox_Click(object sender, RoutedEventArgs e)
    {
        ClickOutput.Text = string.Empty;
    }
}

public partial class GridViewSampleItem : ObservableObject
{
    [ObservableProperty]
    public partial string Title { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Description { get; set; } = string.Empty;

    [ObservableProperty]
    public partial Brush Accent { get; set; } = Brushes.SteelBlue;
}
