using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MaxMind.Db.Helpers;

internal class MemoryByteEqualityComparer : IEqualityComparer<ReadOnlyMemory<byte>>
{
    public static MemoryByteEqualityComparer Instance { get; } = new();

    public bool Equals(ReadOnlyMemory<byte> x, ReadOnlyMemory<byte> y)
    {
        return x.Span.SequenceEqual(y.Span);
    }

    public int GetHashCode([DisallowNull] ReadOnlyMemory<byte> obj)
    {
        return obj.Span.GetHashCodeFast();
    }
}