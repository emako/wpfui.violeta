using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using Wpf.Ui.Violeta.Win32.NativeDialog;

namespace Wpf.Ui.Violeta.Gallery.Pages.Dialogs;

public partial class CredentialDialogPage : Wpf.Ui.Violeta.Controls.Page
{
    public CredentialDialogPage()
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

    private string Target => string.IsNullOrWhiteSpace(TargetTextBox.Text)
        ? "Wpf.Ui.Violeta.Gallery.Sample"
        : TargetTextBox.Text.Trim();

    private void ShowCredentialDialog_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn)
        {
            return;
        }

        var tag = btn.Tag?.ToString();
        nint owner = OwnerHandle;

        try
        {
            switch (tag)
            {
                case "Prompt":
                    PromptAndReport(owner, showSaveCheckBox: false, useAppCache: false);
                    return;

                case "SaveChecked":
                    PromptAndReport(owner, showSaveCheckBox: true, useAppCache: false);
                    return;

                case "AppCache":
                    PromptAndReport(owner, showSaveCheckBox: true, useAppCache: true);
                    return;

                case "Retrieve":
                    RetrieveAndReport();
                    return;

                case "Delete":
                    DeleteAndReport();
                    return;
            }
        }
        catch (Exception ex)
        {
            CredentialDialogResultText.Text = $"结果：异常 — {ex.Message}";
        }
    }

    private void PromptAndReport(nint owner, bool showSaveCheckBox, bool useAppCache)
    {
        var dialog = new CredentialDialog
        {
            Target = Target,
            MainInstruction = "请输入凭据",
            Content = "用于演示 Windows Security 凭据对话框。",
            ShowSaveCheckBox = showSaveCheckBox,
            UseApplicationInstanceCredentialCache = useAppCache,
        };

        bool accepted = dialog.ShowDialog(owner);
        if (!accepted)
        {
            CredentialDialogResultText.Text = "结果：用户取消了对话框。";
            return;
        }

        dialog.ConfirmCredentials(confirm: true);

        CredentialDialogResultText.Text = dialog.IsStoredCredential
            ? $"结果：命中缓存凭据 — 用户名：{dialog.UserName}"
            : $"结果：用户名：{dialog.UserName}，是否勾选保存：{dialog.IsSaveChecked}";
    }

    private void RetrieveAndReport()
    {
        NetworkCredential? credential = CredentialDialog.RetrieveCredential(Target);
        CredentialDialogResultText.Text = credential is null
            ? "结果：未找到已保存的凭据。"
            : $"结果：已保存凭据 — 用户名：{credential.UserName}";
    }

    private void DeleteAndReport()
    {
        bool removed = CredentialDialog.DeleteCredential(Target);
        CredentialDialogResultText.Text = removed
            ? "结果：已删除凭据。"
            : "结果：未找到可删除的凭据。";
    }
}
