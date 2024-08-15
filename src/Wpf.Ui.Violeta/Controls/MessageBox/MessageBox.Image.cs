using System.Threading.Tasks;
using System.Windows;
using Wpf.Ui.Violeta.Resources.Localization;

namespace Wpf.Ui.Violeta.Controls;

public static partial class MessageBox
{
    public static MessageBoxResult Information(string messageBoxText) =>
        Show(messageBoxText, SH.MessageBoxCaptionInformation, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.None);

    public static MessageBoxResult Information(string messageBoxText, string caption) =>
        Show(messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.None);

    public static MessageBoxResult Information(string messageBoxText, string caption, MessageBoxButton button) =>
        Show(messageBoxText, caption, button, MessageBoxImage.Information, MessageBoxResult.None);

    public static MessageBoxResult Information(string messageBoxText, string caption, MessageBoxButton button, MessageBoxResult defaultResult) =>
        Show(messageBoxText, caption, button, MessageBoxImage.Information.ToSymbol(), defaultResult);

    public static MessageBoxResult Information(Window owner, string messageBoxText, string caption, MessageBoxButton button) =>
        Show(owner, messageBoxText, caption, button, MessageBoxImage.Information, MessageBoxResult.None);

    public static MessageBoxResult Information(Window owner, string messageBoxText, string caption, MessageBoxButton button, MessageBoxResult defaultResult) =>
        Show(owner, messageBoxText, caption, button, MessageBoxImage.Information.ToSymbol(), defaultResult);

    public static MessageBoxResult Warning(string messageBoxText) =>
        Show(messageBoxText, SH.MessageBoxCaptionWarning, MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.None);

    public static MessageBoxResult Warning(string messageBoxText, string caption) =>
        Show(messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.None);

    public static MessageBoxResult Warning(string messageBoxText, string caption, MessageBoxButton button) =>
        Show(messageBoxText, caption, button, MessageBoxImage.Warning, MessageBoxResult.None);

    public static MessageBoxResult Warning(string messageBoxText, string caption, MessageBoxButton button, MessageBoxResult defaultResult) =>
        Show(messageBoxText, caption, button, MessageBoxImage.Warning.ToSymbol(), defaultResult);

    public static MessageBoxResult Warning(Window owner, string messageBoxText, string caption, MessageBoxButton button) =>
        Show(owner, messageBoxText, caption, button, MessageBoxImage.Warning, MessageBoxResult.None);

    public static MessageBoxResult Warning(Window owner, string messageBoxText, string caption, MessageBoxButton button, MessageBoxResult defaultResult) =>
        Show(owner, messageBoxText, caption, button, MessageBoxImage.Warning.ToSymbol(), defaultResult);

    public static MessageBoxResult Question(string messageBoxText) =>
        Show(messageBoxText, SH.MessageBoxCaptionQuestion, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.None);

    public static MessageBoxResult Question(string messageBoxText, string caption) =>
        Show(messageBoxText, caption, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.None);

    public static MessageBoxResult Question(string messageBoxText, string caption, MessageBoxButton button) =>
        Show(messageBoxText, caption, button, MessageBoxImage.Question, MessageBoxResult.None);

    public static MessageBoxResult Question(string messageBoxText, string caption, MessageBoxButton button, MessageBoxResult defaultResult) =>
        Show(messageBoxText, caption, button, MessageBoxImage.Question.ToSymbol(), defaultResult);

    public static MessageBoxResult Question(Window owner, string messageBoxText, string caption, MessageBoxButton button) =>
        Show(owner, messageBoxText, caption, button, MessageBoxImage.Question, MessageBoxResult.None);

    public static MessageBoxResult Question(Window owner, string messageBoxText, string caption, MessageBoxButton button, MessageBoxResult defaultResult) =>
        Show(owner, messageBoxText, caption, button, MessageBoxImage.Question.ToSymbol(), defaultResult);

    public static MessageBoxResult Error(string messageBoxText) =>
        Show(messageBoxText, SH.MessageBoxCaptionError, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.None);

    public static MessageBoxResult Error(string messageBoxText, string caption) =>
        Show(messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.None);

    public static MessageBoxResult Error(string messageBoxText, string caption, MessageBoxButton button) =>
        Show(messageBoxText, caption, button, MessageBoxImage.Error, MessageBoxResult.None);

    public static MessageBoxResult Error(string messageBoxText, string caption, MessageBoxButton button, MessageBoxResult defaultResult) =>
        Show(messageBoxText, caption, button, MessageBoxImage.Error.ToSymbol(), defaultResult);

    public static MessageBoxResult Error(Window owner, string messageBoxText, string caption, MessageBoxButton button) =>
        Show(owner, messageBoxText, caption, button, MessageBoxImage.Error, MessageBoxResult.None);

    public static MessageBoxResult Error(Window owner, string messageBoxText, string caption, MessageBoxButton button, MessageBoxResult defaultResult) =>
        Show(owner, messageBoxText, caption, button, MessageBoxImage.Error.ToSymbol(), defaultResult);

    public static Task<MessageBoxResult> InformationAsync(string messageBoxText) =>
        ShowAsync(messageBoxText, SH.MessageBoxCaptionInformation, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.None);

    public static Task<MessageBoxResult> InformationAsync(string messageBoxText, string caption) =>
        ShowAsync(messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.None);

