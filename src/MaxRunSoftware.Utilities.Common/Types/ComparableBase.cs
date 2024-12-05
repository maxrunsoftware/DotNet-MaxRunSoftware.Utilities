// Copyright (c) 2024 Max Run Software (dev@maxrunsoftware.com)
// 
// Licensed under the Apache License, Version 2.0 (the "License")
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

public abstract class ComparableBase<T, TComparer>(TComparer comparer) : IEquatable<T>, IComparable<T>, IComparable
    where T : ComparableBase<T, TComparer>
    where TComparer : IEqualityComparer<T>, IEqualityComparer, IComparer<T>, IComparer
{
    protected T CacheHashCode()
    {
        getHashCode = GetHashCode_Calc();
        getHashCodeLazy = null;
        return (T)this;
    }
    
    protected T CacheHashCodeLazy()
    {
        getHashCode = null;
        getHashCodeLazy = Lzy.Create(GetHashCode_Calc);
        return (T)this;
    }

    private Lzy<int>? getHashCodeLazy;
    private int? getHashCode;

    public TComparer Comparer { get; set; } = comparer;

    public override int GetHashCode()
    {
        // ReSharper disable NonReadonlyMemberInGetHashCode
        if (getHashCode != null) return getHashCode.Value;
        if (getHashCodeLazy != null) return getHashCodeLazy.Value;
        // ReSharper restore NonReadonlyMemberInGetHashCode
        return GetHashCode_Calc();
    }
    private int GetHashCode_Calc() => Comparer.GetHashCode(this);

    public override bool Equals([NotNullWhen(true)] object? obj) => Comparer.Equals(this, obj);
    public bool Equals([NotNullWhen(true)] T? other) => Comparer.Equals(this, other);
    public int CompareTo(object? obj) => Comparer.Compare(this, obj);
    public int CompareTo(T? other) => Comparer.Compare(this, other);
}
