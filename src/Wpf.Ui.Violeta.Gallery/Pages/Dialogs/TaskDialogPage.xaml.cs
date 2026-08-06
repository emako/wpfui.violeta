using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using Wpf.Ui.Violeta.Win32;
using Wpf.Ui.Violeta.Gallery.Globalization;
using LiteObservableLanguages;

namespace Wpf.Ui.Violeta.Gallery.Pages.Dialogs;

public partial class TaskDialogPage : Wpf.Ui.Violeta.Controls.Page
{
    public TaskDialogPage()
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
            TaskDialogResultText.Text = LangKeys.Format_ThemeSwitched.Tr(tag);
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
            ? LangKeys.Sample_395d500fbf.Tr()
            : LangKeys.Format_ResultDetail.Tr(clicked.ButtonType, string.IsNullOrEmpty(clicked.Text) ? string.Empty : LangKeys.Format_DashDetail.Tr(clicked.Text));
    }

    private static TaskDialogButton? ShowInformationTaskDialog(nint owner)
    {
        using TaskDialog dialog = new()
        {
            WindowTitle = LangKeys.Sample_eb38190f53.Tr(),
            MainInstruction = LangKeys.Sample_ed377f66a0.Tr(),
            Content = LangKeys.Sample_c693b80589.Tr(),
            MainIcon = TaskDialogIcon.Information,
        };
        dialog.Buttons.Add(new TaskDialogButton(ButtonType.Ok));
        return dialog.ShowDialog(owner);
    }

    private static TaskDialogButton? ShowWarningTaskDialog(nint owner)
    {
        using TaskDialog dialog = new()
        {
            WindowTitle = LangKeys.Sample_2b68259daf.Tr(),
            MainInstruction = LangKeys.Sample_180108e19c.Tr(),
            Content = LangKeys.Sample_4ed22fb344.Tr(),
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
            WindowTitle = LangKeys.Sample_2cf6355daa.Tr(),
            MainInstruction = LangKeys.Sample_234601f1a4.Tr(),
            Content = LangKeys.Sample_c8a01628d6.Tr(),
            MainIcon = TaskDialogIcon.Error,
            Footer = LangKeys.Sample_b98522c6a1.Tr(),
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
            WindowTitle = LangKeys.Sample_91f1130627.Tr(),
            MainInstruction = LangKeys.Sample_4bf5433ad8.Tr(),
            Content = LangKeys.Sample_7fb644b8b6.Tr(),
            MainIcon = TaskDialogIcon.Information,
            ButtonStyle = TaskDialogButtonStyle.CommandLinks,
            AllowDialogCancellation = true,
        };
        dialog.Buttons.Add(new TaskDialogButton(ButtonType.Custom)
        {
            Text = LangKeys.Sample_27ca568be2.Tr(),
            CommandLinkNote = LangKeys.Sample_c81e7712be.Tr(),
            Default = true,
        });
        dialog.Buttons.Add(new TaskDialogButton(ButtonType.Custom)
        {
            Text = LangKeys.Sample_132c5cdcce.Tr(),
            CommandLinkNote = LangKeys.Sample_24dfe773c7.Tr(),
        });
        dialog.Buttons.Add(new TaskDialogButton(ButtonType.Custom)
        {
            Text = LangKeys.Sample_625fb26b4b.Tr(),
            CommandLinkNote = LangKeys.Sample_e4568d8868.Tr(),
        });
        return dialog.ShowDialog(owner);
    }

    private static TaskDialogButton? ShowExpandedTaskDialog(nint owner)
    {
        using TaskDialog dialog = new()
        {
            WindowTitle = LangKeys.Sample_629dd8a6ea.Tr(),
            MainInstruction = LangKeys.Sample_484693ecc7.Tr(),
            Content = LangKeys.Sample_e2e51ea8f9.Tr(),
            MainIcon = TaskDialogIcon.Information,
            VerificationText = LangKeys.Sample_fb54df5d84.Tr(),
            ExpandedInformation = LangKeys.Sample_76b983b8e6.Tr(),
            ExpandedControlText = LangKeys.Sample_4d4d95676e.Tr(),
            CollapsedControlText = LangKeys.Sample_7053881e1b.Tr(),
            Footer = LangKeys.Sample_85ed85c69f.Tr(),
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
            WindowTitle = LangKeys.Sample_7be4dfd564.Tr(),
            MainInstruction = LangKeys.Sample_da479fcbb1.Tr(),
            Content = LangKeys.Sample_eed4f2841b.Tr(),
            MainIcon = TaskDialogIcon.Information,
        };
        dialog.RadioButtons.Add(new TaskDialogRadioButton { Text = LangKeys.Sample_3407d84a3a.Tr(), Checked = true });
        dialog.RadioButtons.Add(new TaskDialogRadioButton { Text = LangKeys.Sample_9bcbd65034.Tr() });
        dialog.RadioButtons.Add(new TaskDialogRadioButton { Text = LangKeys.Sample_0dd1930c12.Tr() });
        dialog.Buttons.Add(new TaskDialogButton(ButtonType.Ok));
        dialog.Buttons.Add(new TaskDialogButton(ButtonType.Cancel));
        return dialog.ShowDialog(owner);
    }

    private static TaskDialogButton? ShowProgressTaskDialog(nint owner)
    {
        using TaskDialog dialog = new()
        {
            WindowTitle = LangKeys.Sample_d02ebc627c.Tr(),
            MainInstruction = LangKeys.Sample_72628b3afc.Tr(),
            Content = LangKeys.Sample_10129a2fd7.Tr(),
            MainIcon = TaskDialogIcon.Information,
            ProgressBarStyle = ProgressBarStyle.MarqueeProgressBar,
            ProgressBarMarqueeAnimationSpeed = 60,
        };
        dialog.Buttons.Add(new TaskDialogButton(ButtonType.Cancel));
        return dialog.ShowDialog(owner);
    }
}
