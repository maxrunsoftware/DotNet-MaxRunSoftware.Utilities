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

using System.Collections.Concurrent;
using System.Data.Common;
using System.Globalization;
using System.Net;
using System.Numerics;
using System.Security;

namespace MaxRunSoftware.Utilities.Common;

public static class ExtensionsToString
{
    public static int ToStringMaxStringLength<T>(this T value) where T : IMinMaxValue<T> =>
        Math.Max(
            Math.Max(
                (T.MinValue.ToString() ?? string.Empty).Length,
                (T.MaxValue.ToString() ?? string.Empty).Length
            ),
            (value.ToString() ?? string.Empty).Length
        );
    
    #region ToStringPadded
    
    public static string ToStringPadded<T>(this T value, char paddingChar = '0', int? maxPaddingAmount = null) where T : INumber<T>, IMinMaxValue<T>
    {
        var padding = Math.Max(value.ToStringMaxStringLength(), 1);
        padding = Math.Min(padding, maxPaddingAmount ?? int.MaxValue);
        var s = value.ToString() ?? "?";
        return s.PadLeft(padding, paddingChar);
    }
    
    #endregion ToStringPadded
    
    #region ToStringRunningCount
    
    /// <summary>
    /// Gets a 001/100 format for a running count
    /// </summary>
    /// <param name="index">The zero based index, +1 will be added automatically</param>
    /// <param name="total">The total number of items</param>
    /// <param name="delimiter">The delimiter to use to separate index from total</param>
    /// <param name="paddingChar">The character to prefix the index with</param>
    /// <returns>001/100 formatted string</returns>
    public static string ToStringRunningCount<T>(this T index, T total, string delimiter = "/", char paddingChar = '0') where T : IBinaryNumber<T>, IAdditionOperators<T, T, T>, IMinMaxValue<T>
    {
        var i = index + T.One;
        var indexString = i.ToString() ?? string.Empty;
        var totalString = total.ToString() ?? string.Empty;
        
        var maxPaddingAmount = Math.Max(indexString.Length, totalString.Length);
        
        return i.ToStringPadded(
            paddingChar: paddingChar,
            maxPaddingAmount: maxPaddingAmount
        ) + delimiter + totalString;
    }
    
    #endregion ToStringRunningCount

    #region ToStringCommas

    public static string ToStringCommas(this int value) => $"{value:n0}";

    public static string ToStringCommas(this uint value) => $"{value:n0}";

    public static string ToStringCommas(this long value) => $"{value:n0}";

    public static string ToStringCommas(this ulong value) => $"{value:n0}";

    public static string ToStringCommas(this short value) => $"{value:n0}";

    public static string ToStringCommas(this ushort value) => $"{value:n0}";

    #endregion ToStringCommas

    #region ToStringRoundAwayFromZero

    public static string ToString(this double value, MidpointRounding rounding, int decimalPlaces) => value.Round(rounding, decimalPlaces).ToString("N" + decimalPlaces).Replace(",", "");

    public static string ToString(this float value, MidpointRounding rounding, int decimalPlaces) => value.Round(rounding, decimalPlaces).ToString("N" + decimalPlaces).Replace(",", "");

    public static string ToString(this decimal value, MidpointRounding rounding, int decimalPlaces) => value.Round(rounding, decimalPlaces).ToString("N" + decimalPlaces).Replace(",", "");

    #endregion ToStringRoundAwayFromZero
    
    public static string ToStringItems(this IEnumerable enumerable) => "[" + string.Join(", ", enumerable.Cast<object?>().Select(ToStringGuessFormat)) + "]";

    public static string ToString(this DateTime dateTime, DateTimeToStringFormat format) =>
        format switch
        {
            DateTimeToStringFormat.ISO_8601 => dateTime.ToString("o", CultureInfo.InvariantCulture),
            DateTimeToStringFormat.YYYY_MM_DD => dateTime.ToString("yyyy-MM-dd"),
            DateTimeToStringFormat.YYYY_MM_DD_HH_MM_SS => dateTime.ToString("yyyy-MM-dd HH:mm:ss"),
            DateTimeToStringFormat.HH_MM_SS => dateTime.ToString("HH:mm:ss"),
            DateTimeToStringFormat.HH_MM_SS_FFF => dateTime.ToString("HH:mm:ss.fff"),
            _ => throw new NotImplementedException(nameof(DateTimeToStringFormat) + "." + format + " is not implemented"),
        };

    private sealed class ToStringGuessFormatGetLength
    {
        private readonly SlimObj? slimObj;

