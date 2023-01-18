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

using System.Diagnostics.CodeAnalysis;

namespace MaxRunSoftware.Utilities.Common;

[PublicAPI]
public sealed class TypeSlim :
    IEquatable<TypeSlim>, IEquatable<Type>,
    IComparable, IComparable<TypeSlim>, IComparable<Type>
{
    public string Name { get; }
    public string NameFull { get; }
    public Type Type { get; }
    public TypeInfo Info { get; }
    public AssemblySlim Assembly { get; }
    public int TypeHashCode { get; }
    private readonly int getHashCode;
    public IEnumerable<Attribute> Attributes => Type.GetCustomAttributes();

    private readonly Lzy<bool> isRealType;
    public bool IsRealType => isRealType.Value;

    public TypeSlim(Type type)
    {
        Type = type.CheckNotNull(nameof(type));
        Info = Type.GetTypeInfo();
        Assembly = new AssemblySlim(type.Assembly);
        NameFull = (string.IsNullOrWhiteSpace(type.FullName) ? null : type.FullNameFormatted().TrimOrNull())
                   ?? type.NameFormatted().TrimOrNull()
                   ?? type.Name.TrimOrNull()
                   ?? type.ToString().TrimOrNull()
                   ?? type.Name;
        Name = type.NameFormatted().TrimOrNull()
               ?? type.Name.TrimOrNull()
               ?? type.ToString().TrimOrNull()
               ?? NameFull;
        TypeHashCode = Type.GetHashCode();
        getHashCode = Util.Hash(TypeHashCode, Assembly.GetHashCode(), StringComparer.Ordinal.GetHashCode(NameFull));
        isRealType = Lzy.Create(() => Type.IsRealType());
    }

    #region Override

    public override int GetHashCode() => getHashCode;
    public override string ToString() => NameFull;

    #region Equals

    public static bool Equals(TypeSlim? left, TypeSlim? right) => left?.Equals(right) ?? ReferenceEquals(right, null);

    public override bool Equals([NotNullWhen(true)] object? obj) => obj switch
    {
        null => false,
        TypeSlim slim => Equals(slim),
        Type other => Equals(other),
        _ => false,
    };

    public bool Equals([NotNullWhen(true)] Type? other)
    {
        if (ReferenceEquals(other, null)) return false;
        if (ReferenceEquals(Type, other)) return true;
        return Equals(new TypeSlim(other));
    }

    public bool Equals([NotNullWhen(true)] TypeSlim? other)
    {
        if (ReferenceEquals(other, null)) return false;
        if (ReferenceEquals(this, other)) return true;

        if (getHashCode != other.getHashCode) return false;
        if (TypeHashCode != other.TypeHashCode) return false;
        if (!Assembly.Equals(other.Assembly)) return false;
        if (!StringComparer.Ordinal.Equals(NameFull, other.NameFull)) return false;
        if (Type != other.Type) return false;

        return true;
    }

    #endregion Equals

    #region CompareTo

    public int CompareTo(object? obj) => obj switch
    {
        null => 1,
        TypeSlim slim => CompareTo(slim),
        Type other => CompareTo(other),
        _ => 1,
    };

    public int CompareTo(Type? other)
    {
        if (ReferenceEquals(other, null)) return 1;
        if (ReferenceEquals(Type, other)) return 0;
        return CompareTo(new TypeSlim(other));
    }

    public int CompareTo(TypeSlim? other)
    {
        if (ReferenceEquals(other, null)) return 1;
        if (ReferenceEquals(this, other)) return 0;

        int c;
        if (0 != (c = Assembly.CompareTo(other.Assembly))) return c;
        if (0 != (c = Constant.StringComparer_OrdinalIgnoreCase_Ordinal.Compare(NameFull, other.NameFull))) return c;
        if (0 != (c = TypeHashCode.CompareTo(other.TypeHashCode))) return c;
        if (0 != (c = getHashCode.CompareTo(other.getHashCode))) return c;
        return c;
    }

    #endregion CompareTo

    #endregion Override

    #region Implicit / Explicit

    // ReSharper disable ArrangeStaticMemberQualifier

    public static bool operator ==(TypeSlim? left, TypeSlim? right) => TypeSlim.Equals(left, right);
    public static bool operator !=(TypeSlim? left, TypeSlim? right) => !TypeSlim.Equals(left, right);

    // ReSharper restore ArrangeStaticMemberQualifier

    public static implicit operator Type(TypeSlim typeSlim) => typeSlim.Type;
    public static implicit operator TypeSlim(Type type) => new(type);

    public static implicit operator TypeInfo(TypeSlim typeSlim) => typeSlim.Info;
    public static implicit operator TypeSlim(TypeInfo typeInfo) => new(typeInfo);

    #endregion Implicit / Explicit
}

public static class TypeSlimExtensions
{
    public static Type ToType(this TypeSlim obj) => obj;
    public static TypeSlim ToTypeSlim(this Type obj) => obj;

    public static TypeInfo ToTypeInfo(this TypeSlim obj) => obj;
    public static TypeSlim ToTypeSlim(this TypeInfo obj) => obj;
}

