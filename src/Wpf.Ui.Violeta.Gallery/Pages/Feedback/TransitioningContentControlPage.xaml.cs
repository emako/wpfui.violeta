using System;
using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Violeta.Controls;

namespace Wpf.Ui.Violeta.Gallery.Pages.Feedback;

public partial class TransitioningContentControlPage : Wpf.Ui.Violeta.Controls.Page
{
    private readonly string[] _slides =
    [
        "Slide 1 — 欢迎使用 Violeta",
        "Slide 2 — Fluent Design 风格",
        "Slide 3 — 多框架支持",
        "Slide 4 — 11 种语言本地化",
    ];

    private int _slideIndex;

    public TransitioningContentControlPage()
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
        // Content 必须换成新的引用，OnContentChanged 才会触发过渡动画。
        // 复用同一个 UIElement 只改内部属性时，Content 引用不变，动画不会播放。
        TransitionContent.Content = _slides[_slideIndex];
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
        if (TransitionStatusText is null || TransitionContent is null)
        {
            return;
        }

        TransitionStatusText.Text = $"Slide {_slideIndex + 1}/{_slides.Length} | {TransitionContent.Transition}";
    }
}
