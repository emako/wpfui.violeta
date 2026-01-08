using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

namespace Wpf.Ui.Violeta.Resources;

/// <summary>
/// Helper utilities for accessing embedded manifest resources from an
/// <see cref="Assembly"/>. Methods support checking existence, opening a
/// <see cref="Stream"/>, and reading resource contents as text or bytes.
/// When <paramref name="assembly"/> is not provided, the calling
/// assembly is inferred using a lightweight stack trace.
/// </summary>
public static class ManifestResourceProvider
{
    /// <summary>
    /// Determines whether the specified manifest resource exists in the given assembly.
    /// </summary>
    /// <param name="name">Fully qualified resource name as embedded in the assembly.</param>
    /// <param name="assembly">Optional assembly to search for the resource; inferred from the caller when null.</param>
    /// <returns><c>true</c> if the resource exists; otherwise, <c>false</c>.</returns>
    public static bool HasResource(string name, Assembly? assembly = null)
    {
        // If no assembly provided, try to infer the calling assembly by
        // inspecting the stack. This is a convenient default for callers
        // that want to access resources in their own assembly without
        // explicitly passing it.
        if (assembly == null)
        {
            StackTrace stackTrace = new(1);
            StackFrame? stackFrame = stackTrace.GetFrame(0);
            MethodBase? methodBase = stackFrame?.GetMethod()!;

            assembly = methodBase?.DeclaringType?.Assembly!;
        }

        return assembly.GetManifestResourceInfo(name) != null;
    }

    /// <summary>
    /// Opens the specified manifest resource as a <see cref="Stream"/>.
    /// </summary>
    /// <param name="name">Fully qualified resource name as embedded in the assembly.</param>
    /// <param name="assembly">Optional assembly containing the resource; inferred from the caller when null.</param>
    /// <returns>A <see cref="Stream"/> for reading the resource data.</returns>
    /// <exception cref="System.ArgumentNullException">Thrown if the resource is not found.</exception>
    public static Stream GetStream(string name, Assembly? assembly = null)
    {
        // Infer assembly when omitted (see HasResource comment).
        if (assembly == null)
        {
            StackTrace stackTrace = new(1);
            StackFrame? stackFrame = stackTrace.GetFrame(0);
            MethodBase? methodBase = stackFrame?.GetMethod()!;

            assembly = methodBase?.DeclaringType?.Assembly!;
        }

        // Throws if resource not found (caller may catch as needed).
        Stream stream = assembly.GetManifestResourceStream(name)!;
        return stream;
    }

    /// <summary>
    /// Reads the embedded manifest resource with the specified <paramref name="name"/>
    /// and returns its text content. If <paramref name="encoding"/> is
    /// omitted, UTF-8 is used.
    /// </summary>
    /// <param name="name">Fully qualified resource name as embedded in the assembly.</param>
    /// <param name="encoding">Optional text encoding to use when reading the stream.</param>
    /// <param name="assembly">Optional assembly containing the resource; inferred from the caller when null.</param>
    /// <returns>Resource content as a string.</returns>
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

    /// <summary>
    /// Reads the embedded manifest resource and returns its raw bytes.
    /// </summary>
    /// <param name="name">Fully qualified resource name as embedded in the assembly.</param>
    /// <param name="assembly">Optional assembly containing the resource; inferred from the caller when null.</param>
    /// <returns>Byte array containing the resource data.</returns>
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
