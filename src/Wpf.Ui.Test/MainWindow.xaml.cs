using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using Wpf.Ui.Violeta.Appearance;
using Wpf.Ui.Violeta.Controls;
using Wpf.Ui.Violeta.Threading;
using Wpf.Ui.Violeta.Win32;
using ContentDialog = Wpf.Ui.Violeta.Controls.ContentDialog;
using ContentDialogButton = Wpf.Ui.Violeta.Controls.ContentDialogButton;

namespace Wpf.Ui.Test;

[ObservableObject]
public partial class MainWindow : ShellWindow
{
    private static readonly string[] _buttonSpinnerWords =
    [
        "Apple",
        "Banana",
        "Cherry",
        "Durian",
        "Elderberry",
    ];

    private int _buttonSpinnerDefaultValue;
    private int _buttonSpinnerLeftValue;
    private int _buttonSpinnerSplitRightValue;
    private int _buttonSpinnerSplitLeftValue;

    public MainWindow()
    {
        DataContext = this;
        InitializeComponent();

        Thread.Sleep(600); // Load simulation
        Splash.CloseOnLoaded(this, minimumMilliseconds: 1800);

        ScrollViewer.ScrollToEnd();

        InitNode1Value();
        InitMultiComboBoxDemo();
        InitCascadingComboBoxDemoLevel1();
        InitCascadingComboBoxDemoLevel3();
        InitCascadingComboBoxDemoLevel2();
        InitCascadingComboBoxDemoLevel4();
        InitCascadingComboBoxDemoMixedDepth();
        InitTreeComboBoxDemo();
        InitValuePickerDemo();

        Dispatcher.BeginInvoke(async () =>
        {
            await Task.Delay(2222);
            DrawerContainer.Visibility = System.Windows.Visibility.Visible;
        });
    }


    // ── ButtonSpinner ─────────────────────────────────────────────

    private void ButtonSpinnerDemo_OnSpin(object? sender, SpinEventArgs e)
    {
        if (sender is not ButtonSpinner spinner)
        {
            return;
        }

        int delta = e.Direction == SpinDirection.Increase ? 1 : -1;

        if (ReferenceEquals(spinner.Content, ButtonSpinnerDefaultValueText))
        {
            _buttonSpinnerDefaultValue = WrapIndex(_buttonSpinnerDefaultValue + delta, _buttonSpinnerWords.Length);
            ButtonSpinnerDefaultValueText.Text = _buttonSpinnerWords[_buttonSpinnerDefaultValue];
            return;
        }

        if (ReferenceEquals(spinner.Content, ButtonSpinnerLeftValueText))
        {
            _buttonSpinnerLeftValue = WrapIndex(_buttonSpinnerLeftValue + delta, _buttonSpinnerWords.Length);
            ButtonSpinnerLeftValueText.Text = _buttonSpinnerWords[_buttonSpinnerLeftValue];
            return;
        }

        if (ReferenceEquals(spinner.Content, ButtonSpinnerSplitRightValueText))
        {
            _buttonSpinnerSplitRightValue = WrapIndex(_buttonSpinnerSplitRightValue + delta, _buttonSpinnerWords.Length);
            ButtonSpinnerSplitRightValueText.Text = _buttonSpinnerWords[_buttonSpinnerSplitRightValue];
            return;
        }

        if (ReferenceEquals(spinner.Content, ButtonSpinnerSplitLeftValueText))
        {
            _buttonSpinnerSplitLeftValue = WrapIndex(_buttonSpinnerSplitLeftValue + delta, _buttonSpinnerWords.Length);
            ButtonSpinnerSplitLeftValueText.Text = _buttonSpinnerWords[_buttonSpinnerSplitLeftValue];
        }
    }

    private static int WrapIndex(int value, int length)
    {
        return (value % length + length) % length;
    }


    // ── MultiComboBox ─────────────────────────────────────────────

    private void InitMultiComboBoxDemo()
    {
        MultiComboBoxDemo.ItemsSource = new[] { "Apple", "Banana", "Cherry", "Durian", "Elderberry" };
        MultiComboBoxDemo.MultiSelectedItems.CollectionChanged += (_, _) =>
        {
            MultiComboBoxSelectedText = MultiComboBoxDemo.MultiSelectedItems.Count == 0
                ? "Selected: (none)"
                : "Selected: " + string.Join(", ", MultiComboBoxDemo.MultiSelectedItems);
        };
    }

    [ObservableProperty]
    public partial int ThemeIndex { get; set; } = (int)ApplicationTheme.Dark;

    [ObservableProperty]
    public partial bool BoolStateDemoValue { get; set; }

    [ObservableProperty]
    public partial string MultiComboBoxSelectedText { get; set; } = "Selected: (none)";


    // ── TransitioningContentControl ───────────────────────────────

