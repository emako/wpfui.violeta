using System.Globalization;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Controls;

public class Loading : Control
{
    public bool IsRunning
    {
        get => (bool)GetValue(IsRunningProperty);
        set => SetValue(IsRunningProperty, value);
    }

    public static readonly DependencyProperty IsRunningProperty =
        DependencyProperty.Register(nameof(IsRunning), typeof(bool), typeof(Loading), new PropertyMetadata(true));

    public LoadingStyle LoadingStyle
    {
        get => (LoadingStyle)GetValue(LoadingStyleProperty);
        set => SetValue(LoadingStyleProperty, value);
    }

    public static readonly DependencyProperty LoadingStyleProperty =
        DependencyProperty.Register(nameof(LoadingStyle), typeof(LoadingStyle), typeof(Loading), new PropertyMetadata(LoadingStyle.Ring));

    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register(nameof(Minimum), typeof(double), typeof(Loading), new PropertyMetadata(0d));

    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(nameof(Maximum), typeof(double), typeof(Loading), new PropertyMetadata(100d));

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(double), typeof(Loading), new PropertyMetadata(0d));

    static Loading()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Loading), new FrameworkPropertyMetadata(typeof(Loading)));
    }

    public Loading()
    {
    }
}

public enum LoadingStyle
{
    Ring,
}

internal sealed class LoadingBackgroundThicknessConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var actualWidth = (value as double?).GetValueOrDefault();
        return actualWidth == 0 ? 0 : (object)Math.Ceiling(actualWidth / 15);
    }

    public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
    {
        return DependencyProperty.UnsetValue;
    }
}

internal sealed class LoadingLineYConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var actualWidth = (value as double?).GetValueOrDefault();
        if (actualWidth == 0)
            return 0;
        return Math.Ceiling(actualWidth / 4);
    }

    public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
    {
        return new object[] { DependencyProperty.UnsetValue, DependencyProperty.UnsetValue };
    }
}

internal sealed class LoadingClassicMarginConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var actualWidth = (value as double?).GetValueOrDefault();
        if (actualWidth == 0)
            return 0;
        return new Thickness(actualWidth / 2, actualWidth / 2, 0, 0);
    }

    public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
    {
        return DependencyProperty.UnsetValue;
    }
}

internal sealed class LoadingClassicEllipseSizeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var actualWidth = (value as double?).GetValueOrDefault();
        if (actualWidth == 0)
            return 0;
        return Math.Ceiling(actualWidth / 8);
    }

    public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
    {
        return DependencyProperty.UnsetValue;
    }
}

internal sealed class RingProgressBarConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        var width = (double)values[0];
        var height = (double)values[1];

        double radius = 0;
        if (values[2] is Thickness)
            radius = ((Thickness)values[2]).Left;
        else if (values[2] is double)
            radius = ((double)values[2]);

        var percent = 0.33;
        if (values.Length == 6)
        {
            var min = (double)values[3];
            var max = (double)values[4];
            var value = (double)values[5];
            value = value > max ? max : value;
            value = value < min ? min : value;
            percent = (value - min) / (max - min);
        }

        var point1X = height / 2 * Math.Cos((2 * percent - 0.5) * Math.PI) + height / 2;
        var point1Y = height / 2 - height / 2 * Math.Sin((2 * percent + 0.5) * Math.PI);
        var point2X = (height - radius) / 2 * Math.Cos((2 * percent - 0.5) * Math.PI) + height / 2;
        var point2Y = height / 2 - (height - radius) / 2 * Math.Sin((2 * percent + 0.5) * Math.PI);

        var path = "";

        // Use invariant string formatting to prevent exceptions on non-english systems
        // https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/tokens/interpolated#culture-specific-formatting
        if (percent == 0)
        {
            path = "";
        }
        else if (percent < 0.5)
        {
            FormattableString.Invariant($"M {width / 2},{radius / 2} A {(width - radius) / 2},{(width - radius) / 2} 0 0 1 {point2X},{point2Y}");
        }
        else if (percent == 0.5)
        {
            FormattableString.Invariant($"M {width / 2},{radius / 2} A {(width - radius) / 2},{(width - radius) / 2} 0 0 1 {width / 2},{(height - radius / 2)}");
        }
        else
        {
            FormattableString.Invariant($"M {width / 2},{radius / 2} A {(width - radius) / 2},{(width - radius) / 2} 0 0 1 {width / 2},{(height - radius / 2)} A {(width - radius) / 2},{(width - radius) / 2} 0 0 1 {point2X},{point2Y}");
        }

        return PathGeometry.Parse(path);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        return new object[] { DependencyProperty.UnsetValue, DependencyProperty.UnsetValue };
    }
}