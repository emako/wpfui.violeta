using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

namespace Wpf.Ui.Violeta.Resources;

public static class ManifestResourceProvider
{
    public static bool HasResource(string name, Assembly? assembly = null)
    {
        if (assembly == null)
        {
            StackTrace stackTrace = new(1);
            StackFrame? stackFrame = stackTrace.GetFrame(0);
            MethodBase? methodBase = stackFrame?.GetMethod()!;

            assembly = methodBase?.DeclaringType?.Assembly!;
        }

        return assembly.GetManifestResourceInfo(name) != null;
    }

    public static Stream GetStream(string name, Assembly? assembly = null)
    {
        if (assembly == null)
        {
            StackTrace stackTrace = new(1);
            StackFrame? stackFrame = stackTrace.GetFrame(0);
            MethodBase? methodBase = stackFrame?.GetMethod()!;

            assembly = methodBase?.DeclaringType?.Assembly!;
        }

        Stream stream = assembly.GetManifestResourceStream(name)!;
        return stream;
    }

    public static string GetString(string name, Encoding? encoding = null, Assembly? assembly = null)
    {
        if (assembly == null)
        {
            StackTrace stackTrace = new(1);
            StackFrame? stackFrame = stackTrace.GetFrame(0);
            MethodBase? methodBase = stackFrame?.GetMethod()!;

            assembly = methodBase?.DeclaringType?.Assembly!;
        }

        using Stream stream = GetStream(name, assembly);
        using StreamReader streamReader = new(stream, encoding ?? Encoding.UTF8);
        return streamReader.ReadToEnd();
    }

    public static byte[] GetBytes(string name, Assembly? assembly = null)
    {
        if (assembly == null)
        {
            StackTrace stackTrace = new(1);
            StackFrame? stackFrame = stackTrace.GetFrame(0);
            MethodBase? methodBase = stackFrame?.GetMethod()!;

            assembly = methodBase?.DeclaringType?.Assembly!;
        }

        using Stream stream = GetStream(name, assembly);
        using BinaryReader reader = new(stream);
        return reader.ReadBytes((int)stream.Length);
    }
}
