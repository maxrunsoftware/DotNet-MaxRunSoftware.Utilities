﻿// Copyright (c) 2024 Max Run Software (dev@maxrunsoftware.com)
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.IO.Compression;

namespace MaxRunSoftware.Utilities.Common;

public static class ExtensionsByte
{
    #region IsValidUTF8

    // ReSharper disable once InconsistentNaming
    public static bool IsValidUTF8(this byte[] buffer) => IsValidUTF8(buffer, buffer.Length);

    /// <summary></summary>
    /// <param name="buffer"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    // ReSharper disable once InconsistentNaming
    public static bool IsValidUTF8(this byte[] buffer, int length)
    {
        var position = 0;
        var bytes = 0;
        while (position < length)
        {
            if (!IsValidUTF8(buffer, position, length, ref bytes)) return false;

            position += bytes;
        }

        return true;
    }

    /// <summary></summary>
    /// <param name="buffer"></param>
    /// <param name="position"></param>
    /// <param name="length"></param>
    /// <param name="bytes"></param>
    /// <returns></returns>
    // ReSharper disable once InconsistentNaming
    public static bool IsValidUTF8(this byte[] buffer, int position, int length, ref int bytes)
    {
        if (length > buffer.Length) throw new ArgumentException("Invalid length");

        if (position > length - 1)
        {
            bytes = 0;
            return true;
        }

        var ch = buffer[position];

        if (ch <= 0x7F)
        {
            bytes = 1;
            return true;
        }

        if (ch >= 0xc2 && ch <= 0xdf)
        {
            if (position >= length - 2)
            {
                bytes = 0;
                return false;
            }

            if (buffer[position + 1] < 0x80 || buffer[position + 1] > 0xbf)
            {
                bytes = 0;
                return false;
            }

            bytes = 2;
            return true;
        }

        if (ch == 0xe0)
        {
            if (position >= length - 3)
            {
                bytes = 0;
                return false;
            }

            if (buffer[position + 1] < 0xa0 || buffer[position + 1] > 0xbf ||
                buffer[position + 2] < 0x80 || buffer[position + 2] > 0xbf)
            {
                bytes = 0;
                return false;
            }

            bytes = 3;
            return true;
        }

        if (ch >= 0xe1 && ch <= 0xef)
        {
            if (position >= length - 3)
            {
                bytes = 0;
                return false;
            }

            if (buffer[position + 1] < 0x80 || buffer[position + 1] > 0xbf ||
                buffer[position + 2] < 0x80 || buffer[position + 2] > 0xbf)
            {
                bytes = 0;
                return false;
            }

            bytes = 3;
            return true;
        }

        if (ch == 0xf0)
        {
            if (position >= length - 4)
            {
                bytes = 0;
                return false;
            }

            if (buffer[position + 1] < 0x90 || buffer[position + 1] > 0xbf ||
                buffer[position + 2] < 0x80 || buffer[position + 2] > 0xbf ||
                buffer[position + 3] < 0x80 || buffer[position + 3] > 0xbf)
            {
                bytes = 0;
                return false;
            }

            bytes = 4;
            return true;
        }

        if (ch == 0xf4)
        {
            if (position >= length - 4)
            {
                bytes = 0;
                return false;
            }

            if (buffer[position + 1] < 0x80 || buffer[position + 1] > 0x8f ||
                buffer[position + 2] < 0x80 || buffer[position + 2] > 0xbf ||
                buffer[position + 3] < 0x80 || buffer[position + 3] > 0xbf)
            {
                bytes = 0;
                return false;
            }

            bytes = 4;
            return true;
        }

        if (ch >= 0xf1 && ch <= 0xf3)
        {
            if (position >= length - 4)
            {
                bytes = 0;
                return false;
            }

            if (buffer[position + 1] < 0x80 || buffer[position + 1] > 0xbf ||
                buffer[position + 2] < 0x80 || buffer[position + 2] > 0xbf ||
                buffer[position + 3] < 0x80 || buffer[position + 3] > 0xbf)
            {
                bytes = 0;
                return false;
            }

            bytes = 4;
            return true;
        }

        return false;
    }

    #endregion IsValidUTF8

    #region Compression

    /// <summary>
    /// Compresses binary data
    /// </summary>
    /// <param name="data">Data to compress</param>
    /// <param name="compressionLevel">The level of compression</param>
    /// <returns>The compressed data</returns>
    public static byte[] CompressGZip(this byte[] data, CompressionLevel compressionLevel = CompressionLevel.Optimal)
    {
        using var streamWrite = new MemoryStream();
        using var streamCompression = new GZipStream(streamWrite, compressionLevel);
        
        streamCompression.Write(data, 0, data.Length);

        streamCompression.Flush();
        streamWrite.Flush();

        streamCompression.Close();
        streamWrite.Close();

        return streamWrite.ToArray();
    }

    /// <summary>
    /// Decompresses binary data
    /// </summary>
    /// <param name="data">The data to decompress</param>
    /// <returns>The decompressed data</returns>
    public static byte[] DecompressGZip(this byte[] data)
    {
        using var streamRead = new MemoryStream(data);
        using var streamCompression = new GZipStream(streamRead, CompressionMode.Decompress);
        using var streamWrite = new MemoryStream();
        
        streamCompression.CopyTo(streamWrite);
        streamWrite.Flush();
        streamWrite.Close();
        
        return streamWrite.ToArray();
    }

    #endregion Compression

    public static bool EqualsBytes(this Span<byte> x, Span<byte> y) => EqualsBytes((ReadOnlySpan<byte>)x, (ReadOnlySpan<byte>)y);

    public static bool EqualsBytes(this ReadOnlySpan<byte> x, Span<byte> y) => EqualsBytes(x, (ReadOnlySpan<byte>)y);

    public static bool EqualsBytes(this Span<byte> x, ReadOnlySpan<byte> y) => EqualsBytes((ReadOnlySpan<byte>)x, y);

    public static bool EqualsBytes(this ReadOnlySpan<byte> x, ReadOnlySpan<byte> y)
    {
        if (x.Length != y.Length) return false;
        if (x.Length == 0) return true;
        if (x[0] != y[0]) return false; // compare first byte
        if (x[^1] != y[^1]) return false; // compare last byte
        if (x[x.Length / 2] != y[y.Length / 2]) return false; // compare middle byte
        return x.SequenceEqual(y);
    }

    public static bool EqualsBytes(this byte[]? x, byte[]? y)
    {
        if (ReferenceEquals(x, y)) return true; //reference equality check
        if (ReferenceEquals(x, null) || ReferenceEquals(y, null)) return false;
        return EqualsBytes((ReadOnlySpan<byte>)x, (ReadOnlySpan<byte>)y);
    }
}
