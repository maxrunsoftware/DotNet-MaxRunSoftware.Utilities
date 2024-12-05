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

namespace MaxRunSoftware.Utilities.Common;

public interface IComparerHashCodeCached
{
    public int HashCode { get; }
}

public abstract partial class ComparerBase<T> where T : class
{
    



    protected virtual bool Equals_Internal(T x, T y) => throw new NotImplementedException();
    
    protected virtual int GetHashCode_Internal(T obj)
    {
        var h = new HashCode();
        GetHashCode_Internal(obj, ref h);
        return h.ToHashCode();
    }
    
    protected virtual void GetHashCode_Internal(T obj, ref HashCode h) => throw new NotImplementedException();
    
    protected virtual int Compare_Internal(T x, T y) => Compare_Internal_Comparisons(x, y).FirstOrDefault(i => i != 0);

    protected virtual IEnumerable<int> Compare_Internal_Comparisons(T x, T y) => throw new NotImplementedException();
}

public abstract partial class ComparerBase<T> : IEqualityComparer, IComparer
{
    bool IEqualityComparer.Equals(object? x, object? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;

        if (x is T[] xArray && y is T[] yArray) return Equals(xArray, yArray);
        if (x is IReadOnlyList<T?> xReadOnlyList && y is IReadOnlyList<T?> yReadOnlyList) return Equals(xReadOnlyList, yReadOnlyList);
        if (x is IEnumerable<T?> xEnumerable && y is IEnumerable<T?> yEnumerable) return Equals(xEnumerable, yEnumerable);
        if (x is T xx && y is T yy) return Equals(xx, yy);

        return x.Equals(y);
    }
    
    int IEqualityComparer.GetHashCode(object? obj)
    {
        if (ReferenceEquals(obj, null)) return 0;
        
        if (obj is T[] objArray) return GetHashCode(objArray);
        if (obj is IReadOnlyList<T?> objReadOnlyList) return GetHashCode(objReadOnlyList);
        if (obj is IEnumerable<T?> objEnumerable) return GetHashCode(objEnumerable);
        if (obj is T xx) return GetHashCode(xx);
        
        return 0;
    }

    int IComparer.Compare(object? x, object? y)
    {
        if (ReferenceEquals(x, y)) return 0;
        if (ReferenceEquals(x, null)) return -1;
        if (ReferenceEquals(y, null)) return 1;
        
        if (x is T[] xArray && y is T[] yArray) return Compare(xArray, yArray);
        if (x is IReadOnlyList<T?> xReadOnlyList && y is IReadOnlyList<T?> yReadOnlyList) return Compare(xReadOnlyList, yReadOnlyList);
        if (x is IEnumerable<T?> xEnumerable && y is IEnumerable<T?> yEnumerable) return Compare(xEnumerable, yEnumerable);

        if (x is not T xx) throw new ArgumentException($"Expecting type [{typeof(T).FullNameFormatted()}] but was instead [{x.GetType().FullNameFormatted()}]: {x}");
        if (y is not T yy) throw new ArgumentException($"Expecting type [{typeof(T).FullNameFormatted()}] but was instead [{y.GetType().FullNameFormatted()}]: {y}");
        
        return Compare(xx, yy);
    }

}

public abstract partial class ComparerBase<T> : IEqualityComparer<T?>, IComparer<T?>
{
    public virtual bool Equals(T? x, T? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;
        
        return Equals_Internal(x, y);
    }
    
    public virtual int GetHashCode(T? obj) => ReferenceEquals(obj, null) ? 0 : GetHashCode_Internal(obj);

    public virtual int Compare(T? x, T? y)
    {
        if (ReferenceEquals(x, y)) return 0;
        if (ReferenceEquals(x, null)) return -1;
        if (ReferenceEquals(y, null)) return 1;

        return Compare_Internal(x, y);
    }
}

public abstract partial class ComparerBase<T> : IEqualityComparer<T?[]?>, IComparer<T?[]?>
{
    private const bool skipCount = false;
    
    public virtual bool Equals(T?[]? x, T?[]? y) 
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;
        
