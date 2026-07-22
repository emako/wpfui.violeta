using System.ComponentModel;

namespace System.Runtime.CompilerServices;

/// <summary>
/// Fix CS0518 IsExternalInit missing for framework without C#9 init support
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
internal static class IsExternalInit;
