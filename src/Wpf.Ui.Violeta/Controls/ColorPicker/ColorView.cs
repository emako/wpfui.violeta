using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Wpf.Ui.Violeta.Converters;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Presents a color for user editing using a spectrum, palette and component sliders.
/// </summary>
[TemplatePart(Name = "PART_HexTextBox", Type = typeof(TextBox))]
[TemplatePart(Name = "PART_Segmented", Type = typeof(Segmented))]
[TemplatePart(Name = "PART_PalettePanel", Type = typeof(ListBox))]
public partial class ColorView : Control
{
    public event EventHandler<ColorChangedEventArgs>? ColorChanged;

    private TextBox? _hexTextBox;
    private Segmented? _segmented;
    protected bool _ignorePropertyChanged;

    static ColorView()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ColorView),
            new FrameworkPropertyMetadata(typeof(ColorView)));
    }

    private void GetColorFromHexTextBox()
    {
        if (_hexTextBox != null)
        {
            var convertedColor = ColorToHexConverter.ParseHexString(_hexTextBox.Text ?? string.Empty, HexInputAlphaPosition);
            if (convertedColor is Color color)
                SetCurrentValue(ColorProperty, color);
            SetColorToHexTextBox();
        }
    }

    private void SetColorToHexTextBox()
    {
        _hexTextBox?.Text = ColorToHexConverter.ToHexString(
            Color,
            HexInputAlphaPosition,
            includeAlpha: IsAlphaEnabled && IsAlphaVisible,
            includeSymbol: false
        );
    }

    public override void OnApplyTemplate()
    {
        if (_hexTextBox != null)
        {
            _hexTextBox.KeyDown -= HexTextBox_KeyDown;
            _hexTextBox.LostFocus -= HexTextBox_LostFocus;
        }

        if (_segmented != null)
            _segmented.SelectionChanged -= Segmented_SelectionChanged;

        _hexTextBox = GetTemplateChild("PART_HexTextBox") as TextBox;
        _segmented = GetTemplateChild("PART_Segmented") as Segmented;
        SetColorToHexTextBox();
        SyncSegmentedSelection();

        if (_hexTextBox != null)
        {
            _hexTextBox.KeyDown += HexTextBox_KeyDown;
            _hexTextBox.LostFocus += HexTextBox_LostFocus;
        }

        if (_segmented != null)
            _segmented.SelectionChanged += Segmented_SelectionChanged;

        HandlePaletteChanged();

        base.OnApplyTemplate();
    }

    private void Segmented_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_segmented == null || _segmented.SelectedIndex < 0)
            return;

        SetCurrentValue(SelectedIndexProperty, _segmented.SelectedIndex);
    }

    private void SyncSegmentedSelection()
    {
        if (_segmented == null || _segmented.SelectedIndex == SelectedIndex)
            return;

        _segmented.SelectedIndex = SelectedIndex;
    }

    internal void HandleColorChanged(Color oldColor, Color newColor)
    {
        if (_ignorePropertyChanged)
            return;

        _ignorePropertyChanged = true;
        SetCurrentValue(HsvColorProperty, newColor.ToHsv());
        SetColorToHexTextBox();
        OnColorChanged(new ColorChangedEventArgs(oldColor, newColor));
        _ignorePropertyChanged = false;
    }

    internal void HandleHsvColorChanged(HsvColor oldHsv, HsvColor newHsv)
    {
        if (_ignorePropertyChanged)
            return;

        _ignorePropertyChanged = true;
        SetCurrentValue(ColorProperty, newHsv.ToRgb());
        SetColorToHexTextBox();
        OnColorChanged(new ColorChangedEventArgs(oldHsv.ToRgb(), newHsv.ToRgb()));
        _ignorePropertyChanged = false;
    }

    internal void HandlePaletteChanged()
    {
        IColorPalette? palette = Palette;
        if (palette == null)
            return;

        SetCurrentValue(PaletteColumnCountProperty, palette.ColorCount);
        SetCurrentValue(PaletteRowCountProperty, palette.ShadeCount);

        var newPaletteColors = new List<Color>(palette.ColorCount * palette.ShadeCount);
        // WinUI / WCT order: each row is a shade, each column is a hue (left-to-right, top-to-bottom).
        for (int shadeIndex = 0; shadeIndex < palette.ShadeCount; shadeIndex++)
        {
            for (int colorIndex = 0; colorIndex < palette.ColorCount; colorIndex++)
                newPaletteColors.Add(palette.GetColor(colorIndex, shadeIndex));
        }

        SetCurrentValue(PaletteColorsProperty, newPaletteColors);
    }

    protected virtual void OnColorChanged(ColorChangedEventArgs e) => ColorChanged?.Invoke(this, e);

    protected virtual Color OnCoerceColor(Color value)
    {
        if (!IsAlphaEnabled)
            return Color.FromArgb(255, value.R, value.G, value.B);
        return value;
    }

    protected virtual HsvColor OnCoerceHsvColor(HsvColor value)
    {
        if (!IsAlphaEnabled)
            return new HsvColor(1.0, value.H, value.S, value.V);
        return value;
    }

    private void HexTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            GetColorFromHexTextBox();
    }

    private void HexTextBox_LostFocus(object sender, RoutedEventArgs e) => GetColorFromHexTextBox();
}
