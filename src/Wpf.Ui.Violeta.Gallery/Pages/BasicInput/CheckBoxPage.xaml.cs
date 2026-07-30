using System.Windows;
using System.Windows.Controls;

namespace Wpf.Ui.Violeta.Gallery.Pages.BasicInput;

public partial class CheckBoxPage : Wpf.Ui.Violeta.Controls.Page
{
    private bool _updatingSelectAll;

    public CheckBoxPage()
    {
        InitializeComponent();
    }

    private void SelectAll_Checked(object sender, RoutedEventArgs e)
    {
        if (_updatingSelectAll)
        {
            return;
        }

        _updatingSelectAll = true;
        Option1CheckBox.IsChecked = true;
        Option2CheckBox.IsChecked = true;
        Option3CheckBox.IsChecked = true;
        _updatingSelectAll = false;
    }

    private void SelectAll_Unchecked(object sender, RoutedEventArgs e)
    {
        if (_updatingSelectAll)
        {
            return;
        }

        _updatingSelectAll = true;
        Option1CheckBox.IsChecked = false;
        Option2CheckBox.IsChecked = false;
        Option3CheckBox.IsChecked = false;
        _updatingSelectAll = false;
    }

    private void SelectAll_Indeterminate(object sender, RoutedEventArgs e)
    {
    }

    private void Option_CheckedChanged(object sender, RoutedEventArgs e)
    {
        if (_updatingSelectAll)
        {
            return;
        }

        _updatingSelectAll = true;

        var checkedCount =
            (Option1CheckBox.IsChecked == true ? 1 : 0)
            + (Option2CheckBox.IsChecked == true ? 1 : 0)
            + (Option3CheckBox.IsChecked == true ? 1 : 0);

        SelectAllCheckBox.IsChecked = checkedCount switch
        {
            0 => false,
            3 => true,
            _ => null,
        };

        _updatingSelectAll = false;
    }
}
