// Copyright (c) 2024 Max Run Software (dev@maxrunsoftware.com)
// 
// Licensed under the Apache License, Version 2.0 (the "License")
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

    #region IsEqual
    
    public static bool IsEqual(this Span<byte> x, Span<byte> y) => IsEqual((ReadOnlySpan<byte>)x, (ReadOnlySpan<byte>)y);

    public static bool IsEqual(this ReadOnlySpan<byte> x, Span<byte> y) => IsEqual(x, (ReadOnlySpan<byte>)y);

    public static bool IsEqual(this Span<byte> x, ReadOnlySpan<byte> y) => IsEqual((ReadOnlySpan<byte>)x, y);

    public static bool IsEqual(this ReadOnlySpan<byte> x, ReadOnlySpan<byte> y)
    {
        var len = x.Length;
        if (len != y.Length) return false;
        switch (len)
        {
            case 0: return true;
            case 1: return x[0] == y[0];
            case 2: return x[0] == y[0] && x[1] == y[1];
            case 3: return x[0] == y[0] && x[1] == y[1] && x[2] == y[2];
            case 4: return x[0] == y[0] && x[1] == y[1] && x[2] == y[2] && x[3] == y[3];
            case 5: return x[0] == y[0] && x[1] == y[1] && x[2] == y[2] && x[3] == y[3] && x[4] == y[4];
            case 6: return x[0] == y[0] && x[1] == y[1] && x[2] == y[2] && x[3] == y[3] && x[4] == y[4] && x[5] == y[5];
            case 7: return x[0] == y[0] && x[1] == y[1] && x[2] == y[2] && x[3] == y[3] && x[4] == y[4] && x[5] == y[5] && x[6] == y[6];
            case 8: return x[0] == y[0] && x[1] == y[1] && x[2] == y[2] && x[3] == y[3] && x[4] == y[4] && x[5] == y[5] && x[6] == y[6] && x[7] == y[7];
        }

        if (x[0] != y[0]) return false; // compare first byte
        
        if (x[^1] != y[^1]) return false; // compare last byte
        
        var index50 = len / 2;
        if (x[index50] != y[index50]) return false; // compare middle byte

        var index25 = index50 / 2;
        if (x[index25] != y[index25]) return false; // compare 25%
        
        var index75 = Math.Min(index50 + index25, len);
        if (x[index75] != y[index75]) return false; // compare 75%
        
        return x.SequenceEqual(y);
    }

    public static bool IsEqual(this byte[]? x, byte[]? y)
    {
        if (ReferenceEquals(x, y)) return true; //reference equality check
        if (ReferenceEquals(x, null) || ReferenceEquals(y, null)) return false;
        return IsEqual((ReadOnlySpan<byte>)x, (ReadOnlySpan<byte>)y);
    }
    
    #endregion IsEqual

    #region Compare

    public static int Compare(this Span<byte> x, Span<byte> y) => Compare((ReadOnlySpan<byte>)x, (ReadOnlySpan<byte>)y);

    public static int Compare(this ReadOnlySpan<byte> x, Span<byte> y) => Compare(x, (ReadOnlySpan<byte>)y);

    public static int Compare(this Span<byte> x, ReadOnlySpan<byte> y) => Compare((ReadOnlySpan<byte>)x, y);

    public static int Compare(this ReadOnlySpan<byte> x, ReadOnlySpan<byte> y)
    {
        var len = Math.Min(x.Length, y.Length);
        for (var i = 0; i < len; i++)
        {
            int c;
            if ((c = x[i].CompareTo(y[i])) != 0) return c;
        }
        return x.Length.CompareTo(y.Length);
    }

    public static int Compare(this byte[]? x, byte[]? y)
    {
        if (ReferenceEquals(x, y)) return 0; //reference equality check
        if (ReferenceEquals(x, null)) return -1;
        if (ReferenceEquals(y, null)) return 1;

        return Compare((ReadOnlySpan<byte>)x, (ReadOnlySpan<byte>)y);
    }
    
    #endregion Compare

}
