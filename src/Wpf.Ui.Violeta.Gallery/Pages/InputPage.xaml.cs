using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
        CascadingLevel1.ItemsSource = new ObservableCollection<ICascadingItem>
        {
            new CascadingItem("广东"),
            new CascadingItem("浙江"),
            new CascadingItem("江苏"),
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

    private void CascadingLevel1_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var cityMap = new Dictionary<string, string[]>
        {
            ["广东"] = ["广州", "深圳", "东莞", "佛山"],
            ["浙江"] = ["杭州", "宁波", "温州", "嘉兴"],
            ["江苏"] = ["南京", "苏州", "无锡", "常州"],
        };

        if (CascadingLevel1.SelectedItem is ICascadingItem item && cityMap.TryGetValue(item.Label, out var cities))
        {
            CascadingLevel2.IsEnabled = true;
            CascadingLevel2.ItemsSource = cities.Select(c => new CascadingItem(c)).ToList();
            CascadingLevel2.SelectedCascadingItem = null;
        }
        else
        {
            CascadingLevel2.IsEnabled = false;
            CascadingLevel2.ItemsSource = null;
        }

        UpdateCascadingResult();
    }

    private void UpdateCascadingResult()
    {
        var province = CascadingLevel1.SelectedCascadingItem?.Label;
        var city = CascadingLevel2.SelectedCascadingItem?.Label;

        CascadingResultText.Text = (province, city) switch
        {
            (not null, not null) => $"已选择：{province} - {city}",
            (not null, null) => $"已选择：{province}（请选择城市）",
            _ => "已选择：(无)",
        };
    }

    private void ButtonSpinner_OnSpin(object sender, SpinEventArgs e)
    {
        int delta = e.Direction == SpinDirection.Increase ? 1 : -1;
        _spinnerIndex = ((_spinnerIndex + delta) % _spinnerWords.Length + _spinnerWords.Length) % _spinnerWords.Length;
        SpinnerValueText.Text = _spinnerWords[_spinnerIndex];
    }
}
