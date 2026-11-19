using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Wpf.Ui.Violeta.Controls;
using VioletaToolBar = Wpf.Ui.Violeta.Controls.ToolBar;
using WpfUiDropDownButton = Wpf.Ui.Controls.DropDownButton;

namespace ToolBarOverflowRepro;

/// <summary>
/// Repro for nested popups inside the <see cref="VioletaToolBar"/> overflow flyout when the toolbar is hidden
/// (tab switch or in-tab navigation) while a nested popup is open.
/// Arguments: --auto runs every case with real mouse input; --case=&lt;name&gt; runs cases whose name contains it.
/// </summary>
public partial class MainWindow : ShellWindow
{
    private readonly StringBuilder _log = new();
    private readonly string _logPath = Path.Combine(Path.GetTempPath(), "toolbar-overflow-repro.log");
    private readonly Stopwatch _clock = Stopwatch.StartNew();
    private readonly List<PopupCase> _cases;

    private VioletaToolBar Bar => pageTools.toolBar;

    public MainWindow()
    {
        InitializeComponent();
        tabStrip.SelectionChanged += OnTabSelectionChanged;
        _cases = CreateCases();

        Bar.IsVisibleChanged += (_, e) => Log($"[event] toolbar IsVisible {e.OldValue} -> {e.NewValue}");
        DependencyPropertyDescriptor.FromProperty(VioletaToolBar.IsOverflowOpenProperty, typeof(VioletaToolBar))
            .AddValueChanged(Bar, (_, _) => Log($"[event] IsOverflowOpen={Bar.IsOverflowOpen}"));

        var timer = new DispatcherTimer(TimeSpan.FromMilliseconds(200), DispatcherPriority.Background, (_, _) => textStatus.Text = StatusText(), Dispatcher);
        timer.Start();

        string[] args = Environment.GetCommandLineArgs();
        string? filter = args.FirstOrDefault(a => a.StartsWith("--case=", StringComparison.OrdinalIgnoreCase))?.Substring("--case=".Length);
        if (filter is not null || args.Contains("--auto", StringComparer.OrdinalIgnoreCase))
        {
            ContentRendered += async (_, _) => await RunAutoAsync(filter);
        }
    }

    private void OnTabSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UIElement target = tabStrip.SelectedItem switch
        {
            TabStripItem item when ReferenceEquals(item, tabHome) => pageHome,
            TabStripItem item when ReferenceEquals(item, tabSettings) => pageSettings,
            _ => pageTools,
        };

