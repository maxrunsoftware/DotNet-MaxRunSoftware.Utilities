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

namespace MaxRunSoftware.Utilities.Common;

[PublicAPI]
public sealed class TypeSlim : IEquatable<TypeSlim>, IComparable<TypeSlim>, IComparable, IEquatable<Type>, IComparable<Type>
{
    public string NameFull { get; }
    public Type Type { get; }
    public TypeInfo TypeInfo { get; }
    public AssemblySlim Assembly { get; }
    public int TypeHashCode { get; }
    private readonly int getHashCode;

    public TypeSlim(Type type)
    {
        Type = type.CheckNotNull(nameof(type));
        TypeInfo = Type.GetTypeInfo();
        Assembly = new AssemblySlim(type.Assembly);
        NameFull = NameFull_Build(type);
        TypeHashCode = Type.GetHashCode();
        getHashCode = Util.Hash(TypeHashCode, Assembly.GetHashCode(), StringComparer.Ordinal.GetHashCode(NameFull));
    }

    #region Override

    public override int GetHashCode() => getHashCode;
    public override string ToString() => NameFull;

    #region Equals

    public override bool Equals(object? obj) => obj switch
    {
        null => false,
        TypeSlim slim => Equals(slim),
        Type other => Equals(other),
        _ => false
    };

    public bool Equals(Type? other)
    {
        if (ReferenceEquals(other, null)) return false;
        if (ReferenceEquals(Type, other)) return true;
        return Equals(new TypeSlim(other));
    }

    public bool Equals(TypeSlim? other)
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
        _ => 1
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

    #region Static

    private static string NameFull_Build(Type type)
    {
        var name = type.FullName;
        if (!string.IsNullOrWhiteSpace(name))
        {
            name = type.FullNameFormatted().TrimOrNull();
            if (name != null) return name;
        }

        name = type.NameFormatted().TrimOrNull();
        if (name != null) return name;

        name = type.Name.TrimOrNull();
        if (name != null) return name;

        name = type.ToString().TrimOrNull();
        if (name != null) return name;

        return type.Name;
    }

    #endregion Static

    #region Implicit / Explicit

    private static bool Equals(TypeSlim? left, TypeSlim? right) => left?.Equals(right) ?? ReferenceEquals(right, null);
    private static bool Equals(TypeSlim? left, Type? right) => left?.Equals(right) ?? ReferenceEquals(right, null);

    // ReSharper disable ArrangeStaticMemberQualifier

    public static bool operator ==(TypeSlim? left, TypeSlim? right) => TypeSlim.Equals(left, right);
    public static bool operator !=(TypeSlim? left, TypeSlim? right) => !TypeSlim.Equals(left, right);

    public static bool operator ==(TypeSlim? left, Type? right) => TypeSlim.Equals(left, right);
    public static bool operator !=(TypeSlim? left, Type? right) => !TypeSlim.Equals(left, right);

    public static bool operator ==(Type? left, TypeSlim? right) => TypeSlim.Equals(right, left);
    public static bool operator !=(Type? left, TypeSlim? right) => !TypeSlim.Equals(right, left);

    // ReSharper restore ArrangeStaticMemberQualifier

    public static implicit operator Type(TypeSlim typeSlim) => typeSlim.Type;
    public static implicit operator TypeSlim(Type type) => new(type);

    public static implicit operator TypeInfo(TypeSlim typeSlim) => typeSlim.TypeInfo;
    public static implicit operator TypeSlim(TypeInfo typeInfo) => new(typeInfo);

    #endregion Implicit / Explicit
}
