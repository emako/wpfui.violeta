using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Data;

namespace Wpf.Ui.Violeta.Gallery.Pages.BasicInput;

public partial class ComboBoxPage : Wpf.Ui.Violeta.Controls.Page
{
    public ComboBoxPage()
    {
        InitializeComponent();
        SetupGroupedComboBox();
    }

    private void SetupGroupedComboBox()
    {
        var items = new List<GroupedItem>
        {
            new("Fruits", "Apple"),
            new("Fruits", "Banana"),
            new("Fruits", "Orange"),
            new("Vegetables", "Carrot"),
            new("Vegetables", "Broccoli"),
            new("Vegetables", "Spinach"),
        };

        var view = CollectionViewSource.GetDefaultView(items);
        view.GroupDescriptions.Add(new PropertyGroupDescription(nameof(GroupedItem.Category)));
        GroupedComboBox.ItemsSource = view;
    }

    private sealed record GroupedItem(string Category, string Name);
}
