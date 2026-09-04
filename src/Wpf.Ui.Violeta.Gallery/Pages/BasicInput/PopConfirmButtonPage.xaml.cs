using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using LiteObservableLanguages;
using Wpf.Ui.Violeta.Gallery.Globalization;

namespace Wpf.Ui.Violeta.Gallery.Pages.BasicInput;

public partial class PopConfirmButtonPage : Wpf.Ui.Violeta.Controls.Page
{
    public PopConfirmButtonPage()
    {
        ConfirmedCommand = new RelayCommand(() => StatusText.Text = LangKeys.Sample_581eca24d1.Tr());
        CancelledCommand = new RelayCommand(() => StatusText.Text = LangKeys.Sample_692fdb35e2.Tr());
        DataContext = this;
        InitializeComponent();
    }

    public ICommand ConfirmedCommand { get; }

    public ICommand CancelledCommand { get; }
}
