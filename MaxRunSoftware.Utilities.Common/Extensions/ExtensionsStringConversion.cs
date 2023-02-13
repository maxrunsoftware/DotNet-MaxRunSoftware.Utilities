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

using System.Drawing;
using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Numerics;
using System.Security;

namespace MaxRunSoftware.Utilities.Common;

// ReSharper disable InconsistentNaming
public static class ExtensionsStringConversion
{
    public static ImmutableDictionary<Type, MethodSlim> Converters => converters.Value;
    private static readonly Lzy<ImmutableDictionary<Type, MethodSlim>> converters;
    private static ImmutableDictionary<Type, MethodSlim> Converters_Build()
    {
        var b = ImmutableDictionary.CreateBuilder<Type, MethodSlim>();
        foreach (var m in ((TypeSlim)typeof(ExtensionsStringConversion)).GetMethodSlims(BindingFlags.Public | BindingFlags.Static))
        {
            if (!m.Name.StartsWith("To")) continue;
            if (m.ReturnType == null) continue;
            if (m.Name.EndsWithAny(StringComparison.OrdinalIgnoreCase, "Try", "Nullable", "OrNull")) continue;

            b.Add(m.ReturnType.Type, m);
        }
        return b.ToImmutable();
    }

    public static ImmutableDictionary<Type, MethodSlim> ConvertersNullable => convertersNullable.Value;
    private static readonly Lzy<ImmutableDictionary<Type, MethodSlim>> convertersNullable;
    private static ImmutableDictionary<Type, MethodSlim> ConvertersNullable_Build()
    {
        var b = ImmutableDictionary.CreateBuilder<Type, MethodSlim>();
        foreach (var m in ((TypeSlim)typeof(ExtensionsStringConversion)).GetMethodSlims(BindingFlags.Public | BindingFlags.Static))
        {
            if (m.Name.EndsWith("Try")) continue;
            if (!m.Name.StartsWith("To")) continue;
            if (m.ReturnType == null) continue;
            if (!m.Name.EndsWith("Nullable")) continue;
            b.Add(m.ReturnType.Type, m);
        }
        return b.ToImmutable();
    }

    static ExtensionsStringConversion()
    {
        converters = Lzy.Create(Converters_Build);
        convertersNullable = Lzy.Create(ConvertersNullable_Build);
    }

    private delegate bool Try<T>(string? str, out T output) where T : struct;

    private static bool ToNullableTryStruct<T>(string? str, out T? output, Try<T> func) where T : struct
    {
        str = str.TrimOrNull();
        if (str == null)
        {
            output = default;
            return true;
        }

        var r = func(str, out var o);
        output = r ? o : default;
        return r;
    }

    private static T? ToOrNull<T>(string? str, Try<T> func) where T : struct => func(str, out var o) ? o : default(T?);

    #region Primitive

    #region bool

    public static bool ToBool(this string str)
    {
        str = str.Trim();
        return str.ToBoolTry(out var output) ? output : bool.Parse(str);
    }

    public static bool ToBoolTry(this string? str, out bool output)
    {
        str = str.TrimOrNull();
        if (str != null) return Constant.String_Bool.TryGetValue(str, out output);
        output = default;
        return false;

    }

    public static bool? ToBoolNullable(this string? str) => str.TrimOrNull()?.ToBool();

    public static bool ToBoolNullableTry(this string? str, out bool? output) => ToNullableTryStruct(str, out output, ToBoolTry);

    public static bool? ToBoolOrNull(this string? str) => ToOrNull<bool>(str, ToBoolTry);

    #endregion bool

    #region byte

    public static byte ToByte(this string str) => byte.Parse(str.Trim());

    public static bool ToByteTry(this string? str, out byte output) => byte.TryParse(str.TrimOrNull(), out output);

    public static byte? ToByteNullable(this string? str) => str.TrimOrNull()?.ToByte();

    public static bool ToByteNullableTry(this string? str, out byte? output) => ToNullableTryStruct(str, out output, ToByteTry);

    public static byte? ToByteOrNull(this string? str) => ToOrNull<byte>(str, ToByteTry);

    #endregion byte

    #region sbyte

    public static sbyte ToSByte(this string str) => sbyte.Parse(str.Trim());

    public static bool ToSByteTry(this string? str, out sbyte output) => sbyte.TryParse(str.TrimOrNull(), out output);

