﻿// Copyright (c) 2022 Max Run Software (dev@maxrunsoftware.com)
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

using System.Globalization;
using System.Net;
using System.Security;

namespace MaxRunSoftware.Utilities.Common;

public static class ExtensionsToString
{
    #region ToStringPadded

    public static string ToStringPadded(this byte value) => value.ToString().PadLeft(byte.MaxValue.ToString().Length, '0');

    public static string ToStringPadded(this sbyte value) => value.ToString().PadLeft(sbyte.MaxValue.ToString().Length, '0');

    public static string ToStringPadded(this decimal value) => value.ToString(CultureInfo.InvariantCulture).PadLeft(decimal.MaxValue.ToString(CultureInfo.InvariantCulture).Length, '0');

    public static string ToStringPadded(this double value) => value.ToString(CultureInfo.InvariantCulture).PadLeft(double.MaxValue.ToString(CultureInfo.InvariantCulture).Length, '0');

    public static string ToStringPadded(this float value) => value.ToString(CultureInfo.InvariantCulture).PadLeft(float.MaxValue.ToString(CultureInfo.InvariantCulture).Length, '0');

    public static string ToStringPadded(this int value) => value.ToString().PadLeft(int.MaxValue.ToString().Length, '0');

    public static string ToStringPadded(this uint value) => value.ToString().PadLeft(uint.MaxValue.ToString().Length, '0');

    public static string ToStringPadded(this long value) => value.ToString().PadLeft(long.MaxValue.ToString().Length, '0');

    public static string ToStringPadded(this ulong value) => value.ToString().PadLeft(ulong.MaxValue.ToString().Length, '0');

    public static string ToStringPadded(this short value) => value.ToString().PadLeft(short.MaxValue.ToString().Length, '0');

    public static string ToStringPadded(this ushort value) => value.ToString().PadLeft(ushort.MaxValue.ToString().Length, '0');

    #endregion ToStringPadded

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
            _ => throw new NotImplementedException(nameof(DateTimeToStringFormat) + "." + format + " is not implemented")
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
    private static readonly TypeCacheWeak<ToStringGuessFormatGetLength> toStringGuessFormatCount = new ();
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
            } catch (Exception) {}
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

            var getLengthFunc = toStringGuessFormatCount.GetValue(t, tt => new(tt));
            if (getLengthFunc.HasProperty) return $"{name}[" + (getLengthFunc.GetValue(obj)?.ToString() ?? "?") + "]";
            //return $"{name}[?]";
        }

        return obj.ToString();
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
