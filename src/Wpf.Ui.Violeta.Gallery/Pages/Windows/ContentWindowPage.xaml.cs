using System;
using System.Windows;
using System.Windows.Controls;
using LiteObservableLanguages;
using Wpf.Ui.Violeta.Controls;
using Wpf.Ui.Violeta.Gallery.Globalization;
using Button = Wpf.Ui.Controls.Button;
using ControlAppearance = Wpf.Ui.Controls.ControlAppearance;

namespace Wpf.Ui.Violeta.Gallery.Pages.Windows;

public partial class ContentWindowPage : Wpf.Ui.Violeta.Controls.Page
{
    public ContentWindowPage()
    {
        InitializeComponent();
    }

    private void OpenConfiguredContentWindow_Click(object sender, RoutedEventArgs e)
    {
        var dialog = CreateDemoWindow();
        ApplyConfiguredOptions(dialog);
        ShowAndReport(dialog);
    }

    private void OpenPreset_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: string tag })
        {
            return;
        }

        var dialog = CreateDemoWindow();
        dialog.Owner = Window.GetWindow(this);
        dialog.CanKeyDownResult = true;
        dialog.InheritIconFromOwner = true;

        switch (tag)
        {
            case "Dialog":
                dialog.Title = "Dialog preset";
                dialog.MinimizeButtonVisibility = Visibility.Collapsed;
                dialog.MaximizeButtonVisibility = Visibility.Collapsed;
                dialog.CloseButtonVisibility = Visibility.Visible;
                dialog.ResizeMode = ResizeMode.NoResize;
                break;

            case "Tool":
                dialog.Title = "Tool window preset";
                dialog.MinimizeButtonVisibility = Visibility.Visible;
                dialog.MaximizeButtonVisibility = Visibility.Visible;
                dialog.CloseButtonVisibility = Visibility.Visible;
                dialog.ResizeMode = ResizeMode.CanResize;
                dialog.ShowInTaskbar = true;
                break;

            case "Help":
                dialog.Title = "Help button preset";
                dialog.HelpButtonVisibility = Visibility.Visible;
                dialog.Loaded += (_, _) =>
                {
                    if (dialog.TitleBar is { } titleBar)
                    {
                        titleBar.HelpButtonClick -= OnHelpClicked;
                        titleBar.HelpButtonClick += OnHelpClicked;
                    }
                };
                break;

            case "NoTitleBar":
                dialog.Title = "No TitleBar";
                dialog.TitleBarVisibility = Visibility.Collapsed;
                dialog.ResizeMode = ResizeMode.NoResize;
                break;

            case "CenterScreen":
                dialog.Title = "CenterScreen preset";
                dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                dialog.Owner = null;
                dialog.ShowInTaskbar = true;
                dialog.MinimizeButtonVisibility = Visibility.Visible;
                dialog.MaximizeButtonVisibility = Visibility.Visible;
                dialog.CloseButtonVisibility = Visibility.Visible;
                break;
        }

        ShowAndReport(dialog);
    }

    private void OnHelpClicked(object? sender, EventArgs e)
    {
        System.Windows.MessageBox.Show("Help clicked", "ContentWindow");
    }

    private ContentWindow CreateDemoWindow()
    {
        return ContentWindow.Create(new DemoContentWindowControl
        {
            Title = string.IsNullOrWhiteSpace(TitleTextBox.Text) ? "ContentWindow Demo" : TitleTextBox.Text.Trim(),
        });
    }

    private void ApplyConfiguredOptions(ContentWindow dialog)
    {
        dialog.Owner = Window.GetWindow(this);
        dialog.Title = string.IsNullOrWhiteSpace(TitleTextBox.Text) ? "ContentWindow Demo" : TitleTextBox.Text.Trim();
        dialog.Width = 480;
        dialog.Height = 300;
        dialog.CanKeyDownResult = CanKeyDownResultToggle.IsChecked == true;
        dialog.ShowInTaskbar = ShowInTaskbarToggle.IsChecked == true;
        dialog.InheritIconFromOwner = InheritIconToggle.IsChecked == true;
        dialog.IsIconVisible = IsIconVisibleToggle.IsChecked == true;
        dialog.IsTitleVisible = IsTitleVisibleToggle.IsChecked == true;
        dialog.WindowStartupLocation = ParseStartupLocation();
        dialog.ResizeMode = ParseResizeMode();

        dialog.TitleBarVisibility = ToVisibility(TitleBarVisibleToggle.IsChecked);
        dialog.BackButtonVisibility = ToVisibility(BackButtonToggle.IsChecked);
        dialog.PaneToggleButtonVisibility = ToVisibility(PaneToggleButtonToggle.IsChecked);
        dialog.MinimizeButtonVisibility = ToVisibility(MinimizeButtonToggle.IsChecked);
        dialog.MaximizeButtonVisibility = ToVisibility(MaximizeButtonToggle.IsChecked);
        dialog.CloseButtonVisibility = ToVisibility(CloseButtonToggle.IsChecked);
        dialog.HelpButtonVisibility = ToVisibility(HelpButtonToggle.IsChecked);

        if (dialog.HelpButtonVisibility == Visibility.Visible)
        {
            dialog.Loaded += (_, _) =>
            {
                if (dialog.TitleBar is { } titleBar)
                {
                    titleBar.HelpButtonClick -= OnHelpClicked;
                    titleBar.HelpButtonClick += OnHelpClicked;
                }
            };
        }
    }

    private void ShowAndReport(ContentWindow dialog)
    {
        dialog.Width = Math.Max(dialog.Width, 420);
        dialog.Height = Math.Max(dialog.Height, 260);
        _ = dialog.ShowDialog();
        ResultText.Text = LangKeys.Format_Result.Tr(dialog.Result);
    }

    private WindowStartupLocation ParseStartupLocation()
    {
        return StartupLocationComboBox.SelectedItem is ComboBoxItem { Tag: string tag }
            && Enum.TryParse(tag, out WindowStartupLocation location)
            ? location
            : WindowStartupLocation.CenterOwner;
    }

    private ResizeMode ParseResizeMode()
    {
        return ResizeModeComboBox.SelectedItem is ComboBoxItem { Tag: string tag }
            && Enum.TryParse(tag, out ResizeMode mode)
            ? mode
            : ResizeMode.CanResize;
    }

    private static Visibility ToVisibility(bool? isChecked)
        => isChecked == true ? Visibility.Visible : Visibility.Collapsed;

    private sealed class DemoContentWindowControl : ContentWindowControl
    {
        public DemoContentWindowControl()
        {
            var message = new TextBlock
            {
                Text = LangKeys.Sample_6d80adabe3.Tr(),
                TextWrapping = TextWrapping.Wrap,
                VerticalAlignment = VerticalAlignment.Top,
            };

            var cancel = new Button
            {
                Content = LangKeys.Sample_625fb26b4b.Tr(),
                Appearance = ControlAppearance.Secondary,
                Margin = new Thickness(0, 0, 8, 0),
                MinWidth = 80,
            };
            cancel.Click += (_, _) => Owner?.OnResultCommandExecuted(ContentWindowResult.Cancel);

            var ok = new Button
            {
                Content = LangKeys.Sample_12cd9a94dc.Tr(),
                Appearance = ControlAppearance.Primary,
                MinWidth = 80,
            };
            ok.Click += (_, _) => Owner?.OnResultCommandExecuted(ContentWindowResult.OK);

            var buttons = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                Children = { cancel, ok },
            };

            var root = new Grid { Margin = new Thickness(24) };
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            Grid.SetRow(message, 0);
            Grid.SetRow(buttons, 1);
            root.Children.Add(message);
            root.Children.Add(buttons);
            Content = root;
        }
    }
}
