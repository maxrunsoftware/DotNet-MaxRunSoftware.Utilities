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
public sealed class PropertySlim : IEquatable<PropertySlim>, IComparable<PropertySlim>, IComparable, IEquatable<PropertyInfo>, IComparable<PropertyInfo>
{
    public string Name { get; }
    public string NameFull { get; }
    public TypeSlim TypeDeclaring { get; }
    public TypeSlim Type { get; }
    public PropertyInfo Info { get; }
    private readonly int getHashCode;

    private readonly Lzy<bool> isStatic;
    public bool IsStatic => isStatic.Value;

    private readonly Lzy<ExpensiveDataGetSet> expensiveDataGetSet;
    public bool CanGetPublic => expensiveDataGetSet.Value.canGetPublic;
    public bool CanGetNonPublic => expensiveDataGetSet.Value.canGetNonPublic;
    public MethodInfo? GetMethod => expensiveDataGetSet.Value.getMethod;
    public bool CanSetPublic => expensiveDataGetSet.Value.canSetPublic;
    public bool CanSetNonPublic => expensiveDataGetSet.Value.canSetNonPublic;
    public MethodInfo? SetMethod => expensiveDataGetSet.Value.setMethod;

    private readonly Lzy<Func<object?, object?>> getMethodCompiled;
    private readonly Lzy<Action<object?, object?>> setMethodCompiled;

    public IEnumerable<Attribute> Attributes => Info.GetCustomAttributes();

    public PropertySlim(PropertyInfo info)
    {
        Info = info.CheckNotNull(nameof(info));
        TypeDeclaring = info.DeclaringType.CheckNotNull(nameof(info) + "." + nameof(info.DeclaringType));
        Type = info.PropertyType;
        Name = info.Name;
        NameFull = string.IsNullOrWhiteSpace(TypeDeclaring.NameFull) ? Name : (TypeDeclaring.NameFull + "." + Name);
        getHashCode = Util.Hash(TypeDeclaring.GetHashCode(), StringComparer.Ordinal.GetHashCode(info.Name));

        isStatic = Lzy.Create(() => Info.IsStatic());
        expensiveDataGetSet = Lzy.Create(() => new ExpensiveDataGetSet(Info));
        getMethodCompiled = Lzy.Create(() => Info.CreatePropertyGetter());
        setMethodCompiled = Lzy.Create(() => Info.CreatePropertySetter());
    }

    private sealed class ExpensiveDataGetSet
    {
        public readonly bool canGetPublic;
        public readonly bool canGetNonPublic;
        public readonly MethodInfo? getMethod;

        public readonly bool canSetPublic;
        public readonly bool canSetNonPublic;
        public readonly MethodInfo? setMethod;

        public ExpensiveDataGetSet(PropertyInfo info)
        {
            // https://stackoverflow.com/a/302492
            var getPublic = info.GetGetMethod(false);
            var getNonPublic = getPublic ?? info.GetGetMethod(true);
            canGetPublic = getPublic != null;
            canGetNonPublic = getNonPublic != null;
            getMethod = getNonPublic;

            var setPublic = info.GetSetMethod(false);
            var setNonPublic = setPublic ?? info.GetSetMethod(true);
            canSetPublic = setPublic != null;
            canSetNonPublic = setNonPublic != null;
            setMethod = setNonPublic;
        }
    }

    #region Override

    public override int GetHashCode() => getHashCode;
    public override string ToString() => NameFull;

    #region Equals

    public override bool Equals(object? obj) => obj switch
    {
        null => false,
        PropertySlim slim => Equals(slim),
        PropertyInfo other => Equals(other),
        _ => false
    };

    public bool Equals(PropertyInfo? other)
    {
        if (ReferenceEquals(other, null)) return false;
        if (ReferenceEquals(Info, other)) return true;
        return Equals(new PropertySlim(other));
    }

    public bool Equals(PropertySlim? other)
    {
        if (ReferenceEquals(other, null)) return false;
        if (ReferenceEquals(this, other)) return true;

        if (getHashCode != other.getHashCode) return false;
        if (!TypeDeclaring.Equals(other.TypeDeclaring)) return false;
        if (!StringComparer.Ordinal.Equals(Name, other.Name)) return false;
        if (Type != other.Type) return false;

        return true;
    }

    #endregion Equals

    #region CompareTo

    public int CompareTo(object? obj) => obj switch
    {
        null => 1,
        PropertySlim slim => CompareTo(slim),
        PropertyInfo other => CompareTo(other),
        _ => 1
    };

    public int CompareTo(PropertyInfo? other)
    {
        if (ReferenceEquals(other, null)) return 1;
        if (ReferenceEquals(Info, other)) return 0;
        return CompareTo(new PropertySlim(other));
    }

    public int CompareTo(PropertySlim? other)
    {
        if (ReferenceEquals(other, null)) return 1;
        if (ReferenceEquals(this, other)) return 0;

        int c;
        if (0 != (c = TypeDeclaring.CompareTo(other.TypeDeclaring))) return c;
        if (0 != (c = Constant.StringComparer_OrdinalIgnoreCase_Ordinal.Compare(Name, other.Name))) return c;
        if (0 != (c = Type.CompareTo(other.Type))) return c;
        if (0 != (c = getHashCode.CompareTo(other.getHashCode))) return c;
        return c;
    }



    #endregion CompareTo

    #endregion Override

    #region Implicit / Explicit

    private static bool Equals(PropertySlim? left, PropertySlim? right) => left?.Equals(right) ?? ReferenceEquals(right, null);

    // ReSharper disable ArrangeStaticMemberQualifier

    public static bool operator ==(PropertySlim? left, PropertySlim? right) => PropertySlim.Equals(left, right);
    public static bool operator !=(PropertySlim? left, PropertySlim? right) => !PropertySlim.Equals(left, right);

    // ReSharper restore ArrangeStaticMemberQualifier

    public static implicit operator PropertyInfo(PropertySlim obj) => obj.Info;
    public static implicit operator PropertySlim(PropertyInfo obj) => new(obj);

    #endregion Implicit / Explicit

    #region Extras

    public object? Get(object instance) => getMethodCompiled.Value(instance);

    public object? GetStatic() => getMethodCompiled.Value(null);

    public void Set(object instance, object? value) => setMethodCompiled.Value(instance, value);

    public void SetStatic(object? value) => setMethodCompiled.Value(null, value);

    #endregion Extras
}
