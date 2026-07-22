namespace Wpf.Ui.Violeta.Win32;

internal static class TaskDialogResources
{
    public static string DuplicateButtonTypeError =>
        "The task dialog already has a non-custom button with the same type.";

    public static string DuplicateItemIdError =>
        "The task dialog already has an item with the same id.";

    public static string InvalidTaskDialogItemIdError =>
        "The id of a task dialog item must be higher than 0.";

    public static string NoAssociatedTaskDialogError =>
        "The item is not associated with a task dialog.";

    public static string NonCustomTaskDialogButtonIdError =>
        "Cannot change the id for a standard button.";

    public static string TaskDialogEmptyButtonLabelError =>
        "A custom button or radio button cannot have an empty label.";

    public static string TaskDialogIllegalCrossThreadCallError =>
        "Cross-thread operation not valid: Task dialog accessed from a thread other than the thread it was created on while it is visible.";

    public static string TaskDialogItemHasOwnerError =>
        "The task dialog item already belongs to another task dialog.";

    public static string TaskDialogNoButtonsError =>
        "The task dialog must have buttons.";

    public static string TaskDialogNotRunningError =>
        "The task dialog is not current displayed.";

    public static string TaskDialogRunningError =>
        "The task dialog is already being displayed.";

    public static string TaskDialogsNotSupportedError =>
        "The operating system does not support task dialogs.";
}
