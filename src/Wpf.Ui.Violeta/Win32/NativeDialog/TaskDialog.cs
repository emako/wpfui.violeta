using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace Wpf.Ui.Violeta.Win32;

/// <summary>
/// Displays a Task Dialog.
/// </summary>
/// <remarks>
/// The task dialog contains an application-defined message text and title, icons, and any combination of predefined push buttons.
/// Task Dialogs are supported only on Windows Vista and above. No fallback is provided; if you wish to use task dialogs
/// and support operating systems older than Windows Vista, you must provide a fallback yourself. Check the <see cref="OSSupportsTaskDialogs"/>
/// property to see if task dialogs are supported. It is safe to instantiate the <see cref="TaskDialog"/> class on an older
/// OS, but calling <see cref="Show"/> or <see cref="ShowDialog()"/> will throw an exception.
/// </remarks>
/// <threadsafety static="true" instance="false" />
[DefaultProperty("MainInstruction"), DefaultEvent("ButtonClicked"), Description("Displays a task dialog.")]
public partial class TaskDialog : Component
{
    #region Events

    /// <summary>
    /// Event raised when the task dialog has been created.
    /// </summary>
    /// <remarks>
    /// This event is raised once after calling <see cref="ShowDialog(nint)"/>, after the dialog
    /// is created and before it is displayed.
    /// </remarks>
    [Category("Behavior"), Description("Event raised when the task dialog has been created.")]
    public event EventHandler Created;

    /// <summary>
    /// Event raised when the task dialog has been destroyed.
    /// </summary>
    /// <remarks>
    /// The task dialog window no longer exists when this event is raised.
    /// </remarks>
    [Category("Behavior"), Description("Event raised when the task dialog has been destroyed.")]
    public event EventHandler Destroyed;

    /// <summary>
    /// Event raised when the user clicks a button on the task dialog.
    /// </summary>
    /// <remarks>
    /// Set the <see cref="CancelEventArgs.Cancel"/> property to <see langword="true" /> to prevent the dialog from being closed.
    /// </remarks>
    [Category("Action"), Description("Event raised when the user clicks a button.")]
    public event EventHandler<TaskDialogItemClickedEventArgs> ButtonClicked;

    /// <summary>
    /// Event raised when the user clicks a radio button on the task dialog.
    /// </summary>
    /// <remarks>
    /// The <see cref="CancelEventArgs.Cancel"/> property is ignored for this event.
    /// </remarks>
    [Category("Action"), Description("Event raised when the user clicks a button.")]
    public event EventHandler<TaskDialogItemClickedEventArgs> RadioButtonClicked;

    /// <summary>
    /// Event raised when the user clicks a hyperlink.
    /// </summary>
    [Category("Action"), Description("Event raised when the user clicks a hyperlink.")]
    public event EventHandler<HyperlinkClickedEventArgs> HyperlinkClicked;

    /// <summary>
    /// Event raised when the user clicks the verification check box.
    /// </summary>
    [Category("Action"), Description("Event raised when the user clicks the verification check box.")]
    public event EventHandler VerificationClicked;

    /// <summary>
    /// Event raised periodically while the dialog is displayed.
    /// </summary>
    /// <remarks>
    /// <para>
    ///   This event is raised only when the <see cref="RaiseTimerEvent"/> property is set to <see langword="true" />. The event is
    ///   raised approximately every 200 milliseconds.
    /// </para>
    /// <para>
    ///   To reset the tick count, set the <see cref="TimerEventArgs.ResetTickCount" />
    ///   property to <see langword="true" />.
    /// </para>
    /// </remarks>
    [Category("Behavior"), Description("Event raised periodically while the dialog is displayed.")]
    public event EventHandler<TimerEventArgs> Timer;

    /// <summary>
    /// Event raised when the user clicks the expand button on the task dialog.
    /// </summary>
    /// <remarks>
    /// The <see cref="ExpandButtonClickedEventArgs.Expanded"/> property indicates if the expanded information is visible
    /// or not after the click.
    /// </remarks>
    [Category("Action"), Description("Event raised when the user clicks the expand button on the task dialog.")]
    public event EventHandler<ExpandButtonClickedEventArgs> ExpandButtonClicked;

    /// <summary>
    /// Event raised when the user presses F1 while the dialog has focus.
    /// </summary>
    [Category("Action"), Description("Event raised when the user presses F1 while the dialog has focus.")]
    public event EventHandler HelpRequested;

    #endregion Events

    #region Fields

    private TaskDialogItemCollection<TaskDialogButton> _buttons;
    private TaskDialogItemCollection<TaskDialogRadioButton> _radioButtons;
    private TaskDialogNativeMethods.TASKDIALOGCONFIG _config = new();
    private TaskDialogIcon _mainIcon;
    private nint _customMainIcon;
    private nint _customFooterIcon;
    private TaskDialogIcon _footerIcon;
    private Dictionary<int, TaskDialogButton> _buttonsById;
    private Dictionary<int, TaskDialogRadioButton> _radioButtonsById;
    private nint _handle;
    private int _progressBarMarqueeAnimationSpeed = 100;
    private int _progressBarMinimimum;
    private int _progressBarMaximum = 100;
    private int _progressBarValue;
    private ProgressBarState _progressBarState = ProgressBarState.Normal;
    private int _inEventHandler;
    private bool _updatePending;
    private object _tag;
    private nint _windowIcon;

    #endregion Fields

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskDialog"/> class.
    /// </summary>
    public TaskDialog()
    {
        InitializeComponent();

        _config.cbSize = (uint)Marshal.SizeOf(_config);
        _config.pfCallback = new TaskDialogNativeMethods.TaskDialogCallback(TaskDialogCallback);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskDialog"/> class with the specified container.
    /// </summary>
    /// <param name="container">The <see cref="IContainer"/> to add the <see cref="TaskDialog"/> to.</param>
    public TaskDialog(IContainer container)
    {
        container?.Add(this);

        InitializeComponent();

        _config.cbSize = (uint)Marshal.SizeOf(_config);
        _config.pfCallback = new TaskDialogNativeMethods.TaskDialogCallback(TaskDialogCallback);
    }

    #endregion Constructors

    #region Public Properties

    /// <summary>
    /// Gets a value that indicates whether the current operating system supports task dialogs.
    /// </summary>
    /// <value>
    /// Returns <see langword="true" /> for Windows Vista or later; otherwise <see langword="false" />.
    /// </value>
    public static bool OSSupportsTaskDialogs => TaskDialogNativeMethods.IsWindowsVistaOrLater;

    /// <summary>
    /// Gets a value that indicates whether the current operating system supports task dialog dark mode.
    /// </summary>
    /// <value>
    /// Returns <see langword="true" /> on Windows 10 version 1809 (build 17763) or later; otherwise
    /// <see langword="false" /> and dialogs are rendered using the light theme.
    /// </value>
    public static bool SupportsDarkMode => TaskDialogNativeMethods.SupportsTaskDialogDarkMode;

    /// <summary>
    /// Sets the visual theme used by task dialogs created by this library.
    /// </summary>
    /// <param name="theme">
    /// The theme to apply. Use <see cref="TaskDialogTheme.System"/> to follow the Windows app theme preference,
    /// <see cref="TaskDialogTheme.Dark"/> to force dark mode, or <see cref="TaskDialogTheme.Light"/> to force light mode.
    /// </param>
    /// <remarks>
    /// Call this method before showing a task dialog, and again whenever the theme should change.
    /// The setting applies to task dialogs created by this library in the current process.
    /// When <see cref="TaskDialogTheme.System"/> is used, open dialogs also react to system theme changes.
    /// On operating systems older than Windows 10 version 1809 (build 17763), dark mode is not supported and
    /// dialogs always use the light theme regardless of this setting.
    /// </remarks>
    public static void SetTheme(TaskDialogTheme theme)
    {
        TaskDialogThemeHost.SetTheme(theme);
    }

    /// <summary>
    /// Gets a list of the buttons on the Task Dialog.
    /// </summary>
    /// <value>
    /// A list of the buttons on the Task Dialog.
    /// </value>
    /// <remarks>
    /// Custom buttons are displayed in the order they have in the collection. Standard buttons will always be displayed
    /// in the Windows-defined order, regardless of the order of the buttons in the collection.
    /// </remarks>
    [Localizable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), Category("Appearance"), Description("A list of the buttons on the Task Dialog.")]
    public TaskDialogItemCollection<TaskDialogButton> Buttons => _buttons ??= new TaskDialogItemCollection<TaskDialogButton>(this);