    public static sbyte? ToSByteNullable(this string? str) => str.TrimOrNull()?.ToSByte();

    public static bool ToSByteNullableTry(this string? str, out sbyte? output) => ToNullableTryStruct(str, out output, ToSByteTry);

    public static sbyte? ToSByteOrNull(this string? str) => ToOrNull<sbyte>(str, ToSByteTry);

    #endregion sbyte

    #region char

    public static char ToChar(this string str) => char.Parse(str.Trim());

    public static bool ToCharTry(this string? str, out char output) => char.TryParse(str.TrimOrNull(), out output);

    public static char? ToCharNullable(this string? str) => str.TrimOrNull()?.ToChar();

    public static bool ToCharNullableTry(this string? str, out char? output) => ToNullableTryStruct(str, out output, ToCharTry);

    public static char? ToCharOrNull(this string? str) => ToOrNull<char>(str, ToCharTry);

    #endregion char

    #region short

    public static short ToShort(this string str) => short.Parse(str.Trim());

    public static bool ToShortTry(this string? str, out short output) => short.TryParse(str.TrimOrNull(), out output);

    public static short? ToShortNullable(this string? str) => str.TrimOrNull()?.ToShort();

    public static bool ToShortNullableTry(this string? str, out short? output) => ToNullableTryStruct(str, out output, ToShortTry);

    public static short? ToShortOrNull(this string? str) => ToOrNull<short>(str, ToShortTry);

    #endregion short

    #region ushort

    public static ushort ToUShort(this string str) => ushort.Parse(str.Trim());

    public static bool ToUShortTry(this string? str, out ushort output) => ushort.TryParse(str.TrimOrNull(), out output);

    public static ushort? ToUShortNullable(this string? str) => str.TrimOrNull()?.ToUShort();

    public static bool ToUShortNullableTry(this string? str, out ushort? output) => ToNullableTryStruct(str, out output, ToUShortTry);

    public static ushort? ToUShortOrNull(this string? str) => ToOrNull<ushort>(str, ToUShortTry);

    #endregion ushort

    #region int

    public static int ToInt(this string str) => int.Parse(str.Trim());

    public static bool ToIntTry(this string? str, out int output) => int.TryParse(str.TrimOrNull(), out output);

    public static int? ToIntNullable(this string? str) => str.TrimOrNull()?.ToInt();

    public static bool ToIntNullableTry(this string? str, out int? output) => ToNullableTryStruct(str, out output, ToIntTry);

    public static int? ToIntOrNull(this string? str) => ToOrNull<int>(str, ToIntTry);

    #endregion int

    #region uint

    public static uint ToUInt(this string str) => uint.Parse(str.Trim());

    public static bool ToUIntTry(this string? str, out uint output) => uint.TryParse(str.TrimOrNull(), out output);

    public static uint? ToUIntNullable(this string? str) => str.TrimOrNull()?.ToUInt();

    public static bool ToUIntNullableTry(this string? str, out uint? output) => ToNullableTryStruct(str, out output, ToUIntTry);

    public static uint? ToUIntOrNull(this string? str) => ToOrNull<uint>(str, ToUIntTry);

    #endregion uint

    #region long

    public static long ToLong(this string str) => long.Parse(str.Trim());

    public static bool ToLongTry(this string? str, out long output) => long.TryParse(str.TrimOrNull(), out output);

    public static long? ToLongNullable(this string? str) => str.TrimOrNull()?.ToLong();

    public static bool ToLongNullableTry(this string? str, out long? output) => ToNullableTryStruct(str, out output, ToLongTry);

    public static long? ToLongOrNull(this string? str) => ToOrNull<long>(str, ToLongTry);

    #endregion long

    #region ulong

    public static ulong ToULong(this string str) => ulong.Parse(str.Trim());

    public static bool ToULongTry(this string? str, out ulong output) => ulong.TryParse(str.TrimOrNull(), out output);

    public static ulong? ToULongNullable(this string? str) => str.TrimOrNull()?.ToULong();

    public static bool ToULongNullableTry(this string? str, out ulong? output) => ToNullableTryStruct(str, out output, ToULongTry);

    public static ulong? ToULongOrNull(this string? str) => ToOrNull<ulong>(str, ToULongTry);

    #endregion ulong

    #region float

    public static float ToFloat(this string str) => float.Parse(str.Trim());

