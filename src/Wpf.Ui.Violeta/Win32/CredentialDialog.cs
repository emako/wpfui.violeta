using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace Wpf.Ui.Violeta.Win32;

/// <summary>
/// Represents a Windows Security dialog for entering generic credentials.
/// </summary>
public sealed class CredentialDialog
{
    private const int ErrorCancelled = 1223;
    private const int ErrorNotFound = 1168;
    private const int CredUiMaxUserNameLength = 513;
    private const int CredUiMaxPasswordLength = 256;

    private static readonly Dictionary<string, NetworkCredential> ApplicationInstanceCredentialCache = [];

    private readonly NetworkCredential _credentials = new();
    private string? _confirmTarget;
    private bool _isSaveChecked;

    /// <summary>Occurs when <see cref="UserName"/> changes.</summary>
    public event EventHandler? UserNameChanged;

    /// <summary>Occurs when <see cref="Password"/> changes.</summary>
    public event EventHandler? PasswordChanged;

    /// <summary>Gets or sets whether an in-memory application credential cache is used.</summary>
    public bool UseApplicationInstanceCredentialCache { get; set; }

    /// <summary>Gets or sets whether the Save password checkbox is checked.</summary>
    public bool IsSaveChecked
    {
        get => _isSaveChecked;
        set
        {
            _confirmTarget = null;
            _isSaveChecked = value;
        }
    }

