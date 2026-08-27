#if NET462

#pragma warning disable IDE0130 // Namespace does not match folder structure

namespace System;

#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Polyfill for <see cref="ValueTuple{T1}"/> on .NET Framework 4.6.2.
/// </summary>
[Serializable]
public struct ValueTuple<T1>(T1 item1)
{
    public T1 Item1 = item1;
}

/// <summary>
/// Polyfill for <see cref="ValueTuple{T1,T2}"/> on .NET Framework 4.6.2.
/// </summary>
[Serializable]
public struct ValueTuple<T1, T2>(T1 item1, T2 item2)
{
    public T1 Item1 = item1;
    public T2 Item2 = item2;
}

/// <summary>
/// Polyfill for <see cref="ValueTuple{T1,T2,T3}"/> on .NET Framework 4.6.2.
/// </summary>
[Serializable]
public struct ValueTuple<T1, T2, T3>(T1 item1, T2 item2, T3 item3)
{
    public T1 Item1 = item1;
    public T2 Item2 = item2;
    public T3 Item3 = item3;
}

#endif
