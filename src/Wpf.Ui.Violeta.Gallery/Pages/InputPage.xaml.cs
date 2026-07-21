using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Violeta.Controls;

namespace Wpf.Ui.Violeta.Gallery.Pages;

public partial class InputPage : Wpf.Ui.Violeta.Controls.Page
{
    private static readonly string[] _spinnerWords = ["Apple", "Banana", "Cherry", "Durian", "Elderberry"];
    private int _spinnerIndex;

    public InputPage()
    {
        InitializeComponent();
        Loaded += InputPage_OnLoaded;
    }

    private void InputPage_OnLoaded(object sender, RoutedEventArgs e)
    {
        // MultiComboBox
        MultiComboBoxDemo.ItemsSource = new[] { "苹果", "香蕉", "樱桃", "草莓", "蓝莓", "西瓜" };

        // TagComboBox
        TagComboBoxDemo.ItemsSource = new[] { "前端", "后端", "DevOps", "UI/UX", "移动端", "数据库" };

        // CascadingComboBox – 省份/城市
        CascadingComboBoxDemo.ItemsSource = new ICascadingItem[]
        {
            new CascadingItem("广东",
            [
                new CascadingItem("广州"),
                new CascadingItem("深圳"),
                new CascadingItem("东莞"),
                new CascadingItem("佛山"),
            ]),
            new CascadingItem("浙江",
            [
                new CascadingItem("杭州"),
                new CascadingItem("宁波"),
                new CascadingItem("温州"),
                new CascadingItem("嘉兴"),
            ]),
            new CascadingItem("江苏",
            [
                new CascadingItem("南京"),
                new CascadingItem("苏州"),
                new CascadingItem("无锡"),
                new CascadingItem("常州"),
            ]),
        };

        // ValuePicker - 产品规格选择器
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

    private void MultiComboBoxDemo_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (MultiComboBoxDemo.MultiSelectedItems.Count == 0)
        {
            MultiComboBoxResultText.Text = "已选择：(无)";
        }
        else
        {
            MultiComboBoxResultText.Text = "已选择：" + string.Join("、", MultiComboBoxDemo.MultiSelectedItems);
        }
    }

    private void ButtonSpinner_OnSpin(object sender, SpinEventArgs e)
    {
        int delta = e.Direction == SpinDirection.Increase ? 1 : -1;
        _spinnerIndex = ((_spinnerIndex + delta) % _spinnerWords.Length + _spinnerWords.Length) % _spinnerWords.Length;
        SpinnerValueText.Text = _spinnerWords[_spinnerIndex];
    }
}
