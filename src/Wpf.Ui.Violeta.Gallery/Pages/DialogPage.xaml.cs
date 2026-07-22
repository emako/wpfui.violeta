using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using Wpf.Ui.Violeta.Controls;
using Wpf.Ui.Violeta.Win32;
using ContentDialog = Wpf.Ui.Violeta.Controls.ContentDialog;
using ContentDialogButton = Wpf.Ui.Violeta.Controls.ContentDialogButton;
using MessageBox = Wpf.Ui.Violeta.Controls.MessageBox;

namespace Wpf.Ui.Violeta.Gallery.Pages;

public partial class DialogPage : Wpf.Ui.Violeta.Controls.Page
{
    public DialogPage()
    {
        InitializeComponent();
    }

    private nint OwnerHandle
    {
        get
        {
            Window? window = Window.GetWindow(this);
            return window is null ? 0 : new WindowInteropHelper(window).Handle;
        }
    }

    private async void ShowContentDialog_Click(object sender, RoutedEventArgs e)
    {
        ContentDialog dialog = new()
        {
            Title = "示例 ContentDialog",
            Content = "这是 Violeta 提供的 ContentDialog，支持主按钮、次按钮和关闭按钮。",
            CloseButtonText = "关闭",
            PrimaryButtonText = "确认",
            SecondaryButtonText = "稍后",
            DefaultButton = ContentDialogButton.Primary,
        };

        var result = await dialog.ShowAsync();
        ContentDialogResultText.Text = $"结果：{result}";
    }

    private async void ShowContentDialogCustom_Click(object sender, RoutedEventArgs e)
    {
        var stack = new StackPanel { Margin = new Thickness(0, 8, 0, 0) };
        stack.Children.Add(new TextBlock { Text = "请输入您的名称：", Margin = new Thickness(0, 0, 0, 8) });
        stack.Children.Add(new Wpf.Ui.Controls.TextBox { PlaceholderText = "名称...", MinWidth = 200 });

        ContentDialog dialog = new()
        {
            Title = "自定义内容",
            Content = stack,
            CloseButtonText = "取消",
            PrimaryButtonText = "确定",
            DefaultButton = ContentDialogButton.Primary,
        };

        var result = await dialog.ShowAsync();
        ContentDialogResultText.Text = $"结果：{result}";
    }