    public static bool ToFloatTry(this string? str, out float output) => float.TryParse(str.TrimOrNull(), out output);

    public static float? ToFloatNullable(this string? str) => str.TrimOrNull()?.ToFloat();

    public static bool ToFloatNullableTry(this string? str, out float? output) => ToNullableTryStruct(str, out output, ToFloatTry);

    public static float? ToFloatOrNull(this string? str) => ToOrNull<float>(str, ToFloatTry);

    #endregion float

    #region double

    public static double ToDouble(this string str) => double.Parse(str.Trim());

    public static bool ToDoubleTry(this string? str, out double output) => double.TryParse(str.TrimOrNull(), out output);

    public static double? ToDoubleNullable(this string? str) => str.TrimOrNull()?.ToDouble();

    public static bool ToDoubleNullableTry(this string? str, out double? output) => ToNullableTryStruct(str, out output, ToDoubleTry);

    public static double? ToDoubleOrNull(this string? str) => ToOrNull<double>(str, ToDoubleTry);

    #endregion double

    #region decimal

    public static decimal ToDecimal(this string str) => decimal.Parse(str.Trim());

    public static bool ToDecimalTry(this string? str, out decimal output) => decimal.TryParse(str.TrimOrNull(), out output);

    public static decimal? ToDecimalNullable(this string? str) => str.TrimOrNull()?.ToDecimal();

    public static bool ToDecimalNullableTry(this string? str, out decimal? output) => ToNullableTryStruct(str, out output, ToDecimalTry);

    public static decimal? ToDecimalOrNull(this string? str) => ToOrNull<decimal>(str, ToDecimalTry);

    #endregion decimal

    #endregion Primitive

    #region DateTime

    #region DateTime

    public static DateTime ToDateTime(this string str)
    {
        str = str.Trim();
        if (str.ToDateTimeTry(out var o)) return o;
        return DateTime.Parse(str);
    }

    public static bool ToDateTimeTry(this string? str, out DateTime output)
    {
        str = str.TrimOrNull();
        var r = DateTime.TryParse(str, out output);
        if (r) return r;
        if (str == null) return r;

        // char.IsDigit allows other weird things https://stackoverflow.com/a/228565
        static bool IsDigit(char c) => c is '0' or '1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9';

        // Only allow digits
        if (str.All(IsDigit))
        {
            // ReSharper disable once CommentTypo
            //const string formatFixed = "yyyyMMddHHmmss"; // yyyy-MM-dd HH:mm:ss.ffffff
            var format = str.Length switch
            {
                8 => "yyyyMMdd",
                10 => "yyyyMMddHH",
                12 => "yyyyMMddHHmm",
                14 => "yyyyMMddHHmmss",
                > 14 => "yyyyMMddHHmmss".PadRight(str.Length, 'f'),
                _ => null,
            };

            if (format != null) r = DateTime.TryParseExact(str, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out output);
        }

        if (r) return r;

        var strStripped = new string(str.ToCharArray().Where(IsDigit).ToArray());
        if (strStripped.Length > 0 && strStripped.Length != str.Length)
        {
            r = strStripped.ToDateTimeTry(out output);
        }

        if (r) return r;

        // TODO: Any other attempts at processing

        return r;
    }

    public static DateTime? ToDateTimeNullable(this string? str) => str.TrimOrNull()?.ToDateTime();

    public static bool ToDateTimeNullableTry(this string? str, out DateTime? output) => ToNullableTryStruct(str, out output, ToDateTimeTry);

    public static DateTime? ToDateTimeOrNull(this string? str) => ToOrNull<DateTime>(str, ToDateTimeTry);

    #endregion DateTime

    #region DateOnly

    public static DateOnly ToDateOnly(this string str)
    {
        str = str.Trim();
        if (str.ToDateOnlyTry(out var o)) return o;
        return DateOnly.Parse(str);
    }

    public static bool ToDateOnlyTry(this string? str, out DateOnly output)
    {
        str = str.TrimOrNull();
        var r = DateOnly.TryParse(str, out output);
        if (r || str == null) return r;

        r = str.ToDateTimeTry(out var dateTime);
        if (r) output = DateOnly.FromDateTime(dateTime);

        return r;
    }

    public static DateOnly? ToDateOnlyNullable(this string? str) => str.TrimOrNull()?.ToDateOnly();

