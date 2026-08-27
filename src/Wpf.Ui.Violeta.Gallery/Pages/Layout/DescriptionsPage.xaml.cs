using System.Collections.ObjectModel;
using Wpf.Ui.Violeta.Controls;

namespace Wpf.Ui.Violeta.Gallery.Pages.Layout;

public partial class DescriptionsPage : Page
{
    public DescriptionsPage()
    {
        DescriptionItems =
        [
            new DescriptionEntry("Actual Users", "1,480,000"),
            new DescriptionEntry("7-day Retention", "98%"),
            new DescriptionEntry("Security Level", "III"),
            new DescriptionEntry("Category Tag", "E-commerce"),
            new DescriptionEntry("Authorized State", "Unauthorized"),
        ];

        DescriptionItems2 =
        [
            new DescriptionEntry("抖音号", "SemiDesign"),
            new DescriptionEntry("主播类型", "自由主播"),
            new DescriptionEntry("安全等级", "3级"),
            new DescriptionEntry("垂类标签", "编程"),
            new DescriptionEntry("作品数量", "88888888"),
            new DescriptionEntry("认证状态", "这是一个很长很长很长很长很长很长很长很长很长的值"),
            new DescriptionEntry("上次直播时间", "2024-05-01 12:00:00"),
        ];

        InitializeComponent();
        DataContext = this;
    }

    public ObservableCollection<DescriptionEntry> DescriptionItems { get; }

    public ObservableCollection<DescriptionEntry> DescriptionItems2 { get; }

    public sealed record DescriptionEntry(string Label, string Description);
}
