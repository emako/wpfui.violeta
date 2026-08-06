using System;
using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Violeta.Controls;
using LiteObservableLanguages;
using Wpf.Ui.Violeta.Gallery.Globalization;

namespace Wpf.Ui.Violeta.Gallery.Pages.Feedback;

public partial class TransitioningContentControlPage : Wpf.Ui.Violeta.Controls.Page
{
    private readonly string[] _slides =
    [
        LangKeys.Sample_05f27e568a.Tr(),
        LangKeys.Sample_db17cc2401.Tr(),
        LangKeys.Sample_ccdd6cd1e7.Tr(),
        LangKeys.Sample_d09f873566.Tr(),
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