    public static bool ToDateOnlyNullableTry(this string? str, out DateOnly? output) => ToNullableTryStruct(str, out output, ToDateOnlyTry);

    public static DateOnly? ToDateOnlyOrNull(this string? str) => ToOrNull<DateOnly>(str, ToDateOnlyTry);

    #endregion DateOnly

    #region TimeOnly

    public static TimeOnly ToTimeOnly(this string str)
    {
        str = str.Trim();
        if (str.ToTimeOnlyTry(out var o)) return o;
        return TimeOnly.Parse(str);
    }

    public static bool ToTimeOnlyTry(this string? str, out TimeOnly output)
    {
        str = str.TrimOrNull();
        var r = TimeOnly.TryParse(str, out output);
        if (r || str == null) return r;

        r = str.ToDateTimeTry(out var dateTime);
        if (r) output = TimeOnly.FromDateTime(dateTime);

        return r;
    }

    public static TimeOnly? ToTimeOnlyNullable(this string? str) => str.TrimOrNull()?.ToTimeOnly();

    public static bool ToTimeOnlyNullableTry(this string? str, out TimeOnly? output) => ToNullableTryStruct(str, out output, ToTimeOnlyTry);

    public static TimeOnly? ToTimeOnlyOrNull(this string? str) => ToOrNull<TimeOnly>(str, ToTimeOnlyTry);

    #endregion TimeOnly

    #region TimeSpan

    public static TimeSpan ToTimeSpan(this string str) => TimeSpan.Parse(str.Trim());

    public static bool ToTimeSpanTry(this string? str, out TimeSpan output) => TimeSpan.TryParse(str.TrimOrNull(), out output);

    public static TimeSpan? ToTimeSpanNullable(this string? str) => str.TrimOrNull()?.ToTimeSpan();

    public static bool ToTimeSpanNullableTry(this string? str, out TimeSpan? output) => ToNullableTryStruct(str, out output, ToTimeSpanTry);

    public static TimeSpan? ToTimeSpanOrNull(this string? str) => ToOrNull<TimeSpan>(str, ToTimeSpanTry);

    #endregion TimeSpan

    #endregion DateTime

    #region System

    #region Enum

    public static object ToEnum(this string str, Type enumType) => Enum.Parse(enumType, str.Trim(), true);

    public static bool ToEnumTry(this string? str, Type enumType, out object? output) => Enum.TryParse(enumType, str.TrimOrNull(), true, out output);

    public static object? ToEnumNullable(this string? str, Type enumType) => str.TrimOrNull()?.ToEnum(enumType);

    public static bool ToEnumNullableTry(this string? str, Type enumType, out object? output)
    {
        str = str.TrimOrNull();
        if (str == null)
        {
            output = default;
            return true;
        }

        var r = ToEnumTry(str, enumType, out var o);
        output = r ? o : default;
        return r;
    }

    public static object? ToEnumOrNull(this string? str, Type enumType) => ToEnumTry(str, enumType, out var o) ? o : default;

    #endregion Enum

    #region Guid

    public static Guid ToGuid(this string str) => Guid.Parse(str.Trim());

    public static bool ToGuidTry(this string? str, out Guid output) => Guid.TryParse(str.TrimOrNull(), out output);

    public static Guid? ToGuidNullable(this string? str) => str.TrimOrNull()?.ToGuid();

    public static bool ToGuidNullableTry(this string? str, out Guid? output) => ToNullableTryStruct(str, out output, ToGuidTry);

    public static Guid? ToGuidOrNull(this string? str) => ToOrNull<Guid>(str, ToGuidTry);

    #endregion Guid

    #region IPAddress

    // ReSharper disable once InconsistentNaming
    public static IPAddress ToIPAddress(this string str) => IPAddress.Parse(str.Trim());

    // ReSharper disable once InconsistentNaming
    public static IPAddress? ToIPAddressNullable(this string? str)
    {
        str = str.TrimOrNull();
        return str == null ? null : ToIPAddress(str);
    }

    // ReSharper disable once InconsistentNaming
    public static bool ToIPAddressTry(this string? str, out IPAddress? output)
    {
        str = str.TrimOrNull();
        if (str == null)
        {
            output = null;
            return false;
        }

        return IPAddress.TryParse(str, out output);
    }

    public static IPAddress? ToIPAddressOrNull(this string? str) => ToIPAddressTry(str, out var o) ? o : default;

    #endregion IPAddress