        Log($"[event] show {((FrameworkElement)target).Name}");
        foreach (UIElement child in pageHost.Children)
        {
            child.Visibility = ReferenceEquals(child, target) ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private List<PopupCase> CreateCases()
    {
        ToolsPage t = pageTools;
        var dropDownMenu = (ContextMenu)t.dropDownButton.Flyout!;
        var splitMenu = (ContextMenu)t.splitButton.Flyout!;
        var uiMenu = (ContextMenu)t.uiDropDownButton.Flyout!;

        return
        [
            new(
                "DropDownButton",
                () => t.dropDownButton,
                () => IsShown(dropDownMenu),
                () => $"IsDropDownOpen={t.dropDownButton.IsDropDownOpen}",
                () =>
                {
                    dropDownMenu.IsOpen = false;
                    t.dropDownButton.SetCurrentValue(DropDownButton.IsDropDownOpenProperty, false);
                }),
            new(
                "SplitButton",
                () => FindPart(t.splitButton, "PART_ToggleButton"),
                () => IsShown(splitMenu),
                () => $"IsDropDownOpen={t.splitButton.IsDropDownOpen}",
                () =>
                {
                    splitMenu.IsOpen = false;
                    t.splitButton.SetCurrentValue(SplitButton.IsDropDownOpenProperty, false);
                }),
            new(
                "ComboBox",
                () => t.comboBox,
                () => IsShown(FindPopup(t.comboBox)),
                () => $"IsDropDownOpen={t.comboBox.IsDropDownOpen}",
                () => t.comboBox.IsDropDownOpen = false),
            new(
                "ToggleComboBox",
                () => FindPart(t.toggleComboBox, "PART_ToggleButton"),
                () => IsShown(FindPopup(t.toggleComboBox)),
                () => $"IsDropDownOpen={t.toggleComboBox.IsDropDownOpen}",
                () => t.toggleComboBox.IsDropDownOpen = false),
            new(
                "PopConfirmButton",
                () => t.popConfirmButton,
                () => IsShown(GetField(t.popConfirmButton, "_popup") as Popup),
                () => $"IsConfirmOpen={t.popConfirmButton.IsConfirmOpen}",
                () => t.popConfirmButton.CloseConfirm()),
            new(
                "WpfUiDropDownButton",
                () => t.uiDropDownButton,
                () => IsShown(uiMenu),
                () => $"IsDropDownOpen={t.uiDropDownButton.GetValue(GetDependencyProperty(typeof(WpfUiDropDownButton), "IsDropDownOpen"))}",
                () =>
                {
                    uiMenu.IsOpen = false;
                    t.uiDropDownButton.SetCurrentValue(GetDependencyProperty(typeof(WpfUiDropDownButton), "IsDropDownOpen"), false);
                }),
        ];
    }

    private async Task RunAutoAsync(string? filter)
    {
        var results = new List<string>();
        try
        {
            Topmost = true;
            Activate();
            await Settle(800);
            Log($"Wpf.Ui.Violeta: {typeof(VioletaToolBar).Assembly.Location}");

            if (Bar.Template?.FindName("PART_OverflowButton", Bar) is not FrameworkElement overflow)
            {
                Log("PART_OverflowButton not found");
                return;
            }

            foreach (PopupCase item in _cases.Where(c => Matches(filter, c.Name)))
            {
                results.Add($"{item.Name,-20} | switch tab       | {await RunTabSwitchCaseAsync(item, overflow)}");
            }

            if (Matches(filter, "Navigate"))
            {
                results.Add($"{"DropDownButton",-20} | navigate in tab  | {await RunNavigateCaseAsync(_cases[0], overflow)}");
            }
        }
        catch (Exception ex)
        {
            Log("EX " + ex);
        }
        finally
        {
            Log("######## Summary");
            foreach (string line in results)
            {
                Log("  " + line);
            }

            File.WriteAllText(_logPath, _log.ToString());
            Application.Current.Shutdown();
        }
    }

    private async Task<string> RunTabSwitchCaseAsync(PopupCase item, FrameworkElement overflow)
    {
        Log($"######## {item.Name}: open popup -> Settings tab -> Tools tab -> open again");
        await ResetAsync();
        if (!await OpenOverflowAsync(overflow))
        {
            return "SKIP: overflow did not open";
        }

        await Click(item.ClickTarget(), $"{item.Name} (first open)");
        Step(item, "after first open");
        if (!item.IsShown())
        {
            return "SKIP: popup did not open on the first click";
        }

        int switchClicks = await ClickTabUntilSelectedAsync(tabSettings);
        await Settle(800);
        Step(item, "on Settings");

        await ClickTabUntilSelectedAsync(tabTools);
        await Settle(600);
        Step(item, "back on Tools");

        return await ReopenAsync(item, overflow, $"Settings tab took {switchClicks} click(s)");
    }

    private async Task<string> RunNavigateCaseAsync(PopupCase item, FrameworkElement overflow)
    {
        Log($"######## {item.Name}: open menu -> Open Export Page -> Back -> open again");
        await ResetAsync();
        if (!await OpenOverflowAsync(overflow))
        {
            return "SKIP: overflow did not open";
        }

        await Click(item.ClickTarget(), $"{item.Name} (first open)");
        if (!item.IsShown())
        {
            return "SKIP: popup did not open on the first click";
        }

        await Click(pageTools.menuItemOpenExportPage, "Open Export Page");
        await Settle(800);
        Step(item, "on Export page");

        await Click(pageTools.buttonBack, "Back");
        await Settle(600);
        Step(item, "back on Tools home");

        return await ReopenAsync(item, overflow, "navigated inside the tab");
    }

    private async Task<string> ReopenAsync(PopupCase item, FrameworkElement overflow, string note)
    {
        for (int i = 1; i <= 3; i++)
        {
            if (!await OpenOverflowAsync(overflow))
            {
                return $"FAIL: overflow did not reopen ({note})";
            }

            await Click(item.ClickTarget(), $"{item.Name} (reopen #{i})");
            Step(item, $"after reopen click #{i}");
            if (item.IsShown())
            {
                return i == 1 ? $"PASS ({note})" : $"WARN: opened only on click #{i} ({note})";
            }
        }

        return $"FAIL: still closed after 3 clicks ({note})";
    }

    private async Task ResetAsync()
    {
        foreach (PopupCase item in _cases)
        {
            item.Reset();
        }

        await Settle(300);
        Bar.IsOverflowOpen = false;
        tabStrip.SelectedItem = tabTools;
        pageTools.Navigate(pageTools.toolsHome);
        await Settle(400);
    }

    private async Task<bool> OpenOverflowAsync(FrameworkElement overflow)
    {
        if (!Bar.IsOverflowOpen)
        {
            await Click(overflow, "... (overflow button)");
        }

        return Bar.IsOverflowOpen;
    }

    private async Task<int> ClickTabUntilSelectedAsync(TabStripItem tab)
    {
        int clicks = 0;
        while (!tab.IsSelected && clicks < 3)
        {
            clicks++;
            await Click(tab, $"{tab.Content} tab #{clicks}");
        }

        return clicks;
    }

    private void Step(PopupCase item, string label)
        => Log($"== {label}: tab={(tabStrip.SelectedItem as TabStripItem)?.Content} toolbarVisible={Bar.IsVisible} overflow={Bar.IsOverflowOpen} | {item.Name}: shown={item.IsShown()} {item.State()} | captured={Mouse.Captured?.GetType().Name ?? "null"}");

    private string StatusText()
    {
        var builder = new StringBuilder();
        builder.Append($"tab={(tabStrip.SelectedItem as TabStripItem)?.Content} toolbarVisible={Bar.IsVisible} IsOverflowOpen={Bar.IsOverflowOpen} captured={Mouse.Captured?.GetType().Name ?? "null"}");
        foreach (PopupCase item in _cases)
        {
            builder.Append($"\n{item.Name,-20} shown={item.IsShown(),-5} {item.State()}");
        }

        return builder.ToString();
    }

    private static bool Matches(string? filter, string name)
        => filter is null || name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;

    private static bool IsShown(ContextMenu? menu)
        => menu is { IsOpen: true } && PresentationSource.FromVisual(menu) is not null;

    private static bool IsShown(Popup? popup)
        => popup is { IsOpen: true, Child: Visual child } && PresentationSource.FromVisual(child) is not null;

    private static FrameworkElement? FindPart(Control control, string name)
        => control.Template?.FindName(name, control) as FrameworkElement;

    private static Popup? FindPopup(DependencyObject root)
    {
        var pending = new Queue<DependencyObject>();
        pending.Enqueue(root);
        while (pending.Count > 0)
        {
            DependencyObject current = pending.Dequeue();
            if (current is Popup popup)
            {
                return popup;
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(current); i++)
            {
                pending.Enqueue(VisualTreeHelper.GetChild(current, i));
            }
        }

        return null;
    }

    private static object? GetField(object target, string name)
    {
        for (Type? type = target.GetType(); type is not null; type = type.BaseType)
        {
            if (type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic) is { } field)
            {
                return field.GetValue(target);
            }
        }

        return null;
    }