    /// <summary>Gets the password returned by the dialog.</summary>
    public string Password
    {
        get => _credentials.Password;
        private set
        {
            _confirmTarget = null;
            _credentials.Password = value;
            PasswordChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>Gets or sets optional entropy used to protect saved passwords.</summary>
    public byte[]? AdditionalEntropy { get; set; }

    /// <summary>Gets the credentials returned by the dialog.</summary>
    public NetworkCredential Credentials => _credentials;

    /// <summary>Gets the user name returned by the dialog.</summary>
    public string UserName
    {
        get => _credentials.UserName ?? string.Empty;
        private set
        {
            _confirmTarget = null;
            _credentials.UserName = value;
            UserNameChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>Gets or sets the target name under which credentials are stored.</summary>
    public string Target
    {
        get;
        set
        {
            field = value ?? string.Empty;
            _confirmTarget = null;
        }
    } = string.Empty;

    /// <summary>Gets or sets a legacy title for pre-Vista credential dialogs.</summary>
    public string WindowTitle { get; set; } = string.Empty;

    /// <summary>Gets or sets the main instruction displayed by Windows Security.</summary>
    public string MainInstruction { get; set; } = string.Empty;

    /// <summary>Gets or sets supplemental content displayed by the dialog.</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>Gets or sets how text is displayed by pre-Vista credential dialogs.</summary>
    public DownlevelTextMode DownlevelTextMode { get; set; } = DownlevelTextMode.MainInstructionAndContent;

    /// <summary>Gets or sets whether the dialog displays a Save password checkbox.</summary>
    public bool ShowSaveCheckBox { get; set; }

    /// <summary>Gets or sets whether the dialog appears when stored credentials exist.</summary>
    public bool ShowUIForSavedCredentials { get; set; }

    /// <summary>Gets whether the current credentials were loaded from a credential store.</summary>
    public bool IsStoredCredential { get; private set; }

    /// <summary>Shows the credential dialog using the active window as owner.</summary>
    public bool ShowDialog() => ShowDialog(0);

    /// <summary>Shows the credential dialog using the specified owner window.</summary>
    public bool ShowDialog(nint owner)
    {
        if (string.IsNullOrWhiteSpace(Target))
        {
            throw new InvalidOperationException("A non-empty credential target is required.");
        }

        UserName = string.Empty;
        Password = string.Empty;
        IsStoredCredential = false;

        if (UseApplicationInstanceCredentialCache && TryGetApplicationCredential(Target, out var cachedCredential))
        {
            SetCredentials(cachedCredential);
            IsStoredCredential = true;
            _confirmTarget = Target;
            return true;
        }

        NetworkCredential? storedCredential = null;
        var hadStoredCredentials = ShowSaveCheckBox && TryLoadCredential(Target, AdditionalEntropy, out storedCredential);
        if (hadStoredCredentials)
        {
            SetCredentials(storedCredential!);
            IsSaveChecked = true;
            if (!ShowUIForSavedCredentials)
            {
                IsStoredCredential = true;
                _confirmTarget = Target;
                return true;
            }
        }

        return PromptForCredentials(owner == 0 ? User32.GetActiveWindow() : owner, hadStoredCredentials);
    }

    /// <summary>Confirms whether credentials accepted by the dialog are valid.</summary>
    public void ConfirmCredentials(bool confirm)
    {
        if (_confirmTarget is null || !string.Equals(_confirmTarget, Target, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("The credential dialog was not accepted for the current target.");
        }

        _confirmTarget = null;
        if (!IsSaveChecked || !confirm)
        {
            return;
        }

        if (UseApplicationInstanceCredentialCache)
        {
            lock (ApplicationInstanceCredentialCache)
            {
                ApplicationInstanceCredentialCache[Target] = new NetworkCredential(UserName, Password);
            }
        }

        StoreCredential(Target, Credentials, AdditionalEntropy);
    }

    /// <summary>Stores generic credentials for the current Windows user.</summary>
    public static void StoreCredential(string target, NetworkCredential credential, byte[]? additionalEntropy = null)
    {
        ValidateTarget(target);

        _ = credential ?? throw new ArgumentNullException(nameof(credential));

        var protectedPassword = ProtectedData.Protect(
            Encoding.UTF8.GetBytes(credential.Password ?? string.Empty),
            additionalEntropy,
            DataProtectionScope.CurrentUser);
        var passwordBuffer = Marshal.AllocHGlobal(protectedPassword.Length);
        try
        {
            Marshal.Copy(protectedPassword, 0, passwordBuffer, protectedPassword.Length);
            var nativeCredential = new NativeCredential
            {
                Type = CredentialType.Generic,
                TargetName = target,
                CredentialBlobSize = (uint)protectedPassword.Length,
                CredentialBlob = passwordBuffer,
                Persist = CredentialPersist.Enterprise,
                UserName = credential.UserName ?? string.Empty,
            };

            if (!CredWrite(ref nativeCredential, 0))
            {
                throw new CredentialException(Marshal.GetLastWin32Error());
            }
        }
        finally
        {
            Marshal.FreeHGlobal(passwordBuffer);
        }
    }

    /// <summary>Retrieves credentials for the target, or <see langword="null"/> if none exist.</summary>
    public static NetworkCredential? RetrieveCredential(string target, byte[]? additionalEntropy = null)
    {
        ValidateTarget(target);
        if (TryGetApplicationCredential(target, out var cachedCredential))
        {
            return cachedCredential;
        }

        if (!CredRead(target, CredentialType.Generic, 0, out var credentialPointer))
        {
            var error = Marshal.GetLastWin32Error();
            return error == ErrorNotFound ? null : throw new CredentialException(error);
        }

        try
        {
            var nativeCredential = Marshal.PtrToStructure<NativeCredential>(credentialPointer);
            var encryptedPassword = new byte[nativeCredential.CredentialBlobSize];
            if (encryptedPassword.Length > 0)
            {
                Marshal.Copy(nativeCredential.CredentialBlob, encryptedPassword, 0, encryptedPassword.Length);
            }

            string password;
            try
            {
                password = Encoding.UTF8.GetString(ProtectedData.Unprotect(encryptedPassword, additionalEntropy, DataProtectionScope.CurrentUser));
            }
            catch (CryptographicException)
            {
                password = string.Empty;
            }

            return new NetworkCredential(nativeCredential.UserName ?? string.Empty, password);
        }
        finally
        {
            CredFree(credentialPointer);
        }
    }

    /// <summary>Retrieves credentials from the application-instance cache only.</summary>
    public static NetworkCredential? RetrieveCredentialFromApplicationInstanceCache(string target)
    {
        ValidateTarget(target);
        return TryGetApplicationCredential(target, out var credential) ? credential : null;
    }

    /// <summary>Deletes credentials from the application cache and Windows Credential Manager.</summary>
    public static bool DeleteCredential(string target)
    {
        ValidateTarget(target);
        bool found;
        lock (ApplicationInstanceCredentialCache)
        {
            found = ApplicationInstanceCredentialCache.Remove(target);
        }

        if (CredDelete(target, CredentialType.Generic, 0))
        {
            return true;
        }

        var error = Marshal.GetLastWin32Error();
        if (error == ErrorNotFound)
        {
            return found;
        }

        throw new CredentialException(error);
    }

    private bool PromptForCredentials(nint owner, bool hadStoredCredentials)
    {
        var info = new CredentialUiInfo
        {
            cbSize = Marshal.SizeOf<CredentialUiInfo>(),
            hwndParent = owner,
            pszCaptionText = MainInstruction,
            pszMessageText = Content,
        };
        var flags = CredentialUiWindowsFlags.Generic;
        if (ShowSaveCheckBox)
        {
            flags |= CredentialUiWindowsFlags.CheckBox;
        }

        nint inputBuffer = 0;
        nint outputBuffer = 0;
        try
        {
            uint inputSize = 0;
            if (!string.IsNullOrEmpty(UserName))
            {
                if (!CredPackAuthenticationBuffer(0, UserName, Password, 0, ref inputSize) && Marshal.GetLastWin32Error() != 122)
                {
                    throw new CredentialException(Marshal.GetLastWin32Error());
                }

                inputBuffer = Marshal.AllocCoTaskMem((int)inputSize);
                if (!CredPackAuthenticationBuffer(0, UserName, Password, inputBuffer, ref inputSize))
                {
                    throw new CredentialException(Marshal.GetLastWin32Error());
                }
            }

            uint package = 0;
            bool saveChecked = IsSaveChecked;
            var result = CredUIPromptForWindowsCredentials(
                ref info,
                0,
                ref package,
                inputBuffer,
                inputSize,
                out outputBuffer,
                out var outputSize,
                ref saveChecked,
                flags);

            if (result == ErrorCancelled)
            {
                return false;
            }
            if (result != 0)
            {
                throw new CredentialException(result);
            }

            var userName = new StringBuilder(CredUiMaxUserNameLength);
            var password = new StringBuilder(CredUiMaxPasswordLength);
            uint userNameSize = CredUiMaxUserNameLength;
            uint passwordSize = CredUiMaxPasswordLength;
            uint domainSize = 0;
            if (!CredUnPackAuthenticationBuffer(0, outputBuffer, outputSize, userName, ref userNameSize, null, ref domainSize, password, ref passwordSize))
            {
                throw new CredentialException(Marshal.GetLastWin32Error());
            }

            UserName = userName.ToString();
            Password = password.ToString();
            IsSaveChecked = saveChecked;
            if (ShowSaveCheckBox)
            {
                _confirmTarget = Target;
                if (hadStoredCredentials && !IsSaveChecked)
                {
                    DeleteCredential(Target);
                }
            }

            return true;
        }
        finally
        {
            if (inputBuffer != 0) Marshal.FreeCoTaskMem(inputBuffer);
            if (outputBuffer != 0) Marshal.FreeCoTaskMem(outputBuffer);
        }
    }

    private void SetCredentials(NetworkCredential credential)
    {
        UserName = credential.UserName ?? string.Empty;
        Password = credential.Password ?? string.Empty;
    }

    private static bool TryGetApplicationCredential(string target, out NetworkCredential credential)
    {
        credential = null!;
        lock (ApplicationInstanceCredentialCache)
        {
            return ApplicationInstanceCredentialCache.TryGetValue(target, out credential!);
        }
    }

    private static void ValidateTarget(string target)
    {
        _ = target ?? throw new ArgumentNullException(nameof(target));

        if (target.Length == 0)
        {
            throw new ArgumentException("A non-empty credential target is required.", nameof(target));
        }
    }

    private static bool TryLoadCredential(string target, byte[]? additionalEntropy, out NetworkCredential? credential)
    {
        credential = RetrieveCredential(target, additionalEntropy);
        return credential is not null;
    }

    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool CredWrite(ref NativeCredential credential, uint flags);

    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool CredRead(string target, CredentialType type, uint flags, out nint credential);

    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool CredDelete(string target, CredentialType type, uint flags);

    [DllImport("advapi32.dll")]
    private static extern void CredFree(nint buffer);

    [DllImport("credui.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int CredUIPromptForWindowsCredentials(
        ref CredentialUiInfo info, uint authError, ref uint authPackage, nint inputAuthBuffer, uint inputAuthBufferSize,
        out nint outputAuthBuffer, out uint outputAuthBufferSize, [MarshalAs(UnmanagedType.Bool)] ref bool save, CredentialUiWindowsFlags flags);

    [DllImport("credui.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool CredPackAuthenticationBuffer(uint flags, string userName, string password, nint packedCredentials, ref uint packedCredentialsSize);

    [DllImport("credui.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool CredUnPackAuthenticationBuffer(
        uint flags, nint authBuffer, uint authBufferSize, StringBuilder? userName, ref uint userNameSize,
        StringBuilder? domainName, ref uint domainNameSize, StringBuilder? password, ref uint passwordSize);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct CredentialUiInfo
    {
        public int cbSize;
        public nint hwndParent;
        [MarshalAs(UnmanagedType.LPWStr)] public string? pszMessageText;
        [MarshalAs(UnmanagedType.LPWStr)] public string? pszCaptionText;
        public nint hbmBanner;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct NativeCredential
    {
        public uint Flags;
        public CredentialType Type;
        [MarshalAs(UnmanagedType.LPWStr)] public string TargetName;
        [MarshalAs(UnmanagedType.LPWStr)] public string? Comment;
        public long LastWritten;
        public uint CredentialBlobSize;
        public nint CredentialBlob;
        public CredentialPersist Persist;
        public uint AttributeCount;
        public nint Attributes;
        [MarshalAs(UnmanagedType.LPWStr)] public string? TargetAlias;
        [MarshalAs(UnmanagedType.LPWStr)] public string UserName;
    }

    private enum CredentialType : uint { Generic = 1 }

    private enum CredentialPersist : uint { Enterprise = 3 }

    [Flags]
    private enum CredentialUiWindowsFlags : uint
    {
        Generic = 0x1,
        CheckBox = 0x2,
    }
}

/// <summary>Represents an error returned by Windows Credential Manager or CredUI.</summary>
public class CredentialException : Win32Exception
{
    /// <summary>Initializes a new credential exception.</summary>
    public CredentialException() { }

    /// <summary>Initializes a new credential exception for a Windows error code.</summary>
    public CredentialException(int error) : base(error) { }

    /// <summary>Initializes a new credential exception with a message.</summary>
    public CredentialException(string message) : base(message) { }
}

/// <summary>Specifies how text is presented by legacy credential dialogs.</summary>
public enum DownlevelTextMode
{
    /// <summary>Shows the main instruction and content.</summary>
    MainInstructionAndContent,

    /// <summary>Shows only the main instruction.</summary>
    MainInstructionOnly,

    /// <summary>Shows only the content.</summary>
    ContentOnly,
}
