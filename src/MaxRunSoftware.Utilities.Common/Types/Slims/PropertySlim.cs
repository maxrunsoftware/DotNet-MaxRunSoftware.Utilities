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

namespace MaxRunSoftware.Utilities.Common;

[PublicAPI]
public sealed class PropertySlim : ComparableClass<PropertySlim, PropertySlim.Comparer>, ISlimValueGetter, ISlimValueSetter
{
    public string Name { get; }
    public string NameClassFull { get; }
    public string NameClass { get; }
    public TypeSlim TypeDeclaring { get; }
    public TypeSlim Type { get; }
    public PropertyInfo Info { get; }

    private readonly Lzy<bool> isStatic;
    public bool IsStatic => isStatic.Value;

    private readonly Lzy<ExpensiveDataGetSet> expensiveDataGetSet;
    public bool IsGettablePublic => expensiveDataGetSet.Value.canGetPublic;
    public bool IsGettableNonPublic => expensiveDataGetSet.Value.canGetNonPublic;
    public MethodInfo? GetMethod => expensiveDataGetSet.Value.getMethod;
    public bool IsSettablePublic => expensiveDataGetSet.Value.canSetPublic;
    public bool IsSettableNonPublic => expensiveDataGetSet.Value.canSetNonPublic;
    public MethodInfo? SetMethod => expensiveDataGetSet.Value.setMethod;

    private readonly Lzy<Func<object?, object?>> getMethodCompiled;
    private readonly Lzy<Action<object?, object?>> setMethodCompiled;

    public IEnumerable<Attribute> Attributes => Info.GetCustomAttributes();

    public PropertySlim(PropertyInfo info) : base(Comparer.Instance)
    {
        Info = info.CheckNotNull(nameof(info));
        TypeDeclaring = info.DeclaringType.CheckNotNull(nameof(info) + "." + nameof(info.DeclaringType));
        Type = info.PropertyType;
        Name = info.Name;
        NameClassFull = string.IsNullOrWhiteSpace(TypeDeclaring.NameFull) ? Name : TypeDeclaring.NameFull + "." + Name;
        NameClass = string.IsNullOrWhiteSpace(TypeDeclaring.Name) ? Name : TypeDeclaring.Name + "." + Name;

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

    // ReSharper disable RedundantOverriddenMember
    public override bool Equals(object? obj) => base.Equals(obj);
    public override int GetHashCode() => base.GetHashCode();
    // ReSharper restore RedundantOverriddenMember
    public override string ToString() => NameClassFull;

    public static bool operator ==(PropertySlim? left, PropertySlim? right) => Comparer.Instance.Equals(left, right);
    public static bool operator !=(PropertySlim? left, PropertySlim? right) => !Comparer.Instance.Equals(left, right);

    public static implicit operator PropertyInfo(PropertySlim obj) => obj.Info;
    public static implicit operator PropertySlim(PropertyInfo obj) => new(obj);

    public sealed class Comparer : ComparerBaseClass<PropertySlim>
    {
        public static Comparer Instance { get; } = new();
        protected override bool EqualsInternal(PropertySlim x, PropertySlim y) =>
            EqualsStruct(x.GetHashCode(), y.GetHashCode())
            && EqualsClass(x.TypeDeclaring, y.TypeDeclaring)
            && EqualsOrdinal(x.Name, y.Name)
            && EqualsClass(x.Type, y.Type);

        protected override int GetHashCodeInternal(PropertySlim obj) => Hash(
            obj.TypeDeclaring,
            HashOrdinal(obj.Name),
            obj.Type
        );

        protected override int CompareInternal(PropertySlim x, PropertySlim y) =>
            CompareClass(x.TypeDeclaring, y.TypeDeclaring)
            ?? CompareOrdinalIgnoreCaseThenOrdinal(x.Name, y.Name)
            ?? CompareClass(x.Type, y.Type)
            ?? 0;
    }

    public object? GetValue(object? instance) => getMethodCompiled.Value(instance);

    public void SetValue(object? instance, object? value) => setMethodCompiled.Value(instance, value);
}
