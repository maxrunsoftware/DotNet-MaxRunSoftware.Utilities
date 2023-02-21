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
public sealed class ParameterSlim : ComparableClass<ParameterSlim, ParameterSlim.Comparer>
{
    public ParameterInfo Info { get; }
    public int Position { get; }
    public string? Name { get; }
    public TypeSlim Type { get; }

    public bool IsIn { get; }
    public bool IsOut { get; }
    public bool IsRef { get; }

    public IEnumerable<Attribute> Attributes => Info.GetCustomAttributes();

    public ParameterSlim(ParameterInfo info) : base(Comparer.Instance)
    {
        Info = info;
        Name = info.Name;
        Type = info.ParameterType;
        Position = info.Position;
        IsIn = info.IsIn();
        IsOut = info.IsOut();
        IsRef = info.IsRef();
    }

    // ReSharper disable RedundantOverriddenMember
    public override bool Equals(object? obj) => base.Equals(obj);
    public override int GetHashCode() => base.GetHashCode();
    // ReSharper restore RedundantOverriddenMember
    public override string ToString() => Name ?? $"arg{Position}";

    public static bool operator ==(ParameterSlim? left, ParameterSlim? right) => Comparer.Instance.Equals(left, right);
    public static bool operator !=(ParameterSlim? left, ParameterSlim? right) => !Comparer.Instance.Equals(left, right);
    public static implicit operator ParameterInfo(ParameterSlim slim) => slim.Info;
    public static implicit operator ParameterSlim(ParameterInfo info) => new(info);

    public sealed class Comparer : ComparerBaseClass<ParameterSlim>
    {
        public static Comparer Instance { get; } = new();
        protected override bool EqualsInternal(ParameterSlim x, ParameterSlim y) =>
            EqualsStruct(x.GetHashCode(), y.GetHashCode())
            && EqualsStruct(x.Position, y.Position)
            && EqualsClass(x.Type, y.Type)
            && EqualsOrdinal(x.Name, y.Name)
            && EqualsStruct(x.IsIn, y.IsIn)
            && EqualsStruct(x.IsOut, y.IsOut)
            && EqualsStruct(x.IsRef, y.IsRef);

        protected override int GetHashCodeInternal(ParameterSlim obj) => Hash(
            obj.Position,
            obj.Type,
            HashOrdinal(obj.Name),
            obj.IsIn, obj.IsOut, obj.IsRef
        );

        protected override int CompareInternal(ParameterSlim x, ParameterSlim y) =>
            CompareStruct(x.Position, y.Position)
            ?? CompareClass(x.Type, y.Type)
            ?? CompareOrdinalIgnoreCaseThenOrdinal(x.Name, y.Name)
            ?? CompareStruct(x.IsIn, y.IsIn)
            ?? CompareStruct(x.IsOut, y.IsOut)
            ?? CompareStruct(x.IsRef, y.IsRef)
            ?? 0;
    }
}
