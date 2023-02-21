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
public sealed class FieldSlim : ComparableClass<FieldSlim, FieldSlim.Comparer>, ISlimValueGetter, ISlimValueSetter
{
    public string Name { get; }
    public string NameClassFull { get; }
    public string NameClass { get; }
    public TypeSlim TypeDeclaring { get; }
    public TypeSlim Type { get; }
    public FieldInfo Info { get; }

    public bool IsStatic { get; }
    public bool IsSettable { get; }
    private readonly Lzy<Func<object?, object?>> getMethodCompiled;
    private readonly Lzy<Action<object?, object?>> setMethodCompiled;

    public IEnumerable<Attribute> Attributes => Info.GetCustomAttributes();

    public FieldSlim(FieldInfo info) : base(Comparer.Instance)
    {
        Info = info.CheckNotNull(nameof(info));
        TypeDeclaring = info.DeclaringType.CheckNotNull(nameof(info) + "." + nameof(info.DeclaringType));
        Type = info.FieldType;
        Name = info.Name;
        NameClassFull = string.IsNullOrWhiteSpace(TypeDeclaring.NameFull) ? Name : TypeDeclaring.NameFull + "." + Name;
        NameClass = string.IsNullOrWhiteSpace(TypeDeclaring.Name) ? Name : TypeDeclaring.Name + "." + Name;

        IsStatic = info.IsStatic;
        IsSettable = info.IsSettable();
        getMethodCompiled = Lzy.Create(() => Info.CreateFieldGetter());
        setMethodCompiled = Lzy.Create(() => Info.CreateFieldSetter());
    }

    // ReSharper disable RedundantOverriddenMember
    public override bool Equals(object? obj) => base.Equals(obj);
    public override int GetHashCode() => base.GetHashCode();
    // ReSharper restore RedundantOverriddenMember
    public override string ToString() => NameClassFull;

    public static bool operator ==(FieldSlim? left, FieldSlim? right) => PropertySlim.Equals(left, right);
    public static bool operator !=(FieldSlim? left, FieldSlim? right) => !PropertySlim.Equals(left, right);

    public static implicit operator FieldInfo(FieldSlim obj) => obj.Info;
    public static implicit operator FieldSlim(FieldInfo obj) => new(obj);

    public sealed class Comparer : ComparerBaseClass<FieldSlim>
    {
        public static Comparer Instance { get; } = new();

        protected override bool EqualsInternal(FieldSlim x, FieldSlim y) =>
            EqualsStruct(x.GetHashCode(), y.GetHashCode())
            && EqualsClass(x.TypeDeclaring, y.TypeDeclaring)
            && EqualsOrdinal(x.Name, y.Name)
            && EqualsClass(x.Type, y.Type);

        protected override int GetHashCodeInternal(FieldSlim obj) => Hash(
            obj.TypeDeclaring,
            HashOrdinal(obj.Name),
            obj.Type
        );

        protected override int CompareInternal(FieldSlim x, FieldSlim y) =>
            CompareClass(x.TypeDeclaring, y.TypeDeclaring)
            ?? CompareOrdinalIgnoreCaseThenOrdinal(x.Name, y.Name)
            ?? CompareClass(x.Type, y.Type)
            ?? 0;
    }

    public object? GetValue(object? instance) => getMethodCompiled.Value(instance);

    public void SetValue(object? instance, object? value) => setMethodCompiled.Value(instance, value);
}

public static class FieldSlimExtensions
{
    public static FieldInfo ToFieldInfo(this FieldSlim obj) => obj;
    public static FieldSlim ToPropertySlim(this FieldInfo obj) => obj;

    public static ImmutableArray<FieldSlim> GetFieldSlims(this TypeSlim type, BindingFlags flags) =>
        type.Type.GetFieldSlims(flags);

    public static ImmutableArray<FieldSlim> GetFieldSlims(this Type type, BindingFlags flags) =>
        type.GetFields(flags).Select(o => new FieldSlim(o)).ToImmutableArray();

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

        foreach (var f in flagsList)
        {
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