    /// <summary>
    /// Gets a list of the radio buttons on the Task Dialog.
    /// </summary>
    /// <value>
    /// A list of the radio buttons on the Task Dialog.
    /// </value>
    [Localizable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), Category("Appearance"), Description("A list of the radio buttons on the Task Dialog.")]
    public TaskDialogItemCollection<TaskDialogRadioButton> RadioButtons
    {
        get => _radioButtons ??= new TaskDialogItemCollection<TaskDialogRadioButton>(this);
        private set => _radioButtons = value;
    }

    /// <summary>
    /// Gets or sets the window title of the task dialog.
    /// </summary>
    /// <value>
    /// The window title of the task dialog. The default is an empty string ("").
    /// </value>
    [Localizable(true), Category("Appearance"), Description("The window title of the task dialog."), DefaultValue("")]
    public string WindowTitle
    {
        get => _config.pszWindowTitle ?? string.Empty;
        set
        {
            _config.pszWindowTitle = string.IsNullOrEmpty(value) ? null : value;
            UpdateDialog();
        }
    }

    /// <summary>
    /// Gets or sets the dialog's main instruction.
    /// </summary>
    /// <value>
    /// The dialog's main instruction. The default is an empty string ("").
    /// </value>
    /// <remarks>
    /// The main instruction of a task dialog will be displayed in a larger font and a different color than
    /// the other text of the task dialog.
    /// </remarks>
    [Localizable(true), Category("Appearance"), Description("The dialog's main instruction."), DefaultValue("")]
    public string MainInstruction
    {
        get => _config.pszMainInstruction ?? string.Empty;
        set
        {
            _config.pszMainInstruction = string.IsNullOrEmpty(value) ? null : value;
            SetElementText(TaskDialogNativeMethods.TaskDialogElements.MainInstruction, MainInstruction);
        }
    }

    /// <summary>
    /// Gets or sets the dialog's primary content.
    /// </summary>
    /// <value>
    /// The dialog's primary content. The default is an empty string ("").
    /// </value>
    [Localizable(true), Category("Appearance"), Description("The dialog's primary content."), DefaultValue("")]
    public string Content
    {
        get => _config.pszContent ?? string.Empty;
        set
        {
            _config.pszContent = string.IsNullOrEmpty(value) ? null : value;
            SetElementText(TaskDialogNativeMethods.TaskDialogElements.Content, Content);
        }
    }

    /// <summary>
    /// Gets or sets the icon to be used in the title bar of the dialog.
    /// </summary>
    /// <value>
    /// An icon handle (<see cref="nint"/>) that represents the icon of the task dialog's window.
    /// </value>
    /// <remarks>
    /// This property is used only when the dialog is shown as a modeless dialog; if the dialog
    /// is modal, it will have no icon.
    /// </remarks>
    [Localizable(true), Category("Appearance"), Description("The icon to be used in the title bar of the dialog. Used only when the dialog is shown as a modeless dialog."), DefaultValue(typeof(nint), "0")]
    public nint WindowIcon
    {
        get
        {
            if (IsDialogRunning)
            {
                return TaskDialogNativeMethods.SendMessage(Handle, TaskDialogNativeMethods.WM_GETICON, (IntPtr)(int)TaskDialogNativeMethods.ICON_SMALL, IntPtr.Zero);
            }
            return _windowIcon;
        }
        set => _windowIcon = value;
    }

    /// <summary>
    /// Gets or sets the icon to display in the task dialog.
    /// </summary>
    /// <value>
    /// A <see cref="TaskDialogIcon"/> that indicates the icon to display in the main content area of the task dialog.
    /// The default is <see cref="TaskDialogIcon.Custom"/>.
    /// </value>
    /// <remarks>
    /// When this property is set to <see cref="TaskDialogIcon.Custom"/>, use the <see cref="CustomMainIcon"/> property to
    /// specify the icon to use.
    /// </remarks>
    [Localizable(true), Category("Appearance"), Description("The icon to display in the task dialog."), DefaultValue(TaskDialogIcon.Custom)]
    public TaskDialogIcon MainIcon
    {
        get => _mainIcon;
        set
        {
            if (_mainIcon != value)
            {
                _mainIcon = value;
                UpdateDialog();
            }
        }
    }

    /// <summary>
    /// Gets or sets a custom icon to display in the dialog.
    /// </summary>
    /// <value>
    /// An icon handle (<see cref="nint"/>) that represents the icon to display in the main content area of the task dialog,
    /// or zero if no custom icon is used. The default value is zero.
    /// </value>
    /// <remarks>
    /// This property is ignored if the <see cref="MainIcon"/> property has a value other than <see cref="TaskDialogIcon.Custom"/>.
    /// </remarks>
    [Localizable(true), Category("Appearance"), Description("A custom icon to display in the dialog."), DefaultValue(typeof(nint), "0")]
    public nint CustomMainIcon
    {
        get => _customMainIcon;
        set
        {
            if (_customMainIcon != value)
            {
                _customMainIcon = value;
                UpdateDialog();
            }
        }
    }

    /// <summary>
    /// Gets or sets the icon to display in the footer area of the task dialog.
    /// </summary>
    /// <value>
    /// A <see cref="TaskDialogIcon"/> that indicates the icon to display in the footer area of the task dialog.
    /// The default is <see cref="TaskDialogIcon.Custom"/>.
    /// </value>
    /// <remarks>
    /// <para>
    ///   When this property is set to <see cref="TaskDialogIcon.Custom"/>, use the <see cref="CustomFooterIcon"/> property to
    ///   specify the icon to use.
    /// </para>
    /// <para>
    ///   The footer icon is displayed only if the <see cref="Footer"/> property is not an empty string ("").
    /// </para>
    /// </remarks>
    [Localizable(true), Category("Appearance"), Description("The icon to display in the footer area of the task dialog."), DefaultValue(TaskDialogIcon.Custom)]
    public TaskDialogIcon FooterIcon
    {
        get => _footerIcon;
        set
        {
            if (_footerIcon != value)
            {
                _footerIcon = value;
                UpdateDialog();
            }
        }
    }

    /// <summary>
    /// Gets or sets a custom icon to display in the footer area of the task dialog.
    /// </summary>
    /// <value>
    /// An icon handle (<see cref="nint"/>) that represents the icon to display in the footer area of the task dialog,
    /// or zero if no custom icon is used. The default value is zero.
    /// </value>
    /// <remarks>
    /// <para>
    ///   This property is ignored if the <see cref="FooterIcon"/> property has a value other than <see cref="TaskDialogIcon.Custom"/>.
    /// </para>
    /// <para>
    ///   The footer icon is displayed only if the <see cref="Footer"/> property is not an empty string ("").
    /// </para>
    /// </remarks>
    [Localizable(true), Category("Appearance"), Description("A custom icon to display in the footer area of the task dialog."), DefaultValue(typeof(nint), "0")]
    public nint CustomFooterIcon
    {
        get => _customFooterIcon;
        set
        {
            if (_customFooterIcon != value)
            {
                _customFooterIcon = value;
                // TODO: This and customMainIcon don't need to use UpdateDialog, they can use TDM_UPDATE_ICON
                UpdateDialog();
            }
        }
    }

    /// <summary>
    /// Gets or sets a value that indicates whether custom buttons should be displayed as normal buttons or command links.
    /// </summary>
    /// <value>
    /// A <see cref="TaskDialogButtonStyle"/> that indicates the display style of custom buttons on the dialog.
    /// The default value is <see cref="TaskDialogButtonStyle.Standard"/>.
    /// </value>
    /// <remarks>
    /// <para>
    ///   This property affects only custom buttons, not standard ones.
    /// </para>
    /// <para>
    ///   If a custom button is being displayed on a task dialog
    ///   with <see cref="TaskDialog.ButtonStyle"/> set to <see cref="Wpf.Ui.Violeta.Win32.TaskDialogButtonStyle.CommandLinks"/>
    ///   or <see cref="Wpf.Ui.Violeta.Win32.TaskDialogButtonStyle.CommandLinksNoIcon"/>, you delineate the command from the
    ///   note by placing a line break in the string specified by <see cref="TaskDialogItem.Text"/> property.
    /// </para>
    /// </remarks>
    [Category("Behavior"), Description("Indicates whether custom buttons should be displayed as normal buttons or command links."), DefaultValue(TaskDialogButtonStyle.Standard)]
    public TaskDialogButtonStyle ButtonStyle
    {
        get
        {
            return GetFlag(TaskDialogNativeMethods.TaskDialogFlags.UseCommandLinksNoIcon) ? TaskDialogButtonStyle.CommandLinksNoIcon :
                GetFlag(TaskDialogNativeMethods.TaskDialogFlags.UseCommandLinks) ? TaskDialogButtonStyle.CommandLinks :
                TaskDialogButtonStyle.Standard;
        }
        set
        {
            SetFlag(TaskDialogNativeMethods.TaskDialogFlags.UseCommandLinks, value == TaskDialogButtonStyle.CommandLinks);
            SetFlag(TaskDialogNativeMethods.TaskDialogFlags.UseCommandLinksNoIcon, value == TaskDialogButtonStyle.CommandLinksNoIcon);
            UpdateDialog();
        }
    }

    /// <summary>
    /// Gets or sets the label for the verification checkbox.
    /// </summary>
    /// <value>
    /// The label for the verification checkbox, or an empty string ("") if no verification checkbox
    /// is shown. The default value is an empty string ("").
    /// </value>
    /// <remarks>
    /// If no text is set, the verification checkbox will not be shown.
    /// </remarks>
    [Localizable(true), Category("Appearance"), Description("The label for the verification checkbox."), DefaultValue("")]
    public string VerificationText
    {
        get => _config.pszVerificationText ?? string.Empty;
        set
        {
            string? realValue = string.IsNullOrEmpty(value) ? null : value;
            if (_config.pszVerificationText != realValue)
            {
                _config.pszVerificationText = realValue;
                UpdateDialog();
            }
        }
    }

    /// <summary>
    /// Gets or sets a value that indicates whether the verification checkbox is checked ot not.
    /// </summary>
    /// <value>
    /// <see langword="true" /> if the verficiation checkbox is checked; otherwise, <see langword="false" />.
    /// </value>
    /// <remarks>
    /// <para>
    ///   Set this property before displaying the dialog to determine the initial state of the check box.
    ///   Use this property after displaying the dialog to determine whether the check box was checked when
    ///   the user closed the dialog.
    /// </para>
    /// <note>
    ///   This property is only used if <see cref="VerificationText"/> is not an empty string ("").
    /// </note>
    /// </remarks>
    [Category("Behavior"), Description("Indicates whether the verification checkbox is checked ot not."), DefaultValue(false)]
    public bool IsVerificationChecked
    {
        get => GetFlag(TaskDialogNativeMethods.TaskDialogFlags.VerificationFlagChecked);
        set
        {
            if (value != IsVerificationChecked)
            {
                SetFlag(TaskDialogNativeMethods.TaskDialogFlags.VerificationFlagChecked, value);
                if (IsDialogRunning)
                    ClickVerification(value, false);
            }
        }
    }

    /// <summary>
    /// Gets or sets additional information to be displayed on the dialog.
    /// </summary>
    /// <value>
    /// Additional information to be displayed on the dialog. The default value is an empty string ("").
    /// </value>
    /// <remarks>
    /// <para>
    ///   When this property is not an empty string (""), a control is shown on the task dialog that
    ///   allows the user to expand and collapse the text specified in this property.
    /// </para>
    /// <para>
    ///   The text is collapsed by default unless <see cref="ExpandedByDefault"/> is set to <see langword="true" />.
    /// </para>
    /// <para>
    ///   The expanded text is shown in the main content area of the dialog, unless <see cref="ExpandFooterArea"/>
    ///   is set to <see langword="true" />, in which case it is shown in the footer area.
    /// </para>
    /// </remarks>
    [Localizable(true), Category("Appearance"), Description("Additional information to be displayed on the dialog."), DefaultValue("")]
    public string ExpandedInformation
    {
        get => _config.pszExpandedInformation ?? string.Empty;
        set
        {
            _config.pszExpandedInformation = string.IsNullOrEmpty(value) ? null : value;
            SetElementText(TaskDialogNativeMethods.TaskDialogElements.ExpandedInformation, ExpandedInformation);
        }
    }

    /// <summary>
    /// Gets or sets the text to use for the control for collapsing the expandable information specified in <see cref="ExpandedInformation"/>.
    /// </summary>
    /// <value>
    /// The text to use for the control for collapsing the expandable information, or an empty string ("") if the
    /// operating system's default text is to be used. The default is an empty string ("")
    /// </value>
    /// <remarks>
    /// <para>
    ///   If this text is not specified and <see cref="CollapsedControlText"/> is specified, the value of <see cref="CollapsedControlText"/>
    ///   will be used for this property as well. If neither is specified, the operating system's default text is used.
    /// </para>
    /// <note>
    ///   The control for collapsing or expanding the expandable information is displayed only if <see cref="ExpandedInformation"/> is not
    ///   an empty string ("")
    /// </note>
    /// </remarks>
    [Localizable(true), Category("Appearance"), Description("The text to use for the control for collapsing the expandable information."), DefaultValue("")]
    public string ExpandedControlText
    {
        get => _config.pszExpandedControlText ?? string.Empty;
        set
        {
            string? realValue = string.IsNullOrEmpty(value) ? null : value;
            if (_config.pszExpandedControlText != realValue)
            {
                _config.pszExpandedControlText = realValue;
                UpdateDialog();
            }
        }
    }

    /// <summary>
    /// Gets or sets the text to use for the control for expading the expandable information specified in <see cref="ExpandedInformation"/>.
    /// </summary>
    /// <value>
    /// The text to use for the control for expanding the expandable information, or an empty string ("") if the
    /// operating system's default text is to be used. The default is an empty string ("")
    /// </value>
    /// <remarks>
    /// <para>
    ///   If this text is not specified and <see cref="ExpandedControlText"/> is specified, the value of <see cref="ExpandedControlText"/>
    ///   will be used for this property as well. If neither is specified, the operating system's default text is used.
    /// </para>
    /// <note>
    ///   The control for collapsing or expanding the expandable information is displayed only if <see cref="ExpandedInformation"/> is not
    ///   an empty string ("")
    /// </note>
    /// </remarks>
    [Localizable(true), Category("Appearance"), Description("The text to use for the control for expanding the expandable information."), DefaultValue("")]
    public string CollapsedControlText
    {
        get => _config.pszCollapsedControlText ?? string.Empty;
        set
        {
            string? realValue = string.IsNullOrEmpty(value) ? null : value;
            if (_config.pszCollapsedControlText != realValue)
            {
                _config.pszCollapsedControlText = realValue;
                UpdateDialog();
            }
        }
    }

    /// <summary>
    /// Gets or sets the text to be used in the footer area of the task dialog.
    /// </summary>
    /// <value>
    /// The text to be used in the footer area of the task dialog, or an empty string ("")
    /// if the footer area is not displayed. The default value is an empty string ("").
    /// </value>
    [Localizable(true), Category("Appearance"), Description("The text to be used in the footer area of the task dialog."), DefaultValue("")]
    public string Footer
    {
        get => _config.pszFooter ?? string.Empty;
        set
        {
            _config.pszFooter = string.IsNullOrEmpty(value) ? null : value;
            SetElementText(TaskDialogNativeMethods.TaskDialogElements.Footer, Footer);
        }
    }

    /// <summary>
    /// Specifies the width of the task dialog's client area in DLU's.
    /// </summary>
    /// <value>
    /// The width of the task dialog's client area in DLU's, or 0 to have the task dialog calculate the ideal width.
    /// The default value is 0.
    /// </value>
    [Localizable(true), Category("Appearance"), Description("the width of the task dialog's client area in DLU's. If 0, task dialog will calculate the ideal width."), DefaultValue(0)]
    public int Width
    {
        get => (int)_config.cxWidth;
        set
        {
            if (_config.cxWidth != (uint)value)
            {
                _config.cxWidth = (uint)value;
                UpdateDialog();
            }
        }
    }

    /// <summary>
    /// Gets or sets a value that indicates whether hyperlinks are allowed for the <see cref="Content"/>, <see cref="ExpandedInformation"/>
    /// and <see cref="Footer"/> properties.
    /// </summary>
    /// <value>
    /// <see langword="true" /> when hyperlinks are allowed for the <see cref="Content"/>, <see cref="ExpandedInformation"/>
    /// and <see cref="Footer"/> properties; otherwise, <see langword="false" />. The default value is <see langword="false" />.
    /// </value>
    /// <remarks>
    /// <para>
    ///   When  this property is <see langword="true" />, the <see cref="Content"/>, <see cref="ExpandedInformation"/>
    ///   and <see cref="Footer"/> properties can use hyperlinks in the following form: <c>&lt;A HREF="executablestring"&gt;Hyperlink Text&lt;/A&gt;</c>
    /// </para>
    /// <note>
    ///   Enabling hyperlinks when using content from an unsafe source may cause security vulnerabilities.
    /// </note>
    /// <para>
    ///   Task dialogs will not actually execute hyperlinks. To take action when the user presses a hyperlink, handle the
    ///   <see cref="HyperlinkClicked"/> event.
    /// </para>
    /// </remarks>
    [Category("Behavior"), Description("Indicates whether hyperlinks are allowed for the Content, ExpandedInformation and Footer properties."), DefaultValue(false)]
    public bool EnableHyperlinks
    {
        get => GetFlag(TaskDialogNativeMethods.TaskDialogFlags.EnableHyperLinks);
        set
        {
            if (EnableHyperlinks != value)
            {
                SetFlag(TaskDialogNativeMethods.TaskDialogFlags.EnableHyperLinks, value);
                UpdateDialog();
            }
        }
    }

    /// <summary>
    /// Gets or sets a value that indicates that the dialog should be able to be closed using Alt-F4, Escape and the title
    /// bar's close button even if no cancel button is specified.
    /// </summary>
    /// <value>
    /// <see langword="true" /> if the dialog can be closed using Alt-F4, Escape and the title
    /// bar's close button even if no cancel button is specified; otherwise, <see langword="false" />.
    /// The default value is <see langword="false" />.
    /// </value>
    [Category("Behavior"), Description("Indicates that the dialog should be able to be closed using Alt-F4, Escape and the title bar's close button even if no cancel button is specified."), DefaultValue(false)]
    public bool AllowDialogCancellation
    {
        get => GetFlag(TaskDialogNativeMethods.TaskDialogFlags.AllowDialogCancellation);
        set
        {
            if (AllowDialogCancellation != value)
            {
                SetFlag(TaskDialogNativeMethods.TaskDialogFlags.AllowDialogCancellation, value);
                UpdateDialog();
            }
        }
    }

    /// <summary>
    /// Gets or sets a value that indicates that the string specified by the <see cref="ExpandedInformation" /> property
    /// should be displayed at the bottom of the dialog's footer area instead of immediately after the dialog's content.
    /// </summary>
    /// <value>
    /// <see langword="true" /> if the string specified by the <see cref="ExpandedInformation" /> property
    /// should be displayed at the bottom of the dialog's footer area instead of immediately after the dialog's content;
    /// otherwise, <see langword="false" />. The default value is <see langword="false" />.
    /// </value>
    [Category("Behavior"), Description("Indicates that the string specified by the ExpandedInformation property should be displayed at the bottom of the dialog's footer area instead of immediately after the dialog's content."), DefaultValue(false)]
    public bool ExpandFooterArea
    {
        get => GetFlag(TaskDialogNativeMethods.TaskDialogFlags.ExpandFooterArea);
        set
        {
            if (ExpandFooterArea != value)
            {
                SetFlag(TaskDialogNativeMethods.TaskDialogFlags.ExpandFooterArea, value);
                UpdateDialog();
            }
        }
    }

    /// <summary>
    /// Gets or sets a value that indicates that the string specified by the <see cref="ExpandedInformation"/> property
    /// should be displayed by default.
    /// </summary>
    /// <value>
    /// <see langword="true" /> if the string specified by the <see cref="ExpandedInformation"/> property
    /// should be displayed by default; <see langword="false" /> if it is hidden by default. The default value is
    /// <see langword="false" />.
    /// </value>
    [Category("Behavior"), Description("Indicates that the string specified by the ExpandedInformation property should be displayed by default."), DefaultValue(false)]
    public bool ExpandedByDefault
    {
        get => GetFlag(TaskDialogNativeMethods.TaskDialogFlags.ExpandedByDefault);
        set
        {
            if (ExpandedByDefault != value)
            {
                SetFlag(TaskDialogNativeMethods.TaskDialogFlags.ExpandedByDefault, value);
                UpdateDialog();
            }
        }
    }

    /// <summary>
    /// Gets or sets a value that indicates whether the <see cref="Timer"/> event is raised periodically while the dialog
    /// is visible.
    /// </summary>
    /// <value>
    /// <see langword="true" /> when the <see cref="Timer"/> event is raised periodically while the dialog is visible; otherwise,
    /// <see langword="false" />. The default value is <see langword="false" />.
    /// </value>
    /// <remarks>
    /// The <see cref="Timer"/> event will be raised approximately every 200 milliseconds if this property is <see langword="true" />.
    /// </remarks>
    [Category("Behavior"), Description("Indicates whether the Timer event is raised periodically while the dialog is visible."), DefaultValue(false)]
    public bool RaiseTimerEvent
    {
        get => GetFlag(TaskDialogNativeMethods.TaskDialogFlags.CallbackTimer);
        set
        {
            if (RaiseTimerEvent != value)
            {
                SetFlag(TaskDialogNativeMethods.TaskDialogFlags.CallbackTimer, value);
                UpdateDialog();
            }
        }
    }

    /// <summary>
    /// Gets or sets the preferred parent window for the dialog.
    /// </summary>
    [Category("Layout"), Description("The preferred parent window handle."), DefaultValue(0)]
    public nint PreferParent { get; set; }

    /// <summary>
    /// Gets or sets a value that indicates whether the dialog is centered in the parent window instead of the screen.
    /// </summary>
    /// <value>
    /// <see langword="true" /> when the dialog is centered relative to the parent window; <see langword="false" /> when it is centered on the screen.
    /// The default value is <see langword="false" />.
    /// </value>
    [Category("Layout"), Description("Indicates whether the dialog is centered in the parent window instead of the screen."), DefaultValue(false)]
    public bool CenterParent
    {
        get => GetFlag(TaskDialogNativeMethods.TaskDialogFlags.PositionRelativeToWindow);
        set
        {
            if (CenterParent != value)
            {
                SetFlag(TaskDialogNativeMethods.TaskDialogFlags.PositionRelativeToWindow, value);
                UpdateDialog();
            }
        }
    }

    /// <summary>
    /// Gets or sets a value that indicates whether text is displayed right to left.
    /// </summary>
    /// <value>
    /// <see langword="true" /> when the content of the dialog is displayed right to left; otherwise, <see langword="false" />.
    /// The default value is <see langword="false" />.
    /// </value>
    [Localizable(true), Category("Appearance"), Description("Indicates whether text is displayed right to left."), DefaultValue(false)]
    public bool RightToLeft
    {
        get => GetFlag(TaskDialogNativeMethods.TaskDialogFlags.RtlLayout);
        set
        {
            if (RightToLeft != value)
            {
                SetFlag(TaskDialogNativeMethods.TaskDialogFlags.RtlLayout, value);
                UpdateDialog();
            }
        }
    }

    /// <summary>
    /// Gets or sets a value that indicates whether the dialog has a minimize box on its caption bar.
    /// </summary>
    /// <value>
    /// <see langword="true" /> if the dialog has a minimize box on its caption bar when modeless; otherwise,
    /// <see langword="false" />. The default is <see langword="false" />.
    /// </value>
    /// <remarks>
    /// A task dialog can only have a minimize box if it is displayed as a modeless dialog. The minimize box
    /// will never appear when using the designer "Preview" option, since that displays the dialog modally.
    /// </remarks>
    [Category("Window Style"), Description("Indicates whether the dialog has a minimize box on its caption bar."), DefaultValue(false)]
    public bool MinimizeBox
    {
        get => GetFlag(TaskDialogNativeMethods.TaskDialogFlags.CanBeMinimized);
        set
        {
            if (MinimizeBox != value)
            {
                SetFlag(TaskDialogNativeMethods.TaskDialogFlags.CanBeMinimized, value);
                UpdateDialog();
            }
        }
    }

    /// <summary>
    /// Gets or sets the type of progress bar displayed on the dialog.
    /// </summary>
    /// <value>
    /// A <see cref="ProgressBarStyle"/> that indicates the type of progress bar shown on the task dialog.
    /// </value>
    /// <remarks>
    /// <para>
    ///   If this property is set to <see cref="ProgressBarStyle.MarqueeProgressBar"/>, the marquee will
    ///   scroll as long as the dialog is visible.
    /// </para>
    /// <para>
    ///   If this property is set to <see cref="ProgressBarStyle.ProgressBar"/>, the value of the
    ///   <see cref="ProgressBarValue" /> property must be updated to advance the progress bar. This can be done e.g. by
    ///   an asynchronous operation or from the <see cref="Timer"/> event.
    /// </para>
    /// <note>
    ///   Updating the value of the progress bar using the <see cref="ProgressBarValue"/> while the dialog is visible property may only be done from
    ///   the thread on which the task dialog was created.
    /// </note>
    /// </remarks>
    [Category("Behavior"), Description("The type of progress bar displayed on the dialog."), DefaultValue(ProgressBarStyle.None)]
    public ProgressBarStyle ProgressBarStyle
    {
        get
        {
            if (GetFlag(TaskDialogNativeMethods.TaskDialogFlags.ShowMarqueeProgressBar))
                return ProgressBarStyle.MarqueeProgressBar;
            else if (GetFlag(TaskDialogNativeMethods.TaskDialogFlags.ShowProgressBar))
                return ProgressBarStyle.ProgressBar;
            else
                return ProgressBarStyle.None;
        }
        set
        {
            SetFlag(TaskDialogNativeMethods.TaskDialogFlags.ShowMarqueeProgressBar, value == ProgressBarStyle.MarqueeProgressBar);
            SetFlag(TaskDialogNativeMethods.TaskDialogFlags.ShowProgressBar, value == ProgressBarStyle.ProgressBar);
            UpdateProgressBarStyle();
        }
    }

    /// <summary>
    /// Gets or sets the marquee animation speed of the progress bar in milliseconds.
    /// </summary>
    /// <value>
    /// The marquee animation speed of the progress bar in milliseconds. The default value is 100.
    /// </value>
    /// <remarks>
    /// This property is only used if the <see cref="ProgressBarStyle"/> property is
    /// <see cref="ProgressBarStyle.MarqueeProgressBar"/>.
    /// </remarks>
    [Category("Behavior"), Description("The marquee animation speed of the progress bar in milliseconds."), DefaultValue(100)]
    public int ProgressBarMarqueeAnimationSpeed
    {
        get => _progressBarMarqueeAnimationSpeed;
        set
        {
            _progressBarMarqueeAnimationSpeed = value;
            UpdateProgressBarMarqueeSpeed();
        }
    }

    /// <summary>
    /// Gets or sets the lower bound of the range of the task dialog's progress bar.
    /// </summary>
    /// <value>
    /// The lower bound of the range of the task dialog's progress bar. The default value is 0.
    /// </value>
    /// <remarks>
    /// This property is only used if the <see cref="ProgressBarStyle"/> property is
    /// <see cref="ProgressBarStyle.ProgressBar"/>.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">The new property value is not smaller than <see cref="ProgressBarMaximum"/>.</exception>
    [Category("Behavior"), Description("The lower bound of the range of the task dialog's progress bar."), DefaultValue(0)]
    public int ProgressBarMinimum
    {
        get => _progressBarMinimimum;
        set
        {
#pragma warning disable IDE0011 // Suppress IDE0011: Temporarily disable "add braces" style warning
#pragma warning disable CA1512 // Suppress CA1512: Disable rule to allow manual ArgumentOutOfRange check
            if (_progressBarMaximum <= value)
                throw new ArgumentOutOfRangeException(nameof(value));
#pragma warning restore IDE0011 // Restore IDE0011 mandatory braces style check
#pragma warning restore CA1512 // Restore CA1512 out-of-range argument helper rule
            _progressBarMinimimum = value;
            UpdateProgressBarRange();
        }
    }

    /// <summary>
    /// Gets or sets the upper bound of the range of the task dialog's progress bar.
    /// </summary>
    /// <value>
    /// The upper bound of the range of the task dialog's progress bar. The default value is 100.
    /// </value>
    /// <remarks>
    /// This property is only used if the <see cref="ProgressBarStyle"/> property is
    /// <see cref="ProgressBarStyle.ProgressBar"/>.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">The new property value is not larger than <see cref="ProgressBarMinimum"/>.</exception>
    [Category("Behavior"), Description("The upper bound of the range of the task dialog's progress bar."), DefaultValue(100)]
    public int ProgressBarMaximum
    {
        get => _progressBarMaximum;
        set
        {
#pragma warning disable IDE0011 // Suppress IDE0011: Temporarily disable "add braces" style warning
#pragma warning disable CA1512 // Suppress CA1512: Disable rule to allow manual ArgumentOutOfRange check
            if (value <= _progressBarMinimimum)
                throw new ArgumentOutOfRangeException(nameof(value));
#pragma warning restore IDE0011 // Restore IDE0011 mandatory braces style check
#pragma warning restore CA1512 // Restore CA1512 out-of-range argument helper rule
            _progressBarMaximum = value;
            UpdateProgressBarRange();
        }
    }

    /// <summary>
    /// Gets or sets the current value of the task dialog's progress bar.
    /// </summary>
    /// <value>
    /// The current value of the task dialog's progress bar. The default value is 0.
    /// </value>
    /// <remarks>
    /// This property is only used if the <see cref="ProgressBarStyle"/> property is
    /// <see cref="ProgressBarStyle.ProgressBar"/>.
    /// <note>
    ///   Updating the value of the progress bar while the dialog is visible  may only be done from
    ///   the thread on which the task dialog was created.
    /// </note>
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">The new property value is smaller than <see cref="ProgressBarMinimum"/> or larger than <see cref="ProgressBarMaximum"/>.</exception>
    [Category("Behavior"), Description("The current value of the task dialog's progress bar."), DefaultValue(0)]
    public int ProgressBarValue
    {
        get => _progressBarValue;
        set
        {
            if (value < ProgressBarMinimum || value > ProgressBarMaximum)
                throw new ArgumentOutOfRangeException(nameof(value));

            _progressBarValue = value;
            UpdateProgressBarValue();
        }
    }

    /// <summary>
    /// Gets or sets the state of the task dialog's progress bar.
    /// </summary>
    /// <value>
    /// A <see cref="ProgressBarState"/> indicating the state of the task dialog's progress bar.
    /// The default value is <see cref="ProgressBarState.Normal"/>.
    /// </value>
    /// <remarks>
    /// This property is only used if the <see cref="ProgressBarStyle"/> property is
    /// <see cref="ProgressBarStyle.ProgressBar"/>.
    /// </remarks>
    [Category("Behavior"), Description("The state of the task dialog's progress bar."), DefaultValue(ProgressBarState.Normal)]
    public ProgressBarState ProgressBarState
    {
        get => _progressBarState;
        set
        {
            _progressBarState = value;
            UpdateProgressBarState();
        }
    }

    /// <summary>
    /// Gets or sets an object that contains data about the dialog.
    /// </summary>
    /// <value>
    /// An object that contains data about the dialog. The default value is <see langword="null" />.
    /// </value>
    /// <remarks>
    /// Use this property to store arbitrary information about the dialog.
    /// </remarks>
    [Category("Data"), Description("User-defined data about the component."), DefaultValue(null)]
    public object Tag
    {
        get => _tag;
        set => _tag = value;
    }

    #endregion Public Properties

    #region Public methods

    /// <summary>
    /// Shows the task dialog as a modeless dialog.
    /// </summary>
    /// <returns>The button that the user clicked. Can be <see langword="null" /> if the user cancelled the dialog using the
    /// title bar close button.</returns>
    /// <remarks>
    /// <note>
    ///   Although the dialog is modeless, this method does not return until the task dialog is closed.
    /// </note>
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// <para>
    ///   One of the properties or a combination of properties is not valid.
    /// </para>
    /// <para>
    ///   -or-
    /// </para>
    /// <para>
    ///   The dialog is already running.
    /// </para>
    /// </exception>
    /// <exception cref="NotSupportedException">Task dialogs are not supported on the current operating system.</exception>
    public TaskDialogButton Show()
    {
        return ShowDialog(0);
    }

    /// <summary>
    /// Shows the task dialog as a modal dialog.
    /// </summary>
    /// <returns>The button that the user clicked. Can be <see langword="null" /> if the user cancelled the dialog using the
    /// title bar close button.</returns>
    /// <remarks>
    /// The dialog will use the active window as its owner. If the current process has no active window,
    /// the dialog will be displayed as a modeless dialog (identical to calling <see cref="Show"/>).
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// <para>
    ///   One of the properties or a combination of properties is not valid.
    /// </para>
    /// <para>
    ///   -or-
    /// </para>
    /// <para>
    ///   The dialog is already running.
    /// </para>
    /// </exception>
    /// <exception cref="NotSupportedException">Task dialogs are not supported on the current operating system.</exception>
    public TaskDialogButton ShowDialog()
    {
        return ShowDialog(0);
    }

    /// <summary>
    /// Shows the task dialog as a modal dialog.
    /// </summary>
    /// <param name="owner">The owner window handle (<see cref="nint"/>).</param>
    /// <returns>The button that the user clicked. Can be <see langword="null" /> if the user cancelled the dialog using the
    /// title bar close button.</returns>
    public TaskDialogButton ShowDialog(nint owner)
    {
        nint ownerHandle = owner == 0 ? TaskDialogNativeMethods.GetActiveWindow() : owner;
        return ShowDialogCore(ownerHandle);
    }

    /// <summary>
    /// Shows the task dialog as a modal dialog.
    /// </summary>
    /// <param name="owner">The <see cref="IntPtr"/> Win32 handle that is the owner of this task dialog.</param>
    /// <returns>The button that the user clicked. Can be <see langword="null" /> if the user cancelled the dialog using the
    /// title bar close button.</returns>
    private TaskDialogButton ShowDialogCore(nint owner)
    {
        if (!OSSupportsTaskDialogs)
            throw new NotSupportedException(TaskDialogResources.TaskDialogsNotSupportedError);

        if (IsDialogRunning)
            throw new InvalidOperationException(TaskDialogResources.TaskDialogRunningError);

        if (_buttons is null || _buttons.Count == 0)
            throw new InvalidOperationException(TaskDialogResources.TaskDialogNoButtonsError);

        _config.hwndParent = owner;
        _config.dwCommonButtons = 0;
        _config.pButtons = default;
        _config.cButtons = 0;
        List<TaskDialogNativeMethods.TASKDIALOG_BUTTON> buttons = SetupButtons();
        List<TaskDialogNativeMethods.TASKDIALOG_BUTTON> radioButtons = SetupRadioButtons();

        SetupIcon();

        try
        {
            MarshalButtons(buttons, out var pButtons, out _config.cButtons);
            _config.pButtons = pButtons;
            MarshalButtons(radioButtons, out var pRadioButtons, out _config.cRadioButtons);
            _config.pRadioButtons = pRadioButtons;
            int buttonId;
            int radioButton;
            bool verificationFlagChecked;
            using (new ComCtlv6ActivationContext(true))
            {
                TaskDialogNativeMethods.TaskDialogIndirect(ref _config, out buttonId, out radioButton, out verificationFlagChecked);
            }
            IsVerificationChecked = verificationFlagChecked;

            if (_radioButtonsById.TryGetValue(radioButton, out TaskDialogRadioButton? selectedRadioButton))
                selectedRadioButton.Checked = true;

            if (_buttonsById.TryGetValue(buttonId, out TaskDialogButton? selectedButton))
                return selectedButton;
            else
                return null!;
        }
        finally
        {
            nint pButtons = _config.pButtons;
            nint pRadioButtons = _config.pRadioButtons;
            CleanUpButtons(ref pButtons, ref _config.cButtons);
            CleanUpButtons(ref pRadioButtons, ref _config.cRadioButtons);
        }
    }

    /// <summary>
    /// Simulates a click on the verification checkbox of the <see cref="TaskDialog"/>, if it exists.
    /// </summary>
    /// <param name="checkState"><see langword="true" /> to set the state of the checkbox to be checked; <see langword="false" /> to set it to be unchecked.</param>
    /// <param name="setFocus"><see langword="true" /> to set the keyboard focus to the checkbox; otherwise <see langword="false" />.</param>
    /// <exception cref="InvalidOperationException">The task dialog is not being displayed.</exception>
    public void ClickVerification(bool checkState, bool setFocus)
    {
        if (!IsDialogRunning)
            throw new InvalidOperationException(TaskDialogResources.TaskDialogNotRunningError);

        TaskDialogNativeMethods.SendMessage(Handle, (int)TaskDialogNativeMethods.TaskDialogMessages.ClickVerification, (IntPtr)(int)(checkState ? 1 : 0), new IntPtr(setFocus ? 1 : 0));
    }

    #endregion Public methods

    #region Protected methods

    /// <summary>
    /// Raises the <see cref="HyperlinkClicked"/> event.
    /// </summary>
    /// <param name="e">The <see cref="HyperlinkClickedEventArgs"/> containing the data for the event.</param>
    protected virtual void OnHyperlinkClicked(HyperlinkClickedEventArgs e)
    {
        HyperlinkClicked?.Invoke(this, e);
    }

    /// <summary>
    /// Raises the <see cref="ButtonClicked"/> event.
    /// </summary>
    /// <param name="e">The <see cref="TaskDialogItemClickedEventArgs"/> containing the data for the event.</param>
    protected virtual void OnButtonClicked(TaskDialogItemClickedEventArgs e)
    {
        ButtonClicked?.Invoke(this, e);
    }

    /// <summary>
    /// Raises the <see cref="RadioButtonClicked"/> event.
    /// </summary>
    /// <param name="e">The <see cref="TaskDialogItemClickedEventArgs"/> containing the data for the event.</param>
    protected virtual void OnRadioButtonClicked(TaskDialogItemClickedEventArgs e)
    {
        RadioButtonClicked?.Invoke(this, e);
    }

    /// <summary>
    /// Raises the <see cref="VerificationClicked"/> event.
    /// </summary>
    /// <param name="e">The <see cref="EventArgs"/> containing the data for the event.</param>
    protected virtual void OnVerificationClicked(EventArgs e)
    {
        VerificationClicked?.Invoke(this, e);
    }

    /// <summary>
    /// Raises the <see cref="Created"/> event.
    /// </summary>
    /// <param name="e">The <see cref="EventArgs"/> containing the data for the event.</param>
    protected virtual void OnCreated(EventArgs e)
    {
        Created?.Invoke(this, e);
    }

    /// <summary>
    /// Raises the <see cref="Timer"/> event.
    /// </summary>
    /// <param name="e">The <see cref="TimerEventArgs"/> containing the data for the event.</param>
    protected virtual void OnTimer(TimerEventArgs e)
    {
        Timer?.Invoke(this, e);
    }

    /// <summary>
    /// Raises the <see cref="Destroyed"/> event.
    /// </summary>
    /// <param name="e">The <see cref="EventArgs"/> containing the data for the event.</param>
    protected virtual void OnDestroyed(EventArgs e)
    {
        Destroyed?.Invoke(this, e);
    }

    /// <summary>
    /// Raises the <see cref="ExpandButtonClicked"/> event.
    /// </summary>
    /// <param name="e">The <see cref="ExpandButtonClickedEventArgs"/> containing the data for the event.</param>
    protected virtual void OnExpandButtonClicked(ExpandButtonClickedEventArgs e)
    {
        ExpandButtonClicked?.Invoke(this, e);
    }

    /// <summary>
    /// Raises the <see cref="HelpRequested"/> event.
    /// </summary>
    /// <param name="e">The <see cref="EventArgs"/> containing the data for the event.</param>
    protected virtual void OnHelpRequested(EventArgs e)
    {
        HelpRequested?.Invoke(this, e);
    }

    #endregion Protected methods

    #region Internal Members

    internal void SetItemEnabled(TaskDialogItem item)
    {
        if (IsDialogRunning)
        {
            TaskDialogNativeMethods.SendMessage(Handle, (int)(item is TaskDialogButton ? TaskDialogNativeMethods.TaskDialogMessages.EnableButton : TaskDialogNativeMethods.TaskDialogMessages.EnableRadioButton), new IntPtr(item.Id), new IntPtr(item.Enabled ? 1 : 0));
        }
    }

    internal void SetButtonElevationRequired(TaskDialogButton button)
    {
        if (IsDialogRunning)
        {
            TaskDialogNativeMethods.SendMessage(Handle, (int)TaskDialogNativeMethods.TaskDialogMessages.SetButtonElevationRequiredState, (IntPtr)(int)button.Id, new IntPtr(button.ElevationRequired ? 1 : 0));
        }
    }

    internal void ClickItem(TaskDialogItem item)
    {
        if (!IsDialogRunning)
            throw new InvalidOperationException(TaskDialogResources.TaskDialogNotRunningError);

        TaskDialogNativeMethods.SendMessage(Handle, (int)(item is TaskDialogButton ? TaskDialogNativeMethods.TaskDialogMessages.ClickButton : TaskDialogNativeMethods.TaskDialogMessages.ClickRadioButton), (IntPtr)(int)item.Id, IntPtr.Zero);
    }

    #endregion Internal Members

    #region Private members

    internal void UpdateDialog()
    {
        if (IsDialogRunning)
        {
            // If the navigate page message is sent from within the callback, the navigation won't
            // take place until the callback returns. Any further messages sent after the navigate
            // page message before the end of the callback will then be lost as the navigation occurs.
            // For that reason, we defer it all the way until the end.
            if (_inEventHandler > 0)
                _updatePending = true;
            else
            {
                _updatePending = false;
                var pButtons = (IntPtr)_config.pButtons;
                var pRadioButtons = (IntPtr)_config.pRadioButtons;
                CleanUpButtons(ref pButtons, ref _config.cButtons);
                CleanUpButtons(ref pRadioButtons, ref _config.cRadioButtons);
                _config.dwCommonButtons = 0;

                List<TaskDialogNativeMethods.TASKDIALOG_BUTTON> buttons = SetupButtons();
                List<TaskDialogNativeMethods.TASKDIALOG_BUTTON> radioButtons = SetupRadioButtons();

                SetupIcon();

                MarshalButtons(buttons, out pButtons, out _config.cButtons);
                _config.pButtons = pButtons;
                MarshalButtons(radioButtons, out pRadioButtons, out _config.cRadioButtons);
                _config.pRadioButtons = pRadioButtons;

                int size = Marshal.SizeOf(_config);
                nint memory = Marshal.AllocHGlobal(size);
                try
                {
                    Marshal.StructureToPtr(_config, memory, false);
                    TaskDialogNativeMethods.SendMessage(Handle, (int)TaskDialogNativeMethods.TaskDialogMessages.NavigatePage, IntPtr.Zero, memory);
                }
                finally
                {
#pragma warning disable CA2263 // Prefer generic overload when type is known
                    Marshal.DestroyStructure(memory, typeof(TaskDialogNativeMethods.TASKDIALOGCONFIG));
#pragma warning restore CA2263 // Prefer generic overload when type is known
                    Marshal.FreeHGlobal(memory);
                }
            }
        }
    }

    private bool IsDialogRunning
    {
        get
        {
            // Intentially not using the Handle property, since the cross-thread call check should not be performed here.
            return _handle != IntPtr.Zero;
        }
    }

    private void SetElementText(TaskDialogNativeMethods.TaskDialogElements element, string text)
    {
        if (IsDialogRunning)
        {
            nint newTextPtr = Marshal.StringToHGlobalUni(text);
            try
            {
                nint result = TaskDialogNativeMethods.SendMessage(Handle, (int)TaskDialogNativeMethods.TaskDialogMessages.SetElementText, (nint)element, newTextPtr);
            }
            finally
            {
                if (newTextPtr != IntPtr.Zero)
                    Marshal.FreeHGlobal(newTextPtr);
            }
        }
    }

    private void SetupIcon()
    {
        SetupIcon(MainIcon, CustomMainIcon, TaskDialogNativeMethods.TaskDialogFlags.UseHIconMain);
        SetupIcon(FooterIcon, CustomFooterIcon, TaskDialogNativeMethods.TaskDialogFlags.UseHIconFooter);
    }

    private void SetupIcon(TaskDialogIcon icon, nint customIcon, TaskDialogNativeMethods.TaskDialogFlags flag)
    {
        SetFlag(flag, false);
        if (icon == TaskDialogIcon.Custom)
        {
            if (customIcon != 0)
            {
                SetFlag(flag, true);
                if (flag == TaskDialogNativeMethods.TaskDialogFlags.UseHIconMain)
                    _config.hMainIcon = customIcon;
                else
                    _config.hFooterIcon = customIcon;
            }
        }
        else
        {
            if (flag == TaskDialogNativeMethods.TaskDialogFlags.UseHIconMain)
                _config.hMainIcon = new IntPtr((int)icon);
            else
                _config.hFooterIcon = new IntPtr((int)icon);
        }
    }

    private static void CleanUpButtons(ref IntPtr buttons, ref uint count)
    {
        if (buttons != IntPtr.Zero)
        {
            int elementSize = Marshal.SizeOf<TaskDialogNativeMethods.TASKDIALOG_BUTTON>();
            for (int x = 0; x < count; ++x)
            {
                // This'll be safe until they introduce 128 bit machines. :)
                // It's the only way to do it without unsafe code.
                nint offset = (nint)(buttons.ToInt64() + x * elementSize);
#pragma warning disable CA2263 // Prefer generic overload when type is known
                Marshal.DestroyStructure(offset, typeof(TaskDialogNativeMethods.TASKDIALOG_BUTTON));
#pragma warning restore CA2263 // Prefer generic overload when type is known
            }
            Marshal.FreeHGlobal(buttons);
            buttons = IntPtr.Zero;
            count = 0;
        }
    }

    private static void MarshalButtons(List<TaskDialogNativeMethods.TASKDIALOG_BUTTON> buttons, out nint buttonsPtr, out uint count)
    {
        buttonsPtr = IntPtr.Zero;
        count = 0;
        if (buttons.Count > 0)
        {
            int elementSize = Marshal.SizeOf<TaskDialogNativeMethods.TASKDIALOG_BUTTON>();
            buttonsPtr = Marshal.AllocHGlobal(elementSize * buttons.Count);
            for (int x = 0; x < buttons.Count; ++x)
            {
                // This'll be safe until they introduce 128 bit machines. :)
                // It's the only way to do it without unsafe code.
                nint offset = (nint)((long)buttonsPtr + x * elementSize);
                Marshal.StructureToPtr(buttons[x], offset, false);
            }
            count = (uint)buttons.Count;
        }
    }

    private List<TaskDialogNativeMethods.TASKDIALOG_BUTTON> SetupButtons()
    {
        _buttonsById = [];
        List<TaskDialogNativeMethods.TASKDIALOG_BUTTON> buttons = [];
        _config.nDefaultButton = 0;
        foreach (TaskDialogButton button in Buttons)
        {
            if (button.Id < 1)
                throw new InvalidOperationException(TaskDialogResources.InvalidTaskDialogItemIdError);
            _buttonsById.Add(button.Id, button);
            if (button.Default)
                _config.nDefaultButton = button.Id;
            if (button.ButtonType == ButtonType.Custom)
            {
                if (string.IsNullOrEmpty(button.Text))
                    throw new InvalidOperationException(TaskDialogResources.TaskDialogEmptyButtonLabelError);

                TaskDialogNativeMethods.TASKDIALOG_BUTTON taskDialogButton = new()
                {
                    nButtonID = button.Id,
                    pszButtonText = button.Text,
                };
                if (ButtonStyle == TaskDialogButtonStyle.CommandLinks || ButtonStyle == TaskDialogButtonStyle.CommandLinksNoIcon && !string.IsNullOrEmpty(button.CommandLinkNote))
                    taskDialogButton.pszButtonText += "\n" + button.CommandLinkNote;

                buttons.Add(taskDialogButton);
            }
            else
            {
                _config.dwCommonButtons |= button.ButtonFlag;
            }
        }
        return buttons;
    }

    private List<TaskDialogNativeMethods.TASKDIALOG_BUTTON> SetupRadioButtons()
    {
        _radioButtonsById = [];
        List<TaskDialogNativeMethods.TASKDIALOG_BUTTON> radioButtons = [];
        _config.nDefaultRadioButton = 0;
        foreach (TaskDialogRadioButton radioButton in RadioButtons)
        {
            if (string.IsNullOrEmpty(radioButton.Text))
                throw new InvalidOperationException(TaskDialogResources.TaskDialogEmptyButtonLabelError);
            if (radioButton.Id < 1)
                throw new InvalidOperationException(TaskDialogResources.InvalidTaskDialogItemIdError);
            _radioButtonsById.Add(radioButton.Id, radioButton);
            if (radioButton.Checked)
                _config.nDefaultRadioButton = radioButton.Id;
            TaskDialogNativeMethods.TASKDIALOG_BUTTON taskDialogButton = new()
            {
                nButtonID = radioButton.Id,
                pszButtonText = radioButton.Text,
            };
            radioButtons.Add(taskDialogButton);
        }
        SetFlag(TaskDialogNativeMethods.TaskDialogFlags.NoDefaultRadioButton, _config.nDefaultRadioButton == 0);
        return radioButtons;
    }

    private void SetFlag(TaskDialogNativeMethods.TaskDialogFlags flag, bool value)
    {
        if (value)
            _config.dwFlags |= flag;
        else
            _config.dwFlags &= ~flag;
    }

    private bool GetFlag(TaskDialogNativeMethods.TaskDialogFlags flag)
    {
        return (_config.dwFlags & flag) != 0;
    }

    private uint TaskDialogCallback(nint hwnd, uint uNotification, nint wParam, nint lParam, nint dwRefData)
    {
        Interlocked.Increment(ref _inEventHandler);
        try
        {
            switch ((TaskDialogNativeMethods.TaskDialogNotifications)uNotification)
            {
                case TaskDialogNativeMethods.TaskDialogNotifications.Created:
                    _handle = hwnd;
                    DialogCreated();
                    TaskDialogThemeHost.RegisterDialog(hwnd);
                    OnCreated(EventArgs.Empty);
                    break;

                case TaskDialogNativeMethods.TaskDialogNotifications.Destroyed:
                    TaskDialogThemeHost.UnregisterDialog(hwnd);
                    _handle = IntPtr.Zero;
                    OnDestroyed(EventArgs.Empty);
                    break;

                case TaskDialogNativeMethods.TaskDialogNotifications.Navigated:
                    DialogCreated();
                    TaskDialogThemeHost.ApplyTheme(hwnd);
                    break;

                case TaskDialogNativeMethods.TaskDialogNotifications.HyperlinkClicked:
                    string? url = Marshal.PtrToStringUni(lParam);
                    OnHyperlinkClicked(new HyperlinkClickedEventArgs(url ?? string.Empty));
                    break;

                case TaskDialogNativeMethods.TaskDialogNotifications.ButtonClicked:
                    if (_buttonsById.TryGetValue((int)wParam, out TaskDialogButton? button))
                    {
                        TaskDialogItemClickedEventArgs e = new(button);
                        OnButtonClicked(e);
                        if (e.Cancel)
                            return 1;
                    }
                    break;

                case TaskDialogNativeMethods.TaskDialogNotifications.VerificationClicked:
                    IsVerificationChecked = ((int)wParam) == 1;
                    OnVerificationClicked(EventArgs.Empty);
                    break;

                case TaskDialogNativeMethods.TaskDialogNotifications.RadioButtonClicked:
                    if (_radioButtonsById.TryGetValue((int)wParam, out TaskDialogRadioButton? radioButton))
                    {
                        radioButton.Checked = true; // there's no way to click a radio button without checking it, is there?
                        TaskDialogItemClickedEventArgs e = new(radioButton);
                        OnRadioButtonClicked(e);
                    }
                    break;

                case TaskDialogNativeMethods.TaskDialogNotifications.Timer:
                    TimerEventArgs timerEventArgs = new((int)wParam);
                    OnTimer(timerEventArgs);
                    return (uint)(timerEventArgs.ResetTickCount ? 1 : 0);

                case TaskDialogNativeMethods.TaskDialogNotifications.ExpandoButtonClicked:
                    OnExpandButtonClicked(new ExpandButtonClickedEventArgs(wParam != 0));
                    break;

                case TaskDialogNativeMethods.TaskDialogNotifications.Help:
                    OnHelpRequested(EventArgs.Empty);
                    break;
            }
            return 0;
        }
        finally
        {
            Interlocked.Decrement(ref _inEventHandler);
            if (_updatePending)
                UpdateDialog();
        }
    }

    private void DialogCreated()
    {
        if (_config.hwndParent == IntPtr.Zero && _windowIcon != 0)
        {
            TaskDialogNativeMethods.SendMessage(Handle, TaskDialogNativeMethods.WM_SETICON, TaskDialogNativeMethods.ICON_SMALL, _windowIcon);
        }

        foreach (TaskDialogButton button in Buttons)
        {
            if (!button.Enabled)
                SetItemEnabled(button);
            if (button.ElevationRequired)
                SetButtonElevationRequired(button);
        }
        UpdateProgressBarStyle();
        UpdateProgressBarMarqueeSpeed();
        UpdateProgressBarRange();
        UpdateProgressBarValue();
        UpdateProgressBarState();
    }

    private void UpdateProgressBarStyle()
    {
        if (IsDialogRunning)
        {
            TaskDialogNativeMethods.SendMessage(Handle, (int)TaskDialogNativeMethods.TaskDialogMessages.SetMarqueeProgressBar, (IntPtr)(int)(ProgressBarStyle == ProgressBarStyle.MarqueeProgressBar ? 1 : 0), IntPtr.Zero);
        }
    }

    private void UpdateProgressBarMarqueeSpeed()
    {
        if (IsDialogRunning)
        {
            TaskDialogNativeMethods.SendMessage(Handle, (int)TaskDialogNativeMethods.TaskDialogMessages.SetProgressBarMarquee, (IntPtr)(int)(ProgressBarMarqueeAnimationSpeed > 0 ? 1 : 0), (IntPtr)ProgressBarMarqueeAnimationSpeed);
        }
    }

    private void UpdateProgressBarRange()
    {
        if (IsDialogRunning)
        {
            TaskDialogNativeMethods.SendMessage(Handle, (int)TaskDialogNativeMethods.TaskDialogMessages.SetProgressBarRange, 0, new IntPtr(ProgressBarMaximum << 16 | ProgressBarMinimum));
        }
        if (ProgressBarValue < ProgressBarMinimum)
            ProgressBarValue = ProgressBarMinimum;
        if (ProgressBarValue > ProgressBarMaximum)
            ProgressBarValue = ProgressBarMaximum;
    }

    private void UpdateProgressBarValue()
    {
        if (IsDialogRunning)
        {
            TaskDialogNativeMethods.SendMessage(Handle, (int)TaskDialogNativeMethods.TaskDialogMessages.SetProgressBarPos, (IntPtr)(int)ProgressBarValue, IntPtr.Zero);
        }
    }

    private void UpdateProgressBarState()
    {
        if (IsDialogRunning)
        {
            TaskDialogNativeMethods.SendMessage(Handle, (int)TaskDialogNativeMethods.TaskDialogMessages.SetProgressBarState, new IntPtr((int)ProgressBarState + 1), IntPtr.Zero);
        }
    }

    private void CheckCrossThreadCall()
    {
        IntPtr handle = _handle;
        if (handle != IntPtr.Zero)
        {
            var windowThreadId = TaskDialogNativeMethods.GetWindowThreadProcessId(handle, out _);
            var threadId = TaskDialogNativeMethods.GetCurrentThreadId();
            if (windowThreadId != threadId)
                throw new InvalidOperationException(TaskDialogResources.TaskDialogIllegalCrossThreadCallError);
        }
    }

    #endregion Private members

    #region Handle

    /// <summary>
    /// Gets the window handle of the task dialog.
    /// </summary>
    /// <value>
    /// The window handle of the task dialog when it is being displayed, or zero when the dialog
    /// is not being displayed.
    /// </value>
    [Browsable(false)]
    public nint Handle
    {
        get
        {
            CheckCrossThreadCall();
            return _handle;
        }
    }

    #endregion Handle
}

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
