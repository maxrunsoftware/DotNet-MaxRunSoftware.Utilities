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

public sealed class TupleTypeSlimTypeSlim : ComparableClass<TupleTypeSlimTypeSlim, TupleTypeSlimTypeSlim.Comparer>
{
    public TypeSlim Item1 { get; }
    public TypeSlim Item2 { get; }

    public TupleTypeSlimTypeSlim(TypeSlim item1, TypeSlim item2) : base(Comparer.Instance)
    {
        Item1 = item1;
        Item2 = item2;
    }

    public override string ToString() => GetType().NameFormatted() + $"({Item1}, {Item2})";

    public sealed class Comparer : ComparerBaseClass<TupleTypeSlimTypeSlim>
    {
        public static Comparer Instance { get; } = new();
        protected override bool EqualsInternal(TupleTypeSlimTypeSlim x, TupleTypeSlimTypeSlim y) => EqualsStruct(x.GetHashCode(), y.GetHashCode()) && EqualsClass(x.Item1, y.Item1) && EqualsClass(x.Item2, y.Item2);
        protected override int GetHashCodeInternal(TupleTypeSlimTypeSlim obj) => Hash(obj.Item1, obj.Item2);
        protected override int CompareInternal(TupleTypeSlimTypeSlim x, TupleTypeSlimTypeSlim y) => CompareClass(x.Item1, y.Item1) ?? CompareClass(x.Item2, y.Item2) ?? 0;
    }
}

public sealed class TupleTypeSlimBindingFlags : ComparableClass<TupleTypeSlimBindingFlags, TupleTypeSlimBindingFlags.Comparer>
{
    public TypeSlim Item1 { get; }
    public BindingFlags Item2 { get; }

    public TupleTypeSlimBindingFlags(TypeSlim item1, BindingFlags item2) : base(Comparer.Instance)
    {
        Item1 = item1;
        Item2 = item2;
    }

    public override string ToString() => GetType().NameFormatted() + $"({Item1}, {Item2})";

    public sealed class Comparer : ComparerBaseClass<TupleTypeSlimBindingFlags>
    {
        public static Comparer Instance { get; } = new();
        protected override bool EqualsInternal(TupleTypeSlimBindingFlags x, TupleTypeSlimBindingFlags y) => EqualsStruct(x.GetHashCode(), y.GetHashCode()) && EqualsClass(x.Item1, y.Item1) && EqualsStruct(x.Item2, y.Item2);
        protected override int GetHashCodeInternal(TupleTypeSlimBindingFlags obj) => Hash(obj.Item1, obj.Item2);
        protected override int CompareInternal(TupleTypeSlimBindingFlags x, TupleTypeSlimBindingFlags y) => CompareClass(x.Item1, y.Item1) ?? CompareEnumEnumerable(x.Item2, y.Item2) ?? 0;
    }
}
