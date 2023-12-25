using System;
using System.Buffers.Binary;
using System.Net;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace MaxMind.Db.Helpers;

internal static class Extensions
{
    public static ReadOnlySpan<byte> GetAddressReadOnlySpanBytes(this IPAddress ipAddress)
    {
        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_numbers")]
        static extern ref ushort[] GetIPAddress_numbers(IPAddress ipAddress);
        
        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_addressOrScopeId")]
        static extern ref uint GetIPAddress_addressOrScopeId(IPAddress ipAddress);


        var numbers = GetIPAddress_numbers(ipAddress);
        if (numbers != null)
        {
            // ipv6
            if (!BitConverter.IsLittleEndian)
            {
                return MemoryMarshal.AsBytes<ushort>(numbers);
            }
            else
            {
                var destination = new byte[16].AsSpan();
                if (Vector128.IsHardwareAccelerated)
                {
                    Vector128<ushort> vector = Vector128.LoadUnsafe<ushort>(ref MemoryMarshal.GetArrayDataReference<ushort>(numbers));
                    vector = Vector128.ShiftLeft(vector, 8) | Vector128.ShiftRightLogical(vector, 8);
                    vector.AsByte<ushort>().StoreUnsafe(ref MemoryMarshal.GetReference<byte>(destination));
                    return destination;
                }
                for (int i = 0; i < numbers.Length; i++)
                {
                    BinaryPrimitives.WriteUInt16BigEndian(destination.Slice(i * 2), numbers[i]);
                }
                return destination;
            }
        }
        else
        {
            //ipv4
            return MemoryMarshal.CreateSpan(ref Unsafe.As<uint,byte>(ref GetIPAddress_addressOrScopeId(ipAddress)), 4);
        }
    }

    public static int GetHashCodeFast<T>(this ReadOnlySpan<T> span) where T : unmanaged
    {
        var bytes = MemoryMarshal.AsBytes(span);
        ref byte r = ref MemoryMarshal.GetReference(bytes);

        int offset = 0;
        int hash = 5381;
        var length = bytes.Length;


        if (bytes.Length < Vector<byte>.Count)
        {
            while (length >= 8)
            {
                hash = unchecked(((hash << 5) + hash) ^ Unsafe.Add(ref r, offset + 0).GetHashCode());
                hash = unchecked(((hash << 5) + hash) ^ Unsafe.Add(ref r, offset + 1).GetHashCode());
                hash = unchecked(((hash << 5) + hash) ^ Unsafe.Add(ref r, offset + 2).GetHashCode());
                hash = unchecked(((hash << 5) + hash) ^ Unsafe.Add(ref r, offset + 3).GetHashCode());
                hash = unchecked(((hash << 5) + hash) ^ Unsafe.Add(ref r, offset + 4).GetHashCode());
                hash = unchecked(((hash << 5) + hash) ^ Unsafe.Add(ref r, offset + 5).GetHashCode());
                hash = unchecked(((hash << 5) + hash) ^ Unsafe.Add(ref r, offset + 6).GetHashCode());
                hash = unchecked(((hash << 5) + hash) ^ Unsafe.Add(ref r, offset + 7).GetHashCode());

                length -= 8;
                offset += 8;
            }

            if (length >= 4)
            {
                hash = unchecked(((hash << 5) + hash) ^ Unsafe.Add(ref r, offset + 0).GetHashCode());
                hash = unchecked(((hash << 5) + hash) ^ Unsafe.Add(ref r, offset + 1).GetHashCode());
                hash = unchecked(((hash << 5) + hash) ^ Unsafe.Add(ref r, offset + 2).GetHashCode());
                hash = unchecked(((hash << 5) + hash) ^ Unsafe.Add(ref r, offset + 3).GetHashCode());

                length -= 4;
                offset += 4;
            }

            while (length > 0)
            {
                hash = unchecked(((hash << 5) + hash) ^ Unsafe.Add(ref r, offset).GetHashCode());

                length -= 1;
                offset += 1;
            }

            return hash;
        }

        Vector<int> current = new Vector<int>(hash);
        while (offset + Vector<byte>.Count <= length)
        {
            var vec = Vector.LoadUnsafe(ref Unsafe.Add(ref r, offset)).As<byte, int>();
            current = ((current << 5) + current) ^ vec;

            offset += Vector<byte>.Count;
        }

        Vector<byte> end = current.As<int, byte>();
        for (int i = 0; i < Vector<byte>.Count / 8; i += 8)
        {
            hash = unchecked(((hash << 5) + hash) ^ end[i + 0]);
            hash = unchecked(((hash << 5) + hash) ^ end[i + 1]);
            hash = unchecked(((hash << 5) + hash) ^ end[i + 2]);
            hash = unchecked(((hash << 5) + hash) ^ end[i + 3]);
            hash = unchecked(((hash << 5) + hash) ^ end[i + 4]);
            hash = unchecked(((hash << 5) + hash) ^ end[i + 5]);
            hash = unchecked(((hash << 5) + hash) ^ end[i + 6]);
            hash = unchecked(((hash << 5) + hash) ^ end[i + 7]);
        }

        while (offset < length)
        {
            if (length - offset > 8)
            {
                hash = unchecked(((hash << 5) + hash) ^ bytes[0]);
                hash = unchecked(((hash << 5) + hash) ^ bytes[1]);
                hash = unchecked(((hash << 5) + hash) ^ bytes[2]);
                hash = unchecked(((hash << 5) + hash) ^ bytes[3]);
                hash = unchecked(((hash << 5) + hash) ^ bytes[4]);
                hash = unchecked(((hash << 5) + hash) ^ bytes[5]);
                hash = unchecked(((hash << 5) + hash) ^ bytes[6]);
                hash = unchecked(((hash << 5) + hash) ^ bytes[7]);
                offset += 8;
                continue;
            }
            else if (length - offset > 4)
            {
                hash = unchecked(((hash << 5) + hash) ^ bytes[0]);
                hash = unchecked(((hash << 5) + hash) ^ bytes[1]);
                hash = unchecked(((hash << 5) + hash) ^ bytes[2]);
                hash = unchecked(((hash << 5) + hash) ^ bytes[3]);
                offset += 8;
                continue;
            }

            hash = unchecked(((hash << 5) + hash) ^ bytes[offset].GetHashCode());
            offset++;
        }

        return hash;
    }
}
