using System;
using System.Windows;

namespace Wpf.Ui.Violeta.Gallery.Pages.DateTime;

public partial class DatePickerPage : Wpf.Ui.Violeta.Controls.Page
{
    public DatePickerPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        PreselectedDatePicker.SelectedDate = DateTimeOffset.Now;
    }
}
