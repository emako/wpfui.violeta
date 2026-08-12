using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.Input;
using LiteObservableLanguages;
using Wpf.Ui.Violeta.Gallery.Globalization;

namespace Wpf.Ui.Violeta.Gallery.Pages.BasicInput;

public partial class SplitButtonPage : Wpf.Ui.Violeta.Controls.Page
{
    public ICommand PrimaryCommand { get; }
    public ICommand DoubleCommand { get; }

    public SplitButtonPage()
    {
        PrimaryCommand = new RelayCommand(OnPrimaryCommand);
        DoubleCommand = new RelayCommand(OnDoubleCommand);
        DataContext = this;
        InitializeComponent();
    }

    private void SaveSplitButton_OnClick(object sender, RoutedEventArgs e)
    {
        SetStatus(SaveStatusText, LangKeys.Format_SplitButtonPrimaryClick.Tr("Save"));
    }

    private void ShareSplitButton_OnClick(object sender, RoutedEventArgs e)
    {
        SetStatus(ShareStatusText, LangKeys.Format_SplitButtonPrimaryClick.Tr("Share"));
    }

    private void AppearanceSplitButton_OnClick(object sender, RoutedEventArgs e)
    {
        var label = (sender as FrameworkElement)?.Tag?.ToString() ?? "Primary";
        SetStatus(AppearanceStatusText, LangKeys.Format_SplitButtonPrimaryClick.Tr(label));
    }

    private void ColorSplitButton_OnClick(object sender, RoutedEventArgs e)
    {
        var brush = ColorSwatch.Background as SolidColorBrush;
        var hex = brush is null ? "(color)" : brush.Color.ToString();
        SetStatus(ColorStatusText, LangKeys.Format_SplitButtonPrimaryClick.Tr(hex));
    }

    private void OnPrimaryCommand()
    {
        SetStatus(CommandStatusText, LangKeys.Format_SplitButtonPrimaryClick.Tr("Apply"));
    }

    private void OnDoubleCommand()
    {
        SetStatus(CommandStatusText, LangKeys.Format_SplitButtonDoubleCommand.Tr("Apply"));
    }

    private void FlyoutMenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem item)
        {
            return;
        }

        var action = item.Tag?.ToString() ?? item.Header?.ToString() ?? "(menu)";
        var target = ResolveStatusTarget(action);
        SetStatus(target, LangKeys.Format_SplitButtonMenuCommand.Tr(action));
    }

    private void ColorMenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem { Tag: string hex })
        {
            return;
        }

        try
        {
            var color = (Color)ColorConverter.ConvertFromString(hex)!;
            ColorSwatch.Background = new SolidColorBrush(color);
            SetStatus(ColorStatusText, LangKeys.Format_SplitButtonMenuCommand.Tr(hex));
        }
        catch
        {
            SetStatus(ColorStatusText, LangKeys.Format_SplitButtonMenuCommand.Tr(hex));
        }
    }

    private TextBlock ResolveStatusTarget(string action)
    {
        if (action.StartsWith("Primary", System.StringComparison.Ordinal)
            || action.StartsWith("Secondary", System.StringComparison.Ordinal)
            || action.StartsWith("Danger", System.StringComparison.Ordinal))
        {
            return AppearanceStatusText;
        }

        if (action.StartsWith("Send", System.StringComparison.Ordinal)
            || action is "Copy link" or "Publish" or "Share")
        {
            return ShareStatusText;
        }

        if (action is "Apply" or "Preview" or "Reset")
        {
            return CommandStatusText;
        }

        if (action.StartsWith("Save", System.StringComparison.Ordinal)
            || action.StartsWith("Export", System.StringComparison.Ordinal))
        {
            return SaveStatusText;
        }

        if (action.StartsWith("#", System.StringComparison.Ordinal))
        {
            return ColorStatusText;
        }

        return SaveStatusText;
    }

    private static void SetStatus(TextBlock target, string text)
    {
        target.Text = text;
    }
}