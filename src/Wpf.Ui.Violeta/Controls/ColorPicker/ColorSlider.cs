using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Wpf.Ui.Violeta.Controls.ColorPickerHelpers;
using Helpers = Wpf.Ui.Violeta.Controls.ColorPickerHelpers.ColorPickerHelpers;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// A slider with a background that represents a single color component.
/// </summary>
public partial class ColorSlider : Slider
{
    public event EventHandler<ColorChangedEventArgs>? ColorChanged;

    private const double MaxHueComponent = 359;
    protected bool _ignorePropertyChanged;
    private WriteableBitmap? _backgroundBitmap;

    static ColorSlider()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ColorSlider),
            new FrameworkPropertyMetadata(typeof(ColorSlider)));
    }

    public ColorSlider()
    {
        SizeChanged += (_, _) =>
        {
            _backgroundBitmap = null;
            UpdateBackground();
            UpdateSelectorStates();
        };
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        SetColorToSliderValues();
        UpdateBackground();
        UpdateSelectorStates();
    }

    private void UpdateSelectorStates()
    {
        if (Color.A < 128 && (IsAlphaVisible || ColorComponent == ColorComponent.Alpha))
        {
            SetValue(IsDarkSelectorPropertyKey, false);
            SetValue(IsLightSelectorPropertyKey, false);
        }
        else
        {
            Color perceivedColor = ColorModel == ColorModel.Hsva
                ? GetPerceptiveBackgroundColor(HsvColor).ToRgb()
                : GetPerceptiveBackgroundColor(Color);

            if (ColorHelper.GetRelativeLuminance(perceivedColor) <= 0.5)
            {
                SetValue(IsDarkSelectorPropertyKey, false);
                SetValue(IsLightSelectorPropertyKey, true);
            }
            else
            {
                SetValue(IsDarkSelectorPropertyKey, true);
                SetValue(IsLightSelectorPropertyKey, false);
            }
        }
    }

    private async void UpdateBackground()
    {
        double scale = VisualTreeHelper.GetDpi(this).PixelsPerDip;
        int pixelWidth;
        int pixelHeight;

        if (GetTemplateChild("PART_Track") is Track track && track.ActualWidth > 0 && track.ActualHeight > 0)
        {
            pixelWidth = Convert.ToInt32(track.ActualWidth * scale);
            pixelHeight = Convert.ToInt32(track.ActualHeight * scale);
        }
        else
        {
            pixelWidth = Convert.ToInt32(ActualWidth * scale);
            pixelHeight = Convert.ToInt32(ActualHeight * scale);
        }

        if (pixelWidth != 0 && pixelHeight != 0)
        {
            var bgraPixelData = new byte[pixelWidth * pixelHeight * 4];
            await Helpers.CreateComponentBitmapAsync(
                bgraPixelData,
                pixelWidth,
                pixelHeight,
                Orientation,
                ColorModel,
                ColorComponent,
                HsvColor,
                IsAlphaVisible,
                IsPerceptive);

            _backgroundBitmap = Helpers.CreateBitmapFromPixelData(bgraPixelData, pixelWidth, pixelHeight);
            Background = new ImageBrush(_backgroundBitmap);
        }
    }

    private static HsvColor RoundComponentValues(HsvColor hsvColor) =>
        new(
            Math.Round(hsvColor.A, 2, MidpointRounding.AwayFromZero),
            Math.Round(hsvColor.H, 0, MidpointRounding.AwayFromZero),
            Math.Round(hsvColor.S, 2, MidpointRounding.AwayFromZero),
            Math.Round(hsvColor.V, 2, MidpointRounding.AwayFromZero));

    private void SetColorToSliderValues()
    {
        var component = ColorComponent;

        if (ColorModel == ColorModel.Hsva)
        {
            var hsvColor = HsvColor;
            if (IsRoundingEnabled)
                hsvColor = RoundComponentValues(hsvColor);

            switch (component)
            {
                case ColorComponent.Alpha:
                    Minimum = 0; Maximum = 100; Value = hsvColor.A * 100; break;
                case ColorComponent.Component1:
                    Minimum = 0; Maximum = MaxHueComponent; Value = hsvColor.H; break;
                case ColorComponent.Component2:
                    Minimum = 0; Maximum = 100; Value = hsvColor.S * 100; break;
                case ColorComponent.Component3:
                    Minimum = 0; Maximum = 100; Value = hsvColor.V * 100; break;
            }
        }
        else
        {
            var rgbColor = Color;
            switch (component)
            {
                case ColorComponent.Alpha:
                    Minimum = 0; Maximum = 255; Value = Convert.ToDouble(rgbColor.A); break;
                case ColorComponent.Component1:
                    Minimum = 0; Maximum = 255; Value = Convert.ToDouble(rgbColor.R); break;
                case ColorComponent.Component2:
                    Minimum = 0; Maximum = 255; Value = Convert.ToDouble(rgbColor.G); break;
                case ColorComponent.Component3:
                    Minimum = 0; Maximum = 255; Value = Convert.ToDouble(rgbColor.B); break;
            }
        }
    }

    private (Color, HsvColor) GetColorFromSliderValues()
    {
        HsvColor hsvColor = default;
        Color rgbColor = default;
        double range = Maximum - Minimum;
        double sliderPercent = range == 0 ? 0 : Value / range;
        var component = ColorComponent;

        if (ColorModel == ColorModel.Hsva)
        {
            var baseHsvColor = HsvColor;
            switch (component)
            {
                case ColorComponent.Alpha:
                    hsvColor = new HsvColor(sliderPercent, baseHsvColor.H, baseHsvColor.S, baseHsvColor.V);
                    break;
                case ColorComponent.Component1:
                    hsvColor = new HsvColor(baseHsvColor.A, sliderPercent * MaxHueComponent, baseHsvColor.S, baseHsvColor.V);
                    break;
                case ColorComponent.Component2:
                    hsvColor = new HsvColor(baseHsvColor.A, baseHsvColor.H, sliderPercent, baseHsvColor.V);
                    break;
                case ColorComponent.Component3:
                    hsvColor = new HsvColor(baseHsvColor.A, baseHsvColor.H, baseHsvColor.S, sliderPercent);
                    break;
            }
            rgbColor = hsvColor.ToRgb();
        }
        else
        {
            var baseRgbColor = Color;
            byte componentValue = Convert.ToByte(Clamp(sliderPercent * 255, 0, 255));
            switch (component)
            {
                case ColorComponent.Alpha:
                    rgbColor = Color.FromArgb(componentValue, baseRgbColor.R, baseRgbColor.G, baseRgbColor.B);
                    break;
                case ColorComponent.Component1:
                    rgbColor = Color.FromArgb(baseRgbColor.A, componentValue, baseRgbColor.G, baseRgbColor.B);
                    break;
                case ColorComponent.Component2:
                    rgbColor = Color.FromArgb(baseRgbColor.A, baseRgbColor.R, componentValue, baseRgbColor.B);
                    break;
                case ColorComponent.Component3:
                    rgbColor = Color.FromArgb(baseRgbColor.A, baseRgbColor.R, baseRgbColor.G, componentValue);
                    break;
            }
            hsvColor = rgbColor.ToHsv();
        }

        if (IsRoundingEnabled)
            hsvColor = RoundComponentValues(hsvColor);

        return (rgbColor, hsvColor);
    }

    private HsvColor GetPerceptiveBackgroundColor(HsvColor hsvColor)
    {
        if (!IsAlphaVisible && ColorComponent != ColorComponent.Alpha)
            hsvColor = new HsvColor(1.0, hsvColor.H, hsvColor.S, hsvColor.V);

        if (!IsPerceptive)
            return hsvColor;

        return ColorComponent switch
        {
            ColorComponent.Component1 => new HsvColor(hsvColor.A, hsvColor.H, 1.0, 1.0),
            ColorComponent.Component2 => new HsvColor(hsvColor.A, hsvColor.H, hsvColor.S, 1.0),
            ColorComponent.Component3 => new HsvColor(hsvColor.A, hsvColor.H, 1.0, hsvColor.V),
            _ => hsvColor,
        };
    }

    private Color GetPerceptiveBackgroundColor(Color rgbColor)
    {
        if (!IsAlphaVisible && ColorComponent != ColorComponent.Alpha)
            rgbColor = Color.FromArgb(255, rgbColor.R, rgbColor.G, rgbColor.B);

        if (!IsPerceptive)
            return rgbColor;

        return ColorComponent switch
        {
            ColorComponent.Component1 => Color.FromArgb(rgbColor.A, rgbColor.R, 0, 0),
            ColorComponent.Component2 => Color.FromArgb(rgbColor.A, 0, rgbColor.G, 0),
            ColorComponent.Component3 => Color.FromArgb(rgbColor.A, 0, 0, rgbColor.B),
            _ => rgbColor,
        };
    }

    internal void OnColorPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        if (_ignorePropertyChanged)
            return;

        if (e.Property == ColorProperty)
        {
            _ignorePropertyChanged = true;
            SetCurrentValue(HsvColorProperty, Color.ToHsv());
            SetColorToSliderValues();
            UpdateBackground();
            UpdateSelectorStates();
            OnColorChanged(new ColorChangedEventArgs((Color)e.OldValue, (Color)e.NewValue));
            _ignorePropertyChanged = false;
        }
        else
        {
            _ignorePropertyChanged = true;
            SetColorToSliderValues();
            UpdateBackground();
            UpdateSelectorStates();
            _ignorePropertyChanged = false;
        }
    }

    internal void OnHsvColorPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        if (_ignorePropertyChanged)
            return;

        _ignorePropertyChanged = true;
        SetCurrentValue(ColorProperty, HsvColor.ToRgb());
        SetColorToSliderValues();
        UpdateBackground();
        UpdateSelectorStates();
        OnColorChanged(new ColorChangedEventArgs(
            ((HsvColor)e.OldValue).ToRgb(),
            ((HsvColor)e.NewValue).ToRgb()));
        _ignorePropertyChanged = false;
    }

    protected override void OnValueChanged(double oldValue, double newValue)
    {
        base.OnValueChanged(oldValue, newValue);
        if (_ignorePropertyChanged)
            return;

        _ignorePropertyChanged = true;
        Color oldColor = Color;
        (var color, var hsvColor) = GetColorFromSliderValues();

        if (ColorModel == ColorModel.Hsva)
        {
            SetCurrentValue(HsvColorProperty, hsvColor);
            SetCurrentValue(ColorProperty, hsvColor.ToRgb());
        }
        else
        {
            SetCurrentValue(ColorProperty, color);
            SetCurrentValue(HsvColorProperty, color.ToHsv());
        }

        UpdateSelectorStates();
        OnColorChanged(new ColorChangedEventArgs(oldColor, Color));
        _ignorePropertyChanged = false;
    }

    protected virtual void OnColorChanged(ColorChangedEventArgs e) => ColorChanged?.Invoke(this, e);

    private static double Clamp(double value, double min, double max) =>
        value < min ? min : value > max ? max : value;
}
