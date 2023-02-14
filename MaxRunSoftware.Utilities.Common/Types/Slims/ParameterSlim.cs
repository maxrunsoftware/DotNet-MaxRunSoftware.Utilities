// Copyright (c) 2023 Max Run Software (dev@maxrunsoftware.com)
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
public sealed class ParameterSlim :
    IEquatable<ParameterSlim>, IEquatable<ParameterInfo>,
    IComparable, IComparable<ParameterSlim>, IComparable<ParameterInfo>
{
    public ParameterInfo Info { get; }
    public int Position { get; }
    public string? Name { get; }
    public TypeSlim Type { get; }

    public bool IsIn { get; }
    public bool IsOut { get; }
    public bool IsRef { get; }

    private readonly int getHashCode;

    public IEnumerable<Attribute> Attributes => Info.GetCustomAttributes();

    public ParameterSlim(ParameterInfo info)
    {
        Info = info;
        Name = info.Name;
        Type = info.ParameterType;
        Position = info.Position;
        IsIn = info.IsIn();
        IsOut = info.IsOut();
        IsRef = info.IsRef();

        getHashCode = Util.Hash(
            Position,
            Type,
            // ReSharper disable once RedundantCast
            Name == null ? null : (int?)StringComparer.Ordinal.GetHashCode(Name),
            IsIn, IsOut, IsRef
        );
    }

    #region Override

    public override int GetHashCode() => getHashCode;
    public override string ToString() => Name ?? $"arg{Position}";

    #region Equals

    public static bool Equals(ParameterSlim? left, ParameterSlim? right)
    {
        if (ReferenceEquals(left, null)) return ReferenceEquals(right, null);
        if (ReferenceEquals(right, null)) return false;

        if (left.getHashCode != right.getHashCode) return false;
        if (!left.Position.Equals(right.Position)) return false;
        if (!left.Type.Equals(right.Type)) return false;
        if (!StringComparer.Ordinal.Equals(left.Name, right.Name)) return false;
        if (left.IsIn != right.IsIn) return false;
        if (left.IsOut != right.IsOut) return false;
        if (left.IsRef != right.IsRef) return false;

        return true;
    }


    public override bool Equals(object? obj) => obj switch
    {
        null => false,
        ParameterSlim slim => Equals(slim),
        ParameterInfo other => Equals(other),
        _ => false,
    };

    public bool Equals(ParameterInfo? other)
    {
        if (ReferenceEquals(other, null)) return false;
        if (ReferenceEquals(Info, other)) return true;
        return Equals(new ParameterSlim(other));
    }

    public bool Equals(ParameterSlim? other) => Equals(this, other);

    #endregion Equals

    #region CompareTo

    public int CompareTo(object? obj) => obj switch
    {
        null => 1,
        ParameterSlim slim => CompareTo(slim),
        ParameterInfo other => CompareTo(other),
        _ => 1,
    };

    public int CompareTo(ParameterInfo? other)
    {
        if (ReferenceEquals(other, null)) return 1;
        if (ReferenceEquals(Info, other)) return 0;
        return CompareTo(new ParameterSlim(other));
    }

    public int CompareTo(ParameterSlim? other)
    {
        if (ReferenceEquals(other, null)) return 1;
        if (ReferenceEquals(this, other)) return 0;

        int c;
        if (0 != (c = Position.CompareTo(other.Position))) return c;
        if (0 != (c = Type.CompareTo(other.Type))) return c;
        if (0 != (c = Constant.StringComparer_OrdinalIgnoreCase_Ordinal.Compare(Name, other.Name))) return c;
        if (0 != (c = IsIn.CompareTo(other.IsIn))) return c;
        if (0 != (c = IsOut.CompareTo(other.IsOut))) return c;
        if (0 != (c = IsRef.CompareTo(other.IsRef))) return c;
        return c;
    }

    #endregion CompareTo

    #endregion Override

    #region Implicit / Explicit

    // ReSharper disable ArrangeStaticMemberQualifier

    public static bool operator ==(ParameterSlim? left, ParameterSlim? right) => Equals(left, right);
    public static bool operator !=(ParameterSlim? left, ParameterSlim? right) => !Equals(left, right);

    // ReSharper restore ArrangeStaticMemberQualifier

    public static implicit operator ParameterInfo(ParameterSlim slim) => slim.Info;
    public static implicit operator ParameterSlim(ParameterInfo info) => new(info);

    #endregion Implicit / Explicit
}

public class ParameterSlimTypeComparer : IEqualityComparer<ParameterSlim>
{
    public static readonly ParameterSlimTypeComparer INSTANCE = new();

    public bool Equals(ParameterSlim? x, ParameterSlim? y)
    {
        if (ReferenceEquals(x, null)) return ReferenceEquals(y, null);
        if (ReferenceEquals(y, null)) return false;
        return TypeSlim.Equals(x.Type, y.Type);
    }
    public int GetHashCode(ParameterSlim obj) => obj.Type.GetHashCode();
}
