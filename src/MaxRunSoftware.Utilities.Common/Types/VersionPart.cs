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