    private static DependencyProperty GetDependencyProperty(Type type, string name)
        => DependencyPropertyDescriptor.FromName(name, type, type).DependencyProperty;

    private async Task Click(FrameworkElement? element, string label)
    {
        if (element is null || !element.IsVisible || element.ActualWidth < 1 || PresentationSource.FromVisual(element) is null)
        {
            Log($">> click {label}: SKIP (element not visible)");
            return;
        }

        Point center = element.PointToScreen(new Point(element.ActualWidth / 2, element.ActualHeight / 2));
        Log($">> click {label} at {center.X:0},{center.Y:0}");
        SetCursorPos((int)center.X, (int)center.Y);
        await Task.Delay(120);
        mouse_event(LeftDown, 0, 0, 0, UIntPtr.Zero);
        await Task.Delay(80);
        mouse_event(LeftUp, 0, 0, 0, UIntPtr.Zero);
        await Settle(450);
    }

    private async Task Settle(int milliseconds)
    {
        await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
        await Task.Delay(milliseconds);
        await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
    }

    private void Log(string message) => _log.AppendLine($"{_clock.ElapsedMilliseconds,6} {message}");

    private sealed class PopupCase(string name, Func<FrameworkElement?> clickTarget, Func<bool> isShown, Func<string> state, Action reset)
    {
        public string Name { get; } = name;

        public Func<FrameworkElement?> ClickTarget { get; } = clickTarget;

        public Func<bool> IsShown { get; } = isShown;

        public Func<string> State { get; } = state;

        public Action Reset { get; } = reset;
    }

    private const uint LeftDown = 0x0002;
    private const uint LeftUp = 0x0004;

    [DllImport("user32.dll")]
    private static extern bool SetCursorPos(int x, int y);

    [DllImport("user32.dll")]
    private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);
}
