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

using System.Numerics;

namespace MaxRunSoftware.Utilities.Common;

public sealed class VersionPart : IEquatable<VersionPart>, IComparable<VersionPart>, IComparable
{
    public string Value { get; }
    public int? ValueInt { get; }
    public long? ValueLong { get; }
    public BigInteger? ValueBigInteger { get; }
    public DateTime? ValueDateTime { get; }
    public DateOnly? ValueDate { get; }

    public bool IsPreRelease => ValueBigInteger == null;

    public VersionPart(string part)
    {
        Value = part;

        if (part.Length == 0)
        {
            getHashCode = string.Empty.GetHashCode();
            return;
        }

        if (Value.ToIntTry(out var valueInt)) ValueInt = valueInt;
        if (Value.ToLongTry(out var valueLong)) ValueLong = valueLong;
        if (Value.ToBigIntegerTry(out var valueBigInteger)) ValueBigInteger = valueBigInteger;
        if (Value.ToDateTimeTry(out var valueDateTime)) ValueDateTime = valueDateTime;
        if (ValueDateTime.HasValue) ValueDate = DateOnly.FromDateTime(ValueDateTime.Value);

        getHashCode = ValueInt?.GetHashCode()
                      ?? ValueLong?.GetHashCode()
                      ?? ValueBigInteger?.GetHashCode()
                      ?? StringComparer.OrdinalIgnoreCase.GetHashCode(Value);
    }

    public override string ToString() => Value;

    #region Equals

    public bool Equals(VersionPart? other) => Equals(this, other);

    public override bool Equals(object? obj) => Equals(obj as VersionPart);

    public override int GetHashCode() => getHashCode;
    private readonly int getHashCode;

    public static bool Equals(VersionPart? left, VersionPart? right)
    {
        if (ReferenceEquals(left, right)) return true;
        if (ReferenceEquals(left, null))
        {
            if (ReferenceEquals(right, null)) return true;
            return right.ValueBigInteger.HasValue && right.ValueBigInteger.Value == BigInteger.Zero;
        }

        if (ReferenceEquals(right, null))
        {
            if (ReferenceEquals(left, null)) return true;
            return left.ValueBigInteger.HasValue && left.ValueBigInteger.Value == BigInteger.Zero;
        }

        if (left.GetHashCode() != right.GetHashCode()) return false;
        if (!left.ValueInt.IsEqual(right.ValueInt)) return false;
        if (!left.ValueLong.IsEqual(right.ValueLong)) return false;
        if (!left.ValueBigInteger.IsEqual(right.ValueBigInteger)) return false;
        if (!left.Value.EqualsOrdinalIgnoreCase(right.Value)) return false;
        return true;
    }

    public static bool operator ==(VersionPart? left, VersionPart? right) => Equals(left, right);
    public static bool operator !=(VersionPart? left, VersionPart? right) => !Equals(left, right);

    #endregion Equals

    #region CompareTo

    public int CompareTo(VersionPart? other) => CompareTo(this, other);
    public int CompareTo(object? obj)
    {
        if (ReferenceEquals(null, obj)) return 1;
        if (ReferenceEquals(this, obj)) return 0;
        if (obj is VersionPart other) return CompareTo(other);
        throw new ArgumentException($"Object must be of type {nameof(VersionPart)}");
    }

    public static int CompareTo(VersionPart? left, VersionPart? right)
    {
        if (ReferenceEquals(left, right)) return 0;
        if (ReferenceEquals(left, null))
        {
            if (ReferenceEquals(right, null)) return 0;
            return right.IsPreRelease ? 1 : -1;
        }

        if (ReferenceEquals(right, null))
        {
            if (ReferenceEquals(left, null)) return 0;
            return left.IsPreRelease ? -1 : 1;
        }

        if (left.IsPreRelease && !right.IsPreRelease) return -1;
        if (!left.IsPreRelease && right.IsPreRelease) return 1;
        int c;
        if (0 != (c = left.ValueBigInteger.CompareToNullable(right.ValueBigInteger))) return c;
        if (0 != (c = left.Value.CompareToOrdinalIgnoreCaseThenOrdinal(right.Value))) return c;
        return c;
    }

    public static bool operator <(VersionPart? left, VersionPart? right) => CompareTo(left, right) < 0;
    public static bool operator >(VersionPart? left, VersionPart? right) => CompareTo(left, right) > 0;
    public static bool operator <=(VersionPart? left, VersionPart? right) => CompareTo(left, right) <= 0;
    public static bool operator >=(VersionPart? left, VersionPart? right) => CompareTo(left, right) >= 0;

    #endregion CompareTo
}

public sealed class Version : IReadOnlyList<VersionPart>, IEquatable<Version>, IComparable<Version>
{
    private static readonly ImmutableHashSet<char> CHARS = Constant.Chars_A_Z_Upper.Concat(Constant.Chars_A_Z_Lower).ToImmutableHashSet();
    private static readonly ImmutableHashSet<char> NUMS = Constant.Chars_0_9.ToImmutableHashSet();

