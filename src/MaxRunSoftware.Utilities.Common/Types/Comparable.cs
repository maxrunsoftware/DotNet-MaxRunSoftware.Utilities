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

using System.Diagnostics.CodeAnalysis;

namespace MaxRunSoftware.Utilities.Common;

public abstract class ComparableClass<T, TComparer> : IEquatable<T>, IComparable<T>, IComparable
    where T : ComparableClass<T, TComparer>
    where TComparer : IEqualityComparer<T>, IEqualityComparer, IComparer<T>, IComparer
{
    private readonly TComparer comparer;
    protected ComparableClass(TComparer comparer, bool lazyHashCode = true)
    {
        this.comparer = comparer;
        if (lazyHashCode)
        {
            getHashCodeLazy = Lzy.Create(() => this.comparer.GetHashCode(this));
        }
        else
        {
            getHashCode = this.comparer.GetHashCode(this);
        }
    }

    private readonly Lzy<int>? getHashCodeLazy;
    private readonly int? getHashCode;
    public override int GetHashCode()
    {
        if (getHashCode != null) return getHashCode.Value;
        if (getHashCodeLazy != null) return getHashCodeLazy.Value;
        return 0;
    }

    public override bool Equals([NotNullWhen(true)] object? obj) => comparer.Equals(this, obj);
    public bool Equals([NotNullWhen(true)] T? other) => comparer.Equals(this, other);
    public int CompareTo(object? obj) => comparer.Compare(this, obj);
    public int CompareTo(T? other) => comparer.Compare(this, other);
}
