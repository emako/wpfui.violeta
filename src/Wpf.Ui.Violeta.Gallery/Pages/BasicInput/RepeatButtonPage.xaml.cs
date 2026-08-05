using System.Windows;
using System.Windows.Controls;

namespace Wpf.Ui.Violeta.Gallery.Pages.BasicInput;

public partial class RepeatButtonPage : Wpf.Ui.Violeta.Controls.Page
{
    private int _clickCount;
    private int _fastClickCount;
    private int _slowClickCount;
    private int _counter;
    private int _iconCounter;

    public RepeatButtonPage()
    {
        InitializeComponent();
    }

    private void StandardRepeatButton_OnClick(object sender, RoutedEventArgs e)
    {
        _clickCount++;
        ClickCountText.Text = $"点击次数：{_clickCount}";
    }

    private void DisableRepeatButton_Checked(object sender, RoutedEventArgs e) =>
        StandardRepeatButton.IsEnabled = false;

    private void DisableRepeatButton_Unchecked(object sender, RoutedEventArgs e) =>
        StandardRepeatButton.IsEnabled = true;

    private void FastRepeatButton_OnClick(object sender, RoutedEventArgs e)
    {
        _fastClickCount++;
        UpdateTimingText();
    }

    private void SlowRepeatButton_OnClick(object sender, RoutedEventArgs e)
    {
        _slowClickCount++;
        UpdateTimingText();
    }

    private void UpdateTimingText() =>
        TimingClickCountText.Text = $"快速：{_fastClickCount}　缓慢：{_slowClickCount}";

    private void IncrementButton_OnClick(object sender, RoutedEventArgs e)
    {
        _counter++;
        CounterText.Text = _counter.ToString();
    }

    private void DecrementButton_OnClick(object sender, RoutedEventArgs e)
    {
        _counter--;
        CounterText.Text = _counter.ToString();
    }

    private void ResetCounter_OnClick(object sender, RoutedEventArgs e)
    {
        _counter = 0;
        CounterText.Text = "0";
    }

    private void IconIncrement_OnClick(object sender, RoutedEventArgs e)
    {
        _iconCounter++;
        IconCounterText.Text = _iconCounter.ToString();
    }

    private void IconDecrement_OnClick(object sender, RoutedEventArgs e)
    {
        _iconCounter--;
        IconCounterText.Text = _iconCounter.ToString();
    }
}