        // ReSharper disable once NotAccessedPositionalProperty.Local
        private record SlimObj(object Slim, string Name, Type Type, Func<object?, object?> GetValue);

        public ToStringGuessFormatGetLength(Type type)
        {
            var slimProperties = type.GetPropertySlims(BindingFlags.Public | BindingFlags.Instance)
                .Where(o => o.IsGettablePublic)
                .Where(o => Constant.Types_Numeric.Contains(o.Type))
                .Select(o => new SlimObj(o, o.Name, o.Type, o.GetValue))
                .ToArray();

            var slimFields = type.GetFieldSlims(BindingFlags.Public | BindingFlags.Instance)
                .Where(o => Constant.Types_Numeric.Contains(o.Type))
                .Select(o => new SlimObj(o, o.Name, o.Type, o.GetValue))
                .ToArray();

            const string nameCount = nameof(ICollection.Count);
            const string nameLength = nameof(Array.Length);

            slimObj =
                null
                ?? slimProperties.FirstOrDefault(o => StringComparer.Ordinal.Equals(nameCount, o.Name) && o.Type == typeof(int))
                ?? slimProperties.FirstOrDefault(o => StringComparer.Ordinal.Equals(nameCount, o.Name) && o.Type == typeof(long))
                ?? slimFields.FirstOrDefault(o => StringComparer.Ordinal.Equals(nameLength, o.Name) && o.Type == typeof(int))
                ?? slimFields.FirstOrDefault(o => StringComparer.Ordinal.Equals(nameLength, o.Name) && o.Type == typeof(long))
                ?? slimProperties.FirstOrDefault(o => StringComparer.Ordinal.Equals(nameCount, o.Name) && o.Type == typeof(int?))
                ?? slimProperties.FirstOrDefault(o => StringComparer.Ordinal.Equals(nameCount, o.Name) && o.Type == typeof(long?))
                ?? slimFields.FirstOrDefault(o => StringComparer.Ordinal.Equals(nameLength, o.Name) && o.Type == typeof(int?))
                ?? slimFields.FirstOrDefault(o => StringComparer.Ordinal.Equals(nameLength, o.Name) && o.Type == typeof(long?))
                ?? null
                ;
        }

        public long? GetValue(object instance) => slimObj?.GetValue(instance) as long?;
        public bool HasProperty => slimObj != null;
    }

    // ReSharper disable once InconsistentNaming
    private static readonly DictionaryWeakType<ToStringGuessFormatGetLength> toStringGuessFormatCount = new();
    public static string? ToStringGuessFormat(this object? obj) => ToStringGuessFormat(obj, false);

    public static string? ToStringGuessFormat(this object? obj, bool showEnumerableValues)
    {
        if (obj == null) return null;
        if (obj == DBNull.Value) return null;
        if (obj is string objString) return objString;
        if (obj is DateTime objDateTime) return objDateTime.ToString(DateTimeToStringFormat.ISO_8601);
        //if (obj is byte[] objBytes) return "0x" + Util.Base16(objBytes);
        //if (obj is MemoryStream objMemoryStream) return obj.GetType().FullNameFormatted() + $"[{objMemoryStream.Length}]";
        if (obj is Stream objStream)
        {
            try
            {
                return obj.GetType().FullNameFormatted() + $"[{objStream.Length}]";
            }
            catch (Exception) { }
        }

        if (obj is Type objType) return objType.FullNameFormatted();

        var t = obj.GetType();
        if (t.IsNullable(out var underlyingType)) t = underlyingType;

        if (t == typeof(DateTime?)) return ((DateTime?)obj).Value.ToString(DateTimeToStringFormat.ISO_8601);

        if (obj is IEnumerable enumerable)
        {
            var name = (t.IsArray ? t.GetElementType() ?? t : t).FullNameFormatted();
            if (showEnumerableValues)
            {
                return name + enumerable.ToStringItems();
            }

            var getLengthFunc = toStringGuessFormatCount.GetOrAdd(t, tt => new(tt));
            if (getLengthFunc.HasProperty) return $"{name}[" + (getLengthFunc.GetValue(obj)?.ToString() ?? "?") + "]";
            //return $"{name}[?]";
        }

        if (obj is IDbConnection dbConnection) return ToStringGuessFormat(dbConnection);

        return obj.ToString();
    }

