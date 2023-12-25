using System;
using System.Diagnostics.CodeAnalysis;

namespace MaxMind.Db.Helpers
{
    internal unsafe readonly struct ReadOnlyMemorySpan<T> where T : unmanaged
    {
        private readonly IntPtr ptr;
        private readonly Memory<T> memory;
        private readonly int length;
        private readonly bool isMemory;

        public ReadOnlyMemorySpan(IntPtr ptr, int length)
        {
            this.ptr = ptr;
            this.length = length;
            isMemory = false;
        }

        public ReadOnlyMemorySpan(Memory<T> memory)
        {
            this.memory = memory;
            isMemory = true;
        }

        public override int GetHashCode()
        {
            if (isMemory)
            {
                return Extensions.GetHashCodeFast<T>(memory.Span);
            }
            else
            {
                ReadOnlySpan<T> span = this;
                return Extensions.GetHashCodeFast(span);
            }
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (obj is not ReadOnlyMemorySpan<T> span)
                return false;

            ReadOnlySpan<T> a1 = this;
            ReadOnlySpan<T> a2 = span;
            return a1.SequenceEqual(a2);
        }

        public static implicit operator ReadOnlySpan<T>(ReadOnlyMemorySpan<T> unmanagedSpan)
        {
            if (unmanagedSpan.isMemory)
            {
                return unmanagedSpan.memory.Span;
            }
            else
            {
                return new((void*)unmanagedSpan.ptr, unmanagedSpan.length);
            }
        }
    }
}
