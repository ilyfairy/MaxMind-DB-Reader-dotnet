﻿#region

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

#endregion

namespace MaxMind.Db
{
    internal sealed class ArrayBuffer : Buffer
    {
        private readonly byte[] _fileBytes;

        public ArrayBuffer(byte[] array)
        {
            Length = array.LongLength;
            _fileBytes = array;
        }

        public ArrayBuffer(string file) : this(File.ReadAllBytes(file))
        {
        }

        internal ArrayBuffer(Stream stream) : this(BytesFromStream(stream))
        {
        }

        public static async Task<ArrayBuffer> CreateAsync(string file)
        {
            using var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
            return await CreateAsync(stream).ConfigureAwait(false);
        }

        internal static async Task<ArrayBuffer> CreateAsync(Stream stream)
        {
            return new ArrayBuffer(await BytesFromStreamAsync(stream).ConfigureAwait(false));
        }

        private static byte[] BytesFromStream(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream), "The database stream must not be null.");
            }

            byte[] bytes;

            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                bytes = memoryStream.ToArray();
            }

            if (bytes.Length == 0)
            {
                throw new InvalidDatabaseException(
                    "There are zero bytes left in the stream. Perhaps you need to reset the stream's position.");
            }

            return bytes;
        }

        private static async Task<byte[]> BytesFromStreamAsync(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream), "The database stream must not be null.");
            }

            byte[] bytes;

            using (var memoryStream = new MemoryStream())
            {
                await stream.CopyToAsync(memoryStream).ConfigureAwait(false);
                bytes = memoryStream.ToArray();
            }

            if (bytes.Length == 0)
            {
                throw new InvalidDatabaseException(
                    "There are zero bytes left in the stream. Perhaps you need to reset the stream's position.");
            }

            return bytes;
        }

        public override byte[] Read(long offset, int count)
        {
            var bytes = new byte[count];

            if (bytes.Length > 0)
            {
                Array.Copy(_fileBytes, offset, bytes, 0, bytes.Length);
            }

            return bytes;
        }

        public override byte ReadOne(long offset) => _fileBytes[offset];

        public override string ReadString(long offset, int count)
            => Encoding.UTF8.GetString(_fileBytes, (int)offset, count);

        /// <summary>
        ///     Read an int from the buffer.
        /// </summary>
        public override int ReadInt(long offset)
        {
            return _fileBytes[offset] << 24 |
                   _fileBytes[offset + 1] << 16 |
                   _fileBytes[offset + 2] << 8 |
                   _fileBytes[offset + 3];
        }

        /// <summary>
        ///     Read a variable-sized int from the buffer.
        /// </summary>
        public override int ReadVarInt(long offset, int count)
        {
            return count switch
            {
                0 => 0,
                1 => _fileBytes[offset],
                2 => _fileBytes[offset] << 8 |
                     _fileBytes[offset + 1],
                3 => _fileBytes[offset] << 16 |
                     _fileBytes[offset + 1] << 8 |
                     _fileBytes[offset + 2],
                4 => ReadInt(offset),
                _ => throw new InvalidDatabaseException($"Unexpected int32 of size {count}"),
            };
        }
    }
}