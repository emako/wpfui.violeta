using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Wpf.Ui.Violeta.Converters;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Presents a preview color with optional accent colors.
/// </summary>
[TemplatePart(Name = "PART_AccentDecrement1Border", Type = typeof(Border))]
[TemplatePart(Name = "PART_AccentDecrement2Border", Type = typeof(Border))]
[TemplatePart(Name = "PART_AccentIncrement1Border", Type = typeof(Border))]
[TemplatePart(Name = "PART_AccentIncrement2Border", Type = typeof(Border))]
public partial class ColorPreviewer : Control
{
    public event EventHandler<ColorChangedEventArgs>? ColorChanged;

    private bool _eventsConnected;
    private Border? _accentDecrement1Border;
    private Border? _accentDecrement2Border;
    private Border? _accentIncrement1Border;
    private Border? _accentIncrement2Border;

    static ColorPreviewer()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ColorPreviewer),
            new FrameworkPropertyMetadata(typeof(ColorPreviewer)));
    }

    private void ConnectEvents(bool connected)
    {
        if (connected && !_eventsConnected)
        {
            _accentDecrement1Border?.MouseLeftButtonDown += AccentBorder_PointerPressed;
            _accentDecrement2Border?.MouseLeftButtonDown += AccentBorder_PointerPressed;
            _accentIncrement1Border?.MouseLeftButtonDown += AccentBorder_PointerPressed;
            _accentIncrement2Border?.MouseLeftButtonDown += AccentBorder_PointerPressed;
            _eventsConnected = true;
        }
        else if (!connected && _eventsConnected)
        {
            _accentDecrement1Border?.MouseLeftButtonDown -= AccentBorder_PointerPressed;
            _accentDecrement2Border?.MouseLeftButtonDown -= AccentBorder_PointerPressed;
            _accentIncrement1Border?.MouseLeftButtonDown -= AccentBorder_PointerPressed;
            _accentIncrement2Border?.MouseLeftButtonDown -= AccentBorder_PointerPressed;
            _eventsConnected = false;
        }
    }

    public override void OnApplyTemplate()
    {
        ConnectEvents(false);

        _accentDecrement1Border = GetTemplateChild("PART_AccentDecrement1Border") as Border;
        _accentDecrement2Border = GetTemplateChild("PART_AccentDecrement2Border") as Border;
        _accentIncrement1Border = GetTemplateChild("PART_AccentIncrement1Border") as Border;
        _accentIncrement2Border = GetTemplateChild("PART_AccentIncrement2Border") as Border;

        ConnectEvents(true);
        base.OnApplyTemplate();
    }

    protected virtual void OnColorChanged(ColorChangedEventArgs e) => ColorChanged?.Invoke(this, e);

    private void AccentBorder_PointerPressed(object sender, MouseButtonEventArgs e)
    {
        Border? border = sender as Border;
        int accentStep = 0;
        HsvColor hsvColor = HsvColor;

        try
        {
            accentStep = int.Parse(border?.Tag?.ToString() ?? "0", CultureInfo.InvariantCulture);
        }
        catch
        {
            // ignore
        }

        if (accentStep != 0)
            SetCurrentValue(HsvColorProperty, AccentColorConverter.GetAccent(hsvColor, accentStep));
    }
}
