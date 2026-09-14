using System.Collections.ObjectModel;

namespace Wpf.Ui.Violeta.Gallery.Pages.Collections;

public partial class TransferPage : Wpf.Ui.Violeta.Controls.Page
{
    public ObservableCollection<string> BasicItems { get; } =
    [
        "Apple",
        "Banana",
        "Cherry",
        "Dragonfruit",
        "Elderberry",
        "Fig",
        "Grape",
        "Honeydew",
    ];

    public ObservableCollection<TransferDemoItem> CheckBoxItems { get; } =
    [
        new("Alice", "Design"),
        new("Bob", "Engineering"),
        new("Carol", "Product"),
        new("Dave", "Support"),
        new("Eve", "Marketing"),
        new("Frank", "Sales"),
    ];

    public TransferPage()
    {
        DataContext = this;
        InitializeComponent();
    }
}

public sealed record TransferDemoItem(string Name, string Team)
{
    public override string ToString() => $"{Name} ({Team})";
}
