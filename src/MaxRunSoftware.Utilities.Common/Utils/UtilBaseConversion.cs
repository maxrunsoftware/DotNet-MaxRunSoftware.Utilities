﻿// Copyright (c) 2024 Max Run Software (dev@maxrunsoftware.com)
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

namespace MaxRunSoftware.Utilities.Common;

public static partial class Util
{
    #region Base16

    private static readonly ImmutableArray<string> Base16_Strings = Enumerable.Range(0, 256).Select(i => i.ToString("X2")).ToImmutableArray();
    public static string Base16(byte b) => Base16_Strings[b];

    /*
    // ReSharper disable once InconsistentNaming
    private static readonly uint[] Base16_Chars = Enumerable.Range(0, 256).Select(o => o.ToString("X2")).Select(o => o[0] + ((uint)o[1] << 16)).ToArray();

    public static string Base16(byte[] bytes)
    {
        // https://stackoverflow.com/a/24343727/48700
        // https://stackoverflow.com/a/624379

        ReadOnlySpan<uint> ary = Base16_Chars;
        var len = bytes.Length;
        var result = new char[len * 2];
        for (var i = 0; i < len; i++)
        {
            var u = ary[bytes[i]];
            result[2 * i] = (char)u;
            result[2 * i + 1] = (char)(u >> 16);
        }

        return new(result);
    }
    */

    public static string Base16(byte[] bytes)
    {
        Span<char> output = new char[bytes.Length * 2];
        Base16(bytes, output);
        return new(output);
    }
    
    public static void Base16(ReadOnlySpan<byte> bytes, Span<char> output)
    {
        ReadOnlySpan<char> ary = stackalloc char[16] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F', };
        
        var len = bytes.Length;
        for (var i = len-1; i >= 0; --i)
        {
            var b = bytes[i];
            output[2 * i + 1] = ary[b % 16]; // assign to the greater array index first
            output[2 * i] = ary[b / 16];
        }
    }

    public static byte[] Base16(string base16String)
    {
        var numberChars = base16String.Length;
        var bytes = new byte[numberChars / 2];
        for (var i = 0; i < numberChars; i += 2) bytes[i / 2] = Convert.ToByte(base16String.Substring(i, 2), 16);

        return bytes;
    }

    #endregion Base16

    #region Base32
    
    
    /// <summary>
    /// https://stackoverflow.com/a/7135008
    /// https://www.rfc-editor.org/rfc/rfc4648#section-6
    /// </summary>
    public static byte[] Base32(string base32String)
    {
        if (string.IsNullOrEmpty(base32String)) throw new ArgumentNullException(nameof(base32String));

        base32String = base32String.TrimEnd('='); //remove padding characters
        var byteCount = base32String.Length * 5 / 8; //this must be TRUNCATED
        var returnArray = new byte[byteCount];

        byte curByte = 0;
        byte bitsRemaining = 8;
        var arrayIndex = 0;
        foreach (var c in base32String)
        {
            var cValue = CharToValue(c);
            
            if (bitsRemaining > 5)
            {
                var mask = cValue << (bitsRemaining - 5);
                curByte = (byte)(curByte | mask);
                bitsRemaining -= 5;
            }
            else
            {
                var mask = cValue >> (5 - bitsRemaining);
                curByte = (byte)(curByte | mask);
                returnArray[arrayIndex++] = curByte;
                curByte = (byte)(cValue << (3 + bitsRemaining));
                bitsRemaining += 3;
            }
        }

        // if we didn't end with a full byte
        if (arrayIndex != byteCount) returnArray[arrayIndex] = curByte;

        return returnArray;

        static int CharToValue(int value) => value switch
        {
            // 65-90 == uppercase letters
            < 91 and > 64 => value - 65,
            
            // 50-55 == numbers 2-7
            < 56 and > 49 => value - 24,
            
            // 97-122 == lowercase letters
            < 123 and > 96 => value - 97,
            
            _ => throw new ArgumentException("Character is not a Base32 character.", nameof(value)),
        };
    }

    /// <summary>
    /// https://stackoverflow.com/a/7135008
    /// https://www.rfc-editor.org/rfc/rfc4648#section-6
    /// </summary>
    public static string Base32(byte[] bytes)
    {
        if (bytes == null || bytes.Length == 0) throw new ArgumentNullException(nameof(bytes));

        var charCount = (int)Math.Ceiling(bytes.Length / 5d) * 8;
        var returnArray = new char[charCount];

        byte nextChar = 0;
        byte bitsRemaining = 5;
        var arrayIndex = 0;

        foreach (var b in bytes)
        {
            nextChar = (byte)(nextChar | (b >> (8 - bitsRemaining)));
            returnArray[arrayIndex++] = ValueToChar(nextChar);
            
            if (bitsRemaining < 4)
            {
                nextChar = (byte)((b >> (3 - bitsRemaining)) & 31);
                returnArray[arrayIndex++] = ValueToChar(nextChar);
                bitsRemaining += 5;
            }
            
            bitsRemaining -= 3;
            nextChar = (byte)((b << bitsRemaining) & 31);
        }

        // if we didn't end with a full char
        if (arrayIndex != charCount)
        {
            returnArray[arrayIndex++] = ValueToChar(nextChar);
            while (arrayIndex != charCount)
            {
                returnArray[arrayIndex++] = '='; //padding
            }
        }

        return new(returnArray);

        static char ValueToChar(byte b) => b switch
        {
            < 26 => (char)(b + 65),
            < 32 => (char)(b + 24),
            _ => throw new ArgumentException("Byte is not a value Base32 value.", nameof(b)),
        };
    }
    
    #endregion Base32
    
    #region Base64

    public static string Base64(byte[] bytes) => Convert.ToBase64String(bytes);

    public static byte[] Base64(string base64String) => Convert.FromBase64String(base64String);

    #endregion Base64
}
