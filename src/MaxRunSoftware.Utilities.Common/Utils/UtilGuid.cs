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

using System.Security.Cryptography;

namespace MaxRunSoftware.Utilities.Common;

public static partial class Util
{
    // https://stackoverflow.com/a/49372627

    public static Guid GuidFromLongs(long a, long b)
    {
        var guidData = new byte[16];
        Array.Copy(BitConverter.GetBytes(a), guidData, 8);
        Array.Copy(BitConverter.GetBytes(b), 0, guidData, 8, 8);
        return new(guidData);
    }

    public static (long, long) ToLongs(this Guid guid)
    {
        var bytes = guid.ToByteArray();
        var long1 = BitConverter.ToInt64(bytes, 0);
        var long2 = BitConverter.ToInt64(bytes, 8);
        return (long1, long2);
    }

    public static Guid GuidFromULongs(ulong a, ulong b)
    {
        var guidData = new byte[16];
        Array.Copy(BitConverter.GetBytes(a), guidData, 8);
        Array.Copy(BitConverter.GetBytes(b), 0, guidData, 8, 8);
        return new(guidData);
    }

    public static (ulong, ulong) ToULongs(this Guid guid)
    {
        var bytes = guid.ToByteArray();
        var ulong1 = BitConverter.ToUInt64(bytes, 0);
        var ulong2 = BitConverter.ToUInt64(bytes, 8);
        return (ulong1, ulong2);
    }

    public static Guid GuidFromInts(int a, int b, int c, int d)
    {
        var guidData = new byte[16];
        Array.Copy(BitConverter.GetBytes(a), guidData, 4);
        Array.Copy(BitConverter.GetBytes(b), 0, guidData, 4, 4);
        Array.Copy(BitConverter.GetBytes(c), 0, guidData, 8, 4);
        Array.Copy(BitConverter.GetBytes(d), 0, guidData, 12, 4);
        return new(guidData);
    }

    public static (int, int, int, int) ToInts(this Guid guid)
    {
        var bytes = guid.ToByteArray();
        var a = BitConverter.ToInt32(bytes, 0);
        var b = BitConverter.ToInt32(bytes, 4);
        var c = BitConverter.ToInt32(bytes, 8);
        var d = BitConverter.ToInt32(bytes, 12);
        return (a, b, c, d);
    }

    public static Guid GuidFromUInts(uint a, uint b, uint c, uint d)
    {
        var guidData = new byte[16];
        Array.Copy(BitConverter.GetBytes(a), guidData, 4);
        Array.Copy(BitConverter.GetBytes(b), 0, guidData, 4, 4);
        Array.Copy(BitConverter.GetBytes(c), 0, guidData, 8, 4);
        Array.Copy(BitConverter.GetBytes(d), 0, guidData, 12, 4);
        return new(guidData);
    }

    public static (uint, uint, uint, uint) ToUInts(this Guid guid)
    {
        var bytes = guid.ToByteArray();
        var a = BitConverter.ToUInt32(bytes, 0);
        var b = BitConverter.ToUInt32(bytes, 4);
        var c = BitConverter.ToUInt32(bytes, 8);
        var d = BitConverter.ToUInt32(bytes, 12);
        return (a, b, c, d);
    }

    /// Generates a cryptographically secure random Guid.
    ///
    /// Characteristics
    ///     - GUID Variant: RFC 4122
    ///     - GUID Version: 4
    ///     - .NET 5
    /// RFC
    ///     https://tools.ietf.org/html/rfc4122#section-4.1.3
    /// Stackoverflow
    ///     https://stackoverflow.com/a/65514843
    ///     https://stackoverflow.com/a/59437504/10830091
    public static Guid GuidNewSecure()
    {
        // Byte indices
        var versionByteIndex = BitConverter.IsLittleEndian ? 7 : 6;
        const int variantByteIndex = 8;

        // Version mask & shift for `Version 4`
        const int versionMask = 0x0F;
        const int versionShift = 0x40;

        // Variant mask & shift for `RFC 4122`
        const int variantMask = 0x3F;
        const int variantShift = 0x80;

        // Get bytes of cryptographically-strong random values
        var bytes = new byte[16];

        RandomNumberGenerator.Fill(bytes);

        // Set version bits -- 6th or 7th byte according to Endianness, big or little Endian respectively
        bytes[versionByteIndex] = (byte)((bytes[versionByteIndex] & versionMask) | versionShift);

        // Set variant bits -- 8th byte
        bytes[variantByteIndex] = (byte)((bytes[variantByteIndex] & variantMask) | variantShift);

        // Initialize Guid from the modified random bytes
        return new(bytes);
    }
}
