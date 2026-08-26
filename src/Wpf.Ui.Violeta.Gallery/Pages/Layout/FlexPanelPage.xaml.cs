using Wpf.Ui.Controls;

namespace Wpf.Ui.Violeta.Gallery.Pages.Layout;

public partial class FlexPanelPage : Wpf.Ui.Violeta.Controls.Page
{
    public FlexPanelPage()
    {
        InitializeComponent();

        CbDirection.SelectionChanged += (_, _) => UpdatePanelDirection();
        CbJustifyContent.SelectionChanged += (_, _) => UpdatePanelJustifyContent();
        CbAlignItems.SelectionChanged += (_, _) => UpdatePanelAlignItems();
        CbWrap.SelectionChanged += (_, _) => UpdatePanelWrap();
    }

    private void UpdatePanelDirection()
    {
        if (DemoPanel is null || CbDirection.SelectedIndex < 0)
        {
            return;
        }

        DemoPanel.Direction = CbDirection.SelectedIndex switch
        {
            0 => FlexDirection.Row,
            1 => FlexDirection.Column,
            2 => FlexDirection.RowReverse,
            3 => FlexDirection.ColumnReverse,
            _ => FlexDirection.Row,
        };
    }

    private void UpdatePanelJustifyContent()
    {
        if (DemoPanel is null || CbJustifyContent.SelectedIndex < 0)
        {
            return;
        }

        DemoPanel.JustifyContent = CbJustifyContent.SelectedIndex switch
        {
            0 => FlexJustify.Start,
            1 => FlexJustify.End,
            2 => FlexJustify.Center,
            3 => FlexJustify.SpaceBetween,
            4 => FlexJustify.SpaceAround,
            5 => FlexJustify.SpaceEvenly,
            _ => FlexJustify.Start,
        };
    }

    private void UpdatePanelAlignItems()
    {
        if (DemoPanel is null || CbAlignItems.SelectedIndex < 0)
        {
            return;
        }

        DemoPanel.AlignItems = CbAlignItems.SelectedIndex switch
        {
            0 => FlexAlign.Auto,
            1 => FlexAlign.Stretch,
            2 => FlexAlign.Start,
            3 => FlexAlign.End,
            4 => FlexAlign.Center,
            5 => FlexAlign.Baseline,
            _ => FlexAlign.Stretch,
        };
    }

    private void UpdatePanelWrap()
    {
        if (DemoPanel is null || CbWrap.SelectedIndex < 0)
        {
            return;
        }

        DemoPanel.Wrap = CbWrap.SelectedIndex switch
        {
            0 => FlexWrap.NoWrap,
            1 => FlexWrap.Wrap,
            2 => FlexWrap.WrapReverse,
            _ => FlexWrap.NoWrap,
        };
    }
}
