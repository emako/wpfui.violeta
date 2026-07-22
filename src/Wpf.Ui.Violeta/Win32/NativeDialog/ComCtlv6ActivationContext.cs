using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Wpf.Ui.Violeta.Win32;

sealed class ComCtlv6ActivationContext : IDisposable
{
    private nint _cookie;
    private static TaskDialogNativeMethods.ACTCTX _enableThemingActivationContext;
    private static ActivationContextSafeHandle? _activationContext;
    private static bool _contextCreationSucceeded;
    private static readonly object _contextCreationLock = new();

    public ComCtlv6ActivationContext(bool enable)
    {
        if (enable && TaskDialogNativeMethods.IsWindowsXPOrLater)
        {
            if (EnsureActivateContextCreated())
            {
                if (!TaskDialogNativeMethods.ActivateActCtx(_activationContext!, out _cookie))
                {
                    _cookie = IntPtr.Zero;
                }
            }
        }
    }

    ~ComCtlv6ActivationContext()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        _ = disposing; // Unused parameter

        if (_cookie != IntPtr.Zero)
        {
            if (TaskDialogNativeMethods.DeactivateActCtx(0, _cookie))
            {
                _cookie = IntPtr.Zero;
            }
        }
    }

    private static bool EnsureActivateContextCreated()
    {
        lock (_contextCreationLock)
        {
            if (_contextCreationSucceeded)
            {
                return _contextCreationSucceeded;
            }

            const string manifestResourceName = "Wpf.Ui.Violeta.app.manifest";
            string manifestTempFilePath;

            using (var manifest = typeof(ComCtlv6ActivationContext).Assembly.GetManifestResourceStream(manifestResourceName))
            {
                if (manifest is null)
                {
                    throw new InvalidOperationException($"Unable to retrieve {manifestResourceName} embedded resource");
                }

                manifestTempFilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

                using var tempFileStream = new FileStream(
                    manifestTempFilePath,
                    FileMode.CreateNew,
                    FileAccess.ReadWrite,
                    FileShare.Delete | FileShare.ReadWrite);
                manifest.CopyTo(tempFileStream);
            }

            _enableThemingActivationContext = new TaskDialogNativeMethods.ACTCTX
            {
                cbSize = Marshal.SizeOf<TaskDialogNativeMethods.ACTCTX>(),
                lpSource = manifestTempFilePath,
            };

            _activationContext = TaskDialogNativeMethods.CreateActCtx(ref _enableThemingActivationContext);
            _contextCreationSucceeded = !_activationContext.IsInvalid;

            try
            {
                File.Delete(manifestTempFilePath);
            }
            catch (Exception)
            {
                // Best-effort cleanup of the temporary manifest file.
            }

            return _contextCreationSucceeded;
        }
    }
}
