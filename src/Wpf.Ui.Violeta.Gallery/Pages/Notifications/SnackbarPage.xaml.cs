using System;
using System.Windows;
using Wpf.Ui.Controls;

namespace Wpf.Ui.Violeta.Gallery.Pages.Notifications;

public partial class SnackbarPage : Wpf.Ui.Violeta.Controls.Page
{
    private Snackbar? _snackbar;

    public SnackbarPage()
    {
        InitializeComponent();
    }

    private void ShowSnackbar_Click(object sender, RoutedEventArgs e)
    {
        var appearance = AppearanceComboBox.SelectedIndex switch
        {
            1 => ControlAppearance.Secondary,
            2 => ControlAppearance.Info,
            3 => ControlAppearance.Success,
            4 => ControlAppearance.Caution,
            5 => ControlAppearance.Danger,
            6 => ControlAppearance.Light,
            7 => ControlAppearance.Dark,
            8 => ControlAppearance.Transparent,
            _ => ControlAppearance.Primary,
        };

        var timeout = TimeSpan.FromSeconds((int)TimeoutSlider.Value);

        _snackbar ??= new Snackbar(SnackbarPresenter);
        _snackbar.SetCurrentValue(Snackbar.TitleProperty, "Don't Blame Yourself.");
        _snackbar.SetCurrentValue(System.Windows.Controls.ContentControl.ContentProperty, "No Witcher's Ever Died In His Bed.");
        _snackbar.SetCurrentValue(Snackbar.AppearanceProperty, appearance);
        _snackbar.SetCurrentValue(Snackbar.IconProperty, new SymbolIcon(SymbolRegular.Fluent24));
        _snackbar.SetCurrentValue(Snackbar.TimeoutProperty, timeout);
        _snackbar.Show(true);
    }
}
