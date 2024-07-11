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

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable RedundantCast

namespace MaxRunSoftware.Utilities.Common;

[Serializable]
[StructLayout(LayoutKind.Sequential)]
public readonly struct Percent :
    IConvertible, 
    IComparable<Percent?>,
    IEquatable<Percent?>, 
    INumber<Percent>,
    IUnsignedNumber<Percent>,
    IMinMaxValue<Percent>
{
    static Percent() => valuesInt = Lzy.Create(() => Enumerable.Range(0, 101).Select(o => new Percent(o)).ToImmutableArray());

    /// <summary>
    /// https://stackoverflow.com/a/33697376
    /// </summary>
    private const string DOUBLE_FIXED_POINT = "0.###################################################################################################################################################################################################################################################################################################################################################";

    private const long MIN_LONG = 0;
    private const long MAX_LONG = 100;
    private const ulong MAX_ULONG = 100;
    
    private const double ZERO_DOUBLE = 0.0;
    public static Percent Zero => (Percent)ZERO_DOUBLE;

    private const double ONE_DOUBLE = 1.0;
    public static Percent One => (Percent)ONE_DOUBLE;

    public static Percent MinValue => (Percent)ZERO_DOUBLE;
    
    private const double MAX_DOUBLE = 100.0;
    public static Percent MaxValue => (Percent)MAX_DOUBLE;
    
    public static int Radix { get; } = 2;
    
    private readonly double m_value; // Do not rename (binary serialization)

    private static readonly Lzy<ImmutableArray<Percent>> valuesInt;
    public static ImmutableArray<Percent> ValuesInt => valuesInt.Value;

    public Percent() : this(Zero.m_value) { }

    private Percent(double value) => m_value = value switch
    {
        < ZERO_DOUBLE => ZERO_DOUBLE,
        > MAX_DOUBLE => MAX_DOUBLE,
        _ => value,
    };
    
    private Percent(long value) => m_value = value switch
    {
        < MIN_LONG => ZERO_DOUBLE,
        > MAX_LONG => MAX_DOUBLE,
        _ => value,
    }; 

    private Percent(ulong value) => m_value = value switch
    {
        > MAX_ULONG => MAX_DOUBLE,
        _ => value,
    }; 
    
    #region operator

    public static Percent AdditiveIdentity => Zero;
    public static Percent MultiplicativeIdentity => One;
    
    // https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/operator-overloading
    public static bool operator <(Percent left, Percent right) => left.CompareTo(right) < 0;
    public static bool operator >(Percent left, Percent right) => left.CompareTo(right) > 0;
    public static bool operator <=(Percent left, Percent right) => left.CompareTo(right) <= 0;
    public static bool operator >=(Percent left, Percent right) => left.CompareTo(right) >= 0;
    public static bool operator ==(Percent left, Percent right) => left.Equals(right);
    public static bool operator !=(Percent left, Percent right) => !left.Equals(right);
    
    public static Percent operator +(Percent left) => left;
    public static Percent operator -(Percent left) => new(MaxValue.m_value - left.m_value);
    public static Percent operator +(Percent left, Percent right) => new(left.m_value + right.m_value);
    public static Percent operator -(Percent left, Percent right) => new(left.m_value - right.m_value);
    public static Percent operator *(Percent left, Percent right) => new(left.m_value * right.m_value);
    public static Percent operator /(Percent left, Percent right) => new(left.m_value / right.m_value);
    public static Percent operator %(Percent left, Percent right) => new(left.m_value % right.m_value);

    public static Percent operator ++(Percent left)
    {
        // Behave like uint.MaxValue++
        var d = left.m_value + ONE_DOUBLE;
        return d > MAX_DOUBLE ? MinValue : new(d);
    }
    public static Percent operator --(Percent left)
    {
        // Behave like uint.MinValue--
        var d = left.m_value - ONE_DOUBLE;
        return d < ZERO_DOUBLE ? MaxValue : new(d);
    }

    public static implicit operator byte(Percent percent) => Convert.ToByte(percent.m_value);
    public static implicit operator sbyte(Percent percent) => Convert.ToSByte(percent.m_value);
    public static implicit operator short(Percent percent) => (byte)percent;
    public static implicit operator ushort(Percent percent) => (byte)percent;
    public static implicit operator int(Percent percent) => (byte)percent;
    public static implicit operator uint(Percent percent) => (byte)percent;
    public static implicit operator ulong(Percent percent) => (byte)percent;
    public static implicit operator long(Percent percent) => (byte)percent;
    public static implicit operator float(Percent percent) => Convert.ToSingle(percent.m_value);
    public static implicit operator double(Percent percent) => percent.m_value;
    public static implicit operator decimal(Percent percent) => Convert.ToDecimal(percent.m_value);

    public static implicit operator Percent(byte value) => new(value);
    public static implicit operator Percent(sbyte value) => new(value);
    public static implicit operator Percent(short value) => new(value);
    public static implicit operator Percent(ushort value) => new(value);
    public static implicit operator Percent(int value) => new(value);
    public static implicit operator Percent(uint value) => new(value);
    public static implicit operator Percent(long value) => new(value);
    public static implicit operator Percent(ulong value) => new(value);
    public static implicit operator Percent(float value) => new(value);
    public static implicit operator Percent(double value) => new(value);
    public static implicit operator Percent(decimal value) => new(Convert.ToDouble(value));

    #endregion operator

    #region Equals

    #region static

    /// <summary>
    /// https://docs.microsoft.com/en-us/dotnet/api/system.double.equals
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="tolerance"></param>
    /// <returns></returns>
    private static bool Equals(Percent? x, Percent? y, double? tolerance = null)
    {
        if (x == null) return y == null;
        if (y == null) return false;

        var xv = x.Value;
        var yv = y.Value;
        if (tolerance == null) return xv.m_value.Equals(yv.m_value);

        var tv = tolerance.Value;
        return Math.Abs(xv.m_value - yv.m_value) <= Math.Abs(xv.m_value * tv);
    }

    #endregion static

    public override bool Equals([NotNullWhen(true)] object? obj) => obj is Percent p && Equals(p);

    public bool Equals(Percent obj) => Equals(this, obj);
    public bool Equals(Percent obj, double tolerance) => Equals(this, obj, tolerance);
    public bool Equals(Percent obj, double? tolerance) => Equals(this, obj, tolerance);

    public bool Equals([NotNullWhen(true)] Percent? obj) => Equals(this, obj);
    public bool Equals([NotNullWhen(true)] Percent? obj, double tolerance) => Equals(this, obj, tolerance);
    public bool Equals([NotNullWhen(true)] Percent? obj, double? tolerance) => Equals(this, obj, tolerance);

    #endregion Equals

    #region GetHashCode

    public override int GetHashCode() => m_value.GetHashCode();

    #endregion GetHashCode

    #region CompareTo

    #region static

    /// <summary>
    /// https://docs.microsoft.com/en-us/dotnet/api/system.double.equals
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="tolerance"></param>
    /// <returns></returns>
    private static int CompareTo(Percent? x, Percent? y, double? tolerance = null)
    {
        if (x == null) return y == null ? 0 : -1;
        if (y == null) return 1;

        var xv = x.Value;
        var yv = y.Value;
        if (tolerance == null) return xv.m_value.CompareTo(yv.m_value);

        return Equals(xv, yv, tolerance.Value) ? 0 : xv.m_value.CompareTo(yv.m_value);
    }

    #endregion static

    public int CompareTo(object? value) => value == null ? 1 : CompareTo((Percent)value);

    public int CompareTo(Percent obj) => CompareTo(this, obj);
    public int CompareTo(Percent obj, double tolerance) => CompareTo(this, obj, tolerance);
    public int CompareTo(Percent obj, double? tolerance) => CompareTo(this, obj, tolerance);

    public int CompareTo(Percent? obj) => CompareTo(this, obj);
    public int CompareTo(Percent? obj, double tolerance) => CompareTo(this, obj, tolerance);
    public int CompareTo(Percent? obj, double? tolerance) => CompareTo(this, obj, tolerance);

    #endregion CompareTo

    #region ToString

    // ReSharper disable once SpecifyACultureInStringConversionExplicitly
    public override string ToString() => m_value.ToString(DOUBLE_FIXED_POINT);

    string IConvertible.ToString(IFormatProvider? provider) => m_value.ToString(provider);

    public string ToString(string? format) => m_value.ToString(format);

    public string ToString(string? format, IFormatProvider? provider) => m_value.ToString(format, provider);

    public string ToString(int decimalPlaces)
    {
        if (decimalPlaces < byte.MinValue) decimalPlaces = byte.MinValue;
        if (decimalPlaces > byte.MinValue) decimalPlaces = byte.MaxValue;

        var rounded = Math.Round(m_value, decimalPlaces, MidpointRounding.AwayFromZero);
        //return rounded.ToString("N" + decimalPlaces);

        return rounded.ToString("0." + new string('#', decimalPlaces));
    }

    #endregion ToString

    #region TryFormat

    bool ISpanFormattable.TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) => m_value.TryFormat(destination, out charsWritten, format, provider);

    #endregion TryFormat

    #region Parse

    public static Percent Parse(string s) => (Percent)double.Parse(s);

    public static Percent Parse(string s, NumberStyles style) => (Percent)double.Parse(s, style);

    public static Percent Parse(string s, IFormatProvider? provider) => (Percent)double.Parse(s, provider);

    public static Percent Parse(string s, NumberStyles style, IFormatProvider? provider) => (Percent)double.Parse(s, style, provider);

    public static Percent Parse(ReadOnlySpan<char> s, NumberStyles style = NumberStyles.Integer, IFormatProvider? provider = null) => (Percent)double.Parse(s, style, provider);

    public static Percent Parse(ReadOnlySpan<char> s, IFormatProvider? provider = null) => Parse(s, style: NumberStyles.Integer, provider: null);

    private static Percent Parse(ReadOnlySpan<char> s, NumberStyles style, NumberFormatInfo info) => (Percent)double.Parse(s, style, info);

    #endregion Parse

    #region TryParse

    public static bool TryParse([NotNullWhen(true)] string? s, out Percent result)
    {
        var r = double.TryParse(s, out var o);
        if (r)
        {
            result = (Percent)o;
            return true;
        }

        result = MinValue;
        return false;
    }
    
    public static bool TryParse(string? s, IFormatProvider? provider, out Percent result) => throw new NotImplementedException();
    
    public static bool TryParse(ReadOnlySpan<char> s, out Percent result)
    {
        var r = double.TryParse(s, out var o);
        if (r)
        {
            result = (Percent)o;
            return true;
        }

        result = MinValue;
        return false;
    }

    public static bool TryParse([NotNullWhen(true)] string? s, NumberStyles style, IFormatProvider? provider, out Percent result)
    {
        var r = double.TryParse(s, style, provider, out var o);
        if (r)
        {
            result = (Percent)o;
            return true;
        }

        result = MinValue;
        return false;
    }

    public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out Percent result)
    {
        var r = double.TryParse(s, style, provider, out var o);
        if (r)
        {
            result = (Percent)o;
            return true;
        }

        result = MinValue;
        return false;
    }

    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Percent result) => TryParse(s, NumberStyles.Integer, provider, out result);

    #endregion TryParse

    #region TryConvert

