using System;
using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Violeta.Controls;

namespace Wpf.Ui.Violeta.Gallery.Pages.BasicInput;

public partial class ButtonSpinnerPage : Wpf.Ui.Violeta.Controls.Page
{
    private static readonly string[] _words = ["Apple", "Banana", "Cherry", "Durian", "Elderberry"];
    private int _defaultIndex;
    private int _leftIndex;
    private int _splitRightIndex;
    private int _splitLeftIndex;

    public ButtonSpinnerPage()
    {
        InitializeComponent();
    }

    private void ButtonSpinner_OnSpin(object sender, SpinEventArgs e)
    {
        if (sender is not ButtonSpinner spinner)
        {
            return;
        }

        int delta = e.Direction == SpinDirection.Increase ? 1 : -1;

        if (ReferenceEquals(spinner.Content, ButtonSpinnerDefaultValueText))
        {
            _defaultIndex = WrapIndex(_defaultIndex + delta, _words.Length);
            ButtonSpinnerDefaultValueText.Text = _words[_defaultIndex];
            return;
        }

        if (ReferenceEquals(spinner.Content, ButtonSpinnerLeftValueText))
        {
            _leftIndex = WrapIndex(_leftIndex + delta, _words.Length);
            ButtonSpinnerLeftValueText.Text = _words[_leftIndex];
            return;
        }

        if (ReferenceEquals(spinner.Content, ButtonSpinnerSplitRightValueText))
        {
            _splitRightIndex = WrapIndex(_splitRightIndex + delta, _words.Length);
            ButtonSpinnerSplitRightValueText.Text = _words[_splitRightIndex];
            return;
        }

        if (ReferenceEquals(spinner.Content, ButtonSpinnerSplitLeftValueText))
        {
            _splitLeftIndex = WrapIndex(_splitLeftIndex + delta, _words.Length);
            ButtonSpinnerSplitLeftValueText.Text = _words[_splitLeftIndex];
        }
    }

    private static int WrapIndex(int value, int length)
    {
        return (value % length + length) % length;
    }
}
