#if NET462

using System.Collections.Generic;

#pragma warning disable IDE0130 // Namespace does not match folder structure

namespace System.Runtime.CompilerServices;

#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Polyfill for <see cref="TupleElementNamesAttribute"/> on .NET Framework 4.6.2.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue | AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class TupleElementNamesAttribute : Attribute
{
    public TupleElementNamesAttribute(string[] transformNames)
    {
        TransformNames = transformNames ?? throw new ArgumentNullException(nameof(transformNames));
    }

    public TupleElementNamesAttribute()
    {
        TransformNames = [];
    }

    public IList<string> TransformNames { get; }
}

#endif