#nullable disable
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryConvertFromChecked<TOther>(TOther value, out Percent result) where TOther : INumberBase<TOther>
    {
        var b = TryConvertFromChecked_Internal<TOther, double>(value, out var r);
        result = r;
        return b;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryConvertFromChecked_Internal<TOther, T>(TOther value, out T result) where TOther : INumberBase<TOther> where T : INumberBase<T> => T.TryConvertFromChecked(value, out result);
    
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryConvertFromSaturating<TOther>(TOther value, out Percent result) where TOther : INumberBase<TOther>
    {
        var b = TryConvertFromSaturating_Internal<TOther, double>(value, out var r);
        result = r;
        return b;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryConvertFromSaturating_Internal<TOther, T>(TOther value, out T result) where TOther : INumberBase<TOther> where T : INumberBase<T> => T.TryConvertFromSaturating(value, out result);
    
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryConvertFromTruncating<TOther>(TOther value, out Percent result) where TOther : INumberBase<TOther>
    {
        var b = TryConvertFromTruncating_Internal<TOther, double>(value, out var r);
        result = r;
        return b;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryConvertFromTruncating_Internal<TOther, T>(TOther value, out T result) where TOther : INumberBase<TOther> where T : INumberBase<T> => T.TryConvertFromTruncating(value, out result);

    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryConvertToChecked<TOther>(Percent value, out TOther result) where TOther : INumberBase<TOther>
    {
        var b = TryConvertToChecked_Internal<TOther, double>(value, out var r);
        result = r;
        return b;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryConvertToChecked_Internal<TOther, T>(T value, out TOther result) where TOther : INumberBase<TOther> where T : INumberBase<T> => T.TryConvertToChecked(value, out result);

    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryConvertToSaturating<TOther>(Percent value, out TOther result) where TOther : INumberBase<TOther>
    {
        var b = TryConvertToSaturating_Internal<TOther, double>(value, out var r);
        result = r;
        return b;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryConvertToSaturating_Internal<TOther, T>(T value, out TOther result) where TOther : INumberBase<TOther> where T : INumberBase<T> => T.TryConvertToSaturating(value, out result);
    
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryConvertToTruncating<TOther>(Percent value, out TOther result) where TOther : INumberBase<TOther>
    {
        var b = TryConvertToTruncating_Internal<TOther, double>(value, out var r);
        result = r;
        return b;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryConvertToTruncating_Internal<TOther, T>(T value, out TOther result) where TOther : INumberBase<TOther> where T : INumberBase<T> => T.TryConvertToTruncating(value, out result);
    
#nullable enable
    
    #endregion TryConvert
    
    #region IConvertible

    public TypeCode GetTypeCode() => TypeCode.Double;

    bool IConvertible.ToBoolean(IFormatProvider? provider) => Convert.ToBoolean(m_value);

    char IConvertible.ToChar(IFormatProvider? provider) => Convert.ToChar(m_value);

    sbyte IConvertible.ToSByte(IFormatProvider? provider) => Convert.ToSByte(m_value);

    byte IConvertible.ToByte(IFormatProvider? provider) => Convert.ToByte(m_value);

    short IConvertible.ToInt16(IFormatProvider? provider) => Convert.ToInt16(m_value);

    ushort IConvertible.ToUInt16(IFormatProvider? provider) => Convert.ToUInt16(m_value);

    int IConvertible.ToInt32(IFormatProvider? provider) => Convert.ToInt32(m_value);

    uint IConvertible.ToUInt32(IFormatProvider? provider) => Convert.ToUInt32(m_value);

    long IConvertible.ToInt64(IFormatProvider? provider) => Convert.ToInt64(m_value);

    ulong IConvertible.ToUInt64(IFormatProvider? provider) => Convert.ToUInt64(m_value);

    float IConvertible.ToSingle(IFormatProvider? provider) => Convert.ToSingle(m_value);

    double IConvertible.ToDouble(IFormatProvider? provider) => m_value;

    decimal IConvertible.ToDecimal(IFormatProvider? provider) => Convert.ToDecimal(m_value);

    DateTime IConvertible.ToDateTime(IFormatProvider? provider) => Convert.ToDateTime(m_value);

    object IConvertible.ToType(Type type, IFormatProvider? provider) => ((IConvertible)m_value).ToType(type, provider);

    #endregion IConvertible
    
    #region INumber
    
    
    public static Percent Abs(Percent value) => double.Abs(value);

    public static bool IsCanonical(Percent value) => true;

    public static bool IsComplexNumber(Percent value) => false;

    public static bool IsEvenInteger(Percent value) => double.IsEvenInteger(value);

    public static bool IsFinite(Percent value) => double.IsFinite(value);

    public static bool IsImaginaryNumber(Percent value) => IsImaginaryNumber_Internal(value.m_value);
    public static bool IsImaginaryNumber_Internal<T>(T value) where T : INumber<T> => T.IsImaginaryNumber(value);

    public static bool IsInfinity(Percent value) => double.IsInfinity(value);

    public static bool IsInteger(Percent value) => double.IsInteger(value);

    public static bool IsNaN(Percent value) => double.IsNaN(value);

    public static bool IsNegative(Percent value) => double.IsNegative(value);

    public static bool IsNegativeInfinity(Percent value) => double.IsNegativeInfinity(value);

    public static bool IsNormal(Percent value) => double.IsNormal(value);

    public static bool IsOddInteger(Percent value) => double.IsOddInteger(value);

    public static bool IsPositive(Percent value) => double.IsPositive(value);

    public static bool IsPositiveInfinity(Percent value) => double.IsPositiveInfinity(value);

    public static bool IsRealNumber(Percent value) => double.IsRealNumber(value);

    public static bool IsSubnormal(Percent value) => double.IsSubnormal(value);

    public static bool IsZero(Percent value) => value == Zero;

    public static Percent MaxMagnitude(Percent x, Percent y) => double.MaxMagnitude(x, y);

    public static Percent MaxMagnitudeNumber(Percent x, Percent y) => double.MaxMagnitudeNumber(x, y);

    public static Percent MinMagnitude(Percent x, Percent y) => double.MinMagnitude(x, y);

    public static Percent MinMagnitudeNumber(Percent x, Percent y) => double.MinMagnitudeNumber(x, y);
    
    #endregion INumber

}
