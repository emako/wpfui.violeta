using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using LiteObservableLanguages;
using Wpf.Ui.Violeta.Gallery.Globalization;

namespace Wpf.Ui.Violeta.Gallery.Pages.Collections;

public record DragDropItemModel(string Caption)
{
    public override string ToString() => Caption;
}

public record DragDropGridRowModel(string Name, string City);

public class DragDropTreeNode : INotifyPropertyChanged
{
    private bool _isExpanded;

    public DragDropTreeNode(string caption)
    {
        Caption = caption;
        Children = new ObservableCollection<DragDropTreeNode>();
    }

    public string Caption { get; }

    public ObservableCollection<DragDropTreeNode> Children { get; }

    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            if (value == _isExpanded)
            {
                return;
            }

            _isExpanded = value;
            OnPropertyChanged();
        }
    }

    public override string ToString() => Caption;

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class DragDropSampleViewModel
{
    public ObservableCollection<DragDropItemModel> Collection1 { get; } = [];

    public ObservableCollection<DragDropItemModel> Collection2 { get; } = [];

    public ObservableCollection<DragDropTreeNode> TreeCollection1 { get; } = [];

    public ObservableCollection<DragDropTreeNode> TreeCollection2 { get; } = [];

    public ObservableCollection<DragDropGridRowModel> DataGridCollection1 { get; } = [];

    public ObservableCollection<DragDropGridRowModel> DataGridCollection2 { get; } = [];
}

public partial class DragDropPage : Wpf.Ui.Violeta.Controls.Page
{
    private readonly DragDropSampleViewModel _viewModel = new();

    public DragDropPage()
    {
        InitializeComponent();
        DataContext = _viewModel;
        Loaded += (_, _) => InitializeSampleData();
    }

    private void InitializeSampleData()
    {
        _viewModel.Collection1.Clear();
        _viewModel.Collection2.Clear();
        _viewModel.TreeCollection1.Clear();
        _viewModel.TreeCollection2.Clear();
        _viewModel.DataGridCollection1.Clear();
        _viewModel.DataGridCollection2.Clear();

        LeftUnboundListBox.Items.Clear();
        RightUnboundListBox.Items.Clear();

        for (var i = 1; i <= 8; i++)
        {
            _viewModel.Collection1.Add(new DragDropItemModel(string.Format(LangKeys.Sample_DragDrop_Item.Tr(), i)));
        }

        for (var i = 1; i <= 4; i++)
        {
            _viewModel.Collection2.Add(new DragDropItemModel(string.Format(LangKeys.Sample_DragDrop_Item.Tr(), i + 8)));
        }

        for (var i = 1; i <= 5; i++)
        {
            LeftUnboundListBox.Items.Add(new DragDropItemModel(string.Format(LangKeys.Sample_DragDrop_UnboundItem.Tr(), i)));
        }

        for (var i = 6; i <= 10; i++)
        {
            RightUnboundListBox.Items.Add(new DragDropItemModel(string.Format(LangKeys.Sample_DragDrop_UnboundItem.Tr(), i)));
        }

        for (var root = 1; root <= 3; root++)
        {
            var node = new DragDropTreeNode(string.Format(LangKeys.Sample_DragDrop_TreeRoot.Tr(), root))
            {
                IsExpanded = root == 1,
            };

            for (var child = 1; child <= 3; child++)
            {
                node.Children.Add(new DragDropTreeNode(string.Format(LangKeys.Sample_DragDrop_TreeChild.Tr(), child + root * 10)));
            }

            _viewModel.TreeCollection1.Add(node);
        }

        _viewModel.TreeCollection2.Add(new DragDropTreeNode(string.Format(LangKeys.Sample_DragDrop_TreeRoot.Tr(), 4))
        {
            IsExpanded = true,
        });

        var cities = new[]
        {
            LangKeys.Sample_DragDrop_CityA.Tr(),
            LangKeys.Sample_DragDrop_CityB.Tr(),
            LangKeys.Sample_DragDrop_CityC.Tr(),
            LangKeys.Sample_DragDrop_CityD.Tr(),
        };

        for (var i = 0; i < 6; i++)
        {
            _viewModel.DataGridCollection1.Add(new DragDropGridRowModel(
                string.Format(LangKeys.Sample_DragDrop_Person.Tr(), i + 1),
                cities[i % cities.Length]));
        }

        for (var i = 0; i < 2; i++)
        {
            _viewModel.DataGridCollection2.Add(new DragDropGridRowModel(
                string.Format(LangKeys.Sample_DragDrop_Person.Tr(), i + 7),
                cities[(i + 2) % cities.Length]));
        }
    }
}
