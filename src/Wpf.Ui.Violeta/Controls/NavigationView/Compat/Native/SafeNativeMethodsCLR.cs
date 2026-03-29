using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Wpf.Ui.Violeta.Controls.Compat
{
    internal static partial class SafeNativeMethods
    {
        public static IntPtr MonitorFromRect(ref RECT rect, int flags)
        {
            return SafeNativeMethodsPrivate.MonitorFromRect(ref rect, flags);
        }

        internal static void GetWindowRect(HandleRef hWnd, [In, Out] ref RECT rect)
        {
            if (!SafeNativeMethodsPrivate.IntGetWindowRect(hWnd, ref rect))
            {
                throw new Win32Exception();
            }
        }

        private partial class SafeNativeMethodsPrivate
        {
            [DllImport(ExternDll.User32, EntryPoint = "GetWindowRect", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
            public static extern bool IntGetWindowRect(HandleRef hWnd, [In, Out] ref RECT rect);

            [DllImport(ExternDll.User32, ExactSpelling = true)]
            public static extern IntPtr MonitorFromRect(ref RECT rect, int flags);

            [DllImport(ExternDll.User32, EntryPoint = "ScreenToClient", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Auto)]
            public static extern int IntScreenToClient(HandleRef hWnd, [In, Out] POINT pt);
        }
    }
}