public sealed class TypeSlimTuple2 : IEquatable<TypeSlimTuple2>, IComparable<TypeSlimTuple2>, IComparable
{
    public TypeSlimTuple2(TypeSlim item1, TypeSlim item2)
    {
        Item1 = item1;
        Item2 = item2;
        getHashCode = Util.Hash(item1.GetHashCode(), item2.GetHashCode());
    }

    public TypeSlim Item1 { get; }
    public TypeSlim Item2 { get; }
    private readonly int getHashCode;
    public override int GetHashCode() => getHashCode;
    public override string ToString() => GetType().Name + $"({Item1}, {Item2})";

    #region Equals

    public override bool Equals(object? obj) => obj switch { null => false, TypeSlimTuple2 other => Equals(other), _ => false, };

    public bool Equals(TypeSlimTuple2? other)
    {
        if (ReferenceEquals(other, null)) return false;
        if (ReferenceEquals(this, other)) return true;
        if (getHashCode != other.getHashCode) return false;
        if (!Item1.Equals(other.Item1)) return false;
        if (!Item2.Equals(other.Item2)) return false;
        return true;
    }

    #endregion Equals

    #region CompareTo

    public int CompareTo(object? obj) => obj switch { null => 1, TypeSlimTuple2 other => CompareTo(other), _ => 1, };

    public int CompareTo(TypeSlimTuple2? other)
    {
        if (ReferenceEquals(other, null)) return 1;
        if (ReferenceEquals(this, other)) return 0;
        int c;
        if (0 != (c = Item1.CompareTo(other.Item1))) return c;
        if (0 != (c = Item2.CompareTo(other.Item2))) return c;
        if (0 != (c = getHashCode.CompareTo(other.getHashCode))) return c;
        return c;
    }

    #endregion CompareTo

    public static bool Equals(TypeSlimTuple2? left, TypeSlimTuple2? right) => left?.Equals(right) ?? ReferenceEquals(right, null);

    // ReSharper disable ArrangeStaticMemberQualifier

    public static bool operator ==(TypeSlimTuple2? left, TypeSlimTuple2? right) => TypeSlimTuple2.Equals(left, right);
    public static bool operator !=(TypeSlimTuple2? left, TypeSlimTuple2? right) => !TypeSlimTuple2.Equals(left, right);

    // ReSharper restore ArrangeStaticMemberQualifier
}


public sealed class TypeSlimTuple3 : IEquatable<TypeSlimTuple3>, IComparable<TypeSlimTuple3>, IComparable
{
    public TypeSlimTuple3(TypeSlim item1, TypeSlim item2, TypeSlim item3)
    {
        Item1 = item1;
        Item2 = item2;
        Item3 = item3;
        getHashCode = Util.Hash(item1.GetHashCode(), item2.GetHashCode(), item3.GetHashCode());
    }

    public TypeSlim Item1 { get; }
    public TypeSlim Item2 { get; }
    public TypeSlim Item3 { get; }
    private readonly int getHashCode;
    public override int GetHashCode() => getHashCode;
    public override string ToString() => GetType().Name + $"({Item1}, {Item2}, {Item3})";

    #region Equals

    public override bool Equals(object? obj) => obj switch { null => false, TypeSlimTuple3 other => Equals(other), _ => false, };

    public bool Equals(TypeSlimTuple3? other)
    {
        if (ReferenceEquals(other, null)) return false;
        if (ReferenceEquals(this, other)) return true;
        if (getHashCode != other.getHashCode) return false;
        if (!Item1.Equals(other.Item1)) return false;
        if (!Item2.Equals(other.Item2)) return false;
        if (!Item3.Equals(other.Item3)) return false;
        return true;
    }

    #endregion Equals

    #region CompareTo

    public int CompareTo(object? obj) => obj switch { null => 1, TypeSlimTuple3 other => CompareTo(other), _ => 1, };

    public int CompareTo(TypeSlimTuple3? other)
    {
        if (ReferenceEquals(other, null)) return 1;
        if (ReferenceEquals(this, other)) return 0;
        int c;
        if (0 != (c = Item1.CompareTo(other.Item1))) return c;
        if (0 != (c = Item2.CompareTo(other.Item2))) return c;
        if (0 != (c = Item3.CompareTo(other.Item3))) return c;
        if (0 != (c = getHashCode.CompareTo(other.getHashCode))) return c;
        return c;
    }

    #endregion CompareTo

    public static bool Equals(TypeSlimTuple3? left, TypeSlimTuple3? right) => left?.Equals(right) ?? ReferenceEquals(right, null);

    // ReSharper disable ArrangeStaticMemberQualifier

    public static bool operator ==(TypeSlimTuple3? left, TypeSlimTuple3? right) => TypeSlimTuple3.Equals(left, right);
    public static bool operator !=(TypeSlimTuple3? left, TypeSlimTuple3? right) => !TypeSlimTuple3.Equals(left, right);

    // ReSharper restore ArrangeStaticMemberQualifier
}