    private readonly string[] _transitioningDemoSlides =
    [
        "Slide 1 - Welcome to Violeta",
        "Slide 2 - Try changing transition type",
        "Slide 3 - Content changes with animation",
        "Slide 4 - This is a looping demo",
    ];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TransitioningDemoStatus))]
    private int _transitioningDemoIndex;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TransitioningDemoStatus))]
    private TransitionType _transitioningDemoTransition = TransitionType.Right;

    [ObservableProperty]
    private string _transitioningDemoContent = "Slide 1 - Welcome to Violeta";

    public string TransitioningDemoStatus =>
        $"Slide {TransitioningDemoIndex + 1}/{_transitioningDemoSlides.Length} | {TransitioningDemoTransition}";

    partial void OnTransitioningDemoIndexChanged(int value)
    {
        if (value < 0 || value >= _transitioningDemoSlides.Length)
        {
            return;
        }

        TransitioningDemoContent = _transitioningDemoSlides[value];
    }

    [RelayCommand]
    private void NextTransitioningDemo()
    {
        TransitioningDemoIndex = (TransitioningDemoIndex + 1) % _transitioningDemoSlides.Length;
    }

    [RelayCommand]
    private void PreviousTransitioningDemo()
    {
        TransitioningDemoIndex = (TransitioningDemoIndex - 1 + _transitioningDemoSlides.Length) % _transitioningDemoSlides.Length;
    }

    [RelayCommand]
    private void SetTransitioningDemoTransition(string transitionName)
    {
        if (Enum.TryParse(transitionName, true, out TransitionType transition))
        {
            TransitioningDemoTransition = transition;
        }
    }


    // ── CascadingComboBox ─────────────────────────────────────────

    [ObservableProperty]
    public partial ObservableCollection<ICascadingItem> CascadingComboBoxDemoItems_Level1 { get; set; } = [];

    [ObservableProperty]
    public partial ICascadingItem? CascadingComboBoxSelectedValue_Level1 { get; set; }

    [ObservableProperty]
    public partial string CascadingComboBoxSelectedText_Level1 { get; set; } = "Selected: (none)";

    [ObservableProperty]
    public partial ObservableCollection<ICascadingItem> CascadingComboBoxDemoItems_Level3 { get; set; } = [];

    [ObservableProperty]
    public partial ICascadingItem? CascadingComboBoxSelectedValue_Level3 { get; set; }

    [ObservableProperty]
    public partial string CascadingComboBoxSelectedText_Level3 { get; set; } = "Selected: (none)";

    [ObservableProperty]
    public partial ObservableCollection<ICascadingItem> CascadingComboBoxDemoItems_Level2 { get; set; } = [];

    [ObservableProperty]
    public partial ICascadingItem? CascadingComboBoxSelectedValue_Level2 { get; set; }

    [ObservableProperty]
    public partial string CascadingComboBoxSelectedText_Level2 { get; set; } = "Selected: (none)";

    [ObservableProperty]
    public partial ObservableCollection<ICascadingItem> CascadingComboBoxDemoItems_Level4 { get; set; } = [];

    [ObservableProperty]
    public partial ICascadingItem? CascadingComboBoxSelectedValue_Level4 { get; set; }

    [ObservableProperty]
    public partial string CascadingComboBoxSelectedText_Level4 { get; set; } = "Selected: (none)";

    [ObservableProperty]
    public partial ObservableCollection<ICascadingItem> CascadingComboBoxDemoItems_MixedDepth { get; set; } = [];

    [ObservableProperty]
    public partial ICascadingItem? CascadingComboBoxSelectedValue_MixedDepth { get; set; }

    [ObservableProperty]
    public partial string CascadingComboBoxSelectedText_MixedDepth { get; set; } = "Selected: (none)";

    [ObservableProperty]
    public partial ObservableCollection<string> TabItems { get; set; } =
    [
        "Dashboard",
        "Analytics",
        "Reports",
        "Settings",
    ];

    [ObservableProperty]
    public partial string? SelectedTab { get; set; } = "Dashboard";

    partial void OnCascadingComboBoxSelectedValue_Level3Changed(ICascadingItem? value)
    {
        CascadingComboBoxSelectedText_Level3 = value is null
            ? "Selected: (none)"
            : $"Selected: {value.Label}";
    }

    partial void OnCascadingComboBoxSelectedValue_Level1Changed(ICascadingItem? value)
    {
        CascadingComboBoxSelectedText_Level1 = value is null
            ? "Selected: (none)"
            : $"Selected: {value.Label}";
    }

    partial void OnCascadingComboBoxSelectedValue_Level2Changed(ICascadingItem? value)
    {
        CascadingComboBoxSelectedText_Level2 = value is null
            ? "Selected: (none)"
            : $"Selected: {value.Label}";
    }

    partial void OnCascadingComboBoxSelectedValue_Level4Changed(ICascadingItem? value)
    {
        CascadingComboBoxSelectedText_Level4 = value is null
            ? "Selected: (none)"
            : $"Selected: {value.Label}";
    }

    partial void OnCascadingComboBoxSelectedValue_MixedDepthChanged(ICascadingItem? value)
    {
        CascadingComboBoxSelectedText_MixedDepth = value is null
            ? "Selected: (none)"
            : $"Selected: {value.Label}";
    }

    private void InitCascadingComboBoxDemoLevel1()
    {
        CascadingComboBoxDemoItems_Level1 =
        [
            new CascadingItem("Option A"),
            new CascadingItem("Option B"),
            new CascadingItem("Option C"),
            new CascadingItem("Option D"),
            new CascadingItem("Option E"),
        ];
    }

    private void InitCascadingComboBoxDemoLevel3()
    {
        CascadingComboBoxDemoItems_Level3 =
        [
            new CascadingItem("Food",
            [
                new CascadingItem("Fruits",
                [
                    new CascadingItem("Apple"),
                    new CascadingItem("Banana"),
                    new CascadingItem("Cherry"),
                ]),
                new CascadingItem("Vegetables",
                [
                    new CascadingItem("Carrot"),
                    new CascadingItem("Broccoli"),
                    new CascadingItem("Spinach"),
                ]),
            ]),
            new CascadingItem("Drinks",
            [
                new CascadingItem("Hot",
                [
                    new CascadingItem("Coffee"),
                    new CascadingItem("Tea"),
                ]),
                new CascadingItem("Cold",
                [
                    new CascadingItem("Water"),
                    new CascadingItem("Juice"),
                ]),
            ]),
        ];
    }

    private void InitCascadingComboBoxDemoLevel2()
    {
        CascadingComboBoxDemoItems_Level2 =
        [
            new CascadingItem("Electronics",
            [
                new CascadingItem("Phone"),
                new CascadingItem("Laptop"),
                new CascadingItem("Tablet"),
            ]),
            new CascadingItem("Clothing",
            [
                new CascadingItem("Shirt"),
                new CascadingItem("Pants"),
                new CascadingItem("Jacket"),
            ]),
            new CascadingItem("Books",
            [
                new CascadingItem("Fiction"),
                new CascadingItem("Science"),
                new CascadingItem("History"),
            ]),
        ];
    }

    private void InitCascadingComboBoxDemoLevel4()
    {
        CascadingComboBoxDemoItems_Level4 =
        [
            new CascadingItem("Vehicles",
            [
                new CascadingItem("Cars",
                [
                    new CascadingItem("Sedan",
                    [
                        new CascadingItem("Honda Accord"),
                        new CascadingItem("Toyota Camry"),
                        new CascadingItem("BMW 3 Series"),
                    ]),
                    new CascadingItem("SUV",
                    [
                        new CascadingItem("Toyota RAV4"),
                        new CascadingItem("Honda CR-V"),
                        new CascadingItem("BMW X5"),
                    ]),
                    new CascadingItem("Truck",
                    [
                        new CascadingItem("Ford F-150"),
                        new CascadingItem("Chevrolet Silverado"),
                        new CascadingItem("Ram 1500"),
                    ]),
                ]),
                new CascadingItem("Motorcycles",
                [
                    new CascadingItem("Cruiser",
                    [
                        new CascadingItem("Harley-Davidson"),
                        new CascadingItem("Honda Rebel"),
                        new CascadingItem("Yamaha V-Star"),
                    ]),
                    new CascadingItem("Sport",
                    [
                        new CascadingItem("Kawasaki Ninja"),
                        new CascadingItem("Honda CBR"),
                        new CascadingItem("Yamaha YZF"),
                    ]),
                ]),
            ]),
            new CascadingItem("Furniture",
            [
                new CascadingItem("Living Room",
                [
                    new CascadingItem("Sofa",
                    [
                        new CascadingItem("2-Seater"),
                        new CascadingItem("3-Seater"),
                        new CascadingItem("L-Shape"),
                    ]),
                    new CascadingItem("Coffee Table",
                    [
                        new CascadingItem("Wood"),
                        new CascadingItem("Glass"),
                        new CascadingItem("Metal"),
                    ]),
                ]),
                new CascadingItem("Bedroom",
                [
                    new CascadingItem("Bed",
                    [
                        new CascadingItem("Single"),
                        new CascadingItem("Double"),
                        new CascadingItem("Queen"),
                    ]),
                    new CascadingItem("Wardrobe",
                    [
                        new CascadingItem("2-Door"),
                        new CascadingItem("3-Door"),
                        new CascadingItem("4-Door"),
                    ]),
                ]),
            ]),
        ];
    }

    private void InitCascadingComboBoxDemoMixedDepth()
    {
        CascadingComboBoxDemoItems_MixedDepth =
        [
            new CascadingItem("Quick Pick",
            [
                new CascadingItem("Red"),
                new CascadingItem("Green"),
                new CascadingItem("Blue"),
            ]),
            new CascadingItem("Categories",
            [
                new CascadingItem("Electronics",
                [
                    new CascadingItem("Smartphones",
                    [
                        new CascadingItem("iPhone"),
                        new CascadingItem("Samsung Galaxy"),
                        new CascadingItem("Google Pixel"),
                    ]),
                    new CascadingItem("Laptops",
                    [
                        new CascadingItem("Dell"),
                        new CascadingItem("HP"),
                        new CascadingItem("Lenovo"),
                    ]),
                ]),
                new CascadingItem("Clothing",
                [
                    new CascadingItem("Shirts"),
                    new CascadingItem("Pants"),
                    new CascadingItem("Shoes"),
                ]),
            ]),
            new CascadingItem("Direct Selection",
            [
                new CascadingItem("Option A"),
                new CascadingItem("Option B"),
                new CascadingItem("Option C"),
            ]),
        ];
    }

    // ── TreeComboBox ──────────────────────────────────────────────

    [ObservableProperty]
    private System.Collections.Generic.List<TreeComboBoxNode>? _treeComboBoxItems;

    [ObservableProperty]
    private TreeComboBoxNode? _treeComboBoxSelectedItem;

    private void InitTreeComboBoxDemo()
    {
        TreeComboBoxItems =
        [
            new TreeComboBoxNode("Item 1",
            [
                new TreeComboBoxNode("Item 1-1",
                [
                    new TreeComboBoxNode("Item 1-1-1"),
                    new TreeComboBoxNode("Item 1-1-2"),
                ]),
                new TreeComboBoxNode("Item 1-2"),
            ]),
            new TreeComboBoxNode("Item 2",
            [
                new TreeComboBoxNode("Item 2-1"),
                new TreeComboBoxNode("Item 2-2"),
            ]),
            new TreeComboBoxNode("Item 3"),
        ];
    }

    // ── ValuePicker ───────────────────────────────────────────────────────────

    private void InitValuePickerDemo()
    {
        RegionValuePicker.Columns =
        [
            new ValuePickerColumn
            {
                Placeholder = "State",
                Items = ["California", "Texas", "New York"],
            },
            new ValuePickerColumn
            {
                Placeholder = "City",
                Items = ["Los Angeles", "San Francisco", "Austin", "Houston", "New York City"],
            },
        ];
        RegionValuePicker.SelectedValues = ["Texas", "Austin"];
        RegionSelectionText.Text = $"Selected: {string.Join(" / ", RegionValuePicker.SelectedValues)}";
        RegionValuePicker.SelectedValuesChanged += (_, _) =>
        {
            RegionSelectionText.Text = RegionValuePicker.SelectedValues is { Length: > 0 } values
                ? $"Selected: {string.Join(" / ", values)}"
                : "Selected: (none)";
        };

        ProductValuePicker.Columns =
        [
            new ValuePickerColumn
            {
                Placeholder = "Series",
                Items = ["Standard", "Pro", "Ultra"],
            },
            new ValuePickerColumn
            {
                Placeholder = "Storage",
                Items = ["128 GB", "256 GB", "512 GB", "1 TB"],
            },
            new ValuePickerColumn
            {
                Placeholder = "Color",
                Items = ["Black", "White", "Blue"],
                ShouldLoop = false,
            },
        ];
        ProductValuePicker.SelectedValuesChanged += (_, _) =>
        {
            ProductSelectionText.Text = ProductValuePicker.SelectedValues is { Length: > 0 } values
                ? $"Selected: {string.Join(" / ", values)}"
                : "Selected: (none)";
        };
    }

    // ── PinCode ──────────────────────────────────────────────────────────────

    [ObservableProperty]
    private string? _pinCodeResult;

    [RelayCommand]
    private void PinCodeComplete(object? parameter)
    {
        if (parameter is System.Collections.Generic.IList<string> code)
            PinCodeResult = string.Join("", code);
    }

    // ── LoadingButton ─────────────────────────────────────────────────────────

    [ObservableProperty]
    private bool _isLoadingButtonLoading = true;

    [RelayCommand]
    private void ToggleLoadingButton()
    {
        IsLoadingButtonLoading = !IsLoadingButtonLoading;
    }

    [ObservableProperty]
    private bool _isLoadingButtonCommandLoading = false;

    [RelayCommand]
    private async Task SimulateLoadingButtonAsync()
    {
        IsLoadingButtonCommandLoading = true;
        await Task.Delay(1000);
        IsLoadingButtonCommandLoading = false;
        Toast.Success("Succeeded!");
    }

    // ── TagInput ──────────────────────────────────────────────────────────────

    [ObservableProperty]
    private ObservableCollection<string> _tagInputBasicTags = [];

    [ObservableProperty]
    private ObservableCollection<string> _tagInputSeparatorTags = [];

    [ObservableProperty]
    private ObservableCollection<string> _tagInputNoDupTags = [];

    [ObservableProperty]
    private ObservableCollection<string> _tagInputMaxCountTags = [];

    [ObservableProperty]
    private ObservableCollection<string> _tagInputLostFocusTags = [];

    [ObservableProperty]
    private ObservableCollection<string> _tagInputPrefilledTags = ["WPF", "Fluent", "UI"];

    [ObservableProperty]
    private string _tagInputBasicTagsText = "(none)";

    partial void OnTagInputBasicTagsChanged(ObservableCollection<string> value)
    {
        value.CollectionChanged += (_, _) =>
            TagInputBasicTagsText = value.Count == 0 ? "(none)" : string.Join(", ", value);
    }

    // ── TagComboBox ───────────────────────────────────────────────────────────

    [ObservableProperty]
    private ObservableCollection<string> _tagComboBoxFruits =
        ["Apple", "Banana", "Cherry", "Durian", "Elderberry", "Fig", "Grape"];

    [ObservableProperty]
    private ObservableCollection<object> _tagComboBoxSelectedFruits = [];

    [ObservableProperty]
    private ObservableCollection<object> _tagComboBoxPreSelected = ["Apple", "Banana"];

    [ObservableProperty]
    private string _tagComboBoxSelectedFruitsText = "(none)";

    partial void OnTagComboBoxSelectedFruitsChanged(ObservableCollection<object> value)
    {
        value.CollectionChanged += (_, _) =>
            TagComboBoxSelectedFruitsText = value.Count == 0 ? "(none)" : string.Join(", ", value);
    }

    // ── RangeSlider ───────────────────────────────────────────────────────────

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RangeSliderText))]
    private double _rangeSliderLower = 20;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RangeSliderText))]
    private double _rangeSliderUpper = 70;

    public string RangeSliderText => $"[{RangeSliderLower:0}, {RangeSliderUpper:0}]";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RangeSliderTickText))]
    private double _rangeSliderTickLower = 20;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RangeSliderTickText))]
    private double _rangeSliderTickUpper = 60;

    public string RangeSliderTickText => $"[{RangeSliderTickLower:0}, {RangeSliderTickUpper:0}]";

    // ── Skeleton ─────────────────────────────────────────────────────────────

    [ObservableProperty]
    private bool _isSkeletonLoading = true;

    [ObservableProperty]
    private bool _isSkeletonActive = true;

    [RelayCommand]
    private void ToggleSkeleton()
    {
        IsSkeletonLoading = !IsSkeletonLoading;
        IsSkeletonActive = IsSkeletonLoading;
    }


    // ── QrCode ────────────────────────────────────────────────────

    [ObservableProperty]
    private string _qrCodeData = "https://github.com/emako/wpfui.violeta";

    [ObservableProperty]
    private EccLevel _qrCodeEccLevel = EccLevel.Medium;

    [ObservableProperty]
    private double _qrCodeSymbolCornerRatio = 0.5;


    // ── Timeline ──────────────────────────────────────────────────

    public TimelineItemViewModel[] TimelineItems { get; } =
    [
        new() { Time = new DateTime(2024, 1, 1), Header = "Completed", Description = "Step 1 finished successfully.", ItemType = TimelineItemType.Success },
        new() { Time = new DateTime(2024, 6, 1), Header = "In Progress", Description = "Step 2 is currently ongoing.", ItemType = TimelineItemType.Ongoing },
        new() { Time = new DateTime(2025, 1, 1), Header = "Warning", Description = "Step 3 completed with warnings.", ItemType = TimelineItemType.Warning },
        new() { Time = new DateTime(2025, 6, 1), Header = "Failed", Description = "Step 4 encountered an error.", ItemType = TimelineItemType.Error },
        new() { Time = new DateTime(2026, 1, 1), Header = "Pending", Description = "Step 5 has not started yet.", ItemType = TimelineItemType.Default },
    ];


    // ── KeyGestureInput ───────────────────────────────────────────

    private KeyGestureValue? _keyGesture;

    public KeyGestureValue? KeyGesture
    {
        get => _keyGesture;
        set
        {
            _keyGesture = value;
            OnPropertyChanged(nameof(KeyGesture));
        }
    }

    private void OnClearGestureClick(object? sender, RoutedEventArgs e)
    {
        ClearableGestureInput.Clear();
    }

    partial void OnThemeIndexChanged(int value)
    {
        ThemeManager.Apply((ApplicationTheme)value);
        ThemeManager.TrackSystemThemeChanges(isTracked: (ApplicationTheme)value == ApplicationTheme.Unknown);
    }


    // ── Toast ─────────────────────────────────────────────────────

    [RelayCommand]
    private void ShowToast(Button self)
    {
        // Demonstrate toasts with stacking enabled
        var originalIsStacked = Toast.IsStacked;
        Toast.IsStacked = true;

        string message = "This is a toast message";
        ToastLocation toastLocation = (ToastLocation)Enum.Parse(typeof(ToastLocation), self.Content.ToString()!);

        if (self.Tag.ToString() == "Information")
        {
            Toast.Information(message, toastLocation);
        }
        else if (self.Tag.ToString() == "Error")
        {
            Toast.Error(message, toastLocation);
        }
        else if (self.Tag.ToString() == "Success")
        {
            Toast.Success(message, toastLocation);
        }
        else if (self.Tag.ToString() == "Warning")
        {
            Toast.Warning(message, toastLocation);
        }
        else if (self.Tag.ToString() == "Question")
        {
            Toast.Question(message, toastLocation);
        }
        else if (self.Tag.ToString() == "None")
        {
            Toast.Show(null!, message, new ToastConfig()
            {
                Location = toastLocation,
            });
        }

        // Restore original setting after a delay
        Task.Delay(100).ContinueWith(_ => Toast.IsStacked = originalIsStacked);
    }


    // ── Flyout ────────────────────────────────────────────────────

    [RelayCommand]
    private void ShowFlyoutInline()
    {
        Toast.Success("The cake is a lie!");
    }


    // ── Toast Stacking ────────────────────────────────────────────

    [RelayCommand]
    private void ShowStackedToasts()
    {
        // Demonstrate toasts with stacking enabled
        var originalIsStacked = Toast.IsStacked;
        Toast.IsStacked = true;

        Toast.Information("First toast message");
        Toast.Warning("Second toast message");
        Toast.Error("Third toast message");
        Toast.Success("Fourth toast message");
        Toast.Question("Fifth toast message");

        // Restore original setting after a delay
        Task.Delay(100).ContinueWith(_ => Toast.IsStacked = originalIsStacked);
    }

    [RelayCommand]
    private void ShowNonStackedToasts()
    {
        // Demonstrate toasts with stacking disabled
        var originalIsStacked = Toast.IsStacked;
        Toast.IsStacked = false;

        Toast.Information("Non-stacked toast 1");
        Toast.Warning("Non-stacked toast 2");
        Toast.Error("Non-stacked toast 3");

        // Restore original setting after a delay
        Task.Delay(100).ContinueWith(_ => Toast.IsStacked = originalIsStacked);
    }

    [RelayCommand]
    private void ShowLimitedStackedToasts()
    {
        // Demonstrate max stacking limit of 2
        var originalMaxStacked = ToastConfig.MaxStacked;
        ToastConfig.MaxStacked = 2;

        Toast.Information("Limited stack 1");
        Toast.Warning("Limited stack 2");
        Toast.Error("Limited stack 3 (should overlay)");
        Toast.Success("Limited stack 4 (should overlay)");

        // Restore original setting after a delay
        Task.Delay(100).ContinueWith(_ => ToastConfig.MaxStacked = originalMaxStacked);
    }

    [RelayCommand]
    private void ShowMaintainPositionToasts()
    {
        var originalIsStacked = Toast.IsStacked;
        Toast.IsStacked = true;

        Toast.Information("Position maintained 1 (will keep position when others disappear)");
        Toast.Warning("Position maintained 2 (will keep position when others disappear)");
        Toast.Error("Position maintained 3 (will keep position when others disappear)");

        Task.Delay(100).ContinueWith(_ => Toast.IsStacked = originalIsStacked);
    }


    // ── ContentDialog ─────────────────────────────────────────────

    [RelayCommand]
    private async Task ShowContentDialogAsync()
    {
        ContentDialog dialog = new()
        {
            Title = "My sample dialog",
            Content = "Content of the dialog",
            CloseButtonText = "Close button",
            PrimaryButtonText = "Primary button",
            SecondaryButtonText = "Secondary button",
            DefaultButton = ContentDialogButton.Primary,
        };

        _ = await dialog.ShowAsync();
    }

    [RelayCommand]
    [Obsolete]
    [SuppressMessage("Style", "IDE0017:Simplify object initialization")]
    private async Task ShowWithContentPresenterForDialogs()
    {
        Wpf.Ui.Controls.ContentDialog dialog =
            new()
            {
                Title = "My sample dialog",
                Content = "Content of the dialog",
                CloseButtonText = "Close button",
                PrimaryButtonText = "Primary button",
                SecondaryButtonText = "Secondary button"
            };

        // Setting the dialog container
        dialog.DialogHost = ContentDialogHostService.ContentPresenterForDialogs;

        // Showing the dialog
        _ = await dialog.ShowAsync(CancellationToken.None);
    }


    // ── MessageBox ────────────────────────────────────────────────

    [RelayCommand]
    private void ShowMessageBox(Button self)
    {
        if (self.Content.ToString() == "Information")
        {
            _ = MessageBox.Information("This is a information message");
        }
        else if (self.Content.ToString() == "Warning")
        {
            _ = MessageBox.Warning("This is a warning message");
        }
        else if (self.Content.ToString() == "Question")
        {
            _ = MessageBox.Question("This is a question and do you want to click OK?");
        }
        else if (self.Content.ToString() == "Error")
        {
            _ = MessageBox.Error(
                """
                Dummy exception from Violeta:
                   at Violeta.View.MainWindow.OnNotifyIconLeftDoubleClick(NotifyIcon sender, RoutedEventArgs e) in D:\GitHub\Violeta\View\MainWindow.xaml.cs:line 53
                   at System.RuntimeMethodHandle.InvokeMethod(Object target, Void** arguments, Signature sig, Boolean isConstructor)
                   at System.Reflection.MethodBaseInvoker.InvokeDirectByRefWithFewArgs(Object obj, Span`1 copyOfArgs, BindingFlags invokeAttr)
                """
            );
        }
    }

    [RelayCommand]
    private async Task AsyncShowMessageBoxAsync(Button self)
    {
        if (self.Content.ToString() == "Information")
        {
            _ = await MessageBox.InformationAsync("This is a information message");
        }
        else if (self.Content.ToString() == "Warning")
        {
            _ = await MessageBox.WarningAsync("This is a warning message");
        }
        else if (self.Content.ToString() == "Question")
        {
            _ = await MessageBox.QuestionAsync("This is a question and do you want to click OK?");
        }
        else if (self.Content.ToString() == "Error")
        {
            _ = await MessageBox.ErrorAsync("This is a error message");
        }
    }


    // ── TaskDialog ────────────────────────────────────────────────

    [RelayCommand]
    private void ShowTaskDialog(Button self)
    {
        var tag = self.Content.ToString();

        // Theme toggle buttons
        if (tag == "Dark")
        {
            TaskDialog.SetTheme(TaskDialog.Theme.Dark);
            return;
        }
        if (tag == "Light")
        {
            TaskDialog.SetTheme(TaskDialog.Theme.Light);
            return;
        }

        IntPtr owner = new System.Windows.Interop.WindowInteropHelper(this).Handle;

        if (tag == "Information")
        {
            TaskDialog.Show(
                owner,
                title: "TaskDialog — Information",
                mainInstruction: "This is an information TaskDialog",
                content: "Dark-mode support is applied via DWM + SetWindowTheme + window subclassing.\n\nNo external Detours DLL is required.",
                commonButtons: TaskDialogCommonButton.OK,
                mainIcon: TaskDialog.IconInformation);
        }
        else if (tag == "Warning")
        {
            TaskDialog.Show(
                owner,
                title: "TaskDialog — Warning",
                mainInstruction: "Something may need your attention",
                content: "This is a warning TaskDialog with a Shield icon in the footer.",
                commonButtons: TaskDialogCommonButton.OK | TaskDialogCommonButton.Cancel,
                mainIcon: TaskDialog.IconWarning,
                flags: TaskDialogFlags.EnableHyperlinks,
                callback: (hwnd, notif, _, lParam, _) =>
                {
                    if (notif == TaskDialogNotification.HyperlinkClicked)
                    {
                        var url = Marshal.PtrToStringUni(lParam) ?? string.Empty;
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true });
                    }
                    return 0;
                });
        }
        else if (tag == "Error")
        {
            TaskDialog.ShowIndirect(new TaskDialogConfig
            {
                ParentWindow = owner,
                Title = "TaskDialog — Error",
                MainInstruction = "A critical error has occurred",
                Content = "This TaskDialog demonstrates the error icon and a Retry / Cancel choice.",
                CommonButtons = TaskDialogCommonButton.Retry | TaskDialogCommonButton.Cancel,
                MainIcon = TaskDialog.IconError,
                Footer = "Error code: 0x80004005",
                FooterIcon = TaskDialog.IconShield,
            });
        }
        else if (tag == "CommandLinks")
        {
            TaskDialog.ShowIndirect(new TaskDialogConfig
            {
                ParentWindow = owner,
                Title = "TaskDialog — Command Links",
                MainInstruction = "Choose an action",
                Content = "This TaskDialog uses command-link buttons.",
                Flags = TaskDialogFlags.UseCommandLinks | TaskDialogFlags.AllowDialogCancellation,
                Buttons =
                [
                    new(10, "Continue\nProceed with the current operation"),
                    new(11, "Retry\nStart the operation over from the beginning"),
                    new(12, "Cancel\nAbort and return to the previous screen"),
                ],
                DefaultButton = 10,
                MainIcon = TaskDialog.IconInformation,
            });
        }
        else if (tag == "Expanded")
        {
            TaskDialog.ShowIndirect(new TaskDialogConfig
            {
                ParentWindow = owner,
                Title = "TaskDialog — Expanded Info",
                MainInstruction = "Expandable task dialog",
                Content = "Click the expander to reveal additional details.",
                CommonButtons = TaskDialogCommonButton.OK,
                MainIcon = TaskDialog.IconInformation,
                //Flags = TaskDialogFlags.ExpandFooterArea,
                VerificationText = "Don't show this again",
                ExpandedInformation = "This area can contain detailed technical information,\nstack traces, log snippets, or links.",
                ExpandedControlText = "Hide details",
                CollapsedControlText = "Show details",
                Footer = "Switch theme: <A HREF=\"dark\">Dark</A>  |  <A HREF=\"light\">Light</A>",
                FooterIcon = TaskDialog.IconShield,
                Flags = TaskDialogFlags.EnableHyperlinks | TaskDialogFlags.ExpandFooterArea,
                Callback = (_, notif, _, lParam, _) =>
                {
                    if (notif == TaskDialogNotification.HyperlinkClicked)
                    {
                        var link = Marshal.PtrToStringUni(lParam);
                        if (link == "dark") TaskDialog.SetTheme(TaskDialog.Theme.Dark);
                        if (link == "light") TaskDialog.SetTheme(TaskDialog.Theme.Light);
                    }
                    return 0;
                },
            });
        }
    }


    // ── Notification ──────────────────────────────────────────────

    [RelayCommand]
    private void ShowNotification(Button self)
    {
        if (self.Content.ToString() == "Information")
        {
            TrayIconManager.ShowNotification("Information from Wpf.Ui.Violeta", "This is a information message", ToolTipIcon.Info, clickEvent: () => Toast.Information("User Click Notification"), closeEvent: () => Toast.Information("Notification Closed"));
        }
        else if (self.Content.ToString() == "Warning")
        {
            TrayIconManager.ShowNotification("Warning from Wpf.Ui.Violeta", "This is a warning message", ToolTipIcon.Warning, clickEvent: () => Toast.Warning("User Click Notification"), closeEvent: () => Toast.Information("Notification Closed"));
        }
        else if (self.Content.ToString() == "None")
        {
            TrayIconManager.ShowNotification("None from Wpf.Ui.Violeta", "This is a none message", ToolTipIcon.None, clickEvent: () => Toast.Question("User Click Notification"), closeEvent: () => Toast.Information("Notification Closed"));
        }
        else if (self.Content.ToString() == "Error")
        {
            TrayIconManager.ShowNotification("Error from Wpf.Ui.Violeta",
                """
                Dummy exception from Violeta:
                   at Violeta.View.MainWindow.OnNotifyIconLeftDoubleClick(NotifyIcon sender, RoutedEventArgs e) in D:\GitHub\Violeta\View\MainWindow.xaml.cs:line 53
                   at System.RuntimeMethodHandle.InvokeMethod(Object target, Void** arguments, Signature sig, Boolean isConstructor)
                   at System.Reflection.MethodBaseInvoker.InvokeDirectByRefWithFewArgs(Object obj, Span`1 copyOfArgs, BindingFlags invokeAttr)
                """, ToolTipIcon.Error, clickEvent: () => Toast.Error("User Click Notification"), closeEvent: () => Toast.Information("Notification Closed")
            );
        }
    }


    // ── TreeModelListView ─────────────────────────────────────────

    [ObservableProperty]
    public partial RegistryModel TreeRegistryModel { get; set; } = new();

    [ObservableProperty]
    public partial FileModel TreeFileModel { get; set; } = new();

    [ObservableProperty]
    public partial TreeModelCollection<TreeTestModel> TreeTestModel { get; set; } = CreateTestModel();

    [RelayCommand]
    private void AddTreeTestModel()
    {
        TreeTestModel.Add(new TreeTestModel()
        {
            Column1 = "Test Added " + DateTime.Now,
            Column2 = "Test Added " + DateTime.Now,
            Column3 = "Test Added " + DateTime.Now,
        });

        TreeTestModel.Children[0].Children.Add(new TreeTestModel()
        {
            Column1 = "Test Added " + DateTime.Now,
            Column2 = "Test Added " + DateTime.Now,
            Column3 = "Test Added " + DateTime.Now,
        });

        TreeTestModel.Children[0].Children[0].Children.Add(new TreeTestModel()
        {
            Column1 = "Test Added " + DateTime.Now,
            Column2 = "Test Added " + DateTime.Now,
            Column3 = "Test Added " + DateTime.Now,
        });
    }

    [RelayCommand]
    private void RemoveTreeTestModel()
    {
        if (TreeTestModel.Count > 0)
        {
            TreeTestModel.RemoveAt(0);
        }
    }

    [RelayCommand]
    private void ChangeTreeTestModel()
    {
        if (TreeTestModel.FirstOrDefault() is TreeTestModel model)
        {
            model.Column1 = "Test Changed " + DateTime.Now;
            model.Column2 = "Test Changed " + DateTime.Now;
            model.Column3 = "Test Changed " + DateTime.Now;
        }
    }

    [RelayCommand]
    private void ClearTreeTestModel()
    {
        TreeTestModel.Clear();
    }

    public static TreeModelCollection<TreeTestModel> CreateTestModel()
    {
        return new TreeModelCollection<TreeTestModel>()
        {
            Children = new(
            [
                new()
                {
                    Column1 = "Test 1",
                    Column2 = "Test 1",
                    Column3 = "Test 1",
                    Children = new(
                    [
                        new()
                        {
                            Column1 = "Test 1.1",
                            Column2 = "Test 1.1",
                            Column3 = "Test 1.1",
                            Children = new(
                            [
                                new()
                                {
                                    Column1 = "Test 1.2",
                                    Column2 = "Test 1.2",
                                    Column3 = "Test 1.2",
                                },
                            ]),
                        },
                    ]),
                },
                new()
                {
                    Column1 = "Test 2",
                    Column2 = "Test 2",
                    Column3 = "Test 2",
                }
            ]),
        };
    }

    public static TreeModelCollection<TreeTestModel> CreateTestModel(int count1, int count2, int count3)
    {
        TreeModelCollection<TreeTestModel> model = [];

        for (int i = 0; i < count1; i++)
        {
            TreeTestModel p = new()
            {
                Column1 = "Person A " + i.ToString(),
                Column2 = "Column2 A",
                Column3 = "Column3 A",
            };
            model.Children.Add(p);

            for (int n = 0; n < count2; n++)
            {
                TreeTestModel p2 = new()
                {
                    Column1 = "Person B" + n.ToString(),
                    Column2 = "Column2 B",
                    Column3 = "Column3 B",
                };
                p.Children.Add(p2);

                for (int k = 0; k < count3; k++)
                {
                    p2.Children.Add(new TreeTestModel()
                    {
                        Column1 = "Person C" + k.ToString(),
                        Column2 = "Column2 C",
                        Column3 = "Column3 C",
                    });
                }
            }
        }
        return model;
    }


    // ── ListView ──────────────────────────────────────────────────

    [ObservableProperty]
    public partial ObservableCollection<Staff> StaffList { get; set; } = [];

    [ObservableProperty]
    public partial Staff SelectedStaffItem { get; set; } = null!;

    private void InitNode1Value()
    {
        Staff staff = new()
        {
            Name = "Alice",
            Age = 30,
            Sex = "Male",
            Duty = "Manager",
            IsExpanded = true,
            Path = @"C:\Program Files\nodejs\",
        };
        Staff staff2 = new()
        {
            Name = "Alice1",
            Age = 21,
            Sex = "Male",
            Duty = "Normal",
            IsExpanded = true,
            Path = @"C:\Program Files\nodejs\node.exe",
        };
        Staff staff3 = new()
        {
            Name = "Alice11",
            Age = 21,
            Sex = "Male",
            Duty = "Normal",
            Path = @"D:\UI_MonsterSmallIcon_Eremite_Male_Standard_Crossbow.png",
        };

        staff2.StaffList.Add(staff3);
        staff3 = new Staff()
        {
            Name = "Alice22",
            Age = 21,
            Sex = "Female",
            Duty = "Normal",
            Path = @"C:\Program Files\ASUS\ARMOURY CRATE Service\GameVisualPlugin\GameVisual2.ico",
        };
        staff2.StaffList.Add(staff3);
        staff.StaffList.Add(staff2);
        staff2 = new Staff()
        {
            Name = "Alice2",
            Age = 22,
            Sex = "Female",
            Duty = "Normal"
        };
        staff.StaffList.Add(staff2);
        staff2 = new Staff()
        {
            Name = "Alice3",
            Age = 23,
            Sex = "Female",
            Duty = "Normal"
        };
        staff.StaffList.Add(staff2);
        StaffList.Add(staff);

        staff = new Staff()
        {
            Name = "Bob",
            Age = 31,
            Sex = "Male",
            Duty = "CEO"
        };
        staff2 = new Staff()
        {
            Name = "Bob1",
            Age = 24,
            Sex = "Female",
            Duty = "Normal"
        };
        staff.StaffList.Add(staff2);
        staff2 = new Staff()
        {
            Name = "Bob2",
            Age = 25,
            Sex = "Female",
            Duty = "Normal"
        };
        staff.StaffList.Add(staff2);
        staff2 = new Staff()
        {
            Name = "Bob3",
            Age = 26,
            Sex = "Male",
            Duty = "Normal"
        };
        staff.StaffList.Add(staff2);
        StaffList.Add(staff);

        staff = new Staff()
        {
            Name = "Cyber",
            Age = 32,
            Sex = "Female",
            Duty = "Leader"
        };
        staff2 = new Staff()
        {
            Name = "Cyber1",
            Age = 27,
            Sex = "Female",
            Duty = "Normal"
        };
        staff.StaffList.Add(staff2);
        staff2 = new Staff()
        {
            Name = "Cyber2",
            Age = 28,
            Sex = "Female",
            Duty = "Normal"
        };
        staff.StaffList.Add(staff2);
        StaffList.Add(staff);
    }

    [RelayCommand]
    private void AddNode1Value()
    {
        Staff staff = new()
        {
            Name = "Alice",
            Age = 30,
            Sex = "Male",
            Duty = "Manager",
            IsExpanded = true
        };
        Staff staff2 = new()
        {
            Name = "Alice1",
            Age = 21,
            Sex = "Male",
            Duty = "Normal",
            IsExpanded = true
        };
        Staff staff3 = new()
        {
            Name = "Alice11",
            Age = 21,
            Sex = "Male",
            Duty = "Normal"
        };
        staff2.StaffList.Add(staff3);
        staff.StaffList.Add(staff2);
        StaffList.Add(staff2);
    }

    [RelayCommand]
    private void ChangeNode1Value()
    {
        foreach (Staff staff in StaffList)
        {
            staff.Age += 1;
            staff.Sex = staff.Sex == "Male" ? "Female" : "Male";
        }
    }

    [RelayCommand]
    private void ChangeNode2Value()
    {
        foreach (Staff staff in StaffList)
        {
            foreach (Staff staff2 in staff.StaffList)
            {
                staff2.Age += 1;
                staff2.Sex = staff2.Sex == "Male" ? "Female" : "Male";
            }
        }
    }

    [RelayCommand]
    private void OpenStaff()
    {
        // Use `TreeListView::SelectedItem` here ...
        _ = SelectedStaffItem;
    }


    // ── ExceptionReport ───────────────────────────────────────────

    [RelayCommand]
    private void ShowReport()
    {
        ExceptionReport.Show(new SystemException(
            """
            A critical system error occurred while attempting to perform the requested operation.
            The system entered an unexpected state, possibly due to resource exhaustion,
            incompatible configuration settings, or an internal logic flaw.
            Immediate investigation is required to diagnose and rectify the issue.
            Please check system logs, review recent changes,
            and ensure that the environment meets all necessary requirements.
            If the issue cannot be resolved, escalate to the technical team for further analysis.
            """
            ));
    }

    [RelayCommand]
    private void ThrowException()
    {
        throw new InvalidOperationException("The operation could not be completed because the system encountered an unexpected state. This might be due to incorrect usage of the API or an internal error. Please ensure that all prerequisites are met and the operation is performed under the correct conditions. If the problem persists, consult the documentation or contact support for further assistance.");
    }


    // ── PendingBox ────────────────────────────────────────────────

    [RelayCommand]
    private async Task ShowPendingBoxAsync()
    {
        using IPendingHandler pending = PendingBox.Show();
        await Task.Delay(3000);
    }

    [RelayCommand]
    private async Task ShowPendingBoxWithCancelAsync()
    {
        using IPendingHandler pending = PendingBox.Show("Doing something", "I'm a title", isShowCancel: true);
        await Task.Delay(3000);
    }

    [Obsolete("Under development")]
    [RelayCommand]
    private async Task ShowAsyncPendingBoxAsync()
    {
        using STAThread<IPendingHandler> pending = PendingBox.ShowAsync();
        await Task.Delay(3000);
    }


    // ── Drawer ────────────────────────────────────────────────────

    [ObservableProperty]
    public partial bool IsOpenOfLeftDrawer { get; set; } = false;

    [ObservableProperty]
    public partial bool IsOpenOfTopDrawer { get; set; } = false;

    [ObservableProperty]
    public partial bool IsOpenOfRightDrawer { get; set; } = false;

    [ObservableProperty]
    public partial bool IsOpenOfBottomDrawer { get; set; } = false;

    [RelayCommand]
    private void ShowDrawer(string placementString)
    {
        if (placementString == "Left")
            IsOpenOfLeftDrawer = !IsOpenOfLeftDrawer;
        else if (placementString == "Top")
            IsOpenOfTopDrawer = !IsOpenOfTopDrawer;
        else if (placementString == "Right")
            IsOpenOfRightDrawer = !IsOpenOfRightDrawer;
        else if (placementString == "Bottom")
            IsOpenOfBottomDrawer = !IsOpenOfBottomDrawer;
    }


    // ── ShellWindow ───────────────────────────────────────────────

    [RelayCommand]
    private void ShowShellWindow()
    {
        //SecondWindow window = new()
        SecondWindow window = new()
        {
            AllowsTransparency = false,
        };

        window.Show();
    }


    // ── Hyperlink ─────────────────────────────────────────────────

    private void Hyperlink_RequestNavigate(object? sender, RequestNavigateEventArgs e)
    {
    }
}

