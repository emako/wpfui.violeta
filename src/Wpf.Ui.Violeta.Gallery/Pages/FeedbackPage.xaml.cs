using Page = Wpf.Ui.Violeta.Controls.Compat.Page;

namespace Wpf.Ui.Violeta.Gallery.Pages;

public partial class FeedbackPage : Page
{
    private readonly string[] _slides =
    [
        "Slide 1 — 欢迎使用 Violeta",
        "Slide 2 — Fluent Design 风格",
        "Slide 3 — 多框架支持",
        "Slide 4 — 11 种语言本地化",
    ];

    private int _slideIndex;

    public FeedbackPage()
    {
        InitializeComponent();
    }

    private void NextSlide_Click(object sender, RoutedEventArgs e)
    {
        _slideIndex = (_slideIndex + 1) % _slides.Length;
        UpdateSlide();
    }

    private void PrevSlide_Click(object sender, RoutedEventArgs e)
    {
        _slideIndex = (_slideIndex - 1 + _slides.Length) % _slides.Length;
        UpdateSlide();
    }

    private void UpdateSlide()
    {
        TransitionText.Text = _slides[_slideIndex];
        TransitionContent.Content = TransitionText;
        UpdateStatus();
    }

    private void TransitionType_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (TransitionTypeComboBox.SelectedItem is ComboBoxItem item && item.Tag is string tag)
        {
            if (Enum.TryParse<TransitionType>(tag, out var type))
            {
                TransitionContent.Transition = type;
            }
        }

        UpdateStatus();
    }

    private void UpdateStatus()
    {
        TransitionStatusText.Text = $"Slide {_slideIndex + 1}/{_slides.Length} | {TransitionContent.Transition}";
    }
}
