using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Horizontal meter showing how a total is occupied by several categories.
/// </summary>
[TemplatePart(Name = PartBar, Type = typeof(FrameworkElement))]
[TemplatePart(Name = PartLegend, Type = typeof(ItemsControl))]
[StyleTypedProperty(Property = nameof(ItemContainerStyle), StyleTargetType = typeof(MeterBarItem))]
public class MeterBar : ItemsControl
{
    private const string PartBar = "PART_Bar";
    private const string PartLegend = "PART_Legend";

    private static readonly string[] PaletteKeys =
    [
        "MeterBarPalette1",
        "MeterBarPalette2",
        "MeterBarPalette3",
        "MeterBarPalette4",
        "MeterBarPalette5",
        "MeterBarPalette6",
    ];

    private readonly ObservableCollection<MeterBarItem> _legendItems = [];
    private FrameworkElement? _bar;

    static MeterBar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(MeterBar),
            new FrameworkPropertyMetadata(typeof(MeterBar)));

        FocusableProperty.OverrideMetadata(
            typeof(MeterBar),
            new FrameworkPropertyMetadata(false));

        IsTabStopProperty.OverrideMetadata(
            typeof(MeterBar),
            new FrameworkPropertyMetadata(false));
    }

    #region Total

    public static readonly DependencyProperty TotalProperty =
        DependencyProperty.Register(
            nameof(Total),
            typeof(double),
            typeof(MeterBar),
            new PropertyMetadata(100d));

    public double Total
    {
        get => (double)GetValue(TotalProperty);
        set => SetValue(TotalProperty, value);
    }

    #endregion Total

    #region Legend

    public static readonly DependencyProperty LegendProperty =
        DependencyProperty.Register(
            nameof(Legend),
            typeof(MeterLegend),
            typeof(MeterBar),
            new PropertyMetadata(MeterLegend.Horizontal));

    public MeterLegend Legend
    {
        get => (MeterLegend)GetValue(LegendProperty);
        set => SetValue(LegendProperty, value);
    }

    #endregion Legend

    #region BarHeight

    public static readonly DependencyProperty BarHeightProperty =
        DependencyProperty.Register(
            nameof(BarHeight),
            typeof(double),
            typeof(MeterBar),
            new PropertyMetadata(10d));

    public double BarHeight
    {
        get => (double)GetValue(BarHeightProperty);
        set => SetValue(BarHeightProperty, value);
    }

    #endregion BarHeight

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (_bar is not null)
        {
            _bar.SizeChanged -= OnBarSizeChanged;
        }

        _bar = GetTemplateChild(PartBar) as FrameworkElement;
        if (_bar is not null)
        {
            _bar.SizeChanged += OnBarSizeChanged;
        }

        if (GetTemplateChild(PartLegend) is ItemsControl legend)
        {
            legend.ItemsSource = _legendItems;
        }
    }

    protected override bool IsItemItsOwnContainerOverride(object item) => item is MeterBarItem;

    protected override DependencyObject GetContainerForItemOverride() => new MeterBarItem();

    internal void SyncItems(UIElementCollection children)
    {
        _legendItems.Clear();
        foreach (UIElement child in children)
        {
            if (child is MeterBarItem item)
            {
                item.SetPaletteBrush(PaletteBrush(_legendItems.Count));
                _legendItems.Add(item);
            }
        }
    }

    private void OnBarSizeChanged(object sender, SizeChangedEventArgs e)
    {
        double radius = e.NewSize.Height / 2d;
        _bar!.Clip = new RectangleGeometry(new Rect(e.NewSize), radius, radius);
    }

    private Brush PaletteBrush(int index)
    {
        return TryFindResource(PaletteKeys[index % PaletteKeys.Length]) as Brush ?? Brushes.Gray;
    }
}

/// <summary>
/// Legend list whose items are the bar's <see cref="MeterBarItem"/> containers, shown through a data template.
/// </summary>
internal sealed class MeterBarLegendList : ItemsControl
{
    protected override bool IsItemItsOwnContainerOverride(object item) => false;
}
