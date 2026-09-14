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

    public TransferPage()
    {
        DataContext = this;
        InitializeComponent();
    }
}