public partial class RegistryModel : ITreeModel
{
    public IEnumerable GetChildren(object parent)
    {
        if (parent == null)
        {
            yield return Registry.ClassesRoot;
            yield return Registry.CurrentUser;
            yield return Registry.LocalMachine;
            yield return Registry.Users;
            yield return Registry.CurrentConfig;
        }
        else if (parent is RegistryKey key)
        {
            foreach (var name in key.GetSubKeyNames())
            {
                RegistryKey? subKey = null;
                try
                {
                    subKey = key.OpenSubKey(name);
                }
                catch
                {
                }

                if (subKey != null)
                {
                    yield return subKey;
                }
            }

            foreach (var name in key.GetValueNames())
            {
                yield return new RegValue()
                {
                    Name = name,
                    Data = key.GetValue(name),
                    Kind = key.GetValueKind(name)
                };
            }
        }
    }

    public bool HasChildren(object parent)
    {
        return parent is RegistryKey;
    }
}

public class FileModel : ITreeModel
{
    public IEnumerable GetChildren(object? parent)
    {
        if (parent == null)
        {
            // Return the root directory
            yield return new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
        }
        else if (parent is DirectoryInfo directory)
        {
            // Return subdirectories
            foreach (var dir in directory.GetDirectories())
            {
                yield return dir;
            }

            // Return files
            foreach (var file in directory.GetFiles())
            {
                yield return file;
            }
        }
    }