        if (x.Length != y.Length) return false;
        for (var i = 0; i < x.Length; i++)
        {
            if (!Equals(x[i], y[i])) return false;
        }
        return true;
    }

    public virtual int GetHashCode(T?[]? obj) {
        var h = new HashCode();
        if (obj != null)
        {
            for (var i = 0; i < obj.Length; i++)
            {
                h.Add(GetHashCode(obj[i]));
            }
        }

        return h.ToHashCode();
    }

    public virtual int Compare(T?[]? x, T?[]? y) 
    {
        if (ReferenceEquals(x, y)) return 0;
        if (ReferenceEquals(x, null)) return -1;
        if (ReferenceEquals(y, null)) return 1;

        var len = Math.Max(x.Length, y.Length);
        for (var i = 0; i < len; i++)
        {
            var xItem = x.GetAtIndexOrDefault(i);
            var yItem = y.GetAtIndexOrDefault(i);
            var c = Compare(xItem, yItem);
            if (c != 0) return c;
        }

        return 0;
    }
}

public abstract partial class ComparerBase<T> : IEqualityComparer<IReadOnlyList<T?>?>, IComparer<IReadOnlyList<T?>?>
{
    public virtual bool Equals(IReadOnlyList<T?>? x, IReadOnlyList<T?>? y) 
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;

        var xCount = x.Count;
        var yCount = y.Count;
        if (xCount != yCount) return false;
        for (var i = 0; i < xCount; i++)
        {
            if (!Equals(x[i], y[i])) return false;
        }
        return true;
    }

    public virtual int GetHashCode(IReadOnlyList<T?>? obj) {
        var h = new HashCode();
        if (obj != null)
        {
            var count = obj.Count;
            for (var i = 0; i < count; i++)
            {
                h.Add(GetHashCode(obj[i]));
            }
        }

        return h.ToHashCode();
    }

    public virtual int Compare(IReadOnlyList<T?>? x, IReadOnlyList<T?>? y)
    {
        if (ReferenceEquals(x, y)) return 0;
        if (ReferenceEquals(x, null)) return -1;
        if (ReferenceEquals(y, null)) return 1;

        var len = Math.Max(x.Count, y.Count);
        for (var i = 0; i < len; i++)
        {
            var xItem = x.GetAtIndexOrDefault(i);
            var yItem = y.GetAtIndexOrDefault(i);
            var c = Compare(xItem, yItem);
            if (c != 0) return c;
        }

        return 0;
    }
}

public abstract partial class ComparerBase<T> : IEqualityComparer<IEnumerable<T?>?>, IComparer<IEnumerable<T?>?>
{
    public virtual bool Equals(IEnumerable<T?>? x, IEnumerable<T?>? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;

        // ReSharper disable PossibleMultipleEnumeration
        if (!skipCount)
        {
            var xCount = Util.GetCount(x);
            var yCount = Util.GetCount(y);
            if (xCount != null && yCount != null && xCount.Value != yCount.Value) return false;
        }

        using var xEnumerator = x.GetEnumerator();
        using var yEnumerator = y.GetEnumerator();
        // ReSharper restore PossibleMultipleEnumeration

        bool xContinue, yContinue;
        do
        {
            xContinue = xEnumerator.MoveNext();
            yContinue = yEnumerator.MoveNext();
            if (xContinue != yContinue) return false;
            if (!xContinue && !yContinue) return true;
            if (!Equals(xEnumerator.Current, yEnumerator.Current)) return false;
        } while (xContinue && yContinue);

        return true;
    }

    public virtual int GetHashCode(IEnumerable<T?>? obj)
    {
        var h = new HashCode();
        if (obj != null)
        {
            foreach (var item in obj)
            {
                h.Add(GetHashCode(item));
            }
        }
        return h.ToHashCode();
    }

    public virtual int Compare(IEnumerable<T?>? x, IEnumerable<T?>? y)
    {
        if (ReferenceEquals(x, y)) return 0;
        if (ReferenceEquals(x, null)) return -1;
        if (ReferenceEquals(y, null)) return 1;

        using var xEnumerator = x.GetEnumerator();
        using var yEnumerator = y.GetEnumerator();

        bool xContinue, yContinue;
        do
        {
            xContinue = xEnumerator.MoveNext();
            yContinue = yEnumerator.MoveNext();
            if (xContinue && !yContinue) return 1;
            if (!xContinue && yContinue) return -1;
            if (!xContinue && !yContinue) return 0;
            var c = Compare(xEnumerator.Current, yEnumerator.Current);
            if (c != 0) return c;
        } while (xContinue && yContinue);

        return 0;
    }
}
