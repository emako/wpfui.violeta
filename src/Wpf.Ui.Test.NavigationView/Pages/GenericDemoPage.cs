using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Wpf.Ui.Test.NavigationView.Pages;

public class GenericDemoPage : Page
{
    public GenericDemoPage(string title, string tag)
    {
        var root = new Grid
        {
            Margin = new Thickness(20)
        };

        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(12) });
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        var titleBlock = new TextBlock
        {
            FontSize = 26,
            FontWeight = FontWeights.SemiBold,
            Text = string.IsNullOrWhiteSpace(title) ? "Demo Page" : $"{title} Page"
        };

        var card = new Border
        {
            Background = (Brush?)Application.Current.TryFindResource("ControlFillColorSecondaryBrush") ?? Brushes.Transparent,
            BorderBrush = (Brush?)Application.Current.TryFindResource("CardStrokeColorDefaultBrush") ?? Brushes.Gray,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(16)
        };

        Grid.SetRow(card, 2);

        var stack = new StackPanel();
        stack.Children.Add(new TextBlock
        {
            FontWeight = FontWeights.SemiBold,
            Text = "通用页面"
        });
        stack.Children.Add(new TextBlock
        {
            Margin = new Thickness(0, 10, 0, 0),
            Text = $"当前 Tag: {tag}",
            TextWrapping = TextWrapping.Wrap
        });

        card.Child = stack;
        root.Children.Add(titleBlock);
        root.Children.Add(card);

        Content = root;
    }
}
