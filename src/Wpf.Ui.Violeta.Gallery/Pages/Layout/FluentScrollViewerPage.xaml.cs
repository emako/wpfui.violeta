using LiteObservableLanguages;
using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Violeta.Gallery.Globalization;

namespace Wpf.Ui.Violeta.Gallery.Pages.Layout;

public partial class FluentScrollViewerPage : Wpf.Ui.Violeta.Controls.Page
{
    public FluentScrollViewerPage()
    {
        InitializeComponent();
        PopulateSamples();
    }

    private void PopulateSamples()
    {
        for (int i = 1; i <= 40; i++)
        {
            VerticalContent.Children.Add(new TextBlock
            {
                Text = LangKeys.Format_ScrollContentLine.Tr(i),
                Margin = new Thickness(0, 0, 0, 8),
            });
        }

        for (int i = 1; i <= 24; i++)
        {
            HorizontalContent.Children.Add(CreateCard(
                LangKeys.Format_ScrollContentColumn.Tr(i),
                minWidth: 168,
                margin: new Thickness(0, 0, 12, 0),
                padding: new Thickness(16, 20, 16, 20),
                cornerRadius: 8,
                wrap: true));
        }

        for (int row = 1; row <= 24; row++)
        {
            var rowPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 8),
            };

            for (int column = 1; column <= 16; column++)
            {
                rowPanel.Children.Add(CreateCard(
                    LangKeys.Format_Item.Tr($"{row}-{column}"),
                    minWidth: 96,
                    margin: new Thickness(0, 0, 8, 0),
                    padding: new Thickness(12, 8, 12, 8),
                    cornerRadius: 6,
                    wrap: false));
            }

            BothContent.Children.Add(rowPanel);
        }
    }

    private static Border CreateCard(
        string text,
        double minWidth,
        Thickness margin,
        Thickness padding,
        double cornerRadius,
        bool wrap)
    {
        var border = new Border
        {
            MinWidth = minWidth,
            Margin = margin,
            Padding = padding,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(cornerRadius),
            Child = new TextBlock
            {
                Text = text,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextWrapping = wrap ? TextWrapping.Wrap : TextWrapping.NoWrap,
            },
        };
        border.SetResourceReference(Border.BackgroundProperty, "ControlFillColorDefaultBrush");
        border.SetResourceReference(Border.BorderBrushProperty, "CardStrokeColorDefaultBrush");
        return border;
    }
}
