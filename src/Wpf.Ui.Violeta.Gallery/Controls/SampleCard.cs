using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Gallery.Controls;

/// <summary>
/// Clickable overview card that navigates via <see cref="GalleryNavigator"/>.
/// </summary>
public sealed class SampleCard : Border
{
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(SampleCard),
            new PropertyMetadata(string.Empty, OnContentChanged));

    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.Register(nameof(Description), typeof(string), typeof(SampleCard),
            new PropertyMetadata(string.Empty, OnContentChanged));

    public static readonly DependencyProperty GlyphProperty =
        DependencyProperty.Register(nameof(Glyph), typeof(string), typeof(SampleCard),
            new PropertyMetadata("\uE8A5", OnContentChanged));

    public static readonly DependencyProperty NavigateTagProperty =
        DependencyProperty.Register(nameof(NavigateTag), typeof(string), typeof(SampleCard),
            new PropertyMetadata(string.Empty));

    private readonly TextBlock _titleBlock = new() { FontSize = 15, FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 10, 0, 0) };
    private readonly TextBlock _descBlock = new() { Margin = new Thickness(0, 4, 0, 0), TextWrapping = TextWrapping.Wrap };
    private readonly TextBlock _iconBlock = new() { FontSize = 28, HorizontalAlignment = HorizontalAlignment.Left };

    public SampleCard()
    {
        Margin = new Thickness(0, 0, 12, 12);
        Padding = new Thickness(20);
        CornerRadius = new CornerRadius(10);
        Cursor = Cursors.Hand;
        BorderThickness = new Thickness(1);
        SetResourceReference(BackgroundProperty, "ControlFillColorSecondaryBrush");
        SetResourceReference(BorderBrushProperty, "CardStrokeColorDefaultBrush");
        _descBlock.SetResourceReference(TextBlock.ForegroundProperty, "TextFillColorSecondaryBrush");

        Child = new StackPanel
        {
            Children = { _iconBlock, _titleBlock, _descBlock }
        };

        MouseLeftButtonUp += OnMouseLeftButtonUp;
        Loaded += (_, _) =>
        {
            if (TryFindResource("SymbolThemeFontFamily") is FontFamily font)
            {
                _iconBlock.FontFamily = font;
            }

            Refresh();
        };
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public string Glyph
    {
        get => (string)GetValue(GlyphProperty);
        set => SetValue(GlyphProperty, value);
    }

    public string NavigateTag
    {
        get => (string)GetValue(NavigateTagProperty);
        set => SetValue(NavigateTagProperty, value);
    }

    private static void OnContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SampleCard card)
        {
            card.Refresh();
        }
    }

    private void Refresh()
    {
        _titleBlock.Text = Title;
        _descBlock.Text = Description;
        _iconBlock.Text = Glyph;
    }

    private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(NavigateTag))
        {
            GalleryNavigator.Navigate(NavigateTag);
        }
    }
}
