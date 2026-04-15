#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8618, CS8619, CS8625

using System;
using System.Diagnostics.CodeAnalysis;

namespace Wpf.Ui.Violeta.Controls.Compat;

internal static partial class Utility
{
    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
    public static int GET_X_LPARAM(IntPtr lParam)
    {
        return LOWORD(lParam.ToInt32());
    }

    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
    public static int GET_Y_LPARAM(IntPtr lParam)
    {
        return HIWORD(lParam.ToInt32());
    }

    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
    public static int HIWORD(int i)
    {
        return (short)(i >> 16);
    }

    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
    public static int LOWORD(int i)
    {
        return (short)(i & 0xFFFF);
    }
}
