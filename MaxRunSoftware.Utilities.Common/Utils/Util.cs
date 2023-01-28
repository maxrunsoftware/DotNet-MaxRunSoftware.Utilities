// Copyright (c) 2022 Max Run Software (dev@maxrunsoftware.com)
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

using System.Dynamic;
using System.Security.Cryptography;

namespace MaxRunSoftware.Utilities.Common;

public static partial class Util
{
    public static bool DynamicHasProperty(dynamic obj, string propertyName) =>
        obj is ExpandoObject
            ? ((IDictionary<string, object>)obj).ContainsKey(propertyName)
            : (bool)(obj.GetType().GetProperty(propertyName) != null);

    /// <summary>
    /// Gets a 001/100 format for a running count
    /// </summary>
    /// <param name="index">The zero based index, +1 will be added automatically</param>
    /// <param name="total">The total number of items</param>
    /// <returns>001/100 formatted string</returns>
    public static string FormatRunningCount(int index, int total) => (index + 1).ToStringPadded().Right(total.ToString().Length) + "/" + total;

    public static string FormatRunningCountPercent(int index, int total, int decimalPlaces)
    {
        var len = 3;
        if (decimalPlaces > 0) len += 1; // decimal

        len += decimalPlaces;

        decimal indexDecimal = index + 1;
        var totalDecimal = total;
        var multiplierDecimal = 100;

        return (indexDecimal / totalDecimal * multiplierDecimal).ToString(MidpointRounding.AwayFromZero, decimalPlaces).PadLeft(len) + " %";
    }

    public static string RandomString(int size, char[] characterPool)
    {
        // https://stackoverflow.com/a/1344255

        var data = new byte[4 * size];

        using (var crypto = RandomNumberGenerator.Create()) { crypto.GetBytes(data); }

        var result = new StringBuilder(size);
        for (var i = 0; i < size; i++)
        {
            var rnd = BitConverter.ToUInt32(data, i * 4);
            var idx = rnd % characterPool.Length;

            result.Append(characterPool[idx]);
        }

        return result.ToString();
    }

    /// <summary>
    /// Parses an encoding name string to an Encoding. Common values are...
    /// ASCII
    /// BigEndianUnicode
    /// Default
    /// Unicode
    /// UTF32
    /// UTF8
    /// Note: UTF8 is the standard UTF8 with the BOM
    /// </summary>
    /// <param name="encoding">The encoding name string</param>
    /// <returns>The Encoding or ArgumentException if not found</returns>
    public static Encoding ParseEncoding(string encoding)
    {
        if (encoding.Length > 0 && Constant.Encodings.TryGetValue(encoding, out var enc)) return enc;

        var charsLower = Constant.Encodings.Keys.SelectMany(o => o.ToCharArray()).Select(char.ToLower).ToHashSet();
        var encodingCleaned = new string(encoding
            .ToCharArray()
            .Select(char.ToLower)
            .Where(o => charsLower.Contains(o))
            .ToArray()
        );

        if (encoding.Length > 0 && Constant.Encodings.TryGetValue(encodingCleaned, out enc)) return enc;

        encodingCleaned = encodingCleaned.Trim();
        if (encoding.Length > 0 && Constant.Encodings.TryGetValue(encodingCleaned, out enc)) return enc;

        throw new ArgumentException($"Unknown encoding '{encoding}'", nameof(encoding));
    }

    private sealed class CreateDisposableClass : IDisposable
    {
        private readonly Action action;
        public CreateDisposableClass(Action action) => this.action = action;
        public void Dispose() => action();
    }
    public static IDisposable CreateDisposable(Action action) => new CreateDisposableClass(action);
}
