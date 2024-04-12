// Copyright (c) 2024 Max Run Software (dev@maxrunsoftware.com)
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

    private sealed class EncodingParser
    {
        private readonly ImmutableHashSet<char> stripCharacters = Constant.Chars_Alphanumeric.ToImmutableHashSet();
        private string StripCharacters(string s) => new(s.ToCharArray().Where(o => stripCharacters.Contains(o)).ToArray());

        private readonly Dictionary<string, Encoding> cache = new(StringComparer.OrdinalIgnoreCase);
        public EncodingParser()
        {
            cache.AddRange(Encoding.ASCII, "ASCII");
            cache.AddRange(Encoding.BigEndianUnicode, "BigEndianUnicode");
            cache.AddRange(Encoding.Default, "Default");
            cache.AddRange(Encoding.Unicode, "Unicode");
            cache.AddRange(Encoding.UTF32, "UTF32");
            cache.AddRange(Encoding.Latin1, "Latin1");
            cache.AddRange(Constant.Encoding_UTF8_With_BOM, "UTF8", "UTF8_BOM", "UTF8_BOM_YES", "UTF8_With_BOM");
            cache.AddRange(Constant.Encoding_UTF8_Without_BOM, "UTF8_BOM_NO", "UTF8_NoBOM", "UTF8_Without_BOM");
            foreach (var kvp in cache.ToArray())
            {
                var keyStripped = StripCharacters(kvp.Key);
                if (keyStripped.Length != kvp.Key.Length) cache.Add(keyStripped, kvp.Value);
            }
        }
        public Encoding? ParseEncodingString(string name)
        {
            if (name.Length == 0) return null;
            if (cache.TryGetValue(name, out var v)) return v;
            name = name.Trim();
            if (name.Length == 0) return null;
            if (cache.TryGetValue(name, out v)) return Add(name, v);

            v = null;
            try
            {
                v = Encoding.GetEncoding(name);
            }
            catch (Exception) { }

            if (v != null) return Add(name, v);

            var nameStripped = StripCharacters(name);
            if (nameStripped != name) return ParseEncodingString(nameStripped);
            return null;
        }

        private Encoding Add(string encodingString, Encoding encoding)
        {
            cache[encodingString] = encoding;
            return encoding;
        }
    }

    private static readonly EncodingParser encodingParser = new();
    private static readonly object encodingParserLock = new();


    /// <summary>
    /// Parses an encoding name string to an Encoding. Common values are...
    /// ASCII
    /// BigEndianUnicode
    /// Default
    /// Unicode
    /// UTF32
    /// UTF8 / UTF8_BOM / UTF8_BOM_YES / UTF8_With_BOM
    /// UTF8_BOM_NO / UTF8_NoBOM / UTF8_Without_BOM
    /// Note: UTF8 is the standard UTF8 with the BOM
    /// </summary>
    /// <param name="encoding">The encoding name string</param>
    /// <returns>The Encoding or ArgumentException if not found</returns>
    public static Encoding ParseEncoding(string encoding)
    {
        lock (encodingParserLock)
        {
            var enc = encodingParser.ParseEncodingString(encoding);
            if (enc == null) throw new ArgumentException($"Encoding {encoding} not found", nameof(encoding));
            return enc;
        }
    }

    #region CreateDisposable
    
    private sealed class CreateDisposable_Action(Action action) : IDisposable
    {
        public void Dispose() => action();
    }
    
    private sealed class CreateDisposable_Noop() : IDisposable
    {
        public void Dispose() {}
    }
    
    private static readonly CreateDisposable_Noop CreateDisposable_Noop_Instance = new CreateDisposable_Noop();
    
    public static IDisposable CreateDisposable(Action? action = null) =>
        action == null
            ? CreateDisposable_Noop_Instance
            : new CreateDisposable_Action(action);
    
    #endregion CreateDisposable
}
