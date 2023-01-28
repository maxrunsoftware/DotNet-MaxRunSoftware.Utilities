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
public sealed class FieldSlim :
    IEquatable<FieldSlim>, IEquatable<FieldInfo>,
    IComparable, IComparable<FieldSlim>, IComparable<FieldInfo>
{
    public string Name { get; }
    public string NameClassFull { get; }
    public string NameClass { get; }
    public TypeSlim TypeDeclaring { get; }
    public TypeSlim Type { get; }
    public FieldInfo Info { get; }
    private readonly int getHashCode;

    public bool IsStatic { get; }
    public bool IsSettable { get; }
    private readonly Lzy<Func<object?, object?>> getMethodCompiled;
    private readonly Lzy<Action<object?, object?>> setMethodCompiled;

    public IEnumerable<Attribute> Attributes => Info.GetCustomAttributes();

    public FieldSlim(FieldInfo info)
    {
        Info = info.CheckNotNull(nameof(info));
        TypeDeclaring = info.DeclaringType.CheckNotNull(nameof(info) + "." + nameof(info.DeclaringType));
        Type = info.FieldType;
        Name = info.Name;
        NameClassFull = string.IsNullOrWhiteSpace(TypeDeclaring.NameFull) ? Name : (TypeDeclaring.NameFull + "." + Name);
        NameClass = string.IsNullOrWhiteSpace(TypeDeclaring.Name) ? Name : (TypeDeclaring.Name + "." + Name);
        getHashCode = Util.Hash(TypeDeclaring.GetHashCode(), StringComparer.Ordinal.GetHashCode(info.Name));

        IsStatic = info.IsStatic;
        IsSettable = info.IsSettable();
        getMethodCompiled = Lzy.Create(() => Info.CreateFieldGetter());
        setMethodCompiled = Lzy.Create(() => Info.CreateFieldSetter());
    }

    #region Override

    public override int GetHashCode() => getHashCode;
    public override string ToString() => NameClassFull;

    #region Equals

    public override bool Equals(object? obj) => obj switch
    {
        null => false,
        FieldSlim slim => Equals(slim),
        FieldInfo other => Equals(other),
        _ => false
    };

    public bool Equals(FieldInfo? other)
    {
        if (ReferenceEquals(other, null)) return false;
        if (ReferenceEquals(Info, other)) return true;
        return Equals(new FieldSlim(other));
    }

    public bool Equals(FieldSlim? other)
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
        FieldSlim slim => CompareTo(slim),
        FieldInfo other => CompareTo(other),
        _ => 1
    };

    public int CompareTo(FieldInfo? other)
    {
        if (ReferenceEquals(other, null)) return 1;
        if (ReferenceEquals(Info, other)) return 0;
        return CompareTo(new FieldSlim(other));
    }

    public int CompareTo(FieldSlim? other)
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

    private static bool Equals(FieldSlim? left, FieldSlim? right) => left?.Equals(right) ?? ReferenceEquals(right, null);

    // ReSharper disable ArrangeStaticMemberQualifier

    public static bool operator ==(FieldSlim? left, FieldSlim? right) => PropertySlim.Equals(left, right);
    public static bool operator !=(FieldSlim? left, FieldSlim? right) => !PropertySlim.Equals(left, right);

    // ReSharper restore ArrangeStaticMemberQualifier

    public static implicit operator FieldInfo(FieldSlim obj) => obj.Info;
    public static implicit operator FieldSlim(FieldInfo obj) => new(obj);

    #endregion Implicit / Explicit

    #region Extras

    public object? GetValue(object? instance) => getMethodCompiled.Value(instance);

    public void SetValue(object? instance, object? value) => setMethodCompiled.Value(instance, value);

    #endregion Extras
}


public static class FieldSlimExtensions
{
    public static FieldInfo ToFieldInfo(this FieldSlim obj) => obj;
    public static FieldSlim ToPropertySlim(this FieldInfo obj) => obj;

        public static FieldSlim[] GetFieldSlims(this TypeSlim type, BindingFlags flags) =>
            type.Type.GetFieldSlims(flags);

        public static FieldSlim[] GetFieldSlims(this Type type, BindingFlags flags) =>
            type.GetFields(flags).Select(o => new FieldSlim(o)).ToArray();

        public static FieldSlim? GetFieldSlim(this TypeSlim type, string name, BindingFlags? flags = null) =>
            type.Type.GetFieldSlim(name, flags);

        public static FieldSlim? GetFieldSlim(this Type type, string name, BindingFlags? flags = null)
        {
            var flagsList = flags != null
                ? new[] { flags.Value }
                : new[]
                {
                    BindingFlags.Public | BindingFlags.Instance,
                    BindingFlags.Public | BindingFlags.Static,
                    BindingFlags.NonPublic | BindingFlags.Instance,
                    BindingFlags.NonPublic | BindingFlags.Static,
                };

            foreach(var f in flagsList) {
                var items = GetFieldSlims(type, f);
                foreach (var sc in Constant.StringComparers)
                {
                    var matches = items.Where(prop => sc.Equals(prop.Name, name)).ToList();
                    if (matches.Count == 1) return matches[0];
                    if (matches.Count > 1) throw new AmbiguousMatchException($"Found {matches.Count} fields on {type.FullNameFormatted()} with name {name}: " + matches.Select(o => o.Name).ToStringDelimited(", "));
                }
            }

            return null;
        }
}