    private void ShowMessageBox_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn)
        {
            return;
        }

        var tag = btn.Tag?.ToString();
        System.Windows.MessageBoxResult result;

        result = tag switch
        {
            "Information" => MessageBox.Information("这是一条信息消息。"),
            "Warning" => MessageBox.Warning("请注意，这是一条警告。"),
            "Question" => MessageBox.Question("这是一个问题，您确认吗？"),
            "Error" => MessageBox.Error("发生了一个错误，请稍后重试。"),
            _ => System.Windows.MessageBoxResult.None,
        };

        MessageBoxResultText.Text = $"结果：{result}";
    }

    private async void ShowPendingBox_Click(object sender, RoutedEventArgs e)
    {
        using IPendingHandler pending = PendingBox.Show("正在加载数据...", "请稍候");
        await Task.Delay(3000);
    }

    private async void ShowPendingBoxWithCancel_Click(object sender, RoutedEventArgs e)
    {
        using IPendingHandler pending = PendingBox.Show("正在执行操作，请稍候...", "处理中", isShowCancel: true);
        await Task.Delay(3000);
    }

    private void ShowTaskDialog_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn)
        {
            return;
        }

        var tag = btn.Tag?.ToString();
        nint owner = OwnerHandle;

        if (tag is "System" or "Dark" or "Light")
        {
            TaskDialog.SetTheme(tag switch
            {
                "Dark" => TaskDialogTheme.Dark,
                "Light" => TaskDialogTheme.Light,
                _ => TaskDialogTheme.System,
            });
            TaskDialogResultText.Text = $"主题已切换：{tag}";
            return;
        }

        TaskDialogButton? clicked = tag switch
        {
            "Information" => ShowInformationTaskDialog(owner),
            "Warning" => ShowWarningTaskDialog(owner),
            "Error" => ShowErrorTaskDialog(owner),
            "CommandLinks" => ShowCommandLinksTaskDialog(owner),
            "Expanded" => ShowExpandedTaskDialog(owner),
            "Radio" => ShowRadioTaskDialog(owner),
            "Progress" => ShowProgressTaskDialog(owner),
            _ => null,
        };

        TaskDialogResultText.Text = clicked is null
            ? "结果：已取消或关闭"
            : $"结果：{clicked.ButtonType}{(string.IsNullOrEmpty(clicked.Text) ? string.Empty : $" — {clicked.Text}")}";
    }

    private static TaskDialogButton? ShowInformationTaskDialog(nint owner)
    {
        using TaskDialog dialog = new()
        {
            WindowTitle = "TaskDialog — 信息",
            MainInstruction = "这是一个信息 TaskDialog",
            Content = "支持暗色主题，通过 DWM + SetWindowTheme + 窗口子类化实现。",
            MainIcon = TaskDialogIcon.Information,
        };
        dialog.Buttons.Add(new TaskDialogButton(ButtonType.Ok));
        return dialog.ShowDialog(owner);
    }

    private static TaskDialogButton? ShowWarningTaskDialog(nint owner)
    {
        using TaskDialog dialog = new()
        {
            WindowTitle = "TaskDialog — 警告",
            MainInstruction = "请注意以下事项",
            Content = "这是一个带页脚图标和超链接的警告对话框。\n\n访问 <A HREF=\"https://github.com/emako/wpfui.violeta\">WPF UI Violeta</A> 了解更多。",
            MainIcon = TaskDialogIcon.Warning,
            FooterIcon = TaskDialogIcon.Shield,
            EnableHyperlinks = true,
        };
        dialog.Buttons.Add(new TaskDialogButton(ButtonType.Ok));
        dialog.Buttons.Add(new TaskDialogButton(ButtonType.Cancel));
        dialog.HyperlinkClicked += (_, args) =>
        {
            if (!string.IsNullOrEmpty(args.Href))
            {
                Process.Start(new ProcessStartInfo(args.Href) { UseShellExecute = true });
            }
        };
        return dialog.ShowDialog(owner);
    }

    private static TaskDialogButton? ShowErrorTaskDialog(nint owner)
    {
        using TaskDialog dialog = new()
        {
            WindowTitle = "TaskDialog — 错误",
            MainInstruction = "发生了严重错误",
            Content = "演示错误图标与重试 / 取消按钮。",
            MainIcon = TaskDialogIcon.Error,
            Footer = "错误代码：0x80004005",
            FooterIcon = TaskDialogIcon.Shield,
        };
        dialog.Buttons.Add(new TaskDialogButton(ButtonType.Retry));
        dialog.Buttons.Add(new TaskDialogButton(ButtonType.Cancel));
        return dialog.ShowDialog(owner);
    }

    private static TaskDialogButton? ShowCommandLinksTaskDialog(nint owner)
    {
        using TaskDialog dialog = new()
        {
            WindowTitle = "TaskDialog — 命令链接",
            MainInstruction = "请选择一个操作",
            Content = "此对话框使用命令链接按钮。",
            MainIcon = TaskDialogIcon.Information,
            ButtonStyle = TaskDialogButtonStyle.CommandLinks,
            AllowDialogCancellation = true,
        };
        dialog.Buttons.Add(new TaskDialogButton(ButtonType.Custom)
        {
            Text = "继续",
            CommandLinkNote = "继续当前操作",
            Default = true,
        });
        dialog.Buttons.Add(new TaskDialogButton(ButtonType.Custom)
        {
            Text = "重试",
            CommandLinkNote = "从头开始操作",
        });
        dialog.Buttons.Add(new TaskDialogButton(ButtonType.Custom)
        {
            Text = "取消",
            CommandLinkNote = "中止并返回",
        });
        return dialog.ShowDialog(owner);
    }

    private static TaskDialogButton? ShowExpandedTaskDialog(nint owner)
    {
        using TaskDialog dialog = new()
        {
            WindowTitle = "TaskDialog — 可展开信息",
            MainInstruction = "可展开的任务对话框",
            Content = "点击展开控件查看更多详细信息。",
            MainIcon = TaskDialogIcon.Information,
            VerificationText = "不再显示",
            ExpandedInformation = "此处可放置技术细节、堆栈跟踪、日志片段或链接。",
            ExpandedControlText = "隐藏详细信息",
            CollapsedControlText = "显示详细信息",
            Footer = "切换主题：<A HREF=\"dark\">深色</A>  |  <A HREF=\"light\">浅色</A>",
            FooterIcon = TaskDialogIcon.Shield,
            EnableHyperlinks = true,
            ExpandFooterArea = true,
        };
        dialog.Buttons.Add(new TaskDialogButton(ButtonType.Ok));
        dialog.HyperlinkClicked += (_, args) =>
        {
            if (args.Href == "dark") TaskDialog.SetTheme(TaskDialogTheme.Dark);
            if (args.Href == "light") TaskDialog.SetTheme(TaskDialogTheme.Light);
        };
        return dialog.ShowDialog(owner);
    }

    private static TaskDialogButton? ShowRadioTaskDialog(nint owner)
    {
        using TaskDialog dialog = new()
        {
            WindowTitle = "TaskDialog — 单选按钮",
            MainInstruction = "选择安装方式",
            Content = "请选择希望使用的安装选项。",
            MainIcon = TaskDialogIcon.Information,
        };
        dialog.RadioButtons.Add(new TaskDialogRadioButton { Text = "典型安装", Checked = true });
        dialog.RadioButtons.Add(new TaskDialogRadioButton { Text = "自定义安装" });
        dialog.RadioButtons.Add(new TaskDialogRadioButton { Text = "完整安装" });
        dialog.Buttons.Add(new TaskDialogButton(ButtonType.Ok));
        dialog.Buttons.Add(new TaskDialogButton(ButtonType.Cancel));
        return dialog.ShowDialog(owner);
    }

    private static TaskDialogButton? ShowProgressTaskDialog(nint owner)
    {
        using TaskDialog dialog = new()
        {
            WindowTitle = "TaskDialog — 进度条",
            MainInstruction = "正在处理…",
            Content = "此对话框演示跑马灯进度条。",
            MainIcon = TaskDialogIcon.Information,
            ProgressBarStyle = ProgressBarStyle.MarqueeProgressBar,
            ProgressBarMarqueeAnimationSpeed = 60,
        };
        dialog.Buttons.Add(new TaskDialogButton(ButtonType.Cancel));
        return dialog.ShowDialog(owner);
    }

    private void ShowNativeMessageBox_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn)
        {
            return;
        }

        nint owner = OwnerHandle;
        var tag = btn.Tag?.ToString();

        NativeMessageBoxResult result = tag switch
        {
            "OK" => NativeMessageBox.Show(
                owner,
                "这是一个原生 Win32 MessageBox。",
                "NativeMessageBox — 确定",
                NativeMessageBoxButton.OK,
                NativeMessageBoxImage.Information),
            "YesNo" => NativeMessageBox.Show(
                owner,
                "是否继续？",
                "NativeMessageBox — 是/否",
                NativeMessageBoxButton.YesNo,
                NativeMessageBoxImage.Question,
                NativeMessageBoxResult.Yes),
            "OKCancel" => NativeMessageBox.Show(
                owner,
                "可能需要您的注意。\n点击确定继续。",
                "NativeMessageBox — 确定/取消",
                NativeMessageBoxButton.OKCancel,
                NativeMessageBoxImage.Warning,
                NativeMessageBoxResult.OK),
            "YesNoCancel" => NativeMessageBox.Show(
                owner,
                "关闭前是否保存更改？",
                "NativeMessageBox — 是/否/取消",
                NativeMessageBoxButton.YesNoCancel,
                NativeMessageBoxImage.Question,
                NativeMessageBoxResult.Cancel),
            _ => NativeMessageBoxResult.None,
        };

        NativeMessageBoxResultText.Text = $"结果：{result}";
    }
}
