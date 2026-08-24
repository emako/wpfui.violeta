using System;
using System.Windows;

namespace Wpf.Ui.Violeta.Gallery.Pages.Text;

public partial class NumberDisplayerPage : Wpf.Ui.Violeta.Controls.Page
{
    public static readonly DependencyProperty IntValueProperty =
        DependencyProperty.Register(nameof(IntValue), typeof(int), typeof(NumberDisplayerPage), new PropertyMetadata(0));

    public static readonly DependencyProperty LongValueProperty =
        DependencyProperty.Register(nameof(LongValue), typeof(long), typeof(NumberDisplayerPage), new PropertyMetadata(0L));

    public static readonly DependencyProperty DoubleValueProperty =
        DependencyProperty.Register(nameof(DoubleValue), typeof(double), typeof(NumberDisplayerPage), new PropertyMetadata(0d));

    public static readonly DependencyProperty DateValueProperty =
        DependencyProperty.Register(nameof(DateValue), typeof(System.DateTime), typeof(NumberDisplayerPage), new PropertyMetadata(System.DateTime.Now));

    public int IntValue
    {
        get => (int)GetValue(IntValueProperty);
        set => SetValue(IntValueProperty, value);
    }

    public long LongValue
    {
        get => (long)GetValue(LongValueProperty);
        set => SetValue(LongValueProperty, value);
    }

    public double DoubleValue
    {
        get => (double)GetValue(DoubleValueProperty);
        set => SetValue(DoubleValueProperty, value);
    }

    public System.DateTime DateValue
    {
        get => (System.DateTime)GetValue(DateValueProperty);
        set => SetValue(DateValueProperty, value);
    }

    public NumberDisplayerPage()
    {
        DataContext = this;
        InitializeComponent();
    }

    private void Change_Click(object sender, RoutedEventArgs e)
    {
        var r = Random.Shared;
        IntValue = r.Next(int.MaxValue);
        LongValue = ((long)r.Next(int.MaxValue)) * 1000 + r.Next(1000);
        DoubleValue = r.NextDouble() * 100000;
        DateValue = System.DateTime.Today.AddDays(r.Next(1000));
    }
}