    private static string ToStringGuessFormat(IDbConnection connection)
    {
        var str = connection.ToString();
        if (!string.IsNullOrWhiteSpace(str) && str != connection.GetType().FullName) return str;

        var items = new List<(string, string?)>();

        string? state = null;
        try
        {
            state = connection.State.ToString();
        }
        catch (Exception) { }

        items.Add((nameof(connection.State), state));


        string? connectionString = null;
        try
        {
            connectionString = connection.ConnectionString.TrimOrNull();
        }
        catch (Exception) { }

        string? connectionStringModified = null;
        if (connectionString != null)
        {
            try
            {
                var csb = new DbConnectionStringBuilder { ConnectionString = connectionString };
                var keysToRemove = new HashSet<string>();

                foreach (var keyObj in csb.Keys)
                {
                    var keyString = keyObj?.ToString();
                    if (keyString == null) continue;
                    var keyStringTrimmed = keyString.TrimOrNull();
                    if (keyStringTrimmed == null) continue;
                    if (keyStringTrimmed.ContainsAny(StringComparison.OrdinalIgnoreCase, "Password", "PWD")) keysToRemove.Add(keyString);
                }

                foreach (var keyToRemove in keysToRemove) csb[keyToRemove] = "*****";
                connectionStringModified = csb.ConnectionString.TrimOrNull();
            }
            catch (Exception) { }
        }

        var connectionStringFinal = connectionStringModified ?? connectionString;
        if (connectionStringFinal != null) connectionStringFinal = "\"" + connectionStringFinal + "\"";
        items.Add((nameof(connection.ConnectionString), connectionStringFinal));

        var sb = new StringBuilder();
        sb.Append(connection.GetType().FullNameFormatted());
        sb.Append('(');
        sb.Append(items.Select(o => o.Item1 + ": " + (o.Item2 ?? "?")).ToStringDelimited(", "));
        sb.Append(')');

        return sb.ToString();
    }

    public static IEnumerable<string?> ToStringsGuessFormat(this IEnumerable<object> enumerable)
    {
        foreach (var obj in enumerable.OrEmpty()) yield return obj.ToStringGuessFormat();
    }

    /*
    public static string ToStringGenerated(this object obj, BindingFlags flags = BindingFlags.Instance | BindingFlags.Public, string nullValue = "")
    {
        obj.CheckNotNull(nameof(obj));

        var t = obj.GetType();
        var list = new List<string>();

        // TODO: Use IReflectionProperty for faster reads

        foreach (var prop in t.GetProperties(flags))
        {
            if (!prop.CanRead) continue;
            var name = prop.Name;
            var val = prop.GetValue(obj).ToStringGuessFormat() ?? nullValue;
            list.Add(name + "=" + val);
        }

        var sb = new StringBuilder();
        sb.Append(t.NameFormatted());
        sb.Append('(');
        sb.Append(list.ToStringDelimited(", "));
        sb.Append(')');

        return sb.ToString();
    }
    */

    public static string ToStringDelimited<T>(this IEnumerable<T> enumerable, string delimiter) => string.Join(delimiter, enumerable);

    public static string ToStringDelimited(this IEnumerable<object> enumerable, string delimiter) => enumerable.Select(o => o.ToStringGuessFormat()).ToStringDelimited(delimiter);

    public static string ToStringDelimited<T>(this IEnumerable<T> enumerable, char delimiter) => enumerable.ToStringDelimited(delimiter.ToString());

    public static string ToStringDelimited(this IEnumerable<object> enumerable, char delimiter) => enumerable.ToStringDelimited(delimiter.ToString());

    public static string ToStringInsecure(this SecureString secureString) => new NetworkCredential("", secureString).Password;

    public static string ToStringTotalSeconds(this TimeSpan timeSpan, int numberOfDecimalDigits = 0) => timeSpan.TotalSeconds.ToString(MidpointRounding.AwayFromZero, Math.Max(0, numberOfDecimalDigits));

    private static readonly string[] TO_STRING_BASE16_CACHE = Enumerable.Range(0, 256).Select(o => BitConverter.ToString(new[] { (byte)o })).ToArray();

    public static string ToStringBase16(this byte b) => TO_STRING_BASE16_CACHE[b];

    private static readonly string[] TO_STRING_BASE64_CACHE = Enumerable.Range(0, 256).Select(o => Convert.ToBase64String(new[] { (byte)o }).Substring(0, 2)).ToArray();

    public static string ToStringBase64(this byte b) => TO_STRING_BASE64_CACHE[b];
}

public enum DateTimeToStringFormat
{
    // ReSharper disable InconsistentNaming
    ISO_8601,
    YYYY_MM_DD,
    YYYY_MM_DD_HH_MM_SS,
    HH_MM_SS,
    HH_MM_SS_FFF,
    // ReSharper restore InconsistentNaming
}
