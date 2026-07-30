using System.Windows;
using Wpf.Ui.Violeta.Controls;

namespace Wpf.Ui.Violeta.Gallery.Pages.Selectors;

public partial class ValuePickerPage : Wpf.Ui.Violeta.Controls.Page
{
    public ValuePickerPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        ValuePickerDemo.Columns =
        [
            new ValuePickerColumn
            {
                Placeholder = "系列",
                Items = ["标准版", "专业版", "旗舰版"],
            },
            new ValuePickerColumn
            {
                Placeholder = "容量",
                Items = ["128 GB", "256 GB", "512 GB", "1 TB"],
            },
            new ValuePickerColumn
            {
                Placeholder = "颜色",
                Items = ["黑色", "白色", "蓝色", "金色"],
                ShouldLoop = false,
            },
        ];
        ValuePickerDemo.SelectedValuesChanged += (_, _) =>
        {
            ValuePickerResultText.Text = ValuePickerDemo.SelectedValues is { Length: > 0 } values
                ? $"已选择：{string.Join(" / ", values)}"
                : "已选择：(无)";
        };
    }
}