    #region Uri

    public static Uri ToUri(this string str) => new(str.Trim());
    public static Uri? ToUriNullable(this string? str)
    {
        str = str.TrimOrNull();
        return str == null ? null : ToUri(str);
    }

    public static bool ToUriTry(this string? str, out Uri? output)
    {
        str = str.TrimOrNull();
        if (str == null)
        {
            output = null;
            return false;
        }

        return Uri.TryCreate(str, UriKind.Absolute, out output);
    }

    public static Uri? ToUriOrNull(this string? str) => ToUriTry(str, out var o) ? o : default;

    #endregion Uri

    #region MailAddress

    public static MailAddress ToMailAddress(this string str) => new(str.Trim());
    public static MailAddress? ToMailAddressNullable(this string? str)
    {
        str = str.TrimOrNull();
        return str == null ? null : ToMailAddress(str);
    }

    public static bool ToMailAddressTry(this string? str, out MailAddress? output)
    {
        str = str.TrimOrNull();
        if (str == null)
        {
            output = null;
            return false;
        }

        try
        {
            output = new(str);
            return true;
        }
        catch (Exception)
        {
            output = null;
            return false;
        }
    }

    public static MailAddress? ToMailAddressOrNull(this string? str) => ToMailAddressTry(str, out var o) ? o : default;

    #endregion MailAddress

    #region SecureString

    public static SecureString ToSecureString(this string str) => new NetworkCredential("", str).SecurePassword;

    public static SecureString? ToSecureStringNullable(this string? str)
    {
        return str == null ? null : ToSecureString(str);
    }

    #endregion SecureString

    #region BigInteger

    public static BigInteger ToBigInteger(this string str) => BigInteger.Parse(str.Trim());

    public static bool ToBigIntegerTry(this string? str, out BigInteger output) => BigInteger.TryParse(str.TrimOrNull(), out output);

    public static BigInteger? ToBigIntegerNullable(this string? str) => str.TrimOrNull()?.ToBigInteger();

    public static bool ToBigIntegerNullableTry(this string? str, out BigInteger? output) => ToNullableTryStruct(str, out output, ToBigIntegerTry);

    public static BigInteger? ToBigIntegerOrNull(this string? str) => ToOrNull<BigInteger>(str, ToBigIntegerTry);

    #endregion BigInteger

    #region Color

    public static Color ToColor(this string str)
    {
        str = str.CheckNotNullTrimmed();
        try
        {
            return ColorTranslator.FromHtml(str);
        }
        catch (Exception) { }

        var commaCount = str.CountOccurrences(",");
        if (commaCount.In(2, 3))
        {
            var rgbNumsChars = (Constant.Chars_0_9_String + ",").ToHashSet();
            var rgbNums = str
                .Where(o => rgbNumsChars.Contains(o))
                .ToStringJoined()
                .Split(',')
                .TrimOrNull()
                .Select(o => o.ToByteOrNull())
                .WhereNotNull()
                .Select(o => (int)o)
                .ToArray();
            if (rgbNums.Length == 3) return Color.FromArgb(rgbNums[0], rgbNums[1], rgbNums[2]);
            if (rgbNums.Length == 4) return Color.FromArgb(rgbNums[0], rgbNums[1], rgbNums[2], rgbNums[3]);
        }

        var colorNameChars = (Constant.Chars_A_Z_Upper_String + Constant.Chars_A_Z_Lower_String).ToHashSet();
        var colorName = str.Where(o => colorNameChars.Contains(o)).ToStringJoined();
        if (colorName.Length > 0)
        {
            try
            {
                return ColorTranslator.FromHtml(str);
            }
            catch (Exception) { }
        }

        throw new ArgumentException($"Could not parse to {nameof(Color)} -> " + str, nameof(str));
    }

    public static bool ToColorTry(this string? str, out Color output)
    {
        if (str != null)
        {
            try
            {
                output = ToColor(str);
                return true;
            } catch (Exception) {}
        }

        output = default;
        return false;
    }

    public static Color? ToColorNullable(this string? str) => str.TrimOrNull()?.ToColor();

    public static bool ToColorNullableTry(this string? str, out Color? output) => ToNullableTryStruct(str, out output, ToColorTry);

    public static Color? ToColorOrNull(this string? str) => ToOrNull<Color>(str, ToColorTry);

    #endregion Color

    #endregion System

}
