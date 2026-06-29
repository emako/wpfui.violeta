using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using Wpf.Ui.Violeta.Controls;

namespace Wpf.Ui.Violeta.Markup;

[Obsolete("Not ready for production")]
public class AcrylicBrushExtension : MarkupExtension
{
    public FrameworkElement? Target { get; set; }

    public string? TargetName { get; set; }

    public double? Amount { get; set; }

    public Color? TintColor { get; set; }

    public double? TintOpacity { get; set; }

    public double? NoiseOpacity { get; set; }

    public AcrylicBrushExtension()
    {
    }

    public AcrylicBrushExtension(string target)
    {
        TargetName = target;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        if (Target == null && !string.IsNullOrWhiteSpace(TargetName))
        {
            if (serviceProvider.GetService(typeof(IProvideValueTarget)) is IProvideValueTarget pvt)
            {
                if (pvt.TargetObject is FrameworkElement ownerElement)
                {
                    Target = ownerElement.FindName(TargetName) as FrameworkElement;
                }
                else if (pvt.TargetObject is FrameworkContentElement ownerContentElement)
                {
                    Target = ownerContentElement.FindName(TargetName) as FrameworkElement;
                }
            }
        }

        if (Target == null)
        {
            if (serviceProvider.GetService(typeof(IProvideValueTarget)) is IProvideValueTarget pvt)
            {
                Target = pvt.TargetObject as FrameworkElement;
            }
        }
        else
        {
            TargetName = Target.Name;
        }

        if (Target is null)
        {
            return new SolidColorBrush(TintColor ?? Colors.Transparent);
        }

        return CreateAcrylicBrushInternal(Target);
    }

    public Brush CreateAcrylicBrush()
    {
        if (Target is null)
        {
            return new SolidColorBrush(TintColor ?? Colors.Transparent);
        }

        TargetName = Target.Name;
        return CreateAcrylicBrushInternal(Target);
    }

    private VisualBrush CreateAcrylicBrushInternal(FrameworkElement target)
    {
        var acrylicPanel = new AcrylicPanel();

        if (Amount != null)
        {
            acrylicPanel.Amount = (double)Amount;
        }

        if (TintColor != null)
        {
            acrylicPanel.TintColor = (Color)TintColor;
        }

        if (TintOpacity != null)
        {
            acrylicPanel.TintOpacity = (double)TintOpacity;
        }

        if (NoiseOpacity != null)
        {
            acrylicPanel.NoiseOpacity = (double)NoiseOpacity;
        }

        acrylicPanel.SetBinding(AcrylicPanel.TargetProperty, new Binding { Source = target });
        acrylicPanel.SetBinding(AcrylicPanel.SourceProperty, new Binding { Source = target });
        acrylicPanel.SetBinding(FrameworkElement.WidthProperty, new Binding("ActualWidth") { Source = target });
        acrylicPanel.SetBinding(FrameworkElement.HeightProperty, new Binding("ActualHeight") { Source = target });

        var brush = new VisualBrush(acrylicPanel)
        {
            Stretch = Stretch.None,
            AlignmentX = AlignmentX.Left,
            AlignmentY = AlignmentY.Top,
            ViewboxUnits = BrushMappingMode.Absolute,
        };

        return brush;
    }
}
