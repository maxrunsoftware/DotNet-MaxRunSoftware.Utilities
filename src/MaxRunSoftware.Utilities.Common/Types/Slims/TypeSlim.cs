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
public sealed class TypeSlim : ComparableClass<TypeSlim, TypeSlim.Comparer>
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

    public TypeSlim(Type type) : base(Comparer.Instance)
    {
        Type = type.CheckNotNull(nameof(type));
        Info = Type.GetTypeInfo();
        Assembly = new(type.Assembly);
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


    public override string ToString() => NameFull;
    // ReSharper disable RedundantOverriddenMember
    public override bool Equals(object? obj) => base.Equals(obj);
    public override int GetHashCode() => base.GetHashCode();
    // ReSharper restore RedundantOverriddenMember

    public static bool operator ==(TypeSlim? left, TypeSlim? right) => Comparer.Instance.Equals(left, right);
    public static bool operator !=(TypeSlim? left, TypeSlim? right) => !Comparer.Instance.Equals(left, right);

    public static implicit operator Type(TypeSlim obj) => obj.Type;
    public static implicit operator TypeSlim(Type obj) => new(obj);

    public static implicit operator TypeInfo(TypeSlim typeSlim) => typeSlim.Info;
    public static implicit operator TypeSlim(TypeInfo typeInfo) => new(typeInfo);

    public sealed class Comparer : ComparerBaseClass<TypeSlim>
    {
        public static Comparer Instance { get; } = new();
        protected override bool EqualsInternal(TypeSlim x, TypeSlim y) =>
            EqualsStruct(x.GetHashCode(), y.GetHashCode())
            && EqualsStruct(x.TypeHashCode, y.TypeHashCode)
            && EqualsClass(x.Assembly, y.Assembly)
            && EqualsOrdinal(x.NameFull, y.NameFull)
            && x.Type == y.Type;

        protected override int GetHashCodeInternal(TypeSlim obj) => Hash(
            obj.TypeHashCode,
            obj.Assembly.GetHashCode(),
            HashOrdinal(obj.NameFull)
        );

        protected override int CompareInternal(TypeSlim x, TypeSlim y) =>
            CompareClass(x.Assembly, y.Assembly)
            ?? CompareOrdinalIgnoreCaseThenOrdinal(x.NameFull, y.NameFull)
            ?? CompareStruct(x.TypeHashCode, y.TypeHashCode)
            ?? CompareStruct(x.GetHashCode(), y.GetHashCode())
            ?? 0;
    }
}

public static class TypeSlimExtensions
{
    public static Type ToType(this TypeSlim obj) => obj;
    public static TypeSlim ToTypeSlim(this Type obj) => obj;
    
    public static TypeInfo ToTypeInfo(this TypeSlim obj) => obj;
    public static TypeSlim ToTypeSlim(this TypeInfo obj) => obj;
}