    public bool HasChildren(object parent)
    {
        if (parent is DirectoryInfo directory)
        {
            try
            {
                // Check if there are subdirectories or files
                return directory.GetDirectories().Any() || directory.GetFiles().Any();
            }
            catch (UnauthorizedAccessException)
            {
                // Catch access denied exception and return false
                return false;
            }
            catch (Exception ex)
            {
                // Handle other possible exceptions
                Console.WriteLine($"An error occurred: {ex.Message}");
                return false;
            }
        }
        return false;
    }
}

public struct RegValue
{
    public string Name { get; set; }
    public object? Data { get; set; }
    public RegistryValueKind Kind { get; set; }
}

[ObservableObject]
public partial class TreeTestModel : TreeModelObject<TreeTestModel>
{
    [ObservableProperty]
    public partial string? Column1 { get; set; }

    [ObservableProperty]
    public partial string? Column2 { get; set; }

    [ObservableProperty]
    public partial string? Column3 { get; set; }

    [ObservableProperty]
    public partial bool IsChecked { get; set; } = false;
}

public partial class Staff : ObservableObject
{
    [ObservableProperty]
    public partial string Name { get; set; } = null!;

    [ObservableProperty]
    public partial int Age { get; set; }

    [ObservableProperty]
    public partial string Sex { get; set; } = null!;

    [ObservableProperty]
    public partial string Duty { get; set; } = null!;

    [ObservableProperty]
    public partial bool IsChecked { get; set; } = true;

    [ObservableProperty]
    public partial bool IsSelected { get; set; } = false;

    [ObservableProperty]
    public partial bool IsExpanded { get; set; } = false;

    [ObservableProperty]
    public partial string Path { get; set; } = null!;

    [ObservableProperty]
    public partial ObservableCollection<Staff> StaffList { get; set; } = [];

    [RelayCommand]
    private void OnClick()
    {
    }
}

/// <summary>Simple view-model node for TreeComboBox demo.</summary>
public sealed class TreeComboBoxNode(string name, System.Collections.Generic.List<TreeComboBoxNode>? children = null)
{
    public string Name { get; } = name;
    public System.Collections.Generic.List<TreeComboBoxNode> Children { get; } = children ?? [];
}

/// <summary>View-model item for Timeline demo.</summary>
public class TimelineItemViewModel
{
    public DateTime Time { get; set; }
    public string? Header { get; set; }
    public string? Description { get; set; }
    public TimelineItemType ItemType { get; set; }
}