    public static Task<MessageBoxResult> InformationAsync(string messageBoxText, string caption, MessageBoxButton button) =>
        ShowAsync(messageBoxText, caption, button, MessageBoxImage.Information, MessageBoxResult.None);

    public static Task<MessageBoxResult> InformationAsync(string messageBoxText, string caption, MessageBoxButton button, MessageBoxResult defaultResult) =>
        ShowAsync(GetActiveWindow(), messageBoxText, caption, button, MessageBoxImage.Information, defaultResult);

    public static Task<MessageBoxResult> InformationAsync(Window owner, string messageBoxText, string caption, MessageBoxButton button) =>
        ShowAsync(owner, messageBoxText, caption, button, MessageBoxImage.Information, MessageBoxResult.None);

    public static Task<MessageBoxResult> InformationAsync(Window owner, string messageBoxText, string caption, MessageBoxButton button, MessageBoxResult defaultResult) =>
        ShowAsync(owner, messageBoxText, caption, button, MessageBoxImage.Information.ToSymbol(), defaultResult);

    public static Task<MessageBoxResult> WarningAsync(string messageBoxText) =>
        ShowAsync(messageBoxText, SH.MessageBoxCaptionWarning, MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.None);

    public static Task<MessageBoxResult> WarningAsync(string messageBoxText, string caption) =>
        ShowAsync(messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.None);

    public static Task<MessageBoxResult> WarningAsync(string messageBoxText, string caption, MessageBoxButton button) =>
        ShowAsync(messageBoxText, caption, button, MessageBoxImage.Warning, MessageBoxResult.None);

    public static Task<MessageBoxResult> WarningAsync(string messageBoxText, string caption, MessageBoxButton button, MessageBoxResult defaultResult) =>
        ShowAsync(GetActiveWindow(), messageBoxText, caption, button, MessageBoxImage.Warning, defaultResult);

    public static Task<MessageBoxResult> WarningAsync(Window owner, string messageBoxText, string caption, MessageBoxButton button) =>
        ShowAsync(owner, messageBoxText, caption, button, MessageBoxImage.Warning, MessageBoxResult.None);

    public static Task<MessageBoxResult> WarningAsync(Window owner, string messageBoxText, string caption, MessageBoxButton button, MessageBoxResult defaultResult) =>
        ShowAsync(owner, messageBoxText, caption, button, MessageBoxImage.Warning.ToSymbol(), defaultResult);

    public static Task<MessageBoxResult> QuestionAsync(string messageBoxText) =>
        ShowAsync(messageBoxText, SH.MessageBoxCaptionQuestion, MessageBoxButton.YesNo, MessageBoxImage.Information, MessageBoxResult.None);

    public static Task<MessageBoxResult> QuestionAsync(string messageBoxText, string caption) =>
        ShowAsync(messageBoxText, caption, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.None);

    public static Task<MessageBoxResult> QuestionAsync(string messageBoxText, string caption, MessageBoxButton button) =>
        ShowAsync(messageBoxText, caption, button, MessageBoxImage.Question, MessageBoxResult.None);

    public static Task<MessageBoxResult> QuestionAsync(string messageBoxText, string caption, MessageBoxButton button, MessageBoxResult defaultResult) =>
        ShowAsync(GetActiveWindow(), messageBoxText, caption, button, MessageBoxImage.Question, defaultResult);

    public static Task<MessageBoxResult> QuestionAsync(Window owner, string messageBoxText, string caption, MessageBoxButton button) =>
        ShowAsync(owner, messageBoxText, caption, button, MessageBoxImage.Question, MessageBoxResult.None);

    public static Task<MessageBoxResult> QuestionAsync(Window owner, string messageBoxText, string caption, MessageBoxButton button, MessageBoxResult defaultResult) =>
        ShowAsync(owner, messageBoxText, caption, button, MessageBoxImage.Question.ToSymbol(), defaultResult);

    public static Task<MessageBoxResult> ErrorAsync(string messageBoxText) =>
        ShowAsync(messageBoxText, SH.MessageBoxCaptionError, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.None);

    public static Task<MessageBoxResult> ErrorAsync(string messageBoxText, string caption) =>
        ShowAsync(messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.None);

    public static Task<MessageBoxResult> ErrorAsync(string messageBoxText, string caption, MessageBoxButton button) =>
        ShowAsync(messageBoxText, caption, button, MessageBoxImage.Error, MessageBoxResult.None);

    public static Task<MessageBoxResult> ErrorAsync(string messageBoxText, string caption, MessageBoxButton button, MessageBoxResult defaultResult) =>
        ShowAsync(GetActiveWindow(), messageBoxText, caption, button, MessageBoxImage.Error, defaultResult);

    public static Task<MessageBoxResult> ErrorAsync(Window owner, string messageBoxText, string caption, MessageBoxButton button) =>
        ShowAsync(owner, messageBoxText, caption, button, MessageBoxImage.Error, MessageBoxResult.None);

    public static Task<MessageBoxResult> ErrorAsync(Window owner, string messageBoxText, string caption, MessageBoxButton button, MessageBoxResult defaultResult) =>
        ShowAsync(owner, messageBoxText, caption, button, MessageBoxImage.Error.ToSymbol(), defaultResult);
}
