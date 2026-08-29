using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using Wpf.Ui.Violeta.Appearance;

namespace Wpf.Ui.Violeta.Gallery.Pages.Design;

public partial class ThemeColorApproachPage : Wpf.Ui.Violeta.Controls.Page, INotifyPropertyChanged
{
    private Color _lightColor = Color.FromRgb(0x00, 0x78, 0xD4);
    private double _baseL = ThemeColorApproach.DefaultDarkBaseL;

    public ThemeColorApproachPage()
    {
        DataContext = this;
        InitializeComponent();
        RefreshDerived();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public Color LightColor
    {
        get => _lightColor;
        set
        {
            if (_lightColor == value)
            {
                return;
            }

            _lightColor = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(LightHex));
            RefreshDerived();
        }
    }

    public double BaseL
    {
        get => _baseL;
        set
        {
            double clamped = Math.Max(0.0, Math.Min(99.9, value));
            if (Math.Abs(_baseL - clamped) < 0.0001)
            {
                return;
            }

            _baseL = clamped;
            OnPropertyChanged();
            RefreshDerived();
        }
    }

    public Color DarkColor { get; private set; }

    public string LightHex => FormatHex(LightColor);

    public string DarkHex => FormatHex(DarkColor);

    public string IsLightText { get; private set; } = string.Empty;

    public Color AccentLight3 { get; private set; }
    public Color AccentLight2 { get; private set; }
    public Color AccentLight1 { get; private set; }
    public Color AccentBase { get; private set; }
    public Color AccentDark1 { get; private set; }
    public Color AccentDark2 { get; private set; }
    public Color AccentDark3 { get; private set; }

    public Color PrimaryAccentLight { get; private set; }

    public Color PrimaryAccentDark { get; private set; }

    private void RefreshDerived()
    {
        DarkColor = ThemeColorApproach.ToDark(LightColor, BaseL);
        IsLightText = ThemeColorApproach.IsColorLight(LightColor) ? "true" : "false";

        AccentColorPalette palette = ThemeColorApproach.CreateAccentPalette(LightColor);
        AccentLight3 = palette.Light3;
        AccentLight2 = palette.Light2;
        AccentLight1 = palette.Light1;
        AccentBase = palette.Accent;
        AccentDark1 = palette.Dark1;
        AccentDark2 = palette.Dark2;
        AccentDark3 = palette.Dark3;

        PrimaryAccentLight = ThemeColorApproach.GetPrimaryAccent(LightColor, isDarkTheme: false);
        PrimaryAccentDark = ThemeColorApproach.GetPrimaryAccent(LightColor, isDarkTheme: true);

        OnPropertyChanged(nameof(DarkColor));
        OnPropertyChanged(nameof(DarkHex));
        OnPropertyChanged(nameof(IsLightText));
        OnPropertyChanged(nameof(AccentLight3));
        OnPropertyChanged(nameof(AccentLight2));
        OnPropertyChanged(nameof(AccentLight1));
        OnPropertyChanged(nameof(AccentBase));
        OnPropertyChanged(nameof(AccentDark1));
        OnPropertyChanged(nameof(AccentDark2));
        OnPropertyChanged(nameof(AccentDark3));
        OnPropertyChanged(nameof(PrimaryAccentLight));
        OnPropertyChanged(nameof(PrimaryAccentDark));
    }

    private static string FormatHex(Color color) =>
        $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