    private readonly ImmutableList<VersionPart> parts;
    private readonly string toString;
    public Version(string value)
    {
        toString = value;
        const int isChar = 1, isNum = 2, isOther = 3;

        var list = new List<VersionPart>();
        var sb = new StringBuilder(50);
        var lastCharType = isOther;

        foreach (var c in value)
        {
            var charType = CHARS.Contains(c) ? isChar : NUMS.Contains(c) ? isNum : isOther;
            if (charType == isOther || charType != lastCharType)
            {
                list.Add(new(sb.ToString()));
                sb.Clear();
            }

            if (charType != isOther) sb.Append(c);
            lastCharType = charType;
        }

        list.Add(new(sb.ToString()));
        parts = list.Where(o => !string.IsNullOrWhiteSpace(o.Value)).ToImmutableList();
        if (parts.Count == 0) throw new ArgumentException($"'{value}' is not a valid version", nameof(value));
        getHashCode = Util.HashEnumerable(parts);
    }

    public override string ToString() => toString;

    #region Other

    public VersionPart? Major => this.GetAtIndexOrDefault(0);
    public VersionPart? Minor => this.GetAtIndexOrDefault(1);
    public VersionPart? Build => this.GetAtIndexOrDefault(2);
    public VersionPart? Revision => this.GetAtIndexOrDefault(3);

    public System.Version ToSystemVersion()
    {
        var (a, b, c, d) = this.Take4Nullable();
        var exceptionMessage = $"Could not convert '{0}' to int for {typeof(System.Version).FullNameFormatted()}.{1}";
        if (a != null && a.ValueInt == null) throw new ArgumentOutOfRangeException(nameof(System.Version.Major), a.Value, string.Format(exceptionMessage, a.Value, nameof(System.Version.Major)));
        if (b != null && b.ValueInt == null) throw new ArgumentOutOfRangeException(nameof(System.Version.Minor), b.Value, string.Format(exceptionMessage, b.Value, nameof(System.Version.Minor)));
        if (c != null && c.ValueInt == null) throw new ArgumentOutOfRangeException(nameof(System.Version.Build), c.Value, string.Format(exceptionMessage, c.Value, nameof(System.Version.Build)));
        if (d != null && d.ValueInt == null) throw new ArgumentOutOfRangeException(nameof(System.Version.Revision), d.Value, string.Format(exceptionMessage, d.Value, nameof(System.Version.Revision)));

        return new(a?.ValueInt ?? 0, b?.ValueInt ?? 0, c?.ValueInt ?? 0, d?.ValueInt ?? 0);
    }

    public bool IsPreRelease => Count > 0 && this.Any(o => o.IsPreRelease);

    #endregion Other

    #region IReadOnlyList

    public IEnumerator<VersionPart> GetEnumerator() => parts.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => parts.Count;

    public VersionPart this[int index] => parts[index];

    #endregion IReadOnlyList

    #region Equals

    private readonly int getHashCode;
    public override int GetHashCode() => getHashCode;

    public bool Equals(Version? other) => Equals(this, other);
    public override bool Equals(object? obj) => Equals(obj as Version);
    public static bool operator ==(Version? left, Version? right) => Equals(left, right);
    public static bool operator !=(Version? left, Version? right) => !Equals(left, right);

    public static bool Equals(Version? left, Version? right)
    {
        if (ReferenceEquals(left, right)) return true;
        if (ReferenceEquals(left, null) || ReferenceEquals(right, null)) return false;
        var c = Math.Max(left.Count, right.Count);
        for (var i = 0; i < c; i++)
        {
            if (!VersionPart.Equals(left.GetAtIndexOrDefault(i), right.GetAtIndexOrDefault(i))) return false;
        }

        return true;
    }

    #endregion Equals

    #region CompareTo

    public int CompareTo(Version? other) => CompareTo(this, other);
    public int CompareTo(object? obj)
    {
        if (ReferenceEquals(null, obj)) return 1;
        if (ReferenceEquals(this, obj)) return 0;
        if (obj is Version other) return CompareTo(other);
        throw new ArgumentException($"Object must be of type {nameof(Version)}");
    }

    private static int CompareTo(Version? left, Version? right)
    {
        if (ReferenceEquals(left, right)) return 0;
        if (ReferenceEquals(left, null))
        {
            return ReferenceEquals(right, null) ? 0 : -1;
        }

        if (ReferenceEquals(right, null))
        {
            return ReferenceEquals(left, null) ? 0 : 1;
        }

        var max = Math.Max(left.Count, right.Count);
        for (var i = 0; i < max; i++)
        {
            var x = left.GetAtIndexOrDefault(i);
            var y = right.GetAtIndexOrDefault(i);
            var c = VersionPart.CompareTo(x, y);
            if (c != 0) return c;
        }

        return left.Count.CompareTo(right.Count);
    }

    public static bool operator <(Version? left, Version? right) => CompareTo(left, right) < 0;
    public static bool operator >(Version? left, Version? right) => CompareTo(left, right) > 0;
    public static bool operator <=(Version? left, Version? right) => CompareTo(left, right) <= 0;
    public static bool operator >=(Version? left, Version? right) => CompareTo(left, right) >= 0;

    #endregion CompareTo
}
